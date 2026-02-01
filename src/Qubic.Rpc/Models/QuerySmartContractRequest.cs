namespace Qubic.Rpc.Models;

internal sealed class QuerySmartContractRequest
{
    public uint ContractIndex { get; set; }
    public uint InputType { get; set; }
    public uint InputSize { get; set; }
    public string RequestData { get; set; } = string.Empty;
}
