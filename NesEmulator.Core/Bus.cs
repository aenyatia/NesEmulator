using NesEmulator.Core.CartridgeModule;
using NesEmulator.Core.ControllerModule;
using NesEmulator.Core.CpuModule;
using NesEmulator.Core.PpuModule;

namespace NesEmulator.Core;

public class Bus
{
    public event EventHandler? DrawFrame;

    public Cpu Cpu { get; }
    public Ppu Ppu { get; }
    private Ram Ram { get; }
    public Cartridge Cartridge { get; }
    public Controller Controller { get; }

    public int ClockCounter { get; private set; }

    public Bus(Cartridge cartridge)
    {
        Cpu = new Cpu(this);
        Ppu = new Ppu(cartridge, this);
        Ram = new Ram();
        Cartridge = cartridge;
        Controller = new Controller();
    }

    public byte Read(ushort address)
    {
        return address switch
        {
            >= 0x0000 and <= 0x1FFF => Ram.Read(address),
            >= 0x2000 and <= 0x3FFF => Ppu.Read(address),
            >= 0x4000 and <= 0x4015 => 0, // unimplemented apu
            0x4016 => Controller.Read(),
            0x4017 => 0, // unimplemented controller 2
            >= 0x4018 and <= 0x401F => throw new Exception("apu and i/o functionality disabled"),
            >= 0x4020 and <= 0x7FFF => throw new Exception("unimplemented eRom, sRAM"),
            >= 0x8000 and <= 0xFFFF => Cartridge.ReadPrgRom(address)
        };
    }

    public void Write(ushort address, byte data)
    {
        switch (address)
        {
            case >= 0x0000 and <= 0x1FFF:
                Ram.Write(address, data);
                break;

            case >= 0x2000 and <= 0x3FFF:
                Ppu.Write(address, data);
                break;

            case >= 0x4000 and <= 0x4015 and not 0x4014:
                // unimplemented apu
                break;

            case 0x4014: // move implementation here ???
                break;

            case 0x4016:
                Controller.Write(data);
                break;

            case <= 0x4017:
                // unimplemented controller 2
                break;

            case >= 0x4018 and <= 0x401F:
                throw new Exception("apu and i/o functionality disabled");

            case >= 0x4020 and <= 0x7FFF:
                throw new Exception("unimplemented eRom, sRAM");

            case >= 0x8000 and <= 0xFFFF:
                throw new Exception("cannot write to prg rom");
        }
    }
    
    public void Reset()
    {
        Cpu.Reset();

        ClockCounter = 0;
    }

    public void Clock()
    {
        if (Ppu.PollNmiInterrupt())
        {
            Cpu.Nmi();
        }

        var cycles = Cpu.ExecuteSingleInstruction();

        if (Ppu.Tick(cycles * 3))
        {
            DrawFrame?.Invoke(this, EventArgs.Empty);
        }

        ClockCounter += 1;
    }
}