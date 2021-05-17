// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Net.Authentication.Extensions;

namespace CommunityToolkit.Net.Authentication
{
    /// <summary>
    /// Provider to connect to the example data set for Microsoft Graph. Useful for prototyping and samples.
    /// </summary>
    [Obsolete("MockProvider is meant for prototyping and demonstration purposes only. Not for use in production applications.")]
    public class MockProvider : BaseProvider
    {
        private const string GRAPH_PROXY_URL = "https://proxy.apisandbox.msdn.microsoft.com/svc?url=";

        /// <summary>
        /// Initializes a new instance of the <see cref="MockProvider"/> class.
        /// </summary>
        /// <param name="signedIn">Configuration for the MockProvider.</param>
        public MockProvider(bool signedIn = true)
        {
            State = signedIn ? ProviderState.SignedIn : ProviderState.SignedOut;
        }

        /// <inheritdoc/>
        public override Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            // Append the SDK version header
            AddSdkVersion(request);

            // Append the token auth header
            request.AddMockProviderToken();

            // Prepend Proxy Service URI
            var requestUri = request.RequestUri.ToString();
            request.RequestUri = new Uri(GRAPH_PROXY_URL + Uri.EscapeDataString(requestUri));

            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        public override Task<string> GetTokenAsync(bool silentOnly = false)
        {
            return Task.FromResult("<mock-provider-token>");
        }

        /// <inheritdoc/>
        public override async Task SignInAsync()
        {
            if (State == ProviderState.SignedIn)
            {
                return;
            }

            State = ProviderState.Loading;
            await Task.Delay(3000);
            State = ProviderState.SignedIn;
        }

        /// <inheritdoc/>
        public override async Task SignOutAsync()
        {
            if (State == ProviderState.SignedOut)
            {
                return;
            }

            State = ProviderState.Loading;
            await Task.Delay(3000);
            State = ProviderState.SignedOut;
        }

        /// <inheritdoc/>
        public override async Task<bool> TrySilentSignInAsync()
        {
            if (State == ProviderState.SignedIn)
            {
                return true;
            }

            await SignInAsync();
            return true;
        }
    }
}
