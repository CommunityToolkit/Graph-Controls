// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CommunityToolkit.Authentication
{
    /// <summary>
    /// An enumeration of the available authentication providers for use in the AccountsSettingsPane.
    /// </summary>
    public enum WebAccountProviderType
    {
        /// <summary>
        /// Authenticate any available accounts.
        /// Store app association required to support consumer accounts.
        /// Client ID required to support organizational accounts.
        /// </summary>
        Any,

        /// <summary>
        /// Authenticate consumer MSA accounts. Store app association required.
        /// </summary>
        Msa,

        /// <summary>
        /// Authenticate organizational AAD accounts. Client ID required.
        /// </summary>
        Aad,

        /// <summary>
        /// Authenticate the active local account regardles of type (consumer/organizational).
        /// Store app association required to support consumer accounts.
        /// Client ID required to support organizational accounts.
        /// </summary>
        Local,
    }
}
