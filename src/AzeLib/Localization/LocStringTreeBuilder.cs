using Klei;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AzeLib
{
    internal static class LocStringTreeBuilder
    {
        private static readonly MethodInfo collectLocStringTreeRoots =
            typeof(Localization).GetMethod(
                "CollectLocStringTreeRoots",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                binder: null,
                new[] { typeof(string), typeof(Assembly) },
                modifiers: null);

        private static readonly MethodInfo makeRuntimeLocStringTree =
            typeof(Localization).GetMethod(
                "MakeRuntimeLocStringTree",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                binder: null,
                new[] { typeof(Type) },
                modifiers: null);

        internal static IEnumerable<Type> CollectLocStringTreeRoots(string locStringNamespace, Assembly assembly)
        {
            if (collectLocStringTreeRoots != null)
            {
                if (collectLocStringTreeRoots.Invoke(null, new object[] { locStringNamespace, assembly }) is IEnumerable<Type> legacyRoots)
                    return legacyRoots;
            }

            if (assembly == null)
                return Enumerable.Empty<Type>();

            var loadableTypes = GetLoadableTypes(assembly);

            return loadableTypes
                .Where(type => type != null
                    && type.DeclaringType == null
                    && IsInNamespace(type, locStringNamespace))
                .OrderBy(type => type.FullName, StringComparer.Ordinal);
        }

        internal static Dictionary<string, object> MakeRuntimeLocStringTree(Type rootType)
        {
            if (makeRuntimeLocStringTree != null)
            {
                if (makeRuntimeLocStringTree.Invoke(null, new object[] { rootType }) is Dictionary<string, object> legacyTree)
                    return legacyTree;
            }

            return rootType == null ? new Dictionary<string, object>() : BuildTree(rootType);
        }

        private static Dictionary<string, object> BuildTree(Type type)
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal);

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(field => field.FieldType == typeof(LocString))
                .OrderBy(field => field.Name, StringComparer.Ordinal))
            {
                var value = field.GetValue(null);
                result[field.Name] = value is LocString loc ? ExtractText(loc) : string.Empty;
            }

            foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                .OrderBy(nested => nested.Name, StringComparer.Ordinal))
            {
                var nestedTree = BuildTree(nestedType);
                if (nestedTree.Count > 0)
                    result[nestedType.Name] = nestedTree;
            }

            return result;
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type != null);
            }
        }

        private static bool IsInNamespace(Type type, string locStringNamespace)
        {
            if (string.IsNullOrWhiteSpace(locStringNamespace) || type.Namespace == null)
                return false;

            if (type.Namespace.Equals(locStringNamespace, StringComparison.Ordinal))
                return true;

            return type.Namespace.StartsWith(locStringNamespace + ".", StringComparison.Ordinal);
        }

        private static string ExtractText(LocString locString)
        {
            return locString.text ?? locString.ToString() ?? string.Empty;
        }
    }
}
