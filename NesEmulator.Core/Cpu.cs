namespace NesEmulator.Core;

public class Cpu
{
	private readonly Instruction[] _instructions;
	private readonly IMemory _memory;

	private const uint ByteMask = 0x0000_00FF;
	private const uint WordMask = 0x0000_FFFF;

	private uint _a;
	private uint _x;
	private uint _y;
	private uint _sr;
	private uint _sp;
	private uint _pc;

	public Cpu(IMemory memory,
		uint a = 0x00, uint x = 0x00, uint y = 0x00,
		uint sr = 0x00, uint sp = 0x00, uint pc = 0x0000)
	{
		_instructions = CreateInstructions();
		_memory = memory;

		A = a;
		X = x;
		Y = y;

		SR = sr;
		SP = sp;
		PC = pc;
	}

	public uint A
	{
		get => _a;
		private set => _a = value & ByteMask;
	}
	public uint X
	{
		get => _x;
		private set => _x = value & ByteMask;
	}
	public uint Y
	{
		get => _y;
		private set => _y = value & ByteMask;
	}
	public uint SR
	{
		get => _sr;
		private set => _sr = value & ByteMask;
	}
	public uint SP
	{
		get => _sp;
		private set => _sp = value & ByteMask;
	}
	public uint PC
	{
		get => _pc;
		private set => _pc = value & WordMask;
	}

	public bool CarryFlag
	{
		get => GetFlag(Flag.Carry);
		private set => SetFlag(Flag.Carry, value);
	}
	public bool ZeroFlag
	{
		get => GetFlag(Flag.Zero);
		private set => SetFlag(Flag.Zero, value);
	}
	public bool InterruptFlag
	{
		get => GetFlag(Flag.Interrupt);
		private set => SetFlag(Flag.Interrupt, value);
	}
	public bool DecimalFlag
	{
		get => GetFlag(Flag.Decimal);
		private set => SetFlag(Flag.Decimal, value);
	}
	public bool BreakFlag
	{
		get => GetFlag(Flag.Break);
		private set => SetFlag(Flag.Break, value);
	}
	public bool ReservedFlag
	{
		get => GetFlag(Flag.Reserved);
		private set => SetFlag(Flag.Reserved, value);
	}
	public bool OverflowFlag
	{
		get => GetFlag(Flag.Overflow);
		private set => SetFlag(Flag.Overflow, value);
	}
	public bool NegativeFlag
	{
		get => GetFlag(Flag.Negative);
		private set => SetFlag(Flag.Negative, value);
	}

	public void ExecuteSingleInstruction()
	{
		// fetch
		var opcode = NextByte();

		// decode
		var instruction = _instructions[opcode];

		// execute
		instruction.Execute();
	}

	#region Modes

	private uint Acc()
	{
		return 0x000;
	}
	private uint Abs()
	{
		return NextWord();
	}
	private uint Abx()
	{
		return NextWord() + X;
	}
	private uint Aby()
	{
		return NextWord() + Y;
	}
	private uint Imm()
	{
		return PC++;
	}
	private uint Imp()
	{
		return 0x0000;
	}
	private uint Ind()
	{
		var ptr = NextWord();

		return ReadWord(ptr);
	}
	private uint Inx()
	{
		var ptr = NextByte();

		return ReadWord(ptr + X);
	}
	private uint Iny()
	{
		var ptr = NextByte();

		return ReadWord(ptr) + Y;
	}
	private uint Rel()
	{
		return (uint)(PC + (int)NextByte());
	}
	private uint Zpg()
	{
		return NextByte();
	}
	private uint Zpx()
	{
		return NextByte() + X;
	}
	private uint Zpy()
	{
		return NextByte() + Y;
	}

	private uint Unsupported() => 0x0000;

	#endregion

	#region Operations

