using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BadMod.ContainerTooltips.Configuration;
using BadMod.ContainerTooltips.Enums;
using UnityEngine;

namespace BadMod.ContainerTooltips.Storage;

internal static class StorageContentsSummarizer
{
    private sealed class ContentSummary
    {
        private Dictionary<byte, int>? diseases;
        private ContentSummaryCollection? children;

        internal ContentSummary(string name, Tag tag, int depth)
        {
            Name = name;
            Tag = tag;
            Depth = depth;
        }

        internal string Name { get; }

        internal Tag Tag { get; }

        internal int Depth { get; }

        internal int Count { get; set; }

        internal float Mass { get; set; }

        internal float Units { get; set; }

        internal float Calories { get; set; }

        internal float TemperatureSum { get; set; }

        internal int TemperatureSamples { get; set; }

        internal int TotalDiseaseCount { get; private set; }

        internal IReadOnlyDictionary<byte, int>? Diseases => diseases;

        internal ContentSummaryCollection? Children => children;

        internal ContentSummaryCollection EnsureChildren()
        {
            return children ??= new ContentSummaryCollection(Depth + 1);
        }

        internal Dictionary<byte, int> EnsureDiseases()
        {
            return diseases ??= new Dictionary<byte, int>();
        }

        internal void AddDisease(byte diseaseIdx, int amount)
        {
            var values = EnsureDiseases();
            values.TryGetValue(diseaseIdx, out var existing);
            values[diseaseIdx] = existing + amount;
            TotalDiseaseCount += amount;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            AppendTo(builder, this, 0);
            return builder.ToString();
        }

        private static void AppendTo(StringBuilder builder, ContentSummary summary, int indentLevel)
        {
            var indent = new string(' ', indentLevel * 2);
            builder.Append(indent).Append("ContentSummary {");
            builder.Append(" Name=\"").Append(summary.Name).Append('\"');
            builder.Append(", Tag=").Append(summary.Tag);
            builder.Append(", Depth=").Append(summary.Depth);
            builder.Append(", Count=").Append(summary.Count);
            builder.Append(", Mass=").Append(summary.Mass);
            builder.Append(", Units=").Append(summary.Units);
            builder.Append(", Calories=").Append(summary.Calories);
            builder.Append(", TemperatureSum=").Append(summary.TemperatureSum);
            builder.Append(", TemperatureSamples=").Append(summary.TemperatureSamples);
            builder.Append(", TotalDiseaseCount=").Append(summary.TotalDiseaseCount);
            builder.Append(", Diseases=");

            if (summary.diseases == null || summary.diseases.Count == 0)
            {
                builder.Append("[]");
            }
            else
            {
                builder.Append('[');
                var first = true;
                foreach (var disease in summary.diseases)
                {
                    if (!first)
                        builder.Append(", ");

                    builder.Append(disease.Key).Append(':').Append(disease.Value);
                    first = false;
                }

                builder.Append(']');
            }

            builder.Append(", Children=");
            if (summary.children == null || summary.children.Count == 0)
            {
                builder.Append("[]");
            }
            else
            {
                builder.AppendLine("[");
                var items = summary.children.Items;
                for (var i = 0; i < items.Count; i++)
                {
                    AppendTo(builder, items[i], indentLevel + 1);
                    builder.AppendLine(i < items.Count - 1 ? "," : string.Empty);
                }

                builder.Append(indent).Append(']');
            }

            builder.Append(" }");
        }
    }

    private sealed class ContentSummaryCollection
    {
        private readonly int depth;
        private readonly Dictionary<Tag, ContentSummary> lookup = new();
        private readonly List<ContentSummary> items = new();

        internal ContentSummaryCollection(int depth)
        {
            this.depth = depth;
        }

        internal IReadOnlyList<ContentSummary> Items => items;

        internal int Count => items.Count;

        internal int Depth => depth;

        internal ContentSummary GetOrAdd(Tag tag, string name)
        {
            if (!lookup.TryGetValue(tag, out var summary))
            {
                summary = new ContentSummary(name, tag, depth);
                lookup.Add(tag, summary);
                items.Add(summary);
            }

            return summary;
        }

        internal void Sort(ContentSortMode sortMode)
        {
            if (sortMode == ContentSortMode.Default)
                return;

            items.Sort(new ContentSummaryComparer(sortMode));
            foreach (var item in items)
                item.Children?.Sort(sortMode);
        }
    }

