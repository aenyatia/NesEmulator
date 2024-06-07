using NesEmulator.Core;
using NesEmulator.Core.CartridgeModule;
using NesEmulator.Core.ControllerModule;
using NesEmulator.Render;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace NesEmulator;

public class Nes
{
    private const uint Fps = 60;
    private const uint Width = 1024;
    private const uint Height = 1024;
    private const string Title = "Nes Emulator";

    private RenderWindow Window { get; }
    private Bus Bus { get; } = new(NesFileLoader.LoadNesFile("NesRoms/mario.nes"));

    private Dictionary<Keyboard.Key, ControllerButton> _keyMap = new()
    {
        { Keyboard.Key.Up, ControllerButton.Up },
        { Keyboard.Key.Down, ControllerButton.Down },
        { Keyboard.Key.Left, ControllerButton.Left },
        { Keyboard.Key.Right, ControllerButton.Right },
        { Keyboard.Key.Z, ControllerButton.ButtonA },
        { Keyboard.Key.X, ControllerButton.ButtonB },
        { Keyboard.Key.Space, ControllerButton.Select },
        { Keyboard.Key.Enter, ControllerButton.Start },
    };

    public Nes()
    {
        Window = new RenderWindow(new VideoMode(Width, Height), Title);
        Window.Closed += (_, _) => Window.Close();
        Window.KeyPressed += (_, args) =>
        {
            if (_keyMap.TryGetValue(args.Code, out var controllerButton))
            {
                Bus.Controller.SetButtonState(controllerButton, true);
            }
        };
        Window.KeyReleased += (_, args) =>
        {
            if (_keyMap.TryGetValue(args.Code, out var controllerButton))
            {
                Bus.Controller.SetButtonState(controllerButton, false);
            }
        };
        Window.SetFramerateLimit(Fps);
    }

    public void Run()
    {
        var texture = new Texture(256, 240);
        var screen = new Sprite(texture);
        screen.Scale = new Vector2f(4, 4);

        var engine = new Engine();
        var frame = new Frame();
        var colors = new Colors();

        Bus.DrawFrame += (_, _) =>
        {
            engine.Render(Bus.Ppu, Bus.Cartridge, frame, colors);
            screen.Texture.Update(frame);
        };

        while (Window.IsOpen)
        {
            Window.DispatchEvents();
            Window.Clear();
            Window.Draw(screen);
            Window.Display();

            for (var i = 0; i < 200; i++)
                Bus.Tick();
        }
    }

    public void RunSnake()
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
        for (ushort i = 0x0200; i < 0x0600; i++)
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

    public void RunTile()
    {
        const uint textureWidth = 256u;
        const uint textureHeight = 240u;

        var texture = new Texture(textureWidth, textureHeight);
        var chrRom = NesFileLoader.LoadNesFile("NesRoms/mario.nes").ChrRom;
        var frame = new Sprite(texture);

        // var frameData = ShowTile(ref chrRom, 0, 0);
        var frameData = ShowTiles(chrRom);
        texture.Update(frameData);

        frame.Scale = new Vector2f(8, 8);

        while (Window.IsOpen)
        {
            Window.DispatchEvents();
            Window.Clear();
            Window.Draw(frame);
            Window.Display();
        }
    }

    private Frame ShowTiles(ReadOnlyMemory<byte> chrRom)
    {
        var frame = new Frame();
        var colors = new Colors();

        for (var bank = 0; bank < 1; bank++)
        {
            for (var tile = 0; tile < 256; tile++)
            {
                var tileStartIndex = bank * 0x1000 + tile * 16;
                var tileData = chrRom.Span.Slice(tileStartIndex, 16);

                for (var y = 0; y < 8; y++) // iterate through rows
                {
                    var upper = tileData[y];
                    var lower = tileData[y + 8];

                    // lower       upper
                    // 7654 3210   7654 3210
                    // 1001_0100   0011_1101
                    for (var x = 7; x >= 0; x--) // iterate through cols
                    {
                        var value = ((upper & 0b0000_0001) << 1) | (lower & 0b0000_0001);
                        lower >>= 1;
                        upper >>= 1;

                        var color = value switch
                        {
                            0 => colors[0x01],
                            1 => colors[0x23],
                            2 => colors[0x27],
                            3 => colors[0x30],
                            _ => throw new Exception("unknown color index")
                        };

                        frame.SetPixel(x + (tile % 16) * 8, y + (tile / 16) * 8, color);
                    }
                }
            }
        }

        return frame;
    }

    private Frame ShowTile(ref byte[] chrRom, uint bankNumber, uint tileNumber)
    {
        var frame = new Frame();
        var colors = new Colors();

        var bank = bankNumber * 0x1000;

        var tileStartIndex = bank + tileNumber * 16; // inclusive

        var tile = chrRom.AsSpan().Slice((int)tileStartIndex, 16);

        for (var y = 0; y < 8; y++) // iterate through rows
        {
            var lower = tile[y];
            var upper = tile[y + 8];

            // lower       upper
            // 7654 3210   7654 3210
            // 1001_0100   0011_1101
            for (var x = 7; x >= 0; x--) // iterate through cols
            {
                var value = ((upper & 0b0000_0001) << 1) | (lower & 0b0000_0001);
                lower >>= 1;
                upper >>= 1;

                var color = value switch
                {
                    0 => colors[0x01],
                    1 => colors[0x23],
                    2 => colors[0x27],
                    3 => colors[0x30],
                    _ => throw new Exception("unknown color index")
                };

                frame.SetPixel(x, y, color);
            }
        }

        return frame;
    }
}