namespace NesEmulator.Core.CpuModule;

public class Instruction(
	Func<bool> mode,
	Func<bool> operation,
	uint opcode,
	string mnemonic,
	uint cycles)
{
	public uint Opcode { get; } = opcode;
	public string Mnemonic { get; } = mnemonic;
	private uint Cycles { get; } = cycles;

	public uint Execute()
	{
		var modeCross = mode();
		var opCross = operation();

		return modeCross && opCross
			? Cycles + 1
			: Cycles;
	}
}