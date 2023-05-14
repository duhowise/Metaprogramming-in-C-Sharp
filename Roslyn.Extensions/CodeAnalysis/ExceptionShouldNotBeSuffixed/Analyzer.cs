﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslyn.Extensions.CodeAnalysis.ExceptionShouldNotBeSuffixed;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Analyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "PP0001";

    public static readonly DiagnosticDescriptor Rule = new(
         id: DiagnosticId,
         title: "ExceptionShouldNotBeSuffixed",
         messageFormat: "The use of the word 'Exception' should not be added as a suffix - create a well understood and self explanatory name for the exception",
         category: "Naming",
         defaultSeverity: DiagnosticSeverity.Error,
         isEnabledByDefault: true,
         description: null,
         helpLinkUri: string.Empty,
         customTags: Array.Empty<string>());

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(
            HandleClassDeclaration,
            ImmutableArray.Create(
                SyntaxKind.ClassDeclaration));
    }

    void HandleClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = context.Node as ClassDeclarationSyntax;
        if (classDeclaration?.BaseList == null || classDeclaration?.BaseList?.Types == null) return;

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol?.BaseType is null) return;

        var inheritsException = classSymbol.BaseType.ContainingNamespace.Name.StartsWith("System", StringComparison.InvariantCulture) &&
            classSymbol.BaseType.Name.EndsWith("Exception", StringComparison.InvariantCulture);

        if (inheritsException && classDeclaration.Identifier.Text.EndsWith("Exception", StringComparison.InvariantCulture))
        {
            var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
