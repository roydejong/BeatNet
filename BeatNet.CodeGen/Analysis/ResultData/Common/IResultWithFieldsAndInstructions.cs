using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.ResultData.Common;

public interface IResultWithFieldsAndInstructions : IResultWithFields
{
    IEnumerable<DeserializeInstruction> GetInstructions();
}