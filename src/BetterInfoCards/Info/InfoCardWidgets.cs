using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

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
        private static readonly System.Type skinType = AccessTools.Inner(typeof(HoverTextDrawer), "Skin") ?? AccessTools.TypeByName("HoverTextDrawer+Skin");
        public static readonly System.Type PoolType = skinType != null ? AccessTools.Inner(skinType, "Pool`1")?.MakeGenericType(typeof(Image)) : null;
        public static readonly System.Type EntryType = PoolType != null ? AccessTools.Inner(PoolType, "Entry") : null;

        private static readonly Dictionary<System.Type, FieldInfo> rectFieldCache = new();
        private static readonly object rectFieldCacheLock = new();
        private static readonly MethodInfo drawMethod = PoolType?.GetMethod("Draw", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(Vector2) }, null);
        private static readonly string[] shadowPoolFieldNames = { "shadowBars", "shadowBarPool", "m_ShadowBars" };
        private static volatile FieldInfo shadowPoolField;
        private static volatile bool shadowPoolUnavailable;
        private static readonly object _shadowPoolLock = new();
        private static float lastShadowPoolWarningTime;
        private static readonly object _warningLock = new();

        public static MethodInfo DrawMethod => drawMethod;

        public static bool TryGetShadowPool(HoverTextDrawer drawer, out object pool)
        {
            pool = null;

            if (drawer == null || PoolType == null)
            {
                WarnShadowPoolFailure();
                return false;
            }

            if (!shadowPoolUnavailable && shadowPoolField == null)
            {
                lock (_shadowPoolLock)
                {
                    if (!shadowPoolUnavailable && shadowPoolField == null)
                    {
                        var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                        var type = typeof(HoverTextDrawer);

                        foreach (var name in shadowPoolFieldNames)
                        {
                            var field = type.GetField(name, flags);
                            if (field != null && field.FieldType == PoolType)
                            {
                                shadowPoolField = field;
                                break;
                            }
                        }

                        if (shadowPoolField == null)
                            shadowPoolUnavailable = true;
                    }
                }
            }

            if (shadowPoolUnavailable)
            {
                WarnShadowPoolFailure();
                return false;
            }

            pool = shadowPoolField.GetValue(drawer);
            if (pool == null)
            {
                WarnShadowPoolFailure();
                return false;
            }

            return true;

            void WarnShadowPoolFailure()
            {
                if (WarnOncePerSecond(ref lastShadowPoolWarningTime))
                    Debug.LogWarning("[BetterInfoCards] Unable to access the HoverTextDrawer shadow bar pool; skipping width resize.");
            }
        }

        private static bool WarnOncePerSecond(ref float lastWarningTime)
        {
            lock (_warningLock)
            {
                var time = Time.unscaledTime;
                if (time - lastWarningTime >= 1f)
                {
                    lastWarningTime = time;
                    return true;
                }
            }

            return false;
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
            if (!HoverTextEntryAccess.TryGetShadowPool(drawer, out var pool))
                return;

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
