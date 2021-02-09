// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using Windows.Security.Authentication.Web;
using Windows.Security.Authentication.Web.Core;
using Windows.UI.ApplicationSettings;

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// A provider for leveraging Windows system authentication.
    /// </summary>
    public class WindowsProvider : BaseProvider
    {
        private struct AuthenticatedUser
        {
            public Windows.Security.Credentials.PasswordCredential TokenCredential { get; private set; }

            public string GetUserName()
            {
                return TokenCredential?.UserName;
            }

            public string GetToken()
            {
                return TokenCredential?.Password;
            }

            public AuthenticatedUser(Windows.Security.Credentials.PasswordCredential tokenCredential)
            {
                TokenCredential = tokenCredential;
            }
        }

        private const string TokenCredentialResourceName = "WindowsProviderToken";
        private const string WebAccountProviderId = "https://login.microsoft.com";
        private static readonly string[] DefaultScopes = new string[] { "user.read" };
        private static readonly string GraphResourceProperty = "https://graph.microsoft.com";

        /// <summary>
        /// Gets the redirect uri value based on the current app callback uri.
        /// </summary>
        public static string RedirectUri => string.Format("ms-appx-web://Microsoft.AAD.BrokerPlugIn/{0}", WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper());

        private AccountsSettingsPane _currentPane;
        private AuthenticatedUser? _currentUser;
        private string[] _scopes;
        private string _clientId;

        /// <summary>
        /// Creates a new instance of the WindowsProvider and attempts to sign in silently.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="scopes"></param>
        /// <returns>A new instance of a WindowsProvider.</returns>
        public static async Task<WindowsProvider> CreateAsync(string clientId, string[] scopes)
        {
            var provider = new WindowsProvider(clientId, scopes);

            provider.Graph = new GraphServiceClient(provider);

            await provider.TrySilentSignInAsync();

            return provider;
        }

        private WindowsProvider(string clientId, string[] scopes = null)
        {
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _currentPane = null;
            _currentUser = null;
            _scopes = scopes ?? DefaultScopes;

            Graph = new GraphServiceClient(new DelegateAuthenticationProvider(AuthenticateRequestAsync));

            State = ProviderState.SignedOut;
        }

        /// <summary>
        /// Attempts to sign in the logged in user automatically.
        /// </summary>
        /// <returns>Success boolean.</returns>
        public async Task<bool> TrySilentSignInAsync()
        {
            if (State == ProviderState.SignedIn)
            {
                return false;
            }

            State = ProviderState.Loading;

            var tokenCredential = GetCredentialFromLocker();
            if (tokenCredential == null)
            {
                // There is no credential stored in the locker.
                State = ProviderState.SignedOut;
                return false;
            }

            // Populate the password (aka token).
            tokenCredential.RetrievePassword();

            // Log the user in by storing the credential in memory.
            _currentUser = new AuthenticatedUser(tokenCredential);

            try
            {
                var me = await Graph.Me.Request().GetAsync();

                // Update the state to be signed in.
                State = ProviderState.SignedIn;
                return true;
            }
            catch
            {
                // Update the state to be signed in.
                State = ProviderState.SignedOut;
                return false;
            }
        }

        /// <inheritdoc />
        public override Task LoginAsync()
        {
            if (State == ProviderState.SignedIn)
            {
                return Task.CompletedTask;
            }

            State = ProviderState.Loading;

            if (_currentPane != null)
            {
                _currentPane.AccountCommandsRequested -= BuildPaneAsync;
            }

            _currentPane = AccountsSettingsPane.GetForCurrentView();
            _currentPane.AccountCommandsRequested += BuildPaneAsync;

            AccountsSettingsPane.Show();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task LogoutAsync()
        {
            if (State == ProviderState.SignedOut)
            {
                return Task.CompletedTask;
            }

            State = ProviderState.Loading;

            if (_currentPane != null)
            {
                _currentPane.AccountCommandsRequested -= BuildPaneAsync;
                _currentPane = null;
            }

            if (_currentUser != null)
            {
                // Remove the user info from the PaasswordVault
                var vault = new Windows.Security.Credentials.PasswordVault();
                vault.Remove(_currentUser?.TokenCredential);

                _currentUser = null;
            }

            State = ProviderState.SignedOut;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            // Append the token to the authorization header of any outgoing Graph requests.
            var token = _currentUser?.GetToken();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return Task.CompletedTask;
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/uwp/security/web-account-manager#build-the-account-settings-pane.
        /// </summary>
        private async void BuildPaneAsync(AccountsSettingsPane sender, AccountsSettingsPaneCommandsRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                // Providing nothing shows all accounts, providing authority shows only aad
                var msaProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(WebAccountProviderId);

                if (msaProvider == null)
                {
                    State = ProviderState.SignedOut;
                    return;
                }

                var command = new WebAccountProviderCommand(msaProvider, GetTokenAsync);
                args.WebAccountProviderCommands.Add(command);
            }
            catch
            {
                State = ProviderState.SignedOut;
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async void GetTokenAsync(WebAccountProviderCommand command)
        {
            // Build the token request
            WebTokenRequest request = new WebTokenRequest(command.WebAccountProvider, string.Join(',', _scopes), _clientId);
            request.Properties.Add("resource", GraphResourceProperty);

            // Get the results
            WebTokenRequestResult result = await WebAuthenticationCoreManager.RequestTokenAsync(request);

            // Handle user cancellation
            if (result.ResponseStatus == WebTokenRequestStatus.UserCancel)
            {
                State = ProviderState.SignedOut;
                return;
            }

            // Handle any errors
            if (result.ResponseStatus != WebTokenRequestStatus.Success)
            {
                Debug.WriteLine(result.ResponseError.ErrorMessage);
                State = ProviderState.SignedOut;
                return;
            }

            // Extract values from the results
            var token = result.ResponseData[0].Token;
            var account = result.ResponseData[0].WebAccount;

            // The UserName value may be null, but the Id is always present.
            var userName = account.Id;

            // Save the user info to the PaasswordVault
            var vault = new Windows.Security.Credentials.PasswordVault();
            var tokenCredential = new Windows.Security.Credentials.PasswordCredential(TokenCredentialResourceName, userName, token);
            vault.Add(tokenCredential);

            // Set the current user object
            _currentUser = new AuthenticatedUser(tokenCredential);

            // Update the state to be signed in.
            State = ProviderState.SignedIn;
        }

        private Windows.Security.Credentials.PasswordCredential GetCredentialFromLocker()
        {
            Windows.Security.Credentials.PasswordCredential credential = null;

            try
            {
                var vault = new Windows.Security.Credentials.PasswordVault();
                var credentialList = vault.FindAllByResource(TokenCredentialResourceName);
                if (credentialList.Count > 0)
                {
                    // We delete the credential upon logout, so only one user can be stored in the vault at a time.
                    credential = credentialList.First();
                }
            }
            catch
            {
                // FindAllByResource will throw an exception if the resource isn't found.
            }

            return credential;
        }
    }
}
