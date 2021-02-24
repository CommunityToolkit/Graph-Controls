namespace Microsoft.Toolkit.Graph.RoamingSettings
{
    public class RoamingSettings : UserExtensionBase
    {
        public override string ExtensionId => "com.contoso.roamingSettings";

        public RoamingSettings(string userId) : base(userId)
        {
        }
    }
}
