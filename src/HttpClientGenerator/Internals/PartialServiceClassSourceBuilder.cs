﻿using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
                //TODO: issue a diagnostic that it must be top level type
                return null;
            }

            var namespaceName = _classSymbol.ContainingNamespace.ToDisplayString();
            var className = _classSymbol.Name;

            // begin building the generated source
            var source = new SourceBuilder(1);

            foreach (var method in _methodSymbols)
            {
                ProcessMethod(source, method.PartialMethodSymbol, method.HttpVerbAttributeData, _httpServiceInfo.HttpClientAccessorSymbol);
            }

            var namespaceBuilder = new StringBuilder();
            foreach (var nsSymbol in _namespaceSymbols)
            {
                namespaceBuilder.AppendLine($"using {nsSymbol.ToDisplayString()};");
            }

            if (_httpServiceInfo.HttpClientAccessorSymbol == null)
            {
                var generatedSourceCode = ClientClassTemplate.ClassTemplateWithHttpClientDependency
                    .Replace(ClientClassTemplate.ImportNamespaces, namespaceBuilder.ToString())
                    .Replace(ClientClassTemplate.Namespace, namespaceName)
                    .Replace(ClientClassTemplate.ClassName, className)
                    .Replace(ClientClassTemplate.ClassBody, source.ToString());

                return generatedSourceCode;
            }
            else
            {
                var generatedSourceCode = ClientClassTemplate.ClassTemplateWithNoDependency
                    .Replace(ClientClassTemplate.ImportNamespaces, namespaceBuilder.ToString())
                    .Replace(ClientClassTemplate.Namespace, namespaceName)
                    .Replace(ClientClassTemplate.ClassName, className)
                    .Replace(ClientClassTemplate.ClassBody, source.ToString());

                return generatedSourceCode;
            }
        }


        private void ProcessMethod(SourceBuilder source, IMethodSymbol method, AttributeData httpVerbAttribute, ISymbol httpClientSymbol)
        {
            source.Indent();
            source.AppendLine();

            var accessibility = method.DeclaredAccessibility.ToSource();
            var returnTypeName = method.ReturnType.ToTypeParameterNameOnly();
            var @paramsArr = method.Parameters.Select(parameter => $"{parameter.Type.ToDisplayString()} {parameter.Name}")
                .ToArray();

            source.AppendLine($"{accessibility} partial async {returnTypeName} {method.Name}({string.Join(", ", paramsArr)})");

            GenerateMethodBody(source, method, httpVerbAttribute, httpClientSymbol);
            source.AppendLine();
            source.Unindent();
        }

        private void GenerateMethodBody(SourceBuilder source, IMethodSymbol method, AttributeData httpVerbAttribute, ISymbol httpClientSymbol)
        {
            source.OpenBraket();

            // Define method variables
            DefineHttpMethodParameter(source, httpVerbAttribute, "@___httpMethod");
            DefinePathAndRouteAndQueryParameter(source, httpVerbAttribute, method, "@___routes", "@___path", "@___queryParams");
            DefineHeaderParameter(source, httpVerbAttribute, "@___headers");

            // Define HTTP invocation
            DefineHelperSendMethodInvocation(source, method, httpClientSymbol);

            source.CloseBraket();
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

        private void DefineHeaderParameter(SourceBuilder source, AttributeData httpVerbAttribute, string variableName)
        {
            source.AppendLine($"var {variableName} = new Dictionary<string, string>();");
            source.AppendLine("// Header dictionary goes here...");
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