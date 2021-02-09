// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public static class GraphExtensions
    {
        /// <summary>
        /// Gets the Graph Config property value.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IGraphConfig GetGraphConfig(DependencyObject target)
        {
            return (IGraphConfig)target.GetValue(GraphConfigProperty);
        }

        /// <summary>
        /// Sets the GraphConfig property value.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetGraphConfig(DependencyObject target, IGraphConfig value)
        {
            target.SetValue(GraphConfigProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="GraphConfig"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="GraphConfig"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty GraphConfigProperty =
            DependencyProperty.RegisterAttached("GraphConfig", typeof(IGraphConfig), typeof(FrameworkElement), new PropertyMetadata(null, OnGraphConfigChanged));

        private static async void OnGraphConfigChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            IGraphConfig config = GetGraphConfig(sender);

            if (config is MockConfig mockConfig)
            {
                ProviderManager.Instance.GlobalProvider = new MockProvider(mockConfig.SignedIn);
            }
            else if (config is MsalConfig msalConfig)
            {
                ProviderManager.Instance.GlobalProvider = await QuickCreate.CreateMsalProviderAsync(msalConfig);
            }
            else if (config is WindowsConfig winConfig)
            {
                ProviderManager.Instance.GlobalProvider = await QuickCreate.CreateWindowsProviderAsync(winConfig);
            }
        }
    }
}
