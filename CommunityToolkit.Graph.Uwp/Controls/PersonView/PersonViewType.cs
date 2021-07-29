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
        /// <summary>
        /// Render only the avatar.
        /// </summary>
        Avatar = 0,

        /// <summary>
        /// Render the avatar and one line of text.
        /// </summary>
        OneLine = 1,

        /// <summary>
        /// Render the avatar and two lines of text.
        /// </summary>
        TwoLines = 2,

        /// <summary>
        /// Render the avatar and three lines of text.
        /// </summary>
        ThreeLines = 3,
    }
}
