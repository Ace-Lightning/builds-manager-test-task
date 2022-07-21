using BuildsManagerTestTask.Models;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Web;

namespace BuildsManagerTestTask.Clients;

internal class AppCenterClient
{
    private HttpClient _client;

    private HttpClient Client => this._client ?? (_client = new HttpClient());

    public AppCenterClient(string apiToken, string baseApiAddress)
    {
        this.SetUpClient(this.Client, apiToken, new Uri(baseApiAddress));
    }

    private void SetUpClient(HttpClient client, string apiToken, Uri baseApiAddress)
    {
        client.BaseAddress = baseApiAddress;
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Add("X-API-Token", apiToken);
    }

    private async Task<T> InvokeWebRequestAsync<T>(HttpClient client, HttpRequestMessage request)
    {
        HttpResponseMessage response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error has occurred while performing web request. Status code: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<T>(content);
    }

    public Task<IEnumerable<BranchStatus>> GetBranchesAsync(string ownerName, string appName)
    {
        return this.InvokeWebRequestAsync<IEnumerable<BranchStatus>>(
            this.Client,
            new HttpRequestMessage(HttpMethod.Get, $"/v0.1/apps/{ownerName}/{appName}/branches"));
    }

    public Task<Build> StartBuildAsync(string ownerName, string appName, string branchName, string sha)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/v0.1/apps/{ownerName}/{appName}/branches/{HttpUtility.UrlEncode(branchName)}/builds");
        
        request.Content = JsonContent.Create(new {
            sourceVersion = sha,
            debug = true
        });

        return this.InvokeWebRequestAsync<Build>(
            this.Client,
            request);
    }

    public Task<Build> GetBuildAsync(string ownerName, string appName, int buildId)
    {
        return this.InvokeWebRequestAsync<Build>(
            this.Client,
            new HttpRequestMessage(HttpMethod.Get, $"/v0.1/apps/{ownerName}/{appName}/builds/{buildId}"));
    }
}
