using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis.ResultData.Common;

public interface IResultWithFields : IResult
{
    IEnumerable<TypedParam> GetFields();
}