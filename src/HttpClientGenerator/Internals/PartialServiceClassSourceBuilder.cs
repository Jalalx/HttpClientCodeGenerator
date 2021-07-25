﻿using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static HttpClientGenerator.Internals.HttpClientCodeGeneratorSyntaxReceiver;

namespace HttpClientGenerator.Internals
{
    internal class PartialServiceClassSourceBuilder
    {
        private readonly INamedTypeSymbol _classSymbol;
        private readonly HttpClientCodeGeneratorSyntaxReceiver.HttpBaseSerivceInfo _httpServiceInfo;
        private readonly List<HttpClientCodeGeneratorSyntaxReceiver.MarkedMethodInfo> _methodSymbols;
        private readonly IEnumerable<INamespaceSymbol> _namespaceSymbols;

        public PartialServiceClassSourceBuilder(INamedTypeSymbol classSymbol,
            HttpClientCodeGeneratorSyntaxReceiver.HttpBaseSerivceInfo httpServiceInfo, List<HttpClientCodeGeneratorSyntaxReceiver.MarkedMethodInfo> methodSymbols, IEnumerable<INamespaceSymbol> namespaceSymbols)
        {
            _classSymbol = classSymbol;
            _httpServiceInfo = httpServiceInfo;
            _methodSymbols = methodSymbols;
            _namespaceSymbols = namespaceSymbols;
        }


