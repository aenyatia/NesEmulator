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

    //private byte _oamAddress; // 0x2003
    //public readonly byte[] OamDataBuffer = new byte[256]; // internal oam 64 * 4 bytes 0x2004
    // private byte _oamDma; // 0x2014

    private byte _internalDataBuffer;

    private uint _scanline;
    private uint _cycles;
    private bool _nmiInterrupt;

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

                if (ControlRegister.GenerateVBlancNmi())
                {
                    _nmiInterrupt = true;
                }
            }

            if (_scanline >= 262)
            {
                _scanline = 0;
                _nmiInterrupt = false;

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

    public bool PollNmiInterrupt()
    {
        return _nmiInterrupt;
    }

    public byte Read(ushort address)
    {
        // mirroring 0010 0000 0000 0111 = 0x2007
        // 0x2000 - 0x2007
        // 0x2008 - 0x200F
        // 0x2010 - 0x2017
        // 0x2018 - 0x201F
        address &= 0b0010_0000_0000_0111;

        return address switch
        {
            0x2000 => 0,
            0x2001 => 0,
            0x2002 => ReadStatusRegister(),
            0x2003 => 0,
            0x2004 => 0, // add oam
            0x2005 => 0,
            0x2006 => 0,
            0x2007 => ReadPpuRam(),
            _ => throw new Exception($"Attempt to read from write-only PPU address {address}")
        };

        byte ReadStatusRegister() // 0x2002 read-only
        {
            var status = _statusRegister.Get();

            _statusRegister.ResetVerticalVBlanc();
            _addressRegister.ResetLatch();
            _scrollRegister.ResetLatch();

            return status;
        }

        byte ReadPpuRam() // 0x2007 read
        {
            var registerAddress = _addressRegister.Address;
            IncrementVRamAddress();

            if (registerAddress < 0x2000) // read from chr rom
            {
                var data = _internalDataBuffer;
                _internalDataBuffer = cartridge.ReadChrRom(registerAddress);
                return data;
            }

            if (registerAddress is >= 0x2000 and < 0x3000) // read from vRam (name table)
            {
                var data = _internalDataBuffer;
                //var addr = MirrorVRamAddress(address);
                _internalDataBuffer = VRam.Read(registerAddress);
                return data;
            }

            if (registerAddress is >= 0x3000 and < 0x3F00) // read from unused space
            {
                throw new Exception("attempt to read free space");
            }

            if (registerAddress is >= 0x3F00 and < 0x4000) // read from palette table
            {
                // add mirroring
                // 0x3F00 - 0x3F1F
                // 0x3F20 - 0x3F3F
                // 0x3F40 - 0x3F5F
                // ...
                // 0x3FE0 - 0x3FFF
                
                // $3F10/$3F14/$3F18/$3F1C are mirrors of
                // $3F00/$3F04/$3F08/$3F0C
                if (registerAddress is 0x3F10 or 0x3F14 or 0x3F18 or 0x3F1C)
                    registerAddress -= 0x0010;

                registerAddress &= 0b0011_1111_0001_1111; // 0x3F1F
                
                return PaletteTable[registerAddress - 0x3F00];
            }

            throw new Exception("unexpected access to mirrored space");
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
                WriteToPpuCtrl();
                break;
            // write mask
            case 0x2001:
                WriteToPpuMask();
                break;
            // write status
            case 0x2002:
                throw new Exception("status");
            // write oam address
            case 0x2003:
                //WriteToOamAddress();
                break;
            // write oam data
            case 0x2004:
                //WriteToOamData();
                break;
            // write scroll
            case 0x2005:
                WriteToPpuScroll();
                break;
            // write address
            case 0x2006:
                WriteToPpuAddress();
                break;
            // write
            case 0x2007:
                WritePpuRam();
                break;
        }

        return;

        //throw new Exception("$\"Attempt to write to read-only PPU address {address}\"");

        void WriteToPpuCtrl() // 0x2000 write-only
        {
            var nmiStatus = ControlRegister.GenerateVBlancNmi(); // ???
            ControlRegister.Update(data);

            if (!nmiStatus && ControlRegister.GenerateVBlancNmi() && _statusRegister.IsInVBlanc())
                _nmiInterrupt = true;
        }

        void WriteToPpuMask() // 0x2001 write-only
        {
            _maskRegister.Update(data);
        }

        // void WriteToOamAddress() // 0x2003 write-only
        // {
        //     _oamAddress = data;
        // }
        //
        // void WriteToOamData() // 0x2004 write
        // {
        //     OamDataBuffer[_oamAddress] = data;
        //     _oamAddress += 1;
        // }

        void WriteToPpuScroll() // 0x2005 write-only
        {
            _scrollRegister.Write(data);
        }

        void WriteToPpuAddress() // 0x2006 write-only
        {
            _addressRegister.Update(data);
        }

        void WritePpuRam() // 0x2007 write
        {
            var addressRegister = _addressRegister.Address;

            if (addressRegister < 0x2000) // write to chr rom
            {
                throw new Exception("attempt to write to chr rom");
            }
            else if (addressRegister is >= 0x2000 and < 0x3000) // write to vRam (name table)
            {
                VRam.Write(addressRegister, data);
            }
            else if (addressRegister is >= 0x3000 and < 0x3F00) // write to unused space
            {
                throw new Exception("attempt to write free space");
            }
            else if (addressRegister is >= 0x3F00 and < 0x4000) // write to palette
            {
                // $3F10/$3F14/$3F18/$3F1C are mirrors of
                // $3F00/$3F04/$3F08/$3F0C
                if (addressRegister is 0x3F10 or 0x3F14 or 0x3F18 or 0x3F1C)
                    addressRegister -= 0x0010;

                addressRegister &= 0b0011_1111_0001_1111; // 0x3F1F

                PaletteTable[addressRegister - 0x3F00] = data;
            }
            else // write to outside boundary
            {
                throw new Exception("unexpected access to mirrored space");
            }

            IncrementVRamAddress();
        }
    }
}