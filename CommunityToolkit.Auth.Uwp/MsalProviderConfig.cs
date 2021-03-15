// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;

namespace CommunityToolkit.Auth.Uwp
{
    /// <summary>
    /// Configuration values for initializing the MsalProvider.
    /// </summary>
    public class MsalProviderConfig
    {
        static MsalProviderConfig()
        {
            GlobalProvider.RegisterConfig<MsalProviderConfig>((c) => Factory(c as MsalProviderConfig));
        }

        /// <summary>
        /// Static helper method for creating a new MsalProvider instance from this config object.
        /// </summary>
        /// <param name="config">The configuration for the provider.</param>
        /// <returns>A new instance of the MsalProvider based on the provided config.</returns>
        public static MsalProvider Factory(MsalProviderConfig config)
        {
            return new MsalProvider(
                clientid: config.ClientId,
                redirectUri: config.RedirectUri,
                scopes: config.Scopes.ToArray());
        }

        /// <summary>
        /// Gets or sets the Client ID (the unique application (client) ID assigned to your app by Azure AD when the app was registered).
        /// </summary>
        /// <remarks>
        /// For details about how to register an app and get a client ID,
        /// see the <a href="https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app">Register an app quick start</a>.
        /// </remarks>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI (the URI the identity provider will send the security tokens back to).
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the list of Scopes (permissions) to request on initial login.
        /// </summary>
        /// <remarks>
        /// This list can be modified by controls which require specific scopes to function.
        /// This will aid in requesting all scopes required by controls used before login is initiated, if using the LoginButton.
        /// </remarks>
        public ScopeSet Scopes { get; set; }
    }
}