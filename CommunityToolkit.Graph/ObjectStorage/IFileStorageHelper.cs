using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommunityToolkit.Graph.ObjectStorage
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFileStorageHelper
    {
        /// <summary>
        /// Determines whether a file already exists.
        /// </summary>
        /// <param name="filePath">Key of the file (that contains object).</param>
        /// <returns>True if a value exists.</returns>
        Task<bool> FileExistsAsync(string filePath);

        /// <summary>
        /// Determines whether a folder already exists.
        /// </summary>
        /// <param name="folderPath">Key of the folder.</param>
        /// <returns>True if a value exists.</returns>
        Task<bool> FolderExistsAsync(string folderPath);

        /// <summary>
        /// Retrieves an object from a file.
        /// </summary>
        /// <typeparam name="T">Type of object retrieved.</typeparam>
        /// <param name="filePath">Path to the file that contains the object.</param>
        /// <param name="default">Default value of the object.</param>
        /// <returns>Waiting task until completion with the object in the file.</returns>
        Task<T> ReadFileAsync<T>(string filePath, T @default = default(T));

        /// <summary>
        /// Retrieves all file listings for a folder.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        Task<IList<string>> ReadFolderAsync<T>(string folderPath);

        /// <summary>
        /// Saves an object inside a file.
        /// </summary>
        /// <typeparam name="T">Type of object saved.</typeparam>
        /// <param name="filePath">Path to the file that will contain the object.</param>
        /// <param name="value">Object to save.</param>
        /// <returns>Waiting task until completion.</returns>
        Task SaveFileAsync<T>(string filePath, T value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        Task SaveFolderAsync(string folderPath);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemPath"></param>
        /// <returns></returns>
        Task DeleteItemAsync(string itemPath);
    }
}
