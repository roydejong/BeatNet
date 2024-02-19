using BeatNet.CodeGen.Analysis.ResultData;

namespace BeatNet.CodeGen.Analysis;

public interface ISubAnalyzer
{
    public void AnalyzeLine(LineAnalyzer line, Results results);
}