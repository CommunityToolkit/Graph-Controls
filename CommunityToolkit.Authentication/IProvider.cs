// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CommunityToolkit.Authentication
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
        /// Gets the id of the currently signed in user account.
        /// </summary>
        string CurrentAccountId { get; }

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
        /// Retrieve a token for the authenticated user.
        /// </summary>
        /// <param name="silentOnly">Determines if the acquisition should be done without prompts to the user.</param>
        /// <returns>A token string for the authenticated user.</returns>
        Task<string> GetTokenAsync(bool silentOnly = false);

        /// <summary>
        /// Sign in the user.
        /// </summary>
        /// <returns><see cref="Task"/>.</returns>
        Task SignInAsync();

        /// <summary>
        /// Sign out the user.
        /// </summary>
        /// <returns><see cref="Task"/>.</returns>
        Task SignOutAsync();

        /// <summary>
        /// Tries to check if the user is logged in without prompting to login.
        /// </summary>
        /// <returns>A boolean indicating success or failure to sign in silently.</returns>
        Task<bool> TrySilentSignInAsync();
    }
}
