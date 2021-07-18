// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;

namespace CommunityToolkit.Authentication
{
    /// <summary>
    /// Shared provider manager used by controls in Microsoft.Toolkit.Graph.Controls to authenticate and call the Microsoft Graph.
    /// </summary>
    /// <example>To set your own existing provider:
    /// <code>
    /// ProviderManager.Instance.GlobalProvider = await MsalProvider.CreateAsync(...);
    /// </code>
    /// </example>
    public partial class ProviderManager
    {
        /// <summary>
        /// Gets the name of the toolkit client to identify self in Graph calls.
        /// </summary>
        public static readonly string ClientName = "wct/" + ThisAssembly.AssemblyVersion;

        /// <summary>
        /// Gets the instance of the GlobalProvider.
        /// </summary>
        public static ProviderManager Instance { get; } = new ProviderManager();

        /// <summary>
        /// Event called when the <see cref="IProvider"/> instance changes.
        /// </summary>
        public event EventHandler<IProvider> ProviderUpdated;

        /// <summary>
        /// Event called when the <see cref="IProvider"/> state changes.
        /// </summary>
        public event EventHandler<ProviderStateChangedEventArgs> ProviderStateChanged;

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
                var oldState = _provider?.State;
                if (_provider != null)
                {
                    _provider.StateChanged -= OnProviderStateChanged;
                }

                _provider = value;

                var newState = _provider?.State;
                if (_provider != null)
                {
                    _provider.StateChanged += OnProviderStateChanged;
                }

                ProviderUpdated?.Invoke(this, _provider);
                ProviderStateChanged?.Invoke(this, new ProviderStateChangedEventArgs(oldState, newState));
            }
        }

        private void OnProviderStateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            ProviderStateChanged?.Invoke(this, e);
        }

        private ProviderManager()
        {
            // Use Instance
        }
    }
}
