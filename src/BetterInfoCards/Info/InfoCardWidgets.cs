using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BetterInfoCards
{
    public sealed class InfoCardWidgetHandle
    {
        public InfoCardWidgetHandle(object entry, RectTransform rect)
        {
            Entry = entry;
            Rect = rect;
        }

        public object Entry { get; }
        public RectTransform Rect { get; }
    }

    internal static class HoverTextEntryAccess
    {
        public static readonly System.Type PoolType = AccessTools.Inner(typeof(HoverTextDrawer), "Pool`1")?.MakeGenericType(typeof(MonoBehaviour));
        public static readonly System.Type EntryType = PoolType != null ? AccessTools.Inner(PoolType, "Entry") : null;

        private static readonly Dictionary<System.Type, FieldInfo> rectFieldCache = new();
        private static readonly object rectFieldCacheLock = new();
        private static readonly MethodInfo drawMethod = PoolType != null ? AccessTools.Method(PoolType, "Draw") : null;
        private static readonly MemberInfo shadowBarPoolMember = PoolType != null ? FindShadowBarPoolMember() : null;

        public static MethodInfo DrawMethod => drawMethod;
        public static MemberInfo ShadowBarPoolMember => shadowBarPoolMember;

        private static MemberInfo FindShadowBarPoolMember()
        {
            var type = typeof(HoverTextDrawer);
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var field in type.GetFields(flags))
                if (field.FieldType == PoolType)
                    return field;

            foreach (var property in type.GetProperties(flags))
                if (property.PropertyType == PoolType && property.GetIndexParameters().Length == 0)
                    return property;

            return null;
        }

        public static object GetShadowBarPool(HoverTextDrawer drawer)
        {
            if (drawer == null || shadowBarPoolMember == null)
                return null;

            if (shadowBarPoolMember is FieldInfo field)
                return field.GetValue(drawer);

            if (shadowBarPoolMember is PropertyInfo property)
                return property.GetValue(drawer);

            return null;
        }

        public static RectTransform GetRect(object entry)
        {
            if (entry == null)
                return null;

            var entryType = entry.GetType();

            FieldInfo rectField;
            lock (rectFieldCacheLock)
            {
                if (!rectFieldCache.TryGetValue(entryType, out rectField))
                {
                    rectField = AccessTools.Field(entryType, "rect");
                    rectFieldCache[entryType] = rectField;
                }
            }

            if (rectField != null && rectField.GetValue(entry) is RectTransform rect)
                return rect;

            if (rectField != null)
                return null;

            var traverse = Traverse.Create(entry);
            return traverse?.Property("rect")?.GetValue<RectTransform>();
        }
    }

    public class InfoCardWidgets
    {
        public List<InfoCardWidgetHandle> widgets = new();
        public InfoCardWidgetHandle shadowBar;
        public InfoCardWidgetHandle selectBorder;
        public Vector2 offset = new();

        public float YMax => shadowBar?.Rect != null ? shadowBar.Rect.anchoredPosition.y : 0f;
        public float YMin => YMax - Height;
        public float Width => shadowBar?.Rect != null ? shadowBar.Rect.rect.width : 0f;
        public float Height => shadowBar?.Rect != null ? shadowBar.Rect.rect.height : 0f;

        public void AddWidget(object entry, RectTransform rect, GameObject prefab)
        {
            var skin = HoverTextScreen.Instance.drawer.skin;

            rect ??= HoverTextEntryAccess.GetRect(entry);
            var handle = new InfoCardWidgetHandle(entry, rect);

            if (prefab == skin.shadowBarWidget.gameObject)
                shadowBar = handle;
            else if (prefab == skin.selectBorderWidget.gameObject)
                selectBorder = handle;
            else
                widgets.Add(handle);
        }

        public void Translate(float x)
        {
            var shift = new Vector2(x, offset.y);

            if (shadowBar?.Rect != null)
                shadowBar.Rect.anchoredPosition += shift;

            if (selectBorder?.Rect != null)
                selectBorder.Rect.anchoredPosition += shift;

            foreach (var widget in widgets)
                if (widget.Rect != null)
                    widget.Rect.anchoredPosition += shift;
        }

        public void SetWidth(float width)
        {
            if (shadowBar?.Rect == null)
                return;

            // Modifying existing SBs triggers rebuilds somewhere and has a major impact on performance.
            // Genius idea from Peter to just add new ones to fill the gap.
            var rect = shadowBar.Rect;
            var drawer = InterceptHoverDrawer.drawerInstance;
            var pool = HoverTextEntryAccess.GetShadowBarPool(drawer);
            if (pool == null)
            {
                Debug.LogWarning("[BetterInfoCards] Unable to access the HoverTextDrawer shadow bar pool; skipping width resize.");
                return;
            }

            var drawMethod = HoverTextEntryAccess.DrawMethod;
            if (drawMethod == null)
            {
                Debug.LogWarning("[BetterInfoCards] Unable to locate the shadow bar pool Draw() method; skipping width resize.");
                return;
            }

            var newSB = drawMethod.Invoke(pool, new object[] { rect.anchoredPosition + new Vector2(rect.sizeDelta.x, 0f) });
            var newRect = HoverTextEntryAccess.GetRect(newSB) ?? rect;
            newRect.sizeDelta = new Vector2(width - rect.sizeDelta.x, rect.sizeDelta.y);

            if (selectBorder?.Rect != null)
                selectBorder.Rect.sizeDelta = new Vector2(width + 2f, selectBorder.Rect.sizeDelta.y);
        }
    }
}
