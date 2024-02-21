using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.ResultData.Common;

public interface IResultSerializable : IResultWithFields
{
    IEnumerable<DeserializeInstruction> GetInstructions();
}