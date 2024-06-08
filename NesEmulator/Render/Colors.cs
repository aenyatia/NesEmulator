﻿using SFML.Graphics;

namespace NesEmulator.Render;

public class Colors
{
    // private readonly Color[] _colors =
    // [
    //     new Color(84, 84, 84),
    //     new Color(0, 30, 116),
    //     new Color(8, 16, 144),
    //     new Color(48, 0, 136),
    //     new Color(68, 0, 100),
    //     new Color(92, 0, 48),
    //     new Color(84, 4, 0),
    //     new Color(60, 24, 0),
    //     new Color(32, 42, 0),
    //     new Color(8, 58, 0),
    //     new Color(0, 64, 0),
    //     new Color(0, 60, 0),
    //     new Color(0, 50, 60),
    //     new Color(0, 0, 0),
    //     new Color(0, 0, 0),
    //     new Color(0, 0, 0),
    //
    //     new Color(152, 150, 152),
    //     new Color(8, 76, 196),
    //     new Color(48, 50, 236),
    //     new Color(92, 30, 228),
    //     new Color(136, 20, 176),
    //     new Color(160, 20, 100),
    //     new Color(152, 34, 32),
    //     new Color(120, 60, 0),
    //     new Color(84, 90, 0),
    //     new Color(40, 114, 0),
    //     new Color(8, 124, 0),
    //     new Color(0, 118, 40),
    //     new Color(0, 102, 120),
    //     new Color(0, 0, 0),
    //     new Color(0, 0, 0),
    //     new Color(0, 0, 0),
    //
    //     new Color(236, 238, 236),
    //     new Color(76, 154, 236),
    //     new Color(120, 124, 236),
    //     new Color(176, 98, 236),
    //     new Color(228, 84, 236),
    //     new Color(236, 88, 180),
    //     new Color(236, 106, 100),
    //     new Color(212, 136, 32),
    //     new Color(160, 170, 0),
    //     new Color(116, 196, 0),
    //     new Color(76, 208, 32),
    //     new Color(56, 204, 108),
    //     new Color(56, 180, 204),
    //     new Color(60, 60, 60),
    //     new Color(0, 0, 0),
    //     new Color(0, 0, 0),
    //
    //     new Color(236, 238, 236),
    //     new Color(168, 204, 236),
    //     new Color(188, 188, 236),
    //     new Color(212, 178, 236),
    //     new Color(236, 174, 236),
    //     new Color(236, 174, 212),
    //     new Color(236, 180, 176),
    //     new Color(228, 196, 144),
    //     new Color(204, 210, 120),
    //     new Color(180, 222, 120),
    //     new Color(168, 226, 144),
    //     new Color(152, 226, 180),
    //     new Color(160, 214, 228),
    //     new Color(160, 162, 160),
    //     new Color(0, 0, 0),
    //     new Color(0, 0, 0)
    // ];

    private readonly Color[] _colors =
    [
        new Color(0x80, 0x80, 0x80), 
        new Color(0x00, 0x3D, 0xA6), 
        new Color(0x00, 0x12, 0xB0), 
        new Color(0x44, 0x00, 0x96), 
        new Color(0xA1, 0x00, 0x5E), 
        new Color(0xC7, 0x00, 0x28), 
        new Color(0xBA, 0x06, 0x00), 
        new Color(0x8C, 0x17, 0x00), 
        new Color(0x5C, 0x2F, 0x00), 
        new Color(0x10, 0x45, 0x00), 
        new Color(0x05, 0x4A, 0x00), 
        new Color(0x00, 0x47, 0x2E), 
        new Color(0x00, 0x41, 0x66), 
        new Color(0x00, 0x00, 0x00), 
        new Color(0x05, 0x05, 0x05), 
        new Color(0x05, 0x05, 0x05), 
        new Color(0xC7, 0xC7, 0xC7), 
        new Color(0x00, 0x77, 0xFF), 
        new Color(0x21, 0x55, 0xFF), 
        new Color(0x82, 0x37, 0xFA), 
        new Color(0xEB, 0x2F, 0xB5), 
        new Color(0xFF, 0x29, 0x50), 
        new Color(0xFF, 0x22, 0x00), 
        new Color(0xD6, 0x32, 0x00), 
        new Color(0xC4, 0x62, 0x00), 
        new Color(0x35, 0x80, 0x00), 
        new Color(0x05, 0x8F, 0x00), 
        new Color(0x00, 0x8A, 0x55), 
        new Color(0x00, 0x99, 0xCC), 
        new Color(0x21, 0x21, 0x21), 
        new Color(0x09, 0x09, 0x09), 
        new Color(0x09, 0x09, 0x09), 
        new Color(0xFF, 0xFF, 0xFF), 
        new Color(0x0F, 0xD7, 0xFF), 
        new Color(0x69, 0xA2, 0xFF), 
        new Color(0xD4, 0x80, 0xFF), 
        new Color(0xFF, 0x45, 0xF3), 
        new Color(0xFF, 0x61, 0x8B), 
        new Color(0xFF, 0x88, 0x33), 
        new Color(0xFF, 0x9C, 0x12), 
        new Color(0xFA, 0xBC, 0x20), 
        new Color(0x9F, 0xE3, 0x0E), 
        new Color(0x2B, 0xF0, 0x35), 
        new Color(0x0C, 0xF0, 0xA4), 
        new Color(0x05, 0xFB, 0xFF), 
        new Color(0x5E, 0x5E, 0x5E), 
        new Color(0x0D, 0x0D, 0x0D), 
        new Color(0x0D, 0x0D, 0x0D), 
        new Color(0xFF, 0xFF, 0xFF), 
        new Color(0xA6, 0xFC, 0xFF), 
        new Color(0xB3, 0xEC, 0xFF), 
        new Color(0xDA, 0xAB, 0xEB), 
        new Color(0xFF, 0xA8, 0xF9), 
        new Color(0xFF, 0xAB, 0xB3), 
        new Color(0xFF, 0xD2, 0xB0), 
        new Color(0xFF, 0xEF, 0xA6), 
        new Color(0xFF, 0xF7, 0x9C), 
        new Color(0xD7, 0xE8, 0x95), 
        new Color(0xA6, 0xED, 0xAF), 
        new Color(0xA2, 0xF2, 0xDA), 
        new Color(0x99, 0xFF, 0xFC), 
        new Color(0xDD, 0xDD, 0xDD), 
        new Color(0x11, 0x11, 0x11), 
        new Color(0x11, 0x11, 0x11)
    ];
    
    public Color this[int index] => _colors[index];
}