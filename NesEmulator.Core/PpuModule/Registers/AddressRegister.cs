namespace NesEmulator.Core.PpuModule.Registers;

public class AddressRegister
{
    private byte _hiByte;
    private byte _loByte;
    private bool _isHiByte = true;

    public ushort Address
    {
        get => (ushort)((_hiByte << 8) | _loByte);
        private set
        {
            _hiByte = (byte)(value >> 8);
            _loByte = (byte)(value & 0xFF);
        }
    }

    public void Update(byte data)
    {
        switch (_isHiByte)
        {
            case true:
                _hiByte = data;
                break;
            case false:
                _loByte = data;
                break;
        }

        // mirror ???
        if (Address > 0x3FFF)
        {
            Address &= 0b0011_1111_1111_1111;
        }

        _isHiByte = !_isHiByte;
    }

    public void Increment(byte value)
    {
        var lo = _loByte + value;

        if (lo > 0xFF)
        {
            _hiByte++;
            _loByte = (byte)(_loByte + value);
        }
        else
        {
            _loByte += value;
        }

        // mirror ???
        if (Address > 0x3FFF)
        {
            Address &= 0b0011_1111_1111_1111;
        }
    }

    public void ResetLatch()
    {
        _isHiByte = true;
    }
}