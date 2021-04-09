﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Net.Authentication;
using CommunityToolkit.Uwp.Graph.Helpers.RoamingSettings;
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
        public async Task Test_MockProvider_Default()
        {
            var tcs = new TaskCompletionSource<bool>();

            void test()
            {
                try
                {
                    string userId = "TestUserId";
                    string dataStoreId = "RoamingData.json";
                    IObjectSerializer serializer = new SystemSerializer();

                    IRoamingSettingsDataStore dataStore = new UserExtensionDataStore(userId, dataStoreId, serializer, false);

                    // Evaluate the default state is as expected
                    Assert.IsFalse(dataStore.AutoSync);
                    Assert.IsNull(dataStore.Cache);
                    Assert.AreEqual(dataStoreId, dataStore.Id);
                    Assert.AreEqual(userId, dataStore.UserId);

                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            };

            PrepareMockProvider(test);

            await tcs.Task;
        }

        /// <summary>
        /// Test the dafault state of a new instance of the UserExtensionDataStore.
        /// </summary>
        [TestCategory("RoamingSettings")]
        [TestMethod]
        public async Task Test_MockProvider_Sync()
        {
            var tcs = new TaskCompletionSource<bool>();

            async void test()
            {
                try
                {
                    string userId = "TestUserId";
                    string dataStoreId = "RoamingData.json";
                    IObjectSerializer serializer = new SystemSerializer();

                    IRoamingSettingsDataStore dataStore = new UserExtensionDataStore(userId, dataStoreId, serializer, false);

                    dataStore.SyncCompleted += (s, e) =>
                    {
                        try
                        {
                            Assert.Fail("Sync should have failed because we are using the MockProvider.");
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
                            Assert.IsNull((s as IRoamingSettingsDataStore).Cache);
                            tcs.SetResult(true);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    };

                    await dataStore.Sync();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }

            PrepareMockProvider(test);

            await tcs.Task;
        }

        /// <summary>
        /// Test the dafault state of a new instance of the UserExtensionDataStore.
        /// </summary>
        [TestCategory("RoamingSettings")]
        [TestMethod]
        public async Task Test_MockProvider_CRUD()
        {
            var tcs = new TaskCompletionSource<bool>();

            async void test()
            {
                try
                {
                    string userId = "TestUserId";
                    string dataStoreId = "RoamingData.json";
                    IObjectSerializer serializer = new SystemSerializer();

                    IRoamingSettingsDataStore dataStore = new UserExtensionDataStore(userId, dataStoreId, serializer, false);

                    Assert.IsNull(dataStore.Cache);
                    await dataStore.Create();
                    Assert.IsNotNull(dataStore.Cache);

                    string key = "myKey";
                    string value1 = "myFirstValue";
                    string value2 = "mySecondValue";
                    Assert.IsFalse(dataStore.KeyExists(key));

                    dataStore.Save(key, value1);
                    Assert.IsTrue(dataStore.KeyExists(key));
                    Assert.AreEqual(value1, dataStore.Read<string>(key));

                    dataStore.Save(key, value2);
                    Assert.AreEqual(value2, dataStore.Read<string>(key));

                    await dataStore.Delete();
                    Assert.IsNull(dataStore.Cache);

                    Assert.AreEqual(default, dataStore.Read<string>(key));

                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            };

            PrepareMockProvider(test);

            await tcs.Task;
        }

        /// <summary>
        /// Create a new instance of the MockProvider and check that it has the proper default state, then execute the provided action.
        /// </summary>
        private void PrepareMockProvider(Action test)
        {
            ProviderManager.Instance.ProviderUpdated += (s, e) =>
            {
                var providerManager = s as ProviderManager;
                if (providerManager.GlobalProvider.State == ProviderState.SignedIn)
                {
                    test.Invoke();
                }
            };
            ProviderManager.Instance.GlobalProvider = new MockProvider();
        }
    }
}
