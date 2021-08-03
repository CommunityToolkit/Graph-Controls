// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Toolkit.Helpers;

namespace CommunityToolkit.Graph.Extensions
{
    /// <summary>
    /// UserExtensions focused extension methods to the Graph SDK used by the controls and helpers.
    /// </summary>
    public static partial class GraphExtensions
    {
        /// <summary>
        /// Retrieve an extension object for a user.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="userId">The user to access.</param>
        /// <param name="extensionId">The extension to retrieve.</param>
        /// <returns>The extension result.</returns>
        public static async Task<Extension> GetExtension(this GraphServiceClient graph, string userId, string extensionId)
        {
            if (string.IsNullOrWhiteSpace(extensionId))
            {
                throw new ArgumentNullException(nameof(extensionId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var extension = await graph.Users[userId].Extensions[extensionId].Request().GetAsync();
            return extension;
        }

        /// <summary>
        /// Get all extension objects for a user.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="userId">The user to access.</param>
        /// <returns>All extension results.</returns>
        public static async Task<IList<Extension>> GetAllExtensions(this GraphServiceClient graph, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var extensions = await graph.Users[userId].Extensions.Request().GetAsync();
            return extensions;
        }

        /// <summary>
        /// Create a new extension object on a user.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="userId">The user to access.</param>
        /// <param name="extensionId">The id of the new extension.</param>
        /// <returns>The newly created extension.</returns>
        public static async Task<Extension> CreateExtension(this GraphServiceClient graph, string userId, string extensionId)
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
                return await graph.GetExtension(userId, extensionId);
            }
            catch
            {
            }

            string requestUrl = graph.Users[userId].Extensions.Request().RequestUrl;

            string json = "{" +
                    "\"@odata.type\": \"microsoft.graph.openTypeExtension\"," +
                    "\"extensionName\": \"" + extensionId + "\"," +
                "}";

            HttpRequestMessage hrm = new (HttpMethod.Post, requestUrl);
            hrm.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await graph.AuthenticationProvider.AuthenticateRequestAsync(hrm);
            HttpResponseMessage response = await graph.HttpProvider.SendAsync(hrm);
            if (response.IsSuccessStatusCode)
            {
                // Deserialize into Extension object.
                var content = await response.Content.ReadAsStringAsync();
                var extension = graph.HttpProvider.Serializer.DeserializeObject<Extension>(content);
                return extension;
            }

            return null;
        }

        /// <summary>
        /// Delete a user extension by id.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="userId">The user to access.</param>
        /// <param name="extensionId">The id of the extension to delete.</param>
        /// <returns>A task.</returns>
        public static async Task DeleteExtension(this GraphServiceClient graph, string userId, string extensionId)
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
                await graph.GetExtension(userId, extensionId);
            }
            catch
            {
                // If we can't retrieve the extension, it must not exist.
                return;
            }

            await graph.Users[userId].Extensions[extensionId].Request().DeleteAsync();
        }

        /// <summary>
        /// Sets a user extension value at the specified key.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="userId">The user to access.</param>
        /// <param name="extensionId">The id of the target extension.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>A task.</returns>
        public static async Task SetValue(this GraphServiceClient graph, string userId, string extensionId, string key, object value)
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

            await graph.Users[userId].Extensions[extensionId].Request().UpdateAsync(extensionToUpdate);
        }
    }
}
