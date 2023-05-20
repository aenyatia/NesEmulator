namespace NesEmulator.Core.PpuModule;

public class Ppu
{
	private readonly PpuMemory _ppuMemory;
	
	private readonly uint[,] _tblName;
	private readonly uint[] _tblPalette;
	private readonly uint[,] _tblPattern;

	public Ppu(Bus bus)
	{
		_ppuMemory = new PpuMemory(bus);
		
		_tblName = new uint[2, 1024];
		_tblPalette = new uint[32];
		_tblPattern = new uint[2, 4096]; // future
	}

	public uint Read(uint address)
	{
		return 0;
	}

	public void Write(uint address, uint data)
	{
	}
	
	public void CpuWrite(uint address, uint data)
	{
		switch (address)
		{
			case 0x0000: // control
				break;
			case 0x0001: // mask
				break;
			case 0x0002: // status
				break;
			case 0x0003: // oam address
				break;
			case 0x0004: // oam data
				break;
			case 0x0005: // scroll
				break;
			case 0x0006: // ppu address
				break;
			case 0x0007: // ppu data
				break;
		}
	}

	public uint CpuRead(uint address, bool readOnly)
	{
		var data = 0x00U;

		switch (address)
		{
			case 0x0000: // control
				break;
			case 0x0001: // mask
				break;
			case 0x0002: // status
				break;
			case 0x0003: // oam address
				break;
			case 0x0004: // oam data
				break;
			case 0x0005: // scroll
				break;
			case 0x0006: // ppu address
				break;
			case 0x0007: // ppu data
				break;
		}

		return data;
	}

	public void PpuWrite(uint address, uint data)
	{
		address &= 0x3FFF;
	}

	public uint PpuRead(uint address, bool readOnly)
	{
		var data = 0x00U;

		address &= 0x3FFF;

		return data;
	}
}