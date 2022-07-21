using BuildsManagerTestTask.Models;
using Newtonsoft.Json;

namespace BuildsManagerTestTask.Clients;

internal class AppCenterClient
{
    private static readonly HttpClient client = new HttpClient();
    private readonly string token;
    private readonly Uri baseAddress;

    public AppCenterClient(string apiToken, string baseApiAddress)
    {
        this.token = apiToken;
        this.baseAddress = new Uri(baseApiAddress);
    }

    private static void SetUpClient(HttpClient client, string apiToken, Uri baseApiAddress)
    {
        client.BaseAddress = baseApiAddress;
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Add("X-API-Token", apiToken);
    }

    private static async Task<T> InvokeWebRequestAsync<T>(HttpClient client, HttpRequestMessage request, string apiToken, Uri baseApiAddress)
    {
        AppCenterClient.SetUpClient(client, apiToken, baseApiAddress);

        HttpResponseMessage response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error has occurred while performing web request. Status code: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<T>(content);
    }
}
