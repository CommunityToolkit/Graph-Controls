// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

namespace Microsoft.Toolkit.Graph.Providers.Msal
{
    /// <summary>
    /// <a href="https://github.com/AzureAD/microsoft-authentication-library-for-dotnet">MSAL.NET</a> provider helper for tracking authentication state using an <see cref="IAuthenticationProvider"/> class.
    /// </summary>
    public class MsalProvider : BaseProvider
    {
        /// <summary>
        /// Gets or sets the MSAL.NET Client used to authenticate the user.
        /// </summary>
        protected IPublicClientApplication Client { get; set; }

        /// <summary>
        /// Gets or sets the provider used by the graph to manage requests.
        /// </summary>
        protected IAuthenticationProvider Provider { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalProvider"/> class. <see cref="Create(MsalConfig)"/>.
        /// </summary>
        private MsalProvider()
        {
        }

        /// <summary>
        /// Easily creates a <see cref="MsalProvider"/> from a config object.
        /// </summary>
        /// <param name="config">
        /// The configuration object used to initialize the provider.
        /// </param>
        /// <returns>
        /// A new instance of the MsalProvider.
        /// </returns>
        public static MsalProvider Create(MsalConfig config)
        {
            return Create(config.ClientId, config.RedirectUri, config.Scopes.ToArray());
        }

        /// <summary>
        /// Easily creates a <see cref="MsalProvider"/> from a ClientId.
        /// </summary>
        /// <example>
        /// <code>
        /// ProviderManager.Instance.GlobalProvider = await QuickCreate.CreateMsalProviderAsync("MyClientId");
        /// </code>
        /// </example>
        /// <param name="clientid">Registered ClientId.</param>
        /// <param name="redirectUri">RedirectUri for auth response.</param>
        /// <param name="scopes">List of Scopes to initially request.</param>
        /// <returns>New <see cref="MsalProvider"/> reference.</returns>
        public static MsalProvider Create(string clientid, string redirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient", string[] scopes = null)
        {
            //// TODO: Check all config provided

            var client = PublicClientApplicationBuilder.Create(clientid)
                .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
                .WithRedirectUri(redirectUri)
                .WithClientName(ProviderManager.ClientName)
                .WithClientVersion(Assembly.GetExecutingAssembly().GetName().Version.ToString())
                .Build();

            if (scopes == null)
            {
                scopes = new string[] { string.Empty };
            }

            var msal = new MsalProvider
            {
                Client = client,
                Provider = new InteractiveAuthenticationProvider(client, scopes),
            };
            msal.Graph = new GraphServiceClient(msal);

            _ = msal.TrySilentSignInAsync();

            return msal;
        }

        /// <inheritdoc/>
        public override async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            AddSdkVersion(request);

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
