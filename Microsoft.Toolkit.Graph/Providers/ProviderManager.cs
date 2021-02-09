// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;

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
    public class ProviderManager : INotifyPropertyChanged
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
        /// Event called when the <see cref="IProvider"/> changes.
        /// </summary>
        public event EventHandler<ProviderUpdatedEventArgs> ProviderUpdated;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

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

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GlobalProvider)));
            }
        }

        private ProviderManager()
        {
            // Use Instance
        }

        private void ProviderStateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            ProviderUpdated?.Invoke(this, new ProviderUpdatedEventArgs(ProviderManagerChangedState.ProviderStateChanged));
        }
    }
}
