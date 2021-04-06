// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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

        private const string AuthenticationHeaderScheme = "Bearer";
        private const string GraphResourcePropertyKey = "resource";
        private const string GraphResourcePropertyValue = "https://graph.microsoft.com";
        private const string MicrosoftAccountAuthority = "consumers";
        private const string MicrosoftProviderId = "https://login.microsoft.com";
        private const string SettingsKeyAccountId = "WindowsProvider_AccountId";
        private const string SettingsKeyProviderId = "WindowsProvider_ProviderId";

        // Default/minimal scopes for authentication, if none are provided.
        private static readonly string[] DefaultScopes = { "User.Read" };

        /// <summary>
        /// Gets a cache of important values for the signed in user.
        /// </summary>
        protected IDictionary<string, object> Settings => ApplicationData.Current.LocalSettings.Values;

        private string _clientId;
        private string[] _scopes;
        private WebAccount _webAccount;
        private AccountsSettingsPaneConfig? _accountsSettingsPaneConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsProvider"/> class.
        /// </summary>
        /// <param name="clientId">Registered ClientId.</param>
        /// <param name="scopes">List of Scopes to initially request.</param>
        /// <param name="accountsSettingsPaneConfig">Configuration values for the AccountsSettingsPane.</param>
        public WindowsProvider(string clientId, string[] scopes = null, AccountsSettingsPaneConfig? accountsSettingsPaneConfig = null)
        {
            _clientId = clientId;
            _scopes = scopes ?? DefaultScopes;
            _accountsSettingsPaneConfig = accountsSettingsPaneConfig;

            _webAccount = null;

            State = ProviderState.SignedOut;
        }

        /// <inheritdoc />
        public override async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            string token = await GetTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationHeaderScheme, token);
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
        public async Task TrySilentLoginAsync()
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
            Settings.Remove(SettingsKeyAccountId);
            Settings.Remove(SettingsKeyProviderId);

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

        /// <summary>
        /// Retrieve a token for the authenticated user.
        /// </summary>
        /// <param name="silentOnly">Determines if the acquisition should be done without prompts to the user.</param>
        /// <returns>A token string for the authenticated user.</returns>
        public async Task<string> GetTokenAsync(bool silentOnly = false)
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

                // Authenticate with user interaction as appropriate.
                if (authResult?.ResponseStatus != WebTokenRequestStatus.Success)
                {
                    if (silentOnly)
                    {
                        // Silent login may fail if we don't have a cached account, and that's ok.
                        return null;
                    }

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
                else if (authResult?.ResponseStatus == WebTokenRequestStatus.UserCancel)
                {
                    return null;
                }
                else if (authResult?.ResponseError != null)
                {
                    throw new Exception(authResult.ResponseError.ErrorCode + ": " + authResult.ResponseError.ErrorMessage);
                }
                else
                {
                    // Authentication response was not successful or cancelled, but is also missing a ResponseError.
                    throw new Exception("Authentication response was not successful, but is also missing a ResponseError.");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                await LogoutAsync();
            }

            return null;
        }

        private async Task SetAccountAsync(WebAccount account)
        {
            if (account == null)
            {
                // Clear account
                 await LogoutAsync();
                 return;
            }
            else if (account.Id == _webAccount?.Id)
            {
                // No change
                return;
            }

            // Save off the account ids.
            _webAccount = account;
            Settings[SettingsKeyAccountId] = account.Id;
            Settings[SettingsKeyProviderId] = account.WebAccountProvider.Id;

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
                    if (Settings[SettingsKeyAccountId] is string savedAccountId &&
                        Settings[SettingsKeyProviderId] is string savedProviderId)
                    {
                        var savedProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(savedProviderId);
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

        /// <summary>
        /// Show the AccountSettingsPane and wait for the user to make a selection, then process the authentication result.
        /// </summary>
        private async Task<WebTokenRequestResult> ShowAddAccountAndGetResultAsync()
        {
            // The AccountSettingsPane uses events to support the flow of authentication events.
            // Ultimately we need access to the user's selected account provider from the AccountSettingsPane, which is available
            // in the WebAccountProviderCommandInvoked function for the chosen provider.
            // To ensure no funny business, the entire AccountSettingsPane flow is contained here.
            var addAccountTaskCompletionSource = new TaskCompletionSource<WebTokenRequestResult>();

            bool webAccountProviderCommandWasInvoked = false;

            // Handle the selected account provider
            async void WebAccountProviderCommandInvoked(WebAccountProviderCommand command)
            {
                webAccountProviderCommandWasInvoked = true;
                try
                {
                    var webTokenRequest = GetWebTokenRequest(command.WebAccountProvider);

                    var authResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest);
                    addAccountTaskCompletionSource.SetResult(authResult);
                }
                catch (Exception ex)
                {
                    addAccountTaskCompletionSource.SetException(ex);
                }
            }

            // Build the AccountSettingsPane and configure it with available providers.
            async void OnAccountCommandsRequested(AccountsSettingsPane sender, AccountsSettingsPaneCommandsRequestedEventArgs e)
            {
                var deferral = e.GetDeferral();

                try
                {
                    // Configure available providers.
                    List<WebAccountProvider> webAccountProviders = await GetWebAccountProvidersAsync();

                    foreach (WebAccountProvider webAccountProvider in webAccountProviders)
                    {
                        var providerCommand = new WebAccountProviderCommand(webAccountProvider, WebAccountProviderCommandInvoked);
                        e.WebAccountProviderCommands.Add(providerCommand);
                    }

                    // Apply the configured header.
                    var headerText = _accountsSettingsPaneConfig?.HeaderText;
                    if (!string.IsNullOrWhiteSpace(headerText))
                    {
                        e.HeaderText = headerText;
                    }

                    // Apply any configured commands.
                    var commands = _accountsSettingsPaneConfig?.Commands;
                    if (commands != null)
                    {
                        foreach (var command in commands)
                        {
                            e.Commands.Add(new SettingsCommand(command.Id, command.Label, (uic) =>
                            {
                                command.Invoked.Invoke(command);
                                addAccountTaskCompletionSource.SetCanceled();
                            }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    addAccountTaskCompletionSource.SetException(ex);
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
                // Show the AccountSettingsPane and wait for the result.
                await AccountsSettingsPane.ShowAddAccountAsync();

                // If an account was selected, the WebAccountProviderCommand will be invoked.
                // If not, the AccountsSettingsPane must have been cancelled or closed.
                var authResult = webAccountProviderCommandWasInvoked ? await addAccountTaskCompletionSource.Task : null;
                return authResult;
            }
            catch (TaskCanceledException)
            {
                // The task was cancelled. Do nothing.
                return null;
            }
            finally
            {
                pane.AccountCommandsRequested -= OnAccountCommandsRequested;
            }
        }

        private WebTokenRequest GetWebTokenRequest(WebAccountProvider provider)
        {
            WebTokenRequest webTokenRequest = new WebTokenRequest(provider, string.Join(',', _scopes), _clientId);
            webTokenRequest.Properties.Add(GraphResourcePropertyKey, GraphResourcePropertyValue);

            return webTokenRequest;
        }

        private async Task<List<WebAccountProvider>> GetWebAccountProvidersAsync()
        {
            var providers = new List<WebAccountProvider>();

            // MSA
            providers.Add(await WebAuthenticationCoreManager.FindAccountProviderAsync(MicrosoftProviderId, MicrosoftAccountAuthority));

            return providers;
        }
    }

    /// <summary>
    /// Configuration values for the AccountsSettingsPane.
    /// </summary>
    public struct AccountsSettingsPaneConfig
    {
        /// <summary>
        /// Gets or sets the header text for the accounts settings pane.
        /// </summary>
        public string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets the SettingsCommand collection for the account settings pane.
        /// </summary>
        public IList<SettingsCommand> Commands { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountsSettingsPaneConfig"/> struct.
        /// </summary>
        /// <param name="headerText">The header text for the accounts settings pane.</param>
        /// <param name="commands">The SettingsCommand collection for the account settings pane.</param>
        public AccountsSettingsPaneConfig(string headerText, IList<SettingsCommand> commands)
        {
            HeaderText = headerText;
            Commands = commands;
        }
    }
}
