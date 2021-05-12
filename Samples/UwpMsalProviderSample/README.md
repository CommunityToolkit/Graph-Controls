# MsalProvider Authentication Sample for UWP

This sample demonstrates how to configure the MsalProvider to authenticate consumer MSA and organizational AAD accounts in your apps. 

```
string clientId = "YOUR-CLIENT-ID-HERE";
string[] scopes = new string[] { "User.Read" };
ProviderManager.Instance.GlobalProvider = new MsalProvider(clientId, scopes);
```

It uses an IProvider implementation called MsalProvider, which leverages the official Microsoft Authentication Library (MSAL)
to enable authentication for MSA and AAD accounts.