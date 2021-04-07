# WindowsProvider

The WindowsProvider is an authentication provider for accessing locally configured accounts on Windows. 
It extends IProvider and uses the native AccountsSettingsPane APIs for login.

> [!IMPORTANT]
> Be sure to Register Client Id in Azure first following the guidance here: <https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app>
>
> After finishing the initial registration page, you will also need to add an additional redirect URI. Click on "Add a Redirect URI" and check the "https://login.microsoftonline.com/common/oauth2/nativeclient" checkbox on that page. 
> 
> You'll also want to set the toggle to true for "Allow public client flows".
> 
> Then click "Save".

## Syntax 

```CSharp
// Simple
string clientId = "YOUR_CLIENT_ID_HERE";
string[] scopes = new string[] { "User.Read" };

ProviderManager.Instance.GlobalProvider = new WindowsProvider(clientId, scopes);

// Customized AccountsSettingsPane
IList<SettingsCommand> commands = new List<SettingsCommand>() 
{ 
    new SettingsCommand(Guid.NewGuid(), "Click me!", OnSettingsCommandInvoked) 
}
AccountsSettingsPaneConfig accountsSettingsPaneConfig = new AccountsSettingsPaneConfig()
{
    HeaderText = "Custom header text goes here.",
    Commands = commands
}

WindowsProvider windowsProvider = new WindowsProvider(clientId, scopes, accountsSettingsPaneConfig);
ProviderManager.Instance.GlobalProvider = windowsProvider;

// Silent login
await windowsProvider.TrySilentLoginAsync();
```

## Prerequisite Windows Store Association in Visual Studio
To get valid tokens and complete login, the app will need to be associated with the Microsoft Store.

1. In Visual Studio Solution Explorer, right-click the UWP project, then select **Store -> Associate App with the Store...**

2. In the wizard, click **Next**, sign in with your Windows developer account, type a name for your app in **Reserve a new app name**, then click **Reserve**.

3. After completing the app registration, select the new app name, click **Next**, and then click **Associate**. This adds the required Windows Store registration information to the application manifest.

> [!NOTE]
> You must have a Windows Developer account to use the WindowsProvider in your UWP app. You can [register a Microsoft developer account](https://developer.microsoft.com/store/register) if you don't already have one.


## Properties

See IProvider for a full list of supported properties.

| Property | Type | Description |
| -- | -- | -- |
| ClientId | string | Client Id obtained from Azure registration. |
| Scopes | ScopeSet | Comma-delimited list of scopes to pre-authorize from user during authentication. |
| AccountsSettingsPaneConfig | AccountsSettingsPaneConfig | Configuration values for the AccountsSettingsPane, shown during authentication. |
| RedirectUri | string | Static getter for retrieving a customized redirect uri to put in the Azure app registration. |

### AccountsSettingsPane

| Property | Type | Description |
| -- | -- | -- |
| HeaderText | string | Gets or sets the header text for the accounts settings pane. |
| Commands | IList<SettingsCommand> | Gets or sets the SettingsCommand collection for the account settings pane. |


## Methods

See IProvider for a full list of supported methods.

| Method | Arguments | Returns | Description |
| -- | -- | -- | -- |
| GetTokenAsync | bool silentOnly = true | Task | Retrieve a token for the authenticated user. |
| TrySilentLoginAsync | | Task<bool> | Try logging in silently, without prompts. |
