// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;

namespace CommunityToolkit.Authentication
{
    /// <summary>
    /// Shared provider manager used by controls and helpers to authenticate and call the Microsoft Graph.
    /// </summary>
    /// <example>To set your own existing provider:
    /// <code>
    /// ProviderManager.Instance.GlobalProvider = await new MsalProvider(clientId, scopes);
    /// </code>
    /// </example>
    public partial class ProviderManager : INotifyPropertyChanged
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

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GlobalProvider)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            }
        }

        /// <summary>
        /// Gets the ProviderState of the current GlobalProvider instance.
        /// Use for binding scenarios instead of ProviderManager.Instance.GlobalProvider.State.
        /// </summary>
        public ProviderState? State => GlobalProvider?.State;

        private IProvider _provider;

        private ProviderManager()
        {
            // Use Instance
        }

        private void OnProviderStateChanged(object sender, ProviderStateChangedEventArgs args)
        {
            ProviderStateChanged?.Invoke(sender, args);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
        }
    }
}
