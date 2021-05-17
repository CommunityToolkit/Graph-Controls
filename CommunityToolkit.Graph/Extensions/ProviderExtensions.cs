// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using Microsoft.Graph;

namespace CommunityToolkit.Graph.Extensions
{
    /// <summary>
    /// Extension method for enabled Graph client access from an IProvider implementation.
    /// </summary>
    public static class ProviderExtensions
    {
        private static GraphServiceClient _client;
        private static GraphServiceClient _betaClient;

        static ProviderExtensions()
        {
            ProviderManager.Instance.ProviderUpdated += OnProviderUpdated;
        }

        private static void OnProviderUpdated(object sender, ProviderUpdatedEventArgs e)
        {
            var providerManager = sender as ProviderManager;
            if (e.Reason == ProviderManagerChangedState.ProviderChanged || !(providerManager.GlobalProvider?.State == ProviderState.SignedIn))
            {
                _client = null;
                _betaClient = null;
            }
        }

        /// <summary>
        /// Lazily gets a GraphServiceClient instance based on the current GlobalProvider.
        /// The client instance is cleared whenever the GlobalProvider changes.
        /// </summary>
        /// <param name="provider">The provider for authenticating Graph calls.</param>
        /// <returns>A GraphServiceClient instance.</returns>
        public static GraphServiceClient GetClient(this IProvider provider)
        {
            if (_client == null && provider?.State == ProviderState.SignedIn)
            {
                _client = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    await provider.AuthenticateRequestAsync(requestMessage);
                }));
            }

            return _client;
        }

        /// <summary>
        /// Lazily gets a GraphServiceClient instance based on the current GlobalProvider, but configured for the beta endpoint.
        /// The beta client instance is cleared whenever the GlobalProvider changes.
        /// </summary>
        /// <param name="provider">The provider for authenticating Graph calls.</param>
        /// <returns>A GraphServiceClient instance configured for the beta endpoint.</returns>
        public static GraphServiceClient GetBetaClient(this IProvider provider)
        {
            if (_betaClient == null && provider?.State == ProviderState.SignedIn)
            {
                _betaClient = new GraphServiceClient("https://graph.microsoft.com/beta", new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    await provider.AuthenticateRequestAsync(requestMessage);
                }));
            }

            return _betaClient;
        }
    }
}
