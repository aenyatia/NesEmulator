using NesEmulator.Core.CartridgeModule.Mappers;

namespace NesEmulator.Core.CartridgeModule;

public class Cartridge(Header header, IMapper mapper, byte[] prgRom, byte[] chrRom)
{
    private Header Header { get; } = header;
    public Mirroring Mirroring => Header.Mirroring;

    public bool CpuRead(ushort address, ref byte data)
    {
        ushort mappedAddress = 0x0000;

        if (mapper.CpuMapReadPrgRom(address, ref mappedAddress))
        {
            data = prgRom[mappedAddress];
            return true;
        }

        return false;
    }

    public bool CpuWrite(ushort address, byte data)
    {
        ushort mappedAddress = 0x0000;

        if (mapper.CpuMapWritePrgRom(address, ref mappedAddress))
        {
            prgRom[mappedAddress] = data;
            return true;
        }

        return false;
    }

    public bool PpuRead(ushort address, ref byte data)
    {
        ushort mappedAddress = 0x0000;

        if (mapper.PpuMapReadChrRom(address, ref mappedAddress))
        {
            data = chrRom[mappedAddress];
            return true;
        }

        return false;
    }

    public bool PpuWrite(ushort address, byte data)
    {
        ushort mappedAddress = 0x0000;

        if (mapper.CpuMapWritePrgRom(address, ref mappedAddress))
        {
            chrRom[mappedAddress] = data;
            return true;
        }

        return false;
    }

    public void Reset()
    {
    }
}