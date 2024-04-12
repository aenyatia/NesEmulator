namespace NesEmulator.Core.CartridgeModule;

public static class NesFileLoader
{
    private static readonly byte[] NesTag = [0x4E, 0x45, 0x53, 0x1A];

    private const int PrgRomBankSize = 16384;
    private const int ChrRomBankSize = 8192;

    public static Cartridge LoadNesFile(string path)
    {
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fileStream);

        var header = LoadHeader(reader);

        if (!header.NesTag.SequenceEqual(NesTag))
            throw new Exception("invalid nes tag");

        if (header.Mapper != 0)
            throw new Exception("invalid mapper");

        fileStream.Seek(16 + (header.HasTrainer ? 512 : 0), SeekOrigin.Begin);

        var prgRom = LoadPgrRom(reader, header.PrgRomBanksCount * PrgRomBankSize);
        var chrRom = LoadChrRom(reader, header.ChrRomBanksCount * ChrRomBankSize);

        return new Cartridge(header, prgRom, chrRom);
    }

    private static Header LoadHeader(BinaryReader reader)
    {
        var nesTag = reader.ReadBytes(4);
        var prgRomSize = reader.ReadByte();
        var chrRomSize = reader.ReadByte();
        var flag6 = reader.ReadByte();
        var flag7 = reader.ReadByte();

        return new Header(nesTag, prgRomSize, chrRomSize, flag6, flag7);
    }

    private static byte[] LoadPgrRom(BinaryReader reader, int count)
        => reader.ReadBytes(count);

    private static byte[] LoadChrRom(BinaryReader reader, int count)
        => reader.ReadBytes(count);
}