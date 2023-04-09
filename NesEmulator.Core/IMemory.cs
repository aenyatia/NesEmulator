namespace NesEmulator.Core;

public interface IMemory
{
	byte ReadByte(uint address);
	void WriteByte(uint address, uint value);
}