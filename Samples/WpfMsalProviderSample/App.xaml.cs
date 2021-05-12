﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Net.Authentication;
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
                string clientId = "728f3423-bd1e-4424-aa2b-0ac50751c03a";
                string[] scopes = new string[] { "User.Read" };
                string redirectUri = "http://localhost";
                ProviderManager.Instance.GlobalProvider = new MsalProvider(clientId, scopes, redirectUri);
            }

            base.OnActivated(e);
        }
    }
}