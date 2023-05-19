using NesEmulator.Core;

namespace NesEmulator;

public sealed class Computer
{
	public Cpu Cpu { get; }
	public IMemory Memory { get; }

	public Computer()
	{
		Memory = new Memory();
		Cpu = new Cpu(Memory, sr: 0b00000100);
	}

	public void LoadProgram()
	{
		const string path = "nestest.nes";

		// Load the program data from the ROM
		var romBytes = File.ReadAllBytes(path);
		var romData = new ArraySegment<byte>(romBytes, 0x0010, 0x4000).ToArray();

		// Write into the correct memory location (write twice while we don't have a mapper)
		for (ushort i = 0x8000, j = 0; j < romData.Length; i++, j++)
			Memory.WriteByte(i, romData[j]);

		for (ushort i = 0xC000, j = 0; j < romData.Length; i++, j++)
			Memory.WriteByte(i, romData[j]);
	}

	public void Run(int clock)
	{
		Cpu.Irq();
		
		var file = File.ReadAllLines("nestest.log.txt")
			.Select(line => line.Split(" ", StringSplitOptions.RemoveEmptyEntries))
			.ToList();

		var i = 0;
		var fails = 0;
		while (clock > 0)
		{
			var a = $"A:{Cpu.A:X2}";
			var x = $"X:{Cpu.X:X2}";
			var y = $"Y:{Cpu.Y:X2}";
			var sp = $"SP:{Cpu.SP:X2}";
			var sr = $"P:{Cpu.SR:X2}";
			var pc = $"PC:{Cpu.PC:X4}";
			var cycles = $"CYC:{Cpu.Cycles}";

			var cmp = Compare(pc, a, x, y, sr, sp, cycles);
			
			Console.WriteLine($"[{i}] {pc} - {a} {x} {y} {sr} {sp} {cycles} {cmp}");

			if (cmp == false)
				fails++;
			
			if (i == 3328)
				Console.WriteLine();
			
			Cpu.ExecuteSingleInstruction();

			clock--;
		}

		Console.WriteLine($"Fails: {fails}");

		bool Compare(string pc, string a, string x, string y, string sr, string sp, string cycles)
		{
			var line = file[i++];

			if ($"PC:{line[0]}" != pc) return false;
			if (line.First(s => s.StartsWith("A:")) != a) return false;
			if (line.First(s => s.StartsWith("X:")) != x) return false;
			if (line.First(s => s.StartsWith("Y:")) != y) return false;
			if (line.First(s => s.StartsWith("P:")) != sr) return false;
			if (line.First(s => s.StartsWith("SP:")) != sp) return false;
			if (line.Last() != cycles) return false;
			
			return true;
		}
	}
}