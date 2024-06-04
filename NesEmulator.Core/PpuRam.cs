using NesEmulator.Core.CartridgeModule;

namespace NesEmulator.Core;

public class PpuRam(Mirroring mirroring)
{
    private readonly byte[] _memory = new byte[2048];

    public byte Read(ushort address)
    {
        // address = MirrorVRamAddress(address);

        return _memory[address];
    }

    public void Write(ushort address, byte data)
    {
        address = MirrorVRamAddress(address);

        _memory[address] = data;
    }

    private ushort MirrorVRamAddress(ushort address)
    {
        address &= 0b1011_1111_1111_1111;
        
        // address from 0x2000 to 0x2FFF 4kB
        address -= 0x2000; // 0x0000 - 0x0FFF

        var nameTableIndex = address / 0x400;

        // Vertical mirroring: $2000 equals $2800 and $2400 equals $2C00
        // Horizontal mirroring: $2000 equals $2400 and $2800 equals $2C00
        if (mirroring == Mirroring.Horizontal)
        {
            // inx == 0 nothing to do

            if (nameTableIndex == 1)
            {
                address -= 0x0400;
            }

            if (nameTableIndex == 2)
            {
                address -= 0x0400;
            }

            if (nameTableIndex == 3)
            {
                address -= 0x0800;
            }
        }

        if (mirroring == Mirroring.Vertical)
        {
            // inx == 0 or 1 nothing to do

            if (address == 2)
            {
                address -= 0x800;
            }

            if (address == 3)
            {
                address -= 0x800;
            }
        }

        return address;
    }
}