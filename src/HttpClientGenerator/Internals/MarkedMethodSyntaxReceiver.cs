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

        public Dictionary<string, HttpBaseSerivceInfo> HttpServiceInfos { get; } = new Dictionary<string, HttpBaseSerivceInfo>();

#pragma warning disable RS1024 // Compare symbols correctly
        public HashSet<INamespaceSymbol> ImportedNamespaces { get; } = new HashSet<INamespaceSymbol>();
#pragma warning restore RS1024 // Compare symbols correctly

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is PropertyDeclarationSyntax propertyDeclarationSyntax)
            {
                var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax) as IPropertySymbol;
                if (propertySymbol.Type.Name == "HttpClient")
                {
                    var info = GetInfo(propertySymbol.ContainingType);
                    info.HttpClientSymbol = propertySymbol;
                }
            }
            else if (context.Node is FieldDeclarationSyntax variableDeclarationSyntax)
            {
                if (variableDeclarationSyntax.Declaration != null && variableDeclarationSyntax.Declaration.Variables.Any())
                {
                    var fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variableDeclarationSyntax.Declaration.Variables.First()) as IFieldSymbol;
                    if (fieldSymbol != null && fieldSymbol.Type.Name == "HttpClient")
                    {
                        var info = GetInfo(fieldSymbol.ContainingType);
                        info.HttpClientSymbol = fieldSymbol;
                    }
                }
            }
            else if (context.Node is MethodDeclarationSyntax markedMethodDeclarationSyntax && markedMethodDeclarationSyntax.AttributeLists.Any())
            {
                var methodSymbol = context.SemanticModel.GetDeclaredSymbol(markedMethodDeclarationSyntax) as IMethodSymbol;
                if (methodSymbol != null)
                {
                    var attributeSymbol = GetHttpVerbAttribute(methodSymbol);
                    if (attributeSymbol != null)
                    {
                        var info = GetInfo(methodSymbol.ContainingType);
                        info.Methods.Add(new MarkedMethod(methodSymbol, attributeSymbol));
                    }
                }
            }
            else if (context.Node is MethodDeclarationSyntax methodDeclarationSyntax && methodDeclarationSyntax.ParameterList.Parameters.Count == 0)
            {
                var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;
                if (methodSymbol != null)
                {
                    if (methodSymbol.Parameters.Length == 0 && methodSymbol.ReturnType.Name == "HttpClient")
                    {
                        var info = GetInfo(methodSymbol.ContainingType);
                        info.HttpClientSymbol = methodSymbol;
                    }
                }
            }
            else if (context.Node is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
            {
                var namespaceSymbol = context.SemanticModel.GetDeclaredSymbol(namespaceDeclarationSyntax) as INamespaceSymbol;
                if (namespaceSymbol != null)
                {
                    ImportedNamespaces.Add(namespaceSymbol);
                }
            }
        }

        private HttpBaseSerivceInfo GetInfo(INamedTypeSymbol containingTypeSymbol)
        {
            var containingTypeName = containingTypeSymbol.ToDisplayString();
            if (!HttpServiceInfos.ContainsKey(containingTypeName))
            {
                HttpServiceInfos[containingTypeName] = new HttpBaseSerivceInfo();
            }

            return HttpServiceInfos[containingTypeName];
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

        internal class DefinitionMemberInfo
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public bool IsField { get; set; }
        }

        internal class HttpBaseSerivceInfo
        {
            public HashSet<MarkedMethod> Methods { get; } = new HashSet<MarkedMethod>();

            public ISymbol HttpClientSymbol { get; set; }
        }
    }
}
