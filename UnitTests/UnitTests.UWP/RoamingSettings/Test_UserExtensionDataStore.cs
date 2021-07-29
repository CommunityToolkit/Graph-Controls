// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Helpers.RoamingSettings;
using Microsoft.Toolkit.Extensions;
using Microsoft.Toolkit.Helpers;
using Microsoft.Toolkit.Uwp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace UnitTests.UWP.Helpers
{
    [TestClass]
    public class Test_UserExtensionDataStore
    {
        /// <summary>
        /// Test the dafault state of a new instance of the UserExtensionDataStore.
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
                    string userId = "TestUserId";
                    string extensionId = "RoamingData";

                    UserExtensionStorageHelper storageHelper = new UserExtensionStorageHelper(extensionId, userId);
                    
                    Assert.AreEqual(extensionId, storageHelper.ExtensionId);
                    Assert.AreEqual(userId, storageHelper.UserId);
                    Assert.IsNotNull(storageHelper.Serializer);
                    Assert.IsInstanceOfType(storageHelper.Serializer, typeof(SystemSerializer));

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
        /// Test the dafault state of a new instance of the UserExtensionDataStore.
        /// </summary>
        [TestCategory("ObjectStorage")]
        [TestMethod]
        public async Task Test_Sync()
        {
            var tcs = new TaskCompletionSource<bool>();

            async void test()
            {
                try
                {
                    string extensionId = "RoamingData";
                    string userId = "TestUserId";

                    string testKey = "foo";
                    string testValue = "bar";

                    var dataStore = new UserExtensionStorageHelper(extensionId, userId);

                    dataStore.SyncCompleted += async (s, e) =>
                    {
                        try
                        {
                            // Create a second instance to ensure that the Cache doesn't yield a false positive.
                            var dataStore2 = new UserExtensionStorageHelper(extensionId, userId);
                            await dataStore2.Sync();

                            Assert.IsTrue(dataStore.TryRead(testKey, out string storedValue));
                            Assert.AreEqual(testValue, storedValue);

                            tcs.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    };

                    dataStore.SyncFailed = (s, e) =>
                    {
                        try
                        {
                            Assert.Fail("Sync Failed");
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    };

                    dataStore.Save(testKey, testValue);
                    await dataStore.Sync();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }

            PrepareProvider(test);

            var result = await tcs.Task;
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Create a new instance of IProvider and check that it has the proper default state, then execute the provided action.
        /// </summary>
        private async void PrepareProvider(Action test)
        {
            await App.DispatcherQueue.EnqueueAsync(async () =>
            {
                var provider = new WindowsProvider(new string[] { "User.ReadWrite" }, autoSignIn: false);

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
