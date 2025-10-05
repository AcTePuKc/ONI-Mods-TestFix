using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BetterInfoCards.Export
{
    public static class ExportWidgets
    {
        private static InfoCardWidgets curICWidgets;
        private static List<InfoCardWidgets> icWidgets = new();
        private static Type widgetEntryType;
        private static readonly Dictionary<Type, MemberInfo> rectMemberCache = new();
        private static readonly object rectMemberCacheLock = new();

        static ExportWidgets()
        {
            var hoverTextDrawerType = typeof(HoverTextDrawer);
            var entryType = FindWidgetEntryType(hoverTextDrawerType);
            widgetEntryType = entryType;

            var poolType = FindWidgetPoolType(hoverTextDrawerType, ref entryType);
            if (poolType == null)
            {
                Debug.LogWarning("[BetterInfoCards] Unable to locate HoverTextDrawer widget pool via reflection; falling back to Pool<MonoBehaviour>.");
                var fallback = GetFallbackPoolType(hoverTextDrawerType);
                poolType = fallback.poolType;
                entryType ??= fallback.entryType;
            }

            if (poolType == null)
            {
                Debug.LogWarning("[BetterInfoCards] Unable to locate HoverTextDrawer widget pool; skipping widget export patch.");
                return;
            }

            widgetEntryType = entryType ?? widgetEntryType ?? ResolveEntryTypeFromPool(poolType);

            var requiresConstruction = poolType.IsGenericTypeDefinition || (poolType.IsGenericType && poolType.ContainsGenericParameters);
            if (requiresConstruction)
            {
                var poolDefinition = poolType.IsGenericTypeDefinition
                    ? poolType
                    : poolType.IsGenericType ? poolType.GetGenericTypeDefinition() : null;

                if (poolDefinition != null)
                {
                    var componentType = ResolveComponentTypeForPool(poolType, widgetEntryType);
                    if (componentType == null)
                    {
                        Debug.LogWarning($"[BetterInfoCards] Unable to resolve component type for pool '{poolDefinition.FullName}'; skipping widget export patch.");
                        return;
                    }

                    try
                    {
                        var constructed = poolDefinition.MakeGenericType(componentType);
                        if (!ReferenceEquals(poolType, constructed))
                        {
                            if (!HasRectTransformEntry(constructed))
                            {
                                Debug.LogWarning($"[BetterInfoCards] Resolved pool '{constructed.FullName}' does not expose a RectTransform entry; skipping widget export patch.");
                                return;
                            }

                            poolType = constructed;
                        }
                    }
                    catch (ArgumentException)
                    {
                        Debug.LogWarning($"[BetterInfoCards] Component type '{componentType.FullName}' is incompatible with pool '{poolDefinition.FullName}'; skipping widget export patch.");
                        return;
                    }
                }
            }

            if (!HasRectTransformEntry(poolType))
            {
                Debug.LogWarning($"[BetterInfoCards] Unable to confirm RectTransform entry on pool '{poolType.FullName}'; skipping widget export patch.");
                return;
            }

            var drawMethod = AccessTools.Method(poolType, "Draw");
            if (drawMethod == null)
            {
                Debug.LogWarning($"[BetterInfoCards] Unable to locate Draw() on '{poolType.FullName}'; skipping widget export patch.");
                return;
            }

            var postfix = AccessTools.Method(typeof(ExportWidgets), nameof(GetWidget_Postfix));
            if (postfix == null)
            {
                Debug.LogWarning("[BetterInfoCards] Unable to locate ExportWidgets.GetWidget_Postfix; skipping widget export patch.");
                return;
            }

            var harmony = new Harmony("BetterInfoCards.Export.ExportWidgets");
            harmony.Patch(drawMethod, postfix: new HarmonyMethod(postfix));
        }

        private static Type FindWidgetPoolType(Type hoverTextDrawerType, ref Type entryType)
        {
            entryType ??= FindWidgetEntryType(hoverTextDrawerType);

            if (entryType != null)
            {
                foreach (var nestedType in hoverTextDrawerType.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public))
                {
                    if (!nestedType.IsGenericTypeDefinition)
                        continue;

                    if (nestedType.GetGenericArguments().Length != 1)
                        continue;

                    if (!IsPoolTypeName(nestedType))
                        continue;

                    try
                    {
                        var constructed = nestedType.MakeGenericType(entryType);
                        if (HasRectTransformEntry(constructed))
                        {
                            entryType ??= ResolveEntryTypeFromPool(constructed);
                            return constructed;
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Ignore incompatible generic definitions.
                    }
                }
            }

            const BindingFlags memberFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            const string shadowBarsName = "shadowbars";

            Type preferredFieldPool = null;
            Type fallbackFieldPool = null;

            foreach (var field in hoverTextDrawerType.GetFields(memberFlags))
            {
                var poolType = field.FieldType;
                if (!IsPoolType(poolType) || !HasRectTransformEntry(poolType))
                    continue;

                if (!field.IsStatic && field.Name != null && field.Name.IndexOf(shadowBarsName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    entryType ??= ResolveEntryTypeFromPool(poolType);
                    return poolType;
                }

                if (!field.IsStatic && preferredFieldPool == null)
                {
                    preferredFieldPool = poolType;
                    continue;
                }

                if (fallbackFieldPool == null)
                    fallbackFieldPool = poolType;
            }

            if (preferredFieldPool != null)
            {
                entryType ??= ResolveEntryTypeFromPool(preferredFieldPool);
                return preferredFieldPool;
            }

            if (fallbackFieldPool != null)
            {
                entryType ??= ResolveEntryTypeFromPool(fallbackFieldPool);
                return fallbackFieldPool;
            }

            Type preferredPropertyPool = null;

            foreach (var property in hoverTextDrawerType.GetProperties(memberFlags))
            {
                var poolType = property.PropertyType;
                if (!IsPoolType(poolType) || !HasRectTransformEntry(poolType))
                    continue;

                if (property.Name != null && property.Name.IndexOf(shadowBarsName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    entryType ??= ResolveEntryTypeFromPool(poolType);
                    return poolType;
                }

                if (preferredPropertyPool == null)
                    preferredPropertyPool = poolType;
            }

            if (preferredPropertyPool != null)
            {
                entryType ??= ResolveEntryTypeFromPool(preferredPropertyPool);
                return preferredPropertyPool;
            }

            return null;
        }

        private static bool IsPoolType(Type type)
        {
            if (type == null)
                return false;

            var definition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            return IsPoolTypeName(definition);
        }

        private static bool IsPoolTypeName(Type type)
        {
            return type != null && type.Name.StartsWith("Pool", StringComparison.Ordinal);
        }

        private static Type FindWidgetEntryType(Type hoverTextDrawerType)
        {
            return FindWidgetEntryTypeRecursive(hoverTextDrawerType);
        }

        private static Type FindWidgetEntryTypeRecursive(Type declaringType)
        {
            if (declaringType == null)
                return null;

            const BindingFlags nestedFlags = BindingFlags.NonPublic | BindingFlags.Public;
            Type fallback = null;
            foreach (var nestedType in declaringType.GetNestedTypes(nestedFlags))
            {
                var rectMember = GetRectTransformMember(nestedType);
                if (rectMember != null)
                {
                    if (string.Equals(nestedType.Name, "Entry", StringComparison.Ordinal))
                        return nestedType;

                    fallback ??= nestedType;
                }

                var child = FindWidgetEntryTypeRecursive(nestedType);
                if (child != null)
                    return child;
            }

            return fallback;
        }

        private static MemberInfo FindRectTransformMember(Type type)
        {
            if (type == null)
                return null;

            const BindingFlags declaredFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            for (var current = type; current != null; current = current.BaseType)
            {
                foreach (var field in current.GetFields(declaredFlags))
                {
                    if (typeof(RectTransform).IsAssignableFrom(field.FieldType))
                        return field;
                }

                foreach (var property in current.GetProperties(declaredFlags))
                {
                    if (!property.CanRead)
                        continue;

                    if (property.GetIndexParameters().Length > 0)
                        continue;

                    if (typeof(RectTransform).IsAssignableFrom(property.PropertyType))
                        return property;
                }
            }

            return null;
        }

        internal static MemberInfo GetRectTransformMember(Type type)
        {
            if (type == null)
                return null;

            lock (rectMemberCacheLock)
            {
                if (!rectMemberCache.TryGetValue(type, out var member))
                {
                    member = FindRectTransformMember(type);
                    rectMemberCache[type] = member;
                }

                return member;
            }
        }

        private static (Type poolType, Type entryType) GetFallbackPoolType(Type hoverTextDrawerType)
        {
            const BindingFlags nestedFlags = BindingFlags.NonPublic | BindingFlags.Public;

            var genericPool = hoverTextDrawerType.GetNestedType("Pool`1", nestedFlags);
            if (genericPool != null)
            {
                try
                {
                    var monoPool = genericPool.MakeGenericType(typeof(MonoBehaviour));
                    var entryType = monoPool.GetNestedType("Entry", nestedFlags) ?? FindWidgetEntryTypeRecursive(monoPool);

                    if (entryType != null && GetRectTransformMember(entryType) != null)
                        return (monoPool, entryType);

                    if (HasRectTransformEntry(monoPool))
                        return (monoPool, entryType);
                }
                catch (ArgumentException)
                {
                    // Ignore incompatible generic definitions.
                }
            }

            return (null, null);
        }

        private static bool HasRectTransformEntry(Type poolType)
        {
            var entryType = poolType.GetNestedType("Entry", BindingFlags.NonPublic | BindingFlags.Public);
            if (entryType == null)
                return false;

            return GetRectTransformMember(entryType) != null;
        }

        private static Type ResolveEntryTypeFromPool(Type poolType)
        {
            if (poolType == null)
                return null;

            const BindingFlags nestedFlags = BindingFlags.NonPublic | BindingFlags.Public;
            return poolType.GetNestedType("Entry", nestedFlags) ?? FindWidgetEntryTypeRecursive(poolType);
        }

        private static Type ResolveComponentTypeForPool(Type poolType, Type entryType)
        {
            static Type ExtractComponent(Type candidate)
            {
                if (candidate == null)
                    return null;

                if (candidate.IsGenericType && !candidate.ContainsGenericParameters)
                {
                    var arguments = candidate.GetGenericArguments();
                    if (arguments.Length == 1)
                        return arguments[0];
                }

                return null;
            }

            var componentType = ExtractComponent(poolType);
            if (componentType != null)
                return componentType;

            for (var declaring = entryType?.DeclaringType; declaring != null; declaring = declaring.DeclaringType)
            {
                componentType = ExtractComponent(declaring);
                if (componentType != null)
                    return componentType;

                if (poolType != null && poolType.IsGenericTypeDefinition && declaring.IsGenericType && !declaring.ContainsGenericParameters)
                {
                    var definition = declaring.GetGenericTypeDefinition();
                    if (definition == poolType)
                    {
                        var arguments = declaring.GetGenericArguments();
                        if (arguments.Length == 1)
                            return arguments[0];
                    }
                }
            }

            return null;
        }

        public static List<InfoCardWidgets> ConsumeWidgets()
        {
            var cardWidgets = icWidgets;
            icWidgets = new();
            return cardWidgets;
        }

        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginDrawing))]
        class OnBeginDrawing
        {
            static void Postfix()
            {
                curICWidgets = null;
                icWidgets.Clear();
            }
        }

        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginShadowBar))]
        class OnBeginShadowBar
        {
            static void Prefix()
            {
                if (!InterceptHoverDrawer.IsInterceptMode)
                {
                    curICWidgets = new();
                    icWidgets.Add(curICWidgets);
                }
            }
        }

        // This method will be used as a postfix for the Draw method via reflection.
        private static void GetWidget_Postfix(object __result, object ___prefab)
        {
            if (__result == null)
                return;

            var prefab = NormalizeToGameObject(___prefab);
            if (prefab == null)
                return;

            if (InterceptHoverDrawer.IsInterceptMode)
                return;

            if (curICWidgets == null)
            {
                curICWidgets = new();
                icWidgets.Add(curICWidgets);
            }

            if (!ShouldProcessEntry(__result))
                return;

            curICWidgets.AddWidget(__result, prefab);
        }

        private static GameObject NormalizeToGameObject(object instance)
        {
            switch (instance)
            {
                case GameObject gameObject:
                    return gameObject;
                case Component component:
                    return component != null ? component.gameObject : null;
                default:
                    return null;
            }
        }

        private static bool ShouldProcessEntry(object entry)
        {
            if (entry == null)
                return false;

            var type = entry.GetType();

            if (widgetEntryType != null)
            {
                if (widgetEntryType.IsInstanceOfType(entry))
                    return true;

                if (type.IsAssignableFrom(widgetEntryType))
                    return true;
            }

            var rectMember = GetRectTransformMember(type);
            if (rectMember != null)
            {
                widgetEntryType ??= rectMember.DeclaringType ?? type;
                return true;
            }

            if (typeof(Component).IsAssignableFrom(type))
            {
                if (widgetEntryType == null || typeof(Component).IsAssignableFrom(widgetEntryType))
                    widgetEntryType = type;
                return true;
            }

            return false;
        }
    }
}
