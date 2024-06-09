using NesEmulator.Core.CartridgeModule;
using NesEmulator.Core.CpuModule;
using NesEmulator.Core.PpuModule;

namespace NesEmulator.Core;

public class Bus
{
    public Bus(Cartridge cartridge)
    {
        Cpu = new Cpu(this);
        Ppu = new Ppu(cartridge);
        Cartridge = cartridge;
        Ram = new Ram();
    }

    public Cpu Cpu { get; }
    public Ppu Ppu { get; }
    public Ram Ram { get; }
    public Cartridge Cartridge { get; }

    public uint SystemClockCounter { get; private set; }

    public byte CpuRead(ushort address)
    {
        byte data = 0x00;

        if (Cartridge.CpuRead(address, ref data))
        {
        }
        else if (address is >= 0x0000 and <= 0x1FFF)
        {
            data = Ram.CpuRead(address);
        }
        else if (address is >= 0x2000 and <= 0x3FFF)
        {
            data = Ppu.CpuRead(address);
        }
        else if (address is 0x4016 or 0x4017)
        {
            // controller
        }

        return data;
    }

    public void WriteCpu(ushort address, byte data)
    {
        if (Cartridge.CpuWrite(address, data))
        {
        }
        else if (address is >= 0x0000 and <= 0x1FFF)
        {
            Ram.CpuWrite(address, data);
        }
        else if (address is >= 0x2000 and <= 0x3FFF)
        {
            Ppu.CpuWrite(address, data);
        }
        else if (address is 0x4016 or 0x4017)
        {
            // controller
        }
    }

    public void Reset()
    {
        Cpu.Reset();
        Ppu.Reset();
        Cartridge.Reset();

        SystemClockCounter = 0;
    }

    public void Clock()
    {
        Ppu.Clock();

        if (SystemClockCounter % 3 == 0)
        {
            Cpu.Clock();
        }

        if (Ppu.Nmi)
        {
            Ppu.Nmi = false;
            Cpu.Nmi();
        }

        SystemClockCounter += 1;
    }
}