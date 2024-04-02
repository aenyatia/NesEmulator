namespace NesEmulator.Core;

public class Rom
{
    private const int NesTag = 0x1A53454E;
    private const int PrgRomPageSize = 16384;
    private const int ChrRomPageSize = 8192;

    private readonly byte[] _prgRom;
    private readonly byte[] _chrRom;
    private readonly Mirroring _screenMirroring;
    private readonly byte _mapper;

    public Rom(string path)
    {
        using var reader = new BinaryReader(File.OpenRead(path));

        var tag = reader.ReadInt32(); // 0-3 bytes
        if (tag != NesTag)
            throw new Exception("file is not in iNES file format");

        var prgRom = reader.ReadByte(); // 4 byte - number of 16kB rom banks prg
        var chrRom = reader.ReadByte(); // 5 byte - number of 8kB vrom banks chr
        var control1 = reader.ReadByte(); // 6 byte - control byte 1
        var control2 = reader.ReadByte(); // 7 byte - control byte 2
        var prgRam = reader.ReadByte(); // 8 byte - size of prg ram in 8kB units
        var x = reader.ReadByte();

        _mapper = (byte)((control2 & 0b1111_0000) >> 4);

        var iNesVersion = (control2 & 0b0000_1100) >> 2;
        if (iNesVersion != 0)
            throw new Exception("NES2.0 format is not supported");

        var fourScreen = (control1 & 0b0000_1000) >> 3;
        var mirroring = control1 & 0b0000_0001;
        if (fourScreen == 1)
            _screenMirroring = Mirroring.FourScreen;
        else if (mirroring == 0)
            _screenMirroring = Mirroring.Horizontal;
        else if (mirroring == 1)
            _screenMirroring = Mirroring.Vertical;

        var prgRomSize = PrgRomPageSize * prgRom;
        var chrRomSize = ChrRomPageSize * chrRom;

        var trainer = (control1 & 0b0000_0100) >> 2;

        var prgRomStart = trainer == 0 ? 16 : 16 + 512;
        var chrRomStart = prgRomStart + prgRomSize;

        _prgRom = new byte[prgRomSize];
        reader.BaseStream.Seek(prgRomStart, SeekOrigin.Begin);
        var pgrRead = reader.Read(_prgRom, 0, prgRomSize);
        if (pgrRead != prgRomSize)
            throw new Exception("error during reading prg rom");

        _chrRom = new byte[chrRomSize];
        reader.BaseStream.Seek(chrRomStart, SeekOrigin.Begin);
        var chrRead = reader.Read(_chrRom, 0, chrRomSize);
        if (chrRead != chrRomSize)
            throw new Exception("error during reading chr rom");
    }

    public byte ReadPrg(uint address)
    {
        address -= 0x8000;

        if (_prgRom.Length == 0x4000 && address >= 0x4000) // prgRom 16kB instead of 32kB
            address &= 0b0011_1111_1111_1111;
        // address %= 0x4000;

        return _prgRom[address];
    }
}

public enum Mirroring
{
    Vertical,
    Horizontal,
    FourScreen
}