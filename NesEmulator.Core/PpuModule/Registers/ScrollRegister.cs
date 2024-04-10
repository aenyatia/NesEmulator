namespace NesEmulator.Core.PpuModule.Registers;

public class ScrollRegister
{
    private byte _scrollX;
    private byte _scrollY;
    private bool _latch;

    public void Write(byte data)
    {
        if (!_latch)
        {
            _scrollX = data;
        }

        if (_latch)
        {
            _scrollY = data;
        }

        _latch = !_latch;
    }

    public void ResetLatch()
        => _latch = false;
}