// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows;

namespace Microsoft.Toolkit.Wpf.Graph.Providers
{
    /// <summary>
    /// Properties for <see cref="CommonProviderBehaviorBase"/>.
    /// </summary>
    public partial class CommonProviderBehaviorBase
    {
        /// <summary>
        /// Gets or sets the Client ID (the unique application (client) ID assigned to your app by Azure AD when the app was registered).
        /// </summary>
        /// <remarks>
        /// For details about how to register an app and get a client ID,
        /// see the <a href="https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app">Register an app quick start</a>.
        /// </remarks>
        public string ClientId
        {
            get { return (string)GetValue(ClientIdProperty); }
            set { SetValue(ClientIdProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ClientId"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="ClientId"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ClientIdProperty =
            DependencyProperty.Register(nameof(ClientId), typeof(string), typeof(CommonProviderBehaviorBase), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the redirect URI (the URI the identity provider will send the security tokens back to).
        /// </summary>
        public string RedirectUri
        {
            get { return (string)GetValue(RedirectUriProperty); }
            set { SetValue(RedirectUriProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="RedirectUri"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="RedirectUri"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty RedirectUriProperty =
            DependencyProperty.Register(nameof(RedirectUri), typeof(string), typeof(CommonProviderBehaviorBase), new PropertyMetadata("http://localhost")); //// https://aka.ms/msal-net-os-browser

        /// <summary>
        /// Gets or sets the list of Scopes (permissions) to request on initial login.
        /// </summary>
        /// <remarks>
        /// This list can be modified by controls which require specific scopes to function. This will aid in requesting all scopes required by controls used before login is initiated, if using the LoginButton.
        /// </remarks>
        public ScopeSet Scopes { get; set; } = new ScopeSet { "User.Read", "User.ReadBasic.All" };
    }
}
