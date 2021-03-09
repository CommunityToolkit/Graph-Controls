// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers.Extensions;

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// Provider to connect to the example data set for Microsoft Graph. Useful for prototyping and samples.
    /// </summary>
    public class MockProvider : BaseProvider
    {
        private const string GRAPH_PROXY_URL = "https://proxy.apisandbox.msdn.microsoft.com/svc?url=";

        /// <inheritdoc/>
        public override GraphServiceClient Graph => new GraphServiceClient(
                        new DelegateAuthenticationProvider((requestMessage) =>
                    {
                        var requestUri = requestMessage.RequestUri.ToString();

                        // Prepend Proxy Service URI to our request
                        requestMessage.RequestUri = new Uri(GRAPH_PROXY_URL + Uri.EscapeDataString(requestUri));

                        return this.AuthenticateRequestAsync(requestMessage);
                    }));

        /// <summary>
        /// Initializes a new instance of the <see cref="MockProvider"/> class.
        /// </summary>
        /// <param name="config">Configuration for the MockProvider.</param>
        public MockProvider(MockConfig config = null)
        {
            if (config == null || config.SignedIn)
            {
                State = ProviderState.SignedIn;
            }
            else
            {
                State = ProviderState.SignedOut;
            }
        }

        /// <inheritdoc/>
        public override Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            request.AddSdkVersion();

            request.AddMockProviderToken();

            return Task.FromResult(0);
        }

        /// <inheritdoc/>
        public override async Task LoginAsync()
        {
            State = ProviderState.Loading;
            await Task.Delay(3000);
            State = ProviderState.SignedIn;
        }

        /// <inheritdoc/>
        public override async Task LogoutAsync()
        {
            State = ProviderState.Loading;
            await Task.Delay(3000);
            State = ProviderState.SignedOut;
        }
    }
}
