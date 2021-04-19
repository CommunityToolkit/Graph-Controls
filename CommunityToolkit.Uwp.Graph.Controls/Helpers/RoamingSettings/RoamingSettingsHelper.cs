// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Net.Authentication;
using CommunityToolkit.Net.Graph.Extensions;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Storage;

namespace CommunityToolkit.Uwp.Graph.Helpers.RoamingSettings
{
    /// <summary>
    /// An enumeration of the available data storage methods for roaming data.
    /// </summary>
    public enum RoamingDataStore
    {
        /// <summary>
        /// Store data using open extensions on the Graph User.
        /// </summary>
        UserExtensions,

        /// <summary>
        /// Store data in a Graph User's OneDrive.
        /// </summary>
        OneDrive,
    }

    /// <summary>
    /// A helper class for syncing data to roaming data store.
    /// </summary>
    public class RoamingSettingsHelper : IRoamingSettingsDataStore
    {
        /// <summary>
        /// Gets the internal data store instance.
        /// </summary>
        public IRoamingSettingsDataStore DataStore { get; private set; }

        /// <summary>
        /// Gets or sets an event handler for when a remote data sync completes successfully.
        /// </summary>
        public EventHandler SyncCompleted { get; set; }

        /// <summary>
        /// Gets or sets an event handler for when a remote data sync fails.
        /// </summary>
        public EventHandler SyncFailed { get; set; }

        /// <summary>
        /// Gets a value indicating whether the values should immediately sync or not.
        /// </summary>
        public bool AutoSync => DataStore.AutoSync;

        /// <summary>
        /// Gets access to the key/value pairs cache directly.
        /// </summary>
        public IDictionary<string, object> Cache => DataStore.Cache;

        /// <summary>
        /// Gets the id of the data store.
        /// </summary>
        public string Id => DataStore.Id;

        /// <summary>
        /// Gets the id of the target user.
        /// </summary>
        public string UserId => DataStore.UserId;

        /// <summary>
        /// Creates a new RoamingSettingsHelper instance for the currently signed in user.
        /// </summary>
        /// <param name="dataStore">Which specific data store is being used.</param>
        /// <param name="syncOnInit">Whether the values should immediately sync or not.</param>
        /// <param name="autoSync">Whether the values should immediately sync on change or wait until Sync is called explicitly.</param>
        /// <param name="serializer">An object serializer for serialization of objects in the data store.</param>
        /// <returns>A new instance of the RoamingSettingsHelper configured for the current user.</returns>
        public static async Task<RoamingSettingsHelper> CreateForCurrentUser(RoamingDataStore dataStore = RoamingDataStore.UserExtensions, bool syncOnInit = true, bool autoSync = true, IObjectSerializer serializer = null)
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider == null || provider.State != ProviderState.SignedIn)
            {
                throw new InvalidOperationException("The GlobalProvider must be set and signed in to create a new RoamingSettingsHelper for the current user.");
            }

            var me = await provider.Graph().Me.Request().GetAsync();
            return new RoamingSettingsHelper(me.Id, dataStore, syncOnInit, autoSync, serializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoamingSettingsHelper"/> class.
        /// </summary>
        /// <param name="userId">The id of the target Graph User.</param>
        /// <param name="dataStore">Which specific data store is being used.</param>
        /// <param name="syncOnInit">Whether the values should immediately sync or not.</param>
        /// <param name="autoSync">Whether the values should immediately sync on change or wait until Sync is called explicitly.</param>
        /// <param name="serializer">An object serializer for serialization of objects in the data store.</param>
        public RoamingSettingsHelper(string userId, RoamingDataStore dataStore = RoamingDataStore.UserExtensions, bool syncOnInit = true, bool autoSync = true, IObjectSerializer serializer = null)
        {
            // TODO: Infuse unique identifier from Graph registration into the storage name.
            string dataStoreName = "communityToolkit.roamingSettings";

            if (serializer == null)
            {
                serializer = new SystemSerializer();
            }

            switch (dataStore)
            {
                case RoamingDataStore.UserExtensions:
                    DataStore = new UserExtensionDataStore(userId, dataStoreName, serializer, autoSync);
                    break;

                case RoamingDataStore.OneDrive:
                    DataStore = new OneDriveDataStore(userId, dataStoreName, serializer, autoSync);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(dataStore));
            }

            DataStore.SyncCompleted += (s, e) => SyncCompleted?.Invoke(this, e);
            DataStore.SyncFailed += (s, e) => SyncFailed?.Invoke(this, e);

            if (syncOnInit)
            {
                _ = Sync();
            }
        }

