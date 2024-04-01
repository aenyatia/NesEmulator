using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NesEmulator.Core.CpuModule;
using NesEmulator.Wpf.Resources;

namespace NesEmulator.Wpf;

public partial class MainWindow : Window
{
    private readonly Ram _ram;
    private readonly Cpu _cpu;

    private readonly WriteableBitmap _frame;
    private bool _isRunning = true;

    public MainWindow()
    {
        InitializeComponent();

        _ram = new Ram();
        _cpu = new Cpu(_ram);

        _ram.Load(Code.Snake, 0x0600);
        _ram.Write(0xFFFC, 0x00);
        _ram.Write(0xFFFD, 0x06);
        _cpu.Reset();

        // Tworzenie nowej bitmapy o rozmiarze 32x32 pikseli
        int width = 32;
        int height = 32;
        int stride = width * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8);
        byte[] pixelData = new byte[stride * height];

        // Ustawienie kolorów pikseli (przykładowe kombinacje RGB)
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * stride + 4 * x;
                pixelData[index] = (byte)Random.Shared.Next(0, 256); // Składowa niebieska
                pixelData[index + 1] = (byte)Random.Shared.Next(0, 256); // Składowa zielona
                pixelData[index + 2] = (byte)Random.Shared.Next(0, 256); // Składowa czerwona
                pixelData[index + 3] = 255; // Składowa alfa (255 - pełna nieprzezroczystość)
            }
        }

        var screen = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixelData, stride);
        _frame = new WriteableBitmap(screen);

        image.Source = _frame;
    }

    private Color RandomColor()
    {
        Random random = new Random();
        return Color.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
    }

    public bool Start()
    {
        _ram.Write(0xFE, (uint)Random.Shared.Next(1, 16));
        _cpu.ExecuteSingleInstruction();

        return UpdateScreen();
    }

    private bool UpdateScreen()
    {
        _frame.Lock();

        var bufferPtr = _frame.BackBuffer;
        var bpp = _frame.Format.BitsPerPixel / 8;
        var mod = false;
        for (uint i = 0x0200; i < 0x0600; i++)
        {
            var color = ColorManager.GetColor((byte)_ram.Read(i));
            // var color = ColorManager.GetColor((byte)Random.Shared.Next(0, 20));

            var colorData = color.R << 16;
            colorData |= color.G << 8;
            colorData |= color.B;

            var readColor = Marshal.ReadInt32(bufferPtr) & 0x00FFFFFF;

            if (readColor != colorData)
            {
                Marshal.WriteInt32(bufferPtr, colorData);
                mod = true;
            }


            bufferPtr += bpp;
        }

        _frame.AddDirtyRect(new Int32Rect(0, 0, 32, 32));
        _frame.Unlock();

        return mod;
    }

    private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Q)
        {
            _isRunning = false;
        }

        if (e.Key == Key.W)
        {
            _ram.Write(0xFF, 0x77);
        }

        if (e.Key == Key.S)
        {
            _ram.Write(0xFF, 0x73);
        }

        if (e.Key == Key.A)
        {
            _ram.Write(0xFF, 0x61);
        }

        if (e.Key == Key.D)
        {
            _ram.Write(0xFF, 0x64);
        }
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        while (_isRunning)
        {
            if (Start())
            {
                await Task.Delay(32);
            }
        }
    }
}