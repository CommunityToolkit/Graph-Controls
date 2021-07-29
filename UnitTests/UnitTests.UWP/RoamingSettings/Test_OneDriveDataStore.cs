// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Helpers.RoamingSettings;
using Microsoft.Toolkit.Helpers;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace UnitTests.UWP.Helpers
{
    [TestClass]
    public class Test_OneDriveDataStore : VisualUITestBase
    {
        /// <summary>
        /// Test the dafault state of a new instance of the OneDriveDataStore.
        /// </summary>
        [TestCategory("RoamingSettings")]
        [TestMethod]
        public async Task Test_Default()
        {
            var tcs = new TaskCompletionSource<bool>();

            void test()
            {
                try
                {
                    var userId = "TestUserId";
                    var storageHelper = new OneDriveStorageHelper(userId);
                    
                    // Evaluate the default state is as expected
                    Assert.AreEqual(userId, storageHelper.UserId);

                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            };

            PrepareProvider(test);

            await tcs.Task;
        }

        /// <summary>
        /// Create a new instance of IProvider and check that it has the proper default state, then execute the provided action.
        /// </summary>
        private async void PrepareProvider(Action test)
        {
            await App.DispatcherQueue.EnqueueAsync(async () =>
            {
                var provider = new WindowsProvider(new string[] { "User.Read", "Files.ReadWrite" }, autoSignIn: false);

                ProviderManager.Instance.ProviderStateChanged += (s, e) =>
                {
                    var providerManager = s as ProviderManager;
                    if (providerManager.GlobalProvider.State == ProviderState.SignedIn)
                    {
                        test.Invoke();
                    }
                };

                ProviderManager.Instance.GlobalProvider = provider;

                await provider.SignInAsync();
            });
        }
    }
}
