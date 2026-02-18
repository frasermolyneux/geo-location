using System.Net;

using RestSharp;

using MX.Api.Client;

namespace MX.GeoLocation.Api.Client.IntegrationTests;

/// <summary>
/// Bridges RestSharp-based API client to WebApplicationFactory's TestServer HttpClient.
/// Translates RestRequest -> HttpRequestMessage, executes via HttpClient, maps back to RestResponse.
/// </summary>
public class TestServerRestClientService : IRestClientService
{
    private readonly HttpClient _httpClient;

    public TestServerRestClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RestResponse> ExecuteAsync(string baseUrl, RestRequest request, CancellationToken cancellationToken = default)
    {
        var httpRequest = BuildHttpRequest(baseUrl, request);
        var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        return await BuildRestResponse(httpResponse);
    }

    public async Task<RestResponse> ExecuteWithNamedOptionsAsync(string optionsName, RestRequest request, CancellationToken cancellationToken = default)
    {
        // For integration tests, named options aren't needed — delegate to ExecuteAsync with empty base
        return await ExecuteAsync(string.Empty, request, cancellationToken);
    }

    private HttpRequestMessage BuildHttpRequest(string baseUrl, RestRequest request)
    {
        var uri = BuildUri(baseUrl, request);

        var method = request.Method switch
        {
            Method.Get => HttpMethod.Get,
            Method.Post => HttpMethod.Post,
            Method.Put => HttpMethod.Put,
            Method.Delete => HttpMethod.Delete,
            Method.Patch => HttpMethod.Patch,
            Method.Head => HttpMethod.Head,
            Method.Options => HttpMethod.Options,
            _ => HttpMethod.Get
        };

        var httpRequest = new HttpRequestMessage(method, uri);

        // Copy headers
        foreach (var header in request.Parameters.Where(p => p.Type == ParameterType.HttpHeader))
        {
            httpRequest.Headers.TryAddWithoutValidation(header.Name!, header.Value?.ToString() ?? string.Empty);
        }

        // Copy body - handle both AddJsonBody and AddBody parameter styles
        var bodyParam = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
        if (bodyParam != null)
        {
            var bodyValue = bodyParam.Value;
            string bodyString;

            if (bodyValue is string s)
                bodyString = s;
            else
                bodyString = Newtonsoft.Json.JsonConvert.SerializeObject(bodyValue);

            httpRequest.Content = new StringContent(
                bodyString,
                System.Text.Encoding.UTF8,
                bodyParam.ContentType ?? "application/json");
        }

        return httpRequest;
    }

    private string BuildUri(string baseUrl, RestRequest request)
    {
        var resource = request.Resource ?? string.Empty;

        // Apply URL segment parameters
        foreach (var param in request.Parameters.Where(p => p.Type == ParameterType.UrlSegment))
        {
            resource = resource.Replace($"{{{param.Name}}}", Uri.EscapeDataString(param.Value?.ToString() ?? string.Empty));
        }

        // Build query string
        var queryParams = request.Parameters.Where(p => p.Type == ParameterType.QueryString).ToList();
        if (queryParams.Count > 0)
        {
            var queryString = string.Join("&", queryParams.Select(p =>
                $"{Uri.EscapeDataString(p.Name!)}={Uri.EscapeDataString(p.Value?.ToString() ?? string.Empty)}"));
            resource = resource.Contains('?') ? $"{resource}&{queryString}" : $"{resource}?{queryString}";
        }

        if (!string.IsNullOrEmpty(baseUrl))
        {
            // Strip the base URL scheme/host since TestServer HttpClient handles routing
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
            {
                return $"/{resource.TrimStart('/')}";
            }
            return $"/{baseUrl.TrimStart('/')}/{resource.TrimStart('/')}".TrimEnd('/');
        }

        return $"/{resource.TrimStart('/')}";
    }

    private static async Task<RestResponse> BuildRestResponse(HttpResponseMessage httpResponse)
    {
        var content = await httpResponse.Content.ReadAsStringAsync();

        return new RestResponse
        {
            StatusCode = httpResponse.StatusCode,
            Content = content,
            IsSuccessStatusCode = httpResponse.IsSuccessStatusCode,
            ResponseStatus = ResponseStatus.Completed
        };
    }

    public void Dispose()
    {
        // HttpClient lifecycle is managed by the test fixture
    }
}
