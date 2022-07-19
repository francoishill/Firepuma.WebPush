using System;

// ReSharper disable ReplaceSubstringWithRangeIndexer

namespace Firepuma.WebPush.FunctionApp.Infrastructure.Helpers;

public static class TypeExtensions
{
    public static string GetShortTypeName(this Type type)
    {
        var fullName = type.FullName;
        var lastDotIndex = fullName?.LastIndexOf(".");

        return lastDotIndex >= 0
            ? fullName.Substring(lastDotIndex.Value + 1)
            : fullName;
    }
}