﻿namespace NesEmulator.Core.CpuModule;

public class Ram
{
    private readonly byte[] _memory = new byte[65536];

    public uint Read(uint address)
        => _memory[address];

    public void Write(uint address, uint data)
        => _memory[address] = (byte)data;

    public void Load(byte[] program, uint index)
        => Array.Copy(program, 0, _memory, index, program.Length);
}