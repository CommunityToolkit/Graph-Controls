# Manual Graph Request Sample for UWP

This sample demonstrates how to make a request to Microsoft Graph APIs without the SDK.

```csharp
private async Task<IList<TodoTask>> GetDefaultTaskListAsync()
{   
    return await GetResponseAsync<List<TodoTask>>("https://graph.microsoft.com/v1.0/me/todo/lists/tasks/tasks");
}

private async Task<T> GetResponseAsync<T>(string requestUri)
{
    // Build the request
    HttpRequestMessage getRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);

    // Authenticate the request
    await ProviderManager.Instance.GlobalProvider.AuthenticateRequestAsync(getRequest);

    var httpClient = new HttpClient();
    using (httpClient)
    {
        // Send the request
        var response = await httpClient.SendAsync(getRequest);

        if (response.IsSuccessStatusCode)
        {
            // Handle the request response
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jObject = JObject.Parse(jsonResponse);
            if (jObject.ContainsKey("value"))
            {
                var result = JsonConvert.DeserializeObject<T>(jObject["value"].ToString());
                return result;
            }
        }
    }

    return default;
}
```