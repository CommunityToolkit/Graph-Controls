// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;

namespace CommunityToolkit.Authentication
{
    /// <summary>
    /// An authentication provider based on the native AccountsSettingsPane in Windows.
    /// </summary>
    public class WindowsProvider : BaseProvider
    {
        private const string AuthenticationHeaderScheme = "Bearer";
        private const string GraphResourcePropertyKey = "resource";
        private const string GraphResourcePropertyValue = "https://graph.microsoft.com";
        private const string MicrosoftAccountAuthority = "consumers";
        private const string AadAuthority = "organizations";
        private const string LocalProviderId = "https://login.windows.local";
        private const string MicrosoftProviderId = "https://login.microsoft.com";
        private const string SettingsKeyAccountId = "WindowsProvider_AccountId";
        private const string SettingsKeyProviderId = "WindowsProvider_ProviderId";
        private const string SettingsKeyProviderAuthority = "WindowsProvider_Authority";

        private static readonly SemaphoreSlim SemaphoreSlim = new(1);

        // Default/minimal scopes for authentication, if none are provided.
        private static readonly string[] DefaultScopes = { "User.Read" };

        // The default account providers available in the AccountsSettingsPane.
        // Default is Msa because it does not require any additional configuration
        private static readonly WebAccountProviderType DefaultWebAccountsProviderType = WebAccountProviderType.Msa;

        /// <summary>
        /// Gets the redirect uri value based on the current app callback uri.
        /// Used for configuring the Azure app registration.
        /// </summary>
        public static string RedirectUri => string.Format("ms-appx-web://Microsoft.AAD.BrokerPlugIn/{0}", WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper());

        /// <inheritdoc />
        public override string CurrentAccountId => _webAccount?.Id;

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
        /// Gets or sets which DispatcherQueue is used to dispatch UI updates.
        /// </summary>
        public DispatcherQueue DispatcherQueue { get; set; }

        /// <summary>
        /// Gets a cache of important values for the signed in user.
        /// </summary>
        protected IDictionary<string, object> Settings => ApplicationData.Current.LocalSettings.Values;

        private readonly string[] _scopes;
        private readonly AccountsSettingsPaneConfig? _accountsSettingsPaneConfig;
        private readonly WebAccountProviderConfig _webAccountProviderConfig;
        private WebAccount _webAccount = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsProvider"/> class.
        /// </summary>
        /// <param name="scopes">List of Scopes to initially request.</param>
        /// <param name="accountsSettingsPaneConfig">Configuration values for the AccountsSettingsPane.</param>
        /// <param name="webAccountProviderConfig">Configuration value for determining the available web account providers.</param>
        /// <param name="autoSignIn">Determines whether the provider attempts to silently log in upon construction.</param>
        /// <param name="dispatcherQueue">The DispatcherQueue that should be used to dispatch UI updates, or null if this is being called from the UI thread.</param>
        public WindowsProvider(string[] scopes = null, WebAccountProviderConfig? webAccountProviderConfig = null, AccountsSettingsPaneConfig? accountsSettingsPaneConfig = null, bool autoSignIn = true, DispatcherQueue dispatcherQueue = null)
        {
            _scopes = scopes ?? DefaultScopes;
            _webAccountProviderConfig = webAccountProviderConfig ?? new WebAccountProviderConfig()
            {
                WebAccountProviderType = DefaultWebAccountsProviderType,
            };
            _accountsSettingsPaneConfig = accountsSettingsPaneConfig;

            DispatcherQueue = dispatcherQueue ?? DispatcherQueue.GetForCurrentThread();

            State = ProviderState.SignedOut;

            if (autoSignIn)
            {
                _ = TrySilentSignInAsync();
            }
        }

        /// <inheritdoc />
        public override async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            AddSdkVersion(request);

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

            State = ProviderState.Loading;

            // The state will get updated as part of the auth flow.
            var token = await GetTokenAsync();

            if (token == null)
            {
                await SignOutAsync();
            }
        }

