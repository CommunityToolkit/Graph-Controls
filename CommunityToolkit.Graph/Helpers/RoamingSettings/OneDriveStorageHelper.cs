// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using Microsoft.Toolkit.Helpers;

namespace CommunityToolkit.Graph.Helpers.RoamingSettings
{
    /// <summary>
    /// An IFileStorageHelper implementation for interacting with data stored via files and folders in OneDrive.
    /// </summary>
    public class OneDriveStorageHelper : IFileStorageHelper
    {
        /// <summary>
        /// Gets the id of the Graph user.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// Gets an object serializer for converting objects in the data store.
        /// </summary>
        public IObjectSerializer Serializer { get; }

        /// <summary>
        /// Creates a new instance using the userId retrieved from a Graph "Me" request.
        /// </summary>
        /// <param name="objectSerializer">A serializer used for converting stored objects.</param>
        /// <returns>A new instance of the <see cref="OneDriveStorageHelper"/> configured for the current Graph user.</returns>
        public static async Task<OneDriveStorageHelper> CreateForCurrentUserAsync(IObjectSerializer objectSerializer = null)
        {
            var graph = GetGraphClient();
            var me = await graph.GetMeAsync();
            var userId = me.Id;

            return new OneDriveStorageHelper(userId, objectSerializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneDriveStorageHelper"/> class.
        /// </summary>
        /// <param name="userId">The id of the target Graph user.</param>
        /// <param name="objectSerializer">A serializer used for converting stored objects.</param>
        public OneDriveStorageHelper(string userId, IObjectSerializer objectSerializer = null)
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            Serializer = objectSerializer ?? new SystemSerializer();
        }

        /// <inheritdoc />
        public async Task<T> ReadFileAsync<T>(string filePath, T @default = default)
        {
            var graph = ProviderManager.Instance.GlobalProvider.GetClient();
            return await graph.GetFileAsync<T>(UserId, filePath, Serializer) ?? @default;
        }

        /// <inheritdoc />
        public Task<IEnumerable<(DirectoryItemType ItemType, string Name)>> ReadFolderAsync(string folderPath)
        {
            var graph = GetGraphClient();
            return graph.ReadFolderAsync(UserId, folderPath);
        }

        /// <inheritdoc />
        public async Task CreateFileAsync<T>(string filePath, T value)
        {
            var graph = GetGraphClient();
            await graph.SetFileAsync<T>(UserId, filePath, value, Serializer);
        }

        /// <inheritdoc />
        public Task CreateFolderAsync(string folderName)
        {
            var graph = GetGraphClient();
            return graph.CreateFolderAsync(UserId, folderName);
        }

        /// <summary>
        /// Ensure a folder exists at the path specified.
        /// </summary>
        /// <param name="folderName">The name of the new folder.</param>
        /// <param name="folderPath">The path to create the new folder in.</param>
        /// <returns>A task.</returns>
        public Task CreateFolderAsync(string folderName, string folderPath)
        {
            var graph = GetGraphClient();
            return graph.CreateFolderAsync(UserId, folderName, folderPath);
        }

        /// <inheritdoc />
        public async Task<bool> TryDeleteItemAsync(string itemPath)
        {
            try
            {
                var graph = GetGraphClient();
                await graph.DeleteItemAsync(UserId, itemPath);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> TryRenameItemAsync(string itemPath, string newName)
        {
            try
            {
                var graph = GetGraphClient();
                await graph.RenameItemAsync(UserId, itemPath, newName);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static GraphServiceClient GetGraphClient()
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider == null || provider.State != ProviderState.SignedIn)
            {
                throw new InvalidOperationException($"The {nameof(ProviderManager.GlobalProvider)} must be set and signed in to perform this action.");
            }

            return provider.GetClient();
        }
    }
}
