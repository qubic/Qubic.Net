using System.Net;
using System.Text;

namespace Qubic.Rpc.Tests;

/// <summary>
/// A mock HTTP handler that returns preconfigured responses based on request path/method.
/// </summary>
internal sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, HttpResponseMessage> _responses = new();

    /// <summary>
    /// Registers a response for a GET request path.
    /// </summary>
    public void OnGet(string path, string jsonResponse, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responses[$"GET:{path}"] = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };
    }

    /// <summary>
    /// Registers a response for a POST request path.
    /// </summary>
    public void OnPost(string path, string jsonResponse, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responses[$"POST:{path}"] = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var key = $"{request.Method}:{request.RequestUri?.AbsolutePath}";

        if (_responses.TryGetValue(key, out var response))
            return Task.FromResult(response);

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent($"No mock registered for {key}")
        });
    }
}
