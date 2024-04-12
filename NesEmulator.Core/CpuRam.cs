namespace NesEmulator.Core;

public class CpuRam
{
    private readonly byte[] _memory = new byte[2048];

    public byte Read(ushort address)
    {
        // mirroring 0000 0111 1111 1111 - 0x07FF
        // 0x0000 - 0x07FF
        // 0x0800 - 0x0FFF
        // 0x1000 - 0x17FF
        // 0x1800 - 0x1FFF

        address &= 0x07FF;

        return _memory[address];
    }

    public void Write(ushort address, byte data)
    {
        // mirroring 0000 0111 1111 1111 - 0x07FF
        // 0x0000 - 0x07FF
        // 0x0800 - 0x0FFF
        // 0x1000 - 0x17FF
        // 0x1800 - 0x1FFF

        address &= 0x07FF;

        _memory[address] = data;
    }
}