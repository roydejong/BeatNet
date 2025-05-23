﻿using System.Diagnostics;
using System.Net;
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
    public readonly bool IsProperty;
    public readonly string? DeclaredName;
    public readonly List<ClassInheritor>? ClassInheritors;
    public readonly string? DeclaredType;
    public readonly List<TypedParam>? MethodParams;
    public readonly bool IsOpenBracket;
    public readonly bool IsCloseBracket;
    public readonly bool IsEnumCase;
    public readonly string? DefaultValue;
    public readonly bool IsAttribute;

    public LineAnalyzer(string line, string? contextTypeName = null, bool? contextInEnum = false)
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
        IsField = false;
        IsProperty = false;
        DeclaredName = null;
        ClassInheritors = null;
        DeclaredType = null;
        MethodParams = null;
        IsOpenBracket = false;
        IsCloseBracket = false;
        IsAttribute = false;
        
        if (RawLine.Contains("<>") || RawLine.Contains(">5__2"))
            // Weird DisplayClass compiler-generated stuff - skip
            return;
        
        // -------------------------------------------------------------------------------------------------------------
        // Attributes

        if (RawLine.StartsWith("["))
        {
            IsAttribute = true;

            var idxStart = RawLine.IndexOf('[');
            var idxEnd = RawLine.IndexOf(']');
            var attributeContent = RawLine[(idxStart + 1)..idxEnd];
            
            var idxPropStarts = attributeContent.IndexOf('(');
            if (idxPropStarts == -1)
                DeclaredName = attributeContent;
            else
                DeclaredName = attributeContent[..idxPropStarts];

            return;
        }
        
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

        if (Words[0] == "delegate")
        {
            // We don't care about delegates (so far)
            return;
        }

        if (Words[0] == "async")
        {
            Words = Words[1..];
        }

        if (Words[0] == "{")
        {
            IsOpenBracket = true;
            return;
        }
        
        if (Words[0] == "}")
        {
            IsCloseBracket = true;
            return;
        }

        if (Words.Length == 0)
            Debugger.Break();

        // -------------------------------------------------------------------------------------------------------------
        // Enum case

        if (contextInEnum ?? false)
        {
            IsDeclaration = true;
            IsEnumCase = true;
            DeclaredName = Words[0].Trim(',');
            Words = Words[1..];

            var hasExplicitValue = Words.Length > 0 && Words[0] == "=";
            if (hasExplicitValue)
            {
                Words = Words[1..];
                DefaultValue = Words[0].Trim(',');
            }

            return;
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
                DeclaredType = Words[0];
            }

            return;
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
            
            var classLtIndex = DeclaredName.IndexOf('<');
            if (classLtIndex >= 0)
                DeclaredName = DeclaredName[..classLtIndex];

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

            return;
        }
        
        // -------------------------------------------------------------------------------------------------------------
        // Method declaration

        var hasValueAssignment = RawLine.Contains('=') && RawLine.Contains(';');
        if (!hasValueAssignment && (Words[0] == "void" || (IsDeclaration && RawLine.Contains('(') && RawLine.Contains(')'))))
        {
            IsDeclaration = true;
            IsMethod = true;
            
            // Constructor declaration
            if (contextTypeName != null && Words[0].StartsWith(contextTypeName) && Words[0].Contains('('))
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
                
                if (DeclaredName.Contains('<') && DeclaredName.Contains('>'))
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
                                    param.Name = paramPart.Trim(',');
                                }
                            }
                            
                            MethodParams.Add(param);
                        }
                    }
                    
                    var declaredNameEnd = DeclaredName.IndexOf('(');
                    DeclaredName = DeclaredName[..declaredNameEnd];
                }
            }

            return;
        }
        
        // -------------------------------------------------------------------------------------------------------------
        // Field / prop declaration

        IsField = IsDeclaration && RawLine.EndsWith(';');
        IsProperty = !IsField && IsDeclaration && Words.Length == 2;
        
        if (IsField || IsProperty)
        {
            DeclaredType = Words[0];
            DeclaredName = Words[1].Trim(';');

            var typeDotIdx = DeclaredType.IndexOf('.');
            if (typeDotIdx >= 0)
                DeclaredType = DeclaredType[(typeDotIdx + 1)..];

            var eqIdx = RawLine.IndexOf('=');
            if (eqIdx > 0)
                DefaultValue = RawLine[(eqIdx + 1)..].Trim().Trim(';');
            
            return;
        }
    }
}