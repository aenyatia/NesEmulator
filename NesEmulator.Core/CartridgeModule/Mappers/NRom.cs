namespace NesEmulator.Core.CartridgeModule.Mappers;

public interface IMapper
{
	ushort MapReadAddress(ushort address);
	ushort Write(ushort address);
}

public class NRom : IMapper
{
	private readonly byte _prgBanks;

	public NRom(byte prgBanks) => _prgBanks = prgBanks;

	public ushort MapReadAddress(ushort address)
	{
		if (address < 0x2000) // chr
			return address;

		if (address >= 0x8000) // prg
			return (ushort)(address & (_prgBanks > 1 ? 0x7FFF : 0x3FFF));

		throw new Exception("invalid_mapper_address");
	}

	public ushort Write(ushort address)
	{
		if (address < 0x2000) // chr
			return address;

		throw new Exception("invalid_mapper_address");
	}
}