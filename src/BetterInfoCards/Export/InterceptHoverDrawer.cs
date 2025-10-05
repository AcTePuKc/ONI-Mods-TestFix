using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace BetterInfoCards
{
    static class InterceptHoverDrawer
    {
        public static bool IsInterceptMode { get; set; }
        public static HoverTextDrawer drawerInstance;

        private static InfoCard curInfoCard;
        private static List<InfoCard> infoCards = new();
        private static bool loggedMissingCard;

        public static List<InfoCard> ConsumeInfoCards()
        {
            var cards = infoCards;
            infoCards = new();
            return cards;
        }

        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginDrawing))]
        public class BeginDrawing
        {
            public static System.Action onBeginDrawing;

            static void Postfix(HoverTextDrawer __instance)
            {
                drawerInstance = __instance;
                IsInterceptMode = true;
                loggedMissingCard = false;
                onBeginDrawing?.Invoke();
            }
        }

        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginShadowBar))]
        class BeginShadowBar
        {
            static ResetPool<InfoCard> pool = new(ref BeginDrawing.onBeginDrawing);

            [HarmonyPriority(Priority.First)]
            static bool Prefix(bool selected)
            {
                if (IsInterceptMode)
                    infoCards.Add(curInfoCard = pool.Get().Set(selected));
                else
                    curInfoCard = null;
                return !IsInterceptMode;
            }
        }

        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.DrawIcon), new[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int)})]
        class DrawIcon
        {
            static ResetPool<DrawActions.Icon> pool = new(ref BeginDrawing.onBeginDrawing);

            [HarmonyPriority(Priority.First)]
            static bool Prefix(Sprite icon, Color color, int image_size, int horizontal_spacing)
            {
                if (ShouldDeferToVanilla(nameof(DrawIcon)))
                    return true;

                if (curInfoCard == null)
                    return ForceVanillaFallback(nameof(DrawIcon));

                curInfoCard.AddDraw(pool.Get().Set(icon, color, image_size, horizontal_spacing));
                return false;
            }
        }

        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.DrawText), new[] { typeof(string), typeof(TextStyleSetting), typeof(Color), typeof(bool) })]
        class DrawText
        {
            static ResetPool<DrawActions.Text> pool = new(ref BeginDrawing.onBeginDrawing);

            [HarmonyPriority(Priority.First)]
            static bool Prefix(string text, TextStyleSetting style, Color color, bool override_color)
            {
                // Null check avoids crashes from drawing multiple empty strings.
                // This appears to now occur when hovering neutromium tiles.
                if (ShouldDeferToVanilla(nameof(DrawText)))
                    return true;

                if (curInfoCard == null)
                    return ForceVanillaFallback(nameof(DrawText));

                if (!text.IsNullOrWhiteSpace())
                {
                    var (id, data) = ExportSelectToolData.ConsumeTextInfo();
                    var ti = TextInfo.Create(id, text, data);
                    if (ti == null)
                    {
                        Debug.LogWarning($"[BetterInfoCards] Text converter '{id ?? "<default>"}' returned null; falling back to vanilla DrawText.");
                        // Returning true allows the vanilla drawer to render the text when our converter fails.
                        return true;
                    }

                    curInfoCard.AddDraw(pool.Get().Set(ti, style, color, override_color), ti);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.AddIndent))]
        class AddIndent
        {
            static ResetPool<DrawActions.AddIndent> pool = new(ref BeginDrawing.onBeginDrawing);

            [HarmonyPriority(Priority.First)]
            static bool Prefix(int width)
            {
                if (ShouldDeferToVanilla(nameof(AddIndent)))
                    return true;

                if (curInfoCard == null)
                    return ForceVanillaFallback(nameof(AddIndent));

                curInfoCard.AddDraw(pool.Get().Set(width));
                return false;
            }
        }

        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.NewLine))]
        class NewLine
        {
            static ResetPool<DrawActions.NewLine> pool = new(ref BeginDrawing.onBeginDrawing);

            [HarmonyPriority(Priority.First)]
            static bool Prefix(int min_height)
            {
                if (ShouldDeferToVanilla(nameof(NewLine)))
                    return true;

                if (curInfoCard == null)
                    return ForceVanillaFallback(nameof(NewLine));

                curInfoCard.AddDraw(pool.Get().Set(min_height));
                return false;
            }
        }

        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.EndShadowBar))]
        class EndShadowBar
        {
            [HarmonyPriority(Priority.First)]
            static bool Prefix()
            {
                if (ShouldDeferToVanilla(nameof(EndShadowBar)))
                    return true;

                if (curInfoCard == null)
                    return ForceVanillaFallback(nameof(EndShadowBar));

                curInfoCard.selectable = ExportSelectToolData.ConsumeSelectable();
                return false;
            }
        }

        private static bool ShouldDeferToVanilla(string caller)
        {
            if (!IsInterceptMode)
                return true;

            if (curInfoCard != null)
                return false;

            return ForceVanillaFallback(caller);
        }

        private static bool ForceVanillaFallback(string caller)
        {
            if (!loggedMissingCard)
            {
                Debug.LogWarning($"[BetterInfoCards] {caller} received without an active info card; falling back to HoverTextDrawer.");
                loggedMissingCard = true;
            }

            IsInterceptMode = false;
            curInfoCard = null;
            return true;
        }
    }
}
