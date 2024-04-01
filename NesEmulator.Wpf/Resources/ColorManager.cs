using System.Windows.Media;

namespace NesEmulator.Wpf.Resources;

public static class ColorManager
{
    private static readonly Color Black = Color.FromRgb(0x00, 0x00, 0x00);
    private static readonly Color White = Color.FromRgb(0xFF, 0xFF, 0xFF);
    private static readonly Color Gray = Color.FromRgb(0x80, 0x80, 0x80);
    private static readonly Color Cyan = Color.FromRgb(0x00, 0xFF, 0xFF);
    private static readonly Color Red = Color.FromRgb(0xFF, 0x00, 0x00);
    private static readonly Color Green = Color.FromRgb(0x00, 0xFF, 0x00);
    private static readonly Color Blue = Color.FromRgb(0x00, 0x00, 0xFF);
    private static readonly Color Magenta = Color.FromRgb(0xFF, 0x00, 0xFF);
    private static readonly Color Yellow = Color.FromRgb(0xFF, 0xFF, 0x00);

    public static Color GetColor(byte index)
        => index switch
        {
            0 => Black,
            1 => White,
            2 or 9 => Gray,
            3 or 10 => Red,
            4 or 11 => Green,
            5 or 12 => Blue,
            6 or 13 => Magenta,
            7 or 14 => Yellow,
            _ => Cyan
        };
}