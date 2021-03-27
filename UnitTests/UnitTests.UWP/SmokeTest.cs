// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.UWP
{
    /// <summary>
    /// A super basic dummy test to make sure the test harness is prepared properly and ready for more tests.
    /// </summary>
    [TestClass]
    public class SmokeTest
    {
        [TestCategory("SmokeTest")]
        [TestMethod]
        public void TrueIsTrue()
        {
            Assert.IsTrue(true);
        }
    }
}
