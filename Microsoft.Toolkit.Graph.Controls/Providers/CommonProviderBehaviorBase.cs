// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.UI.Behaviors;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// Provides a common base class for UWP XAML based provider wrappers to the Microsoft.Graph.Auth SDK.
    /// </summary>
    public abstract partial class CommonProviderBehaviorBase : BehaviorBase<FrameworkElement>
    {
    }
}
