// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Xaml.Behaviors;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Toolkit.Wpf.Graph.Providers
{
    /// <summary>
    /// Helper Class for XAML string Scope conversion.
    /// </summary>
    [TypeConverter(typeof(ScopeSetTypeConverter))]
    public class ScopeSet : Collection<string>
    {
        /// <summary>
        /// Empty ScopeSet helper.
        /// </summary>
        public static readonly ScopeSet Empty = new ScopeSet(new string[] { });

        /// <summary>
        /// Helper to convert a string of scopes to a list of strings.
        /// </summary>
        /// <param name="rawString">Comma separated scope list.</param>
        /// <returns>New List of strings, i.e. ScopeSet</returns>
        public static ScopeSet ConvertToScopeArray(string rawString)
        {
            if (rawString != null)
            {
                return new ScopeSet(rawString.Split(","));
            }

            return ScopeSet.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeSet"/> class.
        /// </summary>
        /// <param name="arr">Array to copy as ScopeSet.</param>
        public ScopeSet(string[] arr)
        {
            this.AddRange(arr);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeSet"/> class.
        /// </summary>
        /// <param name="list">List to copy as ScopeSet.</param>
        public ScopeSet(List<string> list)
        {
            this.AddRange(list.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeSet"/> class.
        /// </summary>
        public ScopeSet()
        {
        }

        /// <summary>
        /// Adds range of items to the scope set.
        /// </summary>
        /// <param name="items">Items to add.</param>
        public void AddRange(string[] items)
        {
            foreach (var item in items)
            {
                this.Add(item);
            }
        }
    }
}
