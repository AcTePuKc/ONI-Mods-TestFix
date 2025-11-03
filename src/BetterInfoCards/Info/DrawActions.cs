using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BetterInfoCards
{
    // These can not be structs with an interface.
    // The interface would cause boxing, making performance even worse.
    public abstract class DrawActions
    {
        public abstract void Draw(List<InfoCard> cards, HoverTextDrawer drawer);

        public class Text : DrawActions
        {
            TextInfo ti;
            TextStyleSetting style;
            Color color;
            bool overrideColor;

            private static HoverTextDrawer.Skin cachedSkin;
            private static TextStyleSetting cachedSkinStyle;
            private static bool loggedMissingFallbackStyle;

            private static readonly string[] skinStyleMembers = new[]
            {
                "defaultStyle",
                "defaultTextStyle",
                "stringTextStyle",
                "stringStyle",
                "bodyTextStyle",
                "textStyle",
                "basicTextStyle",
                "basicStyle"
            };

            public TextStyleSetting Style => style;

            public Text Set(TextInfo ti, TextStyleSetting style, Color color, bool overrideColor)
            {
                this.ti = ti;
                this.style = style;
                this.color = color;
                this.overrideColor = overrideColor;
                return this;
            }

            public override void Draw(List<InfoCard> cards, HoverTextDrawer drawer)
            {
                if (ti == null)
                {
                    Debug.LogWarning("[BetterInfoCards] Skipping DrawText replay because the captured TextInfo is missing.");
                    return;
                }

                var resolvedStyle = EnsureStyle(style, drawer);
                if (resolvedStyle == null)
                {
                    if (!loggedMissingFallbackStyle)
                    {
                        Debug.LogWarning("[BetterInfoCards] Skipping DrawText replay because no fallback TextStyleSetting could be resolved.");
                        loggedMissingFallbackStyle = true;
                    }
                    return;
                }

                style = resolvedStyle;
                drawer.DrawText(ti.GetTextOverride(cards), resolvedStyle, color, overrideColor);
            }

            internal static TextStyleSetting EnsureStyle(TextStyleSetting current, HoverTextDrawer drawer)
            {
                if (current != null)
                    return current;

                var resolved = ResolveFallbackStyle(drawer)
                    ?? ResolveFallbackStyle(InterceptHoverDrawer.drawerInstance)
                    ?? ResolveFallbackStyle(HoverTextScreen.Instance != null ? HoverTextScreen.Instance.drawer : null);

                return resolved;
            }

            private static TextStyleSetting ResolveFallbackStyle(HoverTextDrawer drawer)
            {
                if (drawer == null)
                    return null;

                var skin = drawer.skin;
                if (skin == null)
                    return null;

                if (cachedSkin == skin && cachedSkinStyle != null)
                    return cachedSkinStyle;

                var styleFromSkin = ExtractStyleFromSkin(skin);
                if (styleFromSkin != null)
                {
                    cachedSkin = skin;
                    cachedSkinStyle = styleFromSkin;
                }

                return styleFromSkin;
            }

            private static TextStyleSetting ExtractStyleFromSkin(object skin)
            {
                if (skin == null)
                    return null;

                var type = skin.GetType();

                foreach (var memberName in skinStyleMembers)
                {
                    var style = GetStyleFromMember(type, skin, memberName);
                    if (style != null)
                        return style;
                }

                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!typeof(TextStyleSetting).IsAssignableFrom(field.FieldType))
                        continue;

                    if (field.GetValue(skin) is TextStyleSetting fieldStyle && fieldStyle != null)
                        return fieldStyle;
                }

                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!property.CanRead || !typeof(TextStyleSetting).IsAssignableFrom(property.PropertyType))
                        continue;

                    if (property.GetValue(skin, null) is TextStyleSetting propertyStyle && propertyStyle != null)
                        return propertyStyle;
                }

                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!typeof(IEnumerable<TextStyleSetting>).IsAssignableFrom(field.FieldType))
                        continue;

                    if (field.GetValue(skin) is IEnumerable<TextStyleSetting> fieldStyles)
                    {
                        foreach (var candidate in fieldStyles)
                            if (candidate != null)
                                return candidate;
                    }
                }

                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!property.CanRead || !typeof(IEnumerable<TextStyleSetting>).IsAssignableFrom(property.PropertyType))
                        continue;

                    if (property.GetValue(skin, null) is IEnumerable<TextStyleSetting> propertyStyles)
                    {
                        foreach (var candidate in propertyStyles)
                            if (candidate != null)
                                return candidate;
                    }
                }

                return null;
            }

            private static TextStyleSetting GetStyleFromMember(System.Type type, object instance, string memberName)
            {
                var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null && typeof(TextStyleSetting).IsAssignableFrom(field.FieldType))
                {
                    if (field.GetValue(instance) is TextStyleSetting fieldStyle && fieldStyle != null)
                        return fieldStyle;
                }

                var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property?.CanRead == true && typeof(TextStyleSetting).IsAssignableFrom(property.PropertyType))
                {
                    if (property.GetValue(instance, null) is TextStyleSetting propertyStyle && propertyStyle != null)
                        return propertyStyle;
                }

                return null;
            }
        }

        public class Icon : DrawActions
        {
            Sprite icon;
            Color color;
            int imageSize;
            int horizontalSpacing;

            public Icon Set(Sprite icon, Color color, int imageSize, int horizontalSpacing)
            {
                this.icon = icon;
                this.color = color;
                this.imageSize = imageSize;
                this.horizontalSpacing = horizontalSpacing;
                return this;
            }

            public override void Draw(List<InfoCard> _, HoverTextDrawer drawer)
            {
                drawer.DrawIcon(icon, color, imageSize, horizontalSpacing);
            }
        }

        public class AddIndent : DrawActions
        {
            int width;

            public AddIndent Set(int width)
            {
                this.width = width;
                return this;
            }

            public override void Draw(List<InfoCard> _, HoverTextDrawer drawer)
            {
                drawer.AddIndent(width);
            }
        }

        public class NewLine : DrawActions
        {
            int minHeight;

            public NewLine Set(int minHeight)
            {
                this.minHeight = minHeight;
                return this;
            }

            public override void Draw(List<InfoCard> _, HoverTextDrawer drawer)
            {
                drawer.NewLine(minHeight);
            }
        }
    }
}
