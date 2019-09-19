# Windows Graph Controls

This is a sub-repo for the [Windows Community Toolkit](https://aka.ms/wct) focused on [Microsoft Graph](https://developer.microsoft.com/en-us/graph/) providing a set of Helpers and Controls for development on Windows 10 with .NET.

This new library replaces the `Microsoft.Toolkit.Uwp.UI.Controls.Graph` package; however, it is not backwards compatible nor does it provide all the same features at this time.

If you need similar controls for the Web, please use the [Microsoft Graph Toolkit](https://aka.ms/mgt).

## <a name="supported"></a> Supported SDKs

* Windows 10 18362 (🚧 TODO: Check Lower SDKs)
* Android via [Uno.Graph-Controls](https://aka.ms/wgt-uno) use `Uno.Microsoft.Graph.Controls` package.
* 🚧 Coming Soon 🚧
  * PeoplePicker control
  * XAML Islands Sample
  * iOS (Waiting on [MSAL#1378](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/pull/1378) merge should be 4.4.0?)

## <a name="documentation"></a> Getting Started

Before using controls that access [Microsoft Graph](https://graph.microsoft.com), you will need to [register your application](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app) to get a **ClientID**.

> After finishing the initial registration page, you will also need to add an additional redirect URI. Clcik on "Add a Redirect URI" and check the `https://login.microsoftonline.com/common/oauth2/nativeclient` checkbox on that page. Then click "Save".

### Android Quick Start

To include the latest preview package in your Visual Studio environment, open your _Package Manager Console_ and type:

```powershell
Install-Package Uno.Microsoft.Toolkit.Graph.Controls -IncludePrerelease
```

Then open your shared `MainPage.xaml.cs` file and add the following initialization in your constructor:

<!-- 🚧 TODO: Can we simplify this pattern somehow in the future? -->

```csharp
            var ClientId = "YOUR_CLIENT_ID_HERE";
            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                ProviderManager.Instance.GlobalProvider = await QuickCreate.CreateMsalProviderAsync(
                    ClientId,
#if __ANDROID__
                    $"msal{ClientId}://auth", // Need to change redirectUri on Android for protocol registration from AndroidManifest.xml, ClientId needs to be updated there as well to match above.
#endif
                    scopes: new string[] { "user.read", "user.readbasic.all", "people.read" });
            });
```

You can use the `Scopes` property to preemptively request permissions from the user of your app for data your app needs to access from Microsoft Graph.

Then also update the `data` tag in your **AndroidManifest.xml** file:

```xml
<data android:scheme="msalYOUR_CLIENT_ID_HERE" android:host="auth" />
```

You need this for the protocol redirect after the user authenticates.

**That's all you need to get started!**

You can add any of the controls now to your XAML pages like we've done in our [sample](SampleGraphApp/SampleGraphApp.Shared/MainPage.xaml).

You can use the `ProviderManager.Instance` to listen to changes in authentication status with the `ProviderUpdated` event or get direct access to the [.NET Graph Beta API](https://github.com/microsoftgraph/msgraph-beta-sdk-dotnet) through `ProviderManager.Instance.GlobalProvider.Graph`, just be sure to check if the `GlobalProvider` has been set first and its `State` is `SignedIn`:

```csharp
var provider = ProviderManager.Instance.GlobalProvider;

if (provider != null && provider.State == ProviderState.SignedIn)
{
    // Do graph call here with provider.Graph...
}
```

### UWP Quick Start

Visit the [windows-toolkit/graph-controls](https://aka.ms/wgt) repo for instructions on using the library on UWP.

## Build Status
| Target | Branch | Status | Recommended package version |
| ------ | ------ | ------ | ------ |
| Pre-release beta testing | master | [![Build Status](https://uno-platform.visualstudio.com/Uno%20Platform/_apis/build/status/Uno%20Platform/Uno.Graph-Controls?branchName=master)](https://uno-platform.visualstudio.com/Uno%20Platform/_build/latest?definitionId=64&branchName=master) | - |

## Feedback and Requests
Please use [GitHub Issues](https://github.com/windows-toolkit/Graph-Controls/issues) for bug reports and feature requests.

## Principles
This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/)
to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](http://dotnetfoundation.org/code-of-conduct).

## .NET Foundation
This project is supported by the [.NET Foundation](http://dotnetfoundation.org).
