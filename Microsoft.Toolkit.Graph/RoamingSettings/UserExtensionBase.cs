using System.Threading.Tasks;
using Microsoft.Graph;

namespace Microsoft.Toolkit.Graph.RoamingSettings
{
    public abstract class UserExtensionBase
    {
        public abstract string ExtensionId { get; }

        private string _userId;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserExtensionBase"/> class.
        /// </summary>
        /// <param name="userId"></param>
        public UserExtensionBase(string userId)
        {
            _userId = userId;
        }

        public virtual async Task<object> Get(string key)
        {
            return await Get(ExtensionId, _userId, key);
        }

        public virtual async Task<T> Get<T>(string key)
        {
            return (T)await Get(ExtensionId, _userId, key);
        }

        public virtual async Task Set(string key, object value)
        {
            var userExtension = await GetExtensionForUser(ExtensionId, _userId);
            await userExtension.SetValue(_userId, key, value);
        }

        public virtual async Task<Extension> Create()
        {
            var userExtension = await Create(ExtensionId, _userId);
            return userExtension;
        }

        public virtual async Task Delete()
        {
            await Delete(ExtensionId, _userId);
        }

        public static async Task<T> Get<T>(string extensionId, string userId, string key)
        {
            return (T)await Get(extensionId, userId, key);
        }

        public static async Task<object> Get(string extensionId, string userId, string key)
        {
            var userExtension = await GetExtensionForUser(extensionId, userId);
            return userExtension.AdditionalData[key];
        }

        public static async Task Set(string extensionId, string userId, string key, object value)
        {
            var userExtension = await GetExtensionForUser(extensionId, userId);
            await userExtension.SetValue(userId, key, value);
        }

        public static async Task<Extension> Create(string extensionId, string userId)
        {
            var userExtension = await UserExtensionsDataSource.CreateExtension(userId, extensionId);
            return userExtension;
        }

        public static async Task Delete(string extensionId, string userId)
        {
            await UserExtensionsDataSource.DeleteExtension(userId, extensionId);
        }

        public static async Task<Extension> GetExtensionForUser(string extensionId, string userId)
        {
            var userExtension = await UserExtensionsDataSource.GetExtension(userId, extensionId);
            return userExtension;
        }
    }
}
