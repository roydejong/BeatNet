using System.Text;
using BeatNet.CodeGen.Analysis.ResultData.Common;

namespace BeatNet.CodeGen.Generator.FixedImpl;

public abstract class FixedImpl
{
    public abstract FixedImplMode Mode { get; }
    public abstract bool AppliesToType(string typeName);
    public abstract void GenerateWriteTo(IResultWithFields item, StringBuilder buffer);
    public abstract void GenerateReadFrom(IResultWithFields item, StringBuilder buffer);
}

public enum FixedImplMode
{
    Prefix,
    Override,
    Postfix
}