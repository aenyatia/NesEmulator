using NesEmulator.Core.CpuModule;

namespace NesEmulator.Core;

public class Bus
{
    private readonly byte[] _cpuRam = new byte[2048];

    public Cpu Cpu { get; }
    private Rom Rom { get; }

    public Bus()
    {
        Cpu = new Cpu(this);
        Rom = new Rom("snake.nes");
    }

    public byte Read(uint address)
    {
        if (address is >= 0x0000 and < 0x2000)
        {
            // mirroring 0000 0111 1111 1111 - 0x07FF
            // 0x0000 - 0x07FF
            // 0x0800 - 0x0FFF
            // 0x1000 - 0x17FF
            // 0x1800 - 0x1FFF

            address &= 0x07FF;

            return _cpuRam[address];
        }

        if (address is >= 0x2000 and < 0x4000)
        {
            // mirroring 0010 0000 0000 0111 - 0x2007
            // 0x2000 - 0x2007
            // 0x2008 - 0x200F
            // 0x2010 - 0x2017
            // 0x2018 - 0x201F

            address &= 0x2007;

            // todo ppu addressing
        }

        if (address is >= 0x8000 and < 0x10000)
        {
            return Rom.ReadPrg(address);
        }

        return 0x00;
    }

    public void Write(uint address, uint data)
    {
        if (address is >= 0x0000 and < 0x2000)
        {
            // mirroring 0000 0111 1111 1111 - 0x07FF
            // 0x0000 - 0x07FF
            // 0x0800 - 0x0FFF
            // 0x1000 - 0x17FF
            // 0x1800 - 0x1FFF

            address &= 0x07FF;

            _cpuRam[address] = (byte)data;
        }

        if (address is >= 0x2000 and < 0x4000)
        {
            // mirroring 0010 0000 0000 0111 - 0x2007
            // 0x2000 - 0x2007
            // 0x2008 - 0x200F
            // 0x2010 - 0x2017
            // 0x2018 - 0x201F

            address &= 0x2007;

            // todo ppu addressing
        }

        if (address is >= 0x8000 and < 0x10000)
        {
            throw new Exception("cannot write to prg rom");
        }
    }
}