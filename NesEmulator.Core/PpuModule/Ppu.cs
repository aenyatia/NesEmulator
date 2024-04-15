using NesEmulator.Core.CartridgeModule;
using NesEmulator.Core.PpuModule.Registers;

namespace NesEmulator.Core.PpuModule;

public class Ppu(Cartridge cartridge)
{
    public PpuRam VRam { get; } = new(cartridge.Header.Mirroring); // nametable 0x0000 - 0x1FFF 8kB
    public readonly byte[] PaletteTable = new byte[32]; // palette table 0x3F00 - 0x3F1F 32 bytes

    public readonly ControlRegister ControlRegister = new(); // 0x2000
    private readonly MaskRegister _maskRegister = new(); // 0x2001
    private readonly StatusRegister _statusRegister = new(); // 0x2002
    private readonly ScrollRegister _scrollRegister = new(); // 0x2005
    private readonly AddressRegister _addressRegister = new(); // 0x2006

    private byte _oamAddress; // 0x2003
    public readonly byte[] OamDataBuffer = new byte[256]; // internal oam 64 * 4 bytes 0x2004

    // private byte _oamDma; // 0x2014

    private byte _internalDataBuffer;

    private uint _scanline;
    private uint _cycles;
    private uint _nmiInterrupt;

    public bool Tick(uint cycles)
    {
        _cycles += cycles;

        if (_cycles >= 341)
        {
            _cycles -= 341;
            _scanline += 1;

            if (_scanline == 241)
            {
                _statusRegister.VerticalVBlanc(true);
                _statusRegister.SpriteZeroHit(false);

                if (ControlRegister.GenerateVBlancNmi() == 1)
                {
                    _nmiInterrupt = 1;
                }
            }

            if (_scanline >= 262)
            {
                _scanline = 0;
                _nmiInterrupt = 0;

                _statusRegister.SpriteZeroHit(false);
                _statusRegister.ResetVerticalVBlanc();

                return true;
            }
        }

        return false;
    }

    private void IncrementVRamAddress()
    {
        _addressRegister.Increment(ControlRegister.VRamAddressIncrement());
    }

    public uint PollNmiInterrupt()
    {
        return _nmiInterrupt;
    }

    public byte Read(uint address)
    {
        // mirroring 0010 0000 0000 0111 - 0x2007
        // 0x2000 - 0x2007
        // 0x2008 - 0x200F
        // 0x2010 - 0x2017
        // 0x2018 - 0x201F
        address &= 0b0010_0000_0000_0111;

        return address switch
        {
            0x2002 => ReadStatusRegister(),
            0x2004 => ReadFromOamData(),
            0x2007 => ReadPpuRam(),
            _ => throw new Exception($"Attempt to read from write-only PPU address {address}")
        };

        byte ReadStatusRegister() // 0x2002 read-only
        {
            var status = _statusRegister.Get();

            _statusRegister.ResetVerticalVBlanc();
            _scrollRegister.ResetLatch();
            _addressRegister.ResetLatch();

            return status;
        }

        byte ReadFromOamData() // 0x2004 read
        {
            return OamDataBuffer[_oamAddress];
        }

        byte ReadPpuRam() // 0x2007 read
        {
            var address = _addressRegister.Get();
            IncrementVRamAddress();

            if (address < 0x2000) // read from chr rom
            {
                var data = _internalDataBuffer;
                _internalDataBuffer = cartridge.ReadChrRom(address);
                return data;
            }

            if (address is >= 0x2000 and < 0x3000) // read from vRam (name table)
            {
                var data = _internalDataBuffer;
                //var addr = MirrorVRamAddress(address);
                _internalDataBuffer = VRam.Read(address);
                return data;
            }

            if (address is >= 0x3000 and < 0x3F00) // read from unused space
            {
                throw new Exception("attempt to read free space");
            }

            if (address is >= 0x3F00 and < 0x4000) // read from palette table
            {
                // add mirroring
                return PaletteTable[address - 0x3F00];
            }

            throw new Exception("unexpected access to mirrored space");
        }
    }

    public void WriteToODma(byte data) // 2014
    {
        var buffer = new byte[256];
        var hi = data << 8;

        for (var i = 0; i < 256; i++)
            buffer[i] = Read((uint)(hi + i));

        foreach (var value in buffer)
        {
            OamDataBuffer[_oamAddress] = value;
            _oamAddress += 1;
        }
    }

    public void Write(ushort address, byte data)
    {
        // mirroring 0010 0000 0000 0111 - 0x2007
        // 0x2000 - 0x2007
        // 0x2008 - 0x200F
        // 0x2010 - 0x2017
        // 0x2018 - 0x201F
        address &= 0b0010_0000_0000_0111;

        switch (address)
        {
            // write control
            case 0x2000:
                WriteToPpuCtrl(data);
                break;
            // write mask
            case 0x2001:
                WriteToPpuMask(data);
                break;
            // write oam address
            case 0x2003:
                WriteToOamAddress(data);
                break;
            // write oam data
            case 0x2004:
                WriteToOamData(data);
                break;
            // write scroll
            case 0x2005:
                WriteToPpuScroll(data);
                break;
            // write address
            case 0x2006:
                WriteToPpuAddress(data);
                break;
            // write
            case 0x2007:
                WritePpuRam(data);
                break;
            case 0x2014:
            {
                
                break;
            }
        }

        throw new Exception("$\"Attempt to write to read-only PPU address {address}\"");

        void WriteToPpuCtrl(byte value) // 0x2000 write-only
        {
            var nmiStatus = ControlRegister.GenerateVBlancNmi(); // ???
            ControlRegister.Update(value);

            if (nmiStatus == 0 && ControlRegister.GenerateVBlancNmi() == 1 && _statusRegister.IsInVBlanc())
                _nmiInterrupt = 1;
        }

        void WriteToPpuMask(byte value) // 0x2001 write-only
        {
            _maskRegister.Update(value);
        }

        void WriteToOamAddress(byte value) // 0x2003 write-only
        {
            _oamAddress = value;
        }

        void WriteToOamData(byte value) // 0x2004 write
        {
            OamDataBuffer[_oamAddress] = value;
            _oamAddress += 1;
        }

        void WriteToPpuScroll(byte value) // 0x2005 write-only
        {
            _scrollRegister.Write(value);
        }

        void WriteToPpuAddress(byte value) // 0x2006 write-only
        {
            ControlRegister.Update(value);
        }

        void WritePpuRam(byte data) // 0x2007 write
        {
            var address = _addressRegister.Get();

            if (address < 0x2000) // write to chr rom
            {
                throw new Exception("attempt to write to chr rom");
            }

            if (address is >= 0x2000 and < 0x3000) // write to vRam (name table)
            {
                //var addr = MirrorVRamAddress(address);
                VRam.Write(address, data);
            }
            else if (address is >= 0x3000 and < 0x3F00) // write to unused space
            {
                throw new Exception("attempt to write free space");
            }
            else if (address is >= 0x3F00 and < 0x4000) // write to palette
            {
                // add mirroring
                PaletteTable[address - 0x3F00] = data;
            }
            else // write to outside boundary
            {
                throw new Exception("unexpected access to mirrored space");
            }

            IncrementVRamAddress();
        }
    }
}