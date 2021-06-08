// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace CommunityToolkit.Authentication
{
    /// <summary>
    /// <see cref="EventArgs"/> class for <see cref="ProviderManager.ProviderUpdated"/> event.
    /// </summary>
    public class ProviderUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderUpdatedEventArgs"/> class.
        /// </summary>
        /// <param name="provider">The newly configured provider.</param>
        public ProviderUpdatedEventArgs(IProvider provider)
        {
            Provider = provider;
        }

        /// <summary>
        /// Gets the updated provider.
        /// </summary>
        public IProvider Provider { get; private set; }
    }
}