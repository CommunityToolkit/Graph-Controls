// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace CommunityToolkit.Net.Authentication
{
    /// <summary>
    /// <a href="https://github.com/AzureAD/microsoft-authentication-library-for-dotnet">MSAL.NET</a> provider helper for tracking authentication state.
    /// </summary>
    public class MsalProvider : BaseProvider
    {
        /// <summary>
        /// Gets the MSAL.NET Client used to authenticate the user.
        /// </summary>
        protected IPublicClientApplication Client { get; private set; }

        /// <summary>
        /// Gets an array of scopes to use for accessing Graph resources.
        /// </summary>
        protected string[] Scopes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalProvider"/> class.
        /// </summary>
        /// <param name="clientId">Registered ClientId.</param>
        /// <param name="redirectUri">RedirectUri for auth response.</param>
        /// <param name="scopes">List of Scopes to initially request.</param>
        public MsalProvider(string clientId, string redirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient", string[] scopes = null)
        {
            var client = PublicClientApplicationBuilder.Create(clientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
                .WithRedirectUri(redirectUri)
                .WithClientName(ProviderManager.ClientName)
                .WithClientVersion(Assembly.GetExecutingAssembly().GetName().Version.ToString())
                .Build();

            Scopes = scopes ?? new string[] { string.Empty };

            Client = client;

            _ = TrySilentSignInAsync();
        }

        /// <inheritdoc/>
        public override async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            IEnumerable<IAccount> accounts = await Client.GetAccountsAsync();
            AuthenticationResult authResult;

            try
            {
                authResult = await Client.AcquireTokenSilent(Scopes, accounts.FirstOrDefault()).ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                authResult = await Client.AcquireTokenInteractive(Scopes).ExecuteAsync();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            }

            if (authResult != null)
            {
                AddSdkVersion(request);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
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
        public override async Task LoginAsync()
        {
            // Force fake request to start auth process
            await AuthenticateRequestAsync(new System.Net.Http.HttpRequestMessage());
        }

        /// <inheritdoc/>
        public override async Task LogoutAsync()
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
