using NesEmulator.Core.CartridgeModule.Mappers;

namespace NesEmulator.Core.CartridgeModule;

public class Cartridge
{
	private const int WelcomeMessage = 0x1A53454E;

	private readonly byte[] _prgRom;
	private readonly byte[] _chrRom;
	private readonly byte[] _prgRam;
	private readonly IMapper _mapper;

	public Header Header { get; set; } 

	public static Cartridge Create(string path)
	{
		using var reader = new BinaryReader(File.OpenRead(path));

		var header = LoadHeader(reader);
		var prgRom = LoadPrgRom(reader, header);
		var chrRom = LoadChrRom(reader, header);
		var mapper = LoadMapper(header);

		return new Cartridge(prgRom, chrRom, mapper, header);
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
		};

		if (header.Message != WelcomeMessage)
			throw new Exception("invalid_cartridge");

		return header;
	}

	private static byte[] LoadPrgRom(BinaryReader reader, Header header)
	{
		var offset = header.ContainsTrainer ? 16 + 512 : 16;

		reader.BaseStream.Seek(offset, SeekOrigin.Begin);

		var prgRom = new byte[header.PrgRomBanks * 16384];

		var countBytes = reader.Read(prgRom, 0, prgRom.Length);
		if (countBytes != prgRom.Length)
			throw new IOException("load_prg_error");

		return prgRom;
	}

	private static byte[] LoadChrRom(BinaryReader reader, Header header)
	{
		var chrRom = new byte[header.ChrRomBanks * 8192];

		var countBytes = reader.Read(chrRom, 0, chrRom.Length);
		if (countBytes != chrRom.Length)
			throw new IOException("load_chr_error");

		return chrRom;
	}

	private static IMapper LoadMapper(Header header)
	{
		return header.MapperId switch
		{
			0 => new NRom(header.PrgRomBanks),
			_ => throw new ArgumentOutOfRangeException(nameof(header))
		};
	}

	private Cartridge(byte[] prgRom, byte[] chrRom, IMapper mapper, Header header)
	{
		_prgRom = prgRom;
		_chrRom = chrRom;
		_mapper = mapper;
		_prgRam = new byte[2048];
		Header = header;
	}

	public byte ReadPrgRam(ushort address)
	{
		address = _mapper.MapReadAddress(address);

		return _prgRam[address];
	}

	public void WritePrgRam(ushort address, byte data)
	{
		address = _mapper.Write(address);

		_prgRam[address] = data;
	}

	public byte ReadPrgRom(ushort address)
	{
		address = _mapper.MapReadAddress(address);

		return _prgRom[address];
	}

	public byte ReadChr(ushort address)
	{
		address = _mapper.MapReadAddress(address);

		return _chrRom[address];
	}

	public void WriteChr(ushort address, byte data)
	{
		address = _mapper.Write(address);

		_chrRom[address] = data;
	}
}