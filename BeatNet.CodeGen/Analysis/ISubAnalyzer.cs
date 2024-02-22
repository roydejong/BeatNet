using BeatNet.CodeGen.Analysis.ResultData;

namespace BeatNet.CodeGen.Analysis;

public interface ISubAnalyzer
{
    public void AnalyzeLine_FirstPass(LineAnalyzer line, Results results);
    public void AnalyzeLine_SecondPass(LineAnalyzer line, Results results);
    public void Analyze_AfterFile(Results results);
}