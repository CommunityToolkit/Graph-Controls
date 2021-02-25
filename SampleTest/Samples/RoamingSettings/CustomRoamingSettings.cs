// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Graph.RoamingSettings
{
    public class CustomRoamingSettings : UserExtensionDataStore
    {
        internal const string _extensionId = "com.custom.roamingSettings";

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRoamingSettings"/> class.
        /// </summary>
        /// <param name="userId"></param>
        public CustomRoamingSettings(string userId, bool autoSync = false)
            : base(_extensionId, userId, autoSync)
        {

        }
    }
}
