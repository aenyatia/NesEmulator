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

	public void Start()
	{
		if (Cartridge is null)
			throw new NullReferenceException();

		while (IsRunning)
		{
			var cycles = Cpu.ExecuteSingleInstruction();

			// 1 cpu cycle = 3 ppu cycle
			for (var i = 0; i < cycles * 3; i++)
			{
				Ppu.Clock();
			}
		}
	}
}