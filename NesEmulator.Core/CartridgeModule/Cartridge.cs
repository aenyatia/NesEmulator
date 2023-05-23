using NesEmulator.Core.CartridgeModule.MapperModule;
using NesEmulator.Core.CartridgeModule.MapperModule.Mappers;

namespace NesEmulator.Core.CartridgeModule;

public class Cartridge
{
	private const int Message = 0x1A53454E;
	
	private readonly Mapper _mapper;
	private readonly byte[] _prgRom;
	private readonly byte[] _chrRom;

	public byte[] ChrRom => _chrRom;

	private Cartridge(Mapper mapper, byte[] prgRom, byte[] chrRom)
	{
		_mapper = mapper;
		_prgRom = prgRom;
		_chrRom = chrRom;
	}

	public static Cartridge Create(string path)
	{
		using var br = new BinaryReader(File.OpenRead(path));

		var header = LoadHeader(br);
		var prgRom = LoadPrgRom(br, header);
		var chrRom = LoadChrRom(br, header);
		var mapper = LoadMapper(header);

		return new Cartridge(mapper, prgRom, chrRom);
	}

	private static Header LoadHeader(BinaryReader reader)
	{
		var header = new Header
		{
			Message = reader.ReadInt32(),
			PrgRomBanks = reader.ReadByte(),
			ChrRomBanks = reader.ReadByte(),
			Flag6 = reader.ReadByte(),
			Flag7 = reader.ReadByte(),
			Flag8 = reader.ReadByte(),
			Flag9 = reader.ReadByte(),
			Flag10 = reader.ReadByte(),
			Unused = reader.ReadBytes(4)
		};
		
		if (header.Message != Message)
			throw new Exception("invalid cartridge");

		return header;
	}

	private static byte[] LoadPrgRom(BinaryReader reader, Header header)
	{
		var offset = header.ContainsTrainer ? 16 + 512 : 16;

		reader.BaseStream.Seek(offset, SeekOrigin.Begin);

		var prgRom = new byte[header.PrgRomBanks * 16 * 1024];

		var readsBytes = reader.Read(prgRom, 0, prgRom.Length);
		if (readsBytes != prgRom.Length)
			throw new IOException();

		return prgRom;
	}

	private static byte[] LoadChrRom(BinaryReader reader, Header header)
	{
		var chrRom = new byte[header.ChrRomBanks * 8 * 1024];

		var readsBytes = reader.Read(chrRom, 0, chrRom.Length);
		if (readsBytes != chrRom.Length)
			throw new IOException();

		return chrRom;
	}

	private static Mapper LoadMapper(Header header)
	{
		return header.MapperId switch
		{
			0 => new Mapper000(header.PrgRomBanks, header.ChrRomBanks),
			1 => new Mapper000(header.PrgRomBanks, header.ChrRomBanks),
			2 => new Mapper000(header.PrgRomBanks, header.ChrRomBanks),
			_ => throw new ArgumentOutOfRangeException(nameof(header))
		};
	}
	
	// read_prg
	// read_chr
	// write_prg
	// write_chr

	public uint Read(uint address)
	{
		// todo mapping
		_mapper.CpuMapRead(address, out var mappedAddress);

		return _prgRom[(int)mappedAddress];
	}

	public void Write(uint address, uint data)
	{
		// todo mapping
	}

	public uint ReadFromPpu(uint address)
	{
		// todo mapping
		_mapper.CpuMapRead(address, out var mappedAddress);

		return _chrRom[(int)mappedAddress];
	}

	public void WriteFromPpu(uint address, uint data)
	{
		// todo mapping
	}
}