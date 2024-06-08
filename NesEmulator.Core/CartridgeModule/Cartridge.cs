using NesEmulator.Core.CartridgeModule.Mappers;

namespace NesEmulator.Core.CartridgeModule;

public class Cartridge(Header header, IMapper mapper, byte[] prgRom, byte[] chrRom)
{
    public Header Header { get; } = header;
    public byte[] ChrRom { get; } = chrRom;
    public Mirroring Mirroring => Header.Mirroring;

    public byte ReadPrgRom(ushort address)
    {
        address = mapper.MapPrgRom(address);

        return prgRom[address];
    }

    public byte ReadChrRom(ushort address)
    {
        address = mapper.MapChrRom(address);

        return ChrRom[address];
    }
}