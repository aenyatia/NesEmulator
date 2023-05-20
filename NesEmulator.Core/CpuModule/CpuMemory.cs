namespace NesEmulator.Core.CpuModule;

public class CpuMemory : IMemory
{
	private readonly uint[] _ram = new uint[2048];
	private readonly Bus _bus;

	public CpuMemory(Bus bus) => _bus = bus;

	public uint Read(uint address)
	{
		return address switch
		{
			< 0x2000 => ReadFromRam(address),
			< 0x4000 => ReadFromPpu(address),
			< 0x4018 => ReadFromApu(address),
			< 0x4020 => ReadFromUnusedMemory(),
			< 0x10000 => ReadFromCartridge(address),
			_ => throw new ArgumentOutOfRangeException(nameof(address))
		};
	}

	public void Write(uint address, uint data)
	{
		switch (address)
		{
			case < 0x2000:
				WriteToRam(address, data);
				break;
			case < 0x4000:
				WriteToPpu(address, data);
				break;
			case < 0x4018:
				WriteToApu(address, data);
				break;
			case < 0x4020:
				WriteToUnusedMemory();
				break;
			case < 0x10000:
				WriteToCartridge(address, data);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(address));
		}
	}

	private uint ReadFromRam(uint address)
	{
		// todo calculate address (mirrors)

		return _ram[address];
	}

	private uint ReadFromPpu(uint address)
	{
		// todo calculate address (mirrors)

		return _bus.Ppu.Read(address);
	}

	private uint ReadFromApu(uint address)
	{
		return _bus.Controller.Read(address);
	}

	private uint ReadFromCartridge(uint address)
	{
		return _bus.Cartridge?.Read(address) ?? throw new NullReferenceException(nameof(_bus.Cartridge));
	}

	private static uint ReadFromUnusedMemory()
	{
		return 0x00;
	}


	private void WriteToRam(uint address, uint data)
	{
		// todo calculate address (mirrors)

		_ram[address] = data;
	}

	private void WriteToPpu(uint address, uint data)
	{
		// todo calculate address (mirrors)

		_bus.Ppu.Write(address, data);
	}

	private void WriteToApu(uint address, uint data)
	{
		_bus.Controller.Write(address, data);
	}

	private void WriteToCartridge(uint address, uint data)
	{
		if (_bus.Cartridge is null)
			throw new NullReferenceException(nameof(_bus.Cartridge));

		_bus.Cartridge.Write(address, data);
	}

	private static void WriteToUnusedMemory()
	{
	}
}