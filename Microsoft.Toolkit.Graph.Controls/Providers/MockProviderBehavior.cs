// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if DOTNET
using System;
using System.Linq;
using System.Windows;
using Microsoft.Toolkit.Graph.Providers;
#else
using System.Linq;
using Windows.UI.Xaml;
#endif

#if DOTNET
namespace Microsoft.Toolkit.Wpf.Graph.Providers
#else
namespace Microsoft.Toolkit.Graph.Providers
#endif
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
    public class MockProviderBehavior : CommonProviderBehaviorBase
    {
        private object lock_sync = new object();
        private bool initialized = false;

        /// <summary>
        /// Gets or sets a value indicating whether the mock provider is signed-in upon initialization.
        /// </summary>
        public bool SignedIn
        {
            get { return (bool)GetValue(SignedInProperty); }
            set { SetValue(SignedInProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="SignedIn"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="SignedIn"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty SignedInProperty =
            DependencyProperty.Register(nameof(SignedIn), typeof(bool), typeof(MockProviderBehavior), new PropertyMetadata(true));

        /// <inheritdoc/>
        protected override bool Initialize()
        {
            lock (lock_sync)
            {
                if (!initialized)
                {
                    ProviderManager.Instance.GlobalProvider = new MockProvider(SignedIn);
                }
            }

            return base.Initialize();
        }
    }
}
