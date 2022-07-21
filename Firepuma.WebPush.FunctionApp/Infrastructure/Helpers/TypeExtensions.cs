using System;
using System.Collections.Concurrent;

// ReSharper disable ReplaceSubstringWithRangeIndexer

namespace Firepuma.WebPush.FunctionApp.Infrastructure.Helpers;

public static class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, string> _cache = new ConcurrentDictionary<Type, string>();

    public static string GetShortTypeName(this Type type)
    {
        var fullName = type.FullName;
        var lastDotIndex = fullName?.LastIndexOf(".");

        return lastDotIndex >= 0
            ? fullName.Substring(lastDotIndex.Value + 1)
            : fullName;
    }

    public static string GetTypeNameExcludingNamespace(this Type type)
    {
        return _cache.GetOrAdd(type, TypeNameExcludingNamespace);
    }

    public static string GetTypeNamespace(this Type type)
    {
        return type.Namespace;
    }

    private static string TypeNameExcludingNamespace(Type type)
    {
        if (type.FullName == null) return "[NULL_FULLNAME]";
        if (type.Namespace == null) return "[NULL_NAMESPACE]";

        return type.FullName.Substring(type.Namespace.Length + 1);
    }
}