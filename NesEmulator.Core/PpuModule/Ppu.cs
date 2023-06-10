using NesEmulator.Core.PpuModule.Registers;

namespace NesEmulator.Core.PpuModule;

public class Ppu
{
	private readonly PpuMemory _ppuMemory;

	private readonly ControlRegister _controlRegister = new();
	private readonly MaskRegister _maskRegister = new();
	private readonly StatusRegister _statusRegister  = new();
	
	// data register
	private byte _ppuDataBuffer;

	// Pixel "dot" position information
	private bool FrameComplete { get; set; }
	private int Scanline { get; set; }
	private int Cycle { get; set; }
	
	// Background rendering
	private byte _bgNextTileId     = 0x00;
	private byte _bgNextTileAttrib = 0x00;
	private byte _bgNextTileLsb    = 0x00;
	private byte _bgNextTileMsb    = 0x00;
	
	private ushort _bgShifterPatternLo = 0x0000;
	private ushort _bgShifterPatternHi = 0x0000;
	private ushort _bgShifterAttribLo  = 0x0000;
	private ushort _bgShifterAttribHi  = 0x0000;
	
	// scroll registers
	private LoopyRegister _vRamAddress = new();
	private LoopyRegister _tRamAddress = new();
	private byte _addressLatch; // x
	private byte _fineX; // w
	
	public bool Nmi { get; set; }
	public byte[,] Screen { get; set; } = new byte[240, 256];
	
	// ctor
	public Ppu(Nes nes) => _ppuMemory = new PpuMemory(nes);

	public uint Read(uint address)
	{
		byte data = 0x00;
		switch (address)
		{
			case 0x0000: // control
				// not readable
				break;
			case 0x0001: // mask
				// not readable
				break;
			case 0x0002: // status
				data = (byte)((_statusRegister.Value & 0xE0) | (_ppuDataBuffer & 0x1F));
				
				_statusRegister.VerticalBlanc = false;
				_addressLatch = 0;
				break;
			case 0x0003: // oam address
				// not readable
				break;
			case 0x0004: // oam data
				break;
			case 0x0005: // scroll
				// not readable
				break;
			case 0x0006: // ppu address
				// not readable
				break;
			case 0x0007: // ppu data
				data = _ppuDataBuffer;
				_ppuDataBuffer = (byte)Read(_vRamAddress.Value);

				if (_vRamAddress.Value > 0x3F00)
					data = _ppuDataBuffer;
				break;
		}

		return data;
	}

	public void Write(uint address, uint data)
	{
		switch (address)
		{
			case 0x0000: // control
				_controlRegister.Value = (byte)data;
				_tRamAddress.NametableX = (byte)(_controlRegister.NametableX ? 1 : 0);
				_tRamAddress.NametableY = (byte)(_controlRegister.NametableY ? 1 : 0);
				break;
			case 0x0001: // mask
				_maskRegister.Value = (byte)data;
				break;
			case 0x0002: // status
				// cannot write
				break;
			case 0x0003: // oam address
				break;
			case 0x0004: // oam data
				break;
			case 0x0005: // scroll
				if (_addressLatch == 0)
				{
					_fineX = (byte)(data & 0x07);
					_tRamAddress.CoarseX = (byte)(data >> 3);
					_addressLatch = 1;
				}
				else
				{
					_tRamAddress.FineY = (byte)(data & 0x07);
					_tRamAddress.CoarseY = (byte)(data >> 3);
					_addressLatch = 0;
				}
				break;
			case 0x0006: // ppu address
				if (_addressLatch == 0)
				{
					_tRamAddress.Value = (ushort)(((data & 0x3F) << 8) | (uint)(_tRamAddress.Value & 0x00FF));
					_addressLatch = 1;
				}
				else
				{
					
					_tRamAddress.Value = (byte)((uint)(_tRamAddress.Value & 0xFF00) | data);
					_vRamAddress.Value = _tRamAddress.Value;
					_addressLatch = 0;
				}
				break;
			case 0x0007: // ppu data
				Write(_vRamAddress.Value, data);
				_vRamAddress.Value += (ushort)(_controlRegister.IncrementMode ? 32 : 1);
				break;
		}
	}
	
	public void Clock()
	{
		// Trigger an NMI at the start of _scanline 241 if VBLANK NMI's are enabled
		if (Scanline == 241 && Cycle == 1)
		{
			_statusRegister.VerticalBlanc = true;
			
			if (_controlRegister.EnableNmi)
				Nmi = true;
		}
		
		// set screen
		Screen[Cycle - 1, Scanline] = (byte)(Random.Shared.Next() % 2 == 0 ? 0x3F : 0x1A);

		Cycle++;
		if (Cycle >= 341)
		{
			Cycle = 0;
			Scanline++;
			if (Scanline >= 261)
			{
				Scanline = -1;
				FrameComplete = true;
			}
		}
	}
}