using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private IObjectStorageHelper DataStore;

        public RoamingSettingsHelper(RoamingDataStore dataStore)
        {
            switch (dataStore)
            {
                case RoamingDataStore.UserExtensions:
                    DataStore = new UserExtensionDataStore();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataStore));
            }
        }

        /// <inheritdoc />
        public Task<bool> FileExistsAsync(string filePath) => DataStore.FileExistsAsync(filePath);

        /// <inheritdoc />
        public bool KeyExists(string key) => KeyExists(key);

        /// <inheritdoc />
        public bool KeyExists(string compositeKey, string key) => KeyExists(compositeKey, key);

        /// <inheritdoc />
        public T Read<T>(string key, T @default = default) => Read<T>(key, @default);

        /// <inheritdoc />
        public T Read<T>(string compositeKey, string key, T @default = default) => Read<T>(compositeKey, key, @default);

        /// <inheritdoc />
        public Task<T> ReadFileAsync<T>(string filePath, T @default = default) => ReadFileAsync<T>(filePath, @default);

        /// <inheritdoc />
        public void Save<T>(string key, T value) => Save<T>(key, value);

        /// <inheritdoc />
        public void Save<T>(string compositeKey, IDictionary<string, T> values) => Save<T>(compositeKey, values);

        /// <inheritdoc />
        public Task<StorageFile> SaveFileAsync<T>(string filePath, T value) => SaveFileAsync<T>(filePath, value);
    }
}