    private sealed class ContentSummaryComparer : IComparer<ContentSummary>
    {
        private readonly ContentSortMode mode;

        internal ContentSummaryComparer(ContentSortMode mode)
        {
            this.mode = mode;
        }

        public int Compare(ContentSummary? x, ContentSummary? y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            if (x is null)
                return -1;
            if (y is null)
                return 1;

            return mode switch
            {
                ContentSortMode.Alphabetical => CompareAlphabetical(x, y),
                ContentSortMode.Amount => CompareAmount(x, y),
                _ => 0
            };
        }

        private static int CompareAlphabetical(ContentSummary x, ContentSummary y)
        {
            var value = string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
            return value == 0 ? CompareAmount(x, y) : value;
        }

        private static int CompareAmount(ContentSummary x, ContentSummary y)
        {
            var value = y.Mass.CompareTo(x.Mass);
            if (value != 0)
                return value;

            value = y.Calories.CompareTo(x.Calories);
            if (value != 0)
                return value;

            value = y.Units.CompareTo(x.Units);
            if (value != 0)
                return value;

            value = y.Count.CompareTo(x.Count);
            return value == 0
                ? string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase)
                : value;
        }
    }

    private static bool loggedInvalidDiseaseIndex;

    internal static string SummarizeStorageContents(Storage storage, int maxLines)
    {
        if (storage == null)
        {
            Debug.LogWarning("[ContainerTooltips]: Skipping null Storage");
            return string.Empty;
        }

        var items = storage.items;
        if (items == null || items.Count == 0)
            return string.Empty;

        var summaries = new ContentSummaryCollection(0);
        var recursionGuard = new HashSet<int>();

        foreach (var item in items)
            ProcessStorageItem(item, summaries, recursionGuard);

        if (summaries.Count == 0)
        {
            Debug.LogWarning($"[ContainerTooltips]: BuildContentsSummary created no summaries after processing items for {storage.name}");
            return string.Empty;
        }

        var options = Options.Instance;
        summaries.Sort(options.SortMode);

        var flattened = FlattenedSummaries(summaries).ToList();
        if (flattened.Count == 0)
            return string.Empty;

        var limit = Mathf.Max(1, maxLines);
        var builder = new StringBuilder(flattened.Count * 100);
        var written = 0;

        foreach (var entry in flattened)
        {
            if (!(entry.Units > 0f) && !(entry.Calories > 0f))
            {
                var diseases = entry.Diseases;
                if (diseases == null || diseases.Count == 0)
                {
                    var children = entry.Children;
                    if (children == null || children.Count == 0)
                    {
                        Debug.Log($"[ContainerTooltips]: Skipping content summary for {storage.name}'s {entry.Name} due to no substantial information.");
                        continue;
                    }
                }
            }

            if (written > 0)
                builder.Append('\n');

            builder.Append(FormatContentSummary(entry, options));
            written++;

            if (written >= limit)
                break;
        }

        if (flattened.Count > limit)
        {
            builder.Append('\n');
            builder.Append('+').Append(flattened.Count - limit).Append(" more...");
        }

        var summary = builder.ToString();
        return written > 1 ? "\n" + summary : summary;
    }

    private static void ProcessStorageItem(GameObject? item, ContentSummaryCollection summaries, HashSet<int> recursionGuard)
    {
        if (item == null)
        {
            Debug.LogWarning("[ContainerTooltips]: Skipping null GameObject");
            return;
        }

        var instanceId = item.GetInstanceID();
        if (!recursionGuard.Add(instanceId))
        {
            Debug.LogWarning($"[ContainerTooltips]: Detected recursive storage reference on {item.name}, aborting nested inspection");
            return;
        }

        try
        {
            if (!item.TryGetComponent(out KPrefabID prefab))
            {
                Debug.LogWarning($"[ContainerTooltips]: GameObject {item.name} missing KPrefabID");
                return;
            }

            var summary = summaries.GetOrAdd(prefab.PrefabTag, prefab.GetProperName());
            summary.Count++;

            if (item.TryGetComponent(out PrimaryElement primary))
            {
                summary.Mass += primary.Mass;
                summary.Units += primary.Units;
                summary.TemperatureSum += primary.Temperature;
                summary.TemperatureSamples++;

                if (primary.DiseaseCount > 0)
                    summary.AddDisease(primary.DiseaseIdx, primary.DiseaseCount);
            }
            else if (item.TryGetComponent(out Pickupable pickupable))
            {
                Debug.Log($"[ContainerTooltips]: Item {item.name} isn't a PrimaryElement, but it is Pickupable ({pickupable.TotalAmount})");
                summary.Units += pickupable.TotalAmount;
            }

            if (item.TryGetComponent(out Edible edible))
                summary.Calories += edible.Calories;

            if (item.TryGetComponent(out Storage nested) && nested.items is { Count: > 0 })
            {
                var children = summary.EnsureChildren();
                foreach (var nestedItem in nested.items)
                    ProcessStorageItem(nestedItem, children, recursionGuard);
            }
        }
        finally
        {
            recursionGuard.Remove(instanceId);
        }
    }

    private static string FormatContentSummary(ContentSummary summary, Options options)
    {
        var hasMultiple = summary.Count > 1;
        string amount;

        if (summary.Mass > 0f)
        {
            amount = GameUtil.GetFormattedMass(
                summary.Mass,
                GameUtil.TimeSlice.None,
                (GameUtil.MetricMassFormat)options.MassUnits,
                includeSuffix: true,
                floatFormat: "{0:0.#}");
        }
        else if (summary.Units > 0f)
        {
            amount = GameUtil.GetFormattedUnits(summary.Units, GameUtil.TimeSlice.None, true, string.Empty);
        }
        else
        {
            Debug.Log($"[ContainerTooltips]: Item {summary} doesn't have any Mass or Units");
            amount = $"{summary.Count} {(summary.Count == 1 ? "item" : "items")}";
            hasMultiple = false;
        }

        if (summary.Calories > 0f)
            amount += " (" + GameUtil.GetFormattedCalories(summary.Calories, GameUtil.TimeSlice.None, true) + ")";

        if (hasMultiple)
            amount += $" ({summary.Count} {(summary.Count == 1 ? "item" : "items")})";

        var samples = summary.TemperatureSamples > 0 ? summary.TemperatureSamples : summary.Count;
        var temperature = GameUtil.GetFormattedTemperature(
            summary.TemperatureSum / Mathf.Max(samples, 1),
            GameUtil.TimeSlice.None,
            GameUtil.TemperatureInterpretation.Absolute,
            true);

        if (samples > 1)
            temperature = "~" + temperature;

        var line = GetIndent(summary.Depth) + string.Format(options.LineFormat, summary.Name, amount, temperature);
        var diseases = FormatDiseases(summary);
        if (!string.IsNullOrEmpty(diseases))
            line += "\n" + GetIndent(summary.Depth + 1) + diseases;

        return line;
    }

    private static string GetIndent(int depth)
    {
        return new string(' ', depth * 4);
    }

    private static string FormatDiseases(ContentSummary summary)
    {
        if (summary.TotalDiseaseCount <= 0 || summary.Diseases == null || summary.Diseases.Count == 0)
            return string.Empty;

        var diseaseDatabase = Db.Get().Diseases;
        var diseaseCount = diseaseDatabase.Count;
        var names = new List<string>();

        foreach (var kvp in summary.Diseases)
        {
            var index = kvp.Key;
            if (index == byte.MaxValue || index < 0 || index >= diseaseCount)
            {
                if (!loggedInvalidDiseaseIndex)
                {
                    Debug.LogWarning($"[ContainerTooltips]: Skipping invalid disease index {index} while summarizing storage contents");
                    loggedInvalidDiseaseIndex = true;
                }

                continue;
            }

            names.Add(GameUtil.GetFormattedDiseaseName(index, false));
        }

        if (names.Count == 0)
            return string.Empty;

        var amount = GameUtil.GetFormattedDiseaseAmount(summary.TotalDiseaseCount, GameUtil.TimeSlice.None);
        var diseaseList = string.Join(" + ", names);
        return amount + " [" + diseaseList + "]";
    }

    private static IEnumerable<ContentSummary> FlattenedSummaries(ContentSummaryCollection entries)
    {
        foreach (var item in entries.Items)
        {
            yield return item;

            if (item.Children == null)
                continue;

            foreach (var child in FlattenedSummaries(item.Children))
                yield return child;
        }
    }
}
