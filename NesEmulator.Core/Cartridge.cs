namespace NesEmulator.Core;

public class Cartridge
{
	private byte[] _prgMemory;
	private byte[] _chrMemory;

	private uint mapperID;
	private uint prgBanks;
	private uint chrBanks;
	private Mapper _mapper;

	public Cartridge(string filename)
	{
		var raw = File.ReadAllBytes(filename);

		// header 16 bytes
		// public char[] name = new char[4]; 0-3
		// public uint prg_rom_chunks = 0; 4
		// public uint chr_rom_chunks = 0; 5
		// public uint mapper1 = 0; 6
		// public uint mapper2 = 0; 7
		// public uint prg_ram_size = 0; 8
		// public uint tv_system1 = 0; 9
		// public uint tv_system2 = 0; 10
		// public char[] unused = new char[5]; 11-15

		// 0 - 3 "NES<EOF>"
		var header = BitConverter.ToInt32(raw, 0);

		// 4 size of prg rom in 16KB
		prgBanks = raw[4];

		// 5 size of chr rom in 8KB
		chrBanks = raw[5];

		// 6 flag mapper1
		var mapper1 = raw[6];

		// 7 flag mapper2
		var mapper2 = raw[7];

		// 8 flag prg_ram_size
		var prgSize = raw[8];

		// 9 flag tv_system1
		// 10 flag tv_system2
		// 11-15 unused

		var hasTrainer = (raw[6] & 0b100) > 0;
		var offset = 16 + (hasTrainer ? 512 : 0);

		mapperID = (uint)(mapper1 >> 4) | (uint)(mapper2 & 0xF0);

		_prgMemory = new byte[prgBanks * 16384]; // 1 * 16KB
		Array.Copy(raw, offset, _prgMemory, 0, _prgMemory.Length);

		_chrMemory = new byte[chrBanks * 8192]; // 1 * 8KB
		Array.Copy(raw, offset + _prgMemory.Length, _chrMemory, 0, _chrMemory.Length);

		_mapper = mapperID switch
		{
			0 => new Mapper000(prgBanks, chrBanks),
			_ => new Mapper000(prgBanks, chrBanks)
		};
	}

	public bool CpuWrite(uint address, uint data)
	{
		return false;
	}

	public bool CpuRead(uint address, out uint data)
	{
		if (_mapper.CpuMapRead(address, out var mappedAddress))
		{
			data = _prgMemory[(int)mappedAddress];
			return true;
		}

		data = 0;
		return false;
	}

	public bool PpuWrite(uint address, uint data)
	{
		return false;
	}

	public bool PpuRead(uint address, out uint data)
	{
		if (_mapper.CpuMapRead(address, out var mappedAddress))
		{
			data = _chrMemory[(int)mappedAddress];
			return true;
		}

		data = 0;
		return false;
	}
}