﻿using System.Collections.Generic;

namespace CommunityToolkit.Graph.ObjectStorage
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDictionaryStorageHelper
    {
        /// <summary>
        /// Determines whether a setting already exists.
        /// </summary>
        /// <param name="key">Key of the setting (that contains object).</param>
        /// <returns>True if a value exists.</returns>
        bool KeyExists(string key);

        /// <summary>
        /// Determines whether a setting already exists in composite.
        /// </summary>
        /// <param name="compositeKey">Key of the composite (that contains settings).</param>
        /// <param name="key">Key of the setting (that contains object).</param>
        /// <returns>True if a value exists.</returns>
        bool KeyExists(string compositeKey, string key);

        /// <summary>
        /// Retrieves a single item by its key.
        /// </summary>
        /// <typeparam name="T">Type of object retrieved.</typeparam>
        /// <param name="key">Key of the object.</param>
        /// <param name="default">Default value of the object.</param>
        /// <returns>The T object</returns>
        T Read<T>(string key, T @default = default(T));

        /// <summary>
        /// Retrieves a single item by its key in composite.
        /// </summary>
        /// <typeparam name="T">Type of object retrieved.</typeparam>
        /// <param name="compositeKey">Key of the composite (that contains settings).</param>
        /// <param name="key">Key of the object.</param>
        /// <param name="default">Default value of the object.</param>
        /// <returns>The T object.</returns>
        T Read<T>(string compositeKey, string key, T @default = default(T));

        /// <summary>
        /// Saves a single item by its key.
        /// </summary>
        /// <typeparam name="T">Type of object saved.</typeparam>
        /// <param name="key">Key of the value saved.</param>
        /// <param name="value">Object to save.</param>
        void Save<T>(string key, T value);

        /// <summary>
        /// Saves a group of items by its key in a composite.
        /// This method should be considered for objects that do not exceed 8k bytes during the lifetime of the application
        /// and for groups of settings which need to be treated in an atomic way.
        /// </summary>
        /// <typeparam name="T">Type of object saved.</typeparam>
        /// <param name="compositeKey">Key of the composite (that contains settings).</param>
        /// <param name="values">Objects to save.</param>
        void Save<T>(string compositeKey, IDictionary<string, T> values);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        void Delete(string key);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="compositeKey"></param>
        void Delete(string key, string compositeKey);
    }
}
