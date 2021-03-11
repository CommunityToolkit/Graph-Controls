// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Auth;
using Microsoft.Graph;

namespace CommunityToolkit.Graph.Extensions
{
    /// <summary>
    /// Extension method for enabled Graph client access from an IProvider implementation.
    /// </summary>
    public static class ProviderExtensions
    {
        /// <summary>
        /// Gets the currently configured Graph service client, based on the active GlobalProvider instance.
        /// </summary>
        public static GraphServiceClient Client { get; private set; }

        static ProviderExtensions()
        {
            ProviderManager.Instance.ProviderUpdated += OnProviderUpdated;
        }

        private static void OnProviderUpdated(object sender, ProviderUpdatedEventArgs e)
        {
            if (e.Reason == ProviderManagerChangedState.ProviderChanged)
            {
                Client = null;
            }
        }

        /// <summary>
        /// Lazily gets a GraphServiceClient instance based on the current GlobalProvider.
        /// The client instance is cleared whenever the GlobalProvider changes.
        /// </summary>
        /// <param name="provider">The provider for authenticating Graph calls.</param>
        /// <returns>A GraphServiceClient instance.</returns>
        public static GraphServiceClient Graph(this IProvider provider)
        {
            if (Client == null)
            {
                Client = provider != null ? new GraphServiceClient(provider) : null;
            }

            return Client;
        }
    }
}
