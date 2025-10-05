using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BetterInfoCards.Export
{
    public static class ExportWidgets
    {
        private static InfoCardWidgets curICWidgets;
        private static List<InfoCardWidgets> icWidgets = new();

        public static List<InfoCardWidgets> ConsumeWidgets()
        {
            var cardWidgets = icWidgets;
            icWidgets = new();
            return cardWidgets;
        }

        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginDrawing))]
        class OnBeginDrawing
        {
            static void Postfix()
            {
                icWidgets.Clear();
            }
        }

        [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginShadowBar))]
        class OnBeginShadowBar
        {
            static void Prefix()
            {
                if (!InterceptHoverDrawer.IsInterceptMode)
                {
                    curICWidgets = new();
                    icWidgets.Add(curICWidgets);
                }
            }
        }

        [HarmonyPatch]
        class GetWidget_Patch
        {
            static MethodBase TargetMethod()
            {
                var method = HoverTextEntryAccess.DrawMethod;
                if (method == null)
                    throw new System.MissingMethodException("[BetterInfoCards] Unable to locate HoverTextDrawer.Pool<MonoBehaviour>.Draw via reflection.");
                return method;
            }

            static void Postfix(object __result, GameObject ___prefab)
            {
                if (__result == null)
                    return;

                if (curICWidgets == null)
                {
                    if (InterceptHoverDrawer.IsInterceptMode)
                        return;

                    curICWidgets = new();
                    icWidgets.Add(curICWidgets);
                }

                var rect = HoverTextEntryAccess.GetRect(__result);
                curICWidgets.AddWidget(__result, rect, ___prefab);
            }
        }
    }
}
