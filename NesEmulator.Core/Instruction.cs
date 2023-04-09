namespace NesEmulator.Core;

public class Instruction
{
	private readonly Func<uint> _mode;
	private readonly Action<uint> _operation;

	public Instruction(Func<uint> mode, Action<uint> operation, uint opcode, string mnemonic, uint cycles)
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

	public void Execute()
	{
		_operation(_mode());
	}
}