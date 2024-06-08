namespace NesEmulator.Core.ControllerModule;

public class Controller
{
    private readonly bool[] _buttonsState = new bool[8];
    private bool _strobe;

    private int _currentButtonIndex;

    public void SetButtonState(ControllerButton button, bool state)
    {
        _buttonsState[(int)button] = state;
    }

    public void Write(byte input)
    {
        _strobe = (input & 1) == 1;
        if (_strobe)
        {
            _currentButtonIndex = 0;
        }
    }

    public byte Read()
    {
        if (_currentButtonIndex > 7) return 1;

        var state = _buttonsState[_currentButtonIndex];
        if (!_strobe)
        {
            _currentButtonIndex++;
        }

        return (byte)(state ? 1 : 0);
    }
}