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
        /// <summary>
        /// Gets a GraphServiceClient instance based on the current GlobalProvider.
        /// </summary>
        /// <param name="provider">The provider for authenticating Graph calls.</param>
        /// <returns>A GraphServiceClient instance.</returns>
        public static GraphServiceClient GetClient(this IProvider provider)
        {
            return new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
            {
                await provider.AuthenticateRequestAsync(requestMessage);
            }));
        }

        /// <summary>
        /// Gets a GraphServiceClient instance based on the current GlobalProvider, but configured for the beta endpoint.
        /// </summary>
        /// <param name="provider">The provider for authenticating Graph calls.</param>
        /// <returns>A GraphServiceClient instance configured for the beta endpoint.</returns>
        public static GraphServiceClient GetBetaClient(this IProvider provider)
        {
            return new GraphServiceClient("https://graph.microsoft.com/beta", new DelegateAuthenticationProvider(async (requestMessage) =>
            {
                await provider.AuthenticateRequestAsync(requestMessage);
            }));
        }
    }
}
