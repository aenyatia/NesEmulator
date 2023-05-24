namespace NesEmulator.Core.CartridgeModule;

public class Header
{
	// Constant $4E $45 $53 $1A (ASCII "NES" followed by MS-DOS end-of-file)
	public int Message { get; init; }

	// Size of PRG ROM in 16 KB units
	public byte PrgRomBanks { get; init; }

	// Size of CHR ROM in 8 KB units (value 0 means the board uses CHR RAM)
	public byte ChrRomBanks { get; init; }

	// 76543210
	// ||||||||
	// |||||||+- Mirroring: 0: horizontal (vertical arrangement) (CIRAM A10 = PPU A11)
	// |||||||              1: vertical (horizontal arrangement) (CIRAM A10 = PPU A10)
	// ||||||+-- 1: Cartridge contains battery-backed PRG RAM ($6000-7FFF) or other persistent memory
	// |||||+--- 1: 512-byte trainer at $7000-$71FF (stored before PRG data)
	// 	||||+---- 1: Ignore mirroring control or above mirroring bit; instead provide four-screen VRAM
	// ++++----- Lower nybble of mapper number
	public byte Flag6 { get; init; }

	// 76543210
	// ||||||||
	// |||||||+- VS Uni system
	// ||||||+-- PlayChoice-10 (8 KB of Hint Screen data stored after CHR data)
	// ||||++--- If equal to 2, flags 8-15 are in NES 2.0 format
	// ++++----- Upper nybble of mapper number
	public byte Flag7 { get; init; }

	// 76543210
	// ||||||||
	// ++++++++- PRG RAM size
	public byte Flag8 { get; init; }

	// 76543210
	// ||||||||
	// |||||||+- TV system (0: NTSC; 1: PAL)
	// +++++++-- Reserved, set to zero
	public byte Flag9 { get; init; }

	// 76543210
	// ||  ||
	// ||  ++- TV system (0: NTSC; 2: PAL; 1/3: dual compatible)
	// |+----- PRG RAM ($6000-$7FFF) (0: present; 1: not present)
	// +------ 0: Board has no bus conflicts; 1: Board has bus conflicts
	public byte Flag10 { get; init; }

	public bool VerticalVRamMirroring => (Flag6 & 0x01) != 0x00;
	public bool BatteryBackedMemory => (Flag6 & 0x02) != 0x00;
	public bool ContainsTrainer => (Flag6 & 0x04) != 0x00;
	public int MapperId => (Flag7 & 0xF0) | (Flag6 >> 4);
}