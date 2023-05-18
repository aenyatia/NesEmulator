namespace NesEmulator.Core;

public class Memory : IMemory
{
	private readonly uint[] _memory = new uint[65536];
	
	public uint ReadByte(uint address) => _memory[(ushort)address];

	public void WriteByte(uint address, uint value) => _memory[(ushort)address] = value;
}