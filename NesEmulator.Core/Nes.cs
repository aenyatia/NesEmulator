using NesEmulator.Core.CartridgeModule;
using NesEmulator.Core.ControllerModule;
using NesEmulator.Core.CpuModule;
using NesEmulator.Core.PpuModule;

namespace NesEmulator.Core;

public class Nes
{
	public Cpu Cpu { get; }
	public CpuMemory CpuMemory { get; }
	public Ppu Ppu { get; }
	public Controller Controller { get; }
	public Cartridge? Cartridge { get; private set; }


	public bool IsRunning { get; set; }
	public int SystemClock { get; set; }


	public Nes()
	{
		Cpu = new Cpu(this);
		CpuMemory = new CpuMemory(this);
		Ppu = new Ppu(this);
		Controller = new Controller();
	}

	public void LoadCartridge(Cartridge cartridge)
	{
		Cartridge = cartridge;
	}

	public void Reset()
	{
		Cpu.Reset();
		SystemClock = 0;
	}

	public void Clock()
	{
		if (Cartridge is null)
			throw new NullReferenceException();

		Ppu.Clock();

		if (SystemClock % 3 == 0)
			Cpu.Clock();

		if (Ppu.Nmi)
		{
			Ppu.Nmi = false;
			Cpu.Nmi();
		}

		SystemClock++;
	}
}