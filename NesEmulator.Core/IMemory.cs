namespace NesEmulator.Core;

public interface IMemory
{
	uint ReadByte(uint address);
	void WriteByte(uint address, uint value);
}