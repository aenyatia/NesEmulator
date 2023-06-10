namespace NesEmulator.Core.PpuModule.Registers;

public class LoopyRegister : Register<ushort>
{
	public LoopyRegister(ushort value = 0) : base(value) { }

	public byte CoarseX { get; set; }
	public byte CoarseY { get; set; } // 5 bit
	public byte NametableX { get; set; } // 1 bit
	public byte NametableY { get; set; } // 1 bit
	public byte FineY { get; set; } // 3 bit
	public byte Unused { get; set; } // 1 bit
}