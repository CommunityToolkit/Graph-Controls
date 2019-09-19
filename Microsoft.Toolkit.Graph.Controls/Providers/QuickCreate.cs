// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Microsoft.Toolkit.Graph.Providers;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Toolkit.Graph.Providers
{
    public static class QuickCreate
    {
        public static async Task<MsalProvider> CreateMsalProviderAsync(string clientid, string redirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient", string[] scopes = null)
        {
            var client = PublicClientApplicationBuilder.Create(clientid)
                .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
                .WithRedirectUri(redirectUri)
                .WithClientName(ProviderManager.ClientName)
                .WithClientVersion(Assembly.GetExecutingAssembly().GetName().Version.ToString())
#if __ANDROID__
                .WithParentActivityOrWindow(() => Uno.UI.ContextHelper.Current as Android.App.Activity)
#endif
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
