// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Storage;

namespace Microsoft.Toolkit.Graph.RoamingSettings
{
    /// <summary>
    /// An ObjectStorageHelper implementation using open extensions on the Graph User for storing key/value pairs.
    /// </summary>
    public class UserExtensionDataStore : IObjectStorageHelper
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
        /// Gets the id of the Graph User extension.
        /// </summary>
        public string ExtensionId { get; }

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

        private readonly IObjectSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserExtensionDataStore"/> class.
        /// </summary>
        public UserExtensionDataStore(IObjectSerializer objectSerializer, string extensionId, string userId, bool autoSync = false)
        {
            _serializer = objectSerializer ?? throw new ArgumentNullException(nameof(objectSerializer));

            ExtensionId = extensionId;
            UserId = userId;
            UserExtension = null;

            if (autoSync)
            {
                _ = Sync();
            }
        }

        /// <summary>
        /// An indexer for accessing the Settings values.
        /// </summary>
        /// <param name="key">The key for the desired value.</param>
        /// <returns>The value for the provided key.</returns>
        public object this[string key]
        {
            get => Settings[key];
            set
            {
                if (Settings != null)
                {
                    Settings[key] = value;
                }
            }
        }

        /// <summary>
        /// Creates a new roaming settings extension on the Graph User.
        /// </summary>
        /// <returns>The newly created Extension object.</returns>
        public async Task<Extension> Create()
        {
            UserExtension = await Create(ExtensionId, UserId);
            return UserExtension;
        }

        /// <summary>
        /// Deletes the roamingSettings extension from the Graph User.
        /// </summary>
        /// <returns>A void task.</returns>
        public async Task Delete()
        {
            await Delete(ExtensionId, UserId);
            UserExtension = null;
        }

        /// <summary>
        /// Update the cached user extension.
        /// </summary>
        /// <returns>The freshly synced user extension.</returns>
        public async Task<Extension> Sync()
        {
            UserExtension = await GetExtensionForUser(ExtensionId, UserId);
            return UserExtension;
        }

        /// <inheritdoc />
        public bool KeyExists(string key)
        {
            return UserExtension.AdditionalData.ContainsKey(key);
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
            if (!Settings.TryGetValue(key, out object value) || value == null)
            {
                return @default;
            }

            return _serializer.Deserialize<T>((string)value);
        }

        /// <inheritdoc />
        public T Read<T>(string compositeKey, string key, T @default = default)
        {
            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)Settings[compositeKey];
            if (composite != null)
            {
                string value = (string)composite[key];
                if (value != null)
                {
                    return _serializer.Deserialize<T>(value);
                }
            }

            return @default;
        }

        /// <inheritdoc />
        public void Save<T>(string key, T value)
        {
            var type = typeof(T);
            var typeInfo = type.GetTypeInfo();

            Settings[key] = _serializer.Serialize(value);
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
            }
            else
            {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
                foreach (KeyValuePair<string, T> setting in values)
                {
                    composite.Add(setting.Key, _serializer.Serialize(setting.Value));
                }

                Settings[compositeKey] = composite;
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
