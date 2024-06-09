namespace NesEmulator.Core.PpuModule.Registers;

public class StatusRegister
{
    public byte Value { get; set; }

    public bool SpriteOverflow
    {
        get => GetFlag(Flag.SpriteOverflow);
        set => SetFlag(Flag.SpriteOverflow, value);
    }

    public bool SpriteZeroHit
    {
        get => GetFlag(Flag.SpriteZeroHit);
        set => SetFlag(Flag.SpriteZeroHit, value);
    }

    public bool VerticalBlanc
    {
        get => GetFlag(Flag.VerticalBlanc);
        set => SetFlag(Flag.VerticalBlanc, value);
    }

    public void ResetVerticalBlanc() => VerticalBlanc = false;

    public static implicit operator byte(StatusRegister statusRegister) => statusRegister.Value;

    private bool GetFlag(Flag flag)
        => (Value & (byte)flag) != 0;

    private void SetFlag(Flag flag, bool value)
    {
        if (value) Value |= (byte)flag;
        else Value &= (byte)~flag;
    }

    private enum Flag
    {
        SpriteOverflow = 0b0010_0000,
        SpriteZeroHit = 0b0100_0000,
        VerticalBlanc = 0b1000_0000
    }
}

/*
     7  bit  0
     ---- ----
     VSO. ....
     |||| ||||
     |||+-++++- PPU open bus. Returns stale PPU bus contents.
     ||+------- Sprite overflow. The intent was for this flag to be set
     ||         whenever more than eight sprites appear on a scanline, but a
     ||         hardware bug causes the actual behavior to be more complicated
     ||         and generate false positives as well as false negatives; see
     ||         PPU sprite evaluation. This flag is set during sprite
     ||         evaluation and cleared at dot 1 (the second dot) of the
     ||         pre-render line.
     |+-------- Sprite 0 Hit.  Set when a nonzero pixel of sprite 0 overlaps
     |          a nonzero background pixel; cleared at dot 1 of the pre-render
     |          line.  Used for raster timing.
     +--------- Vertical blank has started (0: not in vblank; 1: in vblank).
                Set at dot 1 of line 241 (the line *after* the post-render
                line); cleared after reading $2002 and at dot 1 of the
                pre-render line.
*/