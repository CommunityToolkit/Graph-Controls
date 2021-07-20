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
using Microsoft.Toolkit.Helpers;

namespace CommunityToolkit.Graph.Helpers.ObjectStorage
{
    /// <summary>
    /// An IObjectStorageHelper implementation using open extensions on the Graph User for storing key/value pairs.
    /// </summary>
    public class UserExtensionStorageHelper : IRemoteSettingsStorageHelper
    {
        private static readonly IList<string> ReservedKeys = new List<string> { "responseHeaders", "statusCode", "@odata.context" };
        private static readonly SemaphoreSlim SyncLock = new (1);

        /// <summary>
        /// Gets or sets an event that fires whenever a sync request has completed.
        /// </summary>
        public EventHandler SyncCompleted { get; set; }

        /// <summary>
        /// gets or sets an event that fires whenever a remote sync request has failed.
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
        public IDictionary<string, object> Cache { get; }

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
        /// An indexer for easily accessing key values.
        /// </summary>
        /// <param name="key">The key for the desired value.</param>
        /// <returns>The value found for the provided key.</returns>
        public object this[string key]
        {
            get => Read<object>(key);
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

            Cache = new Dictionary<string, object>();
        }

        /// <summary>
        /// Update the remote extension to match the local cache and retrieve any new keys. Any existing remote values are replaced.
        /// </summary>
        /// <returns>The freshly synced user extension.</returns>
        public virtual async Task Sync()
        {
            await SyncLock.WaitAsync();

            try
            {
                IDictionary<string, object> remoteData = null;

                try
                {
                    // Get the remote
                    Extension extension = await UserExtensionDataSource.GetExtension(UserId, ExtensionId);
                    remoteData = extension.AdditionalData;
                }
                catch
                {
                }

                if (Cache != null)
                {
                    // Send updates for all local values, overwriting the remote.
                    foreach (string key in Cache.Keys.ToList())
                    {
                        if (ReservedKeys.Contains(key))
                        {
                            continue;
                        }

                        if (remoteData == null || !remoteData.ContainsKey(key) || !EqualityComparer<object>.Default.Equals(remoteData[key], Cache[key]))
                        {
                            Save(key, Cache[key]);
                        }
                    }
                }

                if (remoteData != null)
                {
                    // Update local cache with additions from remote
                    foreach (string key in remoteData.Keys.ToList())
                    {
                        if (!Cache.ContainsKey(key))
                        {
                            Cache.Add(key, remoteData[key]);
                        }
                    }
                }

                SyncCompleted?.Invoke(this, new EventArgs());
            }
            catch
            {
                SyncFailed?.Invoke(this, new EventArgs());
            }
            finally
            {
                SyncLock.Release();
            }
        }

        /// <inheritdoc />
        public bool KeyExists(string key)
        {
            return Cache.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool KeyExists(string compositeKey, string key)
        {
            if (Cache.TryGetValue(compositeKey, out object compositeObj))
            {
                var composite = compositeObj as Composite;
                return composite != default && composite.ContainsKey(key);
            }

            return false;
        }

        /// <inheritdoc />
        public T Read<T>(string key, T @default = default)
        {
            return Cache.TryGetValue(key, out object value)
                ? DeserializeValue<T>(value)
                : @default;
        }

        /// <inheritdoc />
        public T Read<T>(string compositeKey, string key, T @default = default)
        {
            if (Cache.TryGetValue(compositeKey, out object compositeObj))
            {
                var composite = compositeObj as Composite;
                if (composite != default && composite.TryGetValue(key, out object valueObj))
                {
                    return DeserializeValue<T>(valueObj);
                }
            }

            return @default;
        }

        /// <inheritdoc />
        public void Save<T>(string key, T value)
        {
            Cache[key] = SerializeValue(value);
        }

        /// <inheritdoc />
        public void Save<T>(string compositeKey, IDictionary<string, T> values)
        {
            if (Cache.TryGetValue(compositeKey, out object compositeObj))
            {
                var composite = compositeObj as Composite;

                foreach (KeyValuePair<string, T> setting in values.ToList())
                {
                    string key = setting.Key;
                    object value = SerializeValue(setting.Value);
                    if (composite.ContainsKey(setting.Key))
                    {
                        composite[key] = value;
                    }
                    else
                    {
                        composite.Add(key, value);
                    }
                }

                Cache[compositeKey] = composite;
            }
            else
            {
                var composite = new Composite();
                foreach (KeyValuePair<string, T> setting in values.ToList())
                {
                    string key = setting.Key;
                    object value = SerializeValue(setting.Value);
                    composite.Add(key, value);
                }

                Cache[compositeKey] = composite;
            }
        }

        /// <inheritdoc />
        public void Delete(string key)
        {
            if (!Cache.Remove(key))
            {
                throw new KeyNotFoundException($"Key \"{key}\" was not found.");
            }
        }

        /// <inheritdoc />
        public void Delete(string compositeKey, string key)
        {
            if (!Cache.TryGetValue(compositeKey, out object compositeObj))
            {
                throw new KeyNotFoundException($"Composite key \"{compositeKey}\" was not found.");
            }

            var composite = compositeObj as Composite;

            if (!composite.Remove(key))
            {
                throw new KeyNotFoundException($"Key \"{key}\" was not found in composite \"{compositeKey}\"");
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

        // A "Composite" is really just a dictionary.
        private class Composite : Dictionary<string, object>
        {
        }
    }
}