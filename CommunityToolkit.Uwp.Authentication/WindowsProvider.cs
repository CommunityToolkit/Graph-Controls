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
    /// An enumeration of the available authentication providers for use in the AccountsSettingsPane.
    /// </summary>
    [Flags]
    public enum WebAccountProviderType
    {
        /// <summary>
        /// Authenticate public/consumer MSA accounts.
        /// </summary>
        MSA,
    }

    /// <summary>
    /// An authentication provider based on the native AccountsSettingsPane in Windows.
    /// </summary>
    public class WindowsProvider : BaseProvider
    {
        /// <summary>
        /// Gets the redirect uri value based on the current app callback uri.
        /// Used for configuring in Azure app registration.
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

        // The default account providers available in the AccountsSettingsPane.
        private static readonly WebAccountProviderType DefaultWebAccountsProviderType = WebAccountProviderType.MSA;

        /// <summary>
        /// Gets the list of scopes to pre-authorize during authentication.
        /// </summary>
        public string[] Scopes => _scopes;

        /// <summary>
        /// Gets configuration values for the AccountsSettingsPane.
        /// </summary>
        public AccountsSettingsPaneConfig? AccountsSettingsPaneConfig => _accountsSettingsPaneConfig;

        /// <summary>
        /// Gets the configuration values for determining the available web account providers.
        /// </summary>
        public WebAccountProviderConfig WebAccountProviderConfig => _webAccountProviderConfig;

        /// <summary>
        /// Gets a cache of important values for the signed in user.
        /// </summary>
        protected IDictionary<string, object> Settings => ApplicationData.Current.LocalSettings.Values;

        private string[] _scopes;
        private WebAccount _webAccount;
        private AccountsSettingsPaneConfig? _accountsSettingsPaneConfig;
        private WebAccountProviderConfig _webAccountProviderConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsProvider"/> class.
        /// </summary>
        /// <param name="scopes">List of Scopes to initially request.</param>
        /// <param name="accountsSettingsPaneConfig">Configuration values for the AccountsSettingsPane.</param>
        /// <param name="webAccountProviderConfig">Configuration value for determining the available web account providers.</param>
        /// <param name="autoSignIn">Determines whether the provider attempts to silently log in upon instantionation.</param>
        public WindowsProvider(string[] scopes = null, WebAccountProviderConfig? webAccountProviderConfig = null, AccountsSettingsPaneConfig? accountsSettingsPaneConfig = null, bool autoSignIn = true)
        {
            _scopes = scopes ?? DefaultScopes;
            _webAccountProviderConfig = webAccountProviderConfig ?? new WebAccountProviderConfig()
            {
                WebAccountProviderType = DefaultWebAccountsProviderType,
            };
            _accountsSettingsPaneConfig = accountsSettingsPaneConfig;

            _webAccount = null;

            State = ProviderState.SignedOut;

            if (autoSignIn)
            {
                _ = TrySilentSignInAsync();
            }
        }

        /// <inheritdoc />
        public override async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            string token = await GetTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationHeaderScheme, token);
        }

        /// <inheritdoc />
        public override async Task SignInAsync()
        {
            if (_webAccount != null || State != ProviderState.SignedOut)
            {
                return;
            }

            // The state will get updated as part of the auth flow.
            var token = await GetTokenAsync();

            if (token == null)
            {
                await SignOutAsync();
            }
        }

        /// <summary>
        /// Tries to check if the user is logged in without prompting to login.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> TrySilentSignInAsync()
        {
            if (_webAccount != null && State == ProviderState.SignedIn)
            {
                return true;
            }

            // The state will get updated as part of the auth flow.
            var token = await GetTokenAsync(true);
            return token != null;
        }

        /// <inheritdoc />
        public override async Task SignOutAsync()
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
                var initialState = State;
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
                        State = initialState;
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
                await SignOutAsync();
            }

            return null;
        }

        private async Task SetAccountAsync(WebAccount account)
        {
            if (account == null)
            {
                // Clear account
                 await SignOutAsync();
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
                    // We already have the account.
                    var webAccountProvider = account.WebAccountProvider;
                    var webTokenRequest = GetWebTokenRequest(webAccountProvider);
                    authResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest, account);
                }
                else
                {
                    // We don't have an account. Prompt the user to provide one.
                    var webAccountProvider = await ShowAccountSettingsPaneAndGetProviderAsync();
                    var webTokenRequest = GetWebTokenRequest(webAccountProvider);
                    authResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest);
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
        private async Task<WebAccountProvider> ShowAccountSettingsPaneAndGetProviderAsync()
        {
            // The AccountSettingsPane uses events to support the flow of authentication events.
            // Ultimately we need access to the user's selected account provider from the AccountSettingsPane, which is available
            // in the WebAccountProviderCommandInvoked function for the chosen provider.
            // The entire AccountSettingsPane flow is contained here.
            var webAccountProviderTaskCompletionSource = new TaskCompletionSource<WebAccountProvider>();

            bool webAccountProviderCommandWasInvoked = false;

            // Handle the selected account provider
            void WebAccountProviderCommandInvoked(WebAccountProviderCommand command)
            {
                webAccountProviderCommandWasInvoked = true;
                try
                {
                    webAccountProviderTaskCompletionSource.SetResult(command.WebAccountProvider);
                }
                catch (Exception ex)
                {
                    webAccountProviderTaskCompletionSource.SetException(ex);
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
                            // We don't actually use the provided commands directly. Instead, we make new commands
                            // with matching ids and labels, but we override the invoked action so we can cancel the TaskCompletionSource.
                            e.Commands.Add(new SettingsCommand(command.Id, command.Label, (uic) =>
                            {
                                command.Invoked.Invoke(command);
                                webAccountProviderTaskCompletionSource.SetCanceled();
                            }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    webAccountProviderTaskCompletionSource.SetException(ex);
                }
                finally
                {
                    deferral.Complete();
                }
            }

            AccountsSettingsPane pane = null;
            try
            {
                // GetForCurrentView may throw an exception if the current view isn't ready yet.
                pane = AccountsSettingsPane.GetForCurrentView();
                pane.AccountCommandsRequested += OnAccountCommandsRequested;

                // Show the AccountSettingsPane and wait for the result.
                await AccountsSettingsPane.ShowAddAccountAsync();

                // If an account was selected, the WebAccountProviderCommand will be invoked.
                // If not, the AccountsSettingsPane must have been cancelled or closed.
                var webAccountProvider = webAccountProviderCommandWasInvoked ? await webAccountProviderTaskCompletionSource.Task : null;
                return webAccountProvider;
            }
            catch (TaskCanceledException)
            {
                // The task was cancelled. No provider was chosen.
                return null;
            }
            finally
            {
                if (pane != null)
                {
                    pane.AccountCommandsRequested -= OnAccountCommandsRequested;
                }
            }
        }

        private WebTokenRequest GetWebTokenRequest(WebAccountProvider provider)
        {
            string clientId = _webAccountProviderConfig.ClientId;
            string scopes = string.Join(',', _scopes);

            WebTokenRequest webTokenRequest = clientId != null
                ? new WebTokenRequest(provider, scopes, clientId)
                : new WebTokenRequest(provider, scopes);

            webTokenRequest.Properties.Add(GraphResourcePropertyKey, GraphResourcePropertyValue);

            return webTokenRequest;
        }

        private async Task<List<WebAccountProvider>> GetWebAccountProvidersAsync()
        {
            var providers = new List<WebAccountProvider>();

            // MSA
            if ((_webAccountProviderConfig.WebAccountProviderType & WebAccountProviderType.MSA) == WebAccountProviderType.MSA)
            {
                providers.Add(await WebAuthenticationCoreManager.FindAccountProviderAsync(MicrosoftProviderId, MicrosoftAccountAuthority));
            }

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
        public AccountsSettingsPaneConfig(string headerText = null, IList<SettingsCommand> commands = null)
        {
            HeaderText = headerText;
            Commands = commands;
        }
    }

    /// <summary>
    /// Configuration values for what type of authentication providers to enable.
    /// </summary>
    public struct WebAccountProviderConfig
    {
        /// <summary>
        /// Gets or sets the registered ClientId. Required for AAD login and admin consent.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the types of accounts providers that should be available to the user.
        /// </summary>
        public WebAccountProviderType WebAccountProviderType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebAccountProviderConfig"/> struct.
        /// </summary>
        /// <param name="webAccountProviderType">The types of accounts providers that should be available to the user.</param>
        /// <param name="clientId">The registered ClientId. Required for AAD login and admin consent.</param>
        public WebAccountProviderConfig(WebAccountProviderType webAccountProviderType, string clientId = null)
        {
            WebAccountProviderType = webAccountProviderType;
            ClientId = clientId;
        }
    }
}
