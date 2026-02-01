namespace Qubic.Bob;

/// <summary>
/// Exception thrown when a Bob JSON-RPC call fails.
/// </summary>
public sealed class BobRpcException : Exception
{
    public int ErrorCode { get; }

    public BobRpcException(int errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}
