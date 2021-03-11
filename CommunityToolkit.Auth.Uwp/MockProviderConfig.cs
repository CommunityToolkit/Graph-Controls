// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CommunityToolkit.Auth.Uwp
{
    /// <summary>
    /// The configuration for the MockProvider.
    /// </summary>
    public class MockProviderConfig : IProviderConfig
    {
        static MockProviderConfig()
        {
            GlobalProvider.RegisterConfig(typeof(MockProviderConfig), (c) => Factory(c as MockProviderConfig));
        }

        /// <summary>
        /// Static helper method for creating a new MockProvider instance from this config object.
        /// </summary>
        /// <param name="config">The configuration for the provider.</param>
        /// <returns>A new instance of the MockProvider based on the provided config.</returns>
        public static MockProvider Factory(MockProviderConfig config)
        {
            return new MockProvider(config.SignedIn);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the provider should automatically sign in or not.
        /// </summary>
        public bool SignedIn { get; set; } = true;
    }
}