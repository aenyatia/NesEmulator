namespace NesEmulator.Core.PpuModule;

public class Ppu
{
	private readonly PpuMemory _ppuMemory;

	private readonly uint[,] _tblName;
	// 32bytes
	// 0xF00 + palette * 4bytes + index
	
	// 00     01     10     11
	// 0x3F00 0x3F01 0x3F02 0x3F03
	// 0x3F04 0x3F05 0x3F06 0x3F07
	// 0x3F08 0x3F09 0x3F0A 0x3F0B
	// 0x3F0C 0x3F0D 0x3F0E 0x3F0F
	// 0x3F10 0x3F11 0x3F12 0x3F13
	// 0x3F14 0x3F15 0x3F16 0x3F17
	// 0x3F18 0x3F19 0x3F1A 0x3F1B
	// 0x3F1C 0x3F1D 0x3F1E 0x3F1F

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