using NesEmulator.Core.CartridgeModule;

namespace NesEmulator.Core;

public static class Emulator
{
	public static void Start(string path)
	{
		var cartridge = Cartridge.Create(path);
		var bus = new Bus();

		bus.InsertCartridge(cartridge);
		bus.Start();
	}
}