// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Threading.Tasks;

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
            ProviderManager.Instance.GlobalProvider =
                await QuickCreate.CreateMsalProviderAsync(ClientId, RedirectUri, Scopes.ToArray());
        }
    }
}
