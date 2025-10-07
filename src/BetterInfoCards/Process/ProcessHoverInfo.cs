using BetterInfoCards.Export;
using BetterInfoCards.Process;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace BetterInfoCards
{
    [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.EndDrawing))]
    class ProcessHoverInfo
    {
        static void Prefix()
        {
            var drawer = InterceptHoverDrawer.drawerInstance;
            if (drawer == null)
            {
                if (InterceptHoverDrawer.IsInterceptMode)
                    InterceptHoverDrawer.IsInterceptMode = false;
                Debug.LogWarning("[BetterInfoCards] HoverTextDrawer instance is missing; skipping info card replay.");
                return;
            }

            var infoCards = InterceptHoverDrawer.ConsumeInfoCards();
            var displayCards = new DisplayCards().UpdateData(infoCards);

            ModifyHits.Update(displayCards);

            InterceptHoverDrawer.IsInterceptMode = false;
            foreach (var card in displayCards)
                card.Draw(drawer);
            InterceptHoverDrawer.IsInterceptMode = true;

            var widgets = ExportWidgets.ConsumeWidgets();
            if (widgets.Count > 0)
            {
                foreach (var cardWidgets in widgets)
                {
                    var shadowBarGraphic = cardWidgets?.shadowBar?.Rect != null
                        ? cardWidgets.shadowBar.Rect.GetComponent<Graphic>()
                        : null;

                    CardTweaker.ApplyShadowBarColor(shadowBarGraphic);
                }

                var grid = new Grid(widgets, widgets[0].YMax);
                grid.MoveAndResizeInfoCards();
            }
        }
    }
}
