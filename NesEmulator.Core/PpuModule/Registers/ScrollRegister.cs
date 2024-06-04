namespace NesEmulator.Core.PpuModule.Registers;

public class ScrollRegister
{
    private byte _scrollX;
    private byte _scrollY;
    private bool _latch;

    public void Write(byte data)
    {
        switch (_latch)
        {
            case false:
                _scrollX = data;
                break;
            case true:
                _scrollY = data;
                break;
        }

        _latch = !_latch;
    }

    public void ResetLatch()
        => _latch = false;
}