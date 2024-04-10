namespace NesEmulator.Core.PpuModule.Registers;

public class AddressRegister
{
    private byte _hi;
    private byte _lo;
    private bool _hiPtr;

    public void Update(byte data)
    {
        if (_hiPtr)
        {
            _hi = data;
        }

        if (!_hiPtr)
        {
            _lo = data;
        }

        // mirror ???
        var address = Get();
        if (address > 0x3FFF)
        {
            address &= 0b0011_1111_1111_1111;
            Set((ushort)address);
        }

        _hiPtr = !_hiPtr;
    }

    public void Increment(byte value)
    {
        var lo = _lo + value;

        if (lo > 255)
        {
            _hi++;
            _lo = (byte)(_lo + value);
        }
        else
        {
            _lo += value;
        }

        // mirror ???
        var address = Get();
        if (address > 0x3FFF)
        {
            address &= 0b0011_1111_1111_1111;
            Set((ushort)address);
        }

        // mirror???
    }

    public void ResetLatch()
    {
        _hiPtr = true;
    }

    public uint Get()
        => (uint)((_hi << 8) | _lo);

    private void Set(ushort address)
    {
        _hi = (byte)(address >> 8);
        _lo = (byte)(address & 0xFF);
    }
}