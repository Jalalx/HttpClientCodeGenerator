using Microsoft.CodeAnalysis;
using System;

namespace HttpClientGenerator.Internals
{
    internal static class HelperExtensions
    {
        public static string GetUniqueString() => Guid.NewGuid().ToString("N");
        public static string ToSource(this Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.NotApplicable:
                case Accessibility.Private:
                    return "private";
                case Accessibility.ProtectedAndInternal:
                    return "protected internal";
                case Accessibility.Protected:
                    return "protected";
                case Accessibility.Internal:
                    return "internal";
                case Accessibility.ProtectedOrInternal:
                    return "protected internal";
                case Accessibility.Public:
                    return "public";
                default:
                    return string.Empty;
            }
        }
    }
}
