# WindowsProvider

The WindowsProvider is an authentication provider for accessing locally configured accounts on Windows. 
It extends IProvider and uses the native AccountsSettingsPane APIs for login.

## Syntax 

```CSharp
// Provider config
string clientId = "YOUR_CLIENT_ID_HERE"; // Not used with MSA login, but shown here for completeness.
string[] scopes = { "User.Read", "People.Read", "Calendars.Read", "Mail.Read" };
bool autoSignIn = true;

// Easily create a new WindowsProvider instance and set the GlobalProvider.
// The provider will attempt to sign in automatically. 
ProviderManager.Instance.GlobalProvider = new WindowsProvider(scopes);

// Additional parameters are also available,
// such as custom settings commands for the AccountsSettingsPane.
Guid settingsCommandId = Guid.NewGuid();
void OnSettingsCommandInvoked(IUICommand command)
{
    System.Diagnostics.Debug.WriteLine("AccountsSettingsPane command invoked: " + command.Id);
}

// Configure which types accounts should be available to choose from. The default is MSA, but AAD will come in the future.
// ClientId is not used with MSA login, but shown here for completeness.
var webAccountProviderConfig = new WebAccountProviderConfig(WebAccountProviderType.MSA, clientId);

// Configure details to present in the AccountsSettingsPane, such as custom header text and links.
var accountsSettingsPaneConfig = new AccountsSettingsPaneConfig(
    headerText: "Custom header text", 
    commands: new List<SettingsCommand>()
    {
        new SettingsCommand(settingsCommandId: settingsCommandId, label: "Click me!", handler: OnSettingsCommandInvoked)
    });

// Determine it the provider should automatically sign in or not. Default is true.
// Set to false to delay silent sign in until TrySilentSignInAsync is called.
bool autoSignIn = false;

// Set the GlobalProvider with the extra configuration
ProviderManager.Instance.GlobalProvider = new WindowsProvider(scopes, accountsSettingsPaneConfig, webAccountProviderConfig, autoSignIn);
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
