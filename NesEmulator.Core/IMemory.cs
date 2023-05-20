namespace NesEmulator.Core;

public interface IReadable
{
	uint Read(uint address);
}

public interface IWritable
{
	void Write(uint address, uint data);
}

public interface IMemory : IReadable, IWritable
{
}