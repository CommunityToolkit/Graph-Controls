// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;

namespace Microsoft.Toolkit.Graph.RoamingSettings
{
    /// <summary>
    /// 
    /// </summary>
    public static class UserExtensionsDataSource
    {
        private static GraphServiceClient Graph => ProviderManager.Instance.GlobalProvider?.Graph;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="extensionId"></param>
        /// <returns></returns>
        public static async Task<Extension> GetExtension(string userId, string extensionId)
        {
            if (string.IsNullOrWhiteSpace(extensionId))
            {
                throw new ArgumentNullException(nameof(extensionId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var extension = await Graph.Users[userId].Extensions[extensionId].Request().GetAsync();
            return extension;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<IList<Extension>> GetAllExtensions(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var extensions = await Graph.Users[userId].Extensions.Request().GetAsync();
            return extensions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="extensionId"></param>
        /// <returns></returns>
        public static async Task<Extension> CreateExtension(string userId, string extensionId)
        {
            if (string.IsNullOrWhiteSpace(extensionId))
            {
                throw new ArgumentNullException(nameof(extensionId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            string requestUrl = Graph.Users[userId].Extensions.Request().RequestUrl;

            string json = "{" +
                    "\"@odata.type\": \"microsoft.graph.openTypeExtension\"," +
                    "\"extensionName\": \"" + extensionId + "\"," +
                "}";

            HttpRequestMessage hrm = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            hrm.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await Graph.AuthenticationProvider.AuthenticateRequestAsync(hrm);
            HttpResponseMessage response = await Graph.HttpProvider.SendAsync(hrm);
            if (response.IsSuccessStatusCode)
            {
                // Deserialize into Extension object.
                var content = await response.Content.ReadAsStringAsync();
                var extension = Graph.HttpProvider.Serializer.DeserializeObject<Extension>(content);
                return extension;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="extensionId"></param>
        /// <returns></returns>
        public static async Task DeleteExtension(string userId, string extensionId)
        {
            if (string.IsNullOrWhiteSpace(extensionId))
            {
                throw new ArgumentNullException(nameof(extensionId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            await Graph.Users[userId].Extensions[extensionId].Request().DeleteAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="extension"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetValue<T>(this Extension extension, string key)
        {
            return (T)GetValue(extension, key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object GetValue(this Extension extension, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (extension.AdditionalData.ContainsKey(key))
            {
                return extension.AdditionalData[key];
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="userId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task SetValue(this Extension extension, string userId, string key, object value)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var extensionToUpdate = (Extension)Activator.CreateInstance(typeof(Extension), true);
            extensionToUpdate.AdditionalData = new Dictionary<string, object>() { { key, value } };

            await Graph.Users[userId].Extensions[extension.Id].Request().UpdateAsync(extensionToUpdate);
        }
    }
}
