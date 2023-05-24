namespace NesEmulator.Core.PpuModule;

public class PpuMemory : IMemory
{
	// nametable
	private readonly uint[] _vRam = new uint[2048];
	
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
	private readonly uint[] _palette = new uint[32];
	private readonly Nes _nes;

	public PpuMemory(Nes nes) => _nes = nes;

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
		return _nes.Cartridge?.ReadChr((ushort)address) ?? throw new NullReferenceException(nameof(_nes.Cartridge));
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
		if (_nes.Cartridge is null)
			throw new NullReferenceException(nameof(_nes.Cartridge));

		_nes.Cartridge.WriteChr((ushort)address, (byte)data);
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