// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Toolkit.Helpers;

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// Shared provider manager used by controls in Microsoft.Toolkit.Graph.Controls to authenticate and call the Microsoft Graph.
    /// </summary>
    /// <example>To set your own existing provider:
    /// <code>
    /// ProviderManager.Instance.GlobalProvider = await MsalProvider.CreateAsync(...);
    /// </code>
    /// </example>
    public class ProviderManager
    {
        /// <summary>
        /// Gets the name of the toolkit client to identify self in Graph calls.
        /// </summary>
        public static readonly string ClientName = "Windows Community Toolkit" + ThisAssembly.AssemblyVersion;

        /// <summary>
        /// Gets the instance of the GlobalProvider
        /// </summary>
        public static ProviderManager Instance => Singleton<ProviderManager>.Instance;

        /// <summary>
        /// Event called when the <see cref="IProvider"/> changes.
        /// </summary>
        public event EventHandler<ProviderUpdatedEventArgs> ProviderUpdated;

        private IProvider _provider;

        /// <summary>
        /// Gets or sets the global provider used by all Microsoft.Toolkit.Graph.Controls.
        /// </summary>
        public IProvider GlobalProvider
        {
            get
            {
                return _provider;
            }

            set
            {
                if (_provider != null)
                {
                    _provider.StateChanged -= ProviderStateChanged;
                }

                _provider = value;

                if (_provider != null)
                {
                    _provider.StateChanged += ProviderStateChanged;
                }

                ProviderUpdated?.Invoke(this, new ProviderUpdatedEventArgs(ProviderManagerChangedState.ProviderChanged));
            }
        }

        private void ProviderStateChanged(object sender, StateChangedEventArgs e)
        {
            ProviderUpdated?.Invoke(this, new ProviderUpdatedEventArgs(ProviderManagerChangedState.ProviderStateChanged));
        }
    }
}
