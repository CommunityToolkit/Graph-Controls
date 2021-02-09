// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// A base construct for building Graph Providers on top of.
    /// </summary>
    public abstract class BaseProvider : IProvider
    {
        private ProviderState _state;

        /// <summary>
        /// Gets or sets the current state of the provider.
        /// </summary>
        public ProviderState State
        {
            get => _state;
            protected set
            {
                var oldState = _state;
                var newState = value;
                if (oldState != newState)
                {
                    _state = newState;
                    StateChanged?.Invoke(this, new ProviderStateChangedEventArgs(oldState, newState));
                }
            }
        }

        /// <inheritdoc/>
        public event EventHandler<ProviderStateChangedEventArgs> StateChanged;

        /// <summary>
        /// Gets or sets the service client instance for making Graph calls.
        /// </summary>
        public GraphServiceClient Graph { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProvider"/> class.
        /// </summary>
        public BaseProvider()
        {
            _state = ProviderState.Loading;
        }

        /// <inheritdoc />
        public abstract Task LoginAsync();

        /// <inheritdoc />
        public abstract Task LogoutAsync();

        /// <inheritdoc />
        public abstract Task AuthenticateRequestAsync(HttpRequestMessage request);
    }
}
