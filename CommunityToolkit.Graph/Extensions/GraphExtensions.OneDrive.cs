// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Toolkit.Helpers;

namespace CommunityToolkit.Graph.Extensions
{
    /// <summary>
    /// OneDrive focused extension methods to the Graph SDK used by the controls and helpers.
    /// </summary>
    public static partial class GraphExtensions
    {
        /// <summary>
        /// Updates or create a new file on the remote with the provided content.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="userId">The id of the target Graph user.</param>
        /// <param name="itemPath">The path of the target item.</param>
        /// <param name="fileContents">The contents to put in the file.</param>
        /// <param name="serializer">A serializer for converting stored values.</param>
        /// <typeparam name="T">The type of object to save.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<DriveItem> SetFileAsync<T>(this GraphServiceClient graph, string userId, string itemPath, T fileContents, IObjectSerializer serializer)
        {
            var json = serializer.Serialize(fileContents) as string;
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            return await graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(itemPath).Content.Request().PutAsync<DriveItem>(stream);
        }

        /// <summary>
        /// Get a file from the remote.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="userId">The id of the target Graph user.</param>
        /// <param name="itemPath">The path of the target item.</param>
        /// <param name="serializer">A serializer for converting stored values.</param>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<T> GetFileAsync<T>(this GraphServiceClient graph, string userId, string itemPath, IObjectSerializer serializer)
        {
            Stream stream = await graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(itemPath).Content.Request().GetAsync();

            string streamContents = new StreamReader(stream).ReadToEnd();

            return serializer.Deserialize<T>(streamContents);
        }

        /// <summary>
        /// Delete the file from the remote.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="userId">The id of the target Graph user.</param>
        /// <param name="itemPath">The path of the target item.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task DeleteItemAsync(this GraphServiceClient graph, string userId, string itemPath)
        {
            await graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(itemPath).Request().DeleteAsync();
        }

        /// <summary>
        /// Rename an item.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="userId">The id of the target Graph user.</param>
        /// <param name="itemPath">The path of the target item.</param>
        /// <param name="newName">The new name for the item.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task RenameItemAsync(this GraphServiceClient graph, string userId, string itemPath, string newName)
        {
            var driveItem = new DriveItem
            {
                Name = newName,
            };

            await graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(itemPath).Request().UpdateAsync(driveItem);
        }

        /// <summary>
        /// Ensure a folder exists by name.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="userId">The id of the target Graph user.</param>
        /// <param name="folderName">The name of the new folder.</param>
        /// <param name="path">The path to create the new folder in.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task CreateFolderAsync(this GraphServiceClient graph, string userId, string folderName, string path = null)
        {
            var folderDriveItem = new DriveItem()
            {
                Name = folderName,
                Folder = new Folder(),
            };

            if (path != null)
            {
                await graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(path).Children.Request().AddAsync(folderDriveItem);
            }
            else
            {
                await graph.Users[userId].Drive.Special.AppRoot.Children.Request().AddAsync(folderDriveItem);
            }
        }

        /// <summary>
        /// Retrieve a list of directory items with names and types.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="userId">The id of the target Graph user.</param>
        /// <param name="folderPath">The path to create the new folder in.</param>
        /// <returns>A <see cref="Task"/> with the directory listings.</returns>
        public static async Task<IEnumerable<(DirectoryItemType, string)>> ReadFolderAsync(this GraphServiceClient graph, string userId, string folderPath)
        {
            IDriveItemChildrenCollectionPage folderContents = await graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(folderPath).Children.Request().GetAsync();

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
