namespace NesEmulator.Core.PpuModule.Registers;

public class ControlRegister : Register<byte>
{
	public ControlRegister(byte value = 0) : base(value) { }

	public bool NametableX 
	{ 
		get => GetFlag(Bit0);
		set => SetFlag(Bit0, value); 
	}
	
	public bool NametableY 
	{ 
		get => GetFlag(Bit1);
		set => SetFlag(Bit1, value); 
	}
	
	public bool IncrementMode 
	{ 
		get => GetFlag(Bit2);
		set => SetFlag(Bit2, value); 
	}
	
	public bool SpritePattern 
	{ 
		get => GetFlag(Bit3);
		set => SetFlag(Bit3, value); 
	}
	
	public bool BackgroundPattern 
	{ 
		get => GetFlag(Bit4);
		set => SetFlag(Bit4, value); 
	}
	
	public bool SpriteSize 
	{ 
		get => GetFlag(Bit5);
		set => SetFlag(Bit5, value); 
	}
	
	public bool SlaveMode 
	{ 
		get => GetFlag(Bit6);
		set => SetFlag(Bit6, value); 
	}
	
	public bool EnableNmi 
	{ 
		get => GetFlag(Bit7);
		set => SetFlag(Bit7, value); 
	}
}