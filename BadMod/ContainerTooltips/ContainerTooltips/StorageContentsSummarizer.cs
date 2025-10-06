using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeterHan.PLib.Options;
using UnityEngine;

namespace ContainerTooltips;

public static class StorageContentsSummarizer
{
	private sealed class ContentSummary
	{
		public string Name { get; }

		public Tag Tag { get; }

		public int Depth { get; }

		public int Count { get; set; }

		public float Mass { get; set; }

		public float Units { get; set; }

		public float Calories { get; set; }

		public float TemperatureSum { get; set; }

		public int TemperatureSamples { get; set; }

		public int TotalDiseaseCount { get; private set; }

		public Dictionary<byte, int>? Diseases { get; private set; }

		public ContentSummaryCollection? Children { get; private set; }

		public ContentSummary(string name, Tag tag, int depth)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			Name = name;
			Tag = tag;
			Depth = depth;
		}

		public ContentSummaryCollection EnsureChildren()
		{
			return Children ?? (Children = new ContentSummaryCollection(Depth + 1));
		}

		public Dictionary<byte, int> EnsureDiseases()
		{
			return Diseases ?? (Diseases = new Dictionary<byte, int>());
		}

		public void AddDisease(byte diseaseIdx, int amount)
		{
			Dictionary<byte, int> dictionary = EnsureDiseases();
			dictionary.TryGetValue(diseaseIdx, out var value);
			dictionary[diseaseIdx] = value + amount;
			TotalDiseaseCount += amount;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			AppendTo(stringBuilder, this, 0);
			return stringBuilder.ToString();
			static void AppendTo(StringBuilder builder, ContentSummary summary, int indentLevel)
			{
				//IL_0050: Unknown result type (might be due to invalid IL or missing references)
				string value = new string(' ', indentLevel * 2);
				builder.Append(value);
				builder.Append("ContentSummary {");
				builder.Append(" Name=").Append('"').Append(summary.Name)
					.Append('"');
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
				if (summary.Diseases == null || summary.Diseases.Count == 0)
				{
					builder.Append("[]");
				}
				else
				{
					builder.Append('[');
					bool flag = true;
					foreach (KeyValuePair<byte, int> disease in summary.Diseases)
					{
						if (!flag)
						{
							builder.Append(", ");
						}
						builder.Append(disease.Key);
						builder.Append(':');
						builder.Append(disease.Value);
						flag = false;
					}
					builder.Append(']');
				}
				builder.Append(", Children=");
				if (summary.Children == null || summary.Children.Count == 0)
				{
					builder.Append("[]");
				}
				else
				{
					builder.AppendLine("[");
					IReadOnlyList<ContentSummary> items = summary.Children.Items;
					for (int i = 0; i < items.Count; i++)
					{
						AppendTo(builder, items[i], indentLevel + 1);
						if (i < items.Count - 1)
						{
							builder.AppendLine(",");
						}
						else
						{
							builder.AppendLine();
						}
					}
					builder.Append(value);
					builder.Append(']');
				}
				builder.Append(" }");
			}
		}
	}

	private sealed class ContentSummaryCollection
	{
		private readonly int depth;

		private readonly Dictionary<Tag, ContentSummary> lookup = new Dictionary<Tag, ContentSummary>();

		private readonly List<ContentSummary> items = new List<ContentSummary>();

		public IReadOnlyList<ContentSummary> Items => items;

		public int Count => items.Count;

		public int Depth => depth;

		public ContentSummaryCollection(int depth)
		{
			this.depth = depth;
		}

		public ContentSummary GetOrAdd(Tag tag, string name)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (!lookup.TryGetValue(tag, out ContentSummary value))
			{
				value = new ContentSummary(name, tag, depth);
				lookup.Add(tag, value);
				items.Add(value);
			}
			return value;
		}

		public void Sort(ContentSortMode sortMode)
		{
			if (sortMode == ContentSortMode.Default)
			{
				return;
			}
			items.Sort(new ContentSummaryComparer(sortMode));
			foreach (ContentSummary item in items)
			{
				item.Children?.Sort(sortMode);
			}
		}
	}

	private sealed class ContentSummaryComparer : IComparer<ContentSummary>
	{
		private readonly ContentSortMode mode;

		public ContentSummaryComparer(ContentSortMode mode)
		{
			this.mode = mode;
		}

		public int Compare(ContentSummary? x, ContentSummary? y)
		{
			if (x == y)
			{
				return 0;
			}
			if (x == null)
			{
				return -1;
			}
			if (y == null)
			{
				return 1;
			}
			return mode switch
			{
				ContentSortMode.Alphabetical => CompareAlphabetical(x, y), 
				ContentSortMode.Amount => CompareAmount(x, y), 
				_ => 0, 
			};
		}

		private static int CompareAlphabetical(ContentSummary x, ContentSummary y)
		{
			int num = string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
			if (num == 0)
			{
				return CompareAmount(x, y);
			}
			return num;
		}

		private static int CompareAmount(ContentSummary x, ContentSummary y)
		{
			int num = y.Mass.CompareTo(x.Mass);
			if (num != 0)
			{
				return num;
			}
			num = y.Calories.CompareTo(x.Calories);
			if (num != 0)
			{
				return num;
			}
			num = y.Units.CompareTo(x.Units);
			if (num != 0)
			{
				return num;
			}
			num = y.Count.CompareTo(x.Count);
			if (num == 0)
			{
				return string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
			}
			return num;
		}
	}

	public static string SummarizeStorageContents(Storage storage, int maxLines)
	{
		if ((Object)(object)storage == (Object)null)
		{
			Debug.LogWarning((object)"[ContainerTooltips]: Skipping null Storage");
			return string.Empty;
		}
		List<GameObject> items = storage.items;
		if (items == null || items.Count == 0)
		{
			return string.Empty;
		}
		ContentSummaryCollection contentSummaryCollection = new ContentSummaryCollection(0);
		HashSet<int> recursionGuard = new HashSet<int>();
		foreach (GameObject item in items)
		{
			ProcessStorageItem(item, contentSummaryCollection, recursionGuard);
		}
		if (contentSummaryCollection.Count == 0)
		{
			Debug.LogWarning((object)"[ContainerTooltips]: BuildContentsSummary created no summaries after processing items");
			return string.Empty;
		}
		Options instance = SingletonOptions<Options>.Instance;
		contentSummaryCollection.Sort(instance.SortMode);
		List<ContentSummary> list = FlattenedSummaries(contentSummaryCollection).ToList();
		if (list.Count == 0)
		{
			return string.Empty;
		}
		int num = Mathf.Max(1, maxLines);
		StringBuilder stringBuilder = new StringBuilder(list.Count * 100);
		int num2 = 0;
		foreach (ContentSummary item2 in FlattenedSummaries(contentSummaryCollection))
		{
			if (!(item2.Units > 0f) && !(item2.Calories > 0f))
			{
				Dictionary<byte, int>? diseases = item2.Diseases;
				if (diseases == null || diseases.Count <= 0)
				{
					ContentSummaryCollection? children = item2.Children;
					if (children == null || children.Count <= 0)
					{
						Debug.Log((object)("[ContainerTooltips]: Skipping content summary for " + ((Object)storage).name + "'s " + item2.Name + " due to no substantial information."));
						continue;
					}
				}
			}
			if (num2 > 0)
			{
				stringBuilder.Append('\n');
			}
			stringBuilder.Append(FormatContentSummary(item2, instance));
			num2++;
			if (num2 >= num)
			{
				break;
			}
		}
		if (list.Count > num)
		{
			stringBuilder.Append('\n');
			stringBuilder.Append('+');
			stringBuilder.Append(list.Count - num);
			stringBuilder.Append(" more...");
		}
		return ((num2 > 1) ? "\n" : string.Empty) + stringBuilder.ToString();
	}

	private static void ProcessStorageItem(GameObject? item, ContentSummaryCollection contentSummaries, HashSet<int> recursionGuard)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)item == (Object)null)
		{
			Debug.LogWarning((object)"[ContainerTooltips]: Skipping null GameObject");
			return;
		}
		int instanceID = ((Object)item).GetInstanceID();
		if (!recursionGuard.Add(instanceID))
		{
			Debug.LogWarning((object)("[ContainerTooltips]: Detected recursive storage reference on " + ((Object)item).name + ", aborting nested inspection"));
			return;
		}
		try
		{
			KPrefabID component = item.GetComponent<KPrefabID>();
			if ((Object)(object)component == (Object)null)
			{
				Debug.LogWarning((object)("[ContainerTooltips]: GameObject " + ((Object)item).name + " missing KPrefabID"));
				return;
			}
			ContentSummary orAdd = contentSummaries.GetOrAdd(component.PrefabTag, KSelectableExtensions.GetProperName((Component)(object)component));
			orAdd.Count++;
			PrimaryElement val = default(PrimaryElement);
			Pickupable val2 = default(Pickupable);
			if (item.TryGetComponent<PrimaryElement>(ref val))
			{
				orAdd.Mass += val.Mass;
				orAdd.Units += val.Units;
				orAdd.TemperatureSum += val.Temperature;
				orAdd.TemperatureSamples++;
				if (val.DiseaseCount > 0)
				{
					orAdd.AddDisease(val.DiseaseIdx, val.DiseaseCount);
				}
			}
			else if (item.TryGetComponent<Pickupable>(ref val2))
			{
				Debug.Log((object)$"[ContainerTooltips]: Item {((Object)item).name} isn't a PrimaryElement, but it is Pickupable ({val2.TotalAmount})");
				orAdd.Units += val2.TotalAmount;
			}
			Edible val3 = default(Edible);
			if (item.TryGetComponent<Edible>(ref val3))
			{
				orAdd.Calories += val3.Calories;
			}
			Storage val4 = default(Storage);
			if (!item.TryGetComponent<Storage>(ref val4) || val4.items == null || val4.items.Count <= 0)
			{
				return;
			}
			ContentSummaryCollection contentSummaries2 = orAdd.EnsureChildren();
			foreach (GameObject item2 in val4.items)
			{
				ProcessStorageItem(item2, contentSummaries2, recursionGuard);
			}
		}
		finally
		{
			recursionGuard.Remove(instanceID);
		}
	}

	private static string FormatContentSummary(ContentSummary summary, Options options)
	{
		bool flag = summary.Count > 1;
		string text;
		if (summary.Mass > 0f)
		{
			text = GameUtil.GetFormattedMass(summary.Mass, (TimeSlice)0, (MetricMassFormat)options.MassUnits, true, "{0:0.#}");
		}
		else if (summary.Units > 0f)
		{
			text = GameUtil.GetFormattedUnits(summary.Units, (TimeSlice)0, true, "");
		}
		else
		{
			Debug.Log((object)$"[ContainerTooltips]: Item {summary} doesn't have any Mass or Units");
			text = string.Format("{0} {1}", summary.Count, (summary.Count == 1) ? "item" : "items");
			flag = false;
		}
		if (summary.Calories > 0f)
		{
			text = text + " (" + GameUtil.GetFormattedCalories(summary.Calories, (TimeSlice)0, true) + ")";
		}
		if (flag)
		{
			text += string.Format(" ({0} {1})", summary.Count, (summary.Count == 1) ? "item" : "items");
		}
		int num = ((summary.TemperatureSamples > 0) ? summary.TemperatureSamples : summary.Count);
		string text2 = GameUtil.GetFormattedTemperature(summary.TemperatureSum / (float)Mathf.Max(num, 1), (TimeSlice)0, (TemperatureInterpretation)0, true, false);
		if (num > 1)
		{
			text2 = "~" + text2;
		}
		string text3 = GetIndent(summary.Depth) + string.Format(options.LineFormat, summary.Name, text, text2);
		string text4 = FormatDiseases(summary);
		if (!string.IsNullOrEmpty(text4))
		{
			text3 = text3 + "\n" + GetIndent(summary.Depth + 1) + text4;
		}
		return text3;
	}

	private static string GetIndent(int depth)
	{
		return new string(' ', depth * 4);
	}

       private static bool loggedInvalidDiseaseIndex;

       private static string FormatDiseases(ContentSummary summary)
       {
               if (summary.TotalDiseaseCount <= 0 || summary.Diseases == null || summary.Diseases.Count == 0)
               {
                       return string.Empty;
               }
               int count = Db.Get().Diseases.Count;
               List<string> list = new List<string>();
               foreach (KeyValuePair<byte, int> item in summary.Diseases)
               {
                       int key = item.Key;
                       if (key == byte.MaxValue || key < 0 || key >= count)
                       {
                               if (!loggedInvalidDiseaseIndex)
                               {
                                       Debug.LogWarning((object)$"[ContainerTooltips]: Skipping invalid disease index {key} while summarizing storage contents");
                                       loggedInvalidDiseaseIndex = true;
                               }
                               continue;
                       }
                       list.Add(GameUtil.GetFormattedDiseaseName((byte)key, false));
               }
               if (list.Count == 0)
               {
                       return string.Empty;
               }
               string formattedDiseaseAmount = GameUtil.GetFormattedDiseaseAmount(summary.TotalDiseaseCount, (TimeSlice)0);
               string text = string.Join(" + ", list);
               return formattedDiseaseAmount + " [" + text + "]";
       }

	private static IEnumerable<ContentSummary> FlattenedSummaries(ContentSummaryCollection entries)
	{
		foreach (ContentSummary item in entries.Items)
		{
			yield return item;
			if (item.Children == null)
			{
				continue;
			}
			foreach (ContentSummary item2 in FlattenedSummaries(item.Children))
			{
				yield return item2;
			}
		}
	}
}
