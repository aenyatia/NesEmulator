namespace NesEmulator.Core.PpuModule.Registers;

public class ControlRegister
{
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

    private byte _controlRegister;

    public void Update(byte controlRegister)
        => _controlRegister = controlRegister;

    public ushort NameTableAddress()
    {
        var value = _controlRegister & 0b0000_0011;

        return value switch
        {
            0 => 0x2000,
            1 => 0x2400,
            2 => 0x2800,
            3 => 0x2C00,
            _ => throw new Exception("unknown name table")
        };
    }

    public byte VRamAddressIncrement()
    {
        var value = _controlRegister & 0b0000_0100;

        return (byte)(value == 0
            ? 1 // going across 
            : 32); // going down 
    }

    public uint SpritePatternTableAddress()
    {
        var value = _controlRegister & 0b0000_1000;

        return value == 0
            ? 0x0000u
            : 0x1000u;
    }

    public uint BackgroundPatternTableAddress()
    {
        var value = _controlRegister & 0b0001_0000;

        return value == 0
            ? 0x0000u
            : 0x1000u;
    }

    public uint SpriteSize()
    {
        var value = _controlRegister & 0b0010_0000;

        return value == 0
            ? 8u // 8x8 pixels 
            : 16u; // 8x16 pixels
    }

    public uint PpuMasterSlaveSelect()
    {
        var value = _controlRegister & 0b0100_0000;

        return value == 0
            ? 0u
            : 1u;
    }

    public bool GenerateVBlancNmi()
    {
        var value = _controlRegister & 0b1000_0000;

        return value == 0
            ? false // off
            : true; // on
    }
}