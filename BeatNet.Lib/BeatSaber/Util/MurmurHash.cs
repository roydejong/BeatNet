namespace BeatNet.Lib.BeatSaber.Util;

public static class MurmurHash
{
    public static uint MurmurHash2(this string key)
    {
        const uint seed = 33U;
        
        var length = (uint)key.Length;
        if (length == 0)
            return 0;
        
        var h = seed ^ length;
        var currentIndex = 0;
        
        while (length >= 4U)
        {
            var num4 = (uint)key[currentIndex + 3] << 24 | (uint)key[currentIndex + 2] << 16 | (uint)key[currentIndex + 1] << 8 |
                       key[currentIndex];
            num4 *= 1540483477U;
            num4 ^= num4 >> 24;
            num4 *= 1540483477U;
            h *= 1540483477U;
            h ^= num4;
            currentIndex += 4;
            length -= 4U;
        }

        switch (length)
        {
            case 1U:
                h ^= key[currentIndex];
                h *= 1540483477U;
                break;
            case 2U:
                h ^= (uint)key[currentIndex + 1] << 8;
                h ^= key[currentIndex];
                h *= 1540483477U;
                break;
            case 3U:
                h ^= (uint)key[currentIndex + 2] << 16;
                h ^= (uint)key[currentIndex + 1] << 8;
                h ^= key[currentIndex];
                h *= 1540483477U;
                break;
        }

        h ^= h >> 13;
        h *= 1540483477U;
        return h ^ h >> 15;
    }
}