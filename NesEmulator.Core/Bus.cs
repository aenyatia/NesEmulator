using NesEmulator.Core.CartridgeModule;
using NesEmulator.Core.CpuModule;
using NesEmulator.Core.PpuModule;

namespace NesEmulator.Core;

public class Bus
{
    public event EventHandler? DrawFrame;

    public Cpu Cpu { get; }
    public Ppu Ppu { get; }
    private CpuRam CpuRam { get; } = new();
    public Cartridge Cartridge { get; }

    public Bus(Cartridge cartridge)
    {
        Cpu = new Cpu(this);
        Ppu = new Ppu(cartridge);
        Cartridge = cartridge;

        Cpu.Reset();
        Ppu.Tick(3 * 7);
    }

    public byte Read(ushort address)
    {
        return address switch
        {
            <= 0x1FFF => CpuRam.Read(address),
            <= 0x3FFF => Ppu.Read(address),
            <= 0x4017 => 0,
            <= 0x7FFF => throw new Exception("read from: io registers, eROM, sRAM"),
            <= 0xFFFF => Cartridge.ReadPrgRom(address),
        };
    }

    public void Write(ushort address, byte data)
    {
        switch (address)
        {
            case <= 0x1FFF:
                CpuRam.Write(address, data);
                break;
            case <= 0x3FFF:
                Ppu.Write(address, data);
                break;
            case <= 0x4017:
                break;
            case <= 0x7FFF:
                throw new Exception("write to: io registers, eROM, sRAM");
            case <= 0xFFFF:
                throw new Exception("cannot write to prg rom");
        }
    }

    // ???
    private bool PollNmiStatus()
    {
        return Ppu.PollNmiInterrupt();
    }

    public void Tick()
    {
        if (PollNmiStatus())
        {
            Cpu.Nmi();
        }

        var cycles = Cpu.ExecuteSingleInstruction();

        if (Ppu.Tick(cycles * 3))
        {
            DrawFrame?.Invoke(this, EventArgs.Empty);
        }
    }
}