// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Storage;

namespace CommunityToolkit.Graph.Uwp.Helpers.RoamingSettings
{
    /// <summary>
    /// A DataStore for managing roaming settings in OneDrive.
    /// </summary>
    public class OneDriveDataStore : BaseRoamingSettingsDataStore
    {
        /// <summary>
        /// Retrieve an object stored in a OneDrive file.
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve.</typeparam>
        /// <param name="userId">The id of the target Graph user.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="serializer">An object serializer for handling deserialization.</param>
        /// <returns>The deserialized file contents.</returns>
        public static async Task<T> Get<T>(string userId, string fileName, IObjectSerializer serializer)
        {
            return await OneDriveDataSource.Retrieve<T>(userId, fileName, serializer);
        }

        /// <summary>
        /// Update the contents of a OneDrive file.
        /// </summary>
        /// <typeparam name="T">The type of object being stored.</typeparam>
        /// <param name="userId">The id of the target Graph user.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContents">The object to store.</param>
        /// <param name="serializer">An object serializer for handling serialization.</param>
        /// <returns>A task.</returns>
        public static async Task Set<T>(string userId, string fileName, T fileContents, IObjectSerializer serializer)
        {
            await OneDriveDataSource.Update(userId, fileName, fileContents, serializer);
        }

        /// <summary>
        /// Delete a file from OneDrive by name.
        /// </summary>
        /// <param name="userId">The id of the target Graph user.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>A task.</returns>
        public static async Task Delete(string userId, string fileName)
        {
            await OneDriveDataSource.Delete(userId, fileName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneDriveDataStore"/> class.
        /// </summary>
        public OneDriveDataStore(string userId, string syncDataFileName, IObjectSerializer objectSerializer, bool autoSync = true)
            : base(userId, syncDataFileName, objectSerializer, autoSync)
        {
        }

        /// <inheritdoc />
        public override Task Create()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override async Task Delete()
        {
            // Clear the cache
            Cache.Clear();

            // Delete the remote.
            await Delete(UserId, Id);
        }

        /// <inheritdoc />
        public override async Task<bool> FileExistsAsync(string filePath)
        {
            var roamingSettings = await Get<object>(UserId, Id, Serializer);
            return roamingSettings != null;
        }

        /// <inheritdoc />
        public override async Task<T> ReadFileAsync<T>(string filePath, T @default = default)
        {
            return await Get<T>(UserId, filePath, Serializer) ?? @default;
        }

        /// <inheritdoc />
        public override async Task<StorageFile> SaveFileAsync<T>(string filePath, T value)
        {
            await Set(UserId, filePath, value, Serializer);

            // Can't convert DriveItem to StorageFile, so we return null instead.
            return null;
        }

        /// <inheritdoc />
        public override async Task Sync()
        {
            try
            {
                // Get the remote
                string fileName = Id;
                IDictionary<string, object> remoteData = null;
                try
                {
                    remoteData = await Get<IDictionary<string, object>>(UserId, fileName, Serializer);
                }
                catch
                {
                    // If get fails, we know the remote store does not exist.
                }

                bool needsUpdate = false;
                if (remoteData != null)
                {
                    // Update local cache with additions from remote
                    foreach (string key in remoteData.Keys.ToList())
                    {
                        // Only insert new values. Existing keys should be overwritten on the remote.
                        if (!Cache.ContainsKey(key))
                        {
                            Cache.Add(key, remoteData[key]);
                            needsUpdate = true;
                        }
                    }
                }
                else if (Cache.Count > 0)
                {
                    // The remote does not yet exist, and we have data to save.
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    // Send updates for local values, overwriting the remote.
                    await Set(UserId, fileName, Cache, Serializer);
                }

                SyncCompleted?.Invoke(this, new EventArgs());
            }
            catch
            {
                SyncFailed?.Invoke(this, new EventArgs());
            }
        }
    }
}
