// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Toolkit.Graph.Extensions;

namespace Microsoft.Toolkit.Graph.Providers
{
    //// TODO: Move some of this to a simple base-class for non-MSAL parts related to Provider only and properties?

    /// <summary>
    /// <a href="https://github.com/AzureAD/microsoft-authentication-library-for-dotnet">MSAL.NET</a> provider helper for tracking authentication state using an <see cref="IAuthenticationProvider"/> class.
    /// </summary>
    public class MsalProvider : IProvider
    {
        /// <summary>
        /// Gets or sets the MSAL.NET Client used to authenticate the user.
        /// </summary>
        protected IPublicClientApplication Client { get; set; }

        /// <summary>
        /// Gets or sets the provider used by the graph to manage requests.
        /// </summary>
        protected IAuthenticationProvider Provider { get; set; }

        private ProviderState _state = ProviderState.Loading;

        /// <inheritdoc/>
        public ProviderState State
        {
            get
            {
                return _state;
            }

            private set
            {
                var current = _state;
                _state = value;

                StateChanged?.Invoke(this, new StateChangedEventArgs(current, _state));
            }
        }

        /// <inheritdoc/>
        public GraphServiceClient Graph { get; private set; }

        /// <inheritdoc/>
        public event EventHandler<StateChangedEventArgs> StateChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalProvider"/> class. <see cref="CreateAsync"/>
        /// </summary>
        private MsalProvider()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalProvider"/> class.
        /// </summary>
        /// <param name="client">Existing <see cref="IPublicClientApplication"/> instance.</param>
        /// <param name="provider">Existing <see cref="IAuthenticationProvider"/> instance.</param>
        /// <returns>A <see cref="Task"/> returning a <see cref="MsalProvider"/> instance.</returns>
        public static async Task<MsalProvider> CreateAsync(IPublicClientApplication client, IAuthenticationProvider provider)
        {
            //// TODO: Check all config provided

            var msal = new MsalProvider
            {
                Client = client,
                Provider = provider
            };

            msal.Graph = new GraphServiceClient(msal);

            await msal.TrySilentSignInAsync();

            return msal;
        }

        /// <inheritdoc/>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            request.AddSdkVersion();

            try
            {
                await Provider.AuthenticateRequestAsync(request);
            }
            catch (Exception)
            {
                // TODO: Catch different types of errors and try and re-auth? Should be handled by Graph Auth Providers.
                // Assume we're signed-out on error?
                State = ProviderState.SignedOut;

                return;
            }

            // Check state after request to see if we're now signed-in.
            if (State != ProviderState.SignedIn)
            {
                if ((await Client.GetAccountsAsync()).Any())
                {
                    State = ProviderState.SignedIn;
                }
                else
                {
                    State = ProviderState.SignedOut;
                }
            }
        }

        /// <summary>
        /// Tries to check if the user is logged in without prompting to login.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task TrySilentSignInAsync()
        {
            var account = (await Client.GetAccountsAsync()).FirstOrDefault();

            if (account == null)
            {
                // No accounts
                State = ProviderState.SignedOut;
            }
            else
            {
                try
                {
                    // Try and sign-in // TODO: can we use empty scopes?
                    var result = await Client.AcquireTokenSilent(new string[] { string.Empty }, account).ExecuteAsync();

                    if (!string.IsNullOrWhiteSpace(result.AccessToken))
                    {
                        State = ProviderState.SignedIn;
                    }
                    else
                    {
                        State = ProviderState.SignedOut;
                    }
                }
                catch (MsalUiRequiredException)
                {
                    await LoginAsync();
                }
                catch (Exception)
                {
                    State = ProviderState.SignedOut;
                }
            }
        }

        /// <inheritdoc/>
        public async Task LoginAsync()
        {
            // Force fake request to start auth process
            await AuthenticateRequestAsync(new System.Net.Http.HttpRequestMessage());
        }

        /// <inheritdoc/>
        public async Task LogoutAsync()
        {
            // Forcibly remove each user.
            foreach (var user in await Client.GetAccountsAsync())
            {
                await Client.RemoveAsync(user);
            }

            State = ProviderState.SignedOut;
        }
    }
}
