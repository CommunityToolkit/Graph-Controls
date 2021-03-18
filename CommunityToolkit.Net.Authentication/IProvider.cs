// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CommunityToolkit.Net.Authentication
{
    /// <summary>
    /// Authentication provider to expose more states around the authentication process for graph controls.
    /// </summary>
    public interface IProvider
    {
        /// <summary>
        /// Gets the current login state of the provider.
        /// </summary>
        ProviderState State { get; }

        /// <summary>
        /// Event called when the login <see cref="State"/> changes.
        /// </summary>
        event EventHandler<ProviderStateChangedEventArgs> StateChanged;

        /// <summary>
        /// Authenticate an outgoing request.
        /// </summary>
        /// <param name="request">The request to authenticate.</param>
        /// <returns>A task upon completion.</returns>
        Task AuthenticateRequestAsync(HttpRequestMessage request);

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
