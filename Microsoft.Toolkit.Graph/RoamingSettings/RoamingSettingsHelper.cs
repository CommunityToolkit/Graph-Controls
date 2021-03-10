using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Storage;

namespace Microsoft.Toolkit.Graph.RoamingSettings
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
    }

    /// <summary>
    /// A helper class for syncing data to roaming data store.
    /// </summary>
    public class RoamingSettingsHelper : IObjectStorageHelper
    {
        /// <summary>
        /// Gets the internal data storage helper instance.
        /// </summary>
        public IObjectStorageHelper DataStore { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoamingSettingsHelper"/> class.
        /// </summary>
        /// <param name="serializer">An object serializer for serialization of objects in the data store.</param>
        /// <param name="userId">The id of the target Graph User.</param>
        /// <param name="dataStore">Which specific data store is being used.</param>
        /// <param name="autoSync">Whether the values should immediately sync or not.</param>
        public RoamingSettingsHelper(IObjectSerializer serializer, string userId, RoamingDataStore dataStore, bool autoSync = false)
        {
            switch (dataStore)
            {
                case RoamingDataStore.UserExtensions:
                    // Determine the extension id value using app identity.
                    string aumid = Windows.ApplicationModel.AppInfo.Current.AppUserModelId;
                    DataStore = new UserExtensionDataStore(serializer, "com.toolkit.roamingSettings." + aumid, userId, autoSync);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataStore));
            }
        }

        /// <inheritdoc />
        public Task<bool> FileExistsAsync(string filePath) => DataStore.FileExistsAsync(filePath);

        /// <inheritdoc />
        public bool KeyExists(string key) => DataStore.KeyExists(key);

        /// <inheritdoc />
        public bool KeyExists(string compositeKey, string key) => DataStore.KeyExists(compositeKey, key);

        /// <inheritdoc />
        public T Read<T>(string key, T @default = default) => DataStore.Read<T>(key, @default);

        /// <inheritdoc />
        public T Read<T>(string compositeKey, string key, T @default = default) => DataStore.Read<T>(compositeKey, key, @default);

        /// <inheritdoc />
        public Task<T> ReadFileAsync<T>(string filePath, T @default = default) => DataStore.ReadFileAsync<T>(filePath, @default);

        /// <inheritdoc />
        public void Save<T>(string key, T value) => DataStore.Save<T>(key, value);

        /// <inheritdoc />
        public void Save<T>(string compositeKey, IDictionary<string, T> values) => DataStore.Save<T>(compositeKey, values);

        /// <inheritdoc />
        public Task<StorageFile> SaveFileAsync<T>(string filePath, T value) => DataStore.SaveFileAsync<T>(filePath, value);
    }
}
