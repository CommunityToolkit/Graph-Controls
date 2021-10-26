# Windows Community Toolkit - Graph Helpers and Controls

Welcome! This is a sub-repo for the [Windows Community Toolkit](https://aka.ms/wct) focused on [Microsoft Graph](https://developer.microsoft.com/en-us/graph/) providing a set of Authentication and Graph helpers for Windows applications.

Note: This new library replaces the `Microsoft.Toolkit.Uwp.UI.Controls.Graph` package; however, it is not backwards compatible nor does it provide all the same features at this time.

If you need similar controls for the Web, please use the [Microsoft Graph Toolkit](https://aka.ms/mgt).

## <a name="supported"></a> Supported SDKs

| Package | Min Supported |
|--|--|
| `CommunityToolkit.Authentication` | NetStandard 2.0 |
| `CommunityToolkit.Authentication.Msal` | NetStandard 2.0, UWP, .NET 5, .NET 5 Windows 10.0.17763.0, .NET Core 3.1 |
| `CommunityToolkit.Authentication.Uwp` | UWP Windows 10.0.17134.0 |
| `CommunityTookit.Graph` | NetStandard 2.0 |
| `CommunityToolkit.Graph.Uwp` | UWP Windows 10.0.17763.0 |

## Samples

Check out our samples for getting started with authentication providers and making calls to Microsoft Graph:

- [UwpWindowsProviderSample](./Samples/UwpWindowsProviderSample)
- [UwpMsalProviderSample](./Samples/UwpMsalProviderSample)
- [WpfNetCoreMsalProviderSample](./Samples/WpfNetCoreMsalProviderSample)
- [WpfNetMsalProviderSample](./Samples/WpfNet5WindowsMsalProviderSample)
- [ManualGraphRequestSample](./Samples/ManualGraphRequestSample)

### Contoso Notes Sample

[Contoso Notes](https://github.com/CommunityToolkit/Sample-Graph-ContosoNotes) is a premier sample note-taking app infused with Graph powered features and controls from the Windows Community Toolkit, demonstrated in practical application scenarios.

## <a name="documentation"></a> Getting Started

To get started using Graph data in your application, you'll first need to enable authentication.

### 1A. Setup authentication with MSAL

Leverage the official Microsoft Authentication Library (MSAL) to enable authentication in any NetStandard application.

1. Register your app in Azure AAD
    
    Before requesting data from [Microsoft Graph](https://graph.microsoft.com), you will need to [register your application](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app) to get a **ClientID**.

    > After finishing the initial registration page, you will also need to add an additional redirect URI. Click on "Add a Redirect URI", then "Add a platform", and then on "Mobile and desktop applications". Check the `https://login.microsoftonline.com/common/oauth2/nativeclient` checkbox on that page. Then click "Configure".

3. Install the `CommunityToolkit.Authentication.Msal` package.
4. Set the GlobalProvder to a new instance of MsalProvider with clientId and pre-configured scopes:
    
    ```csharp
    using CommunityToolkit.Authentication;

    string clientId = "YOUR-CLIENT-ID-HERE";
    string[] scopes = new string[] { "User.Read" };

    ProviderManager.Instance.GlobalProvider = new MsalProvider(clientId, scopes);
    ```

> Note: You can use the `Scopes` property to preemptively request permissions from the user of your app for data your app needs to access from Microsoft Graph.

### 1B. Setup authentication with WindowsProvider

Try out the WindowsProvider to enable authentication based on the native Windows Account Manager (WAM) APIs in your UWP apps, without requiring a dependency on MSAL.

1. Associate your app with the Microsoft Store. The app association will act as our minimal app registration for authenticating consumer MSAs. See the [WindowsProvider docs](https://docs.microsoft.com/windows/communitytoolkit/graph/authentication/windows) for more details.
1. Install the `CommunityToolkit.Authentication.Uwp` package.
1. Set the GlobalProvider to a new instance of WindowsProvider with pre-configured scopes:

    ```csharp
    using CommunityToolkit.Authentication;

    string[] scopes = new string[] { "User.Read" };

    ProviderManager.Instance.GlobalProvider = new WindowsProvider(scopes);
    ```

### 2. Make a Graph request with the Graph SDK

Once you are authenticated, you can then make requests to the Graph using the GraphServiceClient instance.

> Install the `CommunityToolkit.Graph` package.

```csharp
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;

ProviderManager.Instance.ProviderStateChanged += OnProviderStateChanged;

void OnProviderStateChanged(object sender, ProviderStateChangedEventArgs args)
{
    var provider = ProviderManager.Instance.GlobalProvider;
    if (provider?.State == ProviderState.SignedIn)
    {
        var graphClient = provider.GetClient();
        var me = await graphClient.Me.Request().GetAsync();
    }
}
```

#### Make a Graph request manually

Alternatively if you do not wish to use the Graph SDK you can make requests to Microsoft Graph manually instead:

```csharp
using CommunityToolkit.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

private async Task<IList<TodoTask>> GetDefaultTaskListAsync()
{
    var httpClient = new HttpClient();
    var requestUri = "https://graph.microsoft.com/v1.0/me/todo/lists/tasks/tasks";

    var getRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
    await ProviderManager.Instance.GlobalProvider.AuthenticateRequestAsync(getRequest);

    using (httpClient)
    {
        var response = await httpClient.SendAsync(getRequest);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(jsonResponse);
            if (jObject.ContainsKey("value"))
            {
                var tasks = JsonConvert.DeserializeObject<List<TodoTask>>(jObject["value"].ToString());
                return tasks;
            }
        }
    }

    return null;
}
```

**That's all you need to get started!**

You can use the `ProviderManager.Instance` to listen to changes in authentication status with the `ProviderStateChanged` event or get direct access to the [.NET Graph Beta API](https://github.com/microsoftgraph/msgraph-beta-sdk-dotnet) through `ProviderManager.Instance.GlobalProvider.GetBetaClient()`, just be sure to check if the `GlobalProvider` has been set first and its `State` is `SignedIn`:

```csharp
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;

public ImageSource GetMyPhoto()
{
    IProvider provider = ProviderManager.Instance.GlobalProvider;
    
    if (provider?.State == ProviderState.SignedIn)
    {
        // Get the beta client
        GraphServiceClient betaGraphClient = provider.GetBetaClient();

        try
        {
            // Make a request to the beta endpoint for the current user's photo.
            var photoStream = await betaGraphClient.Me.Photo.Content.Request().GetAsync();

            using var ras = photoStream.AsRandomAccessStream();
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(ras);

            return bitmap;
        }
        catch
        {
        }
    }

    return null;
}
```

## Feedback and Requests
Please use [GitHub Issues](https://github.com/CommunityToolkit/Graph-Controls/issues) for bug reports and feature requests.

## Principles
This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/)
to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](http://dotnetfoundation.org/code-of-conduct).

## .NET Foundation
This project is supported by the [.NET Foundation](http://dotnetfoundation.org).
