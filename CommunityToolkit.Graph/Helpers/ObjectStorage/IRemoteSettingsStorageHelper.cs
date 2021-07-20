// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Helpers;

namespace CommunityToolkit.Graph.Helpers.ObjectStorage
{
    /// <summary>
    /// Describes a remote settings storage location with basic sync support.
    /// </summary>
    public interface IRemoteSettingsStorageHelper : ISettingsStorageHelper
    {
        /// <summary>
        /// Gets or sets an event that fires whenever a sync request has completed.
        /// </summary>
        EventHandler SyncCompleted { get; set; }

        /// <summary>
        /// gets or sets an event that fires whenever a remote sync request has failed.
        /// </summary>
        EventHandler SyncFailed { get; set; }

        /// <summary>
        /// Gets a cache of the stored values, converted using the provided serializer.
        /// </summary>
        IDictionary<string, object> Cache { get; }

        /// <summary>
        /// Update the remote extension to match the local cache and retrieve any new keys. Any existing remote values are replaced.
        /// </summary>
        /// <returns>The freshly synced user extension.</returns>
        Task Sync();
    }
}
