# MsalProvider Authentication Sample for .NET 6.0 WPF apps

This sample demonstrates how to configure the MsalProvider to authenticate consumer MSA and organizational AAD accounts in your apps.

```
string ClientId = "YOUR-CLIENT-ID-HERE";
string[] Scopes = new string[] { "User.Read" };

var provider = new MsalProvider(ClientId, Scopes, null, false, true);

// Configure the token cache storage for non-UWP applications.
var storageProperties = new StorageCreationPropertiesBuilder(CacheConfig.CacheFileName, CacheConfig.CacheDir)
    .WithLinuxKeyring(
        CacheConfig.LinuxKeyRingSchema,
        CacheConfig.LinuxKeyRingCollection,
        CacheConfig.LinuxKeyRingLabel,
        CacheConfig.LinuxKeyRingAttr1,
        CacheConfig.LinuxKeyRingAttr2)
    .WithMacKeyChain(
        CacheConfig.KeyChainServiceName,
        CacheConfig.KeyChainAccountName)
    .Build();
await provider.InitTokenCacheAsync(storageProperties);

ProviderManager.Instance.GlobalProvider = provider;

await provider.TrySilentSignInAsync();
```

It uses an IProvider implementation called MsalProvider, which leverages the official Microsoft Authentication Library (MSAL)
to enable authentication for MSA and AAD accounts.
