using System;
using System.Collections.Generic;
using System.Reflection;
using BetterInfoCards.Export;
using UnityEngine;
using UnityEngine.UI;

namespace BetterInfoCards
{
    public class InfoCardWidgets
    {
        private static readonly Dictionary<Type, Func<object, RectTransform>> rectAccessors = new();
        private static readonly object rectAccessorLock = new();
        private const BindingFlags memberBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly FieldInfo shadowBarsField = typeof(HoverTextDrawer).GetField("shadowBars", memberBindingFlags);
        private static readonly MethodInfo shadowBarsDrawMethod = shadowBarsField?.FieldType.GetMethod(
            "Draw",
            memberBindingFlags,
            binder: null,
            types: new[] { typeof(Vector2) },
            modifiers: null);

        public List<RectTransform> widgets = new();
        public RectTransform shadowBar;
        public RectTransform selectBorder;
        public Vector2 offset = new();

        public float YMax => shadowBar != null ? shadowBar.anchoredPosition.y : 0f;
        public float YMin => YMax - Height;
        public float Width => shadowBar != null ? shadowBar.rect.width : 0f;
        public float Height => shadowBar != null ? shadowBar.rect.height : 0f;

        public void AddWidget(object entry, GameObject prefab)
        {
            if (entry == null)
                return;

            var rect = ExtractRect(entry);
            if (rect == null)
                return;

            var skin = HoverTextScreen.Instance.drawer.skin;

            if (MatchesWidgetPrefab(prefab, skin.shadowBarWidget?.gameObject))
                shadowBar = rect;
            else if (MatchesWidgetPrefab(prefab, skin.selectBorderWidget?.gameObject))
                selectBorder = rect;
            else
                widgets.Add(rect);
        }

        private static bool MatchesWidgetPrefab(GameObject candidate, GameObject reference)
        {
            if (candidate == null || reference == null)
                return false;

            if (candidate == reference)
                return true;

            if (string.Equals(StripCloneSuffix(candidate.name), StripCloneSuffix(reference.name), StringComparison.Ordinal))
                return true;

            if (!HasMatchingComponents(candidate, reference))
                return false;

            var candidateRect = candidate.GetComponent<RectTransform>();
            var referenceRect = reference.GetComponent<RectTransform>();

            if (candidateRect == null || referenceRect == null)
                return false;

            return candidateRect.rect.size == referenceRect.rect.size;
        }

        private static string StripCloneSuffix(string name)
        {
            const string cloneSuffix = "(Clone)";

            if (string.IsNullOrEmpty(name) || !name.EndsWith(cloneSuffix, StringComparison.Ordinal))
                return name;

            return name.Substring(0, name.Length - cloneSuffix.Length);
        }

        private static bool HasMatchingComponents(GameObject candidate, GameObject reference)
        {
            var candidateComponents = candidate.GetComponents<Component>();
            var referenceComponents = reference.GetComponents<Component>();

            if (candidateComponents.Length != referenceComponents.Length)
                return false;

            for (int i = 0; i < candidateComponents.Length; i++)
            {
                var candidateComponent = candidateComponents[i];
                var referenceComponent = referenceComponents[i];

                if (candidateComponent == null || referenceComponent == null)
                    return false;

                if (candidateComponent.GetType() != referenceComponent.GetType())
                    return false;
            }

            return true;
        }

        public void Translate(float x)
        {
            var shift = new Vector2(x, offset.y);

            if (shadowBar != null)
                shadowBar.anchoredPosition += shift;

            if (selectBorder != null)
                selectBorder.anchoredPosition += shift;

            foreach (var widget in widgets)
                if (widget != null)
                    widget.anchoredPosition += shift;
        }

