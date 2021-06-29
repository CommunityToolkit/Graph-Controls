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
        /// Authenticate any and all available accounts. Client id required to support organizational accounts.
        /// </summary>
        Any,

        /// <summary>
        /// Authenticate public/consumer MSA accounts.
        /// </summary>
        Msa,

        /// <summary>
        /// Authenticate organizational AAD accounts. Client id required.
        /// </summary>
        Aad,

        /// <summary>
        /// Authenticate the active local account, regardless of type (consumer/organizational). Client id required to support organizational accounts.
        /// </summary>
        Local,
    }
}
