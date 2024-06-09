namespace NesEmulator.Core.PpuModule.Registers;

public class MaskRegister
{
    public byte Value { get; set; }

    public bool GrayScale => GetFlag(Flag.GrayScale);
    public bool ShowBackgroundLeftMost8Pixels => GetFlag(Flag.ShowBackgroundLeft);
    public bool ShowSpriteLeftMost8Pixels => GetFlag(Flag.ShowSpritesLeft);
    public bool ShowBackground => GetFlag(Flag.ShowBackground);
    public bool ShowSprites => GetFlag(Flag.ShowSprites);
    public bool EmphasizeRed => GetFlag(Flag.EmphasizeRed);
    public bool EmphasizeGreen => GetFlag(Flag.EmphasizeGreen);
    public bool EmphasizeBlue => GetFlag(Flag.EmphasizeBlue);

    public static implicit operator byte(MaskRegister maskRegister) => maskRegister.Value;

    private bool GetFlag(Flag flag)
        => (Value & (byte)flag) != 0;

    private enum Flag
    {
        GrayScale = 0b0000_0001,
        ShowBackgroundLeft = 0b0000_0010,
        ShowSpritesLeft = 0b0000_0100,
        ShowBackground = 0b0000_1000,
        ShowSprites = 0b0001_0000,
        EmphasizeRed = 0b0010_0000,
        EmphasizeGreen = 0b0100_0000,
        EmphasizeBlue = 0b1000_0000
    }
}

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