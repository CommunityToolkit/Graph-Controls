// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace CommunityToolkit.Authentication
{
    /// <summary>
    /// Configuration values for building a new instance of the <see cref="PublicClientApplication"/> object.
    /// </summary>
    public class PublicClientApplicationConfig
    {
        // App settings

        /// <summary>
        /// Gets or sets the authority used to control which types of accounts can login.
        /// </summary>
        public AadAuthorityAudience Authority { get; set; } = AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount;

        /// <summary>
        /// Gets or sets the client id value from the Azure app registration.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client name value.
        /// </summary>
        public string ClientName { get; set; } = ProviderManager.ClientName;

        /// <summary>
        /// Gets or sets the client version value.
        /// </summary>
        public string ClientVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// Gets or sets the redirect uri value used to complete authentication.
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the array of authorization scopes.
        /// </summary>
        public string[] Scopes { get; set; }

        // Cache settings

        /// <summary>
        /// Gets or sets the name of the cache file.
        /// </summary>
        public string CacheFileName { get; set; } = "msal_cache.dat";

        /// <summary>
        /// Gets or sets the directory of the cache file.
        /// </summary>
        public string CacheDir { get; set; } = "MSAL_CACHE";

        /// <summary>
        /// Gets or sets the key chain service name.
        /// </summary>
        public string KeyChainServiceName { get; set; } = "msal_service";

        /// <summary>
        /// Gets or sets the key chain account name.
        /// </summary>
        public string KeyChainAccountName { get; set; } = "msal_account";

        /// <summary>
        /// Gets or sets the key ring schema on linux.
        /// </summary>
        public string LinuxKeyRingSchema { get; set; } = "com.msal.wct.tokencache";

        /// <summary>
        /// Gets or sets the key ring collection type on linux.
        /// </summary>
        public string LinuxKeyRingCollection { get; set; } = MsalCacheHelper.LinuxKeyRingDefaultCollection;

        /// <summary>
        /// Gets or sets the key ring label on linux.
        /// </summary>
        public string LinuxKeyRingLabel { get; set; } = "Default MSAL token cache for all Windows Community Toolkit based apps.";

        /// <summary>
        /// Gets or sets the first key ring attribute on linux.
        /// </summary>
        public KeyValuePair<string, string> LinuxKeyRingAttr1 { get; set; } = new KeyValuePair<string, string>("Version", "1");

        /// <summary>
        /// Gets or sets the second key ring attribute on linux.
        /// </summary>
        public KeyValuePair<string, string> LinuxKeyRingAttr2 { get; set; } = new KeyValuePair<string, string>("ProductGroup", "MyApps");

        // For Username / Password flow - to be used only for testing!
        // public const string Username = "";
        // public const string Password = "";
    }
}
