using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CommunityToolkit.Net.Authentication;
using Windows.Networking.Connectivity;
using Windows.Security.Authentication.Web;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.ApplicationSettings;

namespace CommunityToolkit.Uwp.Authentication
{
    /// <summary>
    /// 
    /// </summary>
    public class WindowsProvider : BaseProvider
    {
        /// <summary>
        /// Gets the redirect uri value based on the current app callback uri.
        /// </summary>
        public static string RedirectUri => string.Format("ms-appx-web://Microsoft.AAD.BrokerPlugIn/{0}", WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper());

        private const string AzureADAuthority = "organizations";
        private const string MicrosoftAccountProviderId = "https://login.windows.net";
        private const string GraphResourceProperty = "https://graph.microsoft.com";
        private const string WebAccountProviderId = "https://login.microsoft.com";
        private const string SettingsKeyWamAccountId = "WamAccountId";
        private const string SettingsKeyWamProviderId = "WamProviderId";

        private static readonly string[] DefaultScopes =
        {
            "User.Read",
        };

        private ApplicationDataContainer _appSettings;
        private string _clientId;
        private string[] _scopes;
        private WebAccount _webAccount;
        private WebAccountProvider _webAccountProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsProvider"/> class.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="scopes"></param>
        public WindowsProvider (string clientId, string[] scopes = null)
        {
            _appSettings = ApplicationData.Current.LocalSettings;
            _clientId = clientId;
            _scopes = scopes ?? DefaultScopes;
            _webAccount = null;

            State = ProviderState.SignedOut;
        }

        /// <inheritdoc />
        public override async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            string token = await GetTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <inheritdoc />
        public override async Task LoginAsync()
        {
            if (_webAccount != null || State != ProviderState.SignedOut)
            {
                await LogoutAsync();
            }

            // The state will get updated as part of the auth flow.
            var token = await GetTokenAsync();

            if (token == null)
            {
                await LogoutAsync();
            }
        }

        /// <inheritdoc />
        public override async Task LogoutAsync()
        {
            _appSettings.Values.Remove(SettingsKeyWamAccountId);
            _appSettings.Values.Remove(SettingsKeyWamProviderId);

            if (_webAccount != null)
            {
                try
                {
                    await _webAccount.SignOutAsync();
                }
                catch
                {
                    // Failed to remove an account.
                }

                _webAccount = null;
            }

            State = ProviderState.SignedOut;
        }

        private async Task<string> GetTokenAsync(bool silentOnly = false)
        {
            var internetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (internetConnectionProfile == null)
            {
                // We are not online, no token for you.
                // TODO: Is there anything special to do when we go offline?
                return null;
            }

            try
            {
                if (State == ProviderState.SignedOut)
                {
                    State = ProviderState.Loading;
                }

                // Attempt to authenticate silently.
                var authResult = await AuthenticateSilentAsync();

                if (authResult?.ResponseStatus != WebTokenRequestStatus.Success && !silentOnly)
                {
                    // Attempt to authenticate interactively.
                    authResult = await AuthenticateInteractiveAsync();
                }

                if (authResult?.ResponseStatus == WebTokenRequestStatus.Success)
                {
                    var account = _webAccount;
                    var newAccount = authResult.ResponseData[0].WebAccount;

                    if (account == null || account.Id != newAccount.Id)
                    {
                        // Account was switched, update the active account.
                        await SetAccountAsync(newAccount);
                    }

                    var authToken = authResult.ResponseData[0].Token;
                    return authToken;
                }
                else if (authResult?.ResponseError != null)
                {
                    throw new Exception(authResult.ResponseError.ErrorCode + ": " + authResult.ResponseError.ErrorMessage);
                }
            }
            catch (Exception e)
            {
            }

            return null;
        }

        private async Task SetAccountAsync(WebAccount account)
        {
            if (account == null)
            {
                if (_webAccount != null)
                {
                    await LogoutAsync();
                }
                else
                {
                    State = ProviderState.SignedOut;
                }

                return;
            }

            if (account.Id == _webAccount?.Id)
            {
                // no change
                return;
            }

            // Save off the account ids.
            _webAccount = account;
            _appSettings.Values[SettingsKeyWamAccountId] = account.Id;
            _appSettings.Values[SettingsKeyWamProviderId] = account.WebAccountProvider.Id;

            State = ProviderState.SignedIn;
        }

