namespace NesEmulator.Core;

public class Instruction
{
	private readonly Func<bool> _mode;
	private readonly Func<bool> _operation;

	public Instruction(Func<bool> mode, Func<bool> operation,
		uint opcode, string mnemonic, uint cycles)
	{
		_mode = mode;
		_operation = operation;

		Opcode = opcode;
		Mnemonic = mnemonic;
		Cycles = cycles;
	}

	public uint Opcode { get; }
	public string Mnemonic { get; }
	public uint Cycles { get; }

	public uint Execute()
	{
		var modeCross = _mode();
		var opCross = _operation();

		return modeCross && opCross
			? Cycles + 1
			: Cycles;
	}
}