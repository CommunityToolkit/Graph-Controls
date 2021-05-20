// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CommunityToolkit.Graph.Uwp.Controls
{
    /// <summary>
    /// Enumeration of what details should be displayed in a PersonView.
    /// </summary>
    public enum PersonViewType
    {
        /**
         * Render only the avatar
         */
        Avatar = 0,

        /**
         * Render the avatar and one line of text
         */
        OneLine = 1,

        /**
         * Render the avatar and two lines of text
         */
        TwoLines = 2,
    }
}
