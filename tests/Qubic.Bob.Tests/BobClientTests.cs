using Qubic.Bob;

namespace Qubic.Bob.Tests;

public class BobClientTests
{
    [Fact]
    public void Constructor_WithBaseUrl_CreatesInstance()
    {
        using var client = new BobClient("http://localhost:40420");

        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_WithHttpClient_CreatesInstance()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:40420") };

        using var client = new BobClient(httpClient);

        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_NullHttpClient_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new BobClient((HttpClient)null!));
    }

    [Fact]
    public void Constructor_CustomRpcPath_UsesPath()
    {
        using var client = new BobClient("http://localhost:40420", "/custom/rpc");

        Assert.NotNull(client);
    }
}
