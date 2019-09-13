// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Microsoft.Toolkit.Graph
{
    /// <summary>
    /// Shared Provider used by controls in Microsoft.Toolkit.Graph.Controls to authenticate and call the Microsoft Graph.
    /// </summary>
    /// <example>To set your own existing provider:
    /// <code>
    /// TODO
    /// </code>
    /// </example>
    public class GlobalProvider
    {
        /// <summary>
        /// Gets the name of the toolkit client to identify self in Graph calls.
        /// </summary>
        public static readonly string ClientName = "Windows Community Toolkit" + ThisAssembly.AssemblyVersion;

        /// <summary>
        /// Gets the instance of the GlobalProvider
        /// </summary>
        public static GlobalProvider Instance => Singleton<GlobalProvider>.Instance;

        /// <summary>
        /// Gets the <see cref="GraphServiceClient"/> to access the MicrosoftGraph.
        /// </summary>
        public GraphServiceClient Graph { get; private set; }

        private IAuthenticationProvider _provider;

        /// <summary>
        /// Gets or sets the Provider used for calls to the Microsoft.Graph SDK. Automatically constructs a new <see cref="GraphServiceClient"/> with that provider.
        /// </summary>
        public IAuthenticationProvider Provider
        {
            get
            {
                return _provider;
            }

            set
            {
                _provider = value;
                if (Graph == null && _provider != null)
                {
                    Graph = new GraphServiceClient(_provider);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IPublicClientApplication"/> reference used for MSAL.
        /// </summary>
        public IPublicClientApplication Client { get; set; }

        /// <summary>
        /// Logs out all users.
        /// </summary>
        public async void Logout()
        {
            foreach (var user in await Client.GetAccountsAsync())
            {
                await Client.RemoveAsync(user);
            }
        }
    }
}