        /// <inheritdoc />
        public override async Task<bool> TrySilentSignInAsync()
        {
            if (_webAccount != null && State == ProviderState.SignedIn)
            {
                return true;
            }

            State = ProviderState.Loading;

            // The state will get updated as part of the auth flow.
            var token = await GetTokenAsync(true);

            if (token == null)
            {
                State = ProviderState.SignedOut;
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override async Task SignOutAsync()
        {
            State = ProviderState.Loading;

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

        /// <inheritdoc />
        public override async Task<string> GetTokenAsync(bool silentOnly = false)
        {
            await SemaphoreSlim.WaitAsync();

            try
            {
                var scopes = _scopes;

                // Attempt to authenticate silently.
                var authResult = await AuthenticateSilentAsync(scopes);

                // Authenticate with user interaction as appropriate.
                if (authResult?.ResponseStatus != WebTokenRequestStatus.Success)
                {
                    if (silentOnly)
                    {
                        // Silent login may fail if we don't have a cached account, and that's ok.
                        return null;
                    }

                    // Attempt to authenticate interactively.
                    authResult = await AuthenticateInteractiveAsync(scopes);
                }

                if (authResult?.ResponseStatus == WebTokenRequestStatus.Success)
                {
                    var newAccount = authResult.ResponseData[0].WebAccount;
                    await SetAccountAsync(newAccount);

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
                    throw new Exception("Token request was not successful, but is also missing an error message.");
                }
            }
            catch (Exception e)
            {
                // TODO: Log failure
                throw e;
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        /// <summary>
        /// Display AccountSettingsPane for the management of logged-in users.
        /// </summary>
        /// <returns><see cref="Task"/>.</returns>
        public Task ShowAccountManagementPaneAsync()
        {
            if (_webAccount == null)
            {
                throw new InvalidOperationException("A logged in account is required to display the account management pane.");
            }

            var tcs = new TaskCompletionSource<bool>();
            var taskQueued = DispatcherQueue.TryEnqueue(async () =>
            {
                AccountsSettingsPane pane = null;
                try
                {
                    // GetForCurrentView may throw an exception if the current view isn't ready yet.
                    pane = AccountsSettingsPane.GetForCurrentView();
                    pane.AccountCommandsRequested += OnAccountCommandsRequested;

                    // Show the AccountSettingsPane and wait for the result.
                    await AccountsSettingsPane.ShowManageAccountsAsync();

                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
                finally
                {
                    if (pane != null)
                    {
                        pane.AccountCommandsRequested -= OnAccountCommandsRequested;
                    }
                }
            });

            if (!taskQueued)
            {
                tcs.SetException(new InvalidOperationException("Failed to enqueue the operation."));
            }

            return tcs.Task;
        }

        /// <summary>
        /// Build the AccountSettingsPane and configure it with available account commands.
        /// </summary>
        /// <param name="sender">The pane that fired the event.</param>
        /// <param name="e">Arguments for the AccountCommandsRequested event.</param>
        private void OnAccountCommandsRequested(AccountsSettingsPane sender, AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            AccountsSettingsPaneEventDeferral deferral = e.GetDeferral();

            // Apply the configured header.
            var headerText = _accountsSettingsPaneConfig?.ManageAccountHeaderText;
            if (!string.IsNullOrWhiteSpace(headerText))
            {
                e.HeaderText = headerText;
            }

            // Generate any account commands.
            if (_accountsSettingsPaneConfig?.AccountCommandParameter != null)
            {
                var commandParameter = _accountsSettingsPaneConfig.Value.AccountCommandParameter;
                var webAccountCommand = new WebAccountCommand(
                    _webAccount,
                    async (command, args) =>
                    {
                        // When the logout command is triggered, we also need to modify the state of the Provider.
                        if (args.Action == WebAccountAction.Remove)
                        {
                            await SignOutAsync();
                        }

                        commandParameter.Invoked?.Invoke(command, args);
                    },
                    commandParameter.Actions);

                e.WebAccountCommands.Add(webAccountCommand);
            }

            // Apply any configured setting commands.
            var commands = _accountsSettingsPaneConfig?.Commands;
            if (commands != null)
            {
                foreach (var command in commands)
                {
                    e.Commands.Add(command);
                }
            }

            deferral.Complete();
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
            Settings[SettingsKeyProviderAuthority] = account.WebAccountProvider.Authority;

            State = ProviderState.SignedIn;
        }

        private async Task<WebTokenRequestResult> AuthenticateSilentAsync(string[] scopes)
        {
            try
            {
                WebTokenRequestResult authResult = null;

                var account = _webAccount;
                if (account == null)
                {
                    // Check the cache for an existing user
                    if (Settings[SettingsKeyAccountId] is string savedAccountId &&
                        Settings[SettingsKeyProviderId] is string savedProviderId &&
                        Settings[SettingsKeyProviderAuthority] is string savedProviderAuthority)
                    {
                        var savedProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(savedProviderId, savedProviderAuthority);
                        account = await WebAuthenticationCoreManager.FindAccountAsync(savedProvider, savedAccountId);
                    }
                }

                if (account != null)
                {
                    // Prepare a request to get a token.
                    var webTokenRequest = GetWebTokenRequest(account.WebAccountProvider, _webAccountProviderConfig.ClientId, scopes);
                    authResult = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(webTokenRequest, account);
                }

                return authResult;
            }
            catch (HttpRequestException)
            {
                throw; /* probably offline, no point continuing to interactive auth */
            }
        }

        private Task<WebTokenRequestResult> AuthenticateInteractiveAsync(string[] scopes)
        {
            var tcs = new TaskCompletionSource<WebTokenRequestResult>();
            var taskQueued = DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    WebTokenRequestResult authResult = null;

                    var account = _webAccount;
                    if (account != null)
                    {
                        // We already have the account.
                        var webAccountProvider = account.WebAccountProvider;
                        var webTokenRequest = GetWebTokenRequest(webAccountProvider, _webAccountProviderConfig.ClientId, scopes);
                        authResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest, account);
                    }
                    else
                    {
                        // We don't have an account. Prompt the user to provide one.
                        var webAccountProvider = await ShowAccountSettingsPaneAndGetProviderAsync();
                        var webTokenRequest = GetWebTokenRequest(webAccountProvider, _webAccountProviderConfig.ClientId, scopes);
                        authResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest);
                    }

                    tcs.SetResult(authResult);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });

            if (!taskQueued)
            {
                tcs.SetException(new InvalidOperationException("Failed to enqueue the operation."));
            }

            return tcs.Task;
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
                    var headerText = _accountsSettingsPaneConfig?.AddAccountHeaderText;
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

        private WebTokenRequest GetWebTokenRequest(WebAccountProvider provider, string clientId, string[] scopes)
        {
            string scopesString = string.Join(' ', scopes);

            WebTokenRequest webTokenRequest = string.IsNullOrWhiteSpace(clientId)
                ? new WebTokenRequest(provider, scopesString)
                : new WebTokenRequest(provider, scopesString, clientId);

            webTokenRequest.Properties.Add(GraphResourcePropertyKey, GraphResourcePropertyValue);
            if (provider.Authority == MicrosoftAccountAuthority)
            {
                foreach (var property in _webAccountProviderConfig.MSATokenRequestProperties)
                {
                    webTokenRequest.Properties.Add(property);
                }
            }
            else if (provider.Authority == AadAuthority)
            {
                foreach (var property in _webAccountProviderConfig.AADTokenRequestProperties)
                {
                    webTokenRequest.Properties.Add(property);
                }
            }

            return webTokenRequest;
        }

        private async Task<List<WebAccountProvider>> GetWebAccountProvidersAsync()
        {
            var providers = new List<WebAccountProvider>();

            // MSA
            if (_webAccountProviderConfig.WebAccountProviderType == WebAccountProviderType.Any ||
                _webAccountProviderConfig.WebAccountProviderType == WebAccountProviderType.Msa)
            {
                await FindAndAddProviderAsync(MicrosoftProviderId, MicrosoftAccountAuthority);
            }

            // AAD
            if (_webAccountProviderConfig.WebAccountProviderType == WebAccountProviderType.Any ||
                _webAccountProviderConfig.WebAccountProviderType == WebAccountProviderType.Aad)
            {
                await FindAndAddProviderAsync(MicrosoftProviderId, AadAuthority);
            }

            if (_webAccountProviderConfig.WebAccountProviderType == WebAccountProviderType.Local)
            {
                await FindAndAddProviderAsync(LocalProviderId);
            }

            return providers;

            async Task FindAndAddProviderAsync(
                string webAccountProviderId,
                string authority = default)
            {
                var provider = string.IsNullOrEmpty(authority)
                    ? await WebAuthenticationCoreManager.FindAccountProviderAsync(webAccountProviderId)
                    : await WebAuthenticationCoreManager.FindAccountProviderAsync(webAccountProviderId, authority);
                if (provider != null)
                {
                    providers.Add(provider);
                }
            }
        }
    }
}