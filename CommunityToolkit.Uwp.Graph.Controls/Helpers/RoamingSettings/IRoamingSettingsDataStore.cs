// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;

namespace CommunityToolkit.Uwp.Graph.Helpers.RoamingSettings
{
    /// <summary>
    /// Defines the contract for creating storage containers used for roaming data.
    /// </summary>
    public interface IRoamingSettingsDataStore : IObjectStorageHelper
    {
        /// <summary>
        /// Gets a value indicating whether the values should immediately sync or not.
        /// </summary>
        bool AutoSync { get; }

        /// <summary>
        /// Gets access to the key/value pairs cache directly.
        /// </summary>
        IDictionary<string, object> Cache { get; }

        /// <summary>
        /// Create a new storage container.
        /// </summary>
        /// <returns>A Task.</returns>
        Task Create();

        /// <summary>
        /// Delete the existing storage container.
        /// </summary>
        /// <returns>A Task.</returns>
        Task Delete();

        /// <summary>
        /// Syncronize the internal cache with the remote storage endpoint.
        /// </summary>
        /// <returns>A Task.</returns>
        Task Sync();
    }
}