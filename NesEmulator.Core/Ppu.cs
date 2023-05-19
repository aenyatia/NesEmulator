namespace NesEmulator.Core;

public class Ppu
{
	private Cartridge? _cartridge;

	private readonly uint[,] _tblName;
	private readonly uint[] _tblPalette;
	private readonly uint[,] _tblPattern;

	public Ppu()
	{
		_tblName = new uint[2, 1024];
		_tblPalette = new uint[32];
		_tblPattern = new uint[2, 4096]; // future
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

		if (_cartridge.PpuWrite(address, data))
		{
		}
	}

	public uint PpuRead(uint address, bool readOnly)
	{
		var data = 0x00U;

		address &= 0x3FFF;

		if (_cartridge.PpuRead(address, out data))
		{
		}

		return data;
	}

	public void ConnectCartridge(Cartridge cartridge)
	{
		_cartridge = cartridge;
	}

	public void Clock()
	{
	}
}