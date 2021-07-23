// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Windows.UI.ApplicationSettings;

namespace CommunityToolkit.Authentication
{
    /// <summary>
    /// Configuration values for the AccountsSettingsPane.
    /// </summary>
    public struct AccountsSettingsPaneConfig
    {
        /// <summary>
        /// Gets or sets the header text for the add accounts settings pane.
        /// </summary>
        public string AddAccountHeaderText { get; set; }

        /// <summary>
        /// Gets or sets the header text for the manage accounts settings pane.
        /// </summary>
        public string ManageAccountHeaderText { get; set; }

        /// <summary>
        /// Gets or sets the SettingsCommand collection for the account settings pane.
        /// </summary>
        public IList<SettingsCommand> Commands { get; set; }

        /// <summary>
        /// Gets or sets the WebAccountCommandParameter for the account settings pane.
        /// </summary>
        public WebAccountCommandParameter AccountCommandParameter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountsSettingsPaneConfig"/> struct.
        /// </summary>
        /// <param name="addAccountHeaderText">The header text for the add accounts settings pane.</param>
        /// <param name="manageAccountHeaderText">The header text for the manage accounts settings pane.</param>
        /// <param name="commands">The SettingsCommand collection for the account settings pane.</param>
        /// <param name="accountCommandParameter">The WebAccountCommandParameter for the account settings pane.</param>
        public AccountsSettingsPaneConfig(
            string addAccountHeaderText = null,
            string manageAccountHeaderText = null,
            IList<SettingsCommand> commands = null,
            WebAccountCommandParameter accountCommandParameter = null)
        {
            AddAccountHeaderText = addAccountHeaderText;
            ManageAccountHeaderText = manageAccountHeaderText;
            Commands = commands;
            AccountCommandParameter = accountCommandParameter;
        }
    }
}