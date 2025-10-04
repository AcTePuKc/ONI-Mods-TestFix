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
            var poolType = FindWidgetPoolType(hoverTextDrawerType, out var entryType);
            if (poolType == null)
            {
                Debug.LogWarning("[BetterInfoCards] Unable to locate HoverTextDrawer widget pool via reflection; falling back to Pool<MonoBehaviour>.");
                var fallback = GetFallbackPoolType(hoverTextDrawerType);
                poolType = fallback.poolType;
                if (entryType == null)
                    entryType = fallback.entryType;
            }

            if (poolType == null)
            {
                Debug.LogWarning("[BetterInfoCards] Unable to locate HoverTextDrawer widget pool; skipping widget export patch.");
                return;
            }

            var drawMethod = AccessTools.Method(poolType, "Draw");
            if (drawMethod == null)
            {
                Debug.LogWarning($"[BetterInfoCards] Unable to locate Draw() on '{poolType.FullName}'; skipping widget export patch.");
                return;
            }

            if (entryType == null)
            {
                entryType = poolType.GetNestedType("Entry", BindingFlags.NonPublic | BindingFlags.Public) ??
                            FindWidgetEntryTypeRecursive(poolType);

                if (entryType == null)
                    Debug.LogWarning($"[BetterInfoCards] Unable to resolve widget entry type from '{poolType.FullName}'; attempting runtime inference.");
            }

            widgetEntryType = entryType;

            var postfix = AccessTools.Method(typeof(ExportWidgets), nameof(GetWidget_Postfix));
            if (postfix == null)
            {
                Debug.LogWarning("[BetterInfoCards] Unable to locate ExportWidgets.GetWidget_Postfix; skipping widget export patch.");
                return;
            }

            var harmony = new Harmony("BetterInfoCards.Export.ExportWidgets");
            harmony.Patch(drawMethod, postfix: new HarmonyMethod(postfix));
        }

        private static Type FindWidgetPoolType(Type hoverTextDrawerType, out Type entryType)
        {
            entryType = FindWidgetEntryType(hoverTextDrawerType);
            if (entryType == null)
                return null;

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
                        return constructed;
                }
                catch (ArgumentException)
                {
                    // Ignore incompatible generic definitions.
                }
            }

            const BindingFlags memberFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            foreach (var field in hoverTextDrawerType.GetFields(memberFlags))
            {
                var poolType = field.FieldType;
                if (IsPoolType(poolType) && HasRectTransformEntry(poolType))
                    return poolType;
            }

            foreach (var property in hoverTextDrawerType.GetProperties(memberFlags))
            {
                var poolType = property.PropertyType;
                if (IsPoolType(poolType) && HasRectTransformEntry(poolType))
                    return poolType;
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
            foreach (var nestedType in declaringType.GetNestedTypes(nestedFlags))
            {
                if (nestedType.IsValueType && !string.Equals(nestedType.Name, "Entry", StringComparison.Ordinal) && GetRectTransformMember(nestedType) != null)
                    return nestedType;

                var child = FindWidgetEntryTypeRecursive(nestedType);
                if (child != null)
                    return child;
            }

            return null;
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
                icWidgets.Clear();
            }
        }

        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginShadowBar))]
        class OnBeginShadowBar
        {
            static void Postfix()
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

            if (curICWidgets == null)
                return;

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
