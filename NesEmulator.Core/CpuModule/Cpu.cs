namespace NesEmulator.Core.CpuModule;

public sealed class Cpu
{
    private const uint ByteMask = 0x0000_00FF;
    private const uint WordMask = 0x0000_FFFF;

    private const uint NmiVectorL = 0xFFFA;
    private const uint NmiVectorH = 0xFFFB;
    private const uint RstVectorL = 0xFFFC;
    private const uint RstVectorH = 0xFFFD;
    private const uint IrqVectorL = 0xFFFE;
    private const uint IrqVectorH = 0xFFFF;
    private const uint StackTop = 0xFF;
    private const uint StackBot = 0x00;

    private readonly Instruction[] _instructions = new Instruction[256];
    private readonly Bus _bus;

    private uint _a;
    private uint _x;
    private uint _y;
    private uint _sr;
    private uint _sp;
    private uint _pc;

    private uint _cycles;
    private uint _address;
    private sbyte _relAddress;

    public uint Cycles { get; private set; }

    public Cpu(Bus bus,
        uint a = 0x00, uint x = 0x00, uint y = 0x00,
        uint sr = 0x00, uint sp = 0x00, uint pc = 0x0000)
    {
        CreateInstructions();

        _bus = bus;

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

    public void Reset()
    {
        var lowByte = ReadByte(RstVectorL);
        var highByte = ReadByte(RstVectorH);

        PC = (highByte << 8) | lowByte;

        A = 0x00;
        X = 0x00;
        Y = 0x00;

        SP = 0xFD;
        SR = 0x00;

        _address = 0x0000;
        _relAddress = 0x00;

        _cycles = 7; // ??? 7 or 8
        Cycles += 7;
    }

    public void Irq()
    {
        if (!InterruptFlag) return;

        StackPush(HiByte(PC));
        StackPush(LoByte(PC));

        BreakFlag = false;
        ReservedFlag = true;
        InterruptFlag = true;

        StackPush(SR);

        var lowByte = ReadByte(IrqVectorL);
        var highByte = ReadByte(IrqVectorH);

        PC = (highByte << 8) | lowByte;

        _cycles = 7;
        Cycles += 7;
    }

    public void Nmi()
    {
        StackPush(HiByte(PC));
        StackPush(LoByte(PC));

        BreakFlag = false;
        ReservedFlag = true;
        InterruptFlag = true;

        StackPush(SR);

        var lowByte = ReadByte(NmiVectorL);
        var highByte = ReadByte(NmiVectorH);

        PC = (highByte << 8) | lowByte;

        _cycles = 8;
        Cycles += 8;
    }

    public uint ExecuteSingleInstruction()
    {
        // fetch
        var opcode = NextByte();

        // decode
        var instruction = _instructions[opcode];


        // execute
        _cycles = 0;
        var x = instruction.Execute();
        _cycles += x;

        Cycles += _cycles;

        return _cycles;
    }

    public void Clock()
    {
        if (_cycles == 0)
        {
            // fetch
            var opcode = NextByte();

            // decode
            var instruction = _instructions[opcode];

            // execute
            _cycles = 0;
            var x = instruction.Execute();
            _cycles += x;

            Cycles += _cycles;

            Console.WriteLine($"A: {A:X2} X: {X:X2} Y:{Y:21}");
        }

        _cycles--;
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
            var hi = ReadByte(ptr & 0xFF00);

            _address = (hi << 8) | lo;
        }

        return false;
    }

    private bool InxMode()
    {
        var ptr = NextByte();

        var lo = ReadByte((ptr + X) % 256);
        var hi = ReadByte((ptr + X + 1) % 256);

        _address = (hi << 8) | lo;

        return false;
    }

    private bool InyMode()
    {
        var ptr = NextByte();

        var lo = ReadByte(ptr);
        var hi = ReadByte((ptr + 1) % 256);

        var x = (hi << 8) | lo;

        // overflow
        _address = x + Y;

        return IsPageCrossed(x, _address);
    }

    private bool RelMode()
    {
        _relAddress = (sbyte)NextByte();

        return false;
    }

    private bool ZpgMode()
    {
        _address = NextByte();

        return false;
    }

    private bool ZpxMode()
    {
        _address = (NextByte() + X) % 256;

        return false;
    }

    private bool ZpyMode()
    {
        _address = (NextByte() + Y) % 256;

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
        StackPush(SR | (uint)Flag.Break | (uint)Flag.Reserved);

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
        var value = StackPop();

        CarryFlag = (value & 0x01) != 0x00;
        ZeroFlag = (value & 0x02) != 0x00;
        InterruptFlag = (value & 0x04) != 0x00;
        DecimalFlag = (value & 0x08) != 0x00;
        OverflowFlag = (value & 0x40) != 0x00;
        NegativeFlag = (value & 0x80) != 0x00;

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

        OverflowFlag = (~(A ^ memory) & (A ^ result) & 0x0080) != 0x00;

        A = result;

        CarryFlag = HiByte(result) != 0x00;
        ZeroFlag = IsZero(A);
        NegativeFlag = IsNegative(A);

        return true;
    }

