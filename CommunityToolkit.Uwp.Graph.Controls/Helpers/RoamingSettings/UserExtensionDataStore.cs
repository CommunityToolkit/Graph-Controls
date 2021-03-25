// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Uwp.Graph.Common;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Storage;

namespace CommunityToolkit.Uwp.Graph.Helpers.RoamingSettings
{
    /// <summary>
    /// An IObjectStorageHelper implementation using open extensions on the Graph User for storing key/value pairs.
    /// </summary>
    public class UserExtensionDataStore : IRoamingSettingsDataStore
    {
        /// <summary>
        /// Retrieve the value from Graph User extensions and cast the response to the provided type.
        /// </summary>
        /// <typeparam name="T">The type to cast the return result to.</typeparam>
        /// <param name="extensionId">The id of the user extension.</param>
        /// <param name="userId">The id of the user.</param>
        /// <param name="key">The key for the desired value.</param>
        /// <returns>The value from the data store.</returns>
        public static async Task<T> Get<T>(string extensionId, string userId, string key)
        {
            return (T)await Get(extensionId, userId, key);
        }

        /// <summary>
        /// Retrieve the value from Graph User extensions by extensionId, userId, and key.
        /// </summary>
        /// <param name="extensionId">The id of the user extension.</param>
        /// <param name="userId">The id of the user.</param>
        /// <param name="key">The key for the desired value.</param>
        /// <returns>The value from the data store.</returns>
        public static async Task<object> Get(string extensionId, string userId, string key)
        {
            var userExtension = await GetExtensionForUser(extensionId, userId);
            return userExtension.AdditionalData[key];
        }

        /// <summary>
        /// Set a value by key in a Graph User's extension.
        /// </summary>
        /// <param name="extensionId">The id of the user extension.</param>
        /// <param name="userId">The id of the user.</param>
        /// <param name="key">The key for the target value.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>A task upon completion.</returns>
        public static async Task Set(string extensionId, string userId, string key, object value)
        {
            var userExtension = await GetExtensionForUser(extensionId, userId);
            await userExtension.SetValue(userId, key, value);
        }

        /// <summary>
        /// Creates a new roaming settings extension on a Graph User.
        /// </summary>
        /// <param name="extensionId">The id of the user extension.</param>
        /// <param name="userId">The id of the user.</param>
        /// <returns>The newly created user extension.</returns>
        public static async Task<Extension> Create(string extensionId, string userId)
        {
            var userExtension = await UserExtensionsDataSource.CreateExtension(userId, extensionId);
            return userExtension;
        }

        /// <summary>
        /// Deletes an extension by id on a Graph User.
        /// </summary>
        /// <param name="extensionId">The id of the user extension.</param>
        /// <param name="userId">The id of the user.</param>
        /// <returns>A task upon completion.</returns>
        public static async Task Delete(string extensionId, string userId)
        {
            await UserExtensionsDataSource.DeleteExtension(userId, extensionId);
        }

        /// <summary>
        /// Retrieves a user extension.
        /// </summary>
        /// <param name="extensionId">The id of the user extension.</param>
        /// <param name="userId">The id of the user.</param>
        /// <returns>The target extension.</returns>
        public static async Task<Extension> GetExtensionForUser(string extensionId, string userId)
        {
            var userExtension = await UserExtensionsDataSource.GetExtension(userId, extensionId);
            return userExtension;
        }

        /// <summary>
        /// Gets the id of the Graph User.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// Gets or sets the cached Extension object.
        /// </summary>
        public Extension UserExtension { get; protected set; }

        /// <summary>
        /// Gets the cached key value pairs from the internal data store.
        /// </summary>
        public IDictionary<string, object> Settings => UserExtension?.AdditionalData;

        private readonly string _extensionId;
        private readonly IObjectSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserExtensionDataStore"/> class.
        /// </summary>
        public UserExtensionDataStore(string extensionId, string userId, IObjectSerializer objectSerializer = null)
        {
            _extensionId = extensionId;
            _serializer = objectSerializer;

            UserId = userId;
            UserExtension = null;
        }

        /// <summary>
        /// Creates a new roaming settings extension on the Graph User.
        /// </summary>
        /// <returns>The newly created Extension object.</returns>
        public async Task Create()
        {
            UserExtension = await Create(_extensionId, UserId);
        }

        /// <summary>
        /// Deletes the roamingSettings extension from the Graph User.
        /// </summary>
        /// <returns>A void task.</returns>
        public async Task Delete()
        {
            await Delete(_extensionId, UserId);
            UserExtension = null;
        }

        /// <summary>
        /// Update the cached user extension.
        /// </summary>
        /// <returns>The freshly synced user extension.</returns>
        public async Task Sync()
        {
            UserExtension = await GetExtensionForUser(_extensionId, UserId);
        }

        /// <inheritdoc />
        public virtual bool KeyExists(string key)
        {
            return Settings.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool KeyExists(string compositeKey, string key)
        {
            if (KeyExists(compositeKey))
            {
                ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)Settings[compositeKey];
                if (composite != null)
                {
                    return composite.ContainsKey(key);
                }
            }

            return false;
        }

        /// <inheritdoc />
        public T Read<T>(string key, T @default = default)
        {
            if (Settings.TryGetValue(key, out object value) || value == null)
            {
                try
                {
                    return _serializer.Deserialize<T>((string)value);
                }
                catch
                {
                    // Primitive types can't be deserialized.
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            }

            return @default;
        }

        /// <inheritdoc />
        public T Read<T>(string compositeKey, string key, T @default = default)
        {
            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)Settings[compositeKey];
            if (composite != null)
            {
                object value = composite[key];
                if (value != null)
                {
                    try
                    {
                        return _serializer.Deserialize<T>((string)value);
                    }
                    catch
                    {
                        // Primitive types can't be deserialized.
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                }
            }

            return @default;
        }

        /// <inheritdoc />
        public void Save<T>(string key, T value)
        {
            // Set the local cache
            Settings[key] = _serializer.Serialize(value);

            // Send an update to the remote.
            Task.Run(() => Set(_extensionId, UserId, key, value));
        }

        /// <inheritdoc />
        public void Save<T>(string compositeKey, IDictionary<string, T> values)
        {
            if (KeyExists(compositeKey))
            {
                ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)Settings[compositeKey];

                foreach (KeyValuePair<string, T> setting in values)
                {
                    if (composite.ContainsKey(setting.Key))
                    {
                        composite[setting.Key] = _serializer.Serialize(setting.Value);
                    }
                    else
                    {
                        composite.Add(setting.Key, _serializer.Serialize(setting.Value));
                    }
                }

                Settings[compositeKey] = composite;
                Task.Run(() => Set(_extensionId, UserId, compositeKey, composite));
            }
            else
            {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
                foreach (KeyValuePair<string, T> setting in values)
                {
                    composite.Add(setting.Key, _serializer.Serialize(setting.Value));
                }

                Settings[compositeKey] = composite;
                Task.Run(() => Set(_extensionId, UserId, compositeKey, composite));
            }
        }

        /// <inheritdoc />
        public Task<bool> FileExistsAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<T> ReadFileAsync<T>(string filePath, T @default = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<StorageFile> SaveFileAsync<T>(string filePath, T value)
        {
            throw new NotImplementedException();
        }
    }
}