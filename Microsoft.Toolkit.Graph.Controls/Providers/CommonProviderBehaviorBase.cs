// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if DOTNET
using System.Windows;
using Microsoft.Toolkit.Wpf.UI.Behaviors;
#else
using Microsoft.Toolkit.Uwp.UI.Behaviors;
using Windows.UI.Xaml;
#endif

#if DOTNET
namespace Microsoft.Toolkit.Wpf.Graph.Providers
#else
namespace Microsoft.Toolkit.Graph.Providers
#endif
{
    /// <summary>
    /// Provides a common base class for UWP XAML based provider wrappers to the Microsoft.Graph.Auth SDK.
    /// </summary>
    public abstract partial class CommonProviderBehaviorBase : BehaviorBase<FrameworkElement>
    {
    }
}
