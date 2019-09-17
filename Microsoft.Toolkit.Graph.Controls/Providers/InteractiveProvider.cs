// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// Put in app.xaml resources with ClientId
    /// https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-interactively
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;wgt:InteractiveProvider x:Key="MyProvider" ClientId="MyClientIdGuid"/%gt;
    /// </code>
    /// </example>
    public class InteractiveProvider : CommonProviderWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveProvider"/> class.
        /// </summary>
        public InteractiveProvider()
        {
        }

        /// <inheritdoc/>
        protected override async Task InitializeAsync()
        {
            var client = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
                .WithRedirectUri(RedirectUri)
                .WithClientName(ProviderManager.ClientName)
                .WithClientVersion(Assembly.GetExecutingAssembly().GetName().Version.ToString())
                .Build();

            var provider = new InteractiveAuthenticationProvider(client, Scopes);

            ProviderManager.Instance.GlobalProvider = await MsalProvider.CreateAsync(client, provider);
        }
    }
}
