using Qubic.Bob;
using Qubic.Network;
using Qubic.Rpc;

namespace Qubic.ScTester;

public enum QueryBackend { Rpc, Bob, DirectNetwork }

public class ScQueryService : IDisposable
{
    public QueryBackend ActiveBackend { get; set; } = QueryBackend.Rpc;
    public string RpcUrl { get; set; } = "https://rpc.qubic.org";
    public string BobUrl { get; set; } = "https://bob.qubic.li";
    public string NodeHost { get; set; } = "corenet.qubic.li";
    public int NodePort { get; set; } = 21841;

    private QubicRpcClient? _rpcClient;
    private BobClient? _bobClient;
    private QubicNodeClient? _nodeClient;

    public async Task<byte[]> QueryAsync(uint contractIndex, uint inputType, byte[] inputData)
    {
        return ActiveBackend switch
        {
            QueryBackend.Rpc => await QueryViaRpcAsync(contractIndex, inputType, inputData),
            QueryBackend.Bob => await QueryViaBobAsync(contractIndex, inputType, inputData),
            QueryBackend.DirectNetwork => await QueryViaDirectNetworkAsync(contractIndex, inputType, inputData),
            _ => throw new InvalidOperationException($"Unknown backend: {ActiveBackend}")
        };
    }

    private async Task<byte[]> QueryViaRpcAsync(uint contractIndex, uint inputType, byte[] inputData)
    {
        _rpcClient ??= new QubicRpcClient(RpcUrl);
        return await _rpcClient.QuerySmartContractAsync(contractIndex, inputType, inputData);
    }

    private async Task<byte[]> QueryViaBobAsync(uint contractIndex, uint inputType, byte[] inputData)
    {
        _bobClient ??= new BobClient(BobUrl);

        var hexInput = Convert.ToHexString(inputData).ToLowerInvariant();
        var result = await _bobClient.QuerySmartContractAsync((int)contractIndex, (int)inputType, hexInput);

        if (string.IsNullOrEmpty(result))
            return [];

        // Strip optional 0x prefix
        var hex = result.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? result[2..] : result;
        return Convert.FromHexString(hex);
    }

    private async Task<byte[]> QueryViaDirectNetworkAsync(uint contractIndex, uint inputType, byte[] inputData)
    {
        if (_nodeClient == null || !_nodeClient.IsConnected)
        {
            _nodeClient?.Dispose();
            _nodeClient = new QubicNodeClient(NodeHost, NodePort);
            await _nodeClient.ConnectAsync();
        }

        return await _nodeClient.QuerySmartContractAsync(contractIndex, inputType, inputData);
    }

    public void ResetClients()
    {
        _rpcClient?.Dispose();
        _rpcClient = null;
        _bobClient = null;
        _nodeClient?.Dispose();
        _nodeClient = null;
    }

    public void Dispose()
    {
        _rpcClient?.Dispose();
        _nodeClient?.Dispose();
    }
}
