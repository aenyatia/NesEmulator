namespace NesEmulator.Core.PpuModule;

public class StatusRegister
{
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

    private byte _statusRegister;

    public byte Get()
        => _statusRegister;

    // first 5bits unused

    public void SetSpriteOverflow(bool value)
    {
        if (value) _statusRegister |= 0b0010_0000;
        else _statusRegister &= 0b1101_1111;
    }

    public void SpriteZeroHit(bool value)
    {
        if (value) _statusRegister |= 0b0100_0000;
        else _statusRegister &= 0b1011_1111;
    }

    public void VerticalVBlanc(bool value)
    {
        if (value) _statusRegister |= 0b1000_0000;
        else _statusRegister &= 0b0111_1111;
    }

    public void ResetVerticalVBlanc()
    {
        VerticalVBlanc(false);
    }

    public bool IsInVBlanc()
    {
        return (_statusRegister & 0b1000_0000) != 0;
    }
}