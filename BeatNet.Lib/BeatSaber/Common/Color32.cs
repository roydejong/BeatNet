using System.Runtime.InteropServices;

namespace BeatNet.Lib.BeatSaber.Common;

[StructLayout(LayoutKind.Explicit)]
public struct Color32
{
    [FieldOffset(0)]
    public int Rgba;
    [FieldOffset(0)]
    public float R;
    [FieldOffset(1)]
    public float G;
    [FieldOffset(2)]
    public float B;
    [FieldOffset(3)]
    public float A;
    
    public Color32(byte r, byte g, byte b, byte a)
    {
        Rgba = 0;
        R = r;
        G = g;
        B = b;
        A = a;
    }
}