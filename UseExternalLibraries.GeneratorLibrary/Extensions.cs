using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace UseExternalLibraries.GeneratorLibrary;
internal static class Extensions
{
    //this is for all my extensions.
    //allows refactoring.
    //can probably include in the templates since it could be needed a lot.
    public static INamedTypeSymbol GetClassSymbol(this Compilation compilation, ClassDeclarationSyntax clazz)
    {
        var model = compilation.GetSemanticModel(clazz.SyntaxTree);
        var classSymbol = model.GetDeclaredSymbol(clazz)!;
        return classSymbol; //could be cast to something else (not sure).
    }
    public static INamedTypeSymbol GetRecordSymbol(this Compilation compilation, RecordDeclarationSyntax record)
    {
        var model = compilation.GetSemanticModel(record.SyntaxTree);
        var recordSymbol = model.GetDeclaredSymbol(record)!;
        return recordSymbol;
    }
    public static INamedTypeSymbol GetStructSymbol(this Compilation compilation, StructDeclarationSyntax struz)
    {
        var model = compilation.GetSemanticModel(struz.SyntaxTree);
        var structSymbol = model.GetDeclaredSymbol(struz)!;
        return structSymbol;
    }
    public static string GetAccessModifier(this INamedTypeSymbol symbol)
    {
        return symbol.DeclaredAccessibility.ToString().ToLowerInvariant();
    }
    public static bool IsPartial(this ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }
    public static bool IsPartial(this RecordDeclarationSyntax recordDeclaration)
    {
        return recordDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }
    public static bool IsPartial(this StructDeclarationSyntax structDeclaration)
    {
        return structDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }
    public static bool IsMappable(this ClassDeclarationSyntax source) => source.HasInterface("IMappable");
    public static bool HasInterface(this ClassDeclarationSyntax source, string interfaceName)
    {
        if (source.BaseList is null)
        {
            return false;
        }
        IEnumerable<BaseTypeSyntax> baseTypes = source.BaseList.Types.Select(baseType => baseType);

        // To Do - cleaner interface finding.
        return baseTypes.Any(baseType => baseType.ToString() == interfaceName);
    }
    public static bool IsMappable(this RecordDeclarationSyntax source) => source.HasInterface("IMappable");
    public static bool HasInterface(this RecordDeclarationSyntax source, string interfaceName)
    {
        if (source.BaseList is null)
        {
            return false;
        }
        IEnumerable<BaseTypeSyntax> baseTypes = source.BaseList.Types.Select(baseType => baseType);

        // To Do - cleaner interface finding.
        return baseTypes.Any(baseType => baseType.ToString() == interfaceName);
    }
    public static bool IsMappable(this StructDeclarationSyntax source) => source.HasInterface("IMappable");
    public static bool HasInterface(this StructDeclarationSyntax source, string interfaceName)
    {
        if (source.BaseList is null)
        {
            return false;
        }
        IEnumerable<BaseTypeSyntax> baseTypes = source.BaseList.Types.Select(baseType => baseType);

        // To Do - cleaner interface finding.
        return baseTypes.Any(baseType => baseType.ToString() == interfaceName);
    }
    public static bool IsMappable(this ITypeSymbol symbol) => symbol.HasInterface("IMappable");
    public static bool HasInterface(this ITypeSymbol symbol, string interfaceName)
    {
        var firsts = symbol.AllInterfaces;
        return firsts.Any(xx => xx.Name == interfaceName);
    }
    public static bool IsCollection(this ITypeSymbol symbol)
    {
        bool rets = symbol.HasInterface("ICollection");
        return rets;
    }
    public static bool IsCollection(this IPropertySymbol symbol) //has to look for collection now.
    {
        return symbol.Type.IsCollection();
    }
    public static ITypeSymbol? GetCollectionSingleGenericTypeUsed(this ITypeSymbol symbol)
    {
        INamedTypeSymbol? others = symbol as INamedTypeSymbol;
        if (others is null)
        {
            return null;
        }
        if (others.TypeArguments.Count() is not 1)
        {
            return null;
        }
        return others.TypeArguments[0];
    }
    public static ITypeSymbol? GetCollectionSingleGenericTypeUsed(this IPropertySymbol symbol)
    {
        return symbol.Type.GetCollectionSingleGenericTypeUsed();

    }
    //public static bool IsBasicList(this ITypeSymbol symbol) => symbol.hasin
    public static bool IsSimpleType(this ITypeSymbol symbol)
    {
        if (symbol.Name == "String")
        {
            return true;
        }
        if (symbol.Name == "Nullable")
        {
            return true;
        }
        if (symbol.TypeKind == TypeKind.Enum)
        {
            return true;
        }
        if (symbol.TypeKind == TypeKind.Struct)
        {
            return true;
        }
        return false;
    }
    public static bool IsSimpleType(this IPropertySymbol symbol)
    {
        return symbol.Type.IsSimpleType();
    }
    public static List<IPropertySymbol> GetProperties(this INamedTypeSymbol symbol) => symbol.GetMembers().OfType<IPropertySymbol>().ToList();
    public static bool TryGetAttribute(this ISymbol symbol, string attributeName, out IEnumerable<AttributeData> attributes)
    {
        string otherName = attributeName.Replace("Attribute", "");
        attributes = symbol.GetAttributes()
            .Where(a => a.AttributeClass is not null && (a.AttributeClass.Name == attributeName || a.AttributeClass.Name == otherName));
        return attributes.Any();
    }
    public static bool TryGetAttribute(this ISymbol symbol, INamedTypeSymbol attributeType, out IEnumerable<AttributeData> attributes)
    {
        attributes = symbol.GetAttributes()
            .Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType));
        return attributes.Any();
    }
    public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeType)
    {
        return symbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType));
    }
    public static bool HasAttribute(this ISymbol symbol, string attributeName)
    {
        string otherName = attributeName.Replace("Attribute", "");
        return symbol.GetAttributes()
            .Any(a => a.AttributeClass is not null && (a.AttributeClass.Name == attributeName || a.AttributeClass.Name == otherName));
    }
    public static bool AttributePropertyUsed(this IEnumerable<AttributeData> attributes, string propertyName)
    {
        AttributeData attribute = attributes.Single();
        bool? output = (bool?)attribute.NamedArguments.FirstOrDefault(xx => xx.Key.Equals(propertyName)).Value.Value;
        if (output is null)
        {
            return false;
        }
        return output.Value;
    }
}