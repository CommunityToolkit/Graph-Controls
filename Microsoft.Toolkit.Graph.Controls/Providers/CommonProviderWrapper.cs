// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// Provides a common base class for UWP XAML based provider wrappers to the Microsoft.Graph.Auth SDK.
    /// </summary>
    public abstract partial class CommonProviderWrapper : DependencyObject
    {
        private static async void ClientIdPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CommonProviderWrapper provider)
            {
                await provider.InitializeAsync();
            }
        }

        /// <summary>
        /// Called by developers to easily initialize MSAL and a Provider when calling from WPF via XAML Islands.
        /// </summary>
        /// <typeparam name="T">CommonProvider type to initialize</typeparam>
        /// <param name="clientId">Optional shortcut to initialize the ClientId parameter</param>
        /// <param name="redirectUri">Optional shortcut to initialize the RedirectUri parameter</param>
        /// <returns>New <see cref="CommonProviderWrapper"/> instance.</returns>
        public static async Task<T> InitializeAsync<T>(string clientId = null, string redirectUri = null)
             where T : CommonProviderWrapper, new()
        {
            var provider = new T
            {
                ClientId = clientId,
                RedirectUri = redirectUri
            };

            await provider.InitializeAsync();

            return provider;
        }

        /// <summary>
        /// Used by controls to request the additional scopes they require.
        /// </summary>
        /// <param name="scopes">Scopes to request</param>
        public void RequestAdditionalScopes(params string[] scopes)
        {
            Scopes.AddRange(scopes);
        }

        /// <summary>
        /// Called when provider is initialized from XAML (UWP Only).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected abstract Task InitializeAsync();
    }
}
