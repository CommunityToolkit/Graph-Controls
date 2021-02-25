// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Microsoft.Toolkit.Graph.RoamingSettings
{
    public class UserExtensionDataStore
    {
        public string ExtensionId { get; }

        public string UserId { get; }

        public Extension UserExtension { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserExtensionDataStore"/> class.
        /// </summary>
        public UserExtensionDataStore(string extensionId, string userId, bool autoSync = false)
        {
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

        public virtual async Task<object> Get(string key, bool checkCache = true)
        {
            if (checkCache && UserExtension?.AdditionalData != null && UserExtension.AdditionalData.ContainsKey(key))
            {
                return UserExtension.AdditionalData[key];
            }

            object value = await Get(ExtensionId, UserId, key);
            UserExtension.AdditionalData[key] = value;

            return value;
        }

        public virtual async Task<T> Get<T>(string key, bool checkCache = true)
        {
            if (checkCache && UserExtension?.AdditionalData != null && UserExtension.AdditionalData.ContainsKey(key))
            {
                return (T)UserExtension.AdditionalData[key];
            }

            T value = (T)await Get(ExtensionId, UserId, key);
            UserExtension.AdditionalData[key] = value;

            return value;
        }

        public virtual async Task Set(string key, object value)
        {
            await Set(ExtensionId, UserId, key, value);

            UserExtension.AdditionalData[key] = value;
        }

        public virtual async Task Create()
        {
            UserExtension = await Create(ExtensionId, UserId);
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
    }
}
