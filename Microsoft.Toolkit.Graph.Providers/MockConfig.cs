// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// The configuration for the MockProvider.
    /// </summary>
    public class MockConfig : IGraphConfig
    {
        /// <summary>
        /// Gets or sets a value indicating whether the provider should automatically sign in or not.
        /// </summary>
        public bool SignedIn { get; set; } = true;
    }
}
