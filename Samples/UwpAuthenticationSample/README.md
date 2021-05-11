# WindowsProvider Authentication Sample for UWP

This sample demonstrates how to configure the WindowsProvider to authenticate consumer MSAs in your apps. 

```
string[] scopes = new string[] { "User.Read" };
ProviderManager.Instance.GlobalProvider = new WindowsProvider(scopes);
```

It uses a simple IProvider implementation called WindowsProvider, which leverages native Windows Account Manager 
APIs to enable authentication with any locally configured accounts on Windows.

Perhaps the best part is that it doesn't require any AAD configuration for authentication of consumer MSA accounts! 

By first registering your app with the Microsoft Store, the WindowsProvider will use the app registration built right into the Store association. 

> You will need a Microsoft Partner Center account for this, so if you don't have one yet head over to get signed up: https://partner.microsoft.com/dashboard