// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Authentication;
using CommunityToolkit.Authentication.Extensions;
using Microsoft.Identity.Client.Extensions.Msal;

namespace WpfNetWindowsMsalProviderSample
{
    public partial class App : Application
    {
        static readonly string ClientId = "YOUR-CLIENT-ID-HERE";
        static readonly string[] Scopes = new string[] { "User.Read" };

        protected override void OnActivated(EventArgs e)
        {
            InitializeGlobalProviderAsync();
            base.OnActivated(e);
        }

        private async Task InitializeGlobalProviderAsync()
        {
            if (ProviderManager.Instance.GlobalProvider == null)
            {
                var provider = new MsalProvider(ClientId, Scopes, null, false, true);

                // Configure the token cache storage for non-UWP applications.
                var storageProperties = new StorageCreationPropertiesBuilder(CacheConfig.CacheFileName, CacheConfig.CacheDir)
                    .WithLinuxKeyring(
                        CacheConfig.LinuxKeyRingSchema,
                        CacheConfig.LinuxKeyRingCollection,
                        CacheConfig.LinuxKeyRingLabel,
                        CacheConfig.LinuxKeyRingAttr1,
                        CacheConfig.LinuxKeyRingAttr2)
                    .WithMacKeyChain(
                        CacheConfig.KeyChainServiceName,
                        CacheConfig.KeyChainAccountName)
                    .Build();
                await provider.InitTokenCacheAsync(storageProperties);

                ProviderManager.Instance.GlobalProvider = provider;

                await provider.TrySilentSignInAsync();
            }
        }
    }
}
