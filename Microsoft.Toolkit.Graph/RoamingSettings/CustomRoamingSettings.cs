namespace Microsoft.Toolkit.Graph.RoamingSettings
{
    public class CustomRoamingSettings : UserExtensionBase
    {
        public override string ExtensionId => "com.custom.roamingSettings";

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRoamingSettings"/> class.
        /// </summary>
        /// <param name="userId"></param>
        public CustomRoamingSettings(string userId) : base(userId)
        {

        }
    }
}
