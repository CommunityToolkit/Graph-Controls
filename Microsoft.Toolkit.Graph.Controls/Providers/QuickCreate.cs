// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Microsoft.Toolkit.Graph.Providers;

#if DOTNET
namespace Microsoft.Toolkit.Wpf.Graph.Providers
#else
namespace Microsoft.Toolkit.Graph.Providers
#endif
{
    //// TODO: This should probably live in .NET Standard lib; however, Uno one needs to be at UI layer for Parent Window?
    //// TODO: Need to set up XAML Islands sample to test in the new repo and make sure this works from this context.

    /// <summary>
    /// Helper class for XAML Islands to easily initialize provider from just ClientId from within WPF context.
    /// </summary>
    public static class QuickCreate
    {
        /// <summary>
        /// Easily creates a <see cref="MsalProvider"/> from a ClientId.
        /// </summary>
        /// <example>
        /// <code>
        /// ProviderManager.Instance.GlobalProvider = await QuickCreate.CreateMsalProviderAsync("MyClientId");
        /// </code>
        /// </example>
        /// <param name="clientid">Registered ClientId</param>
        /// <param name="redirectUri">RedirectUri for auth response.</param>
        /// <param name="scopes">List of Scopes to initially request.</param>
        /// <returns>New <see cref="MsalProvider"/> reference.</returns>
        public static async Task<MsalProvider> CreateMsalProviderAsync(string clientid, string redirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient", string[] scopes = null)
        {
            var client = PublicClientApplicationBuilder.Create(clientid)
                .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
                .WithRedirectUri(redirectUri)
                .WithClientName(ProviderManager.ClientName)
                .WithClientVersion(Assembly.GetExecutingAssembly().GetName().Version.ToString())
                .Build();

            if (scopes == null)
            {
                scopes = new string[] { string.Empty };
            }

            var provider = new InteractiveAuthenticationProvider(client, scopes);

            return await MsalProvider.CreateAsync(client, provider);
        }
    }
}
