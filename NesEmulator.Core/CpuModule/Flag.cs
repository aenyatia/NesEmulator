namespace NesEmulator.Core.CpuModule;

public enum Flag
{
	Carry = 1 << 0,
	Zero = 1 << 1,
	Interrupt = 1 << 2,
	Decimal = 1 << 3,
	Break = 1 << 4,
	Reserved = 1 << 5,
	Overflow = 1 << 6,
	Negative = 1 << 7
}