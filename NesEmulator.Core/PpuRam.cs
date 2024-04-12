namespace NesEmulator.Core;

public class PpuRam
{
    private readonly byte[] _memory = new byte[2048];

    public byte Read(ushort address)
    {
        return _memory[address];
    }

    public void Write(ushort address, byte data)
    {
        _memory[address] = data;
    }
}