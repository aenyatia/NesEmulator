namespace NesEmulator.Core.PpuModule;

public class Ppu
{
    private readonly byte[] _chrRom; // pattern table 0x0000 - 0x1FFF 8kB
    private readonly byte[] _vRam = new byte[2048]; // name table 0x2000 - 0x2FFF 4 * 1kB (mirroring to 2kB)
    private readonly byte[] _paletteTable = new byte[32]; // palette table 0x3F00 - 0x3F1F 32 bytes
    private readonly Mirroring _mirroring;

    private readonly ControlRegister _controlRegister = new(); // 0x2000
    private readonly MaskRegister _maskRegister = new(); // 0x2001
    private readonly StatusRegister _statusRegister = new(); // 0x2002
    private readonly ScrollRegister _scrollRegister = new(); // 0x2005
    private readonly AddressRegister _addressRegister = new(); // 0x2006

    private byte _oamAddress; // 0x2003
    private readonly byte[] _oamDataBuffer = new byte[256]; // internal oam 64 * 4 bytes 0x2004

    private byte _oamDma; // 0x2014

    private byte _internalDataBuffer;

    public Ppu(Rom rom)
    {
        _mirroring = rom.ScreenMirroring;
        _chrRom = rom.ChrRom;
    }

    private void IncrementVRamAddress()
    {
        _addressRegister.Increment(_controlRegister.VRamAddressIncrement());
    }

    private uint MirrorVRamAddress(ushort address)
    {
        // address from 0x2000 to 0x2FFF 4kB
        address -= 0x2000; // 0x0000 - 0x0FFF

        var nameTableIndex = address / 0x400;

        // Vertical mirroring: $2000 equals $2800 and $2400 equals $2C00
        // Horizontal mirroring: $2000 equals $2400 and $2800 equals $2C00
        if (_mirroring == Mirroring.Horizontal)
        {
            // inx == 0 nothing to do

            if (nameTableIndex == 1)
            {
                address -= 0x0400;
            }

            if (nameTableIndex == 2)
            {
                address -= 0x0400;
            }

            if (nameTableIndex == 3)
            {
                address -= 0x0800;
            }
        }

        if (_mirroring == Mirroring.Vertical)
        {
            // inx == 0 or 1 nothing to do

            if (address == 2)
            {
                address -= 0x800;
            }

            if (address == 3)
            {
                address -= 0x800;
            }
        }

        return address;
    }

    #region Read/Write Registers

    public void WriteToPpuCtrl(byte value) // 0x2000 write-only
    {
        var nmiStatus = _controlRegister.GenerateVBlancNmi(); // ???
        _controlRegister.Update(value);
    }

    public void WriteToPpuMask(byte value) // 0x2001 write-only
    {
        _maskRegister.Update(value);
    }

    public byte ReadFromPpuStatus() // 0x2002 read-only
    {
        var status = _statusRegister.Get();

        // Reading the status register will clear bit 7 mentioned above and also the address latch used by PPUSCROLL and PPUADDR.
        // It does not clear the sprite 0 hit or overflow bit.
        _statusRegister.ResetVerticalVBlanc();
        _scrollRegister.ResetLatch();
        _addressRegister.ResetLatch();

        return status;
    }

    public void WriteToOamAddress(byte value) // 0x2003 write-only
    {
        _oamAddress = value;
    }

    public void WriteToOamData(byte value) // 0x2004 write
    {
        _oamDataBuffer[_oamAddress] = value;
        _oamAddress += 1;
    }

    public byte ReadFromOamData() // 0x2004 read
    {
        return _oamDataBuffer[_oamAddress];
    }

    public void WriteToPpuScroll(byte value) // 0x2005 write-only
    {
        _scrollRegister.Write(value);
    }

    public void WriteToPpuAddress(byte value) // 0x2006 write-only
    {
        _controlRegister.Update(value);
    }

    public void Write(byte value) // 0x2007 write
    {
        var address = _addressRegister.Get();

        if (address is >= 0x0000 and < 0x2000) // write to chr rom
        {
            throw new Exception("attempt to write to chr rom");
        }
        else if (address is >= 0x2000 and < 0x3000) // write to vRam (name table)
        {
            var addr = MirrorVRamAddress((ushort)address);
            _vRam[addr] = value;
        }
        else if (address is >= 0x3000 and < 0x3F00) // write to unused space
        {
            throw new Exception("attempt to write free space");
        }
        else if (address is >= 0x3F00 and < 0x4000) // write to palette
        {
            // add mirroring
            _paletteTable[address - 0x3F00] = value;
        }
        else // write to outside boundary
        {
            throw new Exception("unexpected access to mirrored space");
        }

        IncrementVRamAddress();
    }

    public byte Read() // 0x2007 read
    {
        var address = _addressRegister.Get();
        IncrementVRamAddress();

        if (address is >= 0x0000 and < 0x2000) // read from chr rom
        {
            var data = _internalDataBuffer;
            _internalDataBuffer = _chrRom[address];
            return data;
        }

        if (address is >= 0x2000 and < 0x3000) // read from vRam (name table)
        {
            var data = _internalDataBuffer;
            var addr = MirrorVRamAddress((ushort)address);
            _internalDataBuffer = _vRam[addr];
            return data;
        }

        if (address is >= 0x3000 and < 0x3F00) // read from unused space
        {
            throw new Exception("attempt to read free space");
        }

        if (address is >= 0x3F00 and < 0x4000) // read from palette table
        {
            // add mirroring
            return _paletteTable[address - 0x3F00];
        }

        throw new Exception("unexpected access to mirrored space");
    }

    public void WriteToOamDma(byte[] data) // 0x2014
    {
        foreach (var value in data)
        {
            _oamDataBuffer[_oamAddress] = value;
            _oamAddress += 1;
        }
    }

    #endregion
}