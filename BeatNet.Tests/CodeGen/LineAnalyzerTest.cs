﻿using BeatNet.CodeGen.Analysis;

namespace BeatNet.Tests.CodeGen;

public class LineAnalyzerTest
{
    [Test]
    public void TestPublicStaticClassDeclare()
    {
        var text = "public static class RankModel";
        var line = new LineAnalyzer(text);
        
        Assert.That(line.Modifier, Is.EqualTo("public"));
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.IsClass, Is.True);
        Assert.That(line.DeclaredName, Is.EqualTo("RankModel"));
    }
    
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
    public void TestParsesSimpleEnum()
    {
        var text = "public enum Rank";
        var line = new LineAnalyzer(text);
        
        Assert.That(line.Modifier, Is.EqualTo("public"));
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.IsEnum, Is.True);
        Assert.That(line.DeclaredName, Is.EqualTo("Rank"));
        Assert.That(line.DeclaredType, Is.Null);
    }
    
    [Test]
    public void TestParsesEnumWithBackingType()
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
    public void TestParsesMethodDeclare()
    {
        var text = "public static MultiplayerAvatarsData Deserialize(NetDataReader reader)";
        var line = new LineAnalyzer(text, contextInEnum: false);
        
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.IsMethod, Is.True);
        Assert.That(line.Modifier, Is.EqualTo("public"));
        Assert.That(line.Static, Is.True);
        Assert.That(line.DeclaredType, Is.EqualTo("MultiplayerAvatarsData"));
        Assert.That(line.DeclaredName, Is.EqualTo("Deserialize"));
    }
    
    [Test]
    public void TestParsesMethodWithNoModifierAndDotDeclare()
    {
        var text = "void INetSerializable.Deserialize(NetDataReader reader)";
        var line = new LineAnalyzer(text, contextInEnum: false);
        
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.IsMethod, Is.True);
        Assert.That(line.DeclaredType, Is.EqualTo("void"));
        Assert.That(line.DeclaredName, Is.EqualTo("INetSerializable.Deserialize"));
    }
    
    [Test]
    public void TestParsesComplexMethodDeclare_ListType()
    {
        var text = "private static List<MultiplayerAvatarData> DeserializeAvatarsData(NetDataReader reader)";
        var line = new LineAnalyzer(text, contextInEnum: false);
        
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.Modifier, Is.EqualTo("private"));
        Assert.That(line.IsMethod, Is.True);
        Assert.That(line.Static, Is.True);
        Assert.That(line.DeclaredType, Is.EqualTo("List<MultiplayerAvatarData>"));
        Assert.That(line.DeclaredName, Is.EqualTo("DeserializeAvatarsData"));
    }
    
    [Test]
    public void TestParsesComplexMethodDeclare_WithDefaultParam()
    {
        var text = "public static BeatmapLevelSelectionMask Deserialize(NetDataReader reader, uint version = 0U)";
        var line = new LineAnalyzer(text, contextInEnum: false);
        
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.Modifier, Is.EqualTo("public"));
        Assert.That(line.IsMethod, Is.True);
        Assert.That(line.Static, Is.True);
        Assert.That(line.DeclaredType, Is.EqualTo("BeatmapLevelSelectionMask"));
        Assert.That(line.DeclaredName, Is.EqualTo("Deserialize"));
    }
    
    [Test]
    public void TestParsesConstsWithValues()
    {
        var text = "private const int kBitCount = 16384;";
        var line = new LineAnalyzer(text, contextInEnum: false);
        
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.Modifier, Is.EqualTo("private"));
        Assert.That(line.IsField, Is.True);
        Assert.That(line.Const, Is.True);
        Assert.That(line.DeclaredType, Is.EqualTo("int"));
        Assert.That(line.DeclaredName, Is.EqualTo("kBitCount"));
        Assert.That(line.DefaultValue, Is.EqualTo("16384"));
    }
    
    [Test]
    public void TestParsesPropertyDeclare()
    {
        var text = "public List<IConnectedPlayer> activePlayersAtGameStart";
        var line = new LineAnalyzer(text, contextInEnum: false);
        
        Assert.That(line.IsDeclaration, Is.True);
        Assert.That(line.Modifier, Is.EqualTo("public"));
        Assert.That(line.IsProperty, Is.True);
        Assert.That(line.DeclaredType, Is.EqualTo("List<IConnectedPlayer>"));
        Assert.That(line.DeclaredName, Is.EqualTo("activePlayersAtGameStart"));
    }
    
    [Test]
    public void TestParsesSimpleAttributes()
    {
        var text = "[Flags]";
        var line = new LineAnalyzer(text, contextInEnum: false);
        
        Assert.That(line.IsAttribute, Is.True);
        Assert.That(line.DeclaredName, Is.EqualTo("Flags"));
    }
    
    [Test]
    public void TestParsesLessSimpleAttributes()
    {
        var text = "[AttributeUsage(AttributeTargets.Enum, Inherited = false)]";
        var line = new LineAnalyzer(text, contextInEnum: false);
        
        Assert.That(line.IsAttribute, Is.True);
        Assert.That(line.DeclaredName, Is.EqualTo("AttributeUsage"));
    }
}