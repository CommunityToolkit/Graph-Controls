# Windows Graph Controls

This is a sub-repo for the [Windows Community Toolkit](https://aka.ms/wct) focused on [Microsoft Graph](https://developer.microsoft.com/en-us/graph/) providing a set of Helpers and Controls for development on Windows 10 with .NET.

This new library replaces the `Microsoft.Toolkit.Uwp.UI.Controls.Graph` package; however, it is not backwards compatible nor does it provide all the same features at this time.

If you need similar controls for the Web, please use the [Microsoft Graph Toolkit](https://aka.ms/mgt).

## <a name="supported"></a> Supported SDKs

* UWP Windows 10 18362 (🚧 TODO: Check Lower SDKs for UWP)
* XAML Islands on .NET Core 3 w/ Windows 10 18362 (See Sample)
* `LoginButton` & `PersonView` on Android via [Uno.Graph-Controls](https://aka.ms/wgt-uno) use `Uno.Microsoft.Graph.Controls` package. (🚧 `PeoplePicker` soon!)
* 🚧 Coming Soon 🚧
  * iOS (Waiting on [MSAL#1378](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/pull/1378) merge should be 4.4.0?)

## <a name="documentation"></a> Getting Started

Before using controls that access [Microsoft Graph](https://graph.microsoft.com), you will need to [register your application](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app) to get a **ClientID**.

> After finishing the initial registration page, you will also need to add an additional redirect URI. Click on "Add a Redirect URI", then "Add a platform", and then on "Mobile and desktop applications". Check the `https://login.microsoftonline.com/common/oauth2/nativeclient` checkbox on that page. Then click "Configure".

### UWP Quick Start

Use the following command on the _Package Manager Console_ to install the NuGet to your project:

```powershell
Install-Package Microsoft.Toolkit.Graph.Controls -IncludePrerelease
```

Then open your `MainPage.xaml` file and add the following behavior:

```xml
<Page
    ...
    xmlns:Interactivity="using:Microsoft.Xaml.Interactivity"
    xmlns:providers="using:Microsoft.Toolkit.Graph.Providers">
    <Interactivity:Interaction.Behaviors>
        <providers:InteractiveProviderBehavior ClientId="YOUR_CLIENT_ID_HERE" Scopes="User.Read,User.ReadBasic.All,People.Read"/>
    </Interactivity:Interaction.Behaviors>
</Page>
```

*You can also use the `<providers:MockProviderBehavior/>` instead to provide some test data.*

You can use the `Scopes` property to preemptively request permissions from the user of your app for data your app needs to access from Microsoft Graph.

You only need the main/first page of your app to load use the provider behavior. This will initialize the graph provider for your entire application.

**That's all you need to get started!**

You can add any of the controls now to your XAML pages like we've done in our [sample](SampleTest/MainPage.xaml).

You can use the `ProviderManager.Instance` to listen to changes in authentication status with the `ProviderUpdated` event or get direct access to the [.NET Graph Beta API](https://github.com/microsoftgraph/msgraph-beta-sdk-dotnet) through `ProviderManager.Instance.GlobalProvider.Graph`, just be sure to check if the `GlobalProvider` has been set first and its `State` is `SignedIn`:

```csharp
var provider = ProviderManager.Instance.GlobalProvider;

if (provider != null && provider.State == ProviderState.SignedIn)
{
    // Do graph call here with provider.Graph...
}
```

### Android Quick Start

Visit the [Uno.Graph-Controls](https://aka.ms/wgt-uno) repo for instructions on using the library with Uno on Android.

## Build Status
| Target | Branch | Status | Recommended package version |
| ------ | ------ | ------ | ------ |
| Pre-release beta testing | master | [![Build Status](https://dev.azure.com/dotnet/WindowsCommunityToolkit/_apis/build/status/windows-toolkit.Graph-Controls?branchName=master)](https://dev.azure.com/dotnet/WindowsCommunityToolkit/_build/latest?definitionId=102&branchName=master) | [![MyGet](https://img.shields.io/dotnet.myget/uwpcommunitytoolkit/vpre/Microsoft.Toolkit.Graph.svg)](https://dotnet.myget.org/gallery/uwpcommunitytoolkit) |

## Feedback and Requests
Please use [GitHub Issues](https://github.com/windows-toolkit/Graph-Controls/issues) for bug reports and feature requests.

## Principles
This project has adopted the code of conduct defined by the [Contributor Covenant](http://contributor-covenant.org/)
to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](http://dotnetfoundation.org/code-of-conduct).

## .NET Foundation
This project is supported by the [.NET Foundation](http://dotnetfoundation.org).
