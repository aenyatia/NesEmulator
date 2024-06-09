namespace NesEmulator.Core.PpuModule.Registers;

public class ControlRegister
{
    public byte Value { get; set; }

    public bool NametableX
    {
        get => GetFlag(Flag.NametableX);
        set => SetFlag(Flag.NametableX, value);
    }

    public bool NametableY
    {
        get => GetFlag(Flag.NametableY);
        set => SetFlag(Flag.NametableY, value);
    }

    public bool IncrementMode
    {
        get => GetFlag(Flag.IncrementMode);
        set => SetFlag(Flag.IncrementMode, value);
    }

    public bool PatternSprite
    {
        get => GetFlag(Flag.PatternSprite);
        set => SetFlag(Flag.PatternSprite, value);
    }

    public bool PatternBackground
    {
        get => GetFlag(Flag.PatternBackground);
        set => SetFlag(Flag.PatternBackground, value);
    }

    public bool SpriteSize
    {
        get => GetFlag(Flag.SpriteSize);
        set => SetFlag(Flag.SpriteSize, value);
    }

    public bool SlaveMode
    {
        get => GetFlag(Flag.SlaveMode);
        set => SetFlag(Flag.SlaveMode, value);
    }

    public bool EnableNmi
    {
        get => GetFlag(Flag.EnableNmi);
        set => SetFlag(Flag.EnableNmi, value);
    }

    public static implicit operator byte(ControlRegister controlRegister) => controlRegister.Value;

    private bool GetFlag(Flag flag)
        => (Value & (byte)flag) != 0;

    private void SetFlag(Flag flag, bool value)
    {
        if (value) Value |= (byte)flag;
        else Value &= (byte)~flag;
    }

    private enum Flag
    {
        NametableX = 0b0000_0001,
        NametableY = 0b0000_0010,
        IncrementMode = 0b0000_0100,
        PatternSprite = 0b0000_1000,
        PatternBackground = 0b0001_0000,
        SpriteSize = 0b0010_0000,
        SlaveMode = 0b0100_0000,
        EnableNmi = 0b1000_0000
    }
}

/* control register
    7  bit  0
    ---- ----
    VPHB SINN
    |||| ||||
    |||| ||++- Base nametable address
    |||| ||    (0 = $2000; 1 = $2400; 2 = $2800; 3 = $2C00)
    |||| |+--- VRAM address increment per CPU read/write of PPUDATA
    |||| |     (0: add 1, going across; 1: add 32, going down)
    |||| +---- Sprite pattern table address for 8x8 sprites
    ||||       (0: $0000; 1: $1000; ignored in 8x16 mode)
    |||+------ Background pattern table address (0: $0000; 1: $1000)
    ||+------- Sprite size (0: 8x8 pixels; 1: 8x16 pixels – see PPU OAM#Byte 1)
    |+-------- PPU master/slave select
    |          (0: read backdrop from EXT pins; 1: output color on EXT pins)
    +--------- Generate an NMI at the start of the
               vertical blanking interval (0: off; 1: on)
*/