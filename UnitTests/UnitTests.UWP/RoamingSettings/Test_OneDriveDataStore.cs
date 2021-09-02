// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Helpers.RoamingSettings;
using Microsoft.Toolkit.Helpers;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        [TestCategory("RoamingSettings")]
        [TestMethod]
        public async Task Test_FileCRUD()
        {
            var tcs = new TaskCompletionSource<bool>();

            async void test()
            {
                try
                {
                    var filePath = "TestFile.txt";
                    var fileContents = "this is a test";
                    var fileContents2 = "this is also a test";
                    var storageHelper = await OneDriveStorageHelper.CreateForCurrentUserAsync();

                    // Create a file
                    await storageHelper.CreateFileAsync(filePath, fileContents);

                    // Read a file
                    var readContents = await storageHelper.ReadFileAsync<string>(filePath);
                    Assert.AreEqual(fileContents, readContents);

                    // Update a file
                    await storageHelper.CreateFileAsync(filePath, fileContents2);
                    var readContents2 = await storageHelper.ReadFileAsync<string>(filePath);
                    Assert.AreEqual(fileContents2, readContents2);

                    // Delete a file
                    var itemDeleted = await storageHelper.TryDeleteItemAsync(filePath);
                    Assert.IsTrue(itemDeleted);

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

        [TestCategory("RoamingSettings")]
        [TestMethod]
        public async Task Test_FolderCRUD()
        {
            var tcs = new TaskCompletionSource<bool>();

            async void test()
            {
                try
                {
                    var subfolderName = "TestSubFolder";
                    var folderName = "TestFolder";
                    var fileName = "TestFile.txt";
                    var filePath = $"{folderName}/{fileName}";
                    var fileContents = "this is a test";
                    var storageHelper = await OneDriveStorageHelper.CreateForCurrentUserAsync();

                    // Create a folder
                    await storageHelper.CreateFolderAsync(folderName);

                    // Create a subfolder
                    await storageHelper.CreateFolderAsync(subfolderName, folderName);

                    // Create a file in a folder
                    await storageHelper.CreateFileAsync(filePath, fileContents);

                    // Read a file from a folder
                    var readContents = await storageHelper.ReadFileAsync<string>(filePath);
                    Assert.AreEqual(fileContents, readContents);

                    // List folder contents
                    var folderItems = await storageHelper.ReadFolderAsync(folderName);
                    var folderItemsList = folderItems.ToList();
                    Assert.AreEqual(2, folderItemsList.Count());
                    Assert.AreEqual(subfolderName, folderItemsList[0].Name);
                    Assert.AreEqual(DirectoryItemType.Folder, folderItemsList[0].ItemType);
                    Assert.AreEqual(fileName, folderItemsList[1].Name);
                    Assert.AreEqual(DirectoryItemType.File, folderItemsList[1].ItemType);

                    // Delete a folder
                    var itemDeleted = await storageHelper.TryDeleteItemAsync(folderName);
                    Assert.IsTrue(itemDeleted);

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
