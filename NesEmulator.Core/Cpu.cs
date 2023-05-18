namespace NesEmulator.Core;

public sealed class Cpu
{
	private const uint ByteMask = 0x0000_00FF;
	private const uint WordMask = 0x0000_FFFF;

	private const ushort IrqVectorL = 0xFFFE;
	private const ushort IrqVectorH = 0xFFFF;

	private readonly Instruction[] _instructions = new Instruction[256];
	private readonly IMemory _memory;

	private uint _a;
	private uint _x;
	private uint _y;
	private uint _sr;
	private uint _sp;
	private uint _pc;

	private uint _cycles;
	private uint _address;
	private int _relAddress;

	public Cpu(IMemory memory,
		uint a = 0x00, uint x = 0x00, uint y = 0x00,
		uint sr = 0x00, uint sp = 0x00, uint pc = 0x0000)
	{
		CreateInstructions();

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
		_cycles = instruction.Execute();
	}

	#region Modes

	private bool AccMode()
	{
		_address = 0x0000;

		return false;
	}

	private bool AbsMode()
	{
		_address = NextWord();

		return false;
	}

	private bool AbxMode()
	{
		var word = NextWord();

		_address = word + X;

		return IsPageCrossed(word, _address);
	}

	private bool AbyMode()
	{
		var word = NextWord();

		_address = word + Y;

		return IsPageCrossed(word, _address);
	}

	private bool ImmMode()
	{
		_address = PC++;

		return false;
	}

	private bool ImpMode()
	{
		_address = 0x0000;

		return false;
	}

	private bool IndMode()
	{
		var ptr = NextWord();

		if (LoByte(ptr) != 0xFF)
			_address = ReadWord(ptr);
		else
		{
			var lo = ReadByte(ptr);
			var hi = ReadByte((ptr + 1) & 0xFF00);

			_address = (hi << 8) | lo;
		}

		return false;
	}

	private bool InxMode()
	{
		var ptr = NextByte();

		_address = ReadWord(ptr + X);

		return false;
	}

	private bool InyMode()
	{
		var ptr = NextByte();
		var word = ReadWord(ptr);

		_address = word + Y;

		return IsPageCrossed(word, _address);
	}

	private bool RelMode()
	{
		_relAddress = (int)NextByte();

		return false;
	}

	private bool ZpgMode()
	{
		_address = NextByte();

		return false;
	}

	private bool ZpxMode()
	{
		_address = NextByte() + X;

		return false;
	}

	private bool ZpyMode()
	{
		_address = NextByte() + Y;

		return false;
	}

	#endregion

	#region Operations

	// Transfer Instructions (12)
	private bool Lda()
	{
		A = ReadByte(_address);

		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);

