# How To: Quickly setup the WindowsProvider for authentication
Here are some quick setup steps for using the WindowsProvider in a UWP app:
 
1. Open Visual Studio and create a new C# UWP application.
 
2. Add the following nuget packages (Nuget feed: [WindowsCommunityToolkit-MainLatest](https://pkgs.dev.azure.com/dotnet/WindowsCommunityToolkit/_packaging/WindowsCommunityToolkit-MainLatest/nuget/v3/index.json)):
    * `CommunityToolkit.Uwp.Authentication` – For the WindowsProvider
    * `CommunityToolkit.Uwp.Graph.Controls` – For the LoginButton
 
3. Open the App.xaml.cs and add the following code to the bottom of the OnLaunched. You’ll need to make the method async as well.

```
// Find this line in the OnLaunched method and add the following code immediately after.
Window.Current.Activate();
 
string[] scopes = new string[] { "User.Read", "Tasks.ReadWrite" };
ProviderManager.Instance.GlobalProvider = new WindowsProvider(scopes);
```
 
4. Add a LoginButton to the MainPage.xaml

```
xmlns:graphcontrols="using:CommunityToolkit.Uwp.Graph.Controls"
 
<graphcontrols:LoginButton />
```
 
5. Associate the app to the Store in VS by selecting `Project` -> `Publish` -> `Associate App with the Store...` and following the prompts.
 
6. Run the app and click the button to initiate login.
