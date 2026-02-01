using System.Buffers.Binary;
using Qubic.Core.Entities;
using Qubic.Serialization;
using Qubic.Serialization.Readers;

namespace Qubic.Serialization.Tests;

public class QubicPacketReaderTests
{
    private readonly QubicPacketReader _reader = new();

    #region ReadHeader Tests

    [Fact]
    public void ReadHeader_ValidData_ReturnsCorrectHeader()
    {
        // Create a valid header: type=28, size=24 (header + 16 bytes payload)
        var data = new byte[8];
        uint sizeAndType = 24 | (28u << 24); // Size in lower 24 bits, type in upper 8 bits
        BinaryPrimitives.WriteUInt32LittleEndian(data, sizeAndType);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4), 0x12345678); // Dejavu

        var header = _reader.ReadHeader(data);

        Assert.Equal(28, header.Type);
        Assert.Equal(24, header.PacketSize);
        Assert.Equal(16, header.PayloadSize);
        Assert.Equal(0x12345678u, header.Dejavu);
    }

    [Fact]
    public void ReadHeader_DataTooShort_ThrowsException()
    {
        var data = new byte[4]; // Only 4 bytes, need 8

        Assert.Throws<ArgumentException>(() => _reader.ReadHeader(data));
    }

    [Fact]
    public void ReadHeader_MaxValues_HandlesCorrectly()
    {
        var data = new byte[8];
        uint sizeAndType = 0x00FFFFFF | (0xFFu << 24); // Max size (16MB-1), max type (255)
        BinaryPrimitives.WriteUInt32LittleEndian(data, sizeAndType);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4), 0xFFFFFFFF);

        var header = _reader.ReadHeader(data);

        Assert.Equal(255, header.Type);
        Assert.Equal(0x00FFFFFF, header.PacketSize);
    }

    #endregion

    #region ReadCurrentTickInfo Tests

    [Fact]
    public void ReadCurrentTickInfo_ValidPayload_ReturnsCorrectInfo()
    {
        var payload = new byte[16];
        var offset = 0;

        BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(offset), 1000); // TickDuration
        offset += 2;
        BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(offset), 150); // Epoch
        offset += 2;
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(offset), 12345678); // Tick
        offset += 4;
        BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(offset), 450); // NumberOfAlignedVotes
        offset += 2;
        BinaryPrimitives.WriteUInt16LittleEndian(payload.AsSpan(offset), 5); // NumberOfMisalignedVotes
        offset += 2;
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(offset), 10000000); // InitialTick

        var tickInfo = _reader.ReadCurrentTickInfo(payload);

        Assert.Equal(1000, tickInfo.TickDuration);
        Assert.Equal(150, tickInfo.Epoch);
        Assert.Equal(12345678u, tickInfo.Tick);
        Assert.Equal(450, tickInfo.NumberOfAlignedVotes);
        Assert.Equal(5, tickInfo.NumberOfMisalignedVotes);
        Assert.Equal(10000000u, tickInfo.InitialTick);
    }

    [Fact]
    public void ReadCurrentTickInfo_PayloadTooShort_ThrowsException()
    {
        var payload = new byte[10]; // Only 10 bytes, need 16

        Assert.Throws<ArgumentException>(() => _reader.ReadCurrentTickInfo(payload));
    }

    [Fact]
    public void ReadCurrentTickInfo_ZeroValues_HandlesCorrectly()
    {
        var payload = new byte[16]; // All zeros

        var tickInfo = _reader.ReadCurrentTickInfo(payload);

        Assert.Equal(0, tickInfo.TickDuration);
        Assert.Equal(0, tickInfo.Epoch);
        Assert.Equal(0u, tickInfo.Tick);
        Assert.Equal(0, tickInfo.NumberOfAlignedVotes);
        Assert.Equal(0, tickInfo.NumberOfMisalignedVotes);
        Assert.Equal(0u, tickInfo.InitialTick);
    }

    #endregion

    #region ReadEntityResponse Tests

    [Fact]
    public void ReadEntityResponse_ValidPayload_ReturnsCorrectBalance()
    {
        var identity = QubicIdentity.FromIdentity("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARMID");
        var payload = new byte[56];
        var offset = 0;

        // Public key echo (32 bytes) - skip
        Array.Copy(identity.PublicKey, 0, payload, offset, 32);
        offset += 32;

        BinaryPrimitives.WriteInt64LittleEndian(payload.AsSpan(offset), 1000000000); // IncomingAmount
        offset += 8;
        BinaryPrimitives.WriteInt64LittleEndian(payload.AsSpan(offset), 500000000); // OutgoingAmount
        offset += 8;
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(offset), 100); // NumberOfIncomingTransfers
        offset += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(offset), 50); // NumberOfOutgoingTransfers

        var balance = _reader.ReadEntityResponse(payload, identity);

        Assert.Equal(identity.ToString(), balance.Identity.ToString());
        Assert.Equal(500000000, balance.Amount); // Incoming - Outgoing
        Assert.Equal(100u, balance.IncomingCount);
        Assert.Equal(50u, balance.OutgoingCount);
    }

    [Fact]
    public void ReadEntityResponse_ZeroBalance_ReturnsCorrectBalance()
    {
        var identity = QubicIdentity.FromIdentity("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARMID");
        var payload = new byte[56];

        // Public key echo
        Array.Copy(identity.PublicKey, 0, payload, 0, 32);
        // All amounts are zero

        var balance = _reader.ReadEntityResponse(payload, identity);

        Assert.Equal(0, balance.Amount);
        Assert.Equal(0u, balance.IncomingCount);
        Assert.Equal(0u, balance.OutgoingCount);
    }

    [Fact]
    public void ReadEntityResponse_NegativeBalance_CalculatesCorrectly()
    {
        var identity = QubicIdentity.FromIdentity("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARMID");
        var payload = new byte[56];
        var offset = 32;

        BinaryPrimitives.WriteInt64LittleEndian(payload.AsSpan(offset), 100); // IncomingAmount
        offset += 8;
        BinaryPrimitives.WriteInt64LittleEndian(payload.AsSpan(offset), 500); // OutgoingAmount (more than incoming)

        var balance = _reader.ReadEntityResponse(payload, identity);

        Assert.Equal(-400, balance.Amount); // Can be negative in theory
    }

    [Fact]
    public void ReadEntityResponse_PayloadTooShort_ThrowsException()
    {
        var identity = QubicIdentity.FromIdentity("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARMID");
        var payload = new byte[50]; // Only 50 bytes, need 56

        Assert.Throws<ArgumentException>(() => _reader.ReadEntityResponse(payload, identity));
    }

    #endregion

    #region ReadFullEntityResponse Tests

    [Fact]
    public void ReadFullEntityResponse_WithoutSiblings_ReturnsCorrectResponse()
    {
        var payload = new byte[72]; // 64 (EntityRecord) + 4 (tick) + 4 (spectrumIndex)
        var offset = 0;

        // Public key (32 bytes)
        var publicKey = new byte[32];
        new Random(42).NextBytes(publicKey);
        Array.Copy(publicKey, 0, payload, offset, 32);
        offset += 32;

        BinaryPrimitives.WriteInt64LittleEndian(payload.AsSpan(offset), 2000000000); // IncomingAmount
        offset += 8;
        BinaryPrimitives.WriteInt64LittleEndian(payload.AsSpan(offset), 1000000000); // OutgoingAmount
        offset += 8;
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(offset), 200); // NumberOfIncomingTransfers
        offset += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(offset), 100); // NumberOfOutgoingTransfers
        offset += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(offset), 12340000); // LatestIncomingTransferTick
        offset += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(offset), 12345000); // LatestOutgoingTransferTick
        offset += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(offset), 12345678); // Tick
        offset += 4;
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(offset), 12345); // SpectrumIndex

        var response = _reader.ReadFullEntityResponse(payload);

        Assert.Equal(publicKey, response.Entity.PublicKey);
        Assert.Equal(2000000000, response.Entity.IncomingAmount);
        Assert.Equal(1000000000, response.Entity.OutgoingAmount);
        Assert.Equal(1000000000, response.Entity.Balance);
        Assert.Equal(200u, response.Entity.NumberOfIncomingTransfers);
        Assert.Equal(100u, response.Entity.NumberOfOutgoingTransfers);
        Assert.Equal(12340000u, response.Entity.LatestIncomingTransferTick);
        Assert.Equal(12345000u, response.Entity.LatestOutgoingTransferTick);
        Assert.Equal(12345678u, response.Tick);
        Assert.Equal(12345, response.SpectrumIndex);
        Assert.Null(response.Siblings);
    }

    [Fact]
    public void ReadFullEntityResponse_WithSiblings_ReturnsCorrectResponse()
    {
        var payload = new byte[72 + 24 * 32]; // Base + 24 siblings
        var offset = 0;

        // Entity record (simplified)
        offset += 64;
        BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(offset), 12345678); // Tick
        offset += 4;
        BinaryPrimitives.WriteInt32LittleEndian(payload.AsSpan(offset), 100); // SpectrumIndex
        offset += 4;

        // Write 24 siblings
        var siblings = new byte[24][];
        for (int i = 0; i < 24; i++)
        {
            siblings[i] = new byte[32];
            for (int j = 0; j < 32; j++)
            {
                siblings[i][j] = (byte)(i + j);
            }
            Array.Copy(siblings[i], 0, payload, offset, 32);
            offset += 32;
        }

        var response = _reader.ReadFullEntityResponse(payload);

        Assert.NotNull(response.Siblings);
        Assert.Equal(24, response.Siblings.Length);
        for (int i = 0; i < 24; i++)
        {
            Assert.Equal(siblings[i], response.Siblings[i]);
        }
    }

    [Fact]
    public void ReadFullEntityResponse_PayloadTooShort_ThrowsException()
    {
        var payload = new byte[60]; // Too short

        Assert.Throws<ArgumentException>(() => _reader.ReadFullEntityResponse(payload));
    }

    #endregion

    #region Roundtrip Tests

    [Fact]
    public void Header_RoundTrip_PreservesValues()
    {
        // Create a header using the standard method
        var original = QubicPacketHeader.Create(QubicPacketTypes.RespondCurrentTickInfo, 16);

        // Serialize it
        var bytes = new byte[8];
        uint sizeAndType = (uint)original.PacketSize | ((uint)original.Type << 24);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, sizeAndType);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), original.Dejavu);

        // Read it back
        var parsed = _reader.ReadHeader(bytes);

        Assert.Equal(original.Type, parsed.Type);
        Assert.Equal(original.PacketSize, parsed.PacketSize);
        Assert.Equal(original.PayloadSize, parsed.PayloadSize);
        Assert.Equal(original.Dejavu, parsed.Dejavu);
    }

    #endregion
}
