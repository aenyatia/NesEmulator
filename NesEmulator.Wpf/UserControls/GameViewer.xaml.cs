using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NesEmulator.Core;
using NesEmulator.Core.CartridgeModule;
using NesEmulator.Core.PpuModule;

namespace NesEmulator.Wpf.UserControls;

public partial class GameViewer
{
    private const int ScreenWidth = 256;
    private const int ScreenHeight = 240;
    private const int BytesPerPixel = 4;

    private readonly Bus _bus = new(NesFileLoader.LoadNesFile("../../../../NesEmulator/NesRoms/kong.nes"));
    private readonly byte[] _pixels = new byte[ScreenWidth * ScreenHeight * BytesPerPixel];
    private readonly Random _random = new();
    private readonly Colors _colors = new();

    public WriteableBitmap Bitmap { get; }

    public GameViewer()
    {
        InitializeComponent();
        DataContext = this;
        Bitmap = new WriteableBitmap(ScreenWidth, ScreenHeight, 96, 96, PixelFormats.Bgr32, null);

        _bus.DrawFrame += BusOnDrawFrame;
        _bus.Reset();
    }

    private void BusOnDrawFrame(object? sender, EventArgs e)
    {
        UpdateBackground();

        // Bitmap.Lock();
        //
        // Marshal.Copy(_pixels, 0, Bitmap.BackBuffer, _pixels.Length);
        //
        // Bitmap.AddDirtyRect(new Int32Rect(0, 0, ScreenWidth, ScreenHeight));
        //
        // Bitmap.Unlock();
        
        Dispatcher.Invoke(() =>
        {
            Bitmap.Lock();
        
            Marshal.Copy(_pixels, 0, Bitmap.BackBuffer, _pixels.Length);
        
            Bitmap.AddDirtyRect(new Int32Rect(0, 0, ScreenWidth, ScreenHeight));
        
            Bitmap.Unlock();
        });

        Thread.Sleep(32);
    }

    private void UpdateBackground()
    {
        var ppu = _bus.Ppu;
        var cartridge = _bus.Cartridge;

        // draw background
        var bank = ppu.ControlRegister.BackgroundPatternTableAddress();

        for (ushort i = 0; i < 0x3C0; i++)
        {
            var tile = ppu.VRam.Read(i);
            var tileX = i % 32;
            var tileY = i / 32;

            var tileData = cartridge.ChrRom.AsSpan().Slice((int)(bank + tile * 16), 16);
            var palette = BackgroundPalette(ppu, tileX, tileY);

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
                        0 => _colors[palette[0]],
                        1 => _colors[palette[1]],
                        2 => _colors[palette[2]],
                        3 => _colors[palette[3]],
                        _ => throw new Exception("unknown color index")
                    };

                    SetPixel(tileX * 8 + x, tileY * 8 + y, color);
                }
            }
        }
    }

    private void SetPixel(int x, int y, Color color)
    {
        var baseIndex = y * ScreenWidth * 4 + x * 4;

        if (baseIndex + 3 < _pixels.Length)
        {
            _pixels[baseIndex] = color.B;
            _pixels[baseIndex + 1] = color.G;
            _pixels[baseIndex + 2] = color.R;
            _pixels[baseIndex + 3] = color.A;
        }
    }

    private byte[] BackgroundPalette(Ppu ppu, int tileColumn, int tileRow)
    {
        var attributeTable = tileRow / 4 * 8 + tileColumn / 4;
        var attributeByte = ppu.VRam.Read((ushort)(0x3c0 + attributeTable));

        var paletteIndex = (tileColumn % 4 / 2, tileRow % 4 / 2);

        byte x = 0;
        if (paletteIndex == (0, 0))
        {
            x = (byte)(attributeByte & 0b0000_0011);
        }

        if (paletteIndex == (1, 0))
        {
            x = (byte)((attributeByte >> 2) & 0b0000_0011);
        }

        if (paletteIndex == (0, 1))
        {
            x = (byte)((attributeByte >> 4) & 0b0000_0011);
        }

        if (paletteIndex == (1, 1))
        {
            x = (byte)((attributeByte >> 6) & 0b0000_0011);
        }

        var paletteStart = 1 + x * 4;

        return
        [
            ppu.PaletteTable[0],
            ppu.PaletteTable[paletteStart],
            ppu.PaletteTable[paletteStart + 1],
            ppu.PaletteTable[paletteStart + 2]
        ];
    }

    private bool _x;

    private void ButtonBase_OnClick_Start(object sender, RoutedEventArgs e)
    {
        _x = !_x;

        if (_x)
        {
            
            Task.Run(() =>
            {
                while (_x)
                {
                    _bus.Clock();
                }
            });
        }
    }

    private void ButtonBase_OnClick_Random(object? sender, RoutedEventArgs? e)
    {
        UpdateBitmap();
    }

    private void UpdateBitmap()
    {
        RandomPixels();

        Bitmap.Lock();

        Marshal.Copy(_pixels, 0, Bitmap.BackBuffer, _pixels.Length);

        Bitmap.AddDirtyRect(new Int32Rect(0, 0, ScreenWidth, ScreenHeight));

        Bitmap.Unlock();
    }

    private void RandomPixels()
    {
        for (var y = 0; y < ScreenHeight; y++)
        {
            for (var x = 0; x < ScreenWidth; x++)
            {
                var index = (y * ScreenWidth + x) * 4;

                var r = (byte)_random.Next(0, 256);
                var g = (byte)_random.Next(0, 256);
                var b = (byte)_random.Next(0, 256);

                _pixels[index] = b;
                _pixels[index + 1] = g;
                _pixels[index + 2] = r;
                _pixels[index + 3] = 255;
            }
        }
    }
}