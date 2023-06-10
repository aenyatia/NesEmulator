namespace NesEmulator.Core.PpuModule.Registers;

public class MaskRegister : Register<byte>
{
	public MaskRegister(byte value = 0) : base(value) { }
	
	public bool GrayScale
	{
		get => GetFlag(Bit0); 
		set => SetFlag(Bit0, value);
	}
	
	public bool RenderBackgroundLeft
	{
		get => GetFlag(Bit1); 
		set => SetFlag(Bit1, value);
	}
	
	public bool RenderSpritesLeft
	{
		get => GetFlag(Bit2); 
		set => SetFlag(Bit2, value);
	}
	
	public bool RenderBackground
	{
		get => GetFlag(Bit3); 
		set => SetFlag(Bit3, value);
	}
	
	public bool RenderSprites
	{
		get => GetFlag(Bit4); 
		set => SetFlag(Bit4, value);
	}
	
	public bool EnhanceRed
	{
		get => GetFlag(Bit5); 
		set => SetFlag(Bit5, value);
	}
	
	public bool EnhanceGreen
	{
		get => GetFlag(Bit6); 
		set => SetFlag(Bit6, value);
	}
	
	public bool EnhanceBlue
	{
		get => GetFlag(Bit7); 
		set => SetFlag(Bit7, value);
	}
}