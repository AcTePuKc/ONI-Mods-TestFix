﻿using System.Collections.Generic;
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

                if (style == null)
                {
                    Debug.LogWarning("[BetterInfoCards] Skipping DrawText replay because the captured TextStyleSetting is missing.");
                    return;
                }

                drawer.DrawText(ti.GetTextOverride(cards), style, color, overrideColor);
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
