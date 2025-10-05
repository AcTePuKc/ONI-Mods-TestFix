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

        private static readonly FieldInfo rectField = EntryType != null ? AccessTools.Field(EntryType, "rect") : null;
        private static readonly MethodInfo drawMethod = PoolType != null ? AccessTools.Method(PoolType, "Draw") : null;

        public static MethodInfo DrawMethod => drawMethod;

        public static RectTransform GetRect(object entry)
        {
            if (entry == null)
                return null;

            if (rectField != null && rectField.GetValue(entry) is RectTransform rect)
                return rect;

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
            var newSB = InterceptHoverDrawer.drawerInstance.shadowBars.Draw(rect.anchoredPosition + new Vector2(rect.sizeDelta.x, 0f));
            var newRect = HoverTextEntryAccess.GetRect(newSB) ?? rect;
            newRect.sizeDelta = new Vector2(width - rect.sizeDelta.x, rect.sizeDelta.y);

            if (selectBorder?.Rect != null)
                selectBorder.Rect.sizeDelta = new Vector2(width + 2f, selectBorder.Rect.sizeDelta.y);
        }
    }
}
