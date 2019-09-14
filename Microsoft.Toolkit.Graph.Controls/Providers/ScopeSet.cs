// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// Helper Class for XAML string Scope conversion.
    /// </summary>
    [Windows.Foundation.Metadata.CreateFromString(MethodName = "Microsoft.Toolkit.Graph.Providers.ScopeSet.ConvertToScopeArray")]
    public class ScopeSet : List<string>
    {
        /// <summary>
        /// Helper to convert a string of scopes to a list of strings.
        /// </summary>
        /// <param name="rawString">Comma separated scope list.</param>
        /// <returns>New List of strings, i.e. ScopeSet</returns>
        public static ScopeSet ConvertToScopeArray(string rawString)
        {
            if (rawString != null)
            {
                return (ScopeSet)rawString.Split(",").ToList<string>();
            }

            return (ScopeSet)new List<string>();
        }
    }
}
