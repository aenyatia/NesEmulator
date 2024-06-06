using System.Windows;
using NesEmulator.Wpf.UserControls;

namespace NesEmulator.Wpf;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ButtonSprites_OnClick(object sender, RoutedEventArgs e)
    {
        var sprites = new TileViewer(0x0000)
        {
            Padding = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Content.Content = sprites;
    }


    private void ButtonBackground_OnClick(object sender, RoutedEventArgs e)
    {
        var background = new TileViewer(0x1000)
        {
            Padding = new Thickness(10),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Content.Content = background;
    }
}