// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Storage;

namespace CommunityToolkit.Uwp.Graph.Helpers.RoamingSettings
{
    /// <summary>
    /// A base class for easily building roaming settings helper implementations.
    /// </summary>
    public abstract class BaseRoamingSettingsDataStore : IRoamingSettingsDataStore
    {
        /// <inheritdoc />
        public EventHandler SyncCompleted { get; set; }

        /// <inheritdoc />
        public EventHandler SyncFailed { get; set; }

        /// <inheritdoc />
        public bool AutoSync { get; }

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public string UserId { get; }

        /// <inheritdoc />
        public IDictionary<string, object> Cache { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets an object serializer for converting objects in the data store.
        /// </summary>
        protected IObjectSerializer Serializer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRoamingSettingsDataStore"/> class.
        /// </summary>
        /// <param name="userId">The id of the target Graph user.</param>
        /// <param name="dataStoreId">A unique id for the data store.</param>
        /// <param name="objectSerializer">An IObjectSerializer used for serializing objects.</param>
        /// <param name="autoSync">Determines if the data store should sync for every interaction.</param>
        public BaseRoamingSettingsDataStore(string userId, string dataStoreId, IObjectSerializer objectSerializer, bool autoSync = true)
        {
            AutoSync = autoSync;
            Id = dataStoreId;
            UserId = userId;
            Serializer = objectSerializer;
        }

        /// <summary>
        /// Create a new instance of the data storage container.
        /// </summary>
        /// <returns>A task.</returns>
        public abstract Task Create();

        /// <summary>
        /// Delete the instance of the data storage container.
        /// </summary>
        /// <returns>A task.</returns>
        public abstract Task Delete();

        /// <summary>
        /// Determines whether a setting already exists.
        /// </summary>
        /// <param name="key">Key of the setting (that contains object).</param>
        /// <returns>True if a value exists.</returns>
        public bool KeyExists(string key)
        {
            return Cache?.ContainsKey(key) ?? false;
        }

        /// <summary>
        /// Determines whether a setting already exists in composite.
        /// </summary>
        /// <param name="compositeKey">Key of the composite (that contains settings).</param>
        /// <param name="key"> Key of the setting (that contains object).</param>
        /// <returns>True if a value exists.</returns>
        public bool KeyExists(string compositeKey, string key)
        {
            if (KeyExists(compositeKey))
            {
                ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)Cache[compositeKey];
                if (composite != null)
                {
                    return composite.ContainsKey(key);
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves a single item by its key.
        /// </summary>
        /// <param name="key">Key of the object.</param>
        /// <param name="default">Default value of the object.</param>
        /// <typeparam name="T">Type of object retrieved.</typeparam>
        /// <returns>The T object.</returns>
        public T Read<T>(string key, T @default = default)
        {
            if (Cache != null && Cache.TryGetValue(key, out object value))
            {
                return DeserializeValue<T>(value);
            }

            return @default;
        }

        /// <summary>
        /// Retrieves a single item by its key in composite.
        /// </summary>
        /// <param name="compositeKey"> Key of the composite (that contains settings).</param>
        /// <param name="key">Key of the object.</param>
        /// <param name="default">Default value of the object.</param>
        /// <typeparam name="T">Type of object retrieved.</typeparam>
        /// <returns>The T object.</returns>
        public T Read<T>(string compositeKey, string key, T @default = default)
        {
            if (Cache != null)
            {
                ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)Cache[compositeKey];
                if (composite != null)
                {
                    object value = composite[key];
                    if (value != null)
                    {
                        return DeserializeValue<T>(value);
                    }
                }
            }

            return @default;
        }

        /// <summary>
        /// Saves a single item by its key.
        /// </summary>
        /// <param name="key">Key of the value saved.</param>
        /// <param name="value">Object to save.</param>
        /// <typeparam name="T">Type of object saved.</typeparam>
        public void Save<T>(string key, T value)
        {
            // Update the cache
            Cache[key] = SerializeValue(value);

            if (AutoSync)
            {
                // Update the remote
                Task.Run(() => Sync());
            }
        }

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
        public void Save<T>(string compositeKey, IDictionary<string, T> values)
        {
            var type = typeof(T);
            var typeInfo = type.GetTypeInfo();

            if (KeyExists(compositeKey))
            {
                ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)Cache[compositeKey];

                foreach (KeyValuePair<string, T> setting in values.ToList())
                {
                    string key = setting.Key;
                    object value = SerializeValue(setting.Value);
                    if (composite.ContainsKey(setting.Key))
                    {
                        composite[key] = value;
                    }
                    else
                    {
                        composite.Add(key, value);
                    }
                }

                // Update the cache
                Cache[compositeKey] = composite;

                if (AutoSync)
                {
                    // Update the remote
                    Task.Run(() => Sync());
                }
            }
            else
            {
                ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
                foreach (KeyValuePair<string, T> setting in values.ToList())
                {
                    string key = setting.Key;
                    object value = SerializeValue(setting.Value);
                    composite.Add(key, value);
                }

                // Update the cache
                Cache[compositeKey] = composite;

                if (AutoSync)
                {
                    // Update the remote
                    Task.Run(() => Sync());
                }
            }
        }

        /// <summary>
        /// Determines whether a file already exists.
        /// </summary>
        /// <param name="filePath">Key of the file (that contains object).</param>
        /// <returns>True if a value exists.</returns>
        public abstract Task<bool> FileExistsAsync(string filePath);

        /// <summary>
        /// Retrieves an object from a file.
        /// </summary>
        /// <param name="filePath">Path to the file that contains the object.</param>
        /// <param name="default">Default value of the object.</param>
        /// <typeparam name="T">Type of object retrieved.</typeparam>
        /// <returns>Waiting task until completion with the object in the file.</returns>
        public abstract Task<T> ReadFileAsync<T>(string filePath, T @default = default);

        /// <summary>
        /// Saves an object inside a file.
        /// </summary>
        /// <param name="filePath">Path to the file that will contain the object.</param>
        /// <param name="value">Object to save.</param>
        /// <typeparam name="T">Type of object saved.</typeparam>
        /// <returns>Waiting task until completion.</returns>
        public abstract Task<StorageFile> SaveFileAsync<T>(string filePath, T value);

        /// <inheritdoc />
        public abstract Task Sync();

        /// <summary>
        /// Delete the internal cache.
        /// </summary>
        protected void DeleteCache()
        {
            Cache.Clear();
        }

        /// <summary>
        /// Use the serializer to deserialize a value appropriately for the type.
        /// </summary>
        /// <typeparam name="T">The type of object expected.</typeparam>
        /// <param name="value">The value to deserialize.</param>
        /// <returns>An object of type T.</returns>
        protected T DeserializeValue<T>(object value)
        {
            try
            {
                return Serializer.Deserialize<T>((string)value);
            }
            catch
            {
                // Primitive types can't be deserialized.
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }

        /// <summary>
        /// Use the serializer to serialize a value appropriately for the type.
        /// </summary>
        /// <typeparam name="T">The type of object being serialized.</typeparam>
        /// <param name="value">The object to serialize.</param>
        /// <returns>The serialized object.</returns>
        protected object SerializeValue<T>(T value)
        {
            var type = typeof(T);
            var typeInfo = type.GetTypeInfo();

            // Skip serialization for primitives.
            if (typeInfo.IsPrimitive || type == typeof(string))
            {
                // Update the cache
                return value;
            }
            else
            {
                // Update the cache
                return Serializer.Serialize(value);
            }
        }
    }
}
