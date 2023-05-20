using NesEmulator.Core.CartridgeModule;
using NesEmulator.Core.ControllerModule;
using NesEmulator.Core.CpuModule;
using NesEmulator.Core.PpuModule;

namespace NesEmulator.Core;

public class Bus
{
	public Cpu Cpu { get; }
	public CpuMemory CpuMemory { get; }
	public Ppu Ppu { get; }
	public Controller Controller { get; }
	public Cartridge? Cartridge { get; private set; }

	public Bus()
	{
		Cpu = new Cpu(this);
		CpuMemory = new CpuMemory(this);
		Ppu = new Ppu(this);
		Controller = new Controller();
	}

	public void InsertCartridge(Cartridge cartridge)
	{
		Cartridge = cartridge;
	}

	public void Start()
	{
		if (Cartridge is null)
			throw new NullReferenceException();
	}
}