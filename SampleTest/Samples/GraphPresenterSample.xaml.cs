// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text.RegularExpressions;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using Microsoft.Graph.Extensions;
using Windows.UI.Xaml.Controls;

namespace SampleTest.Samples
{
    public sealed partial class GraphPresenterSample : Page
    {
        public static string ToLocalTime(DateTimeTimeZone value)
        {
            // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2407
            return value.ToDateTimeOffset().LocalDateTime.ToString("g");
        }

        public static string ToLocalTime(DateTimeOffset? value)
        {
            // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2654
            return value?.LocalDateTime.ToString("g");
        }

        public GraphPresenterSample()
        {
            InitializeComponent();
        }
    }
}
