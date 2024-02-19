using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.ResultData.Common;

public interface ISerializeDeserialize
{
    IEnumerable<TypedParam> GetFields();
    IEnumerable<DeserializeInstruction> GetInstructions();
}