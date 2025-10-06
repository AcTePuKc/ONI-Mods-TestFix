using System;
using System.Collections.Generic;
using System.Reflection;
using PeterHan.PLib.Core;
using PeterHan.PLib.UI;
using UnityEngine;

namespace PeterHan.PLib.Options;

internal class CompositeOptionsEntry : OptionsEntry
{
	protected readonly IDictionary<PropertyInfo, IOptionsEntry> subOptions;

	protected readonly Type targetType;

	protected object value;

	public int ChildCount => subOptions.Count;

	public override object Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value != null && targetType.IsAssignableFrom(value.GetType()))
			{
				this.value = value;
			}
		}
	}

	internal static CompositeOptionsEntry Create(IOptionSpec spec, PropertyInfo info, int depth)
	{
		Type propertyType = info.PropertyType;
		CompositeOptionsEntry compositeOptionsEntry = new CompositeOptionsEntry(info.Name, spec, propertyType);
		PropertyInfo[] properties = propertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			IOptionsEntry optionsEntry = OptionsEntry.TryCreateEntry(propertyInfo, depth + 1);
			if (optionsEntry != null)
			{
				compositeOptionsEntry.AddField(propertyInfo, optionsEntry);
			}
		}
		if (compositeOptionsEntry.ChildCount <= 0)
		{
			return null;
		}
		return compositeOptionsEntry;
	}

	public CompositeOptionsEntry(string field, IOptionSpec spec, Type fieldType)
		: base(field, spec)
	{
		subOptions = new Dictionary<PropertyInfo, IOptionsEntry>(16);
		targetType = fieldType ?? throw new ArgumentNullException("fieldType");
		value = OptionsDialog.CreateOptions(fieldType);
	}

	public void AddField(PropertyInfo info, IOptionsEntry entry)
	{
		if (entry == null)
		{
			throw new ArgumentNullException("entry");
		}
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		subOptions.Add(info, entry);
	}

	public override void CreateUIEntry(PGridPanel parent, ref int row)
	{
		int row2 = row;
		bool flag = true;
		parent.AddOnRealize(WhenRealized);
		foreach (KeyValuePair<PropertyInfo, IOptionsEntry> subOption in subOptions)
		{
			if (!flag)
			{
				row2++;
				parent.AddRow(new GridRowSpec());
			}
			subOption.Value.CreateUIEntry(parent, ref row2);
			flag = false;
		}
		row = row2;
	}

	public override GameObject GetUIComponent()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		return new GameObject("Empty");
	}

	public override void ReadFrom(object settings)
	{
		base.ReadFrom(settings);
		foreach (KeyValuePair<PropertyInfo, IOptionsEntry> subOption in subOptions)
		{
			subOption.Value.ReadFrom(value);
		}
	}

	public override string ToString()
	{
		return "{1}[field={0},title={2},children=[{3}]]".F(base.Field, GetType().Name, base.Title, subOptions.Join());
	}

	private void WhenRealized(GameObject _)
	{
		foreach (KeyValuePair<PropertyInfo, IOptionsEntry> subOption in subOptions)
		{
			subOption.Value.ReadFrom(value);
		}
	}

	public override void WriteTo(object settings)
	{
		foreach (KeyValuePair<PropertyInfo, IOptionsEntry> subOption in subOptions)
		{
			subOption.Value.WriteTo(value);
		}
		base.WriteTo(settings);
	}
}
