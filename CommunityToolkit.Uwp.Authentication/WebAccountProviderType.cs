// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CommunityToolkit.Authentication.Uwp
{
    /// <summary>
    /// An enumeration of the available authentication providers for use in the AccountsSettingsPane.
    /// </summary>
    public enum WebAccountProviderType
    {
        /// <summary>
        /// Authenticate all available accounts.
        /// </summary>
        All,

        /// <summary>
        /// Authenticate public/consumer MSA accounts.
        /// </summary>
        Msa,
    }
}
