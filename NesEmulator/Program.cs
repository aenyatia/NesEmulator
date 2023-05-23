using NesEmulator;
using NesEmulator.Core.CartridgeModule;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

var patterTable = Cartridge.Create("mario.nes").ChrRom;
var colors = new Colors();
var palette = new byte[]
{
	0x0e, // background 00
	0x12, // first color 01 
	0x15, // second color 10
	0x18 // third color 11
};

const int rowPixels = 128;
const int colPixels = 128;
const int rowTiles = 16;
const int colTiles = 16;

var pixelTable = new byte[rowPixels, colPixels];
var pixels = new Color[128, 128];

// patterTable
for (var i = 0; i < rowTiles; i++)
for (var j = 0; j < colTiles; j++)
{
	// begin of tile
	var offset = i * 256 + j * 16;

	// tile 8 x 8 iterate over 8 rows
	for (var row = 0; row < 8; row++)
	{
		// 0 - left, 1 - right
		var lsb = patterTable[0 * 0x1000 + offset + row + 0];
		var msb = patterTable[0 * 0x1000 + offset + row + 8];

		// iterate over columns
		for (var col = 0; col < 8; col++)
		{
			var pixel = (lsb & 0x01) + (msb & 0x01);
			lsb >>= 1;
			msb >>= 1;

			//        _ _ _ _ _ _ _ _
			// col =  7 6 5 4 3 2 1 

			pixelTable[i * 8 + row, j * 8 + (7 - col)] = (byte)pixel;
		}
	}
}

for (var i = 0; i < rowPixels; i++)
for (var j = 0; j < colPixels; j++)
	pixels[i, j] = colors.GetColor(palette[pixelTable[i, j]]);

// sfml
const int width = 128;
const int height = 128;

var videoMode = new VideoMode(width, height);
var window = new RenderWindow(videoMode, "Tile");

window.Closed += (_, _) => window.Close();
window.SetFramerateLimit(120);

var img = new Image(pixels);
var texture = new Texture(img);
var sprite = new Sprite(texture);

var view = window.GetView();
view.Rotation = 90;
view.Size = new Vector2f(width, -height);
window.SetView(view);
window.Size = new Vector2u(800, 800);

while (window.IsOpen)
{
	window.DispatchEvents();

	window.Clear();

	window.Draw(sprite);

	window.Display();
}