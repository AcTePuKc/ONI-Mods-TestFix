﻿using System;
using System.Collections.Generic;
using System.Linq;
using STRINGS;
using UnityEngine;

namespace BetterInfoCards
{
    public static class ConverterManager
    {
        public const string title = "Title";
        public const string germs = "Germs";
        public const string temp = "Temp";

        public const string sumSuffix = " <color=#ababab>(Σ)</color>";
        public const string avgSuffix = " <color=#ababab>(μ)</color>";

        private static readonly Dictionary<string, Func<string, string, object, TextInfo>> converters = new Dictionary<string, Func<string, string, object, TextInfo>>();
        private static Func<string, string, object, TextInfo> defaultConverter;
        private static Func<string, string, object, TextInfo> titleConverter;
        private static bool hasLoggedInvalidDiseaseIndex;
        private static bool hasLoggedMissingPrimaryElementForTitle;

        static ConverterManager()
        {
            // DEFAULT
            AddConverter<object>(
                string.Empty,
                data => null,
                null,
                null);

            // TITLE
            AddConverter(
                title,
                data => {
                    GameObject go = data as GameObject;
                    KPrefabID prefabID = go?.GetComponent<KPrefabID>();
                    if (prefabID != null && Assets.IsTagCountable(prefabID.PrefabTag))
                    {
                        var primaryElement = go?.GetComponent<PrimaryElement>();
                        if (primaryElement == null)
                        {
                            LogMissingPrimaryElementOnce(ref hasLoggedMissingPrimaryElementForTitle, go, title);
                            return 1f;
                        }

                        return primaryElement.Units;
                    }
                    return 1f;
                },
                (original, counts) => original.RemoveCountSuffix() + " x " + counts.Sum());

            // GERMS
            AddConverter<(byte idx, int count)>(
                germs,
                data => {
                    try {
                    PrimaryElement element = ((GameObject)data).GetComponent<PrimaryElement>();
                    return (element.DiseaseIdx, element.DiseaseCount);
                    }
                    catch (NullReferenceException)
                    {
                        Debug.Log("Issue encountered in germs converter (getValue)");
                        Debug.Log("Data: " + data);
                        Debug.Log("GameObject: " + ((GameObject)data) + "; " + ((GameObject)data)?.name);

                        var element = ((GameObject)data)?.GetComponent<PrimaryElement>();
                        Debug.Log("Element: " + element);

                        Debug.Log("Idx: " + element?.DiseaseIdx + "; Count: " + element?.DiseaseCount);

                        Debug.LogError("Hi, you've hit an edge case crash in Better Info Cards.\n" +
                            "PLEASE upload the full player.log to the below issue so I can pin it down.\n" +
                            "https://github.com/AzeTheGreat/ONI-Mods/issues/33\n" +
                            "--------------------------------------------------");
                        throw;
                    }
                },
                // Impossible for multiple storages to overlap, so no need to worry about that part of the germ text since it will never be overwritten
                (original, pairs) => {
                    string text = UI.OVERLAYS.DISEASE.NO_DISEASE;
                    var diseaseIdx = pairs[0].idx;
                    if (diseaseIdx == byte.MaxValue || diseaseIdx >= Db.Get().Diseases.Count)
                    {
                        if (!hasLoggedInvalidDiseaseIndex)
                        {
                            hasLoggedInvalidDiseaseIndex = true;
                            Debug.LogWarning("[BetterInfoCards] Ignoring invalid disease index in germs converter.");
                        }
                    }
                    else
                    {
                        text = GameUtil.GetFormattedDisease(diseaseIdx, pairs.Sum(x => x.count), true) + sumSuffix;
                    }
                    return text;
                },
                null /* caller must supply proper splitListDefs when required */);

            // TEMP
            AddConverter(
                temp,
                data => ((GameObject)data).GetComponent<PrimaryElement>().Temperature,
                (original, temps) => GameUtil.GetFormattedTemperature(temps.Average()) + avgSuffix,
                null /* caller must supply proper splitListDefs when required */);
        }

        public static void AddConverter<T>(string name, Func<object, T> getValue, Func<string, List<T>, string> getTextOverride = null, List<(Func<T, float>, float)> splitListDefs = null) where T : new()
        {
            // Pass the same delegate field reference type that ResetPool expects.
            // InterceptHoverDrawer.BeginDrawing.onBeginDrawing is declared as System.Action in that file,
            // so pass it directly (no aliasing) to avoid type-alias collisions.
            var pool = new ResetPool<TextInfo<T>>(ref InterceptHoverDrawer.BeginDrawing.onBeginDrawing);
            Func<string, string, object, TextInfo> factory = (string k, string n, object d) =>
                pool.Get().Set(k, n, d, getValue, getTextOverride, splitListDefs);

            if (name == string.Empty)
            {
                if (defaultConverter is not null)
                    throw new Exception("Attempted to add default converter, but one is already registered.");

                defaultConverter = factory;
                return;
            }

            if (name == title)
            {
                if (titleConverter is not null)
                    throw new Exception("Attempted to add title converter, but one is already registered.");

                titleConverter = factory;
                return;
            }

            if (converters.ContainsKey(name))
                throw new Exception("Attempted to add converter with name: " + name + ", but converter with name is already present.");

            converters.Add(name, factory);
        }

        // This is not to be used internally - for reflection from external mods only.
        private static void AddConverterReflect(string name, object getValue, object getTextOverride, object splitListDefs)
        {
            var type = getValue.GetType().GetGenericArguments()[1];
            var method = typeof(ConverterManager).GetMethod(nameof(AddConverter)).MakeGenericMethod(type);
            method.Invoke(null, new object[] { name, getValue, getTextOverride, splitListDefs });
        }

        public static bool TryGetConverter(string id, out Func<string, string, object, TextInfo> converter)
        {
            if (id == string.Empty)
            {
                converter = defaultConverter;
                return false;
            }

            if (id == title)
            {
                converter = titleConverter ?? defaultConverter;
                return titleConverter is not null;
            }

            if (converters.TryGetValue(id, out converter))
                return true;

            converter = defaultConverter;
            return false;
        }

        private static void LogMissingPrimaryElementOnce(ref bool hasLogged, GameObject go, string converterId)
        {
            if (hasLogged)
                return;

            hasLogged = true;
            string objectName = go != null ? go.name : "<null>";
            Debug.LogWarning(string.Format("[BetterInfoCards] Missing PrimaryElement for converter '{0}' on object '{1}'. Using a safe default.", converterId, objectName));
        }
    }
}
