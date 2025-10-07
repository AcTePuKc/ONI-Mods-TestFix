﻿using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace BetterInfoCards
{
    // Using patches seems unintuitive here - the calls are being intercepted, why not just modify the values from where they're manually invoked?
    // The issue is that not all calls are intercepted.  For example: EndShadowBar has a NewLine call in it, and that inner call isn't.
    // By patching the original method, it is ensured that the tweaks will always apply.
    // To help prevent unnecessary calls from being made, tweaks should be skippable by false prefixes (use ref parameters).
    public static class CardTweaker
    {
        static bool ShouldTweak => Options.Opts.InfoCardSize.ShouldOverride;

        [HarmonyPatch(typeof(HoverTextDrawer), MethodType.Constructor, new[] { typeof(HoverTextDrawer.Skin), typeof(RectTransform) })]
        class TweakShadowBarPrefab
        {
            // It is unclear where this magic number "+2" came from.
            private static readonly Vector2 border = new(Options.Opts.InfoCardSize.YPadding + 2, Options.Opts.InfoCardSize.YPadding);

            static void Prefix(ref HoverTextDrawer.Skin skin)
            {
                if (ShouldTweak)
                    skin.shadowBarBorder = border;
            }
        }

        // There's no easy way to modify this in setup.
        // HoverTextConfigurations are spawned multiple times, break reference equality, yet preserve values.
        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.DrawText), new[] { typeof(string), typeof(TextStyleSetting), typeof(Color), typeof(bool) })]
        class TweakFontSize 
        {
            private static readonly int fontSizeChange = Options.Opts.InfoCardSize.FontSizeChange;
            static bool Prepare() => ShouldTweak;

            // Because this prefix can be skipped, state must be used to tell it when to undo the size change.
            static void Prefix(ref TextStyleSetting style, out bool __state)
            {
                __state = false;
                if (style)
                {
                    style.fontSize += fontSizeChange;
                    __state = true;
                } 
            }

            // Unfortunately, this will be called extraneously: once for each intercepted call, once for each real call.
            static void Postfix(ref TextStyleSetting style, bool __state)
            {
                if (__state)
                    style.fontSize -= fontSizeChange;
            }
        }

        // Instead of a min line height, switch to a fixed line spacing.
        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.NewLine))]
        class TweakNewLineHeight
        {
            private static readonly int lineSpacing = Options.Opts.InfoCardSize.LineSpacing;
            static bool Prepare() => ShouldTweak;

            static void Prefix(HoverTextDrawer __instance, ref int min_height)
            {
                min_height = 0;
                DrawPositionAccessor.AdjustY(__instance, -lineSpacing);
            }
        }

        // EndShadowBar calls NewLine to encompass the previously drawn text.
        // This removes the excess line spacing that is added due to that.
        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.EndShadowBar))]
        class FixupLineHeight
        {
            private static readonly int lineSpacing = Options.Opts.InfoCardSize.LineSpacing;
            static bool Prepare() => ShouldTweak;

            static void Prefix(HoverTextDrawer __instance)
            {
                if (!InterceptHoverDrawer.IsInterceptMode)
                    DrawPositionAccessor.AdjustY(__instance, lineSpacing);
            }
        }

        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.DrawIcon), new[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) })]
        class TweakIconSize
        {
            private static readonly int iconSizeChange = Options.Opts.InfoCardSize.IconSizeChange;
            static bool Prepare() => ShouldTweak;

            static void Prefix(ref int image_size) => image_size += iconSizeChange;
        }

        internal static Color GetShadowBarColor()
        {
            var background = Options.Opts.InfoCardBackgroundColor;

            return new Color(
                background.r / 255f,
                background.g / 255f,
                background.b / 255f,
                Options.Opts.InfoCardOpacity / 100f);
        }

        internal static void ApplyShadowBarColor(Graphic graphic)
        {
            if (graphic == null)
                return;

            var tint = GetShadowBarColor();
            graphic.color = tint;

            var colorStyleField = AccessTools.Field(graphic.GetType(), "colorStyleSetting");
            var colorStyleProperty = colorStyleField == null
                ? AccessTools.Property(graphic.GetType(), "colorStyleSetting")
                : null;

            object style = colorStyleField != null
                ? colorStyleField.GetValue(graphic)
                : colorStyleProperty?.CanRead == true
                    ? colorStyleProperty.GetValue(graphic)
                    : null;

            if (style == null)
                return;

            var styleType = style.GetType();
            var inactiveField = AccessTools.Field(styleType, "inactiveColor");
            var activeField = AccessTools.Field(styleType, "activeColor");
            var inactiveProperty = inactiveField == null ? AccessTools.Property(styleType, "inactiveColor") : null;
            var activeProperty = activeField == null ? AccessTools.Property(styleType, "activeColor") : null;

            if (inactiveField != null)
                inactiveField.SetValue(style, tint);
            else if (inactiveProperty?.CanWrite == true)
                inactiveProperty.SetValue(style, tint, null);

            if (activeField != null)
                activeField.SetValue(style, tint);
            else if (activeProperty?.CanWrite == true)
                activeProperty.SetValue(style, tint, null);

            if (colorStyleField != null)
                colorStyleField.SetValue(graphic, style);
            else if (colorStyleProperty?.CanWrite == true)
                colorStyleProperty.SetValue(graphic, style, null);
        }

        private static class DrawPositionAccessor
        {
            private static readonly FieldInfo currentPosField = AccessTools.Field(typeof(HoverTextDrawer), "currentPos");
            private static readonly FieldInfo drawStateField = AccessTools.Field(typeof(HoverTextDrawer), "drawState");
            private static readonly FieldInfo drawStateCurrentPositionField = drawStateField != null
                ? AccessTools.Field(drawStateField.FieldType, "currentPosition")
                : null;

            private static Vector2 GetCurrentPosition(HoverTextDrawer instance)
            {
                if (drawStateField != null && drawStateCurrentPositionField != null)
                {
                    object drawState = drawStateField.GetValue(instance);
                    if (drawState != null)
                    {
                        return (Vector2)drawStateCurrentPositionField.GetValue(drawState);
                    }
                }

                if (currentPosField != null)
                    return (Vector2)currentPosField.GetValue(instance);

                throw new MissingFieldException("HoverTextDrawer", "current draw position");
            }

            private static void SetCurrentPosition(HoverTextDrawer instance, Vector2 value)
            {
                if (drawStateField != null && drawStateCurrentPositionField != null)
                {
                    object drawState = drawStateField.GetValue(instance);
                    if (drawState != null)
                    {
                        drawStateCurrentPositionField.SetValue(drawState, value);
                        drawStateField.SetValue(instance, drawState);
                        return;
                    }
                }

                if (currentPosField != null)
                {
                    currentPosField.SetValue(instance, value);
                    return;
                }

                throw new MissingFieldException("HoverTextDrawer", "current draw position");
            }

            public static void AdjustY(HoverTextDrawer instance, float delta)
            {
                Vector2 position = GetCurrentPosition(instance);
                position.y += delta;
                SetCurrentPosition(instance, position);
            }
        }
    }
}
