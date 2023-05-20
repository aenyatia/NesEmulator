namespace NesEmulator.Core.CartridgeModule.MapperModule.Mappers;

public class Mapper000 : Mapper
{
	public Mapper000(int prgBanks, int chrBanks) : base(prgBanks, chrBanks)
	{
	}

	public override bool CpuMapRead(uint address, out uint mappedAddress)
	{
		if (address is >= 0x8000 and <= 0xFFFF)
		{
			mappedAddress = (uint)(address & (PrgBanks > 1 ? 0x7FFF : 0x3FFF));
			return true;
		}

		mappedAddress = 0;
		return false;
	}

	public override bool CpuMapWrite(uint address, out uint mappedAddress)
	{
		if (address is >= 0x8000 and <= 0xFFFF)
		{
			mappedAddress = 0;
			return true;
		}

		mappedAddress = 0;
		return false;
	}

	public override bool PpuMapRead(uint address, out uint mappedAddress)
	{
		if (address <= 0x1FFF)
		{
			mappedAddress = address;
			return true;
		}

		mappedAddress = 0;
		return false;
	}

	public override bool PpuMapWrite(uint address, out uint mappedAddress)
	{
		// if (address <= 0x1FFF)
		// {
		// 	mappedAddress = 0;
		// 	return true;
		// }

		mappedAddress = 0;
		return false;
	}
}