    private bool Sbc()
    {
        var memory = ReadByte(_address) ^ 0xFF;
        var result = A + memory + (CarryFlag ? 1U : 0U);

        OverflowFlag = (~(A ^ memory) & (A ^ result) & 0x0080) != 0x00;

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
        ZeroFlag = IsZero(result);
        NegativeFlag = IsNegative(result);

        WriteByte(_address, result);

        return false;
    }

    private bool RorA()
    {
        var value = A;

        var result = (value >> 1) | ((CarryFlag ? 1U : 0U) << 7);

        CarryFlag = (value & 0x0001) != 0x00;
        ZeroFlag = IsZero(result);
        NegativeFlag = IsNegative(result);

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
        var value = (byte)ReadByte(_address);
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

    // Unsupported
    private static bool Unsupported() => false;

    private static bool NopUn() => true;

    private bool LaxUn()
    {
        var mem = ReadByte(_address);

        A = mem;
        X = mem;

        ZeroFlag = IsZero(mem);
        NegativeFlag = IsNegative(mem);

        return true;
    }

    private bool SaxUn()
    {
        WriteByte(_address, (byte)(A & X));

        return false;
    }

    private bool SbcUn()
    {
        Sbc();

        return false;
    }

    private bool DcpUn()
    {
        Dec();
        Cmp();

        return false;
    }

    private bool IsbUn()
    {
        Inc();
        Sbc();

        return false;
    }

    private bool SloUn()
    {
        Asl();
        Ora();

        return false;
    }

    private bool RlaUn()
    {
        Rol();
        And();

        return false;
    }

    private bool SreUn()
    {
        Lsr();
        Eor();

        return false;
    }

    private bool RraUn()
    {
        Ror();
        Adc();

        return false;
    }

    #endregion

    #region Utils

    private void WriteByte(uint address, uint value)
    {
        _bus.WriteCpu((ushort)address, (byte)value);
    }

    private uint ReadByte(uint address)
    {
        return _bus.CpuRead((ushort)address);
    }

    private uint ReadWord(uint address)
    {
        var lo = ReadByte(address);
        var hi = ReadByte(address + 1);

        return (hi << 8) | lo;
    }

    private uint NextByte()
    {
        return _bus.CpuRead((ushort)PC++);
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
        WriteByte(0x0100 + SP, value);
        if (SP == StackBot) SP = StackTop;
        else SP--;
    }

    private uint StackPop()
    {
        if (SP == StackTop) SP = StackBot;
        else SP++;

        return ReadByte(0x0100 + SP);
    }

    private static bool IsZero(uint value) => (value & 0xFF) == 0x00;
    private static bool IsNegative(uint value) => (value & 0x80) != 0x00;
    private static bool IsPageCrossed(uint left, uint right) => HiByte(left) != HiByte(right);
    private static uint HiByte(uint value) => (value >> 8) & 0x00FF;
    private static uint LoByte(uint value) => value & 0x00FF;

    #endregion

    private void CreateInstructions()
    {
        // 0 -> 0 - 15
        _instructions[0x00] = new Instruction(ImpMode, Brk, 0x00, "BRK oper", 7);
        _instructions[0x01] = new Instruction(InxMode, Ora, 0x01, "ORA oper", 6);
        _instructions[0x02] = new Instruction(Unsupported, Unsupported, 0x02, "unsupported", 0);
        _instructions[0x03] = new Instruction(InxMode, SloUn, 0x03, "SLO (unsupp)", 8);
        _instructions[0x04] = new Instruction(ZpgMode, NopUn, 0x04, "NOP (unsupp)", 3);
        _instructions[0x05] = new Instruction(ZpgMode, Ora, 0x05, "ORA oper", 3);
        _instructions[0x06] = new Instruction(ZpgMode, Asl, 0x06, "ASL oper", 5);
        _instructions[0x07] = new Instruction(ZpgMode, SloUn, 0x07, "SLO (unsupp)", 5);
        _instructions[0x08] = new Instruction(ImpMode, Php, 0x08, "PHP oper", 3);
        _instructions[0x09] = new Instruction(ImmMode, Ora, 0x09, "ORA oper", 2);
        _instructions[0x0A] = new Instruction(AccMode, AslA, 0x0A, "ASL oper", 2);
        _instructions[0x0B] = new Instruction(Unsupported, Unsupported, 0x0B, "unsupported", 0);
        _instructions[0x0C] = new Instruction(AbsMode, NopUn, 0x0C, "NOP (unsupp)", 4);
        _instructions[0x0D] = new Instruction(AbsMode, Ora, 0x0D, "ORA oper", 4);
        _instructions[0x0E] = new Instruction(AbsMode, Asl, 0x0E, "ASL oper", 6);
        _instructions[0x0F] = new Instruction(AbsMode, SloUn, 0x0F, "SLO (unsupp)", 6);

        // 1 -> 16 - 31
        _instructions[0x10] = new Instruction(RelMode, Bpl, 0x10, "BPL oper", 2);
        _instructions[0x11] = new Instruction(InyMode, Ora, 0x11, "ORA oper", 5);
        _instructions[0x12] = new Instruction(Unsupported, Unsupported, 0x12, "unsupported", 0);
        _instructions[0x13] = new Instruction(InyMode, SloUn, 0x13, "SLO (unsupp)", 8);
        _instructions[0x14] = new Instruction(ZpxMode, NopUn, 0x14, "NOP (unsupp)", 4);
        _instructions[0x15] = new Instruction(ZpxMode, Ora, 0x15, "ORA oper", 4);
        _instructions[0x16] = new Instruction(ZpxMode, Asl, 0x16, "ASL oper", 6);
        _instructions[0x17] = new Instruction(ZpxMode, SloUn, 0x17, "SLO (unsupp)", 6);
        _instructions[0x18] = new Instruction(ImpMode, Clc, 0x18, "CLC oper", 2);
        _instructions[0x19] = new Instruction(AbyMode, Ora, 0x19, "ORA oper", 4);
        _instructions[0x1A] = new Instruction(ImpMode, NopUn, 0x1A, "NOP (unsupp)", 2);
        _instructions[0x1B] = new Instruction(AbyMode, SloUn, 0x1B, "SLO (unsupp)", 7);
        _instructions[0x1C] = new Instruction(AbxMode, NopUn, 0x1C, "NOP (unsupp)", 4);
        _instructions[0x1D] = new Instruction(AbxMode, Ora, 0x1D, "ORA oper", 4);
        _instructions[0x1E] = new Instruction(AbxMode, Asl, 0x1E, "ASL oper", 7);
        _instructions[0x1F] = new Instruction(AbxMode, SloUn, 0x1F, "SLO (unsupp)", 7);

        // 2 -> 32 - 47
        _instructions[0x20] = new Instruction(AbsMode, Jsr, 0x20, "JSR oper", 6);
        _instructions[0x21] = new Instruction(InxMode, And, 0x21, "AND oper", 6);
        _instructions[0x22] = new Instruction(Unsupported, Unsupported, 0x22, "Unsupported", 0);
        _instructions[0x23] = new Instruction(InxMode, RlaUn, 0x23, "RLA (unsupp)", 8);
        _instructions[0x24] = new Instruction(ZpgMode, Bit, 0x24, "BIT oper", 3);
        _instructions[0x25] = new Instruction(ZpgMode, And, 0x25, "AND oper", 3);
        _instructions[0x26] = new Instruction(ZpgMode, Rol, 0x26, "ROL oper", 5);
        _instructions[0x27] = new Instruction(ZpgMode, RlaUn, 0x27, "RLA (unsupp)", 5);
        _instructions[0x28] = new Instruction(ImpMode, Plp, 0x28, "PLP oper", 4);
        _instructions[0x29] = new Instruction(ImmMode, And, 0x29, "AND oper", 2);
        _instructions[0x2A] = new Instruction(AccMode, RolA, 0x2A, "ROL oper", 2);
        _instructions[0x2B] = new Instruction(Unsupported, Unsupported, 0x2B, "Unsupported", 0);
        _instructions[0x2C] = new Instruction(AbsMode, Bit, 0x2C, "BIT oper", 4);
        _instructions[0x2D] = new Instruction(AbsMode, And, 0x2D, "AND oper", 4);
        _instructions[0x2E] = new Instruction(AbsMode, Rol, 0x2E, "ROL oper", 6);
        _instructions[0x2F] = new Instruction(AbsMode, RlaUn, 0x2F, "RLA (unsupp)", 6);

        // 3 -> 48 - 63
        _instructions[0x30] = new Instruction(RelMode, Bmi, 0x30, "BMI oper", 2);
        _instructions[0x31] = new Instruction(InyMode, And, 0x31, "AND oper", 5);
        _instructions[0x32] = new Instruction(Unsupported, Unsupported, 0x32, "Unsupported", 0);
        _instructions[0x33] = new Instruction(InyMode, RlaUn, 0x33, "RLA (unsupp)", 8);
        _instructions[0x34] = new Instruction(ZpxMode, NopUn, 0x34, "NOP (unsupp)", 4);
        _instructions[0x35] = new Instruction(ZpxMode, And, 0x35, "AND oper", 4);
        _instructions[0x36] = new Instruction(ZpxMode, Rol, 0x36, "ROL oper", 6);
        _instructions[0x37] = new Instruction(ZpxMode, RlaUn, 0x37, "RLA (unsupp)", 6);
        _instructions[0x38] = new Instruction(ImpMode, Sec, 0x38, "SEC oper", 2);
        _instructions[0x39] = new Instruction(AbyMode, And, 0x39, "AND oper", 4);
        _instructions[0x3A] = new Instruction(ImpMode, NopUn, 0x3A, "NOP (unsupp)", 2);
        _instructions[0x3B] = new Instruction(AbyMode, RlaUn, 0x3B, "RLA (unsupp)", 7);
        _instructions[0x3C] = new Instruction(AbxMode, NopUn, 0x3C, "NOP (unsupp)", 4);
        _instructions[0x3D] = new Instruction(AbxMode, And, 0x3D, "AND oper", 4);
        _instructions[0x3E] = new Instruction(AbxMode, Rol, 0x3E, "ROL oper", 7);
        _instructions[0x3F] = new Instruction(AbxMode, RlaUn, 0x3F, "RLA (unsupp)", 7);

        // 4 -> 64 - 79
        _instructions[0x40] = new Instruction(ImpMode, Rti, 0x40, "RTI oper", 6);
        _instructions[0x41] = new Instruction(InxMode, Eor, 0x41, "EOR oper", 6);
        _instructions[0x42] = new Instruction(Unsupported, Unsupported, 0x42, "Unsupported", 0);
        _instructions[0x43] = new Instruction(InxMode, SreUn, 0x43, "SRE (unsupp)", 8);
        _instructions[0x44] = new Instruction(ZpgMode, NopUn, 0x44, "NOP (unsupp)", 3);
        _instructions[0x45] = new Instruction(ZpgMode, Eor, 0x45, "EOR oper", 3);
        _instructions[0x46] = new Instruction(ZpgMode, Lsr, 0x46, "LSR oper", 5);
        _instructions[0x47] = new Instruction(ZpgMode, SreUn, 0x47, "SRE (unsupp)", 5);
        _instructions[0x48] = new Instruction(ImpMode, Pha, 0x48, "PHA oper", 3);
        _instructions[0x49] = new Instruction(ImmMode, Eor, 0x49, "EOR oper", 2);
        _instructions[0x4A] = new Instruction(AccMode, LsrA, 0x4A, "LSR oper", 2);
        _instructions[0x4B] = new Instruction(Unsupported, Unsupported, 0x4B, "Unsupported", 0);
        _instructions[0x4C] = new Instruction(AbsMode, Jmp, 0x4C, "JMP oper", 3);
        _instructions[0x4D] = new Instruction(AbsMode, Eor, 0x4D, "EOR oper", 4);
        _instructions[0x4E] = new Instruction(AbsMode, Lsr, 0x4E, "LSR oper", 6);
        _instructions[0x4F] = new Instruction(AbsMode, SreUn, 0x4F, "SRE (unsupp)", 6);

        // 5 -> 80 - 95
        _instructions[0x50] = new Instruction(RelMode, Bvc, 0x50, "BVC oper", 2);
        _instructions[0x51] = new Instruction(InyMode, Eor, 0x51, "EOR oper", 5);
        _instructions[0x52] = new Instruction(Unsupported, Unsupported, 0x52, "Unsupported", 0);
        _instructions[0x53] = new Instruction(InyMode, SreUn, 0x53, "SRE (unsupp)", 8);
        _instructions[0x54] = new Instruction(ZpxMode, NopUn, 0x54, "NOP (unsupp)", 4);
        _instructions[0x55] = new Instruction(ZpxMode, Eor, 0x55, "EOR oper", 4);
        _instructions[0x56] = new Instruction(ZpxMode, Lsr, 0x56, "LSR oper", 6);
        _instructions[0x57] = new Instruction(ZpxMode, SreUn, 0x57, "SRE (unsupp)", 6);
        _instructions[0x58] = new Instruction(ImpMode, Cli, 0x58, "CLI oper", 2);
        _instructions[0x59] = new Instruction(AbyMode, Eor, 0x59, "EOR oper", 4);
        _instructions[0x5A] = new Instruction(ImpMode, NopUn, 0x5A, "NOP (unsupp)", 2);
        _instructions[0x5B] = new Instruction(AbyMode, SreUn, 0x5B, "SRE (unsupp)", 7);
        _instructions[0x5C] = new Instruction(AbxMode, NopUn, 0x5C, "NOP (unsupp)", 4);
        _instructions[0x5D] = new Instruction(AbxMode, Eor, 0x5D, "EOR oper", 4);
        _instructions[0x5E] = new Instruction(AbxMode, Lsr, 0x5E, "LSR oper", 7);
        _instructions[0x5F] = new Instruction(AbxMode, SreUn, 0x5F, "SRE (unsupp)", 7);

        // 6 -> 96 - 111
        _instructions[0x60] = new Instruction(ImpMode, Rts, 0x60, "RTS oper", 6);
        _instructions[0x61] = new Instruction(InxMode, Adc, 0x61, "ADC oper", 6);
        _instructions[0x62] = new Instruction(Unsupported, Unsupported, 0x62, "Unsupported", 0);
        _instructions[0x63] = new Instruction(InxMode, RraUn, 0x63, "RRA (unsupp)", 8);
        _instructions[0x64] = new Instruction(ZpgMode, NopUn, 0x64, "NOP (unsupp)", 3);
        _instructions[0x65] = new Instruction(ZpgMode, Adc, 0x65, "ADC oper", 3);
        _instructions[0x66] = new Instruction(ZpgMode, Ror, 0x66, "ROR oper", 5);
        _instructions[0x67] = new Instruction(ZpgMode, RraUn, 0x67, "RRA (unsupp)", 5);
        _instructions[0x68] = new Instruction(ImpMode, Pla, 0x68, "PLA oper", 4);
        _instructions[0x69] = new Instruction(ImmMode, Adc, 0x69, "ADC oper", 2);
        _instructions[0x6A] = new Instruction(AccMode, RorA, 0x6A, "ROR oper", 2);
        _instructions[0x6B] = new Instruction(Unsupported, Unsupported, 0x6B, "Unsupported", 0);
        _instructions[0x6C] = new Instruction(IndMode, Jmp, 0x6C, "JMP oper", 5);
        _instructions[0x6D] = new Instruction(AbsMode, Adc, 0x6D, "ADC oper", 4);
        _instructions[0x6E] = new Instruction(AbsMode, Ror, 0x6E, "ROR oper", 6);
        _instructions[0x6F] = new Instruction(AbsMode, RraUn, 0x6F, "RRA (unsupp)", 6);

        // 7 -> 112 - 127
        _instructions[0x70] = new Instruction(RelMode, Bvs, 0x70, "BVS oper", 2);
        _instructions[0x71] = new Instruction(InyMode, Adc, 0x71, "ADC oper", 5);
        _instructions[0x72] = new Instruction(Unsupported, Unsupported, 0x72, "Unsupported", 0);
        _instructions[0x73] = new Instruction(InyMode, RraUn, 0x73, "RRA (unsupp)", 8);
        _instructions[0x74] = new Instruction(ZpxMode, NopUn, 0x74, "NOP (unsupp)", 4);
        _instructions[0x75] = new Instruction(ZpxMode, Adc, 0x75, "ADC oper", 4);
        _instructions[0x76] = new Instruction(ZpxMode, Ror, 0x76, "ROR oper", 6);
        _instructions[0x77] = new Instruction(ZpxMode, RraUn, 0x77, "RRA (unsupp)", 6);
        _instructions[0x78] = new Instruction(ImpMode, Sei, 0x78, "SEI oper", 2);
        _instructions[0x79] = new Instruction(AbyMode, Adc, 0x79, "ADC oper", 4);
        _instructions[0x7A] = new Instruction(ImpMode, NopUn, 0x7A, "NOP (unsupp)", 2);
        _instructions[0x7B] = new Instruction(AbyMode, RraUn, 0x7B, "RRA (unsupp)", 7);
        _instructions[0x7C] = new Instruction(AbxMode, NopUn, 0x7C, "NOP (unsupp)", 4);
        _instructions[0x7D] = new Instruction(AbxMode, Adc, 0x7D, "ADC oper", 4);
        _instructions[0x7E] = new Instruction(AbxMode, Ror, 0x7E, "ROR oper", 7);
        _instructions[0x7F] = new Instruction(AbxMode, RraUn, 0x7F, "RRA (unsupp)", 7);

        // 8 -> 128 - 143
        _instructions[0x80] = new Instruction(ImmMode, NopUn, 0x80, "NOP (unsupp)", 2);
        _instructions[0x81] = new Instruction(InxMode, Sta, 0x81, "STA oper", 6);
        _instructions[0x82] = new Instruction(ImmMode, NopUn, 0x82, "NOP (unsupp)", 2);
        _instructions[0x83] = new Instruction(InxMode, SaxUn, 0x83, "SAX (unsupp)", 6);
        _instructions[0x84] = new Instruction(ZpgMode, Sty, 0x84, "STY oper", 3);
        _instructions[0x85] = new Instruction(ZpgMode, Sta, 0x85, "STA oper", 3);
        _instructions[0x86] = new Instruction(ZpgMode, Stx, 0x86, "STX oper", 3);
        _instructions[0x87] = new Instruction(ZpgMode, SaxUn, 0x87, "SAX (unsupp)", 3);
        _instructions[0x88] = new Instruction(ImpMode, Dey, 0x88, "DEY oper", 2);
        _instructions[0x89] = new Instruction(ImmMode, NopUn, 0x89, "NOP (unsupp)", 2);
        _instructions[0x8A] = new Instruction(ImpMode, Txa, 0x8A, "TXA oper", 2);
        _instructions[0x8B] = new Instruction(Unsupported, Unsupported, 0x8B, "Unsupported", 0);
        _instructions[0x8C] = new Instruction(AbsMode, Sty, 0x8C, "STY oper", 4);
        _instructions[0x8D] = new Instruction(AbsMode, Sta, 0x8D, "STA oper", 4);
        _instructions[0x8E] = new Instruction(AbsMode, Stx, 0x8E, "STX oper", 4);
        _instructions[0x8F] = new Instruction(AbsMode, SaxUn, 0x8F, "SAX (unsupp)", 4);

        // 9 -> 144 - 159
        _instructions[0x90] = new Instruction(RelMode, Bcc, 0x90, "BCC oper", 2);
        _instructions[0x91] = new Instruction(InyMode, Sta, 0x91, "STA oper", 6);
        _instructions[0x92] = new Instruction(Unsupported, Unsupported, 0x92, "Unsupported", 0);
        _instructions[0x93] = new Instruction(Unsupported, Unsupported, 0x93, "Unsupported", 0);
        _instructions[0x94] = new Instruction(ZpxMode, Sty, 0x94, "STY oper", 4);
        _instructions[0x95] = new Instruction(ZpxMode, Sta, 0x95, "STA oper", 4);
        _instructions[0x96] = new Instruction(ZpyMode, Stx, 0x96, "STX oper", 4);
        _instructions[0x97] = new Instruction(ZpyMode, SaxUn, 0x97, "SAX (unsupp)", 4);
        _instructions[0x98] = new Instruction(ImpMode, Tya, 0x98, "TYA oper", 2);
        _instructions[0x99] = new Instruction(AbyMode, Sta, 0x99, "STA oper", 5);
        _instructions[0x9A] = new Instruction(ImpMode, Txs, 0x9A, "TXS oper", 2);
        _instructions[0x9B] = new Instruction(Unsupported, Unsupported, 0x9B, "Unsupported", 0);
        _instructions[0x9C] = new Instruction(Unsupported, Unsupported, 0x9C, "Unsupported", 0);
        _instructions[0x9D] = new Instruction(AbxMode, Sta, 0x9D, "STA oper", 5);
        _instructions[0x9E] = new Instruction(Unsupported, Unsupported, 0x9E, "Unsupported", 0);
        _instructions[0x9F] = new Instruction(Unsupported, Unsupported, 0x9F, "Unsupported", 0);

        // A -> 160 - 175
        _instructions[0xA0] = new Instruction(ImmMode, Ldy, 0xA0, "LDY oper", 2);
        _instructions[0xA1] = new Instruction(InxMode, Lda, 0xA1, "LDA oper", 6);
        _instructions[0xA2] = new Instruction(ImmMode, Ldx, 0xA2, "LDX oper", 2);
        _instructions[0xA3] = new Instruction(InxMode, LaxUn, 0xA3, "LAX (unsupp)", 6);
        _instructions[0xA4] = new Instruction(ZpgMode, Ldy, 0xA4, "LDY oper", 3);
        _instructions[0xA5] = new Instruction(ZpgMode, Lda, 0xA5, "LDA oper", 3);
        _instructions[0xA6] = new Instruction(ZpgMode, Ldx, 0xA6, "LDX oper", 3);
        _instructions[0xA7] = new Instruction(ZpgMode, LaxUn, 0xA7, "LAX (unsupp)", 3);
        _instructions[0xA8] = new Instruction(ImpMode, Tay, 0xA8, "TAY oper", 2);
        _instructions[0xA9] = new Instruction(ImmMode, Lda, 0xA9, "LDA oper", 2);
        _instructions[0xAA] = new Instruction(ImpMode, Tax, 0xAA, "TAX oper", 2);
        _instructions[0xAB] = new Instruction(Unsupported, Unsupported, 0xAB, "Unsupported", 0);
        _instructions[0xAC] = new Instruction(AbsMode, Ldy, 0xAC, "LDY oper", 4);
        _instructions[0xAD] = new Instruction(AbsMode, Lda, 0xAD, "LDA oper", 4);
        _instructions[0xAE] = new Instruction(AbsMode, Ldx, 0xAE, "LDX oper", 4);
        _instructions[0xAF] = new Instruction(AbsMode, LaxUn, 0xAF, "LAX (unsupp)", 4);

        // B -> 176 - 191
        _instructions[0xB0] = new Instruction(RelMode, Bcs, 0xB0, "BCS oper", 2);
        _instructions[0xB1] = new Instruction(InyMode, Lda, 0xB1, "LDA oper", 5);
        _instructions[0xB2] = new Instruction(Unsupported, Unsupported, 0xB2, "Unsupported", 0);
        _instructions[0xB3] = new Instruction(InyMode, LaxUn, 0xB3, "LAX (unsupp)", 5);
        _instructions[0xB4] = new Instruction(ZpxMode, Ldy, 0xB4, "LDY oper", 4);
        _instructions[0xB5] = new Instruction(ZpxMode, Lda, 0xB5, "LDA oper", 4);
        _instructions[0xB6] = new Instruction(ZpyMode, Ldx, 0xB6, "LDX oper", 4);
        _instructions[0xB7] = new Instruction(ZpyMode, LaxUn, 0xB7, "LAX (unsupp)", 4);
        _instructions[0xB8] = new Instruction(ImpMode, Clv, 0xB8, "CLV oper", 2);
        _instructions[0xB9] = new Instruction(AbyMode, Lda, 0xB9, "LDA oper", 4);
        _instructions[0xBA] = new Instruction(ImpMode, Tsx, 0xBA, "TSX oper", 2);
        _instructions[0xBB] = new Instruction(Unsupported, Unsupported, 0xBB, "Unsupported", 0);
        _instructions[0xBC] = new Instruction(AbxMode, Ldy, 0xBC, "LDY oper", 4);
        _instructions[0xBD] = new Instruction(AbxMode, Lda, 0xBD, "LDA oper", 4);
        _instructions[0xBE] = new Instruction(AbyMode, Ldx, 0xBE, "LDX oper", 4);
        _instructions[0xBF] = new Instruction(AbyMode, LaxUn, 0xBF, "LAX (unsupp)", 4);

        // C -> 192 - 207
        _instructions[0xC0] = new Instruction(ImmMode, Cpy, 0xC0, "CPY oper", 2);
        _instructions[0xC1] = new Instruction(InxMode, Cmp, 0xC1, "CMP oper", 6);
        _instructions[0xC2] = new Instruction(ImmMode, NopUn, 0xC2, "NOP (unsupp)", 2);
        _instructions[0xC3] = new Instruction(InxMode, DcpUn, 0xC3, "DCP (unsupp)", 8);
        _instructions[0xC4] = new Instruction(ZpgMode, Cpy, 0xC4, "CPY oper", 3);
        _instructions[0xC5] = new Instruction(ZpgMode, Cmp, 0xC5, "CMP oper", 3);
        _instructions[0xC6] = new Instruction(ZpgMode, Dec, 0xC6, "DEC oper", 5);
        _instructions[0xC7] = new Instruction(ZpgMode, DcpUn, 0xC7, "DCP (unsupp)", 5);
        _instructions[0xC8] = new Instruction(ImpMode, Iny, 0xC8, "INY oper", 2);
        _instructions[0xC9] = new Instruction(ImmMode, Cmp, 0xC9, "CMP oper", 2);
        _instructions[0xCA] = new Instruction(ImpMode, Dex, 0xCA, "DEX oper", 2);
        _instructions[0xCB] = new Instruction(Unsupported, Unsupported, 0xCB, "Unsupported", 0);
        _instructions[0xCC] = new Instruction(AbsMode, Cpy, 0xCC, "CPY oper", 4);
        _instructions[0xCD] = new Instruction(AbsMode, Cmp, 0xCD, "CMP oper", 4);
        _instructions[0xCE] = new Instruction(AbsMode, Dec, 0xCE, "DEC oper", 6);
        _instructions[0xCF] = new Instruction(AbsMode, DcpUn, 0xCF, "DCP (unsupp)", 6);

        // D -> 208 - 223
        _instructions[0xD0] = new Instruction(RelMode, Bne, 0xD0, "BNE oper", 2);
        _instructions[0xD1] = new Instruction(InyMode, Cmp, 0xD1, "CMP oper", 5);
        _instructions[0xD2] = new Instruction(Unsupported, Unsupported, 0xD2, "Unsupported", 0);
        _instructions[0xD3] = new Instruction(InyMode, DcpUn, 0xD3, "DCP (unsupp)", 8);
        _instructions[0xD4] = new Instruction(ZpxMode, NopUn, 0xD4, "NOP (unsupp)", 4);
        _instructions[0xD5] = new Instruction(ZpxMode, Cmp, 0xD5, "CMP oper", 4);
        _instructions[0xD6] = new Instruction(ZpxMode, Dec, 0xD6, "DEC oper", 6);
        _instructions[0xD7] = new Instruction(ZpxMode, DcpUn, 0xD7, "DCP (unsupp)", 6);
        _instructions[0xD8] = new Instruction(ImpMode, Cld, 0xD8, "CLD oper", 2);
        _instructions[0xD9] = new Instruction(AbyMode, Cmp, 0xD9, "CMP oper", 4);
        _instructions[0xDA] = new Instruction(ImpMode, NopUn, 0xDA, "NOP (unsupp)", 2);
        _instructions[0xDB] = new Instruction(AbyMode, DcpUn, 0xDB, "DCP (unsupp)", 7);
        _instructions[0xDC] = new Instruction(AbxMode, NopUn, 0xDC, "NOP (unsupp)", 4);
        _instructions[0xDD] = new Instruction(AbxMode, Cmp, 0xDD, "CMP oper", 4);
        _instructions[0xDE] = new Instruction(AbxMode, Dec, 0xDE, "DEC oper", 7);
        _instructions[0xDF] = new Instruction(AbxMode, DcpUn, 0xDF, "DCP (unsupp)", 7);

        // E -> 224 - 239
        _instructions[0xE0] = new Instruction(ImmMode, Cpx, 0xE0, "CPX oper", 2);
        _instructions[0xE1] = new Instruction(InxMode, Sbc, 0xE1, "SBC oper", 6);
        _instructions[0xE2] = new Instruction(ImmMode, NopUn, 0xE2, "NOP (unsupp)", 2);
        _instructions[0xE3] = new Instruction(InxMode, IsbUn, 0xE3, "ISB (unsupp)", 8);
        _instructions[0xE4] = new Instruction(ZpgMode, Cpx, 0xE4, "CPX oper", 3);
        _instructions[0xE5] = new Instruction(ZpgMode, Sbc, 0xE5, "SBC oper", 3);
        _instructions[0xE6] = new Instruction(ZpgMode, Inc, 0xE6, "INC oper", 5);
        _instructions[0xE7] = new Instruction(ZpgMode, IsbUn, 0xE7, "ISB (unsupp)", 5);
        _instructions[0xE8] = new Instruction(ImpMode, Inx, 0xE8, "INX oper", 2);
        _instructions[0xE9] = new Instruction(ImmMode, Sbc, 0xE9, "SBC oper", 2);
        _instructions[0xEA] = new Instruction(ImpMode, Nop, 0xEA, "NOP oper", 2);
        _instructions[0xEB] = new Instruction(ImmMode, SbcUn, 0xEB, "SBC (unsupp)", 2);
        _instructions[0xEC] = new Instruction(AbsMode, Cpx, 0xEC, "CPX oper", 4);
        _instructions[0xED] = new Instruction(AbsMode, Sbc, 0xED, "SBC oper", 4);
        _instructions[0xEE] = new Instruction(AbsMode, Inc, 0xEE, "INC oper", 6);
        _instructions[0xEF] = new Instruction(AbsMode, IsbUn, 0xEF, "ISB (unsupp)", 6);

        // F -> 240 - 255
        _instructions[0xF0] = new Instruction(RelMode, Beq, 0xF0, "BEQ oper", 2);
        _instructions[0xF1] = new Instruction(InyMode, Sbc, 0xF1, "SBC oper", 5);
        _instructions[0xF2] = new Instruction(Unsupported, Unsupported, 0xF2, "Unsupported", 0);
        _instructions[0xF3] = new Instruction(InyMode, IsbUn, 0xF3, "ISB (unsupp)", 8);
        _instructions[0xF4] = new Instruction(ZpxMode, NopUn, 0xF4, "NOP (unsupp)", 4);
        _instructions[0xF5] = new Instruction(ZpxMode, Sbc, 0xF5, "SBC oper", 4);
        _instructions[0xF6] = new Instruction(ZpxMode, Inc, 0xF6, "INC oper", 6);
        _instructions[0xF7] = new Instruction(ZpxMode, IsbUn, 0xF7, "ISB (unsupp)", 6);
        _instructions[0xF8] = new Instruction(ImpMode, Sed, 0xF8, "SED oper", 2);
        _instructions[0xF9] = new Instruction(AbyMode, Sbc, 0xF9, "SBC oper", 4);
        _instructions[0xFA] = new Instruction(ImpMode, NopUn, 0xFA, "NOP (unsupp)", 2);
        _instructions[0xFB] = new Instruction(AbyMode, IsbUn, 0xFB, "ISB (unsupp)", 7);
        _instructions[0xFC] = new Instruction(AbxMode, NopUn, 0xFC, "NOP (unsupp)", 4);
        _instructions[0xFD] = new Instruction(AbxMode, Sbc, 0xFD, "SBC oper", 4);
        _instructions[0xFE] = new Instruction(AbxMode, Inc, 0xFE, "INC oper", 7);
        _instructions[0xFF] = new Instruction(AbxMode, IsbUn, 0xFF, "ISB (unsupp)", 7);
    }
}