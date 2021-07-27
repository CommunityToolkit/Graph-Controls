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
using Microsoft.Toolkit.Helpers;

namespace CommunityToolkit.Graph.Helpers.ObjectStorage
{
    /// <summary>
    /// A base class for easily building roaming settings helper implementations.
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
            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider == null || provider.State != ProviderState.SignedIn)
            {
                throw new InvalidOperationException($"The {nameof(ProviderManager.GlobalProvider)} must be set and signed in to create a new {nameof(OneDriveStorageHelper)} for the current user.");
            }

            var me = await provider.GetClient().Me.Request().GetAsync();
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
            return await OneDriveDataSource.GetFileAsync<T>(UserId, filePath, Serializer) ?? @default;
        }

        /// <inheritdoc />
        public Task<IEnumerable<(DirectoryItemType ItemType, string Name)>> ReadFolderAsync(string folderPath)
        {
            return OneDriveDataSource.ReadFolderAsync(UserId, folderPath);
        }

        /// <inheritdoc />
        public async Task CreateFileAsync<T>(string filePath, T value)
        {
            await OneDriveDataSource.SetFileAsync<T>(UserId, filePath, value, Serializer);
        }

        /// <inheritdoc />
        public Task CreateFolderAsync(string folderPath)
        {
            return OneDriveDataSource.CreateFolderAsync(UserId, folderPath);
        }

        /// <inheritdoc />
        public Task DeleteItemAsync(string itemPath)
        {
            return OneDriveDataSource.DeleteItemAsync(UserId, itemPath);
        }
    }
}
