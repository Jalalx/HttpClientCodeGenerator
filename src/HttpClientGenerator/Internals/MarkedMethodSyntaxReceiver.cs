using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace HttpClientGenerator.Internals
{
    internal class MarkedMethodSyntaxReceiver : ISyntaxContextReceiver
    {
        public static readonly string[] HttpVerbAttributes = new[]
        {
            "HttpClientGenerator.Shared.HttpVerbAttribute",
            "HttpClientGenerator.Shared.HttpGetAttribute",
            "HttpClientGenerator.Shared.HttpPostAttribute",
            "HttpClientGenerator.Shared.HttpPutAttribute",
            "HttpClientGenerator.Shared.HttpOptionAttribute",
            "HttpClientGenerator.Shared.HttpDeleteAttribute",
            "HttpClientGenerator.Shared.HttpPatchAttribute",
        };

        public static AttributeData GetHttpVerbAttribute(IMethodSymbol methodSymbol)
         => methodSymbol.GetAttributes().FirstOrDefault(attr =>
                HttpVerbAttributes.Contains(attr.AttributeClass.ToDisplayString()));

        public List<MarkedMethod> Methods { get; } = new List<MarkedMethod>();

        public HashSet<INamespaceSymbol> ImportedNamespaces { get; set; } = new HashSet<INamespaceSymbol>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is MethodDeclarationSyntax methodDeclarationSyntax && methodDeclarationSyntax.AttributeLists.Any())
            {
                var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;
                var attributeSymbol = GetHttpVerbAttribute(methodSymbol);
                if (attributeSymbol != null)
                {
                    Methods.Add(new MarkedMethod(methodSymbol, attributeSymbol));
                }
            }
            else if (context.Node is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
            {
                var namespaceSymbol = context.SemanticModel.GetDeclaredSymbol(namespaceDeclarationSyntax) as INamespaceSymbol;
                ImportedNamespaces.Add(namespaceSymbol);
            }
        }

        internal class MarkedMethod
        {
            public MarkedMethod(IMethodSymbol methodSymbol, AttributeData typeSymbol)
            {
                MethodSymbol = methodSymbol;
                HttpVerbAttributeSymbol = typeSymbol;
            }

            public IMethodSymbol MethodSymbol { get; }
            public AttributeData HttpVerbAttributeSymbol { get; }
        }
    }
}
