namespace NesEmulator.Core;

public abstract class Mapper
{
	protected readonly uint PrgBanks;
	protected readonly uint ChrBanks;

	protected Mapper(uint prgBanks, uint chrBanks)
	{
		PrgBanks = prgBanks;
		ChrBanks = chrBanks;
	}

	public abstract bool CpuMapRead(uint address, out uint mappedAddress);
	public abstract bool CpuMapWrite(uint address, out uint mappedAddress);
	public abstract bool PpuMapRead(uint address, out uint mappedAddress);
	public abstract bool PpuMapWrite(uint address, out uint mappedAddress);
}