using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

/// <summary>
/// Created on demand before each generation pass
/// </summary>
namespace UseExternalLibraries.GeneratorLibrary;
internal class SyntaxReceiver : ISyntaxReceiver
{
    public IList<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();
    public IList<RecordDeclarationSyntax> CandidateRecords { get; } = new List<RecordDeclarationSyntax>();
    public IList<StructDeclarationSyntax> CandidateStructs { get; } = new List<StructDeclarationSyntax>();

    /// <summary>
    /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
    /// </summary>
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
        {
            //the sample accepts all classes.  we could change it eventually if we need to.
            //default is to accept all classes.  however, could change to require at least one attribute to consider.
            CandidateClasses.Add(classDeclarationSyntax);
        }
        if (syntaxNode is RecordDeclarationSyntax recordDeclarationSyntax)
        {
            CandidateRecords.Add(recordDeclarationSyntax);
        }
        if (syntaxNode is StructDeclarationSyntax structDeclarationSyntax)
        {
            CandidateStructs.Add(structDeclarationSyntax);
        }
    }
}