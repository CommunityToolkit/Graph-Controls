// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace CommunityToolkit.Auth.Uwp
{
    /// <summary>
    /// Creates a new provider instance for the provided configuration and sets the GlobalProvider.
    /// </summary>
    public static class GlobalProvider
    {
        /// <summary>
        /// Gets or sets the GlobalProvider instance on the ProviderManager.
        /// This class has a similar name and allows interaction with the GlobalProvider from either XAML or C#.
        /// </summary>
        public static IProvider Instance
        {
            get => ProviderManager.Instance.GlobalProvider;
            set => ProviderManager.Instance.GlobalProvider = value;
        }

        /// <summary>
        /// Gets the Graph Config property value.
        /// </summary>
        /// <param name="target">
        /// The target object to retrieve the property value from.
        /// </param>
        /// <returns>
        /// The value of the property on the target.
        /// </returns>
        public static IProviderConfig GetConfig(ResourceDictionary target)
        {
            return (IProviderConfig)target.GetValue(ConfigProperty);
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
        public static void SetConfig(ResourceDictionary target, IProviderConfig value)
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
            DependencyProperty.RegisterAttached("Config", typeof(IProviderConfig), typeof(GlobalProvider), new PropertyMetadata(null, OnConfigChanged));

        private static void OnConfigChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ResourceDictionary rd)
            {
                IProviderConfig config = GetConfig(rd);

                Type configType = config.GetType();
                if (_providers.ContainsKey(configType))
                {
                    var providerFactory = _providers[configType];
                    Instance = providerFactory.Invoke(config);
                }
            }
            else
            {
                Instance = null;
            }
        }

        private static readonly Dictionary<Type, Func<IProviderConfig, IProvider>> _providers = new Dictionary<Type, Func<IProviderConfig, IProvider>>();

        /// <summary>
        /// Register a provider to be available for declaration in XAML using the ConfigProperty.
        /// Use in the static constructor of an IGraphConfig implementation.
        /// </summary>
        /// <code>
        /// static MsalConfig()
        /// {
        ///     Graph.RegisterConfig(typeof(MsalConfig), (c) => MsalProvider.Create(c as MsalConfig));
        /// }.
        /// </code>
        /// <param name="configType">
        /// The Type of the IGraphConfig implementation associated with provider.
        /// </param>
        /// <param name="providerFactory">
        /// A factory function for creating a new instance of the IProvider implementation.
        /// </param>
        public static void RegisterConfig(Type configType, Func<IProviderConfig, IProvider> providerFactory)
        {
            if (!_providers.ContainsKey(configType))
            {
                _providers.Add(configType, providerFactory);
            }
        }
    }
}
