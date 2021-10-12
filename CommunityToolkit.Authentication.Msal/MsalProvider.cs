// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

#if WINDOWS_UWP
using Windows.Security.Authentication.Web;
#else
using System.Diagnostics;
using Microsoft.Identity.Client.Extensions.Msal;
#endif

namespace CommunityToolkit.Authentication
{
    /// <summary>
    /// <a href="https://github.com/AzureAD/microsoft-authentication-library-for-dotnet">MSAL.NET</a> provider helper for tracking authentication state.
    /// </summary>
    public class MsalProvider : BaseProvider
    {
        public static readonly string RedirectUriPrefix = "ms-appx-web://microsoft.aad.brokerplugin/";

        private static readonly SemaphoreSlim SemaphoreSlim = new (1);

        private IAccount _account;

        /// <inheritdoc />
        public override string CurrentAccountId => _account?.HomeAccountId?.Identifier;

        /// <summary>
        /// Gets the configuration values for creating the <see cref="PublicClientApplication"/> instance.
        /// </summary>
        protected PublicClientApplicationConfig Config { get; private set; }

        /// <summary>
        /// Gets the MSAL.NET Client used to authenticate the user.
        /// </summary>
        protected IPublicClientApplication Client { get; private set; }

        /// <summary>
        /// Gets an array of scopes to use for accessing Graph resources.
        /// </summary>
        protected string[] Scopes => Config.Scopes;

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalProvider"/> class using a configuration object.
        /// </summary>
        /// <param name="config">Configuration values for building the <see cref="PublicClientApplication"/> instance.</param>
        /// <param name="autoSignIn">Determines whether the provider attempts to silently log in upon creation.</param>
        public MsalProvider(PublicClientApplicationConfig config, bool autoSignIn = true)
        {
            Config = config;
            Client = CreatePublicClientApplication(config);

            InitTokenCacheAsync(autoSignIn);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MsalProvider"/> class with default configuration values.
        /// </summary>
        /// <param name="clientId">Registered ClientId.</param>
        /// <param name="redirectUri">RedirectUri for auth response.</param>
        /// <param name="scopes">List of Scopes to initially request.</param>
        /// <param name="autoSignIn">Determines whether the provider attempts to silently log in upon creation.</param>
        public MsalProvider(string clientId, string[] scopes = null, string redirectUri = null, bool autoSignIn = true)
        {
            Config = new PublicClientApplicationConfig()
            {
                ClientId = clientId,
                Scopes = scopes.Select(s => s.ToLower()).ToArray() ?? new string[] { string.Empty },
                RedirectUri = redirectUri,
            };

            Client = CreatePublicClientApplication(Config);

            InitTokenCacheAsync(autoSignIn);
        }

        /// <inheritdoc/>
        public override async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            string token;

            // Check if any specific scopes are being requested.
            if (request.Properties.TryGetValue(nameof(GraphRequestContext), out object requestContextObj) &&
                requestContextObj is GraphRequestContext requestContext &&
                requestContext.MiddlewareOptions.TryGetValue(nameof(AuthenticationHandlerOption), out IMiddlewareOption optionsMiddleware) &&
                optionsMiddleware is AuthenticationHandlerOption options &&
                options.AuthenticationProviderOption?.Scopes != null && options.AuthenticationProviderOption.Scopes.Length > 0)
            {
                var withScopes = options.AuthenticationProviderOption.Scopes;
                token = await this.GetTokenWithScopesAsync(withScopes);
            }
            else
            {
                token = await this.GetTokenAsync();
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <inheritdoc/>
        public override async Task<bool> TrySilentSignInAsync()
        {
            if (_account != null && State == ProviderState.SignedIn)
            {
                return true;
            }

            State = ProviderState.Loading;

            var token = await GetTokenAsync(true);
            if (token == null)
            {
                await SignOutAsync();
                return false;
            }

            State = ProviderState.SignedIn;
            return true;
        }

        /// <inheritdoc/>
        public override async Task SignInAsync()
        {
            if (_account != null || State != ProviderState.SignedOut)
            {
                return;
            }

            State = ProviderState.Loading;

            var token = await GetTokenAsync();
            if (token == null)
            {
                await SignOutAsync();
            }
            else
            {
                State = ProviderState.SignedIn;
            }
        }

        /// <inheritdoc />
        public override async Task SignOutAsync()
        {
            if (_account != null)
            {
                await Client.RemoveAsync(_account);
                _account = null;
            }

            State = ProviderState.SignedOut;
        }

        /// <inheritdoc/>
        public override Task<string> GetTokenAsync(bool silentOnly = false)
        {
            return this.GetTokenWithScopesAsync(Scopes, silentOnly);
        }

        private static IPublicClientApplication CreatePublicClientApplication(PublicClientApplicationConfig config)
        {
            var clientBuilder = PublicClientApplicationBuilder.Create(config.ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, config.Authority)
                .WithClientName(config.ClientName)
                .WithClientVersion(config.ClientVersion);

#if WINDOWS_UWP
            clientBuilder = clientBuilder
                .WithBroker()
                .WithWindowsBrokerOptions(new WindowsBrokerOptions()
                {
                    ListWindowsWorkAndSchoolAccounts = true,
                });
#endif

            if (config.RedirectUri == null)
            {
#if WINDOWS_UWP
                string sid = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper();
                config.RedirectUri = $"{RedirectUriPrefix}{sid}";
#else
                config.RedirectUri = "http://localhost";
                // config.RedirectUri = $"{RedirectUriPrefix}{config.ClientId}";
#endif
            }

            return clientBuilder.WithRedirectUri(config.RedirectUri).Build();
        }

        private async Task InitTokenCacheAsync(bool trySignIn)
        {
#if !WINDOWS_UWP
            // Token cache persistence (not required on UWP as MSAL does it for you)
            var storageProperties = new StorageCreationPropertiesBuilder(Config.CacheFileName, Config.CacheDir)
                .WithLinuxKeyring(
                    Config.LinuxKeyRingSchema,
                    Config.LinuxKeyRingCollection,
                    Config.LinuxKeyRingLabel,
                    Config.LinuxKeyRingAttr1,
                    Config.LinuxKeyRingAttr2)
                .WithMacKeyChain(
                    Config.KeyChainServiceName,
                    Config.KeyChainAccountName)
                .Build();

            var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
            cacheHelper.RegisterCache(Client.UserTokenCache);
#endif

            if (trySignIn)
            {
                _ = TrySilentSignInAsync();
            }
        }

        private async Task<string> GetTokenWithScopesAsync(string[] scopes, bool silentOnly = false)
        {
            await SemaphoreSlim.WaitAsync();

            try
            {
                AuthenticationResult authResult = null;
                try
                {
                    var account = _account ?? (await Client.GetAccountsAsync()).FirstOrDefault();
                    if (account != null)
                    {
                        authResult = await Client.AcquireTokenSilent(scopes, account).ExecuteAsync();
                    }
                }
                catch (MsalUiRequiredException)
                {
                }
                catch
                {
                    // Unexpected exception
                    // TODO: Send exception to a logger.
                }

                if (authResult == null && !silentOnly)
                {
                    try
                    {
                        var paramBuilder = Client.AcquireTokenInteractive(scopes);

                        if (_account != null)
                        {
                            paramBuilder = paramBuilder.WithAccount(_account);
                        }

#if WINDOWS_UWP
                        // For UWP, specify NoPrompt for the least intrusive user experience.
                        paramBuilder = paramBuilder.WithPrompt(Prompt.NoPrompt);
#else
                        // Otherwise, get the process by FriendlyName from Application Domain
                        var friendlyName = AppDomain.CurrentDomain.FriendlyName;
                        var proc = Process.GetProcessesByName(friendlyName).First();

                        var windowHandle = proc.MainWindowHandle;
                        paramBuilder = paramBuilder.WithParentActivityOrWindow(windowHandle);
#endif

                        authResult = await paramBuilder.ExecuteAsync();
                    }
                    catch
                    {
                        // Unexpected exception
                        // TODO: Send exception to a logger.
                    }
                }

                _account = authResult?.Account;

                return authResult?.AccessToken;
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }
    }
}
