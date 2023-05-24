namespace NesEmulator.Core.PpuModule;

public class Ppu
{
	private readonly PpuMemory _ppuMemory;

	public Ppu(Nes nes) => _ppuMemory = new PpuMemory(nes);

	public byte[,] BitMapData { get; } = new byte[128, 128];

	public void X()
	{
		var palette = new byte[] { 0x0e, 0x3d, 0x06, 0x18 };

		for (var i = 0; i < 16; i++)
		for (var j = 0; j < 16; j++)
		{
			var offset = i * 256 + j * 16;

			// tile 8px per 8px
			for (var row = 0; row < 8; row++)
			{
				var lsb = _ppuMemory.Read((uint)(offset + row + 0));
				var msb = _ppuMemory.Read((uint)(offset + row + 8));

				// iterate over columns
				for (var col = 0; col < 8; col++)
				{
					var colorIndex = (lsb & 0x01) + (msb & 0x01);
					lsb >>= 1;
					msb >>= 1;

					BitMapData[i * 8 + row, j * 8 + (7 -col)] = palette[colorIndex];
				}
			}
		}
	}

	public uint Read(uint address)
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

		return 0;
	}

	public void Write(uint address, uint data)
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

	public void Clock()
	{
		Console.WriteLine("tick");
	}
}