		return true;
	}

	private bool Ldx()
	{
		X = ReadByte(_address);

		ZeroFlag = IsZero(X);
		NegativeFlag = IsNegative(X);

		return true;
	}

	private bool Ldy()
	{
		Y = ReadByte(_address);

		ZeroFlag = IsZero(Y);
		NegativeFlag = IsNegative(Y);

		return true;
	}

	private bool Sta()
	{
		WriteByte(_address, A);

		return false;
	}

	private bool Stx()
	{
		WriteByte(_address, X);

		return false;
	}

	private bool Sty()
	{
		WriteByte(_address, Y);

		return false;
	}

	private bool Tax()
	{
		X = A;

		ZeroFlag = IsZero(X);
		NegativeFlag = IsNegative(X);

		return false;
	}

	private bool Tay()
	{
		Y = A;

		ZeroFlag = IsZero(Y);
		NegativeFlag = IsNegative(Y);

		return false;
	}

	private bool Tsx()
	{
		X = SP;

		ZeroFlag = IsZero(X);
		NegativeFlag = IsNegative(X);

		return false;
	}

	private bool Txa()
	{
		A = X;

		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);

		return false;
	}

	private bool Txs()
	{
		SP = X;

		return false;
	}

	private bool Tya()
	{
		A = Y;

		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);

		return false;
	}

	// Stack Instructions (4)
	private bool Pha()
	{
		StackPush(A);

		return false;
	}

	private bool Php()
	{
		StackPush(SR);

		return false;
	}

	private bool Pla()
	{
		A = StackPop();

		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);

		return false;
	}

	private bool Plp()
	{
		SR = StackPop();

		return false;
	}

	// Decrements & Increments (6)
	private bool Dec()
	{
		var value = ReadByte(_address) - 1;
		WriteByte(_address, value);

		ZeroFlag = IsZero(value);
		NegativeFlag = IsNegative(value);

		return false;
	}

	private bool Dex()
	{
		X -= 1;

		ZeroFlag = IsZero(X);
		NegativeFlag = IsNegative(X);

		return false;
	}

	private bool Dey()
	{
		Y -= 1;

		ZeroFlag = IsZero(Y);
		NegativeFlag = IsNegative(Y);

		return false;
	}

	private bool Inc()
	{
		var value = ReadByte(_address) + 1;
		WriteByte(_address, value);

		ZeroFlag = IsZero(value);
		NegativeFlag = IsNegative(value);

		return false;
	}

	private bool Inx()
	{
		X += 1;

		ZeroFlag = IsZero(X);
		NegativeFlag = IsNegative(X);

		return false;
	}

	private bool Iny()
	{
		Y += 1;

		ZeroFlag = IsZero(Y);
		NegativeFlag = IsNegative(Y);

		return false;
	}

	// Arithmetic Operations (2)
	private bool Adc()
	{
		var memory = ReadByte(_address);
		var result = A + memory + (CarryFlag ? 1U : 0U);

		OverflowFlag = ((~(A ^ memory) & (A ^ result)) & 0x0080) != 0x00;

		A = result;

		CarryFlag = HiByte(result) != 0x00;
		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);

		return true;
	}

	private bool Sbc()
	{
		var memory = ReadByte(_address);
		var result = A - memory - (CarryFlag ? 1U : 0U);

		OverflowFlag = ((~(A ^ memory) & (A ^ result)) & 0x0080) != 0x00;

		A = result;

		CarryFlag = HiByte(result) != 0x00;
		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);

		return true;
	}

	// Logical Operations (3)
	private bool And()
	{
		A &= ReadByte(_address);

		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);

		return true;
	}

	private bool Eor()
	{
		A ^= ReadByte(_address);

		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);

		return true;
	}

	private bool Ora()
	{
		A |= ReadByte(_address);

		ZeroFlag = IsZero(A);
		NegativeFlag = IsNegative(A);

		return true;
	}

	// Shift & Rotate Instructions (4)
	private bool Asl()
	{
		var value = ReadByte(_address);

		value <<= 1;

		CarryFlag = (value & 0xFF00) != 0x00;
		ZeroFlag = IsZero(value);
		NegativeFlag = IsNegative(value);

		WriteByte(_address, value);

		return false;
	}

	private bool AslA()
	{
		var value = A;

		value <<= 1;

		CarryFlag = (value & 0xFF00) != 0x00;
		ZeroFlag = IsZero(value);
		NegativeFlag = IsNegative(value);

		A = value;

		return false;
	}

	private bool Lsr()
	{
		var value = ReadByte(_address);

		CarryFlag = (value & 0x0001) != 0x0000;

		value >>= 1;

		ZeroFlag = IsZero(value);
		NegativeFlag = IsNegative(value);

		WriteByte(_address, value);

		return false;
	}

	private bool LsrA()
	{
		var value = A;

		CarryFlag = (value & 0x0001) != 0x0000;

		value >>= 1;

		ZeroFlag = IsZero(value);
		NegativeFlag = IsNegative(value);

		A = value;

		return false;
	}

	private bool Rol()
	{
		var value = ReadByte(_address);

		value = (value << 1) | (CarryFlag ? 1U : 0U);

		CarryFlag = (value & 0xFF00) != 0x0000;
		ZeroFlag = IsZero(value);
		NegativeFlag = IsNegative(value);

		WriteByte(_address, value);

		return false;
	}

	private bool RolA()
	{
		var value = A;

		value = (value << 1) | (CarryFlag ? 1U : 0U);

		CarryFlag = (value & 0xFF00) != 0x0000;
		ZeroFlag = IsZero(value);
		NegativeFlag = IsNegative(value);

		A = value;

		return false;
	}

	private bool Ror()
	{
		var value = ReadByte(_address);

		var result = (value >> 1) | (CarryFlag ? 1U : 0U) << 7;

		CarryFlag = (value & 0x0001) != 0x0000;
		ZeroFlag = IsZero(value);
		NegativeFlag = IsNegative(value);

		WriteByte(_address, result);

		return false;
	}

	private bool RorA()
	{
		var value = A;

		var result = (value >> 1) | (CarryFlag ? 1U : 0U) << 7;

		CarryFlag = (value & 0x0001) != 0x0000;
		ZeroFlag = IsZero(value);
		NegativeFlag = IsNegative(value);

		A = result;

		return false;
	}

	// Flag Instructions (7)
	private bool Clc()
	{
		CarryFlag = false;

		return false;
	}

	private bool Cld()
	{
		DecimalFlag = false;

		return false;
	}

	private bool Cli()
	{
		InterruptFlag = false;

		return false;
	}

	private bool Clv()
	{
		OverflowFlag = false;

		return false;
	}

	private bool Sec()
	{
		CarryFlag = true;

		return false;
	}

	private bool Sed()
	{
		DecimalFlag = true;

		return false;
	}

	private bool Sei()
	{
		InterruptFlag = true;

		return false;
	}

	// Comparison (3)
	private bool Cmp()
	{
		var value = ReadByte(_address);
		var cmp = A - value;

		CarryFlag = A >= value;
		ZeroFlag = IsZero(cmp);
		NegativeFlag = IsNegative(cmp);

		return true;
	}

	private bool Cpx()
	{
		var value = ReadByte(_address);
		var cmp = X - value;

		CarryFlag = X >= value;
		ZeroFlag = IsZero(cmp);
		NegativeFlag = IsNegative(cmp);

		return true;
	}

	private bool Cpy()
	{
		var value = ReadByte(_address);
		var cmp = Y - value;

		CarryFlag = Y >= value;
		ZeroFlag = IsZero(cmp);
		NegativeFlag = IsNegative(cmp);

		return true;
	}

	// Conditional Branch Instructions (8)
	private bool Bcc()
	{
		if (CarryFlag) return false;

		_cycles++;

		var newAddress = (uint)(PC + _relAddress);

		if (HiByte(PC) != HiByte(newAddress))
			_cycles++;

		PC = newAddress;

		return false;
	}

	private bool Bcs()
	{
		if (!CarryFlag) return false;

		_cycles++;

		var newAddress = (uint)(PC + _relAddress);

		if (HiByte(PC) != HiByte(newAddress))
			_cycles++;

		PC = newAddress;

		return false;
	}

	private bool Beq()
	{
		if (!ZeroFlag) return false;

		_cycles++;

		var newAddress = (uint)(PC + _relAddress);

		if (HiByte(PC) != HiByte(newAddress))
			_cycles++;

		PC = newAddress;

		return false;
	}

	private bool Bmi()
	{
		if (!NegativeFlag) return false;

		_cycles++;

		var newAddress = (uint)(PC + _relAddress);

		if (HiByte(PC) != HiByte(newAddress))
			_cycles++;

		PC = newAddress;

		return false;
	}

	private bool Bne()
	{
		if (ZeroFlag) return false;

		_cycles++;

		var newAddress = (uint)(PC + _relAddress);

		if (HiByte(PC) != HiByte(newAddress))
			_cycles++;

		PC = newAddress;

		return false;
	}

	private bool Bpl()
	{
		if (NegativeFlag) return false;

		_cycles++;

		var newAddress = (uint)(PC + _relAddress);

		if (HiByte(PC) != HiByte(newAddress))
			_cycles++;

		PC = newAddress;

		return false;
	}

	private bool Bvc()
	{
		if (OverflowFlag) return false;

		_cycles++;

		var newAddress = (uint)(PC + _relAddress);

		if (HiByte(PC) != HiByte(newAddress))
			_cycles++;

		PC = newAddress;

		return false;
	}

	private bool Bvs()
	{
		if (!OverflowFlag) return false;

		_cycles++;

		var newAddress = (uint)(PC + _relAddress);

		if (HiByte(PC) != HiByte(newAddress))
			_cycles++;

		PC = newAddress;

		return false;
	}

	// Jumps & Subroutines (3)
	private bool Jmp()
	{
		PC = _address;

		return false;
	}

	private bool Jsr()
	{
		PC--;

		StackPush(HiByte(PC));
		StackPush(LoByte(PC));

		PC = _address;

		return false;
	}

	private bool Rts()
	{
		var low = StackPop();
		var high = StackPop();

		PC = (high << 8) | low;
		PC++;

		return false;
	}

	// Interrupts (2)
	private bool Brk()
	{
		PC++;

		StackPush(HiByte(PC));
		StackPush(LoByte(PC));

		StackPush(SR);

		var low = ReadByte(IrqVectorL);
		var high = ReadByte(IrqVectorH);

		PC = (high << 8) | low;

		InterruptFlag = true;

		return false;
	}

	private bool Rti()
	{
		var value = StackPop();

		CarryFlag = (value & 0x01) != 0x0;
		ZeroFlag = (value & 0x02) != 0x0;
		InterruptFlag = (value & 0x04) != 0x0;
		DecimalFlag = (value & 0x08) != 0x0;
		OverflowFlag = (value & 0x40) != 0x0;
		NegativeFlag = (value & 0x80) != 0x0;

		var low = StackPop();
		var high = StackPop();

		PC = (high << 8) | low;

		return false;
	}

	// Other (2)
	private bool Bit()
	{
		var value = ReadByte(_address);

		var result = A & value;

		ZeroFlag = IsZero(result);
		OverflowFlag = (value & 0x40) != 0x00;
		NegativeFlag = (value & 0x80) != 0x00;

		return false;
	}

	private static bool Nop() => false;

	private static bool Unsupported() => false;

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

	private void StackPush(uint value)
	{
	}

	private uint StackPop()
	{
		return 0x00;
	}

	private static bool IsZero(uint value) => value == 0x00;
	private static bool IsNegative(uint value) => (value & 0x80) != 0x00;
	private static bool IsPageCrossed(uint left, uint right) => false;
	private static uint HiByte(uint value) => 0x00;
	private static uint LoByte(uint value) => 0x00;

	#endregion

	private void CreateInstructions()
	{
		// 0 -> 0 - 15
		_instructions[0x00] = new Instruction(ImpMode, Brk, 0x00, "BRK oper", 1);
		_instructions[0x01] = new Instruction(InxMode, Ora, 0x01, "ORA oper", 1);
		_instructions[0x02] = new Instruction(Unsupported, Unsupported, 0x02, "unsupported", 0);
		_instructions[0x03] = new Instruction(Unsupported, Unsupported, 0x03, "unsupported", 0);
		_instructions[0x04] = new Instruction(Unsupported, Unsupported, 0x04, "unsupported", 0);
		_instructions[0x05] = new Instruction(ZpgMode, Ora, 0x05, "ORA oper", 1);
		_instructions[0x06] = new Instruction(ZpgMode, Asl, 0x06, "ASL oper", 1);
		_instructions[0x07] = new Instruction(Unsupported, Unsupported, 0x07, "unsupported", 0);
		_instructions[0x08] = new Instruction(ImpMode, Php, 0x08, "PHP oper", 1);
		_instructions[0x09] = new Instruction(ImmMode, Ora, 0x09, "ORA oper", 1);
		_instructions[0x0A] = new Instruction(AccMode, AslA, 0x0A, "ASL oper", 1);
		_instructions[0x0B] = new Instruction(Unsupported, Unsupported, 0x0B, "unsupported", 0);
		_instructions[0x0C] = new Instruction(Unsupported, Unsupported, 0x0C, "unsupported", 0);
		_instructions[0x0D] = new Instruction(AbsMode, Ora, 0x0D, "ORA oper", 1);
		_instructions[0x0E] = new Instruction(AbsMode, Asl, 0x0E, "ASL oper", 1);
		_instructions[0x0F] = new Instruction(Unsupported, Unsupported, 0x0F, "unsupported", 0);

		// 1 -> 16 - 31
		_instructions[0x10] = new Instruction(RelMode, Bpl, 0x10, "REL oper", 1);
		_instructions[0x11] = new Instruction(InyMode, Ora, 0x11, "INY oper", 1);
		_instructions[0x12] = new Instruction(Unsupported, Unsupported, 0x12, "unsupported", 0);
		_instructions[0x13] = new Instruction(Unsupported, Unsupported, 0x13, "unsupported", 0);
		_instructions[0x14] = new Instruction(Unsupported, Unsupported, 0x14, "unsupported", 0);
		_instructions[0x15] = new Instruction(ZpxMode, Ora, 0x15, "ZPX oper", 1);
		_instructions[0x16] = new Instruction(ZpxMode, Asl, 0x16, "ZPX oper", 1);
		_instructions[0x17] = new Instruction(Unsupported, Unsupported, 0x17, "unsupported", 0);
		_instructions[0x18] = new Instruction(ImpMode, Clc, 0x18, "IMP oper", 1);
		_instructions[0x19] = new Instruction(AbyMode, Ora, 0x19, "ABY oper", 1);
		_instructions[0x1A] = new Instruction(Unsupported, Unsupported, 0x1A, "unsupported", 0);
		_instructions[0x1B] = new Instruction(Unsupported, Unsupported, 0x1B, "unsupported", 0);
		_instructions[0x1C] = new Instruction(Unsupported, Unsupported, 0x1C, "unsupported", 0);
		_instructions[0x1D] = new Instruction(AbxMode, Ora, 0x1D, "ABX oper", 1);
		_instructions[0x1E] = new Instruction(AbxMode, Asl, 0x1E, "ABX oper", 1);
		_instructions[0x1F] = new Instruction(Unsupported, Unsupported, 0x1F, "unsupported", 0);

		// 2 -> 32 - 47
		_instructions[0x20] = new Instruction(AbsMode, Jsr, 0x20, "JSR oper", 1);
		_instructions[0x21] = new Instruction(InxMode, And, 0x21, "AND oper", 1);
		_instructions[0x22] = new Instruction(Unsupported, Unsupported, 0x22, "Unsupported", 0);
		_instructions[0x23] = new Instruction(Unsupported, Unsupported, 0x23, "Unsupported", 0);
		_instructions[0x24] = new Instruction(ZpgMode, Bit, 0x24, "BIT oper", 1);
		_instructions[0x25] = new Instruction(ZpgMode, And, 0x25, "AND oper", 1);
		_instructions[0x26] = new Instruction(ZpgMode, Rol, 0x26, "ROL oper", 1);
		_instructions[0x27] = new Instruction(Unsupported, Unsupported, 0x27, "Unsupported", 0);
		_instructions[0x28] = new Instruction(ImpMode, Plp, 0x28, "PLP oper", 1);
		_instructions[0x29] = new Instruction(ImmMode, And, 0x29, "AND oper", 1);
		_instructions[0x2A] = new Instruction(AccMode, RolA, 0x2A, "ROL oper", 1);
		_instructions[0x2B] = new Instruction(Unsupported, Unsupported, 0x2B, "Unsupported", 0);
		_instructions[0x2C] = new Instruction(AbsMode, Bit, 0x2C, "BIT oper", 1);
		_instructions[0x2D] = new Instruction(AbsMode, And, 0x2D, "AND oper", 1);
		_instructions[0x2E] = new Instruction(AbsMode, Rol, 0x2E, "ROL oper", 1);
		_instructions[0x2F] = new Instruction(Unsupported, Unsupported, 0x2F, "Unsupported", 0);

		// 3 -> 48 - 63
		_instructions[0x30] = new Instruction(RelMode, Bmi, 0x30, "BMI oper", 1);
		_instructions[0x31] = new Instruction(InyMode, And, 0x31, "AND oper", 1);
		_instructions[0x32] = new Instruction(Unsupported, Unsupported, 0x32, "Unsupported", 0);
		_instructions[0x33] = new Instruction(Unsupported, Unsupported, 0x33, "Unsupported", 0);
		_instructions[0x34] = new Instruction(Unsupported, Unsupported, 0x34, "Unsupported", 0);
		_instructions[0x35] = new Instruction(ZpxMode, And, 0x35, "AND oper", 1);
		_instructions[0x36] = new Instruction(ZpxMode, Rol, 0x36, "ROL oper", 1);
		_instructions[0x37] = new Instruction(Unsupported, Unsupported, 0x37, "Unsupported", 0);
		_instructions[0x38] = new Instruction(ImpMode, Sec, 0x38, "SEC oper", 1);
		_instructions[0x39] = new Instruction(AbyMode, And, 0x39, "AND oper", 1);
		_instructions[0x3A] = new Instruction(Unsupported, Unsupported, 0x3A, "Unsupported", 0);
		_instructions[0x3B] = new Instruction(Unsupported, Unsupported, 0x3B, "Unsupported", 0);
		_instructions[0x3C] = new Instruction(Unsupported, Unsupported, 0x3C, "Unsupported", 0);
		_instructions[0x3D] = new Instruction(AbxMode, And, 0x3D, "AND oper", 1);
		_instructions[0x3E] = new Instruction(AbxMode, Rol, 0x3E, "ROL oper", 1);
		_instructions[0x3F] = new Instruction(Unsupported, Unsupported, 0x3F, "Unsupported", 0);

		// 4 -> 64 - 79
		_instructions[0x40] = new Instruction(ImpMode, Rti, 0x40, "RTI oper", 1);
		_instructions[0x41] = new Instruction(InxMode, Eor, 0x41, "EOR oper", 1);
		_instructions[0x42] = new Instruction(Unsupported, Unsupported, 0x42, "Unsupported", 0);
		_instructions[0x43] = new Instruction(Unsupported, Unsupported, 0x43, "Unsupported", 0);
		_instructions[0x44] = new Instruction(Unsupported, Unsupported, 0x44, "Unsupported", 0);
		_instructions[0x45] = new Instruction(ZpgMode, Eor, 0x45, "EOR oper", 1);
		_instructions[0x46] = new Instruction(ZpgMode, Lsr, 0x46, "LSR oper", 1);
		_instructions[0x47] = new Instruction(Unsupported, Unsupported, 0x47, "Unsupported", 0);
		_instructions[0x48] = new Instruction(ImpMode, Pha, 0x48, "PHA oper", 1);
		_instructions[0x49] = new Instruction(ImmMode, Eor, 0x49, "EOR oper", 1);
		_instructions[0x4A] = new Instruction(AccMode, LsrA, 0x4A, "LSR oper", 1);
		_instructions[0x4B] = new Instruction(Unsupported, Unsupported, 0x4B, "Unsupported", 0);
		_instructions[0x4C] = new Instruction(AbsMode, Jmp, 0x4C, "JMP oper", 1);
		_instructions[0x4D] = new Instruction(AbsMode, Eor, 0x4D, "EOR oper", 1);
		_instructions[0x4E] = new Instruction(AbsMode, Lsr, 0x4E, "LSR oper", 1);
		_instructions[0x4F] = new Instruction(Unsupported, Unsupported, 0x4F, "Unsupported", 0);

		// 5 -> 80 - 95
		_instructions[0x50] = new Instruction(RelMode, Bvc, 0x50, "BVC oper", 1);
		_instructions[0x51] = new Instruction(InyMode, Eor, 0x51, "EOR oper", 1);
		_instructions[0x52] = new Instruction(Unsupported, Unsupported, 0x52, "Unsupported", 0);
		_instructions[0x53] = new Instruction(Unsupported, Unsupported, 0x53, "Unsupported", 0);
		_instructions[0x54] = new Instruction(Unsupported, Unsupported, 0x54, "Unsupported", 0);
		_instructions[0x55] = new Instruction(ZpxMode, Eor, 0x55, "EOR oper", 1);
		_instructions[0x56] = new Instruction(ZpxMode, Lsr, 0x56, "LSR oper", 1);
		_instructions[0x57] = new Instruction(Unsupported, Unsupported, 0x57, "Unsupported", 0);
		_instructions[0x58] = new Instruction(ImpMode, Cli, 0x58, "CLI oper", 1);
		_instructions[0x59] = new Instruction(AbyMode, Eor, 0x59, "EOR oper", 1);
		_instructions[0x5A] = new Instruction(Unsupported, Unsupported, 0x5A, "Unsupported", 0);
		_instructions[0x5B] = new Instruction(Unsupported, Unsupported, 0x5B, "Unsupported", 0);
		_instructions[0x5C] = new Instruction(Unsupported, Unsupported, 0x5C, "Unsupported", 0);
		_instructions[0x5D] = new Instruction(AbxMode, Eor, 0x5D, "EOR oper", 1);
		_instructions[0x5E] = new Instruction(AbxMode, Lsr, 0x5E, "LSR oper", 1);
		_instructions[0x5F] = new Instruction(Unsupported, Unsupported, 0x5F, "Unsupported", 0);

		// 6 -> 96 - 111
		_instructions[0x60] = new Instruction(ImpMode, Rts, 0x60, "RTS oper", 1);
		_instructions[0x61] = new Instruction(InxMode, Adc, 0x61, "ADC oper", 1);
		_instructions[0x62] = new Instruction(Unsupported, Unsupported, 0x62, "Unsupported", 0);
		_instructions[0x63] = new Instruction(Unsupported, Unsupported, 0x63, "Unsupported", 0);
		_instructions[0x64] = new Instruction(Unsupported, Unsupported, 0x64, "Unsupported", 0);
		_instructions[0x65] = new Instruction(ZpgMode, Adc, 0x65, "ADC oper", 1);
		_instructions[0x66] = new Instruction(ZpgMode, Ror, 0x66, "ROR oper", 1);
		_instructions[0x67] = new Instruction(Unsupported, Unsupported, 0x67, "Unsupported", 0);
		_instructions[0x68] = new Instruction(ImpMode, Pla, 0x68, "PLA oper", 1);
		_instructions[0x69] = new Instruction(ImmMode, Adc, 0x69, "ADC oper", 1);
		_instructions[0x6A] = new Instruction(AccMode, RorA, 0x6A, "ROR oper", 1);
		_instructions[0x6B] = new Instruction(Unsupported, Unsupported, 0x6B, "Unsupported", 0);
		_instructions[0x6C] = new Instruction(IndMode, Jmp, 0x6C, "JMP oper", 1);
		_instructions[0x6D] = new Instruction(AbsMode, Adc, 0x6D, "ADC oper", 1);
		_instructions[0x6E] = new Instruction(AbsMode, Ror, 0x6E, "ROR oper", 1);
		_instructions[0x6F] = new Instruction(Unsupported, Unsupported, 0x6F, "Unsupported", 0);

		// 7 -> 112 - 127
		_instructions[0x70] = new Instruction(RelMode, Bvs, 0x70, "BVS oper", 1);
		_instructions[0x71] = new Instruction(InyMode, Adc, 0x71, "ADC oper", 1);
		_instructions[0x72] = new Instruction(Unsupported, Unsupported, 0x72, "Unsupported", 0);
		_instructions[0x73] = new Instruction(Unsupported, Unsupported, 0x73, "Unsupported", 0);
		_instructions[0x74] = new Instruction(Unsupported, Unsupported, 0x74, "Unsupported", 0);
		_instructions[0x75] = new Instruction(ZpxMode, Adc, 0x75, "ADC oper", 1);
		_instructions[0x76] = new Instruction(ZpxMode, Ror, 0x76, "ROR oper", 1);
		_instructions[0x77] = new Instruction(Unsupported, Unsupported, 0x77, "Unsupported", 0);
		_instructions[0x78] = new Instruction(ImpMode, Sei, 0x78, "SEI oper", 1);
		_instructions[0x79] = new Instruction(AbyMode, Adc, 0x79, "ADC oper", 1);
		_instructions[0x7A] = new Instruction(Unsupported, Unsupported, 0x7A, "Unsupported", 0);
		_instructions[0x7B] = new Instruction(Unsupported, Unsupported, 0x7B, "Unsupported", 0);
		_instructions[0x7C] = new Instruction(Unsupported, Unsupported, 0x7C, "Unsupported", 0);
		_instructions[0x7D] = new Instruction(AbxMode, Adc, 0x7D, "ADC oper", 1);
		_instructions[0x7E] = new Instruction(AbxMode, Ror, 0x7E, "ROR oper", 1);
		_instructions[0x7F] = new Instruction(Unsupported, Unsupported, 0x7F, "Unsupported", 0);

		// 8 -> 128 - 143
		_instructions[0x80] = new Instruction(Unsupported, Unsupported, 0x80, "Unsupported", 0);
		_instructions[0x81] = new Instruction(InxMode, Sta, 0x81, "STA oper", 1);
		_instructions[0x82] = new Instruction(Unsupported, Unsupported, 0x82, "Unsupported", 0);
		_instructions[0x83] = new Instruction(Unsupported, Unsupported, 0x83, "Unsupported", 0);
		_instructions[0x84] = new Instruction(ZpgMode, Sty, 0x84, "STY oper", 1);
		_instructions[0x85] = new Instruction(ZpgMode, Sta, 0x85, "STA oper", 1);
		_instructions[0x86] = new Instruction(ZpgMode, Stx, 0x86, "STX oper", 1);
		_instructions[0x87] = new Instruction(Unsupported, Unsupported, 0x87, "Unsupported", 0);
		_instructions[0x88] = new Instruction(ImpMode, Dey, 0x88, "DEY oper", 1);
		_instructions[0x89] = new Instruction(Unsupported, Unsupported, 0x89, "Unsupported", 0);
		_instructions[0x8A] = new Instruction(ImpMode, Txa, 0x8A, "TXA oper", 1);
		_instructions[0x8B] = new Instruction(Unsupported, Unsupported, 0x8B, "Unsupported", 0);
		_instructions[0x8C] = new Instruction(AbsMode, Sty, 0x8C, "STY oper", 1);
		_instructions[0x8D] = new Instruction(AbsMode, Sta, 0x8D, "STA oper", 1);
		_instructions[0x8E] = new Instruction(AbsMode, Stx, 0x8E, "STX oper", 1);
		_instructions[0x8F] = new Instruction(Unsupported, Unsupported, 0x8F, "Unsupported", 0);

		// 9 -> 144 - 159
		_instructions[0x90] = new Instruction(RelMode, Bcc, 0x90, "BCC oper", 1);
		_instructions[0x91] = new Instruction(InyMode, Sta, 0x91, "STA oper", 1);
		_instructions[0x92] = new Instruction(Unsupported, Unsupported, 0x92, "Unsupported", 0);
		_instructions[0x93] = new Instruction(Unsupported, Unsupported, 0x93, "Unsupported", 0);
		_instructions[0x94] = new Instruction(ZpxMode, Sty, 0x94, "STY oper", 1);
		_instructions[0x95] = new Instruction(ZpxMode, Sta, 0x95, "STA oper", 1);
		_instructions[0x96] = new Instruction(ZpyMode, Stx, 0x96, "STX oper", 1);
		_instructions[0x97] = new Instruction(Unsupported, Unsupported, 0x97, "Unsupported", 0);
		_instructions[0x98] = new Instruction(ImpMode, Tya, 0x98, "TYA oper", 1);
		_instructions[0x99] = new Instruction(AbyMode, Sta, 0x99, "STA oper", 1);
		_instructions[0x9A] = new Instruction(ImpMode, Txs, 0x9A, "TXS oper", 1);
		_instructions[0x9B] = new Instruction(Unsupported, Unsupported, 0x9B, "Unsupported", 0);
		_instructions[0x9C] = new Instruction(Unsupported, Unsupported, 0x9C, "Unsupported", 0);
		_instructions[0x9D] = new Instruction(AbxMode, Sta, 0x9D, "STA oper", 1);
		_instructions[0x9E] = new Instruction(Unsupported, Unsupported, 0x9E, "Unsupported", 0);
		_instructions[0x9F] = new Instruction(Unsupported, Unsupported, 0x9F, "Unsupported", 0);

		// A -> 160 - 175
		_instructions[0xA0] = new Instruction(ImmMode, Ldy, 0xA0, "LDY oper", 1);
		_instructions[0xA1] = new Instruction(InxMode, Lda, 0xA1, "LDA oper", 1);
		_instructions[0xA2] = new Instruction(ImmMode, Ldx, 0xA2, "LDX oper", 1);
		_instructions[0xA3] = new Instruction(Unsupported, Unsupported, 0xA3, "Unsupported", 0);
		_instructions[0xA4] = new Instruction(ZpgMode, Ldy, 0xA4, "LDY oper", 1);
		_instructions[0xA5] = new Instruction(ZpgMode, Lda, 0xA5, "LDA oper", 1);
		_instructions[0xA6] = new Instruction(ZpgMode, Ldx, 0xA6, "LDX oper", 1);
		_instructions[0xA7] = new Instruction(Unsupported, Unsupported, 0xA7, "Unsupported", 0);
		_instructions[0xA8] = new Instruction(ImpMode, Tay, 0xA8, "TAY oper", 1);
		_instructions[0xA9] = new Instruction(ImmMode, Lda, 0xA9, "LDA oper", 1);
		_instructions[0xAA] = new Instruction(ImpMode, Tax, 0xAA, "TAX oper", 1);
		_instructions[0xAB] = new Instruction(Unsupported, Unsupported, 0xAB, "Unsupported", 0);
		_instructions[0xAC] = new Instruction(AbsMode, Ldy, 0xAC, "LDY oper", 1);
		_instructions[0xAD] = new Instruction(AbsMode, Lda, 0xAD, "LDA oper", 1);
		_instructions[0xAE] = new Instruction(AbsMode, Ldx, 0xAE, "LDX oper", 1);
		_instructions[0xAF] = new Instruction(Unsupported, Unsupported, 0xAF, "Unsupported", 0);

		// B -> 176 - 191
		_instructions[0xB0] = new Instruction(RelMode, Bcs, 0xB0, "BCS oper", 1);
		_instructions[0xB1] = new Instruction(InyMode, Lda, 0xB1, "LDA oper", 1);
		_instructions[0xB2] = new Instruction(Unsupported, Unsupported, 0xB2, "Unsupported", 0);
		_instructions[0xB3] = new Instruction(Unsupported, Unsupported, 0xB3, "Unsupported", 0);
		_instructions[0xB4] = new Instruction(ZpxMode, Ldy, 0xB4, "LDY oper", 1);
		_instructions[0xB5] = new Instruction(ZpxMode, Lda, 0xB5, "LDA oper", 1);
		_instructions[0xB6] = new Instruction(ZpyMode, Ldx, 0xB6, "LDX oper", 1);
		_instructions[0xB7] = new Instruction(Unsupported, Unsupported, 0xB7, "Unsupported", 0);
		_instructions[0xB8] = new Instruction(ImpMode, Clv, 0xB8, "CLV oper", 1);
		_instructions[0xB9] = new Instruction(AbyMode, Lda, 0xB9, "LDA oper", 1);
		_instructions[0xBA] = new Instruction(ImpMode, Tsx, 0xBA, "TSX oper", 1);
		_instructions[0xBB] = new Instruction(Unsupported, Unsupported, 0xBB, "Unsupported", 0);
		_instructions[0xBC] = new Instruction(AbxMode, Ldy, 0xBC, "LDY oper", 1);
		_instructions[0xBD] = new Instruction(AbxMode, Lda, 0xBD, "LDA oper", 1);
		_instructions[0xBE] = new Instruction(AbyMode, Ldx, 0xBE, "LDX oper", 1);
		_instructions[0xBF] = new Instruction(Unsupported, Unsupported, 0xBF, "Unsupported", 0);

		// C -> 192 - 207
		_instructions[0xC0] = new Instruction(ImmMode, Cpy, 0xC0, "CPY oper", 1);
		_instructions[0xC1] = new Instruction(InxMode, Cmp, 0xC1, "CMP oper", 1);
		_instructions[0xC2] = new Instruction(Unsupported, Unsupported, 0xC2, "Unsupported", 0);
		_instructions[0xC3] = new Instruction(Unsupported, Unsupported, 0xC3, "Unsupported", 0);
		_instructions[0xC4] = new Instruction(ZpgMode, Cpy, 0xC4, "CPY oper", 1);
		_instructions[0xC5] = new Instruction(ZpgMode, Cmp, 0xC5, "CMP oper", 1);
		_instructions[0xC6] = new Instruction(ZpgMode, Dec, 0xC6, "DEC oper", 1);
		_instructions[0xC7] = new Instruction(Unsupported, Unsupported, 0xC7, "Unsupported", 0);
		_instructions[0xC8] = new Instruction(ImpMode, Iny, 0xC8, "INY oper", 1);
		_instructions[0xC9] = new Instruction(ImmMode, Cmp, 0xC9, "CMP oper", 1);
		_instructions[0xCA] = new Instruction(ImpMode, Dex, 0xCA, "DEX oper", 1);
		_instructions[0xCB] = new Instruction(Unsupported, Unsupported, 0xCB, "Unsupported", 0);
		_instructions[0xCC] = new Instruction(AbsMode, Cpy, 0xCC, "CPY oper", 1);
		_instructions[0xCD] = new Instruction(AbsMode, Cmp, 0xCD, "CMP oper", 1);
		_instructions[0xCE] = new Instruction(AbsMode, Dec, 0xCE, "DEC oper", 1);
		_instructions[0xCF] = new Instruction(Unsupported, Unsupported, 0xCF, "Unsupported", 0);

		// D -> 208 - 223
		_instructions[0xD0] = new Instruction(RelMode, Bne, 0xD0, "BNE oper", 1);
		_instructions[0xD1] = new Instruction(InyMode, Cmp, 0xD1, "CMP oper", 1);
		_instructions[0xD2] = new Instruction(Unsupported, Unsupported, 0xD2, "Unsupported", 0);
		_instructions[0xD3] = new Instruction(Unsupported, Unsupported, 0xD3, "Unsupported", 0);
		_instructions[0xD4] = new Instruction(Unsupported, Unsupported, 0xD4, "Unsupported", 0);
		_instructions[0xD5] = new Instruction(ZpxMode, Cmp, 0xD5, "CMP oper", 1);
		_instructions[0xD6] = new Instruction(ZpxMode, Dec, 0xD6, "DEC oper", 1);
		_instructions[0xD7] = new Instruction(Unsupported, Unsupported, 0xD7, "Unsupported", 0);
		_instructions[0xD8] = new Instruction(ImpMode, Cld, 0xD8, "CLD oper", 1);
		_instructions[0xD9] = new Instruction(AbyMode, Cmp, 0xD9, "CMP oper", 1);
		_instructions[0xDA] = new Instruction(Unsupported, Unsupported, 0xDA, "Unsupported", 0);
		_instructions[0xDB] = new Instruction(Unsupported, Unsupported, 0xDB, "Unsupported", 0);
		_instructions[0xDC] = new Instruction(Unsupported, Unsupported, 0xDC, "Unsupported", 0);
		_instructions[0xDD] = new Instruction(AbxMode, Cmp, 0xDD, "CMP oper", 1);
		_instructions[0xDE] = new Instruction(AbxMode, Dec, 0xDE, "DEC oper", 1);
		_instructions[0xDF] = new Instruction(Unsupported, Unsupported, 0xDF, "Unsupported", 0);

		// E -> 224 - 239
		_instructions[0xE0] = new Instruction(ImmMode, Cpx, 0xE0, "CPX oper", 1);
		_instructions[0xE1] = new Instruction(InxMode, Sbc, 0xE1, "SBC oper", 1);
		_instructions[0xE2] = new Instruction(Unsupported, Unsupported, 0xE2, "Unsupported", 0);
		_instructions[0xE3] = new Instruction(Unsupported, Unsupported, 0xE3, "Unsupported", 0);
		_instructions[0xE4] = new Instruction(ZpgMode, Cpx, 0xE4, "CPX oper", 1);
		_instructions[0xE5] = new Instruction(ZpgMode, Sbc, 0xE5, "SBC oper", 1);
		_instructions[0xE6] = new Instruction(ZpgMode, Inc, 0xE6, "INC oper", 1);
		_instructions[0xE7] = new Instruction(Unsupported, Unsupported, 0xE7, "Unsupported", 0);
		_instructions[0xE8] = new Instruction(ImpMode, Inx, 0xE8, "INX oper", 1);
		_instructions[0xE9] = new Instruction(ImmMode, Sbc, 0xE9, "SBC oper", 1);
		_instructions[0xEA] = new Instruction(ImpMode, Nop, 0xEA, "NOP oper", 1);
		_instructions[0xEB] = new Instruction(Unsupported, Unsupported, 0xEB, "Unsupported", 0);
		_instructions[0xEC] = new Instruction(AbsMode, Cpx, 0xEC, "CPX oper", 1);
		_instructions[0xED] = new Instruction(AbsMode, Sbc, 0xED, "SBC oper", 1);
		_instructions[0xEE] = new Instruction(AbsMode, Inc, 0xEE, "INC oper", 1);
		_instructions[0xEF] = new Instruction(Unsupported, Unsupported, 0xEF, "Unsupported", 0);

		// F -> 240 - 255
		_instructions[0xF0] = new Instruction(RelMode, Beq, 0xF0, "BEQ oper", 1);
		_instructions[0xF1] = new Instruction(InyMode, Sbc, 0xF1, "SBC oper", 1);
		_instructions[0xF2] = new Instruction(Unsupported, Unsupported, 0xF2, "Unsupported", 0);
		_instructions[0xF3] = new Instruction(Unsupported, Unsupported, 0xF3, "Unsupported", 0);
		_instructions[0xF4] = new Instruction(Unsupported, Unsupported, 0xF4, "Unsupported", 0);
		_instructions[0xF5] = new Instruction(ZpxMode, Sbc, 0xF5, "SBC oper", 1);
		_instructions[0xF6] = new Instruction(ZpxMode, Inc, 0xF6, "INC oper", 1);
		_instructions[0xF7] = new Instruction(Unsupported, Unsupported, 0xF7, "Unsupported", 0);
		_instructions[0xF8] = new Instruction(ImpMode, Sed, 0xF8, "SED oper", 1);
		_instructions[0xF9] = new Instruction(AbyMode, Sbc, 0xF9, "SBC oper", 1);
		_instructions[0xFA] = new Instruction(Unsupported, Unsupported, 0xFA, "Unsupported", 0);
		_instructions[0xFB] = new Instruction(Unsupported, Unsupported, 0xFB, "Unsupported", 0);
		_instructions[0xFC] = new Instruction(Unsupported, Unsupported, 0xFC, "Unsupported", 0);
		_instructions[0xFD] = new Instruction(AbxMode, Sbc, 0xFD, "SBC oper", 1);
		_instructions[0xFE] = new Instruction(AbxMode, Inc, 0xFE, "INC oper", 1);
		_instructions[0xFF] = new Instruction(Unsupported, Unsupported, 0xFF, "Unsupported", 0);
	}
}