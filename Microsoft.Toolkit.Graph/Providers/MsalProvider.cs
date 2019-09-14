using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Microsoft.Toolkit.Graph.Providers
{
    //// TODO: Move some of this to a simple base-class for non-MSAL parts related to Provider only and properties?

    /// <summary>
    /// <a href="https://github.com/AzureAD/microsoft-authentication-library-for-dotnet">MSAL.NET</a> provider helper for tracking authentication state using an <see cref="IAuthenticationProvider"/> class.
    /// </summary>
    public class MsalProvider : IProvider
    {
        /// <summary>
        /// Gets or sets the initial scopes to request.
        /// </summary>
        public List<string> Scopes { get; protected set; } // TODO: Do we need this?

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

            set
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
        /// <param name="scopes">Initial scopes to request.</param>
        /// <returns>A <see cref="Task"/> returning a <see cref="MsalProvider"/> instance.</returns>
        public static async Task<MsalProvider> CreateAsync(IPublicClientApplication client, IAuthenticationProvider provider, List<string> scopes = null)
        {
            //// TODO: Check all config provided

            var msal = new MsalProvider
            {
                Client = client,
                Provider = provider,
                Graph = new GraphServiceClient(provider),
                Scopes = scopes // TODO: Redundant? Can I use a dummy for the try silent below?
            };

            await msal.TrySilentSignInAsync();

            return msal;
        }

        /// <inheritdoc/>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
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
                    var result = await Client.AcquireTokenSilent(Scopes, account).ExecuteAsync();

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
        }
    }
}
