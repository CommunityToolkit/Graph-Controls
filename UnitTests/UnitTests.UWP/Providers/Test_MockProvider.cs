// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Net.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace UnitTests.UWP.Authentication
{
    [TestClass]
    public class Test_MockProvider
    {
        /// <summary>
        /// Create a new instance of the MockProvider and check that is has the proper default state.
        /// </summary>
        [TestCategory("Providers")]
        [TestMethod]
        public void Test_MockProvider_Default()
        {
            IProvider provider = new MockProvider();

            Assert.AreEqual(ProviderState.SignedIn, provider.State);
        }

        /// <summary>
        /// Create a new instance of the MockProvider and initiates login.
        /// The test checks that the appropriate events are fired and that the provider transitions 
        /// through the different states as expected.
        /// </summary>
        [TestCategory("Providers")]
        [TestMethod]
        public async Task Test_MockProvider_LoginAsync()
        {
            // Create the new provider, pre-signed out.
            IProvider provider = new MockProvider(false);

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

                    default:
                        // This is unexpected, something went wrong during the test.
                        Assert.Fail("The provider has transitioned from an unexpected state: " + Enum.GetName(typeof(ProviderState), e.OldState));
                        break;
                }
            };

            // Initiate logout.
            await provider.LoginAsync();

            // Logout has completed, the provider should be signed out.
            Assert.AreEqual(ProviderState.SignedIn, provider.State);

            // Ensure the proper number of events were fired.
            Assert.AreEqual(eventCount, 2);
        }

        /// <summary>
        /// Create a new instance of the MockProvider and initiates logout.
        /// The test checks that the appropriate events are fired and that the provider transitions 
        /// through the different states as expected.
        /// </summary>
        [TestCategory("Providers")]
        [TestMethod]
        public async Task Test_MockProvider_LogoutAsync()
        {
            // Create the new provider, pre-signed in.
            IProvider provider = new MockProvider(true);

            // The newly created provider should be in a logged in state.
            Assert.AreEqual(ProviderState.SignedIn, provider.State);

            // Listen for changes in the provider state and count them.
            int eventCount = 0;
            provider.StateChanged += (s, e) =>
            {
                eventCount += 1;

                // Ensure that the states are properly reported through the StateChanged event.
                switch (e.OldState)
                {
                    case ProviderState.SignedIn:
                        // Logout has been initiated, the provider should now be loading.
                        Assert.AreEqual(ProviderState.Loading, e.NewState);

                        // Loading should be the first event fired.
                        Assert.AreEqual(eventCount, 1);
                        break;

                    case ProviderState.Loading:
                        // The provider has completed logout, the provider should now be signed out.
                        Assert.AreEqual(ProviderState.SignedOut, e.NewState);

                        // SignedOut should be the second event fired.
                        Assert.AreEqual(eventCount, 2);
                        break;

                    default:
                        // This is unexpected, something went wrong during the test.
                        Assert.Fail("The provider has transitioned from an unexpected state: " + Enum.GetName(typeof(ProviderState), e.OldState));
                        break;
                }
            };

            // Initiate logout.
            await provider.LogoutAsync();

            // Logout has completed, the provider should be signed out.
            Assert.AreEqual(ProviderState.SignedOut, provider.State);

            // Ensure the proper number of events were fired.
            Assert.AreEqual(eventCount, 2);
        }

        /// <summary>
        /// Authenticate an empty request and detect that the approapriate headers have been added.
        /// </summary>
        [TestCategory("Providers")]
        [TestMethod]
        public async Task Test_MockProvider_AuthenticateRequestAsync()
        {
            // Create a new instance of the MockProvider.
            IProvider provider = new MockProvider(true);

            // Create an empty message to authenticate.
            var message = new HttpRequestMessage(HttpMethod.Get, new Uri("https://graph.microsoft.com/v1/me"));

            // Use the provider to authenticate the message.
            await provider.AuthenticateRequestAsync(message);

            // Check for the absence of the SdkVersion header value on the empty message.
            bool sdkVersionHeaderExists = message.Headers.TryGetValues("SdkVersion", out _);
            Assert.IsFalse(sdkVersionHeaderExists, "SdkVersion header values should not exist on an empty request that does not originate from the SDK.");

            // Check for the authorization header
            Assert.IsNotNull(message.Headers.Authorization, "Authorization header was not found.");
            Assert.AreEqual("Bearer", message.Headers.Authorization.Scheme);
            Assert.AreEqual("{token:https://graph.microsoft.com/}", message.Headers.Authorization.Parameter);
        }
    }
}
