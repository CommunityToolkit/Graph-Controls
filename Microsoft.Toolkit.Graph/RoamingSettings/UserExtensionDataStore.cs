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
    public class UserExtensionDataStore : IObjectStorageHelper
    {
        public static async Task<T> Get<T>(string extensionId, string userId, string key)
        {
            return (T)await Get(extensionId, userId, key);
        }

        public static async Task<object> Get(string extensionId, string userId, string key)
        {
            var userExtension = await GetExtensionForUser(extensionId, userId);
            return userExtension.AdditionalData[key];
        }

        public static async Task Set(string extensionId, string userId, string key, object value)
        {
            var userExtension = await GetExtensionForUser(extensionId, userId);
            await userExtension.SetValue(userId, key, value);
        }

        public static async Task<Extension> Create(string extensionId, string userId)
        {
            var userExtension = await UserExtensionsDataSource.CreateExtension(userId, extensionId);
            return userExtension;
        }

        public static async Task Delete(string extensionId, string userId)
        {
            await UserExtensionsDataSource.DeleteExtension(userId, extensionId);
        }

        public static async Task<Extension> GetExtensionForUser(string extensionId, string userId)
        {
            var userExtension = await UserExtensionsDataSource.GetExtension(userId, extensionId);
            return userExtension;
        }

        public string ExtensionId { get; }

        public string UserId { get; }

        public Extension UserExtension { get; protected set; }

        protected IDictionary<string, object> Settings => UserExtension?.AdditionalData;

        private readonly IObjectSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserExtensionDataStore"/> class.
        /// </summary>
        public UserExtensionDataStore(IObjectSerializer objectSerializer, string extensionId, string userId, bool autoSync = false)
        {
            serializer = objectSerializer ?? throw new ArgumentNullException(nameof(objectSerializer));

            ExtensionId = extensionId;
            UserId = userId;
            UserExtension = null;

            if (autoSync)
            {
                _ = Sync();
            }
        }

        public object this[string key]
        {
            get => UserExtension?.AdditionalData[key];
            set
            {
                if (UserExtension?.AdditionalData != null)
                {
                    UserExtension.AdditionalData[key] = value;
                }
            }
        }

        public virtual async Task<Extension> Create()
        {
            UserExtension = await Create(ExtensionId, UserId);
            return UserExtension;
        }

        public virtual async Task Delete()
        {
            await Delete(ExtensionId, UserId);
            UserExtension = null;
        }

        public virtual async Task Sync()
        {
            UserExtension = await GetExtensionForUser(ExtensionId, UserId);
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

            return serializer.Deserialize<T>((string)value);
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
                    return serializer.Deserialize<T>(value);
                }
            }

            return @default;
        }

        /// <inheritdoc />
        public void Save<T>(string key, T value)
        {
            var type = typeof(T);
            var typeInfo = type.GetTypeInfo();

            Settings[key] = serializer.Serialize(value);
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
                        composite[setting.Key] = serializer.Serialize(setting.Value);
                    }
                    else
                    {
                        composite.Add(setting.Key, serializer.Serialize(setting.Value));
                    }
                }
            }
            else
            {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
                foreach (KeyValuePair<string, T> setting in values)
                {
                    composite.Add(setting.Key, serializer.Serialize(setting.Value));
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
