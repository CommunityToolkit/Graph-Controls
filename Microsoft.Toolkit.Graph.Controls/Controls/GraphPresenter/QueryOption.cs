// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// XAML Proxy for <see cref="Microsoft.Graph.QueryOption"/>.
    /// </summary>
    public class QueryOption
    {
        /// <inheritdoc cref="Microsoft.Graph.Option.Name"/>
        public string Name { get; set; }

        /// <inheritdoc cref="Microsoft.Graph.Option.Value"/>
        public string Value { get; set; }

        /// <summary>
        /// Constructs a <see cref="Microsoft.Graph.QueryOption"/> value representing this proxy.
        /// </summary>
        /// <returns><see cref="Microsoft.Graph.QueryOption"/> result.</returns>
        public Microsoft.Graph.QueryOption ToQueryOption()
        {
            return new Microsoft.Graph.QueryOption(Name, Value);
        }
    }
}
