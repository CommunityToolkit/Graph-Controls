// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// <see cref="IAuthenticationProvider"/> helper wrapper to expose more states around the authentication process for graph controls.
    /// </summary>
    public interface IProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Gets the current login state of the provider.
        /// </summary>
        ProviderState State { get; }

        /// <summary>
        /// Gets the <see cref="GraphServiceClient"/> object to access the Microsoft Graph APIs.
        /// </summary>
        GraphServiceClient Graph { get; }

        /// <summary>
        /// Event called when the login <see cref="State"/> changes.
        /// </summary>
        event EventHandler<ProviderStateChangedEventArgs> StateChanged;

        /// <summary>
        /// Login the user.
        /// </summary>
        /// <returns><see cref="Task"/>.</returns>
        Task LoginAsync();

        /// <summary>
        /// Logout the user.
        /// </summary>
        /// <returns><see cref="Task"/>.</returns>
        Task LogoutAsync();
    }
}
