using NesEmulator.Core.CpuModule;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace NesEmulator.SnakeGui;

public class SnakeGame
{
    private static readonly Time TimePerFrame = Time.FromSeconds(1f / 30f);
    private const uint Width = 800; // columns
    private const uint Height = 800; // rows
    private const string Title = "Snake Emulator";

    private RenderWindow Window { get; }
    private Cpu Cpu { get; }
    private Ram Ram { get; } = new();

    public SnakeGame()
    {
        Cpu = new Cpu(Ram);

        Window = new RenderWindow(new VideoMode(Width, Height), Title);
        Window.Closed += (_, _) => Window.Close();
        Window.SetFramerateLimit(30);
        Window.KeyPressed += (sender, args) =>
        {
            switch (args.Code)
            {
                case Keyboard.Key.W:
                    Ram.Write(0xFF, 0x77);
                    break;
                case Keyboard.Key.S:
                    Ram.Write(0xFF, 0x73);
                    break;
                case Keyboard.Key.A:
                    Ram.Write(0xFF, 0x61);
                    break;
                case Keyboard.Key.D:
                    Ram.Write(0xFF, 0x64);
                    break;
            }
        };

        var view = Window.GetView();
        view.Rotation = 90;
        view.Size = new Vector2f(Width, -Height);
        Window.SetView(view);
    }

    public void LoadAndRun()
    {
        Ram.Load(Code.Snake, 0x0600);
        Ram.Write(0xFFFC, 0x00);
        Ram.Write(0xFFFD, 0x06);
        Cpu.Reset();

        StartNes();
    }

    private void StartNes()
    {
        var texture = new Texture(32, 32);
        var screen = new Image(32, 32, Color.Yellow);
        var frame = new Sprite(texture);
        frame.Scale = new Vector2f(20f, 20f);

        texture.Update(screen);

        var clock = new Clock();
        var timeSinceLastUpdate = Time.Zero;

        while (Window.IsOpen)
        {
            // var elapsedTime = clock.Restart();
            // timeSinceLastUpdate += elapsedTime;

            // Cpu.ExecuteSingleInstruction();
            //
            // while (timeSinceLastUpdate > TimePerFrame)
            // {
            //     Ram.Write(0xFE, (uint)Random.Shared.Next(1, 16));
            //     Window.DispatchEvents();
            //     Window.Clear(Color.Magenta);
            //     Window.Draw(frame);
            //     Window.Display();
            //
            //     timeSinceLastUpdate -= TimePerFrame;
            //
            //     if (UpdateImg(screen))
            //         texture.Update(screen);
            // }

            for (var i = 0; i < 100; i++)
                Cpu.ExecuteSingleInstruction();
            
            Ram.Write(0xFE, (uint)Random.Shared.Next(1, 16));
            Window.DispatchEvents();
            Window.Clear(Color.Magenta);
            Window.Draw(frame);
            Window.Display();
            
            if (UpdateImg(screen))
                texture.Update(screen);
        }
    }

    private bool UpdateImg(Image img)
    {
        var x = 0u;
        var m = false;
        for (uint i = 0x0200; i < 0x0600; i++, x++)
        {
            var colorIndex = Ram.Read(i);
            var color = img.GetPixel(x / 32, x % 32);

            img.SetPixel(x / 32, x % 32, GetColor(colorIndex));
            m = true;
        }

        return m;
    }

    private void UpdateFrame(ref byte[] screenState)
    {
        var frameIndex = 0;
        for (uint i = 0x0200; i < 0x0600; i++)
        {
            var colorIndex = Ram.Read(i);
            var color = GetColor(colorIndex);

            screenState[frameIndex] = color.R;
            screenState[frameIndex + 1] = color.G;
            screenState[frameIndex + 2] = color.B;
            screenState[frameIndex + 3] = color.A;

            frameIndex += 4;
        }
    }

    private Color GetColor(uint index)
    {
        if (index == 0) return Color.Black;
        if (index == 1) return Color.White;
        if (index is 2 or 9) return Color.Cyan; // random color
        if (index is 3 or 10) return Color.Red;
        if (index is 4 or 11) return Color.Green;
        if (index is 5 or 12) return Color.Blue;
        if (index is 6 or 13) return Color.Magenta;
        if (index is 7 or 14) return Color.Yellow;

        return Color.Cyan;
    }
}