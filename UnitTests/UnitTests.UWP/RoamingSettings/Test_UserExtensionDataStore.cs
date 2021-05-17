// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using CommunityToolkit.Uwp.Graph.Helpers.RoamingSettings;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
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
                    string dataStoreId = "RoamingData";
                    IObjectSerializer serializer = new SystemSerializer();

                    IRoamingSettingsDataStore dataStore = new UserExtensionDataStore(userId, dataStoreId, serializer, false);

                    // Evaluate the default state is as expected
                    Assert.IsFalse(dataStore.AutoSync);
                    Assert.IsNotNull(dataStore.Cache);
                    Assert.AreEqual(dataStoreId, dataStore.Id);
                    Assert.AreEqual(userId, dataStore.UserId);

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
        [TestCategory("RoamingSettings")]
        [TestMethod]
        public async Task Test_Sync()
        {
            var tcs = new TaskCompletionSource<bool>();

            async void test()
            {
                try
                {
                    string userId = "TestUserId";
                    string dataStoreId = "RoamingData";
                    IObjectSerializer serializer = new SystemSerializer();

                    IRoamingSettingsDataStore dataStore = new UserExtensionDataStore(userId, dataStoreId, serializer, false);

                    try
                    {
                        // Attempt to delete the remote first.
                        await dataStore.Delete();
                    }
                    catch
                    {
                    }

                    dataStore.SyncCompleted += async (s, e) =>
                    {
                        try
                        {
                            // Create a second instance to ensure that the Cache doesn't yield a false positive.
                            IRoamingSettingsDataStore dataStore2 = new OneDriveDataStore(userId, dataStoreId, serializer, false);
                            await dataStore2.Sync();

                            var foo = dataStore.Read<string>("foo");
                            Assert.AreEqual("bar", foo);

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

                    dataStore.Save("foo", "bar");
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

                ProviderManager.Instance.ProviderUpdated += (s, e) =>
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
