using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.Net.Interfaces;

public interface INetSerializable
{
    void WriteTo(ref NetWriter writer);

    void ReadFrom(ref NetReader reader);
}