using BeatNet.CodeGen.Analysis;

namespace BeatNet.Tests.CodeGen;

public class LineAnalyzerTest
{
    [Test]
    public void TestParsesStructDeclare()
    {
        var text = "public struct ColorSchemeNetSerializable : INetSerializable";
        var line = new LineAnalyzer(text, null);
        
        Assert.That(line.Modifier, Is.EqualTo("public"));
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.IsStruct, Is.True);
        Assert.That(line.DeclaredName, Is.EqualTo("ColorSchemeNetSerializable"));
        Assert.That(line.ClassInheritors, Is.Not.Empty);
        Assert.That(line.ClassInheritors![0].TypeName, Is.EqualTo("INetSerializable"));
    }
    
    [Test]
    public void TestParsesFieldDeclare()
    {
        var text = "public ColorNoAlphaSerializable saberAColor;";
        var line = new LineAnalyzer(text, null);
        
        Assert.That(line.Modifier, Is.EqualTo("public"));
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.IsField, Is.True);
        Assert.That(line.DeclaredName, Is.EqualTo("saberAColor"));
    }
}