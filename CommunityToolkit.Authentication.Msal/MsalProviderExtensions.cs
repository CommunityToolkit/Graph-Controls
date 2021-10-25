// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Extensions.Msal;

namespace CommunityToolkit.Authentication.Extensions
{
    /// <summary>
    /// Helpers for working with the MsalProvider.
    /// </summary>
    public static class MsalProviderExtensions
    {
        /// <summary>
        /// Helper function to initialize the token cache for non-UWP apps. MSAL handles this automatically on UWP.
        /// </summary>
        /// <param name="provider">The instance of <see cref="MsalProvider"/> to init the cache for.</param>
        /// <param name="storageProperties">Properties for configuring the storage cache.</param>
        /// <param name="logger">Passing null uses the default TraceSource logger.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public static async Task InitTokenCacheAsync(
            this MsalProvider provider,
            StorageCreationProperties storageProperties,
            TraceSource logger = null)
        {
#if !WINDOWS_UWP
            // Token cache persistence (not required on UWP as MSAL does it for you)
            var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties, logger);
            cacheHelper.RegisterCache(provider.Client.UserTokenCache);
#endif
        }
    }
}
