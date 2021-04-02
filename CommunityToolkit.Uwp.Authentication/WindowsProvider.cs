using System;
using System.Collections.Generic;
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
    /// An authentication provider based on the native AccountSettingsPane in Windows.
    /// </summary>
    public class WindowsProvider : BaseProvider
    {
        /// <summary>
        /// Gets the redirect uri value based on the current app callback uri.
        /// </summary>
        public static string RedirectUri => string.Format("ms-appx-web://Microsoft.AAD.BrokerPlugIn/{0}", WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper());

        private const string GraphResourceProperty = "https://graph.microsoft.com";
        private const string MicrosoftProviderId = "https://login.microsoft.com";
        private const string AzureActiveDirectoryAuthority = "organizations";
        private const string MicrosoftAccountAuthority = "consumers";

        private static readonly string[] DefaultScopes =
        {
            "User.Read",
        };

        /// <summary>
        /// Gets a cache of important values for the signed in user.
        /// </summary>
        protected IDictionary<string, object> Settings => ApplicationData.Current.LocalSettings.Values;

        /// <summary>
        /// The settings key for the active account id.
        /// </summary>
        protected const string SettingsKeyWamAccountId = "WamAccountId";

        /// <summary>
        /// The settings key for the active provider id.
        /// </summary>
        protected const string SettingsKeyWamProviderId = "WamProviderId";

        private string _clientId;
        private string[] _scopes;
        private WebAccount _webAccount;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsProvider"/> class.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="scopes"></param>
        public WindowsProvider (string clientId, string[] scopes = null)
        {
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

        /// <summary>
        /// Try logging in silently.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task TryLoginSilentAsync()
        {
            if (_webAccount != null || State != ProviderState.SignedOut)
            {
                return;
            }

            // The state will get updated as part of the auth flow.
            var token = await GetTokenAsync(true);

            if (token == null)
            {
                await LogoutAsync();
            }
        }

        /// <inheritdoc />
        public override async Task LogoutAsync()
        {
            Settings.Remove(SettingsKeyWamAccountId);
            Settings.Remove(SettingsKeyWamProviderId);

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
                System.Diagnostics.Debug.WriteLine(e.Message);
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
            Settings[SettingsKeyWamAccountId] = account.Id;
            Settings[SettingsKeyWamProviderId] = account.WebAccountProvider.Id;

            State = ProviderState.SignedIn;
        }

        private async Task<WebTokenRequestResult> AuthenticateSilentAsync()
        {
            try
            {
                WebTokenRequestResult authResult = null;

                var account = _webAccount;
                if (account == null)
                {
                    // Check the cache for an existing user
                    if (Settings[SettingsKeyWamAccountId] is string savedAccountId)
                    {
                        var savedProvider = await GetWebAccountProviderAsync();
                        if (Settings[SettingsKeyWamProviderId] is string savedProviderId)
                        {
                            savedProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(savedProviderId);
                        }

                        account = await WebAuthenticationCoreManager.FindAccountAsync(savedProvider, savedAccountId);
                    }
                }

                if (account != null)
                {
                    // Prepare a request to get a token.
                    var webTokenRequest = GetWebTokenRequest(account.WebAccountProvider);
                    authResult = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(webTokenRequest, account);
                }

                return authResult;
            }
            catch (HttpRequestException)
            {
                throw; /* probably offline, no point continuing to interactive auth */
            }
        }

        private async Task<WebTokenRequestResult> AuthenticateInteractiveAsync()
        {
            try
            {
                WebTokenRequestResult authResult = null;

                var account = _webAccount;
                if (account != null)
                {
                    var webTokenRequest = GetWebTokenRequest(account.WebAccountProvider);
                    authResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest, account);
                }
                else
                {
                    authResult = await ShowAddAccountAndGetResultAsync();
                }

                return authResult;
            }
            catch (HttpRequestException)
            {
                throw; /* probably offline, no point continuing to interactive auth */
            }
        }

        private async Task<WebTokenRequestResult> ShowAddAccountAndGetResultAsync()
        {
            var addAccountTaskCompletionSource = new TaskCompletionSource<WebTokenRequestResult>();

            async void WebAccountProviderCommandInvoked(WebAccountProviderCommand command)
            {
                var webTokenRequest = GetWebTokenRequest(command.WebAccountProvider);

                var authResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest);
                addAccountTaskCompletionSource.SetResult(authResult);
            }

            async void OnAccountCommandsRequested(AccountsSettingsPane sender, AccountsSettingsPaneCommandsRequestedEventArgs e)
            {
                var deferral = e.GetDeferral();

                try
                {
                    WebAccountProvider webAccountProvider = await GetWebAccountProviderAsync();

                    var providerCommand = new WebAccountProviderCommand(webAccountProvider, WebAccountProviderCommandInvoked);
                    e.WebAccountProviderCommands.Add(providerCommand);

                    // TODO: Expose configuration so developers can have some control over the popup.
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

            var pane = AccountsSettingsPane.GetForCurrentView();
            pane.AccountCommandsRequested += OnAccountCommandsRequested;

            try
            {
                await AccountsSettingsPane.ShowAddAccountAsync();

                var authResult = await addAccountTaskCompletionSource.Task;
                return authResult;
            }
            finally
            {
                pane.AccountCommandsRequested -= OnAccountCommandsRequested;
            }
        }

        private WebTokenRequest GetWebTokenRequest(WebAccountProvider provider)
        {
            var webTokenRequest = new WebTokenRequest(provider, string.Join(',', _scopes), _clientId);
            webTokenRequest.Properties.Add("resource", GraphResourceProperty);

            return webTokenRequest;
        }

        private async Task<WebAccountProvider> GetWebAccountProviderAsync()
        {
            // TODO: Enable devs to turn on/off which account sources they wish to integrate with.

            // MSA - Works
            // return await WebAuthenticationCoreManager.FindAccountProviderAsync(MicrosoftProviderId, MicrosoftAccountAuthority);

            // AAD - Fails complaining about 'client_assertion' or 'client_secret'
            // return await WebAuthenticationCoreManager.FindAccountProviderAsync(MicrosoftProviderId, AzureActiveDirectoryAuthority);

            // Both
            return await WebAuthenticationCoreManager.FindAccountProviderAsync(MicrosoftProviderId);
        }
    }
}
