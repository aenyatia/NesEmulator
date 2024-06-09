using NesEmulator.Core.CartridgeModule;
using NesEmulator.Core.PpuModule.Registers;

namespace NesEmulator.Core.PpuModule;

public class Ppu(Cartridge cartridge)
{
    // ppu registers
    private readonly ControlRegister _controlRegister = new(); // 0x2000
    private readonly MaskRegister _maskRegister = new(); // 0x2001
    private readonly StatusRegister _statusRegister = new(); // 0x2002

    private readonly ScrollRegister _scrollRegister = new(); // 0x2005
    private readonly AddressRegister _addressRegister = new(); // 0x2006

    // vRam
    private readonly VRam _vRam = new(cartridge.Mirroring);

    // palette table
    private readonly byte[] _paletteTable = new byte[32];

    // pixel position
    private uint _cycles;
    private uint _scanline;

    // internal data
    private byte _ppuDataBuffer;

    public bool Nmi { get; set; }

    public byte CpuRead(ushort address)
    {
        if (address is < 0x2000 or > 0x3FFF) throw new ArgumentOutOfRangeException(nameof(address));

        address &= 0x0007;

        byte data = 0x00;

        if (address == 0x0000)
        {
        }
        else if (address == 0x0001)
        {
        }
        else if (address == 0x0002)
        {
            data = _statusRegister.Value;

            _statusRegister.ResetVerticalBlanc();
            _addressRegister.ResetLatch();
        }
        else if (address == 0x0003)
        {
        }
        else if (address == 0x0004)
        {
        }
        else if (address == 0x0005)
        {
        }
        else if (address == 0x0006)
        {
        }
        else if (address == 0x0007)
        {
            data = _ppuDataBuffer;

            _ppuDataBuffer = PpuRead(_addressRegister);

            if (_addressRegister >= 0x3F00)
            {
                data = _ppuDataBuffer;
            }

            _addressRegister.Increment(_controlRegister.IncrementMode);
        }

        return data;
    }

    public void CpuWrite(ushort address, byte data)
    {
        if (address is < 0x2000 or > 0x3FFF) throw new ArgumentOutOfRangeException(nameof(address));

        address &= 0x0007;

        if (address == 0x0000)
        {
            _controlRegister.Value = data;
        }
        else if (address == 0x0001)
        {
            _maskRegister.Value = data;
        }
        else if (address == 0x0002)
        {
        }
        else if (address == 0x0003)
        {
        }
        else if (address == 0x0004)
        {
        }
        else if (address == 0x0005)
        {
        }
        else if (address == 0x0006)
        {
            _addressRegister.Update(data);
        }
        else if (address == 0x0007)
        {
            PpuWrite(_addressRegister, data);

            _addressRegister.Increment(_controlRegister.IncrementMode);
        }
    }

    private byte PpuRead(ushort address)
    {
        address &= 0x3FFF;

        byte data = 0x00;

        if (cartridge.PpuRead(address, ref data))
        {
        }
        else if (address is >= 0x2000 and <= 0x3EFF)
        {
            data = _vRam.PpuRead(address);
        }
        else if (address is >= 0x3F00 and <= 0x3FFF)
        {
            address &= 0x001F;

            if (address is 0x0010 or 0x0014 or 0x0018 or 0x001C)
            {
                address = 0x00;
            }

            data = _paletteTable[address];
        }

        return data;
    }

    private void PpuWrite(ushort address, byte data)
    {
        address &= 0x3FFF;

        if (cartridge.PpuWrite(address, data))
        {
        }
        else if (address is >= 0x2000 and <= 0x3EFF)
        {
            _vRam.PpuWrite(address, data);
        }
        else if (address is >= 0x3F00 and <= 0x3FFF)
        {
            address &= 0x001F;

            if (address is 0x0010 or 0x0014 or 0x0018 or 0x001C)
            {
                address = 0x00;
            }

            _paletteTable[address] = data;
        }
    }

    public void Reset()
    {
        _statusRegister.Value = 0;
        _maskRegister.Value = 0;
        _controlRegister.Value = 0;
        _addressRegister.Value = 0;

        _scanline = 0;
        _cycles = 0;

        _ppuDataBuffer = 0;
    }

    public void Clock()
    {
        _cycles++;

        if (_cycles >= 341)
        {
            _cycles -= 341;
            _scanline += 1;

            if (_scanline == 241)
            {
                _statusRegister.VerticalBlanc = true;
                _statusRegister.SpriteZeroHit = false;

                if (_controlRegister.EnableNmi)
                {
                    Nmi = true;
                }
            }

            if (_scanline >= 262)
            {
                _scanline = 0;
                Nmi = false;

                _statusRegister.SpriteZeroHit = false;
                _statusRegister.VerticalBlanc = false;
            }
        }
    }
}