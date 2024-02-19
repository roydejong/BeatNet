using BeatNet.CodeGen.Analysis;

namespace BeatNet.Tests.CodeGen;

public class LineAnalyzerTest
{
    [Test]
    public void TestParsesStructDeclare()
    {
        var text = "public struct ColorSchemeNetSerializable : INetSerializable";
        var line = new LineAnalyzer(text);
        
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
        var line = new LineAnalyzer(text);
        
        Assert.That(line.Modifier, Is.EqualTo("public"));
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.IsField, Is.True);
        Assert.That(line.DeclaredName, Is.EqualTo("saberAColor"));
        Assert.That(line.DeclaredType, Is.EqualTo("ColorNoAlphaSerializable"));
    }
    
    [Test]
    public void TestParsesFieldWithDotDeclare()
    {
        var text = "private GameplayModifiers.EnabledObstacleType _enabledObstacleType;";
        var line = new LineAnalyzer(text);
        
        Assert.That(line.Modifier, Is.EqualTo("private"));
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.IsField, Is.True);
        Assert.That(line.DeclaredName, Is.EqualTo("_enabledObstacleType"));
        Assert.That(line.DeclaredType, Is.EqualTo("EnabledObstacleType"));
    }
    
    [Test]
    public void TestParsesEnum()
    {
        var text = "private enum RpcType : byte";
        var line = new LineAnalyzer(text);
        
        Assert.That(line.Modifier, Is.EqualTo("private"));
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.IsEnum, Is.True);
        Assert.That(line.DeclaredName, Is.EqualTo("RpcType"));
        Assert.That(line.DeclaredType, Is.EqualTo("byte"));
    }
    
    [Test]
    public void TestParsesEnumCase()
    {
        var text = "SetPlayersMissingEntitlementsToLevel = 123,";
        var line = new LineAnalyzer(text, contextInEnum: true);
        
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.IsEnumCase, Is.True);
        Assert.That(line.DeclaredName, Is.EqualTo("SetPlayersMissingEntitlementsToLevel"));
        Assert.That(line.DefaultValue, Is.EqualTo("123"));
    }
    
    [Test]
    public void TestParsesMethodWithNoModifierAndDotDeclare()
    {
        var text = "void INetSerializable.Deserialize(NetDataReader reader)";
        var line = new LineAnalyzer(text, contextInEnum: false);
        
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.IsMethod, Is.True);
        Assert.That(line.DeclaredName, Is.EqualTo("INetSerializable.Deserialize"));
    }
}