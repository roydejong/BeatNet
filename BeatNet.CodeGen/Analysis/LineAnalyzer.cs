using System.Diagnostics;
using BeatNet.CodeGen.Analysis.Structs;

namespace BeatNet.CodeGen.Analysis;

// The following work is a work of art and should be treated as such.
//  (is this a dumb way to do it? yes. does it also work really well? also yes.)

public class LineAnalyzer
{
    public readonly string RawLine;
    public readonly string[] Words;
    public readonly bool IsDeclaration;
    public readonly string? Modifier;
    public readonly bool Static;
    public readonly bool Abstract;
    public readonly bool Override;
    public readonly bool Virtual;
    public readonly bool ReadOnly;
    public readonly bool Const;
    public readonly bool IsEnum;
    public readonly bool IsStruct;
    public readonly bool IsClass;
    public readonly bool IsMethod;
    public readonly bool IsConstructor;
    public readonly bool IsField;
    public readonly string? DeclaredName;
    public readonly string? EnumBaseType;
    public readonly List<ClassInheritor>? ClassInheritors;
    public readonly string? DeclaredType;
    public readonly List<TypedParam>? MethodParams;

    public LineAnalyzer(string line, string? contextTypeName)
    {
        RawLine = line;
        Words = RawLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        IsDeclaration = false;
        Modifier = null;
        Static = false;
        Abstract = false;
        Override = false;
        Virtual = false;
        ReadOnly = false;
        Const = false;
        IsEnum = false;
        IsStruct = false;
        IsClass = false;
        IsMethod = false;
        IsConstructor = false;
        DeclaredName = null;
        EnumBaseType = null;
        ClassInheritors = null;
        DeclaredType = null;
        MethodParams = null;

        // -------------------------------------------------------------------------------------------------------------
        // Access modifiers

        var hasModifier = Words[0] switch
        {
            "public" => true,
            "private" => true,
            "protected" => true,
            "internal" => true,
            _ => false
        };

        if (hasModifier)
        {
            Modifier = hasModifier ? Words[0] : null;
            IsDeclaration = true;
            Words = Words[1..];

            // Check for dual modifier (protected internal, private protected)
            var hasDualModifier = Words[0] switch
            {
                "protected" => true,
                "internal" => true,
                _ => false
            };

            if (hasDualModifier)
            {
                Modifier = $"{Modifier} {Words[0]}";
                Words = Words[1..];
            }
        }

        // -------------------------------------------------------------------------------------------------------------
        // static, readonly, const

        if (Words[0] == "static")
        {
            Static = true;
            IsDeclaration = true;
            Words = Words[1..];
        }

        if (Words[0] == "abstract")
        {
            Abstract = true;
            IsDeclaration = true;
            Words = Words[1..];
        }

        if (Words[0] == "virtual")
        {
            Virtual = true;
            IsDeclaration = true;
            Words = Words[1..];
        }

        if (Words[0] == "override")
        {
            Override = true;
            IsDeclaration = true;
            Words = Words[1..];
        }

        if (Words[0] == "readonly")
        {
            ReadOnly = true;
            IsDeclaration = true;
            Words = Words[1..];
        }

        if (Words[0] == "const")
        {
            Const = true;
            IsDeclaration = true;
            Words = Words[1..];
        }

        // -------------------------------------------------------------------------------------------------------------
        // Enum declaration

        if (Words[0] == "enum")
        {
            Words = Words[1..];
            IsDeclaration = true;
            IsEnum = true;

            DeclaredName = Words[0];
            Words = Words[1..];

            if (Words.Length > 0 && Words[0] == ":")
            {
                Words = Words[1..];
                EnumBaseType = Words[0];
            }
        }

        // -------------------------------------------------------------------------------------------------------------
        // Class declaration
        
        if (Words[0] == "class")
        {
            IsClass = true;
        }
        else if (Words[0] == "struct")
        {
            IsStruct = true;
        }

        if (IsClass || IsStruct)
        {
            IsDeclaration = true;
            Words = Words[1..];

            DeclaredName = Words[0];
            Words = Words[1..];

            if (Words.Length > 0 && Words[0] == ":")
            {
                // Base classes / interfaces
                Words = Words[1..];

                ClassInheritors = new List<ClassInheritor>();

                ClassInheritor? inheritor = null;
                var inGenericGroup = false;

                foreach (var inheritWord in Words)
                {
                    if (inheritor == null)
                    {
                        // Starting a new inheritor
                        inheritor = new ClassInheritor();
                        inheritor.TypeName = inheritWord;
                        inGenericGroup = false;
                    }
                    
                    var ltIndex = inheritWord.IndexOf('<');
                    var gtIndex = inheritWord.IndexOf('>');
                    
                    if (inGenericGroup)
                    {
                        if (inGenericGroup && gtIndex > 0)
                        {
                            var lastGeneric = inheritWord[..gtIndex];
                            inheritor.Generics!.Add(lastGeneric);

                            inGenericGroup = false;
                        }
                        else if (inGenericGroup)
                        {
                            var nextGeneric = inheritWord;
                            nextGeneric = nextGeneric.Trim(',', '<', '>');
                            inheritor.Generics!.Add(nextGeneric);
                        }
                    }
                    else if (ltIndex > 0)
                    {
                        // Starting a new generic group
                        inheritor.TypeName = inheritWord[..ltIndex];

                        var firstGeneric = inheritWord[ltIndex..];
                        firstGeneric = firstGeneric.Trim(',', '<', '>');
                        inheritor.Generics = new List<string>() { firstGeneric };

                        if (gtIndex == -1)
                        {
                            // Opens but does not close, next loop will continue generic group
                            inGenericGroup = true;
                        }
                    }
                    
                    if (!inGenericGroup)
                    {
                        // Not continuing a generic group
                        ClassInheritors.Add(inheritor);
                        inheritor = null;
                    }
                }
            }
        }
        
        // -------------------------------------------------------------------------------------------------------------
        // Method declaration
        
        if (IsDeclaration && RawLine.Contains('(') && RawLine.Contains(')'))
        {
            IsMethod = true;
            
            // Constructor declaration
            if (contextTypeName != null && Words[0].StartsWith(contextTypeName))
            {
                IsConstructor = true;
                
                DeclaredType = contextTypeName;
                DeclaredName = contextTypeName;
            }
            else
            {
                // Method declaration
                DeclaredType = Words[0];
                DeclaredName = Words[1];

                if (DeclaredType == "event")
                    // We don't care about events (so far)
                    return;
                
                if (RawLine.Contains('<') && RawLine.Contains('>'))
                    // We don't care about generic methods (so far)
                    return;

                if (DeclaredType == "operator" || DeclaredName == "operator")
                    // We don't care about operator overloads (so far)
                    return;
                
                var paramListIdx = RawLine.IndexOf('(');

                if (paramListIdx >= 0)
                {
                    var paramListEndIdx = RawLine.IndexOf(')');
                    var paramList = RawLine[(paramListIdx + 1)..paramListEndIdx];

                    if (paramList.Length > 0)
                    {
                        MethodParams = new List<TypedParam>();

                        foreach (var paramDeclare in paramList.Split(','))
                        {
                            var partIdx = 0;
                            var param = new TypedParam();
                            
                            foreach (var paramPart in paramDeclare.Trim().Split(' '))
                            {
                                partIdx++;
                                
                                if (partIdx == 1)
                                {
                                    param.TypeName = paramPart;
                                }
                                else if (partIdx == 2)
                                {
                                    param.ParamName = paramPart.Trim(',');
                                }
                            }
                            
                            MethodParams.Add(param);
                        }
                    }
                    
                    var declaredNameEnd = DeclaredName.IndexOf('(');
                    DeclaredName = DeclaredName[..declaredNameEnd];
                }
            }
        }
        
        // -------------------------------------------------------------------------------------------------------------
        // Field declaration

        if (IsDeclaration && !IsMethod && RawLine.EndsWith(';'))
        {
            IsField = true;
            DeclaredType = Words[0];
            DeclaredName = Words[1].Trim(';');
        }
    }
}