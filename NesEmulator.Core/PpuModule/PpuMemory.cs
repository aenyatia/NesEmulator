namespace NesEmulator.Core.PpuModule;

public class PpuMemory : IMemory
{
	private readonly uint[] _vRam = new uint[2048];
	private readonly uint[] _palette = new uint[32];
	private readonly Bus _bus;

	public PpuMemory(Bus bus) => _bus = bus;

	public uint Read(uint address)
	{
		return address switch
		{
			< 0x2000 => ReadFromCartridge(address),
			< 0x3F00 => ReadFromRam(address),
			< 0x4000 => ReadFromPalette(address),
			_ => throw new ArgumentOutOfRangeException(nameof(address))
		};
	}

	public void Write(uint address, uint data)
	{
		switch (address)
		{
			case < 0x2000:
				WriteToCartridge(address, data);
				break;
			case < 0x3F00:
				WriteToRam(address, data);
				break;
			case < 0x4000:
				WriteToPalette(address, data);
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(address));
		}
	}

	private uint ReadFromCartridge(uint address)
	{
		return _bus.Cartridge?.Read(address) ?? throw new NullReferenceException(nameof(_bus.Cartridge));
	}

	private uint ReadFromRam(uint address)
	{
		// todo calculate address (mirrors)

		return _vRam[address];
	}

	private uint ReadFromPalette(uint address)
	{
		// todo calculate address (mirrors)

		return _palette[address];
	}


	private void WriteToCartridge(uint address, uint data)
	{
		if (_bus.Cartridge is null)
			throw new NullReferenceException(nameof(_bus.Cartridge));

		_bus.Cartridge.Write(address, data);
	}

	private void WriteToRam(uint address, uint data)
	{
		// todo calculate address (mirrors)

		_vRam[address] = data;
	}

	private void WriteToPalette(uint address, uint data)
	{
		// todo calculate address (mirrors)

		_palette[address] = data;
	}
}