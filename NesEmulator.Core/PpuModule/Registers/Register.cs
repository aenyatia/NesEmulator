using System.Numerics;

namespace NesEmulator.Core.PpuModule.Registers;

public abstract class Register<T> where T : IBitwiseOperators<T, T, T>, IEqualityOperators<T, T, bool>
{
	protected const byte Bit0 = 0b00000001;
	protected const byte Bit1 = 0b00000010;
	protected const byte Bit2 = 0b00000100;
	protected const byte Bit3 = 0b00001000;
	protected const byte Bit4 = 0b00010000;
	protected const byte Bit5 = 0b00100000;
	protected const byte Bit6 = 0b01000000;
	protected const byte Bit7 = 0b10000000;
	protected const ushort Bit8 = 0b00000001_00000000;
	protected const ushort Bit9 = 0b00000010_00000000;
	protected const ushort Bit10 = 0b00000100_00000000;
	protected const ushort Bit11 = 0b00001000_00000000;
	protected const ushort Bit12 = 0b00010000_00000000;
	protected const ushort Bit13 = 0b00100000_00000000;
	protected const ushort Bit14 = 0b01000000_00000000;
	protected const ushort Bit15 = 0b10000000_00000000;

	protected Register(T value)
	{
		Value = value;
	}
	
	public T Value { get; set; }

	protected bool GetFlag(T flag)
	{
		return (Value & flag) != default;
	}

	protected void SetFlag(T flag, bool value)
	{
		if (value) Value |= flag;
		else Value &= ~flag;
	}
}