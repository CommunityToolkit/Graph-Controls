// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using Windows.UI.Core;

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// Put in a xaml page with ClientId
    /// https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-interactively
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;Interactivity:Interaction.Behaviors&gt;
    ///   &lt;providers:InteractiveProviderBehavior ClientId = "MyClientIdGuid"/&gt;
    /// &lt;/Interactivity:Interaction.Behaviors&gt;
    /// </code>
    /// </example>
    public class InteractiveProviderBehavior : CommonProviderBehaviorBase
    {
        private object lock_sync = new object();
        private bool initialized = false;

        /// <inheritdoc/>
        protected override bool Initialize()
        {
            lock (lock_sync)
            {
                if (!initialized)
                {
                    _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        ProviderManager.Instance.GlobalProvider =
                            await QuickCreate.CreateMsalProviderAsync(ClientId, RedirectUri, Scopes.ToArray());
                    });
                }
            }

            return base.Initialize();
        }
    }
}