        /// <summary>
        /// An indexer for easily accessing key values.
        /// </summary>
        /// <param name="key">The key for the desired value.</param>
        /// <returns>The value found for the provided key.</returns>
        public object this[string key]
        {
            get => DataStore.Read<object>(key);
            set => DataStore.Save(key, value);
        }

        /// <summary>
        /// Determines whether a file already exists.
        /// </summary>
        /// <param name="filePath">Key of the file (that contains object).</param>
        /// <returns>True if a value exists.</returns>
        public Task<bool> FileExistsAsync(string filePath) => DataStore.FileExistsAsync(filePath);

        /// <summary>
        /// Determines whether a setting already exists.
        /// </summary>
        /// <param name="key">Key of the setting (that contains object).</param>
        /// <returns>True if a value exists.</returns>
        public bool KeyExists(string key) => DataStore.KeyExists(key);

        /// <summary>
        /// Determines whether a setting already exists in composite.
        /// </summary>
        /// <param name="compositeKey">Key of the composite (that contains settings).</param>
        /// <param name="key"> Key of the setting (that contains object).</param>
        /// <returns>True if a value exists.</returns>
        public bool KeyExists(string compositeKey, string key) => DataStore.KeyExists(compositeKey, key);

        /// <summary>
        /// Retrieves a single item by its key.
        /// </summary>
        /// <param name="key">Key of the object.</param>
        /// <param name="default">Default value of the object.</param>
        /// <typeparam name="T">Type of object retrieved.</typeparam>
        /// <returns>The T object.</returns>
        public T Read<T>(string key, T @default = default) => DataStore.Read<T>(key, @default);

        /// <summary>
        /// Retrieves a single item by its key in composite.
        /// </summary>
        /// <param name="compositeKey"> Key of the composite (that contains settings).</param>
        /// <param name="key">Key of the object.</param>
        /// <param name="default">Default value of the object.</param>
        /// <typeparam name="T">Type of object retrieved.</typeparam>
        /// <returns>The T object.</returns>
        public T Read<T>(string compositeKey, string key, T @default = default) => DataStore.Read(compositeKey, key, @default);

        /// <summary>
        /// Retrieves an object from a file.
        /// </summary>
        /// <param name="filePath">Path to the file that contains the object.</param>
        /// <param name="default">Default value of the object.</param>
        /// <typeparam name="T">Type of object retrieved.</typeparam>
        /// <returns>Waiting task until completion with the object in the file.</returns>
        public Task<T> ReadFileAsync<T>(string filePath, T @default = default) => DataStore.ReadFileAsync(filePath, @default);

        /// <summary>
        /// Saves a single item by its key.
        /// </summary>
        /// <param name="key">Key of the value saved.</param>
        /// <param name="value">Object to save.</param>
        /// <typeparam name="T">Type of object saved.</typeparam>
        public void Save<T>(string key, T value) => DataStore.Save<T>(key, value);

        /// <summary>
        /// Saves a group of items by its key in a composite. This method should be considered
        /// for objects that do not exceed 8k bytes during the lifetime of the application
        /// (refers to Microsoft.Toolkit.Uwp.Helpers.IObjectStorageHelper.SaveFileAsync``1(System.String,``0)
        /// for complex/large objects) and for groups of settings which need to be treated
        /// in an atomic way.
        /// </summary>
        /// <param name="compositeKey">Key of the composite (that contains settings).</param>
        /// <param name="values">Objects to save.</param>
        /// <typeparam name="T">Type of object saved.</typeparam>
        public void Save<T>(string compositeKey, IDictionary<string, T> values) => DataStore.Save<T>(compositeKey, values);

        /// <summary>
        /// Saves an object inside a file.
        /// </summary>
        /// <param name="filePath">Path to the file that will contain the object.</param>
        /// <param name="value">Object to save.</param>
        /// <typeparam name="T">Type of object saved.</typeparam>
        /// <returns>Waiting task until completion.</returns>
        public Task<StorageFile> SaveFileAsync<T>(string filePath, T value) => DataStore.SaveFileAsync<T>(filePath, value);

        /// <summary>
        /// Create a new storage container.
        /// </summary>
        /// <returns>A Task.</returns>
        public Task Create() => DataStore.Create();

        /// <summary>
        /// Delete the existing storage container.
        /// </summary>
        /// <returns>A Task.</returns>
        public Task Delete() => DataStore.Delete();

        /// <summary>
        /// Syncronize the internal cache with the remote storage endpoint.
        /// </summary>
        /// <returns>A Task.</returns>
        public async Task Sync()
        {
            try
            {
                await DataStore.Sync();
            }
            catch
            {
                // Sync may fail if the storage container does not yet exist.
                await DataStore.Create();
                await DataStore.Sync();
            }
        }
    }
}