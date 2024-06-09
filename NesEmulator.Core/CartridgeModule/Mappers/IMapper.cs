namespace NesEmulator.Core.CartridgeModule.Mappers;

public interface IMapper
{
    bool CpuMapReadPrgRom(ushort address, ref ushort mappedAddress);
    bool CpuMapWritePrgRom(ushort address, ref ushort mappedAddress);
    bool PpuMapReadChrRom(ushort address, ref ushort mappedAddress);
    bool PpuMapWriteChrRom(ushort address, ref ushort mappedAddress);
}