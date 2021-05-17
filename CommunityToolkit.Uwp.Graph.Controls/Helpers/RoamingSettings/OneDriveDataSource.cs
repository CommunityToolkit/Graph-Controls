// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Net.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.Helpers;

namespace CommunityToolkit.Uwp.Graph.Helpers.RoamingSettings
{
    /// <summary>
    /// Helpers for interacting with files in the special OneDrive AppRoot folder.
    /// </summary>
    internal static class OneDriveDataSource
    {
        private static GraphServiceClient Graph => ProviderManager.Instance.GlobalProvider?.GetClient();

        // Create a new file.
        // This fails, because OneDrive doesn't like empty files. Use Update instead.
        // public static async Task Create(string fileWithExt)
        // {
        //     var driveItem = new DriveItem()
        //     {
        //         Name = fileWithExt,
        //     };
        //     await Graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(fileWithExt).Request().CreateAsync(driveItem);
        // }

        /// <summary>
        /// Updates or create a new file on the remote with the provided content.
        /// </summary>
        /// <typeparam name="T">The type of object to save.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<DriveItem> Update<T>(string userId, string fileWithExt, T fileContents, IObjectSerializer serializer)
        {
            var json = serializer.Serialize(fileContents) as string;
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            return await Graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(fileWithExt).Content.Request().PutAsync<DriveItem>(stream);
        }

        /// <summary>
        /// Get a file from the remote.
        /// </summary>
        /// <typeparam name="T">The type of object to return.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<T> Retrieve<T>(string userId, string fileWithExt, IObjectSerializer serializer)
        {
            Stream stream = await Graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(fileWithExt).Content.Request().GetAsync();

            string streamContents = new StreamReader(stream).ReadToEnd();

            return serializer.Deserialize<T>(streamContents);
        }

        /// <summary>
        /// Delete the file from the remote.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Delete(string userId, string fileWithExt)
        {
            await Graph.Users[userId].Drive.Special.AppRoot.ItemWithPath(fileWithExt).Request().DeleteAsync();
        }
    }
}
