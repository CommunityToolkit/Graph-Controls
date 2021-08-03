// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using Microsoft.Toolkit.Extensions;
using Microsoft.Toolkit.Helpers;

namespace CommunityToolkit.Graph.Helpers.RoamingSettings
{
    /// <summary>
    /// An ISettingsStorageHelper implementation using open extensions on the Graph User for storing key/value pairs.
    /// </summary>
    public class UserExtensionStorageHelper : ISettingsStorageHelper<string>
    {
        private static readonly IList<string> ReservedKeys = new List<string> { "responseHeaders", "statusCode", "@odata.context" };
        private static readonly SemaphoreSlim SyncLock = new (1);

        /// <summary>
        /// Gets or sets an event that fires whenever a sync request has completed.
        /// </summary>
        public EventHandler SyncCompleted { get; set; }

        /// <summary>
        /// Gets or sets an event that fires whenever a remote sync request has failed.
        /// </summary>
        public EventHandler SyncFailed { get; set; }

        /// <summary>
        /// Gets the id for the target extension on a Graph user.
        /// </summary>
        public string ExtensionId { get; }

        /// <summary>
        /// Gets the id of the target Graph user.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// Gets an object serializer for converting objects in the data store.
        /// </summary>
        public IObjectSerializer Serializer { get; }

        /// <summary>
        /// Gets a cache of the stored values, converted using the provided serializer.
        /// </summary>
        public IReadOnlyDictionary<string, object> Cache => _cache;

        private readonly Dictionary<string, object> _cache;
        private bool _cleared;

        /// <summary>
        /// Creates a new instance using the userId retrieved from a Graph "Me" request.
        /// </summary>
        /// <param name="extensionId">The id for the target extension on a Graph user.</param>
        /// <param name="objectSerializer">A serializer used for converting stored objects.</param>
        /// <returns>A new instance of the <see cref="UserExtensionStorageHelper"/> configured for the current Graph user.</returns>
        public static async Task<UserExtensionStorageHelper> CreateForCurrentUserAsync(string extensionId, IObjectSerializer objectSerializer = null)
        {
            if (extensionId == null)
            {
                throw new ArgumentNullException(nameof(extensionId));
            }

            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider == null || provider.State != ProviderState.SignedIn)
            {
                throw new InvalidOperationException($"The {nameof(ProviderManager.GlobalProvider)} must be set and signed in to create a new {nameof(UserExtensionStorageHelper)} for the current user.");
            }

            var me = await provider.GetClient().Me.Request().GetAsync();
            var userId = me.Id;

            return new UserExtensionStorageHelper(extensionId, userId, objectSerializer);
        }

        /// <summary>
        /// Retrieve an instance of the GraphServiceClient, or throws an exception if not signed in.
        /// </summary>
        /// <returns>A <see cref="GraphServiceClient"/> instance.</returns>
        protected static GraphServiceClient GetGraphClient()
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider == null || provider.State != ProviderState.SignedIn)
            {
                throw new InvalidOperationException($"The {nameof(ProviderManager.GlobalProvider)} must be set and signed in to perform this action.");
            }

            return provider.GetClient();
        }

        /// <summary>
        /// An indexer for easily accessing key values.
        /// </summary>
        /// <param name="key">The key for the desired value.</param>
        /// <returns>The value found for the provided key.</returns>
        public object this[string key]
        {
            get => ISettingsStorageHelperExtensions.Read<string, object>(this, key);
            set => Save(key, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserExtensionStorageHelper"/> class.
        /// </summary>
        /// <param name="extensionId">The id for the target extension on a Graph user.</param>
        /// <param name="userId">The id of the target Graph user.</param>
        /// <param name="objectSerializer">A serializer used for converting stored objects.</param>
        public UserExtensionStorageHelper(string extensionId, string userId, IObjectSerializer objectSerializer = null)
        {
            ExtensionId = extensionId ?? throw new ArgumentNullException(nameof(extensionId));
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            Serializer = objectSerializer ?? new SystemSerializer();

            _cache = new Dictionary<string, object>();
            _cleared = false;
        }

        /// <inheritdoc />
        public void Save<T>(string key, T value)
        {
            _cache[key] = SerializeValue(value);
        }

        /// <inheritdoc />
        public bool TryRead<TValue>(string key, out TValue value)
        {
            if (_cache.TryGetValue(key, out object cachedValue))
            {
                value = DeserializeValue<TValue>(cachedValue);
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        /// <inheritdoc />
        public bool TryDelete(string key)
        {
            return _cache.Remove(key);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _cache.Clear();
            _cleared = true;
        }

        /// <summary>
        /// Synchronize the cache with the remote:
        /// - If the cache has been cleared, the remote will be deleted and recreated.
        /// - Any cached keys will be saved to the remote, overwriting existing values.
        /// - Any new keys from the remote will be stored in the cache.
        /// </summary>
        /// <returns>The freshly synced user extension.</returns>
        public virtual async Task Sync()
        {
            await SyncLock.WaitAsync();

            try
            {
                var graph = GetGraphClient();

                IDictionary<string, object> remoteData = null;

                // Check if the extension should be cleared.
                if (_cleared)
                {
                    // Delete and re-create the remote extension.
                    await graph.DeleteExtension(UserId, ExtensionId);
                    Extension extension = await graph.CreateExtension(UserId, ExtensionId);
                    remoteData = extension.AdditionalData;

                    _cleared = false;
                }
                else
                {
                    // Get the remote extension.
                    Extension extension = await graph.GetExtension(UserId, ExtensionId);
                    remoteData = extension.AdditionalData;
                }

                // Send updates for all local values, overwriting the remote.
                foreach (string key in _cache.Keys.ToList())
                {
                    if (ReservedKeys.Contains(key))
                    {
                        continue;
                    }

                    if (!remoteData.ContainsKey(key) || !EqualityComparer<object>.Default.Equals(remoteData[key], Cache[key]))
                    {
                        Save(key, _cache[key]);
                    }
                }

                if (remoteData != null)
                {
                    // Update local cache with additions from remote
                    foreach (string key in remoteData.Keys.ToList())
                    {
                        if (ReservedKeys.Contains(key))
                        {
                            continue;
                        }

                        if (!_cache.ContainsKey(key))
                        {
                            _cache.Add(key, remoteData[key]);
                        }
                    }
                }

                SyncCompleted?.Invoke(this, new EventArgs());
            }
            catch (Exception e)
            {
                SyncFailed?.Invoke(this, new EventArgs());
                throw e;
            }
            finally
            {
                SyncLock.Release();
            }
        }

        /// <summary>
        /// Use the serializer to deserialize a value appropriately for the type.
        /// </summary>
        /// <typeparam name="T">The type of object expected.</typeparam>
        /// <param name="value">The value to deserialize.</param>
        /// <returns>An object of type T.</returns>
        protected T DeserializeValue<T>(object value)
        {
            try
            {
                return Serializer.Deserialize<T>((string)value);
            }
            catch
            {
                // Primitive types can't be deserialized.
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }

        /// <summary>
        /// Use the serializer to serialize a value appropriately for the type.
        /// </summary>
        /// <typeparam name="T">The type of object being serialized.</typeparam>
        /// <param name="value">The object to serialize.</param>
        /// <returns>The serialized object.</returns>
        protected object SerializeValue<T>(T value)
        {
            var type = typeof(T);
            var typeInfo = type.GetTypeInfo();

            // Skip serialization for primitives.
            if (typeInfo.IsPrimitive || type == typeof(string))
            {
                // Update the cache
                return value;
            }
            else
            {
                // Update the cache
                return Serializer.Serialize(value);
            }
        }
    }
}