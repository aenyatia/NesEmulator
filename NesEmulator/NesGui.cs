using NesEmulator.Core;
using NesEmulator.Core.CartridgeModule;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace NesEmulator;

public class NesGui
{
	private const uint Width = 256; // columns
	private const uint Height = 240; // rows
	private const string Title = "Nes Emulator";

	private RenderWindow Window { get; }
	private Nes Nes { get; }
	private Colors Colors { get; } = new();

	public NesGui()
	{
		Window = new RenderWindow(new VideoMode(Width, Height), Title);
		Window.Closed += (_, _) => Window.Close();
		Window.SetFramerateLimit(120);

		var view = Window.GetView();
		view.Rotation = 90;
		view.Size = new Vector2f(Width, -Height);
		Window.SetView(view);
		Window.Size = new Vector2u(Width * 4, Height * 4);

		Nes = new Nes();
	}

	public void LoadCartridge(string path)
	{
		var cartridge = Cartridge.Create(path);

		Nes.LoadCartridge(cartridge);
	}

	public void StartNes()
	{
		var bitmapData = new byte[Width * Height * 4];

		GetBitmapData(bitmapData);

		var img = new Image(Width, Height, bitmapData);
		var texture = new Texture(img);
		var frame = new Sprite(texture);

		while (Window.IsOpen)
		{
			Nes.Clock();
			
			Window.DispatchEvents();
			Window.Clear();

			Window.Draw(frame);
			
			GetBitmapData(bitmapData);
			UpdateFrame(frame, bitmapData);

			Window.Display();
		}
	}

	private static void UpdateFrame(Sprite frame, byte[] pixels)
	{
		frame.Texture.Update(pixels);
	}

	private void GetBitmapData(byte[] bitmapData)
	{
		var screen = Nes.Ppu.Screen;
		var rows = screen.GetLength(0);
		var cols = screen.GetLength(1);

		for (var row = 0; row < rows; row++)
		for (var col = 0; col < cols; col++)
		{
			var colorIndex = screen[row, col];
			var pixelColor = Colors[colorIndex];
			
			bitmapData[4 * row * Width + 4 * col + 0] = pixelColor.R;
			bitmapData[4 * row * Width + 4 * col + 1] = pixelColor.G;
			bitmapData[4 * row * Width + 4 * col + 2] = pixelColor.B;
			bitmapData[4 * row * Width + 4 * col + 3] = pixelColor.A;
		}
	}
}