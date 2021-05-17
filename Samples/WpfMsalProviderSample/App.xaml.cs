// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using System;
using System.Windows;

namespace WpfMsalProviderSample
{
    public partial class App : Application
    {
        protected override void OnActivated(EventArgs e)
        {
            if (ProviderManager.Instance.GlobalProvider == null)
            {
                string clientId = "YOUR-CLIENT-ID-HERE";
                string[] scopes = new string[] { "User.Read" };
                string redirectUri = "http://localhost";
                ProviderManager.Instance.GlobalProvider = new MsalProvider(clientId, scopes, redirectUri);
            }

            base.OnActivated(e);
        }
    }
}
