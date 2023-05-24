using NesEmulator.Core;
using NesEmulator.Core.CartridgeModule;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace NesEmulator;

public class NesGui
{
	private const uint Width = 128;
	private const uint Height = 128;
	private const string Title = "Nes Emulator";
	private const bool FullScreen = true;

	private RenderWindow Window { get; }
	private Nes Nes { get; }
	private Colors Colors { get; } = new();

	public NesGui()
	{
		Window = new RenderWindow(new VideoMode(Width, Height), Title, FullScreen ? Styles.Fullscreen : Styles.Default);
		Window.Closed += (_, _) => Window.Close();
		Window.SetFramerateLimit(120);

		if (!FullScreen)
		{
			var view = Window.GetView();
			view.Rotation = 90;
			view.Size = new Vector2f(Width, -Height);
			Window.SetView(view);
			Window.Size = new Vector2u(800, 800);
		}

		Nes = new Nes();
	}

	public void LoadCartridge(string path)
	{
		var cartridge = Cartridge.Create(path);

		Nes.LoadCartridge(cartridge);
	}

	public void StartNes()
	{
		Nes.Ppu.X();

		var pixelColors = new Color[128, 128];
		var pixelArray = new byte[128 * 128 * 4];
		for (var i = 0; i < Nes.Ppu.BitMapData.GetLength(0); i++)
		{
			for (var j = 0; j < Nes.Ppu.BitMapData.GetLength(1); j++)
			{
				var color = Colors.GetColor(Nes.Ppu.BitMapData[i, j]);

				pixelColors[i, j] = color;

				pixelArray[4 * i * 128 + 4 * j] = color.R;
				pixelArray[4 * i * 128 + 4 * j + 1] = color.G;
				pixelArray[4 * i * 128 + 4 * j + 2] = color.B;
				pixelArray[4 * i * 128 + 4 * j + 3] = color.A;
			}
		}

		var img = new Image(pixelColors);
		var texture = new Texture(img);
		var frame = new Sprite(texture);

		while (Window.IsOpen)
		{
			Window.DispatchEvents();
			Window.Clear();

			Window.Draw(frame);
			UpdateFrame(frame, pixelArray);

			Window.Display();
		}
	}

	private void UpdateFrame(Sprite frame, byte[] pixels)
	{
		frame.Texture.Update(pixels);
	}
}