// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CommunityToolkit.Authentication
{
    /// <summary>
    /// Configuration values for what type of authentication providers to enable.
    /// </summary>
    public struct WebAccountProviderConfig
    {
        /// <summary>
        /// Gets or sets the registered ClientId. Required for AAD login and admin consent.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the types of accounts providers that should be available to the user.
        /// </summary>
        public WebAccountProviderType WebAccountProviderType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use Version 2 model, only for MSA, the default value is <c>False</c>.
        /// </summary>
        /// <remarks>
        /// This option is configured for pre-authorization applications.
        /// If the application is configured with MSA pre-authorization,
        /// this option can be set to <c>True</c> to skip consent page.
        /// </remarks>
        public bool UseApiVersion2 { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebAccountProviderConfig"/> struct.
        /// </summary>
        /// <param name="webAccountProviderType">The types of accounts providers that should be available to the user.</param>
        /// <param name="clientId">The registered ClientId. Required for AAD login and admin consent.</param>
        /// <param name="useApiVersion2">Whether to enable the version 2 model for the MSA validate.</param>
        public WebAccountProviderConfig(WebAccountProviderType webAccountProviderType, string clientId = null, bool useApiVersion2 = false)
        {
            WebAccountProviderType = webAccountProviderType;
            ClientId = clientId;
            UseApiVersion2 = useApiVersion2;
        }
    }
}
