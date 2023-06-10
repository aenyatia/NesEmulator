namespace NesEmulator.Core.PpuModule;

public class PpuMemory : IMemory
{
	// nametable
	private readonly uint[] _vRam = new uint[2048];
	
	// 32bytes
	// 0x3F00 + palette * 4bytes + index
	
	// bg     col1   col2   col3
	// 00     01     10     11
	// 0x3F00 0x3F01 0x3F02 0x3F03
	// 0x3F04 0x3F05 0x3F06 0x3F07
	// 0x3F08 0x3F09 0x3F0A 0x3F0B
	// 0x3F0C 0x3F0D 0x3F0E 0x3F0F
	// 0x3F10 0x3F11 0x3F12 0x3F13
	// 0x3F14 0x3F15 0x3F16 0x3F17
	// 0x3F18 0x3F19 0x3F1A 0x3F1B
	// 0x3F1C 0x3F1D 0x3F1E 0x3F1F
	private readonly uint[] _palette = {
		0x00, 0x01, 0x02, 0x03, 
		0x04, 0x05, 0x06, 0x07, 
		0x08, 0x09, 0x0A, 0x0B,
		0x0C, 0x0D, 0x0E, 0x0F,
		0x00, 0x01, 0x02, 0x03, 
		0x04, 0x05, 0x06, 0x07, 
		0x08, 0x09, 0x0A, 0x0B,
		0x0C, 0x0D, 0x0E, 0x0F
	};
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
		// Address in VRAM indexed with 0 at 0x2000
		address &= 0x0FFF;

		if (_nes.Cartridge is { Header.VerticalVRamMirroring: true })
		{
			switch (address)
			{
				// Vertical
				case < 0x0800:
					return _vRam[address];
				case < 0x1000:
					return _vRam[address - 0x0800];
			}
		}
		else
		{
			// Horizontal
			switch (address)
			{
				case < 0x0400:
					return _vRam[address];
				case < 0x0800:
					return _vRam[address - 0x0400];
				case < 0x0C00:
					return _vRam[address - 0x0400];
				case < 0x1000:
					return _vRam[address - 0x0800];
			}
		}

		throw new InvalidOperationException();
	}

	private uint ReadFromPalette(uint address)
	{
		// 0x3F00 -> 0x3F1F
		// 0x3F20 -> 0x3F3F
		// 0x3F40 -> 0x3F5F
		// ...

		var offset = (address - 0x3F00) % 32;
		
		// Mirror $3F10, $3F14, $3F18, $3F1C to $3F00, $3F04, $3F08 $3F0C
		if (offset >= 16 && (offset - 16) % 4 == 0) 
			return _palette[offset - 16];
		
		return _palette[offset];
	}
	
	private void WriteToCartridge(uint address, uint data)
	{
		if (_nes.Cartridge is null)
			throw new NullReferenceException(nameof(_nes.Cartridge));

		_nes.Cartridge.WriteChr((ushort)address, (byte)data);
	}

	private void WriteToRam(uint address, uint data)
	{
		// Address in VRAM indexed with 0 at 0x2000
		address &= 0x0FFF;

		if (_nes.Cartridge is { Header.VerticalVRamMirroring: true })
		{
			switch (address)
			{
				// Vertical
				case < 0x0800:
					_vRam[address] = data;
					break;
				case < 0x1000:
					_vRam[address - 0x0800] = data;
					break;
			}
		}
		else
		{
			// Horizontal
			switch (address)
			{
				case < 0x0400:
					_vRam[address] = data;
					break;
				case < 0x0800:
					_vRam[address - 0x0400] = data;
					break;
				case < 0x0C00:
					_vRam[address - 0x0400] = data;
					break;
				case < 0x1000:
					_vRam[address - 0x0800] = data;
					break;
			}
		}

		throw new InvalidOperationException();
	}

	private void WriteToPalette(uint address, uint data)
	{
		// todo calculate address (mirrors)

		var offset = (address - 0x3F00) % 32;
		
		// Mirror $3F10, $3F14, $3F18, $3F1C to $3F00, $3F04, $3F08 $3F0C
		if (offset >= 16 && (offset - 16) % 4 == 0)
			_palette[offset - 16] = data;
		else
			_palette[offset] = data;
	}
}