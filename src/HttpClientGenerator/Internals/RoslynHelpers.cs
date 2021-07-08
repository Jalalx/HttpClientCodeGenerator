using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace HttpClientGenerator.Internals
{
    internal static class RoslynHelpers
    {
        static readonly SymbolDisplayFormat typeWithNamespaceWriteFormat = new SymbolDisplayFormat(
                genericsOptions: SymbolDisplayGenericsOptions.None,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly);

        public static string FullName(this ITypeSymbol type)
        {
            var namedType = type as INamedTypeSymbol;

            var builder = new StringBuilder();
            builder.Append(type.ToDisplayString(typeWithNamespaceWriteFormat));

            if (namedType.IsGenericType)
            {
                builder.Append("<");
                for (int i = 0; i < namedType.TypeArguments.Length; i++)
                {
                    var typeParam = namedType.TypeArguments[i];
                    builder.Append($"{typeParam.ToDisplayString(typeWithNamespaceWriteFormat)}");
                    if (i != namedType.TypeArguments.Length - 1) builder.Append(", ");
                }
                builder.Append(">");
            }
            return builder.ToString();
        }

        public static bool IsComplexType(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol.IsValueType)
            {
                return false;
            }

            if (typeSymbol.Name == "String")
            {
                return false;
            }

            return true;
        }

        public static IParameterSymbol GetComplexTypeParameter(this IMethodSymbol method)
        {
            return method.Parameters.FirstOrDefault(p => p.Type.IsComplexType());
        }
    }
}
