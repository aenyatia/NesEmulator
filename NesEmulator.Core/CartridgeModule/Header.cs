namespace NesEmulator.Core.CartridgeModule;

public class Header
{
	public bool VerticalVRamMirroring => (Flag6 & 0x01) != 0x00;
	public bool BatteryBackedMemory => (Flag6 & 0x02) != 0x00;
	public bool ContainsTrainer => (Flag6 & 0x04) != 0x00;
	public int MapperId => (Flag7 & 0xF0) | (Flag6 >> 4);

	public int Message { get; init; }
	public byte PrgRomBanks { get; init; }
	public byte ChrRomBanks { get; init; }
	public byte Flag6 { get; init; }
	public byte Flag7 { get; init; }
	public byte Flag8 { get; init; }
	public byte Flag9 { get; init; }
	public byte Flag10 { get; init; }
	public byte[] Unused { get; init; } = new byte[4];
}