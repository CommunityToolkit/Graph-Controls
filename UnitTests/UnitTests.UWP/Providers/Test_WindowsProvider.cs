// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using Microsoft.Toolkit.Uwp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace UnitTests.UWP.Authentication
{
    [TestClass]
    public class Test_WindowsProvider : VisualUITestBase
    {
        /// <summary>
        /// Create a new instance of the WindowsProvider and check that is has the proper default state.
        /// </summary>
        [TestCategory("Providers")]
        [TestMethod]
        public void Test_WindowsProvider_Default()
        {
            WindowsProvider provider = new WindowsProvider();

            Assert.AreEqual(ProviderState.SignedOut, provider.State);
        }

        /// <summary>
        /// Create a new instance of the MockProvider and initiates login.
        /// The test checks that the appropriate events are fired and that the provider transitions 
        /// through the different states as expected.
        /// </summary>
        [TestCategory("Providers")]
        [TestMethod]
        public async Task Test_WindowsProvider_SignInAsync()
        {
            await App.DispatcherQueue.EnqueueAsync(async () =>
            {
                // Create the new provider.
                WindowsProvider provider = new WindowsProvider();

                // Run logout to ensure that no cached users affect the test.
                await provider.SignOutAsync();

                // The newly created provider should be in a logged out state.
                Assert.AreEqual(ProviderState.SignedOut, provider.State);

                // Listen for changes in the provider state and count them.
                int eventCount = 0;
                provider.StateChanged += (s, e) =>
                {
                    eventCount += 1;

                    // Ensure that the states are properly reported through the StateChanged event.
                    switch (e.OldState)
                    {
                        case ProviderState.SignedOut:
                            // Login has been initiated, the provider should now be loading.
                            Assert.AreEqual(ProviderState.Loading, e.NewState);

                            // Loading should be the first event fired.
                            Assert.AreEqual(eventCount, 1);
                            break;

                        case ProviderState.Loading:
                            // The provider has completed login, the provider should now be signed in.
                            Assert.AreEqual(ProviderState.SignedIn, e.NewState);

                            // SignedIn should be the second event fired.
                            Assert.AreEqual(eventCount, 2);
                            break;

                        case ProviderState.SignedIn:
                            // The provider has completed login, the provider should now be signed in.
                            Assert.AreEqual(ProviderState.SignedOut, e.NewState);

                            // SignedIn should be the second event fired.
                            Assert.AreEqual(eventCount, 3);
                            break;

                        default:
                            // This is unexpected, something went wrong during the test.
                            Assert.Fail("The provider has transitioned from an unexpected state: " + Enum.GetName(typeof(ProviderState), e.OldState));
                            break;
                    }
                };

                // Initiate logout.
                await provider.SignInAsync();

                // Logout has completed, the provider should be signed out.
                Assert.AreEqual(ProviderState.SignedIn, provider.State);

                // Initiate logout, which should skip loading, and go straight to signed out.
                await provider.SignOutAsync();

                // Ensure the proper number of events were fired.
                Assert.AreEqual(eventCount, 3);
            });
        }
    }
}