	// Transfer Instructions (12)
	private void Lda(uint address)
	{
		A = ReadByte(address);

		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);
	}
	private void Ldx(uint address)
	{
		X = ReadByte(address);

		ZeroFlag = IsZero(X);
		NegativeFlag = IsNegative(X);
	}
	private void Ldy(uint address)
	{
		Y = ReadByte(address);

		ZeroFlag = IsZero(Y);
		NegativeFlag = IsNegative(Y);
	}
	private void Sta(uint address)
	{
		WriteByte(address, A);
	}
	private void Stx(uint address)
	{
		WriteByte(address, X);
	}
	private void Sty(uint address)
	{
		WriteByte(address, Y);
	}
	private void Tax(uint address)
	{
		X = A;

		ZeroFlag = IsZero(X);
		NegativeFlag = IsNegative(X);
	}
	private void Tay(uint address)
	{
		Y = A;

		ZeroFlag = IsZero(Y);
		NegativeFlag = IsNegative(Y);
	}
	private void Tsx(uint address)
	{
		X = SP;

		ZeroFlag = IsZero(X);
		NegativeFlag = IsNegative(X);
	}
	private void Txa(uint address)
	{
		A = X;

		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);
	}
	private void Txs(uint address)
	{
		SP = X;
	}
	private void Tya(uint address)
	{
		A = Y;

		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);
	}

	// Stack Instructions (4)
	private void Pha(uint address)
	{
		StackPush(A);
	}
	private void Php(uint address)
	{
		StackPush(SR);
	}
	private void Pla(uint address)
	{
		A = StackPop();

		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);
	}
	private void Plp(uint address)
	{
		SR = StackPop();

		// todo plp -> update flags
	}

	// Decrements & Increments (6)
	private void Dec(uint address)
	{
		var value = ReadByte(address) - 1;
		WriteByte(address, value);

		ZeroFlag = IsZero(value);
		NegativeFlag = IsNegative(value);
	}
	private void Dex(uint address)
	{
		X -= 1;

		ZeroFlag = IsZero(X);
		NegativeFlag = IsNegative(X);
	}
	private void Dey(uint address)
	{
		Y -= 1;

		ZeroFlag = IsZero(Y);
		NegativeFlag = IsNegative(Y);
	}
	private void Inc(uint address)
	{
		var value = ReadByte(address) + 1;
		WriteByte(address, value);

		ZeroFlag = IsZero(value);
		NegativeFlag = IsNegative(value);
	}
	private void Inx(uint address)
	{
		X += 1;

		ZeroFlag = IsZero(X);
		NegativeFlag = IsNegative(X);
	}
	private void Iny(uint address)
	{
		Y += 1;

		ZeroFlag = IsZero(Y);
		NegativeFlag = IsNegative(Y);
	}

	// Arithmetic Operations (2)
	private void Adc(uint address) { }
	private void Sbc(uint address) { }

	// Logical Operations (3)
	private void And(uint address)
	{
		A &= ReadByte(address);

		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);
	}
	private void Eor(uint address)
	{
		A ^= ReadByte(address);

		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);
	}
	private void Ora(uint address)
	{
		A |= ReadByte(address);

		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);
	}

	// Shift & Rotate Instructions (4)
	private void Asl(uint address) { }
	private void Lsr(uint address) { }
	private void Rol(uint address) { }
	private void Ror(uint address) { }

	// Flag Instructions (7)
	private void Clc(uint address) { }
	private void Cld(uint address) { }
	private void Cli(uint address) { }
	private void Clv(uint address) { }
	private void Sec(uint address) { }
	private void Sed(uint address) { }
	private void Sei(uint address) { }

	// Comparison (3)
	private void Cmp(uint address) { }
	private void Cpx(uint address) { }
	private void Cpy(uint address) { }

	// Conditional Branch Instructions (8)
	private void Bcc(uint address) { }
	private void Bcs(uint address) { }
	private void Beq(uint address) { }
	private void Bmi(uint address) { }
	private void Bne(uint address) { }
	private void Bpl(uint address) { }
	private void Bvc(uint address) { }
	private void Bvs(uint address) { }

	// Jumps & Subroutines (3)
	private void Jmp(uint address) { }
	private void Jsr(uint address) { }
	private void Rts(uint address) { }

	// Interrupts (2)
	private void Brk(uint address) { }
	private void Rti(uint address) { }

	// Other (2)
	private void Bit(uint address) { }
	private void Nop(uint address) { }

	private void Unsupported(uint address) { }

	#endregion

	#region Utils

	private void WriteByte(uint address, uint value)
	{
		_memory.WriteByte(address, value);
	}

	private uint ReadByte(uint address)
	{
		return _memory.ReadByte(address);
	}
	private uint ReadWord(uint address)
	{
		var lo = ReadByte(address);
		var hi = ReadByte(address + 1);

		return (hi << 8) | lo;
	}

	private uint NextByte()
	{
		return _memory.ReadByte(PC++);
	}
	private uint NextWord()
	{
		var lo = NextByte();
		var hi = NextByte();

		return (hi << 8) | lo;
	}

	private bool GetFlag(Flag flag)
	{
		return (SR & (uint)flag) != 0x00;
	}
	private void SetFlag(Flag flag, bool value)
	{
		if (value) SR |= (uint)flag;
		else SR &= (uint)~flag;
	}

	private void StackPush(uint value) { }
	private uint StackPop()
	{
		return 0x00;
	}

	private static bool IsZero(uint value) => value == 0x00;
	private static bool IsNegative(uint value) => (value & 0x80) != 0x00;

	#endregion

	private Instruction[] CreateInstructions()
	{
		var instructions = new Instruction[256];

		// 0 -> 0 - 15
		instructions[0x00] = new Instruction(Imp, Brk, 0x00, "BRK oper", 1);
		instructions[0x01] = new Instruction(Inx, Ora, 0x01, "ORA oper", 1);
		instructions[0x02] = new Instruction(Unsupported, Unsupported, 0x02, "unsupported", 0);
		instructions[0x03] = new Instruction(Unsupported, Unsupported, 0x03, "unsupported", 0);
		instructions[0x04] = new Instruction(Unsupported, Unsupported, 0x04, "unsupported", 0);
		instructions[0x05] = new Instruction(Zpg, Ora, 0x05, "ORA oper", 1);
		instructions[0x06] = new Instruction(Zpg, Asl, 0x06, "ASL oper", 1);
		instructions[0x07] = new Instruction(Unsupported, Unsupported, 0x07, "unsupported", 0);
		instructions[0x08] = new Instruction(Imp, Php, 0x08, "PHP oper", 1);
		instructions[0x09] = new Instruction(Imm, Ora, 0x09, "ORA oper", 1);
		instructions[0x0A] = new Instruction(Acc, Asl, 0x0A, "ASL oper", 1);
		instructions[0x0B] = new Instruction(Unsupported, Unsupported, 0x0B, "unsupported", 0);
		instructions[0x0C] = new Instruction(Unsupported, Unsupported, 0x0C, "unsupported", 0);
		instructions[0x0D] = new Instruction(Abs, Ora, 0x0D, "ORA oper", 1);
		instructions[0x0E] = new Instruction(Abs, Asl, 0x0E, "ASL oper", 1);
		instructions[0x0F] = new Instruction(Unsupported, Unsupported, 0x0F, "unsupported", 0);

		// 1 -> 16 - 31
		instructions[0x10] = new Instruction(Rel, Bpl, 0x10, "REL oper", 1);
		instructions[0x11] = new Instruction(Iny, Ora, 0x11, "INY oper", 1);
		instructions[0x12] = new Instruction(Unsupported, Unsupported, 0x12, "unsupported", 0);
		instructions[0x13] = new Instruction(Unsupported, Unsupported, 0x13, "unsupported", 0);
		instructions[0x14] = new Instruction(Unsupported, Unsupported, 0x14, "unsupported", 0);
		instructions[0x15] = new Instruction(Zpx, Ora, 0x15, "ZPX oper", 1);
		instructions[0x16] = new Instruction(Zpx, Asl, 0x16, "ZPX oper", 1);
		instructions[0x17] = new Instruction(Unsupported, Unsupported, 0x17, "unsupported", 0);
		instructions[0x18] = new Instruction(Imp, Clc, 0x18, "IMP oper", 1);
		instructions[0x19] = new Instruction(Aby, Ora, 0x19, "ABY oper", 1);
		instructions[0x1A] = new Instruction(Unsupported, Unsupported, 0x1A, "unsupported", 0);
		instructions[0x1B] = new Instruction(Unsupported, Unsupported, 0x1B, "unsupported", 0);
		instructions[0x1C] = new Instruction(Unsupported, Unsupported, 0x1C, "unsupported", 0);
		instructions[0x1D] = new Instruction(Abx, Ora, 0x1D, "ABX oper", 1);
		instructions[0x1E] = new Instruction(Abx, Asl, 0x1E, "ABX oper", 1);
		instructions[0x1F] = new Instruction(Unsupported, Unsupported, 0x1F, "unsupported", 0);

		// 2 -> 32 - 47
		instructions[0x20] = new Instruction(Abs, Jsr, 0x20, "JSR oper", 1);
		instructions[0x21] = new Instruction(Inx, And, 0x21, "AND oper", 1);
		instructions[0x22] = new Instruction(Unsupported, Unsupported, 0x22, "Unsupported", 0);
		instructions[0x23] = new Instruction(Unsupported, Unsupported, 0x23, "Unsupported", 0);
		instructions[0x24] = new Instruction(Zpg, Bit, 0x24, "BIT oper", 1);
		instructions[0x25] = new Instruction(Zpg, And, 0x25, "AND oper", 1);
		instructions[0x26] = new Instruction(Zpg, Rol, 0x26, "ROL oper", 1);
		instructions[0x27] = new Instruction(Unsupported, Unsupported, 0x27, "Unsupported", 0);
		instructions[0x28] = new Instruction(Imp, Plp, 0x28, "PLP oper", 1);
		instructions[0x29] = new Instruction(Imm, And, 0x29, "AND oper", 1);
		instructions[0x2A] = new Instruction(Acc, Rol, 0x2A, "ROL oper", 1);
		instructions[0x2B] = new Instruction(Unsupported, Unsupported, 0x2B, "Unsupported", 0);
		instructions[0x2C] = new Instruction(Abs, Bit, 0x2C, "BIT oper", 1);
		instructions[0x2D] = new Instruction(Abs, And, 0x2D, "AND oper", 1);
		instructions[0x2E] = new Instruction(Abs, Rol, 0x2E, "ROL oper", 1);
		instructions[0x2F] = new Instruction(Unsupported, Unsupported, 0x2F, "Unsupported", 0);

		// 3 -> 48 - 63
		instructions[0x30] = new Instruction(Rel, Bmi, 0x30, "BMI oper", 1);
		instructions[0x31] = new Instruction(Iny, And, 0x31, "AND oper", 1);
		instructions[0x32] = new Instruction(Unsupported, Unsupported, 0x32, "Unsupported", 0);
		instructions[0x33] = new Instruction(Unsupported, Unsupported, 0x33, "Unsupported", 0);
		instructions[0x34] = new Instruction(Unsupported, Unsupported, 0x34, "Unsupported", 0);
		instructions[0x35] = new Instruction(Zpx, And, 0x35, "AND oper", 1);
		instructions[0x36] = new Instruction(Zpx, Rol, 0x36, "ROL oper", 1);
		instructions[0x37] = new Instruction(Unsupported, Unsupported, 0x37, "Unsupported", 0);
		instructions[0x38] = new Instruction(Imp, Sec, 0x38, "SEC oper", 1);
		instructions[0x39] = new Instruction(Aby, And, 0x39, "AND oper", 1);
		instructions[0x3A] = new Instruction(Unsupported, Unsupported, 0x3A, "Unsupported", 0);
		instructions[0x3B] = new Instruction(Unsupported, Unsupported, 0x3B, "Unsupported", 0);
		instructions[0x3C] = new Instruction(Unsupported, Unsupported, 0x3C, "Unsupported", 0);
		instructions[0x3D] = new Instruction(Abx, And, 0x3D, "AND oper", 1);
		instructions[0x3E] = new Instruction(Abx, Rol, 0x3E, "ROL oper", 1);
		instructions[0x3F] = new Instruction(Unsupported, Unsupported, 0x3F, "Unsupported", 0);

		// 4 -> 64 - 79
		instructions[0x40] = new Instruction(Imp, Rti, 0x40, "RTI oper", 1);
		instructions[0x41] = new Instruction(Inx, Eor, 0x41, "EOR oper", 1);
		instructions[0x42] = new Instruction(Unsupported, Unsupported, 0x42, "Unsupported", 0);
		instructions[0x43] = new Instruction(Unsupported, Unsupported, 0x43, "Unsupported", 0);
		instructions[0x44] = new Instruction(Unsupported, Unsupported, 0x44, "Unsupported", 0);
		instructions[0x45] = new Instruction(Zpg, Eor, 0x45, "EOR oper", 1);
		instructions[0x46] = new Instruction(Zpg, Lsr, 0x46, "LSR oper", 1);
		instructions[0x47] = new Instruction(Unsupported, Unsupported, 0x47, "Unsupported", 0);
		instructions[0x48] = new Instruction(Imp, Pha, 0x48, "PHA oper", 1);
		instructions[0x49] = new Instruction(Imm, Eor, 0x49, "EOR oper", 1);
		instructions[0x4A] = new Instruction(Acc, Lsr, 0x4A, "LSR oper", 1);
		instructions[0x4B] = new Instruction(Unsupported, Unsupported, 0x4B, "Unsupported", 0);
		instructions[0x4C] = new Instruction(Abs, Jmp, 0x4C, "JMP oper", 1);
		instructions[0x4D] = new Instruction(Abs, Eor, 0x4D, "EOR oper", 1);
		instructions[0x4E] = new Instruction(Abs, Lsr, 0x4E, "LSR oper", 1);
		instructions[0x4F] = new Instruction(Unsupported, Unsupported, 0x4F, "Unsupported", 0);

		// 5 -> 80 - 95
		instructions[0x50] = new Instruction(Rel, Bvc, 0x50, "BVC oper", 1);
		instructions[0x51] = new Instruction(Iny, Eor, 0x51, "EOR oper", 1);
		instructions[0x52] = new Instruction(Unsupported, Unsupported, 0x52, "Unsupported", 0);
		instructions[0x53] = new Instruction(Unsupported, Unsupported, 0x53, "Unsupported", 0);
		instructions[0x54] = new Instruction(Unsupported, Unsupported, 0x54, "Unsupported", 0);
		instructions[0x55] = new Instruction(Zpx, Eor, 0x55, "EOR oper", 1);
		instructions[0x56] = new Instruction(Zpx, Lsr, 0x56, "LSR oper", 1);
		instructions[0x57] = new Instruction(Unsupported, Unsupported, 0x57, "Unsupported", 0);
		instructions[0x58] = new Instruction(Imp, Cli, 0x58, "CLI oper", 1);
		instructions[0x59] = new Instruction(Aby, Eor, 0x59, "EOR oper", 1);
		instructions[0x5A] = new Instruction(Unsupported, Unsupported, 0x5A, "Unsupported", 0);
		instructions[0x5B] = new Instruction(Unsupported, Unsupported, 0x5B, "Unsupported", 0);
		instructions[0x5C] = new Instruction(Unsupported, Unsupported, 0x5C, "Unsupported", 0);
		instructions[0x5D] = new Instruction(Abx, Eor, 0x5D, "EOR oper", 1);
		instructions[0x5E] = new Instruction(Abx, Lsr, 0x5E, "LSR oper", 1);
		instructions[0x5F] = new Instruction(Unsupported, Unsupported, 0x5F, "Unsupported", 0);

		// 6 -> 96 - 111
		instructions[0x60] = new Instruction(Imp, Rts, 0x60, "RTS oper", 1);
		instructions[0x61] = new Instruction(Inx, Adc, 0x61, "ADC oper", 1);
		instructions[0x62] = new Instruction(Unsupported, Unsupported, 0x62, "Unsupported", 0);
		instructions[0x63] = new Instruction(Unsupported, Unsupported, 0x63, "Unsupported", 0);
		instructions[0x64] = new Instruction(Unsupported, Unsupported, 0x64, "Unsupported", 0);
		instructions[0x65] = new Instruction(Zpg, Adc, 0x65, "ADC oper", 1);
		instructions[0x66] = new Instruction(Zpg, Ror, 0x66, "ROR oper", 1);
		instructions[0x67] = new Instruction(Unsupported, Unsupported, 0x67, "Unsupported", 0);
		instructions[0x68] = new Instruction(Imp, Pla, 0x68, "PLA oper", 1);
		instructions[0x69] = new Instruction(Imm, Adc, 0x69, "ADC oper", 1);
		instructions[0x6A] = new Instruction(Acc, Ror, 0x6A, "ROR oper", 1);
		instructions[0x6B] = new Instruction(Unsupported, Unsupported, 0x6B, "Unsupported", 0);
		instructions[0x6C] = new Instruction(Ind, Jmp, 0x6C, "JMP oper", 1);
		instructions[0x6D] = new Instruction(Abs, Adc, 0x6D, "ADC oper", 1);
		instructions[0x6E] = new Instruction(Abs, Ror, 0x6E, "ROR oper", 1);
		instructions[0x6F] = new Instruction(Unsupported, Unsupported, 0x6F, "Unsupported", 0);

		// 7 -> 112 - 127
		instructions[0x70] = new Instruction(Rel, Bvs, 0x70, "BVS oper", 1);
		instructions[0x71] = new Instruction(Iny, Adc, 0x71, "ADC oper", 1);
		instructions[0x72] = new Instruction(Unsupported, Unsupported, 0x72, "Unsupported", 0);
		instructions[0x73] = new Instruction(Unsupported, Unsupported, 0x73, "Unsupported", 0);
		instructions[0x74] = new Instruction(Unsupported, Unsupported, 0x74, "Unsupported", 0);
		instructions[0x75] = new Instruction(Zpx, Adc, 0x75, "ADC oper", 1);
		instructions[0x76] = new Instruction(Zpx, Ror, 0x76, "ROR oper", 1);
		instructions[0x77] = new Instruction(Unsupported, Unsupported, 0x77, "Unsupported", 0);
		instructions[0x78] = new Instruction(Imp, Sei, 0x78, "SEI oper", 1);
		instructions[0x79] = new Instruction(Aby, Adc, 0x79, "ADC oper", 1);
		instructions[0x7A] = new Instruction(Unsupported, Unsupported, 0x7A, "Unsupported", 0);
		instructions[0x7B] = new Instruction(Unsupported, Unsupported, 0x7B, "Unsupported", 0);
		instructions[0x7C] = new Instruction(Unsupported, Unsupported, 0x7C, "Unsupported", 0);
		instructions[0x7D] = new Instruction(Abx, Adc, 0x7D, "ADC oper", 1);
		instructions[0x7E] = new Instruction(Abx, Ror, 0x7E, "ROR oper", 1);
		instructions[0x7F] = new Instruction(Unsupported, Unsupported, 0x7F, "Unsupported", 0);

		// 8 -> 128 - 143
		instructions[0x80] = new Instruction(Unsupported, Unsupported, 0x80, "Unsupported", 0);
		instructions[0x81] = new Instruction(Inx, Sta, 0x81, "STA oper", 1);
		instructions[0x82] = new Instruction(Unsupported, Unsupported, 0x82, "Unsupported", 0);
		instructions[0x83] = new Instruction(Unsupported, Unsupported, 0x83, "Unsupported", 0);
		instructions[0x84] = new Instruction(Zpg, Sty, 0x84, "STY oper", 1);
		instructions[0x85] = new Instruction(Zpg, Sta, 0x85, "STA oper", 1);
		instructions[0x86] = new Instruction(Zpg, Stx, 0x86, "STX oper", 1);
		instructions[0x87] = new Instruction(Unsupported, Unsupported, 0x87, "Unsupported", 0);
		instructions[0x88] = new Instruction(Imp, Dey, 0x88, "DEY oper", 1);
		instructions[0x89] = new Instruction(Unsupported, Unsupported, 0x89, "Unsupported", 0);
		instructions[0x8A] = new Instruction(Imp, Txa, 0x8A, "TXA oper", 1);
		instructions[0x8B] = new Instruction(Unsupported, Unsupported, 0x8B, "Unsupported", 0);
		instructions[0x8C] = new Instruction(Abs, Sty, 0x8C, "STY oper", 1);
		instructions[0x8D] = new Instruction(Abs, Sta, 0x8D, "STA oper", 1);
		instructions[0x8E] = new Instruction(Abs, Stx, 0x8E, "STX oper", 1);
		instructions[0x8F] = new Instruction(Unsupported, Unsupported, 0x8F, "Unsupported", 0);

		// 9 -> 144 - 159
		instructions[0x90] = new Instruction(Rel, Bcc, 0x90, "BCC oper", 1);
		instructions[0x91] = new Instruction(Iny, Sta, 0x91, "STA oper", 1);
		instructions[0x92] = new Instruction(Unsupported, Unsupported, 0x92, "Unsupported", 0);
		instructions[0x93] = new Instruction(Unsupported, Unsupported, 0x93, "Unsupported", 0);
		instructions[0x94] = new Instruction(Zpx, Sty, 0x94, "STY oper", 1);
		instructions[0x95] = new Instruction(Zpx, Sta, 0x95, "STA oper", 1);
		instructions[0x96] = new Instruction(Zpy, Stx, 0x96, "STX oper", 1);
		instructions[0x97] = new Instruction(Unsupported, Unsupported, 0x97, "Unsupported", 0);
		instructions[0x98] = new Instruction(Imp, Tya, 0x98, "TYA oper", 1);
		instructions[0x99] = new Instruction(Aby, Sta, 0x99, "STA oper", 1);
		instructions[0x9A] = new Instruction(Imp, Txs, 0x9A, "TXS oper", 1);
		instructions[0x9B] = new Instruction(Unsupported, Unsupported, 0x9B, "Unsupported", 0);
		instructions[0x9C] = new Instruction(Unsupported, Unsupported, 0x9C, "Unsupported", 0);
		instructions[0x9D] = new Instruction(Abx, Sta, 0x9D, "STA oper", 1);
		instructions[0x9E] = new Instruction(Unsupported, Unsupported, 0x9E, "Unsupported", 0);
		instructions[0x9F] = new Instruction(Unsupported, Unsupported, 0x9F, "Unsupported", 0);

		// A -> 160 - 175
		instructions[0xA0] = new Instruction(Imm, Ldy, 0xA0, "LDY oper", 1);
		instructions[0xA1] = new Instruction(Inx, Lda, 0xA1, "LDA oper", 1);
		instructions[0xA2] = new Instruction(Imm, Ldx, 0xA2, "LDX oper", 1);
		instructions[0xA3] = new Instruction(Unsupported, Unsupported, 0xA3, "Unsupported", 0);
		instructions[0xA4] = new Instruction(Zpg, Ldy, 0xA4, "LDY oper", 1);
		instructions[0xA5] = new Instruction(Zpg, Lda, 0xA5, "LDA oper", 1);
		instructions[0xA6] = new Instruction(Zpg, Ldx, 0xA6, "LDX oper", 1);
		instructions[0xA7] = new Instruction(Unsupported, Unsupported, 0xA7, "Unsupported", 0);
		instructions[0xA8] = new Instruction(Imp, Tay, 0xA8, "TAY oper", 1);
		instructions[0xA9] = new Instruction(Imm, Lda, 0xA9, "LDA oper", 1);
		instructions[0xAA] = new Instruction(Imp, Tax, 0xAA, "TAX oper", 1);
		instructions[0xAB] = new Instruction(Unsupported, Unsupported, 0xAB, "Unsupported", 0);
		instructions[0xAC] = new Instruction(Abs, Ldy, 0xAC, "LDY oper", 1);
		instructions[0xAD] = new Instruction(Abs, Lda, 0xAD, "LDA oper", 1);
		instructions[0xAE] = new Instruction(Abs, Ldx, 0xAE, "LDX oper", 1);
		instructions[0xAF] = new Instruction(Unsupported, Unsupported, 0xAF, "Unsupported", 0);

		// B -> 176 - 191
		instructions[0xB0] = new Instruction(Rel, Bcs, 0xB0, "BCS oper", 1);
		instructions[0xB1] = new Instruction(Iny, Lda, 0xB1, "LDA oper", 1);
		instructions[0xB2] = new Instruction(Unsupported, Unsupported, 0xB2, "Unsupported", 0);
		instructions[0xB3] = new Instruction(Unsupported, Unsupported, 0xB3, "Unsupported", 0);
		instructions[0xB4] = new Instruction(Zpx, Ldy, 0xB4, "LDY oper", 1);
		instructions[0xB5] = new Instruction(Zpx, Lda, 0xB5, "LDA oper", 1);
		instructions[0xB6] = new Instruction(Zpy, Ldx, 0xB6, "LDX oper", 1);
		instructions[0xB7] = new Instruction(Unsupported, Unsupported, 0xB7, "Unsupported", 0);
		instructions[0xB8] = new Instruction(Imp, Clv, 0xB8, "CLV oper", 1);
		instructions[0xB9] = new Instruction(Aby, Lda, 0xB9, "LDA oper", 1);
		instructions[0xBA] = new Instruction(Imp, Tsx, 0xBA, "TSX oper", 1);
		instructions[0xBB] = new Instruction(Unsupported, Unsupported, 0xBB, "Unsupported", 0);
		instructions[0xBC] = new Instruction(Abx, Ldy, 0xBC, "LDY oper", 1);
		instructions[0xBD] = new Instruction(Abx, Lda, 0xBD, "LDA oper", 1);
		instructions[0xBE] = new Instruction(Aby, Ldx, 0xBE, "LDX oper", 1);
		instructions[0xBF] = new Instruction(Unsupported, Unsupported, 0xBF, "Unsupported", 0);

		// C -> 192 - 207
		instructions[0xC0] = new Instruction(Imm, Cpy, 0xC0, "CPY oper", 1);
		instructions[0xC1] = new Instruction(Inx, Cmp, 0xC1, "CMP oper", 1);
		instructions[0xC2] = new Instruction(Unsupported, Unsupported, 0xC2, "Unsupported", 0);
		instructions[0xC3] = new Instruction(Unsupported, Unsupported, 0xC3, "Unsupported", 0);
		instructions[0xC4] = new Instruction(Zpg, Cpy, 0xC4, "CPY oper", 1);
		instructions[0xC5] = new Instruction(Zpg, Cmp, 0xC5, "CMP oper", 1);
		instructions[0xC6] = new Instruction(Zpg, Dec, 0xC6, "DEC oper", 1);
		instructions[0xC7] = new Instruction(Unsupported, Unsupported, 0xC7, "Unsupported", 0);
		instructions[0xC8] = new Instruction(Imp, Iny, 0xC8, "INY oper", 1);
		instructions[0xC9] = new Instruction(Imm, Cmp, 0xC9, "CMP oper", 1);
		instructions[0xCA] = new Instruction(Imp, Dex, 0xCA, "DEX oper", 1);
		instructions[0xCB] = new Instruction(Unsupported, Unsupported, 0xCB, "Unsupported", 0);
		instructions[0xCC] = new Instruction(Abs, Cpy, 0xCC, "CPY oper", 1);
		instructions[0xCD] = new Instruction(Abs, Cmp, 0xCD, "CMP oper", 1);
		instructions[0xCE] = new Instruction(Abs, Dec, 0xCE, "DEC oper", 1);
		instructions[0xCF] = new Instruction(Unsupported, Unsupported, 0xCF, "Unsupported", 0);

		// D -> 208 - 223
		instructions[0xD0] = new Instruction(Rel, Bne, 0xD0, "BNE oper", 1);
		instructions[0xD1] = new Instruction(Iny, Cmp, 0xD1, "CMP oper", 1);
		instructions[0xD2] = new Instruction(Unsupported, Unsupported, 0xD2, "Unsupported", 0);
		instructions[0xD3] = new Instruction(Unsupported, Unsupported, 0xD3, "Unsupported", 0);
		instructions[0xD4] = new Instruction(Unsupported, Unsupported, 0xD4, "Unsupported", 0);
		instructions[0xD5] = new Instruction(Zpx, Cmp, 0xD5, "CMP oper", 1);
		instructions[0xD6] = new Instruction(Zpx, Dec, 0xD6, "DEC oper", 1);
		instructions[0xD7] = new Instruction(Unsupported, Unsupported, 0xD7, "Unsupported", 0);
		instructions[0xD8] = new Instruction(Imp, Cld, 0xD8, "CLD oper", 1);
		instructions[0xD9] = new Instruction(Aby, Cmp, 0xD9, "CMP oper", 1);
		instructions[0xDA] = new Instruction(Unsupported, Unsupported, 0xDA, "Unsupported", 0);
		instructions[0xDB] = new Instruction(Unsupported, Unsupported, 0xDB, "Unsupported", 0);
		instructions[0xDC] = new Instruction(Unsupported, Unsupported, 0xDC, "Unsupported", 0);
		instructions[0xDD] = new Instruction(Abx, Cmp, 0xDD, "CMP oper", 1);
		instructions[0xDE] = new Instruction(Abx, Dec, 0xDE, "DEC oper", 1);
		instructions[0xDF] = new Instruction(Unsupported, Unsupported, 0xDF, "Unsupported", 0);

		// E -> 224 - 239
		instructions[0xE0] = new Instruction(Imm, Cpx, 0xE0, "CPX oper", 1);
		instructions[0xE1] = new Instruction(Inx, Sbc, 0xE1, "SBC oper", 1);
		instructions[0xE2] = new Instruction(Unsupported, Unsupported, 0xE2, "Unsupported", 0);
		instructions[0xE3] = new Instruction(Unsupported, Unsupported, 0xE3, "Unsupported", 0);
		instructions[0xE4] = new Instruction(Zpg, Cpx, 0xE4, "CPX oper", 1);
		instructions[0xE5] = new Instruction(Zpg, Sbc, 0xE5, "SBC oper", 1);
		instructions[0xE6] = new Instruction(Zpg, Inc, 0xE6, "INC oper", 1);
		instructions[0xE7] = new Instruction(Unsupported, Unsupported, 0xE7, "Unsupported", 0);
		instructions[0xE8] = new Instruction(Imp, Inx, 0xE8, "INX oper", 1);
		instructions[0xE9] = new Instruction(Imm, Sbc, 0xE9, "SBC oper", 1);
		instructions[0xEA] = new Instruction(Imp, Nop, 0xEA, "NOP oper", 1);
		instructions[0xEB] = new Instruction(Unsupported, Unsupported, 0xEB, "Unsupported", 0);
		instructions[0xEC] = new Instruction(Abs, Cpx, 0xEC, "CPX oper", 1);
		instructions[0xED] = new Instruction(Abs, Sbc, 0xED, "SBC oper", 1);
		instructions[0xEE] = new Instruction(Abs, Inc, 0xEE, "INC oper", 1);
		instructions[0xEF] = new Instruction(Unsupported, Unsupported, 0xEF, "Unsupported", 0);

		// F -> 240 - 255
		instructions[0xF0] = new Instruction(Rel, Beq, 0xF0, "BEQ oper", 1);
		instructions[0xF1] = new Instruction(Iny, Sbc, 0xF1, "SBC oper", 1);
		instructions[0xF2] = new Instruction(Unsupported, Unsupported, 0xF2, "Unsupported", 0);
		instructions[0xF3] = new Instruction(Unsupported, Unsupported, 0xF3, "Unsupported", 0);
		instructions[0xF4] = new Instruction(Unsupported, Unsupported, 0xF4, "Unsupported", 0);
		instructions[0xF5] = new Instruction(Zpx, Sbc, 0xF5, "SBC oper", 1);
		instructions[0xF6] = new Instruction(Zpx, Inc, 0xF6, "INC oper", 1);
		instructions[0xF7] = new Instruction(Unsupported, Unsupported, 0xF7, "Unsupported", 0);
		instructions[0xF8] = new Instruction(Imp, Sed, 0xF8, "SED oper", 1);
		instructions[0xF9] = new Instruction(Aby, Sbc, 0xF9, "SBC oper", 1);
		instructions[0xFA] = new Instruction(Unsupported, Unsupported, 0xFA, "Unsupported", 0);
		instructions[0xFB] = new Instruction(Unsupported, Unsupported, 0xFB, "Unsupported", 0);
		instructions[0xFC] = new Instruction(Unsupported, Unsupported, 0xFC, "Unsupported", 0);
		instructions[0xFD] = new Instruction(Abx, Sbc, 0xFD, "SBC oper", 1);
		instructions[0xFE] = new Instruction(Abx, Inc, 0xFE, "INC oper", 1);
		instructions[0xFF] = new Instruction(Unsupported, Unsupported, 0xFF, "Unsupported", 0);

		return instructions;
	}
}