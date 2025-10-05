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
        public enum PendingShadowBarState
        {
            None,
            Pending,
            Resolved
        }

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
        private const float collapseTolerance = 0.01f;
        private static bool hoverDrawerUnavailableLogged;

        public List<RectTransform> widgets = new();
        public RectTransform shadowBar;
        public RectTransform selectBorder;
        public Vector2 offset = new();
        private readonly List<RectTransform> pendingShadowBars = new();
        private readonly List<ShadowBarCandidate> pendingShadowBarCandidates = new();

        public float YMax => shadowBar != null ? shadowBar.anchoredPosition.y : 0f;
        public float YMin => YMax - Height;
        public float Width => shadowBar != null ? shadowBar.rect.width : 0f;
        public float Height => shadowBar != null ? shadowBar.rect.height : 0f;

        public void AddWidget(object entry, GameObject prefab)
        {
            if (entry == null)
                return;

            var rect = ExtractRect(entry);
            EnsureShadowBarUsable();

            var hoverTextScreen = HoverTextScreen.Instance;
            var drawer = hoverTextScreen?.drawer;

            if (drawer == null)
            {
                if (!hoverDrawerUnavailableLogged)
                {
                    hoverDrawerUnavailableLogged = true;
                    Debug.LogWarning("[BetterInfoCards] HoverTextDrawer instance is unavailable; skipping widget assignment.");
                }

                return;
            }

            var skin = drawer.skin;
            var skinShadowBar = skin?.shadowBarWidget;
            var selectBorderWidget = skin?.selectBorderWidget;

            if (TryAssignShadowBar(rect, prefab, skinShadowBar?.gameObject, skinShadowBar?.rectTransform))
                return;

            if (rect == null)
            {
                if (MatchesWidgetPrefab(prefab, skinShadowBar?.gameObject))
                {
                    if (TryResolveShadowBarFromPrefab(entry, prefab))
                        return;

                    CachePendingShadowBarCandidate(entry, prefab);
                }

                return;
            }

            if (MatchesWidgetPrefab(prefab, selectBorderWidget?.gameObject) ||
                     MatchesWidgetRect(rect, selectBorderWidget?.rectTransform))
                selectBorder = rect;
            else
                widgets.Add(rect);
        }

        public PendingShadowBarState ResolvePendingWidgets()
        {
            return ResolvePendingWidgets(scheduleDeferredChecks: true);
        }

        internal PendingShadowBarState ResolvePendingWidgets(bool scheduleDeferredChecks)
        {
            var state = UpdatePendingShadowBarState();

            if (scheduleDeferredChecks)
            {
                if (state == PendingShadowBarState.Pending)
                    DeferredShadowBarResolver.Register(this);
                else
                    DeferredShadowBarResolver.Unregister(this);
            }

            return state;
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

        private static bool MatchesWidgetRect(RectTransform candidate, RectTransform reference)
        {
            if (candidate == null || reference == null)
                return false;

            if (candidate == reference)
                return true;

            if (!string.Equals(StripCloneSuffix(candidate.name), StripCloneSuffix(reference.name), StringComparison.Ordinal))
                return false;

            if (HasMatchingComponents(candidate.gameObject, reference.gameObject))
                return true;

            return HasComponentSuperset(candidate.gameObject, reference.gameObject);
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

        private static bool HasComponentSuperset(GameObject candidate, GameObject reference)
        {
            var candidateComponents = candidate.GetComponents<Component>();
            var referenceComponents = reference.GetComponents<Component>();

            if (candidateComponents.Length < referenceComponents.Length)
                return false;

            int referenceIndex = 0;

            for (int candidateIndex = 0; candidateIndex < candidateComponents.Length && referenceIndex < referenceComponents.Length; candidateIndex++)
            {
                var candidateComponent = candidateComponents[candidateIndex];
                var referenceComponent = referenceComponents[referenceIndex];

                if (candidateComponent == null || referenceComponent == null)
                    return false;

                if (candidateComponent.GetType() == referenceComponent.GetType())
                    referenceIndex++;
            }

            return referenceIndex == referenceComponents.Length;
        }

        private bool TryAssignShadowBar(RectTransform rect, GameObject prefab, GameObject referenceObject, RectTransform referenceRect)
        {
            if (shadowBar != null && HasUsableSize(shadowBar))
                return true;

            var matchesShadowBarPrefab = MatchesWidgetPrefab(prefab, referenceObject);
            var matchesShadowBarRect = MatchesWidgetRect(rect, referenceRect);

            if (!matchesShadowBarPrefab && !matchesShadowBarRect)
                return false;

            if (rect == null)
                return shadowBar != null;

            shadowBar = rect;

            if (HasUsableSize(rect))
            {
                pendingShadowBars.Clear();
                pendingShadowBarCandidates.Clear();
                DeferredShadowBarResolver.Unregister(this);
            }
            else
            {
                CachePendingShadowBar(rect);
            }

            return true;
        }

        private void EnsureShadowBarUsable()
        {
            if (shadowBar == null)
                return;

            if (HasUsableSize(shadowBar))
                return;

            CachePendingShadowBar(shadowBar);
            shadowBar = null;
        }

        private void CachePendingShadowBar(RectTransform rect)
        {
            if (rect == null)
                return;

            if (!pendingShadowBars.Contains(rect))
            {
                pendingShadowBars.Add(rect);
                DeferredShadowBarResolver.Register(this);
            }
        }

        private void CachePendingShadowBarCandidate(object entry, GameObject prefab)
        {
            if (entry == null)
                return;

            if (pendingShadowBarCandidates.Exists(candidate => ReferenceEquals(candidate.Entry, entry)))
                return;

            pendingShadowBarCandidates.Add(new ShadowBarCandidate(entry, prefab));
            DeferredShadowBarResolver.Register(this);
        }

        private bool HasPendingShadowBarCandidates()
        {
            return pendingShadowBars.Count > 0 || pendingShadowBarCandidates.Count > 0;
        }

        private static bool HasUsableSize(RectTransform rect)
        {
            if (rect == null)
                return false;

            var size = rect.rect;
            return size.height > collapseTolerance && size.width > collapseTolerance;
        }

        private PendingShadowBarState UpdatePendingShadowBarState()
        {
            EnsureShadowBarUsable();

            if (shadowBar != null)
                return PendingShadowBarState.Resolved;

            var state = PendingShadowBarState.None;

            for (int i = pendingShadowBarCandidates.Count - 1; i >= 0; i--)
            {
                var candidate = pendingShadowBarCandidates[i];

                if (candidate.Entry == null)
                {
                    pendingShadowBarCandidates.RemoveAt(i);
                    continue;
                }

                if (TryResolveShadowBarFromPrefab(candidate.Entry, candidate.Prefab))
                {
                    pendingShadowBarCandidates.RemoveAt(i);

                    if (shadowBar != null)
                    {
                        pendingShadowBarCandidates.Clear();
                        return PendingShadowBarState.Resolved;
                    }

                    continue;
                }

                state = PendingShadowBarState.Pending;
            }

            for (int i = pendingShadowBars.Count - 1; i >= 0; i--)
            {
                var candidate = pendingShadowBars[i];
                if (HasUsableSize(candidate))
                {
                    shadowBar = candidate;
                    pendingShadowBars.Clear();
                    pendingShadowBarCandidates.Clear();
                    return PendingShadowBarState.Resolved;
                }

                if (candidate == null)
                {
                    pendingShadowBars.RemoveAt(i);
                }
                else
                {
                    state = PendingShadowBarState.Pending;
                }
            }

            return state;
        }

        private bool TryResolveShadowBarFromPrefab(object entry, GameObject prefab)
        {
            if (entry == null)
                return false;

            var rect = ExtractRect(entry);

            if (rect == null)
                rect = ResolveRectFromEntry(entry, prefab);

            if (rect == null)
                return false;

            shadowBar = rect;

            if (HasUsableSize(rect))
            {
                pendingShadowBars.Clear();
                pendingShadowBarCandidates.Clear();
                DeferredShadowBarResolver.Unregister(this);
            }
            else
            {
                CachePendingShadowBar(rect);
            }

            return true;
        }

        private static RectTransform ResolveRectFromEntry(object entry, GameObject prefab)
        {
            var componentRoot = entry as Component;
            var gameObject = componentRoot != null ? componentRoot.gameObject : entry as GameObject;

            if (gameObject == null)
                return null;

            var referenceRect = HoverTextScreen.Instance?.drawer?.skin?.shadowBarWidget?.rectTransform;

            if (referenceRect != null)
            {
                var ownRect = gameObject.GetComponent<RectTransform>();
                if (MatchesWidgetRect(ownRect, referenceRect))
                    return ownRect;

                var candidates = gameObject.GetComponentsInChildren<RectTransform>(includeInactive: true);
                foreach (var candidate in candidates)
                {
                    if (candidate == null)
                        continue;

                    if (MatchesWidgetRect(candidate, referenceRect))
                        return candidate;
                }
            }

            if (prefab != null)
            {
                var prefabRect = prefab.GetComponentInChildren<RectTransform>(includeInactive: true);
                if (prefabRect != null)
                {
                    var targetName = StripCloneSuffix(prefabRect.name);
                    var candidates = gameObject.GetComponentsInChildren<RectTransform>(includeInactive: true);
                    foreach (var candidate in candidates)
                    {
                        if (candidate == null)
                            continue;

                        if (string.Equals(StripCloneSuffix(candidate.name), targetName, StringComparison.Ordinal))
                            return candidate;
                    }
                }
            }

            return null;
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
                    if (entry is not Component component || component == null)
                        return null;

                    var skin = HoverTextScreen.Instance?.drawer?.skin;
                    var referenceRect = skin?.shadowBarWidget?.rectTransform;

                    if (referenceRect == null)
                        return null;

                    var ownRect = component.GetComponent<RectTransform>();
                    if (ownRect != null && MatchesWidgetRect(ownRect, referenceRect))
                        return ownRect;

                    var candidates = component.GetComponentsInChildren<RectTransform>(includeInactive: true);
                    foreach (var candidate in candidates)
                    {
                        if (candidate == null || candidate == ownRect)
                            continue;

                        if (MatchesWidgetRect(candidate, referenceRect))
                            return candidate;
                    }

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

        private sealed class ShadowBarCandidate
        {
            public ShadowBarCandidate(object entry, GameObject prefab)
            {
                Entry = entry;
                Prefab = prefab;
            }

            public object Entry { get; }
            public GameObject Prefab { get; }
        }

        private static class DeferredShadowBarResolver
        {
            private static readonly List<InfoCardWidgets> pendingCards = new();
            private static LateUpdateDriver driver;

            public static void Register(InfoCardWidgets card)
            {
                if (card == null)
                    return;

                if (card.shadowBar != null || !card.HasPendingShadowBarCandidates())
                {
                    Unregister(card);
                    return;
                }

                if (!pendingCards.Contains(card))
                    pendingCards.Add(card);

                EnsureDriver()?.Activate();
            }

            public static void Unregister(InfoCardWidgets card)
            {
                if (card == null)
                    return;

                if (!pendingCards.Remove(card))
                    return;

                if (pendingCards.Count == 0 && driver != null)
                    driver.enabled = false;
            }

            private static void Process()
            {
                for (int i = pendingCards.Count - 1; i >= 0; i--)
                {
                    var card = pendingCards[i];

                    if (card == null)
                    {
                        pendingCards.RemoveAt(i);
                        continue;
                    }

                    var state = card.ResolvePendingWidgets(scheduleDeferredChecks: false);

                    if (state != PendingShadowBarState.Pending)
                        pendingCards.RemoveAt(i);
                }

                if (pendingCards.Count == 0 && driver != null)
                    driver.enabled = false;
            }

            private static LateUpdateDriver EnsureDriver()
            {
                if (driver != null)
                    return driver;

                var screen = HoverTextScreen.Instance;
                if (screen == null)
                    return null;

                driver = screen.gameObject.GetComponent<LateUpdateDriver>();
                if (driver == null)
                    driver = screen.gameObject.AddComponent<LateUpdateDriver>();

                return driver;
            }

            private sealed class LateUpdateDriver : MonoBehaviour
            {
                public void Activate()
                {
                    enabled = true;
                }

                private void OnEnable()
                {
                    if (pendingCards.Count == 0)
                        enabled = false;
                }

                private void LateUpdate()
                {
                    Process();
                }
            }
        }
    }
}
