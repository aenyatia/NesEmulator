namespace NesEmulator.Core.CartridgeModule.MapperModule;

public abstract class Mapper
{
	protected readonly int PrgBanks;
	protected readonly int ChrBanks;

	protected Mapper(int prgBanks, int chrBanks)
	{
		PrgBanks = prgBanks;
		ChrBanks = chrBanks;
	}

	public abstract bool CpuMapRead(uint address, out uint mappedAddress);
	public abstract bool CpuMapWrite(uint address, out uint mappedAddress);
	public abstract bool PpuMapRead(uint address, out uint mappedAddress);
	public abstract bool PpuMapWrite(uint address, out uint mappedAddress);
}