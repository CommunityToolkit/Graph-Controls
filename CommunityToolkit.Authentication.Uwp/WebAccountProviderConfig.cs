﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

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
        /// Gets or sets the properties that need to be added when constructing WebTokenRequest (for MSA).
        /// </summary>
        public IDictionary<string, string> MSATokenRequestProperties { get; set; }

        /// <summary>
        /// Gets or sets the properties that need to be added when constructing WebTokenRequest (for AAD).
        /// </summary>
        public IDictionary<string, string> AADTokenRequestProperties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebAccountProviderConfig"/> struct.
        /// </summary>
        /// <param name="webAccountProviderType">The types of accounts providers that should be available to the user.</param>
        /// <param name="clientId">The registered ClientId. Required for AAD login and admin consent.</param>
        /// <param name="msaTokenRequestProperties">Request properties for MSA.</param>
        /// <param name="aadTokenRequestProperties">Request properties for AAD.</param>
        public WebAccountProviderConfig(
            WebAccountProviderType webAccountProviderType,
            string clientId = null,
            IDictionary<string, string> msaTokenRequestProperties = null,
            IDictionary<string, string> aadTokenRequestProperties = null)
        {
            WebAccountProviderType = webAccountProviderType;
            ClientId = clientId;
            MSATokenRequestProperties = msaTokenRequestProperties;
            AADTokenRequestProperties = aadTokenRequestProperties;
        }
    }
}
