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
        public static object GetConfig(ResourceDictionary target)
        {
            return (object)target.GetValue(ConfigProperty);
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
        public static void SetConfig(ResourceDictionary target, object value)
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
            DependencyProperty.RegisterAttached("Config", typeof(object), typeof(GlobalProvider), new PropertyMetadata(null, OnConfigChanged));

        private static void OnConfigChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ResourceDictionary rd)
            {
                object config = GetConfig(rd);

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

        private static readonly Dictionary<Type, Func<object, IProvider>> _providers = new Dictionary<Type, Func<object, IProvider>>();

        /// <summary>
        /// Registers a provider to be available for declaration in XAML using the ConfigProperty.
        /// </summary>
        /// <code>
        /// //Put this in the static constructor of the config object.
        /// static MyConfig()
        /// {
        ///     Graph.RegisterConfig(typeof(MyConfig), (c) => MyProvider.Create(c as MyConfig));
        /// }.
        /// </code>
        /// <param name="configType">
        /// The Type of the config object associated with provider.
        /// </param>
        /// <param name="providerFactory">
        /// A factory function for creating a new instance of the IProvider implementation.
        /// </param>
        public static void RegisterConfig(Type configType, Func<object, IProvider> providerFactory)
        {
            if (!_providers.ContainsKey(configType))
            {
                _providers.Add(configType, providerFactory);
            }
        }

        /// <summary>
        /// Registers a provider to be available for declaration in XAML using the ConfigProperty.
        /// </summary>
        /// <code>
        /// //Put this in the static constructor of the config object.
        /// static MyConfig()
        /// {
        ///     Graph.RegisterConfig&lt;MyConfig&gt;((c) => MyProvider.Create(c as MyConfig));
        /// }.
        /// </code>
        /// <typeparam name="T">
        /// The Type of the config object associated with provider.
        /// </typeparam>
        /// <param name="providerFactory">
        /// A factory function for creating a new instance of the IProvider implementation.
        /// </param>
        public static void RegisterConfig<T>(Func<object, IProvider> providerFactory)
        {
            RegisterConfig(typeof(T), providerFactory);
        }
    }
}
