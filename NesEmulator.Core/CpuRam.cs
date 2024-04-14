namespace NesEmulator.Core;

public class CpuRam
{
    private readonly byte[] _memory = new byte[2048];

    public byte Read(ushort address)
    {
        if (address >= 0x2000)
            throw new Exception("address out of range");

        address = MirrorRamAddress(address);

        address &= 0x07FF;

        return _memory[address];
    }

    public void Write(ushort address, byte data)
    {
        if (address >= 0x2000)
            throw new Exception("address out of range");

        address = MirrorRamAddress(address);

        _memory[address] = data;
    }

    private static ushort MirrorRamAddress(ushort address)
    {
        // mirroring 0000 0111 1111 1111 - 0x07FF
        // 0x0000 - 0x07FF
        // 0x0800 - 0x0FFF
        // 0x1000 - 0x17FF
        // 0x1800 - 0x1FFF

        return (ushort)(address & 0x07FF);
    }
}