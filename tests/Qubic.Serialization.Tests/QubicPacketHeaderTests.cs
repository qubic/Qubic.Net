using Qubic.Serialization;

namespace Qubic.Serialization.Tests;

public class QubicPacketHeaderTests
{
    [Fact]
    public void Create_ValidParameters_SetsCorrectValues()
    {
        var header = QubicPacketHeader.Create(27, 100);

        Assert.Equal(27, header.Type);
        Assert.Equal(108, header.PacketSize); // 100 + 8 (header size)
        Assert.Equal(100, header.PayloadSize);
    }

    [Fact]
    public void HeaderSize_IsEightBytes()
    {
        Assert.Equal(8, QubicPacketHeader.Size);
    }

    [Fact]
    public void Type_GetSet_WorksCorrectly()
    {
        var header = new QubicPacketHeader();

        header.Type = 0xFF;

        Assert.Equal(0xFF, header.Type);
    }

    [Fact]
    public void PacketSize_GetSet_WorksCorrectly()
    {
        var header = new QubicPacketHeader();

        header.PacketSize = 0x00FFFFFF; // Max 24-bit value

        Assert.Equal(0x00FFFFFF, header.PacketSize);
    }
}
