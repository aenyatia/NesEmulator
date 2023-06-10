namespace NesEmulator.Core.CpuModule;

public class CpuMemory : IMemory
{
	private readonly uint[] _ram = new uint[2048];
	private readonly Nes _nes;

	public CpuMemory(Nes nes) => _nes = nes;

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
		return _ram[address % 0x0800];
	}

	private uint ReadFromPpu(uint address)
	{
		address = (address - 0x2000) % 0x0008;
		
		return _nes.Ppu.Read(address);
	}

	private uint ReadFromApu(uint address)
	{
		return _nes.Controller.Read(address);
	}

	private uint ReadFromCartridge(uint address)
	{
		if (_nes.Cartridge is null)
			throw new NullReferenceException();
		
		return _nes.Cartridge.ReadPrgRom((ushort)address);
	}

	private static uint ReadFromUnusedMemory()
	{
		return 0x00;
	}


	private void WriteToRam(uint address, uint data)
	{
		_ram[address % 0x0800] = data;
	}

	private void WriteToPpu(uint address, uint data)
	{
		address = (address - 0x2000) % 0x0008;

		_nes.Ppu.Write(address, data);
	}

	private void WriteToApu(uint address, uint data)
	{
		_nes.Controller.Write(address, data);
	}

	private void WriteToCartridge(uint address, uint data)
	{
		if (_nes.Cartridge is null)
			throw new NullReferenceException();

		_nes.Cartridge.WritePrgRam((ushort)address, (byte)data);
	}

	private static void WriteToUnusedMemory()
	{
	}
}