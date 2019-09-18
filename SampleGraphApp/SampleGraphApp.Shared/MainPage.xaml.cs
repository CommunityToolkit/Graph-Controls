using Microsoft.Toolkit.Graph.Providers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SampleGraphApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            // Register Client ID Instructions: https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app
            var ClientId = "CLIENT_ID_HERE";
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                ProviderManager.Instance.GlobalProvider = await QuickCreate.CreateMsalProviderAsync(
                    ClientId,
#if __ANDROID__
                    $"msal{ClientId}://auth", // Need to change redirectUri on Android for protocol registration from AndroidManifest.xml, ClientId needs to be updated there as well to match above.
#endif
                    scopes: new string[] { "user.read", "user.readbasic.all", "people.read" });
            });

            this.InitializeComponent();
        }
    }
}
