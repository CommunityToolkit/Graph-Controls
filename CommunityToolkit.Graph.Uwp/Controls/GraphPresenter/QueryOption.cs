// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;

namespace CommunityToolkit.Graph.Uwp.Controls
{
    /// <summary>
    /// XAML Proxy for <see cref="Microsoft.Graph.QueryOption"/>.
    /// </summary>
    [Experimental]
    public sealed class QueryOption
    {
        /// <inheritdoc cref="Microsoft.Graph.Option.Name"/>
        public string Name { get; set; }

        /// <inheritdoc cref="Microsoft.Graph.Option.Value"/>
        public string Value { get; set; }

        /// <summary>
        /// Implicit conversion for <see cref="QueryOption"/> to <see cref="Microsoft.Graph.QueryOption"/>.
        /// </summary>
        /// <param name="option">query option to convert.</param>
        public static implicit operator Microsoft.Graph.QueryOption(QueryOption option)
        {
            return new Microsoft.Graph.QueryOption(option.Name, option.Value);
        }
    }
}
