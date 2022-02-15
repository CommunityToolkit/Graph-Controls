// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Authentication.Extensions;

namespace CommunityToolkit.Authentication
{
    /// <summary>
    /// Provider to connect to the example data set for Microsoft Graph. Useful for prototyping and samples.
    /// </summary>
    [Obsolete("MockProvider is meant for prototyping and demonstration purposes only. Not for use in production applications.")]
    public class MockProvider : BaseProvider
    {
        private const string GRAPH_PROXY_URL_REQUEST_ENDPOINT = "https://cdn.graph.office.net/en-us/graph/api/proxy/endpoint";
        private const string FALLBACK_GRAPH_PROXY_URL = "https://proxy.apisandbox.msdn.microsoft.com/svc?url=";

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private string _baseUrl = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockProvider"/> class.
        /// </summary>
        /// <param name="signedIn">Configuration for the MockProvider.</param>
        public MockProvider(bool signedIn = true)
        {
            State = signedIn ? ProviderState.SignedIn : ProviderState.SignedOut;
        }

        /// <inheritdoc />
        public override string CurrentAccountId => State == ProviderState.SignedIn ? "mock-account-id" : null;

        /// <inheritdoc/>
        public override async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            // Append the SDK version header
            AddSdkVersion(request);

            // Append the token auth header
            request.AddMockProviderToken();

            var baseUrl = await GetBaseUrlAsync();

            // Prepend Proxy Service URI
            var requestUri = request.RequestUri.ToString();
            request.RequestUri = new Uri(baseUrl + Uri.EscapeDataString(requestUri));
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

        private async Task<string> GetBaseUrlAsync()
        {
            await _semaphore.WaitAsync();

            if (_baseUrl == null)
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(GRAPH_PROXY_URL_REQUEST_ENDPOINT);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (responseContent.StartsWith("\"") && responseContent.EndsWith("\""))
                    {
                        responseContent = responseContent.Substring(1, responseContent.Length - 2);
                    }

                    _baseUrl = $"{responseContent}?url=";
                }
                else
                {
                    _baseUrl = FALLBACK_GRAPH_PROXY_URL;
                }
            }

            _semaphore.Release();

            return _baseUrl;
        }
    }
}
