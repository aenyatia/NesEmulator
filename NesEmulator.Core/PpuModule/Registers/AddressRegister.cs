using System.Runtime.CompilerServices;

namespace NesEmulator.Core.PpuModule.Registers;

public class AddressRegister
{
    private bool _isHiByte = true;

    private byte _hiByte;
    private byte _loByte;

    public ushort Value
    {
        get => (ushort)((_hiByte << 8) | _loByte);
        set
        {
            _hiByte = (byte)(value >> 8);
            _loByte = (byte)(value & 0x00FF);
        }
    }

    public static implicit operator ushort(AddressRegister addressRegister) => addressRegister.Value;

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

        _isHiByte = !_isHiByte;
    }

    public void Increment(bool incrementMode)
    {
        var value = (byte)(incrementMode ? 32 : 1);

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
    }


    public void ResetLatch() => _isHiByte = true;
}