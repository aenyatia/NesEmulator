using NesEmulator.Core.PpuModule;

namespace NesEmulator.Render;

public class Engine
{
    public void Render(Ppu ppu, Frame frame, Colors colors)
    {
        // draw background
        var bank = ppu.ControlRegister.BackgroundPatternTableAddress();

        for (var i = 0; i < 960; i++)
        {
            var tile = ppu.VRam[i];
            var tileX = i % 32;
            var tileY = i / 32;

            var tileData = ppu.ChrRom.AsSpan().Slice((int)(bank + tile * 16), 16);
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
                        0 => colors[ppu.PaletteTable[0]],
                        1 => colors[palette[1]],
                        2 => colors[palette[2]],
                        3 => colors[palette[3]],
                        _ => throw new Exception("unknown color index")
                    };

                    frame.SetPixel(tileX * 8 + x, tileY * 8 + y, color);
                }
            }
        }

        // draw sprites
        for (var i = ppu.OamDataBuffer.Length - 4; i >= 0; i -= 4)
        {
            var tileIndex = ppu.OamDataBuffer[i + 1];
            var tileX = ppu.OamDataBuffer[i + 3];
            var tileY = ppu.OamDataBuffer[i];

            var flipVertical = ((ppu.OamDataBuffer[i + 2] >> 7) & 1) == 1;
            var flipHorizontal = ((ppu.OamDataBuffer[i + 2] >> 6) & 1) == 1;

            var paletteIndex = ppu.OamDataBuffer[i + 2] & 0b0000_0011;
            var spritePalette = SpritePalette(ppu, (byte)paletteIndex);

            var bankSprite = ppu.ControlRegister.SpritePatternTableAddress();
            var tile = ppu.ChrRom.AsSpan().Slice((int)(bankSprite + tileIndex * 16), 16);

            for (var y = 0; y < 8; y++) // iterate through rows
            {
                var upper = tile[y];
                var lower = tile[y + 8];

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
                        0 => colors[ppu.PaletteTable[0]],
                        1 => colors[spritePalette[1]],
                        2 => colors[spritePalette[2]],
                        3 => colors[spritePalette[3]],
                        _ => throw new Exception("unknown color index")
                    };

                    if (flipHorizontal == false && flipVertical == false)
                    {
                        frame.SetPixel(tileX + x, tileY + y, color);
                    }

                    if (flipHorizontal == true && flipVertical == false)
                    {
                        frame.SetPixel(tileX + 7 - x, tileY + y, color);
                    }

                    if (flipHorizontal == false && flipVertical == true)
                    {
                        frame.SetPixel(tileX + x, tileY + 7 - y, color);
                    }

                    if (flipHorizontal == true && flipVertical == true)
                    {
                        frame.SetPixel(tileX + 7 - x, tileY + 7 - y, color);
                    }
                }
            }
        }
    }

    private byte[] SpritePalette(Ppu ppu, byte paletteIndex)
    {
        var start = 0x11 + paletteIndex * 4;

        return
        [
            0,
            ppu.PaletteTable[start],
            ppu.PaletteTable[start + 1],
            ppu.PaletteTable[start + 2],
        ];
    }


    private byte[] BackgroundPalette(Ppu ppu, int tileColumn, int tileRow)
    {
        var attributeTable = tileRow / 4 * 8 + tileColumn / 4;
        var attributeByte = ppu.VRam[0x3c0 + attributeTable];

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
}