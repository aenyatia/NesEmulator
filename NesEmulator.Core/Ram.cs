namespace NesEmulator.Core;

public class Ram
{
    private readonly byte[] _ram = new byte[2048];

    public byte CpuRead(ushort address)
    {
        if (address > 0x1FFF) throw new ArgumentOutOfRangeException(nameof(address));

        address &= 0x07FF;

        return _ram[address];
    }

    public void CpuWrite(ushort address, byte data)
    {
        if (address > 0x1FFF) throw new ArgumentOutOfRangeException(nameof(address));

        address &= 0x07FF;

        _ram[address] = data;
    }
}