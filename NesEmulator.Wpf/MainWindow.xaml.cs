using System.Windows;
using NesEmulator.Wpf.UserControls;

namespace NesEmulator.Wpf;

public partial class MainWindow
{
    private TileViewer? _sprites;
    private TileViewer? _background;
    private GameViewer? _gameViewer;

    private TileViewer SpritesTile => _sprites ??= new TileViewer(0x0000);
    private TileViewer BackgroundTile => _background ??= new TileViewer(0x1000);
    private GameViewer GameViewer => _gameViewer ??= new GameViewer();

    public MainWindow()
    {
        InitializeComponent();
    }

    private void ButtonSprites_OnClick(object sender, RoutedEventArgs e)
    {
        Content.Content = SpritesTile;
    }

    private void ButtonBackground_OnClick(object sender, RoutedEventArgs e)
    {
        Content.Content = BackgroundTile;
    }

    private void ButtonEmulator_OnClick(object sender, RoutedEventArgs e)
    {
        Content.Content = GameViewer;
    }
}