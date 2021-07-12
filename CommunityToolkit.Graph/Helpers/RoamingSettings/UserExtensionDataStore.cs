// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Storage;

namespace CommunityToolkit.Graph.Helpers.RoamingSettings
{
    /// <summary>
    /// An IObjectStorageHelper implementation using open extensions on the Graph User for storing key/value pairs.
    /// </summary>
    public class UserExtensionDataStore : BaseRoamingSettingsDataStore
    {
        /// <summary>
        /// Retrieve the value from Graph User extensions and cast the response to the provided type.
        /// </summary>
        /// <typeparam name="T">The type to cast the return result to.</typeparam>
        /// <param name="userId">The id of the user.</param>
        /// <param name="extensionId">The id of the user extension.</param>
        /// <param name="key">The key for the desired value.</param>
        /// <returns>The value from the data store.</returns>
        public static async Task<T> Get<T>(string userId, string extensionId, string key)
        {
            return (T)await Get(userId, extensionId, key);
        }

        /// <summary>
        /// Retrieve the value from Graph User extensions by extensionId, userId, and key.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="extensionId">The id of the user extension.</param>
        /// <param name="key">The key for the desired value.</param>
        /// <returns>The value from the data store.</returns>
        public static async Task<object> Get(string userId, string extensionId, string key)
        {
            var userExtension = await GetExtensionForUser(userId, extensionId);
            return userExtension.AdditionalData[key];
        }

        /// <summary>
        /// Set a value by key in a Graph User's extension.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="extensionId">The id of the user extension.</param>
        /// <param name="key">The key for the target value.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>A task upon completion.</returns>
        public static async Task Set(string userId, string extensionId, string key, object value)
        {
            await UserExtensionsDataSource.SetValue(userId, extensionId, key, value);
        }

        /// <summary>
        /// Creates a new roaming settings extension on a Graph User.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="extensionId">The id of the user extension.</param>
        /// <returns>The newly created user extension.</returns>
        public static async Task<Extension> Create(string userId, string extensionId)
        {
            var userExtension = await UserExtensionsDataSource.CreateExtension(userId, extensionId);
            return userExtension;
        }

        /// <summary>
        /// Deletes an extension by id on a Graph User.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="extensionId">The id of the user extension.</param>
        /// <returns>A task upon completion.</returns>
        public static async Task Delete(string userId, string extensionId)
        {
            await UserExtensionsDataSource.DeleteExtension(userId, extensionId);
        }

        /// <summary>
        /// Retrieves a user extension.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="extensionId">The id of the user extension.</param>
        /// <returns>The target extension.</returns>
        public static async Task<Extension> GetExtensionForUser(string userId, string extensionId)
        {
            var userExtension = await UserExtensionsDataSource.GetExtension(userId, extensionId);
            return userExtension;
        }

        private static readonly IList<string> ReservedKeys = new List<string> { "responseHeaders", "statusCode", "@odata.context" };

        /// <summary>
        /// Initializes a new instance of the <see cref="UserExtensionDataStore"/> class.
        /// </summary>
        public UserExtensionDataStore(string userId, string extensionId, IObjectSerializer objectSerializer, bool autoSync = true)
            : base(userId, extensionId, objectSerializer, autoSync)
        {
        }

        /// <summary>
        /// Creates a new roaming settings extension on the Graph User.
        /// </summary>
        /// <returns>The newly created Extension object.</returns>
        public override async Task Create()
        {
            await Create(UserId, Id);
        }

        /// <summary>
        /// Deletes the roamingSettings extension from the Graph User.
        /// </summary>
        /// <returns>A void task.</returns>
        public override async Task Delete()
        {
            // Delete the cache
            Cache.Clear();

            // Delete the remote.
            await Delete(UserId, Id);
        }

        /// <summary>
        /// Update the remote extension to match the local cache and retrieve any new keys. Any existing remote values are replaced.
        /// </summary>
        /// <returns>The freshly synced user extension.</returns>
        public override async Task Sync()
        {
            try
            {
                IDictionary<string, object> remoteData = null;

                try
                {
                    // Get the remote
                    Extension extension = await GetExtensionForUser(UserId, Id);
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
        }

        /// <inheritdoc />
        public override Task<bool> FileExistsAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override Task<T> ReadFileAsync<T>(string filePath, T @default = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override Task<StorageFile> SaveFileAsync<T>(string filePath, T value)
        {
            throw new NotImplementedException();
        }
    }
}