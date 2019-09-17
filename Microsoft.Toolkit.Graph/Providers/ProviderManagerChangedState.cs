// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Graph.Providers
{
    /// <summary>
    /// Enum representing reasons for provider state changing.
    /// </summary>
    public enum ProviderManagerChangedState
    {
        /// <summary>
        /// The <see cref="IProvider"/> itself changed.
        /// </summary>
        ProviderChanged,

        /// <summary>
        /// The <see cref="IProvider.State"/> changed.
        /// </summary>
        ProviderStateChanged
    }
}