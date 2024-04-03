using NesEmulator.Core;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace NesEmulator;

public class Nes
{
    private const uint Fps = 60;
    private const uint Width = 640;
    private const uint Height = 640;
    private const string Title = "Nes Emulator";

    private RenderWindow Window { get; }
    private Bus Bus { get; } = new();

    public Nes()
    {
        Window = new RenderWindow(new VideoMode(Width, Height), Title);
        Window.Closed += (_, _) => Window.Close();
        Window.KeyPressed += (_, args) =>
        {
            switch (args.Code)
            {
                case Keyboard.Key.W:
                    Bus.Write(0xFF, 0x77);
                    break;
                case Keyboard.Key.S:
                    Bus.Write(0xFF, 0x73);
                    break;
                case Keyboard.Key.A:
                    Bus.Write(0xFF, 0x61);
                    break;
                case Keyboard.Key.D:
                    Bus.Write(0xFF, 0x64);
                    break;
            }
        };
        Window.SetFramerateLimit(Fps);
    }

    public void Load()
    {
        Bus.Cpu.Reset();
    }

    public void Run()
    {
        var texture = new Texture(32, 32);
        var frame = new Sprite(texture);

        frame.Scale = new Vector2f(Width / 32f, Height / 32f);

        var frameData = new byte[32 * 32 * 4];
        while (Window.IsOpen)
        {
            Window.DispatchEvents();
            Window.Clear();
            Window.Draw(frame);
            Window.Display();

            for (var i = 0; i < 100; i++)
                Bus.Cpu.ExecuteSingleInstruction();

            Bus.Write(0xFE, (byte)Random.Shared.Next(1, 16));

            UpdateFrameData(ref frameData);
            texture.Update(frameData);
        }
    }

    private void UpdateFrameData(ref byte[] frameData)
    {
        var pixelIndex = 0;
        for (uint i = 0x0200; i < 0x0600; i++)
        {
            var color = GetColor(Bus.Read(i));

            frameData[pixelIndex] = color.R;
            frameData[pixelIndex + 1] = color.G;
            frameData[pixelIndex + 2] = color.B;
            frameData[pixelIndex + 3] = color.A;

            pixelIndex += 4;
        }
    }

    private static Color GetColor(uint index)
    {
        if (index == 0) return Color.Black;
        if (index == 1) return Color.White;
        if (index is 2 or 9) return Color.Cyan; // gray
        if (index is 3 or 10) return Color.Red;
        if (index is 4 or 11) return Color.Green;
        if (index is 5 or 12) return Color.Blue;
        if (index is 6 or 13) return Color.Magenta;
        if (index is 7 or 14) return Color.Yellow;

        return Color.Cyan;
    }
}