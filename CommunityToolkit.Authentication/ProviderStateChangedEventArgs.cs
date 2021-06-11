// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace CommunityToolkit.Authentication
{
    /// <summary>
    /// <see cref="IProvider.StateChanged"/> event arguments.
    /// </summary>
    public class ProviderStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderStateChangedEventArgs"/> class.
        /// </summary>
        /// <param name="oldState">Previous <see cref="ProviderState"/>.</param>
        /// <param name="newState">Current <see cref="ProviderState"/>.</param>
        public ProviderStateChangedEventArgs(ProviderState? oldState, ProviderState? newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        /// <summary>
        /// Gets the previous state of the <see cref="IProvider"/>.
        /// </summary>
        public ProviderState? OldState { get; private set; }

        /// <summary>
        /// Gets the new state of the <see cref="IProvider"/>.
        /// </summary>
        public ProviderState? NewState { get; private set; }
    }
}