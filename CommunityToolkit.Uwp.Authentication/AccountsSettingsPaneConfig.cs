// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Windows.UI.ApplicationSettings;

namespace CommunityToolkit.Uwp.Authentication
{
    /// <summary>
    /// Configuration values for the AccountsSettingsPane.
    /// </summary>
    public struct AccountsSettingsPaneConfig
    {
        /// <summary>
        /// Gets or sets the header text for the accounts settings pane.
        /// </summary>
        public string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets the SettingsCommand collection for the account settings pane.
        /// </summary>
        public IList<SettingsCommand> Commands { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountsSettingsPaneConfig"/> struct.
        /// </summary>
        /// <param name="headerText">The header text for the accounts settings pane.</param>
        /// <param name="commands">The SettingsCommand collection for the account settings pane.</param>
        public AccountsSettingsPaneConfig(string headerText = null, IList<SettingsCommand> commands = null)
        {
            HeaderText = headerText;
            Commands = commands;
        }
    }
}
