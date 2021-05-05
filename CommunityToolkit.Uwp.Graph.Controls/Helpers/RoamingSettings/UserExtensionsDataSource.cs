// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Net.Authentication;
using CommunityToolkit.Net.Graph.Extensions;
using Microsoft.Graph;

namespace CommunityToolkit.Uwp.Graph.Helpers.RoamingSettings
{
    /// <summary>
    /// Manages Graph interaction with open extensions on the user.
    /// </summary>
    internal static class UserExtensionsDataSource
    {
        private static GraphServiceClient Graph => ProviderManager.Instance.GlobalProvider?.GetClient();

        /// <summary>
        /// Retrieve an extension object for a user.
        /// </summary>
        /// <param name="userId">The user to access.</param>
        /// <param name="extensionId">The extension to retrieve.</param>
        /// <returns>The extension result.</returns>
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
        /// Get all extension objects for a user.
        /// </summary>
        /// <param name="userId">The user to access.</param>
        /// <returns>All extension results.</returns>
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
        /// Create a new extension object on a user.
        /// </summary>
        /// <param name="userId">The user to access.</param>
        /// <param name="extensionId">The id of the new extension.</param>
        /// <returns>The newly created extension.</returns>
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

            try
            {
                // Try to see if the extension already exists.
               return await GetExtension(userId, extensionId);
            }
            catch
            {
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
        /// Delete a user extension by id.
        /// </summary>
        /// <param name="userId">The user to access.</param>
        /// <param name="extensionId">The id of the extension to delete.</param>
        /// <returns>A task.</returns>
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

            try
            {
                await GetExtension(userId, extensionId);
            }
            catch
            {
                // If we can't retrieve the extension, it must not exist.
                return;
            }

            await Graph.Users[userId].Extensions[extensionId].Request().DeleteAsync();
        }

        /// <summary>
        /// Get a value from an extension by key.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <param name="extension">The target extension.</param>
        /// <param name="key">The key for the desired value.</param>
        /// <returns>The value for the provided key.</returns>
        public static T GetValue<T>(this Extension extension, string key)
        {
            return (T)GetValue(extension, key);
        }

        /// <summary>
        /// Get a value from a user extension by key.
        /// </summary>
        /// <param name="extension">The target extension.</param>
        /// <param name="key">The key for the desired value.</param>
        /// <returns>The value for the provided key.</returns>
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
        /// Sets a user extension value at the specified key.
        /// </summary>
        /// <param name="userId">The user to access.</param>
        /// <param name="extensionId">The id of the target extension.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>A task.</returns>
        public static async Task SetValue(string userId, string extensionId, string key, object value)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(extensionId))
            {
                throw new ArgumentNullException(nameof(extensionId));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var extensionToUpdate = (Extension)Activator.CreateInstance(typeof(Extension), true);
            extensionToUpdate.AdditionalData = new Dictionary<string, object>() { { key, value } };

            await Graph.Users[userId].Extensions[extensionId].Request().UpdateAsync(extensionToUpdate);
        }
    }
}