        private async Task<WebTokenRequestResult> AuthenticateSilentAsync()
        {
            var account = _webAccount;
            if (account != null)
            {
                // Prepare a request to get a token.
                var webTokenRequest = GetWebTokenRequest(account.WebAccountProvider);

                try
                {
                    WebTokenRequestResult authResult = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(webTokenRequest, account);
                    return authResult;
                }
                catch (HttpRequestException)
                {
                    throw; /* probably offline, no point continuing to interactive auth */
                }
            }

            return null;
        }

        private async Task<WebTokenRequestResult> AuthenticateInteractiveAsync()
        {
            var pane = AccountsSettingsPane.GetForCurrentView();
            pane.AccountCommandsRequested += OnAccountCommandsRequested;

            try
            {
                WebTokenRequestResult authResult = null;

                var account = _webAccount;
                if (account == null)
                {
                    await AccountsSettingsPane.ShowAddAccountAsync();

                    // _webAccountProvider will be set once the user has selected an account.
                    if (_webAccountProvider != null)
                    {
                        var webTokenRequest = GetWebTokenRequest(_webAccountProvider);

                        // The webAccountProvider may need to come from the commands event instead.

                        // If we reached here, then WebAccountProviderCommandInvoked
                        // was called and a new _webTokenRequest was generated based
                        // on the user's selection in the dialog.
                        authResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest);
                    }
                }
                else
                {
                    var webTokenRequest = GetWebTokenRequest(account.WebAccountProvider);
                    authResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest, account);
                }

                return authResult;
            }
            catch (HttpRequestException)
            {
                throw; /* probably offline, no point continuing to interactive auth */
            }
            finally
            {
                pane.AccountCommandsRequested -= OnAccountCommandsRequested;
            }
        }

        private async void OnAccountCommandsRequested(AccountsSettingsPane sender, AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            void WebAccountProviderCommandInvoked(WebAccountProviderCommand command)
            {
                _webAccountProvider = command.WebAccountProvider;
            }

            var deferral = e.GetDeferral();

            try
            {
                WebAccountProvider webAccountProvider = await GetWebAccountProvider();

                var providerCommand = new WebAccountProviderCommand(webAccountProvider, WebAccountProviderCommandInvoked);
                e.WebAccountProviderCommands.Add(providerCommand);

                // e.HeaderText = _resourceLoader.GetString("WAMTitle");

                // We only show the privacy link if the debugger is not attached because it throws internally as part of
                // parsing the string (CSettingsCommandFactory::CreateSettingsCommand first tries to parse the string as a guid
                // and uses exceptions in determining it is not a guid :( ).
                // if (!Debugger.IsAttached)
                //    e.Commands.Add(new SettingsCommand("privacypolicy", "Privacy policy", PrivacyPolicyInvoked));
            }
            catch
            {
                await LogoutAsync();
            }
            finally
            {
                deferral.Complete();
            }
        }

        /// <summary>
        /// Create a token request. Executing the request will prompt the authentication flow to begin.
        /// </summary>
        private WebTokenRequest GetWebTokenRequest(WebAccountProvider provider)
        {
            var webTokenRequest = new WebTokenRequest(provider, string.Join(',', _scopes), _clientId);
            webTokenRequest.Properties.Add("resource", GraphResourceProperty);

            return webTokenRequest;
        }

        private async Task<WebAccountProvider> GetWebAccountProvider()
        {
            // Find the provider for general MSA login.
            // Org accounts will work out of the box, but MSA's won't work unless the app is associated with the store.
            return await WebAuthenticationCoreManager.FindAccountProviderAsync(WebAccountProviderId);

            // Find the provider for org account login.
            // return await WebAuthenticationCoreManager.FindAccountProviderAsync(MicrosoftAccountProviderId, AzureADAuthority);
        }
    }
}
