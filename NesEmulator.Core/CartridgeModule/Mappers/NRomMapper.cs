namespace NesEmulator.Core.CartridgeModule.Mappers;

public interface IMapper
{
    ushort MapChrRom(ushort address);
    ushort MapPrgRom(ushort address);
}

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
}