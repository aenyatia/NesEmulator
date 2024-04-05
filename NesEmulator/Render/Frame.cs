using SFML.Graphics;

namespace NesEmulator.Render;

public class Frame
{
    private readonly byte[] _frameData = new byte[Width * Height * 4];
    private const uint Width = 256;
    private const uint Height = 240;

    public void SetPixel(int x, int y, Color color)
    {
        var baseIndex = y * Width * 4 + x * 4;

        if (baseIndex + 3 < _frameData.Length)
        {
            _frameData[baseIndex] = color.R;
            _frameData[baseIndex + 1] = color.G;
            _frameData[baseIndex + 2] = color.B;
            _frameData[baseIndex + 3] = color.A;
        }
    }

    public static implicit operator byte[](Frame frame)
    {
        return frame._frameData;
    }
}