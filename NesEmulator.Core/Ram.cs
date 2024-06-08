namespace NesEmulator.Core;

public class Ram
{
    private readonly byte[] _ram = new byte[2048];

    public byte Read(ushort address)
    {
        if (address > 0x1FFF) throw new Exception("ram address out of range");

        address = MirrorRamAddress(address);

        return _ram[address];
    }

    public void Write(ushort address, byte data)
    {
        if (address > 0x1FFF) throw new Exception("ram address out of range");

        address = MirrorRamAddress(address);

        _ram[address] = data;
    }

    private static ushort MirrorRamAddress(ushort address)
    {
        // ram    0x0000 - 0x07FF
        // mirror 0x0800 - 0x0FFF
        // mirror 0x1000 - 0x17FF
        // mirror 0x1800 - 0x1FFF

        return (ushort)(address & 0x07FF);
    }
}