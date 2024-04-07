using NesEmulator.Core.CartridgeModule;
using NesEmulator.Core.CpuModule;
using NesEmulator.Core.PpuModule;

namespace NesEmulator.Core;

public class Bus
{
    private readonly byte[] _cpuRam = new byte[2048];

    public event EventHandler? DrawFrame;

    public Cpu Cpu { get; }
    public Ppu Ppu { get; }
    private Rom Rom { get; }

    public Bus()
    {
        Cpu = new Cpu(this);
        Rom = new Rom("NesRoms/pacman.nes");
        Ppu = new Ppu(Rom);
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

        if (address is 0x2000 or 0x2001 or 0x2003 or 0x2005 or 0x2006 or 0x4014)
            throw new Exception($"Attempt to read from write-only PPU address {address}");

        if (address is 0x2002) // status register
        {
            return Ppu.ReadFromPpuStatus();
        }

        if (address is 0x2004) // oam data
        {
            return Ppu.ReadFromOamData();
        }

        if (address is 0x2007) // read
        {
            return Ppu.Read();
        }

        if (address is >= 0x2008 and < 0x4000)
        {
            // mirroring 0010 0000 0000 0111 - 0x2007
            // 0x2000 - 0x2007
            // 0x2008 - 0x200F
            // 0x2010 - 0x2017
            // 0x2018 - 0x201F

            address &= 0b0010_0000_0000_0111;

            return Read(address);
        }

        if (address is >= 0x8000 and < 0x10000)
        {
            return Rom.ReadPrg(address);
        }

        return 0x00;
    }

    public void Write(uint address, byte data)
    {
        if (address is >= 0x0000 and < 0x2000)
        {
            // mirroring 0000 0111 1111 1111 - 0x07FF
            // 0x0000 - 0x07FF
            // 0x0800 - 0x0FFF
            // 0x1000 - 0x17FF
            // 0x1800 - 0x1FFF

            address &= 0x07FF;

            _cpuRam[address] = data;
        }

        if (address is 0x2002)
            throw new Exception($"Attempt to write to read-only PPU address {address}");

        if (address is 0x2000) // write control
        {
            Ppu.WriteToPpuCtrl(data);
        }

        if (address is 0x2001) // write mask
        {
            Ppu.WriteToPpuMask(data);
        }

        if (address is 0x2003) // write oam address
        {
            Ppu.WriteToOamAddress(data);
        }

        if (address is 0x2004) // write oam data
        {
            Ppu.WriteToOamData(data);
        }

        if (address is 0x2005) // write scroll
        {
            Ppu.WriteToPpuScroll(data);
        }

        if (address is 0x2006) // write address
        {
            Ppu.WriteToPpuAddress(data);
        }

        if (address is 0x2007) // write
        {
            Ppu.Write(data);
        }

        if (address is 0x2014)
        {
            var buffer = new byte[256];
            var hi = data << 8;

            for (var i = 0; i < 256; i++)
                buffer[i] = Read((uint)(hi + i));

            Ppu.WriteToOamDma(buffer);
        }

        if (address is >= 0x2008 and < 0x4000)
        {
            // mirroring 0010 0000 0000 0111 - 0x2007
            // 0x2000 - 0x2007
            // 0x2008 - 0x200F
            // 0x2010 - 0x2017
            // 0x2018 - 0x201F

            address &= 0b0010_0000_0000_0111;

            Write(address, data);
        }

        if (address is >= 0x8000 and < 0x10000)
        {
            throw new Exception("cannot write to prg rom");
        }
    }

    private bool PollNmiStatus()
    {
        if (Ppu.PollNmiInterrupt() == 0)
            return false;

        if (Ppu.PollNmiInterrupt() == 1)
            return true;

        throw new Exception("invalid nmi state");
    }

    private bool _first = true;

    public void Tick()
    {
        if (_first)
        {
            Cpu.Reset();
            Ppu.Tick(24);
            _first = false;
        }

        if (PollNmiStatus())
        {
            Cpu.Nmi();
        }

        // move to cpu???
        var cycles = Cpu.ExecuteSingleInstruction();

        if (Ppu.Tick(cycles * 3))
        {
            DrawFrame?.Invoke(this, EventArgs.Empty);
        }
    }
}