namespace NesEmulator.Core.CartridgeModule.Mappers;

public class NRomMapper(uint chrRomBanks, uint prgRomBanks) : IMapper
{
    public ushort MapChrRom(ushort address)
    {
        return address;
    }

    public ushort MapPrgRom(ushort address)
    {
        return prgRomBanks switch
        {
            1 => (ushort)(address & 0b0011_1111_1111_1111),
            2 => (ushort)(address & 0b0111_1111_1111_1111),
            _ => throw new Exception("invalid prg rom banks count")
        };
    }

    public bool CpuMapReadPrgRom(ushort address, ref ushort mappedAddress)
    {
        if (address is >= 0x8000 and <= 0xFFFF)
        {
            mappedAddress = (ushort)(address & (prgRomBanks > 1 ? 0x7FFF : 0x3FFF));
            return true;
        }

        return false;
    }

    public bool CpuMapWritePrgRom(ushort address, ref ushort mappedAddress)
    {
        if (address is >= 0x8000 and <= 0xFFFF)
        {
            mappedAddress = (ushort)(address & (prgRomBanks > 1 ? 0x7FFF : 0x3FFF));
            return true;
        }

        return false;
    }

    public bool PpuMapReadChrRom(ushort address, ref ushort mappedAddress)
    {
        if (address is >= 0x0000 and <= 0x1FFF)
        {
            mappedAddress = address;
            return true;
        }

        return false;
    }

    public bool PpuMapWriteChrRom(ushort address, ref ushort mappedAddress)
    {
        if (address is >= 0x0000 and <= 0x1FFF)
        {
            if (chrRomBanks == 0)
            {
                mappedAddress = address;
                return true;
            }
        }

        return false;
    }
}