        public string Generate()
        {
            if (!_classSymbol.ContainingSymbol.Equals(_classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                return $"#error {_classSymbol.Name} must be a top level class but it is nested inside {_classSymbol.ContainingSymbol.Name}.";
            }

            // begin building the generated source
            var source = new SourceBuilder(0);
            WriteAutoGeneratedCodeComment(source);

            WriteUsingNamespaces(source);

            OpenClassDeclaration(source);

            if (_httpServiceInfo.HttpClientAccessorSymbol == null)
            {
                DeclareHttpClientDependencyConstructor(source);
            }

            foreach (var method in _methodSymbols)
            {
                ProcessMethod(source, method, _httpServiceInfo.HttpClientAccessorSymbol);
            }

            CloseClassDeclaration(source);

            return source.ToString();
        }

        private void DeclareHttpClientDependencyConstructor(SourceBuilder source)
        {
            source.AppendLine("protected readonly HttpClient _httpClient;");
            source.AppendLine();

            var className = _classSymbol.Name;
            source.AppendLine($"public {className}(HttpClient httpClient)");
            source.OpenBraket();
            source.AppendLine("_httpClient = httpClient;");
            source.CloseBraket();
        }

        private void OpenClassDeclaration(SourceBuilder source)
        {
            var namespaceName = _classSymbol.ContainingNamespace.ToDisplayString();
            source.AppendLine($"namespace {namespaceName}");
            source.OpenBraket();

            var className = _classSymbol.Name;
            source.AppendLine($"public partial class {className}");
            source.OpenBraket();
        }

        private void CloseClassDeclaration(SourceBuilder source)
        {
            source.CloseBraket(); // class
            source.CloseBraket(); // namespace
        }

        private void WriteUsingNamespaces(SourceBuilder source)
        {
            source.AppendLine(@"
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;");

            foreach (var nsSymbol in _namespaceSymbols)
            {
                source.AppendLine($"using {nsSymbol.ToDisplayString()};");
            }

            source.AppendLine();
            source.AppendLine();
        }

        private void WriteAutoGeneratedCodeComment(SourceBuilder source)
        {
            source.AppendLine(@"
// <auto-generated>
//     This code was generated by HttpClientCodeGenerator.
// </auto-generated>");
        }

        private void ProcessMethod(SourceBuilder source, MarkedMethodInfo methodInfo, ISymbol httpClientSymbol)
        {
            source.AppendLine();

            var method = methodInfo.PartialMethodSymbol;
            var accessibility = method.DeclaredAccessibility.ToSource();
            var returnTypeName = method.ReturnType.ToTypeParameterNameOnly();
            var @paramsArr = method.Parameters.Select(parameter => $"{parameter.Type.ToDisplayString()} {parameter.Name}")
                .ToArray();

            source.AppendLine($"{accessibility} partial async {returnTypeName} {method.Name}({string.Join(", ", paramsArr)})");

            source.OpenBraket();

            var httpVerbAttribute = methodInfo.HttpVerbAttributeData;

            // Define method variables
            DefineHttpMethodParameter(source, httpVerbAttribute, "@___httpMethod");
            DefinePathAndRouteAndQueryParameter(source, httpVerbAttribute, method, "@___routes", "@___path", "@___queryParams");
            DefineHeaderParameter(source, methodInfo.HttpRequestHeaderAttributeData, "@___headers");

            // Define HTTP invocation
            DefineHelperSendMethodInvocation(source, method, httpClientSymbol);

            source.CloseBraket();

            source.AppendLine();
        }


        private static string GetHttpClientMemberInvocation(ISymbol symbol)
            => symbol switch
            {
                IFieldSymbol fieldSymbol => fieldSymbol.Name,
                IPropertySymbol propertySymbol => propertySymbol.Name,
                IMethodSymbol methodSymbol => $"{methodSymbol.Name}()",
                _ => "_httpClient"
            };


        private static void DefineHelperSendMethodInvocation(SourceBuilder source, IMethodSymbol method, ISymbol httpClientSymbol)
        {
            var complexParameter = method.GetComplexTypeParameter();
            var methodReturnType = method.ReturnType as INamedTypeSymbol;
            var httpClientRef = GetHttpClientMemberInvocation(httpClientSymbol);

            if (methodReturnType.Name == "Task" && methodReturnType.IsGenericType)
            {
                var genericType = methodReturnType.TypeArguments[0].FullName();

                if (complexParameter == null)
                {
                    // HttpClientHelper.SendAsync<TResponse>(...)
                    var sourceCode = $"return await HttpClientGenerator.Shared.HttpClientHelper.SendAsync<{genericType}>({httpClientRef}, @___httpMethod, @___path, @___headers, @___routes, @___queryParams);";
                    source.AppendLine(sourceCode);
                }
                else
                {
                    // HttpClientHelper.SendDataAsync<TRequest, TResponse>(...)
                    var requestType = complexParameter.Type.FullName();
                    var sourceCode = $"return await HttpClientGenerator.Shared.HttpClientHelper.SendDataAsync<{requestType}, {genericType}>({httpClientRef}, @___httpMethod, @___path, @___headers, @___routes, @___queryParams, {complexParameter.Name});";
                    source.AppendLine(sourceCode);
                }
            }
            else if (methodReturnType.Name == "Task")
            {
                if (complexParameter == null)
                {
                    // HttpClientHelper.SendAsync(...)
                    var sourceCode = $"await HttpClientGenerator.Shared.HttpClientHelper.SendAsync({httpClientRef}, @___httpMethod, @___path, @___headers, @___routes, @___queryParams);";
                    source.AppendLine(sourceCode);
                }
                else
                {
                    // HttpClientHelper.SendDataAsync<TRequest>(...)
                    var requestType = complexParameter.Type.FullName();
                    var sourceCode = $"return await HttpClientGenerator.Shared.HttpClientHelper.SendDataAsync<{requestType}>({httpClientRef}, @___httpMethod, @___path, @___headers, @___routes, @___queryParams, {complexParameter.Name});";
                    source.AppendLine(sourceCode);
                }
            }
            else
            {
                // not supported
                source.AppendLine("/*Method not supported */ return Task.FromResult(0);");
            }
        }

        private void DefineHeaderParameter(SourceBuilder source, AttributeData[] httpHeaderAttribute, string variableName)
        {
            source.AppendLine($"var {variableName} = new Dictionary<string, string>();");
            if (httpHeaderAttribute != null && httpHeaderAttribute.Any())
            {
                foreach (var headerAttribute in httpHeaderAttribute)
                {
                    if (headerAttribute.ConstructorArguments.Length == 2)
                    {
                        var headerName = headerAttribute.ConstructorArguments[0].Value?.ToString();
                        var headerValues = headerAttribute.ConstructorArguments[1].Value?.ToString();

                        source.AppendLine($"{variableName}[\"{headerName}\"] = \"{headerValues}\";");
                    }
                }
            }
            else
            {
                source.AppendLine("// Header dictionary goes here...");
            }
            source.AppendLine();
        }

        private void DefineHttpMethodParameter(SourceBuilder source, AttributeData httpVerbAttribute, string variableName)
        {
            var httpVerb = "GET";
            switch (httpVerbAttribute.AttributeClass.Name)
            {
                case "HttpGetAttribute":
                    httpVerb = "GET";
                    break;
                case "HttpPatchAttribute":
                    httpVerb = "PATCH";
                    break;
                case "HttpPostAttribute":
                    httpVerb = "POST";
                    break;
                case "HttpPutAttribute":
                    httpVerb = "PUT";
                    break;
                case "HttpOptionAttribute":
                    httpVerb = "OPTION";
                    break;
                case "HttpDeleteAttribute":
                    httpVerb = "DELETE";
                    break;
            }

            source.AppendLine($"const string {variableName} = \"{httpVerb}\";");
            source.AppendLine();
        }

        private void DefinePathAndRouteAndQueryParameter(SourceBuilder source, AttributeData httpVerbAttribute,
                IMethodSymbol method, string routeVariableName, string pathVariableName, string queryVariableName)
        {
            if (method.Parameters.Count(x => x.Type.IsComplexType()) > 1)
            {
                source.AppendError($"Can not have more than one reference type as agrument.");
                return;
            }

            var path = httpVerbAttribute.ConstructorArguments[0].Value?.ToString();
            source.AppendLine($"var {pathVariableName} = \"{path}\";");

            source.AppendLine($"var {routeVariableName} = new Dictionary<string, object>();");

            var matches = Regex.Matches(path, "\\{\\w+\\}");
            var routeMatchedVariables = new HashSet<string>(matches.OfType<Match>().Where(m => m.Success).Select(m => m.Value.Trim('{', '}')));
            foreach (var routeKey in routeMatchedVariables)
            {
                // comparison is case-sensitive
                var methodParameter = method.Parameters.FirstOrDefault(p => p.Name == routeKey);
                if (methodParameter != null)
                {
                    if (methodParameter.Type.IsValueType)
                    {
                        source.AppendLine($"{routeVariableName}[\"{routeKey}\"] = {routeKey};");
                    }
                }
                else
                {
                    source.AppendError($"No parameter for route value {{{routeKey}}} was found!");
                }
            }

            source.AppendLine();
            source.AppendLine($"var {queryVariableName} = new Dictionary<string, object>();");

            var queryStringParams = method.Parameters.Where(p => !p.Type.IsComplexType() && !routeMatchedVariables.Contains(p.Name)).ToArray();
            if (queryStringParams.Length == 0)
            {
                source.AppendLine("// Query String dictionary goes here...");
            }
            else
            {
                foreach (var parameter in queryStringParams)
                {
                    source.AppendLine($"{queryVariableName}[\"{parameter.Name}\"] = {parameter.Name};");
                }
            }

            source.AppendLine();
        }
    }
}
