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
        /// <summary>
        /// A prefix value used to create the redirect URI value for use in AAD.
        /// </summary>
        public static readonly string MSAccountBrokerRedirectUriPrefix = "ms-appx-web://microsoft.aad.brokerplugin/";

        private static readonly SemaphoreSlim SemaphoreSlim = new (1);

        /// <summary>
        /// Gets or sets the currently authenticated user account.
        /// </summary>
        public IAccount Account { get; protected set; }

        /// <inheritdoc />
        public override string CurrentAccountId => Account?.HomeAccountId?.Identifier;

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
            AddSdkVersion(request);

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
            if (Account != null && State == ProviderState.SignedIn)
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
            if (Account != null || State != ProviderState.SignedOut)
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
            if (Account != null)
            {
                await Client.RemoveAsync(Account);
                Account = null;
            }

            State = ProviderState.SignedOut;
        }

        /// <inheritdoc/>
        public override Task<string> GetTokenAsync(bool silentOnly = false)
        {
            return this.GetTokenWithScopesAsync(Scopes, silentOnly);
        }

        /// <summary>
        /// Create an instance of <see cref="PublicClientApplication"/> using the provided config.
        /// </summary>
        /// <param name="config">An set of properties used to configure the <see cref="PublicClientApplication"/> creation.</param>
        /// <returns>A new instance of <see cref="PublicClientApplication"/>.</returns>
        protected IPublicClientApplication CreatePublicClientApplication(PublicClientApplicationConfig config)
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
                    ListWindowsWorkAndSchoolAccounts = config.ListWindowsWorkAndSchoolAccounts,
                });
#endif

            if (config.RedirectUri == null)
            {
#if WINDOWS_UWP
                string sid = WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper();
                config.RedirectUri = $"{MSAccountBrokerRedirectUriPrefix}{sid}";
#else
                config.RedirectUri = "http://localhost";
#endif
            }

            return clientBuilder.WithRedirectUri(config.RedirectUri).Build();
        }

        /// <summary>
        /// Initialize the token cache and if requested, attempt to sign in silently.
        /// </summary>
        /// <param name="trySignIn">A value indicating whether to attempt silent login after the cache configuration.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        protected async Task InitTokenCacheAsync(bool trySignIn)
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

        /// <summary>
        /// Retrieve an authorization token using the provided scopes.
        /// </summary>
        /// <param name="scopes">An array of scopes to pass along with the Graph request.</param>
        /// <param name="silentOnly">A value to determine whether account broker UI should be shown, if required by MSAL.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        protected async Task<string> GetTokenWithScopesAsync(string[] scopes, bool silentOnly = false)
        {
            await SemaphoreSlim.WaitAsync();

            try
            {
                AuthenticationResult authResult = null;
                try
                {
                    var account = Account ?? (await Client.GetAccountsAsync()).FirstOrDefault();
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

                        if (Account != null)
                        {
                            paramBuilder = paramBuilder.WithAccount(Account);
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

                Account = authResult?.Account;

                return authResult?.AccessToken;
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }
    }
}
