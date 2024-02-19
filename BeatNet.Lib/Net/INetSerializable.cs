using BeatNet.Lib.Net.IO;

namespace BeatNet.Lib.Net;

public interface INetSerializable
{
    void WriteTo(ref NetWriter writer);

    void ReadFrom(ref NetReader reader);
}