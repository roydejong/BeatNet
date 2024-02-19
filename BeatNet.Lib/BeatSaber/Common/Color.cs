namespace BeatNet.Lib.BeatSaber.Common;

public struct Color
{
    public float R;
    public float G;
    public float B;
    public float A;
    
    public Color(float r, float g, float b, float a)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = a;
    }

    public Color(float r, float g, float b)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = 1f;
    }
}