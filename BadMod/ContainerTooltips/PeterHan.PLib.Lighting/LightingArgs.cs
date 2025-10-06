using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PeterHan.PLib.Lighting;

public sealed class LightingArgs : EventArgs, IDictionary<int, float>, ICollection<KeyValuePair<int, float>>, IEnumerable<KeyValuePair<int, float>>, IEnumerable
{
	public IDictionary<int, float> Brightness { get; }

	public int Range { get; }

	public GameObject Source { get; }

	public int SourceCell { get; }

	public ICollection<int> Keys => Brightness.Keys;

	public ICollection<float> Values => Brightness.Values;

	public int Count => Brightness.Count;

	public bool IsReadOnly => Brightness.IsReadOnly;

	public float this[int key]
	{
		get
		{
			return Brightness[key];
		}
		set
		{
			Brightness[key] = value;
		}
	}

	internal LightingArgs(GameObject source, int cell, int range, IDictionary<int, float> output)
	{
		if ((Object)(object)source == (Object)null)
		{
			throw new ArgumentNullException("source");
		}
		Brightness = output ?? throw new ArgumentNullException("output");
		Range = range;
		Source = source;
		SourceCell = cell;
	}

	public bool ContainsKey(int key)
	{
		return Brightness.ContainsKey(key);
	}

	public void Add(int key, float value)
	{
		Brightness.Add(key, value);
	}

	public bool Remove(int key)
	{
		return Brightness.Remove(key);
	}

	public bool TryGetValue(int key, out float value)
	{
		return Brightness.TryGetValue(key, out value);
	}

	public void Add(KeyValuePair<int, float> item)
	{
		Brightness.Add(item);
	}

	public void Clear()
	{
		Brightness.Clear();
	}

	public bool Contains(KeyValuePair<int, float> item)
	{
		return Brightness.Contains(item);
	}

	public void CopyTo(KeyValuePair<int, float>[] array, int arrayIndex)
	{
		Brightness.CopyTo(array, arrayIndex);
	}

	public bool Remove(KeyValuePair<int, float> item)
	{
		return Brightness.Remove(item);
	}

	public IEnumerator<KeyValuePair<int, float>> GetEnumerator()
	{
		return Brightness.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Brightness.GetEnumerator();
	}

	public override string ToString()
	{
		return $"LightingArgs[source={SourceCell:D},range={Range:D}]";
	}
}