        public void SetWidth(float width)
        {
            if (shadowBar == null)
                return;

            // Modifying existing SBs triggers rebuilds somewhere and has a major impact on performance.
            // Genius idea from Peter to just add new ones to fill the gap.
            Vector2 newShadowBarPosition = shadowBar.anchoredPosition + new Vector2(shadowBar.sizeDelta.x, 0f);
            var drawer = InterceptHoverDrawer.drawerInstance;
            if (drawer == null)
                return;

            if (shadowBarsField == null)
            {
                Debug.LogWarning("[BetterInfoCards] Unable to locate HoverTextDrawer.shadowBars field.");
                return;
            }

            if (shadowBarsDrawMethod == null)
            {
                Debug.LogWarning("[BetterInfoCards] Unable to locate Draw(Vector2) on HoverTextDrawer.shadowBars.");
                return;
            }

            var shadowBars = shadowBarsField.GetValue(drawer);
            if (shadowBars == null)
            {
                Debug.LogWarning("[BetterInfoCards] HoverTextDrawer.shadowBars instance is null.");
                return;
            }

            var newShadowBar = ExtractRect(shadowBarsDrawMethod.Invoke(shadowBars, new object[] { newShadowBarPosition }));
            if (newShadowBar != null)
            {
                newShadowBar.sizeDelta = new Vector2(width - shadowBar.sizeDelta.x, shadowBar.sizeDelta.y);
                var graphic = newShadowBar.GetComponent<Graphic>();
                if (graphic != null)
                    CardTweaker.ApplyShadowBarColor(graphic);
            }

            if (selectBorder != null)
                selectBorder.sizeDelta = new Vector2(width + 2f, selectBorder.sizeDelta.y);
        }

        private static RectTransform ExtractRect(object entry)
        {
            var type = entry.GetType();

            if (!rectAccessors.TryGetValue(type, out var accessor))
            {
                lock (rectAccessorLock)
                {
                    if (!rectAccessors.TryGetValue(type, out accessor))
                    {
                        accessor = CreateAccessor(type);
                        rectAccessors[type] = accessor;
                    }
                }
            }

            return accessor(entry);
        }

        private static Func<object, RectTransform> CreateAccessor(Type type)
        {
            if (typeof(RectTransform).IsAssignableFrom(type))
                return entry => entry as RectTransform;

            if (TryCreateAccessorFromMember(ExportWidgets.GetRectTransformMember(type), out var accessor))
                return accessor;

            if (TryCreateAccessorFromMember(type.GetField("rect", memberBindingFlags), out accessor))
                return accessor;

            if (TryCreateAccessorFromMember(type.GetField("rectTransform", memberBindingFlags), out accessor))
                return accessor;

            if (TryCreateAccessorFromMember(type.GetProperty("rect", memberBindingFlags), out accessor))
                return accessor;

            if (TryCreateAccessorFromMember(type.GetProperty("rectTransform", memberBindingFlags), out accessor))
                return accessor;

            if (typeof(Component).IsAssignableFrom(type))
                return entry =>
                {
                    if (entry is Component component)
                        return component != null ? component.GetComponent<RectTransform>() : null;

                    return null;
                };

            return _ => null;
        }

        private static bool TryCreateAccessorFromMember(MemberInfo member, out Func<object, RectTransform> accessor)
        {
            accessor = null;

            switch (member)
            {
                case FieldInfo field when typeof(RectTransform).IsAssignableFrom(field.FieldType):
                    accessor = entry =>
                    {
                        if (entry == null)
                            return null;

                        return field.GetValue(entry) as RectTransform;
                    };
                    return true;

                case PropertyInfo property when IsRectTransformProperty(property):
                    var getter = property.GetGetMethod(true);
                    if (getter == null)
                        return false;

                    accessor = entry =>
                    {
                        if (entry == null)
                            return null;

                        return getter.Invoke(entry, null) as RectTransform;
                    };
                    return true;
            }

            return false;
        }

        private static bool IsRectTransformProperty(PropertyInfo property)
        {
            if (property == null)
                return false;

            if (property.GetIndexParameters().Length > 0)
                return false;

            if (!typeof(RectTransform).IsAssignableFrom(property.PropertyType))
                return false;

            return true;
        }
    }
}
