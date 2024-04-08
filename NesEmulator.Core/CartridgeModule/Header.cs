namespace NesEmulator.Core.CartridgeModule;

public class Header(
    byte[] nesTag,
    byte prgRomSize,
    byte chrRomSize,
    byte flag6,
    byte flag7,
    byte flag8,
    byte flag9,
    byte flag10,
    byte[] unused)
{
    private byte[] _nesTag = nesTag;
    private byte _prgRomSize = prgRomSize;
    private byte _chrRomSize = chrRomSize;
    private byte _flag6 = flag6;
    private readonly byte _flag7 = flag7;
    private readonly byte _flag8 = flag8;
    private readonly byte _flag9 = flag9;
    private readonly byte _flag10 = flag10;
    private readonly byte[] _unused = unused;
}