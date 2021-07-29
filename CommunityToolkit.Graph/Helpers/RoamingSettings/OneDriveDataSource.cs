// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using Microsoft.Toolkit.Helpers;

namespace CommunityToolkit.Graph.Helpers.RoamingSettings
{
    /// <summary>
    /// Helpers for interacting with files in the special OneDrive AppRoot folder.
    /// </summary>
    internal static class OneDriveDataSource
    {
        private static GraphServiceClient Graph => ProviderManager.Instance.GlobalProvider?.GetClient();

        /// <summary>
        /// Updates or create a new file on the remote with the provided content.
        /// </summary>
        /// <typeparam name="T">The type of object to save.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<DriveItem> SetFileAsync<T>(string userId, string itemPath, T fileContents, IObjectSerializer serializer)
        {
            var json = serializer.Serialize(fileContents) as string;
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            return await Graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(itemPath).Content.Request().PutAsync<DriveItem>(stream);
        }

        /// <summary>
        /// Get a file from the remote.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<T> GetFileAsync<T>(string userId, string itemPath, IObjectSerializer serializer)
        {
            Stream stream = await Graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(itemPath).Content.Request().GetAsync();

            string streamContents = new StreamReader(stream).ReadToEnd();

            return serializer.Deserialize<T>(streamContents);
        }

        /// <summary>
        /// Delete the file from the remote.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task DeleteItemAsync(string userId, string itemPath)
        {
            await Graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(itemPath).Request().DeleteAsync();
        }

        public static async Task CreateFolderAsync(string userId, string folderName, string path = null)
        {
            var folderDriveItem = new DriveItem()
            {
                Name = folderName,
                Folder = new Folder(),
            };

            if (path != null)
            {
                await Graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(path).Children.Request().AddAsync(folderDriveItem);
            }
            else
            {
                await Graph.Users[userId].Drive.Special.AppRoot.Children.Request().AddAsync(folderDriveItem);
            }
        }

        public static async Task<IEnumerable<(DirectoryItemType, string)>> ReadFolderAsync(string userId, string folderPath)
        {
            IDriveItemChildrenCollectionPage folderContents = await Graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(folderPath).Children.Request().GetAsync();

            var results = new List<(DirectoryItemType, string)>();
            foreach (var item in folderContents)
            {
                var itemType = (item.Folder != null)
                    ? DirectoryItemType.Folder
                    : item.Size != null
                        ? DirectoryItemType.File
                        : DirectoryItemType.None;

                var itemName = item.Name;

                results.Add((itemType, itemName));
            }

            return results;
        }
    }
}
