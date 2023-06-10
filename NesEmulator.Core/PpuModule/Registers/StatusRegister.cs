namespace NesEmulator.Core.PpuModule.Registers;

public class StatusRegister : Register<byte>
{
	public StatusRegister(byte value = 0) : base(value) { }

	public bool SpriteOverflow
	{
		get => GetFlag(Bit5);
		set => SetFlag(Bit5, value);
	}

	public bool SpriteZeroHit
	{
		get => GetFlag(Bit6);
		set => SetFlag(Bit6, value);
	}

	public bool VerticalBlanc
	{
		get => GetFlag(Bit7);
		set => SetFlag(Bit7, value);
	}
}