// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers.Mock;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// Creates a new provider instance for the provided configuration and sets the GlobalProvider.
    /// </summary>
    public static class Graph
    {
        /// <summary>
        /// Gets the Graph Config property value.
        /// </summary>
        /// <param name="target">
        /// The target object to retrieve the property value from.
        /// </param>
        /// <returns>
        /// The value of the property on the target.
        /// </returns>
        public static IGraphConfig GetConfig(DependencyObject target)
        {
            return (IGraphConfig)target.GetValue(ConfigProperty);
        }

        /// <summary>
        /// Sets the GraphConfig property value.
        /// </summary>
        /// <param name="target">
        /// The target object to set the value on.
        /// </param>
        /// <param name="value">
        /// The value to apply to the target property.
        /// </param>
        public static void SetConfig(DependencyObject target, IGraphConfig value)
        {
            target.SetValue(ConfigProperty, value);
        }

        /// <summary>
        /// Identifies the Config dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the Config dependency property.
        /// </returns>
        public static readonly DependencyProperty ConfigProperty =
            DependencyProperty.RegisterAttached("Config", typeof(IGraphConfig), typeof(FrameworkElement), new PropertyMetadata(null, OnConfigChanged));

        private static void OnConfigChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            IGraphConfig config = GetConfig(sender);

            Type configType = config.GetType();
            if (_providers.ContainsKey(configType))
            {
                var providerFactory = _providers[configType];
                ProviderManager.Instance.GlobalProvider = providerFactory.Invoke(config);
            }
            else if (config is MockConfig mockConfig)
            {
                ProviderManager.Instance.GlobalProvider = new MockProvider(mockConfig);
            }
        }

        private static Dictionary<Type, Func<IGraphConfig, IProvider>> _providers = new Dictionary<Type, Func<IGraphConfig, IProvider>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configType"></param>
        /// <param name="providerFactory"></param>
        public static void RegisterConfig(Type configType, Func<IGraphConfig, IProvider> providerFactory)
        {
            if (!_providers.ContainsKey(configType))
            {
                _providers.Add(configType, providerFactory);
            }
        }
    }
}
