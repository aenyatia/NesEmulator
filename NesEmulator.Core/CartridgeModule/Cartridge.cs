namespace NesEmulator.Core.CartridgeModule;

public class Cartridge(Header header, byte[] prgRom, byte[] chrRom)
{
    public Header Header { get; } = header;
    public byte[] ChrRom { get; } = chrRom;

    public byte ReadPrgRom(ushort address) // cpu [0x8000 - 0xFFFF]
    {
        if (address < 0x8000)
            throw new Exception("address out of range");

        address -= 0x8000;

        if (prgRom.Length == 0x4000 && address >= 0x4000)
            address &= 0x3FFF;

        return prgRom[address];
    }

    public byte ReadChrRom(ushort address) // ppu [0x0000 - 0x2000]
    {
        if (address > 0x1FFF)
            throw new Exception("address out of range");

        return chrRom[address];
    }
}