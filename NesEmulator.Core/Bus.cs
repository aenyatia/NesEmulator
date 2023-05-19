namespace NesEmulator.Core;

public class Bus
{
	private readonly byte[] _cpuRam;

	private readonly Cpu _cpu;
	private readonly Ppu _ppu;
	private Cartridge? _cartridge;

	private uint _systemClockCounter;

	public Bus()
	{
		_cpuRam = new byte[2048];
		_cpu = new Cpu(new Memory());
		_ppu = new Ppu();
	}

	public void CpuWrite(uint address, uint data)
	{
		if (_cartridge != null && _cartridge.CpuWrite(address, data)) // not needed
		{
		}
		else if (address <= 0x1FFF)
			_cpuRam[address & 0x07FF] = (byte)data;

		else if (address is >= 0x2000 and <= 0x3FFF)
			_ppu.CpuWrite(address & 0x0007, data);
	}

	public uint CpuRead(uint address, bool readOnly)
	{
		var data = 0x00U;

		if (_cartridge != null && _cartridge.CpuRead(address, out data)) // not needed
		{
		}
		else if (address <= 0x1FFF)
			data = _cpuRam[address];

		else if (address is >= 0x2000 and <= 0x3FFF)
			_ppu.CpuRead(address & 0x0007, readOnly);

		return data;
	}

	public void InsertCartridge(Cartridge cartridge)
	{
		_cartridge = cartridge;
		_ppu.ConnectCartridge(cartridge);
	}

	public void Reset()
	{
		_cpu.Reset();
		_systemClockCounter = 0;
	}

	public void Clock()
	{
	}
}