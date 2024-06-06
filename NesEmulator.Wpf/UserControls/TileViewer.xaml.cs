using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NesEmulator.Core.CartridgeModule;

namespace NesEmulator.Wpf.UserControls;

public sealed partial class TileViewer
{
    private readonly Color[] _colors =
    [
        Color.FromRgb(146, 144, 255),
        Color.FromRgb(181, 49, 32),
        Color.FromRgb(234, 158, 34),
        Color.FromRgb(107, 109, 0)
    ];

    public TileViewer(int offset)
    {
        InitializeComponent();

        LoadTiles(offset);
    }

    private void LoadTiles(int offset)
    {
        var cartridge = NesFileLoader.LoadNesFile("../../../../NesEmulator/NesRoms/mario.nes");

        for (var tileY = 0; tileY < 16; tileY++)
        {
            for (var tileX = 0; tileX < 16; tileX++)
            {
                var start = offset + tileY * 16 * 16 + tileX * 16;
                var tilePixels = LoadTile(cartridge.ChrRom[start..(start + 16)]);
                var tileBitmap = BitmapSource.Create(8, 8, 96, 96, PixelFormats.Bgr32, null, tilePixels, 8 * 4);
                var tile = new Image
                {
                    Source = tileBitmap
                };
                var border = new Border
                {
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    // Margin = new Thickness(0.25)
                };

                RenderOptions.SetBitmapScalingMode(tile, BitmapScalingMode.NearestNeighbor);

                border.Child = tile;

                Grid.SetRow(border, tileY);
                Grid.SetColumn(border, tileX);

                GridContainer.Children.Add(border);
            }
        }
    }

    private byte[] LoadTile(byte[] tileData)
    {
        var tilePixels = new byte[8 * 8 * 4];

        // iterate through tile rows
        for (var y = 0; y < 8; y++)
        {
            var hiByte = tileData[y];
            var loByte = tileData[y + 8];

            // iterate through tile cols
            for (var x = 7; x >= 0; x--)
            {
                var lsb = hiByte & 0b0000_0001;
                var msb = loByte & 0b0000_0001;

                hiByte >>= 1;
                loByte >>= 1;

                var colorIndex = msb << 1 | lsb;
                var color = _colors[colorIndex];

                var pixelIndex = (y * 8 + x) * 4;

                tilePixels[pixelIndex] = color.B;
                tilePixels[pixelIndex + 1] = color.G;
                tilePixels[pixelIndex + 2] = color.R;
                tilePixels[pixelIndex + 3] = color.A;
            }
        }

        return tilePixels;
    }

    private static BitmapImage LoadImage()
    {
        const string relativePath = "Resources/horo.jpeg";
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var path = Path.Combine(baseDirectory, relativePath);

        var uri = new Uri(path);
        var bitmap = new BitmapImage(uri);

        return bitmap;
    }
}