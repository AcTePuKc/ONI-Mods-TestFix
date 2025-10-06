using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PeterHan.PLib.Options;

internal sealed class ColorGradient : Image
{
	private Color current;

	private bool dirty;

	private Vector2 hue;

	private float position;

	private Texture2D preview;

	private Vector2 sat;

	private Vector2 val;

	public float Position
	{
		get
		{
			return position;
		}
		set
		{
			position = Mathf.Clamp01(value);
			SetPosition();
		}
	}

	public Color SelectedColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return current;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			current = value;
			EstimatePosition();
		}
	}

	public ColorGradient()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		current = Color.black;
		position = 0f;
		preview = null;
		dirty = true;
		hue = Vector2.right;
		sat = Vector2.right;
		val = Vector2.right;
	}

	internal void EstimatePosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		float num = default(float);
		float num2 = default(float);
		float num3 = default(float);
		Color.RGBToHSV(current, ref num, ref num2, ref num3);
		float num4 = 0f;
		float num5 = 0f;
		if (!Mathf.Approximately(hue.x, hue.y))
		{
			num4 += Mathf.Clamp01(Mathf.InverseLerp(hue.x, hue.y, num));
			num5 += 1f;
		}
		if (!Mathf.Approximately(sat.x, sat.y))
		{
			num4 += Mathf.Clamp01(Mathf.InverseLerp(sat.x, sat.y, num2));
			num5 += 1f;
		}
		if (!Mathf.Approximately(val.x, val.y))
		{
			num4 += Mathf.Clamp01(Mathf.InverseLerp(val.x, val.y, num3));
			num5 += 1f;
		}
		if (num5 > 0f)
		{
			position = num4 / num5;
			SetPosition();
		}
	}

	protected override void OnDestroy()
	{
		if ((Object)(object)preview != (Object)null)
		{
			Object.Destroy((Object)(object)preview);
			preview = null;
		}
		if ((Object)(object)((Image)this).sprite != (Object)null)
		{
			Object.Destroy((Object)(object)((Image)this).sprite);
			((Image)this).sprite = null;
		}
		((Graphic)this).OnDestroy();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		((Graphic)this).OnRectTransformDimensionsChange();
		dirty = true;
	}

	internal void SetPosition()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01(Mathf.Lerp(hue.x, hue.y, position));
		float num2 = Mathf.Clamp01(Mathf.Lerp(sat.x, sat.y, position));
		float num3 = Mathf.Clamp01(Mathf.Lerp(val.x, val.y, position));
		current = Color.HSVToRGB(num, num2, num3);
	}

	public void SetRange(float hMin, float hMax, float sMin, float sMax, float vMin, float vMax)
	{
		if (hMin > hMax)
		{
			hue.x = hMax;
			hue.y = hMin;
		}
		else
		{
			hue.x = hMin;
			hue.y = hMax;
		}
		if (sMin > sMax)
		{
			sat.x = sMax;
			sat.y = sMin;
		}
		else
		{
			sat.x = sMin;
			sat.y = sMax;
		}
		if (vMin > vMax)
		{
			val.x = vMax;
			val.y = vMin;
		}
		else
		{
			val.x = vMin;
			val.y = vMax;
		}
		EstimatePosition();
		dirty = true;
	}

	protected override void Start()
	{
		((UIBehaviour)this).Start();
		dirty = true;
	}

	internal void Update()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Expected O, but got Unknown
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		if (!dirty && !((Object)(object)preview == (Object)null) && !((Object)(object)((Image)this).sprite == (Object)null))
		{
			return;
		}
		Rect rect = ((Graphic)this).rectTransform.rect;
		int num = Mathf.RoundToInt(((Rect)(ref rect)).width);
		int num2 = Mathf.RoundToInt(((Rect)(ref rect)).height);
		if (num > 0 && num2 > 0)
		{
			Color[] array = (Color[])(object)new Color[num * num2];
			float x = hue.x;
			float y = hue.y;
			float x2 = sat.x;
			float y2 = sat.y;
			float x3 = val.x;
			float y3 = val.y;
			Sprite sprite = ((Image)this).sprite;
			bool flag = (Object)(object)preview == (Object)null || (Object)(object)sprite == (Object)null || ((Texture)preview).width != num || ((Texture)preview).height != num2;
			if (flag)
			{
				if ((Object)(object)preview != (Object)null)
				{
					Object.Destroy((Object)(object)preview);
				}
				if ((Object)(object)sprite != (Object)null)
				{
					Object.Destroy((Object)(object)sprite);
				}
				preview = new Texture2D(num, num2);
			}
			for (int i = 0; i < num; i++)
			{
				float num3 = (float)i / (float)num;
				array[i] = Color.HSVToRGB(Mathf.Lerp(x, y, num3), Mathf.Lerp(x2, y2, num3), Mathf.Lerp(x3, y3, num3));
			}
			for (int j = 1; j < num2; j++)
			{
				Array.Copy(array, 0, array, j * num, num);
			}
			preview.SetPixels(array);
			preview.Apply();
			if (flag)
			{
				((Image)this).sprite = Sprite.Create(preview, new Rect(0f, 0f, (float)num, (float)num2), new Vector2(0.5f, 0.5f));
			}
		}
		dirty = false;
	}
}
