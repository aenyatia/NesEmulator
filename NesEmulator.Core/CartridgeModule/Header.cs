namespace NesEmulator.Core.CartridgeModule;

public class Header(byte[] nesTag, byte prgRomSize, byte chrRomSize, byte flag6, byte flag7)
{
    public byte[] NesTag { get; } = nesTag;
    public byte PrgRomBanksCount { get; } = prgRomSize;
    public byte ChrRomBanksCount { get; } = chrRomSize;
    public int Mapper { get; } = (flag7 & 0b1111_0000) | ((flag6 & 0b1111_0000) >> 4);
    public bool HasTrainer { get; } = (flag6 & 0b0000_0100) == 0b0000_0100;

    public Mirroring Mirroring { get; } = (flag6 & 0b0000_0001) == 0b0000_0001
        ? Mirroring.Vertical
        : Mirroring.Horizontal;
}