namespace NesEmulator.Core.PpuModule.Registers;

public class MaskRegister
{
    /*
     7  bit  0
     ---- ----
     BGRs bMmG
     |||| ||||
     |||| |||+- Greyscale (0: normal color, 1: produce a greyscale display)
     |||| ||+-- 1: Show background in leftmost 8 pixels of screen, 0: Hide
     |||| |+--- 1: Show sprites in leftmost 8 pixels of screen, 0: Hide
     |||| +---- 1: Show background
     |||+------ 1: Show sprites
     ||+------- Emphasize red (green on PAL/Dendy)
     |+-------- Emphasize green (red on PAL/Dendy)
     +--------- Emphasize blue
    */

    private byte _maskRegister;

    public void Update(byte maskRegister)
        => _maskRegister = maskRegister;

    public bool IsGrayScale()
    {
        return (_maskRegister & 0b0000_0001) != 0;
    }

    public bool ShowBackgroundLeftMost8Pixels()
    {
        return (_maskRegister & 0b0000_0010) != 0; 
    }
    
    public bool ShowSpriteLeftMost8Pixels()
    {
        return (_maskRegister & 0b0000_0100) != 0; 
    }
    
    public bool ShowBackground()
    {
        return (_maskRegister & 0b0000_1000) != 0; 
    }
    
    public bool ShowSprites()
    {
        return (_maskRegister & 0b0001_0000) != 0; 
    }
    
    public bool EmphasizeRed()
    {
        return (_maskRegister & 0b0010_0000) != 0; 
    }
    
    public bool EmphasizeGreen()
    {
        return (_maskRegister & 0b0100_0000) != 0; 
    }
    
    public bool EmphasizeBlue()
    {
        return (_maskRegister & 0b1000_0000) != 0; 
    }
}