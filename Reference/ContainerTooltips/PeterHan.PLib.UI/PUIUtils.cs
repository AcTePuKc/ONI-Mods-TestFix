using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using PeterHan.PLib.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeterHan.PLib.UI;

public static class PUIUtils
{
	private static void AddComponentText(StringBuilder result, Component component)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		FieldInfo[] fields = ((object)component).GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		TMP_Text val = (TMP_Text)(object)((component is TMP_Text) ? component : null);
		if (val != null)
		{
			result.AppendFormat(", Text={0}, Color={1}, Font={2}", val.text, ((Graphic)val).color, val.font);
		}
		else
		{
			Image val2 = (Image)(object)((component is Image) ? component : null);
			if (val2 != null)
			{
				result.AppendFormat(", Color={0}", ((Graphic)val2).color);
				if ((Object)(object)val2.sprite != (Object)null)
				{
					result.AppendFormat(", Sprite={0}", val2.sprite);
				}
			}
			else
			{
				HorizontalOrVerticalLayoutGroup val3 = (HorizontalOrVerticalLayoutGroup)(object)((component is HorizontalOrVerticalLayoutGroup) ? component : null);
				if (val3 != null)
				{
					result.AppendFormat(", Child Align={0}, Control W={1}, Control H={2}", ((LayoutGroup)val3).childAlignment, val3.childControlWidth, val3.childControlHeight);
				}
			}
		}
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			object obj = fieldInfo.GetValue(component) ?? "null";
			if (obj is LayerMask val4)
			{
				obj = "Layer #" + ((LayerMask)(ref val4)).value;
			}
			else if (obj is ICollection values)
			{
				obj = "[" + values.Join() + "]";
			}
			result.AppendFormat(", {0}={1}", fieldInfo.Name, obj);
		}
	}

	public static void AddPinkOverlay(GameObject parent)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)PUIElements.CreateUI(parent, "Overlay").AddComponent<Image>()).color = new Color(1f, 0f, 1f, 0.2f);
	}

	public static void AddSideScreenContent<T>(GameObject uiPrefab = null) where T : SideScreenContent
	{
		AddSideScreenContentWithOrdering<T>(null, insertBefore: true, uiPrefab);
	}

	public static void AddSideScreenContentWithOrdering<T>(string targetClassName, bool insertBefore = true, GameObject uiPrefab = null) where T : SideScreenContent
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		DetailsScreen instance = DetailsScreen.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			LogUIWarning("DetailsScreen is not yet initialized, try a postfix on DetailsScreen.OnPrefabInit");
			return;
		}
		List<SideScreenRef> list = UIDetours.SIDE_SCREENS.Get(instance);
		GameObject sideScreenContent = GetSideScreenContent(instance);
		string name = typeof(T).Name;
		if ((Object)(object)sideScreenContent != (Object)null && list != null)
		{
			SideScreenRef val = new SideScreenRef();
			GameObject val2 = PUIElements.CreateUI(sideScreenContent, name);
			val2.AddComponent<BoxLayoutGroup>().Params = new BoxLayoutParams
			{
				Direction = PanelDirection.Vertical,
				Alignment = (TextAnchor)1,
				Margin = new RectOffset(1, 1, 0, 1)
			};
			T val3 = val2.AddComponent<T>();
			if ((Object)(object)uiPrefab != (Object)null)
			{
				UIDetours.SS_CONTENT_CONTAINER.Set((SideScreenContent)(object)val3, uiPrefab);
				uiPrefab.transform.SetParent(val2.transform);
			}
			val.name = name;
			UIDetours.SS_OFFSET.Set(val, Vector2.zero);
			UIDetours.SS_PREFAB.Set(val, (SideScreenContent)(object)val3);
			UIDetours.SS_INSTANCE.Set(val, (SideScreenContent)(object)val3);
			InsertSideScreenContent(list, val, targetClassName, insertBefore);
		}
	}

	public static GameObject AddTo(this IUIComponent component, GameObject parent, int index = -2)
	{
		if (component == null)
		{
			throw new ArgumentNullException("component");
		}
		if ((Object)(object)parent == (Object)null)
		{
			throw new ArgumentNullException("parent");
		}
		GameObject val = component.Build();
		val.SetParent(parent);
		if (index == -1)
		{
			val.transform.SetAsLastSibling();
		}
		else if (index >= 0)
		{
			val.transform.SetSiblingIndex(index);
		}
		return val;
	}

	internal static LayoutSizes CalcSizes(GameObject obj, PanelDirection direction, IEnumerable<Component> components)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		float value = 0f;
		float value2 = 0f;
		float value3 = 0f;
		int pri = int.MinValue;
		int pri2 = int.MinValue;
		int pri3 = int.MinValue;
		Vector3 localScale = obj.transform.localScale;
		float num = Math.Abs((direction == PanelDirection.Horizontal) ? localScale.x : localScale.y);
		bool ignore = false;
		foreach (Component component in components)
		{
			Component obj2 = ((component is ILayoutIgnorer) ? component : null);
			if (obj2 != null && ((ILayoutIgnorer)obj2).ignoreLayout)
			{
				ignore = true;
				break;
			}
			Component obj3 = ((component is Behaviour) ? component : null);
			if (obj3 != null && !((Behaviour)obj3).isActiveAndEnabled)
			{
				continue;
			}
			ILayoutElement val = (ILayoutElement)(object)((component is ILayoutElement) ? component : null);
			if (val != null)
			{
				int layoutPriority = val.layoutPriority;
				if (direction == PanelDirection.Horizontal)
				{
					val.CalculateLayoutInputHorizontal();
					PriValue(ref value, val.minWidth, layoutPriority, ref pri);
					PriValue(ref value2, val.preferredWidth, layoutPriority, ref pri2);
					PriValue(ref value3, val.flexibleWidth, layoutPriority, ref pri3);
				}
				else
				{
					val.CalculateLayoutInputVertical();
					PriValue(ref value, val.minHeight, layoutPriority, ref pri);
					PriValue(ref value2, val.preferredHeight, layoutPriority, ref pri2);
					PriValue(ref value3, val.flexibleHeight, layoutPriority, ref pri3);
				}
			}
		}
		LayoutSizes result = new LayoutSizes(obj, value * num, Math.Max(value, value2) * num, value3);
		result.ignore = ignore;
		return result;
	}

	public static void DebugObjectHierarchy(this GameObject item)
	{
		string text = "null";
		if ((Object)(object)item != (Object)null)
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			do
			{
				Transform parent = item.transform.parent;
				stringBuilder.Append("- ");
				stringBuilder.Append(((Object)item).name);
				if ((Object)(object)parent != (Object)null)
				{
					item = ((Component)parent).gameObject;
					if ((Object)(object)item != (Object)null)
					{
						stringBuilder.AppendLine();
					}
				}
				else
				{
					item = null;
				}
			}
			while ((Object)(object)item != (Object)null);
			text = stringBuilder.ToString();
		}
		LogUIDebug("Object Tree:" + Environment.NewLine + text);
	}

	public static void DebugObjectTree(this GameObject root)
	{
		string text = "null";
		if ((Object)(object)root != (Object)null)
		{
			text = GetObjectTree(root, 0);
		}
		LogUIDebug("Object Dump:" + Environment.NewLine + text);
	}

	public static TextStyleSetting DeriveStyle(this TextStyleSetting root, int size = 0, Color? newColor = null, FontStyles? style = null)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)root == (Object)null)
		{
			throw new ArgumentNullException("root");
		}
		TextStyleSetting obj = ScriptableObject.CreateInstance<TextStyleSetting>();
		obj.enableWordWrapping = root.enableWordWrapping;
		obj.style = ((!style.HasValue) ? root.style : style.Value);
		obj.fontSize = ((size > 0) ? size : root.fontSize);
		obj.sdfFont = root.sdfFont;
		obj.textColor = (Color)(((_003F?)newColor) ?? root.textColor);
		return obj;
	}

	public static GameObject ForceLayoutRebuild(GameObject uiElement)
	{
		if ((Object)(object)uiElement == (Object)null)
		{
			throw new ArgumentNullException("uiElement");
		}
		RectTransform val = Util.rectTransform(uiElement);
		if ((Object)(object)val != (Object)null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(val);
		}
		return uiElement;
	}

	public static float GetEmWidth(TextStyleSetting style)
	{
		float result = 0f;
		if ((Object)(object)style == (Object)null)
		{
			throw new ArgumentNullException("style");
		}
		TMP_FontAsset sdfFont = style.sdfFont;
		if ((Object)(object)sdfFont != (Object)null && sdfFont.characterDictionary.TryGetValue(109, out var value))
		{
			FaceInfo fontInfo = sdfFont.fontInfo;
			float num = (float)style.fontSize / (fontInfo.PointSize * fontInfo.Scale);
			result = ((TMP_TextElement)value).width * num + (float)style.fontSize * 0.01f * sdfFont.normalSpacingOffset;
		}
		return result;
	}

	public static float GetLineHeight(TextStyleSetting style)
	{
		float result = 0f;
		if ((Object)(object)style == (Object)null)
		{
			throw new ArgumentNullException("style");
		}
		TMP_FontAsset sdfFont = style.sdfFont;
		if ((Object)(object)sdfFont != (Object)null)
		{
			FaceInfo fontInfo = sdfFont.fontInfo;
			result = fontInfo.LineHeight * (float)style.fontSize / (fontInfo.Scale * fontInfo.PointSize);
		}
		return result;
	}

	private static string GetObjectTree(GameObject root, int indent)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder(1024);
		StringBuilder stringBuilder2 = new StringBuilder(indent);
		for (int i = 0; i < indent; i++)
		{
			stringBuilder2.Append(' ');
		}
		string value = stringBuilder2.ToString();
		Transform transform = root.transform;
		int childCount = transform.childCount;
		stringBuilder.Append(value).AppendFormat("GameObject[{0}, {1:D} child(ren), Layer {2:D}, Active={3}]", ((Object)root).name, childCount, root.layer, root.activeInHierarchy).AppendLine();
		stringBuilder.Append(value).AppendFormat(" Translation={0} [{3}] Rotation={1} [{4}] Scale={2}", transform.position, transform.rotation, transform.localScale, transform.localPosition, transform.localRotation).AppendLine();
		Component[] components = root.GetComponents<Component>();
		foreach (Component val in components)
		{
			RectTransform val2 = (RectTransform)(object)((val is RectTransform) ? val : null);
			if (val2 != null)
			{
				Rect rect = val2.rect;
				Vector2 size = ((Rect)(ref rect)).size;
				Vector2 anchorMin = val2.anchorMin;
				Vector2 anchorMax = val2.anchorMax;
				Vector2 offsetMin = val2.offsetMin;
				Vector2 offsetMax = val2.offsetMax;
				Vector2 pivot = val2.pivot;
				stringBuilder.Append(value).AppendFormat(" Rect[Size=({0:F2},{1:F2}) Min=({2:F2},{3:F2}) ", size.x, size.y, LayoutUtility.GetMinWidth(val2), LayoutUtility.GetMinHeight(val2));
				stringBuilder.AppendFormat("Preferred=({0:F2},{1:F2}) Flexible=({2:F2},{3:F2}) ", LayoutUtility.GetPreferredWidth(val2), LayoutUtility.GetPreferredHeight(val2), LayoutUtility.GetFlexibleWidth(val2), LayoutUtility.GetFlexibleHeight(val2));
				stringBuilder.AppendFormat("Pivot=({4:F2},{5:F2}) AnchorMin=({0:F2},{1:F2}) AnchorMax=({2:F2},{3:F2}) ", anchorMin.x, anchorMin.y, anchorMax.x, anchorMax.y, pivot.x, pivot.y);
				stringBuilder.AppendFormat("OffsetMin=({0:F2},{1:F2}) OffsetMax=({2:F2},{3:F2})]", offsetMin.x, offsetMin.y, offsetMax.x, offsetMax.y).AppendLine();
			}
			else if ((Object)(object)val != (Object)null && !(val is Transform))
			{
				stringBuilder.Append(value).Append(" Component[").Append(((object)val).GetType().FullName);
				AddComponentText(stringBuilder, val);
				stringBuilder.AppendLine("]");
			}
		}
		if (childCount > 0)
		{
			stringBuilder.Append(value).AppendLine(" Children:");
		}
		for (int k = 0; k < childCount; k++)
		{
			GameObject gameObject = ((Component)transform.GetChild(k)).gameObject;
			if ((Object)(object)gameObject != (Object)null)
			{
				stringBuilder.AppendLine(GetObjectTree(gameObject, indent + 2));
			}
		}
		return stringBuilder.ToString().TrimEnd();
	}

	internal static float GetProperSize(LayoutSizes sizes, float allocated)
	{
		float num = sizes.min;
		float num2 = Math.Max(sizes.preferred, num);
		if (allocated > num)
		{
			num = Math.Min(num2, allocated);
		}
		if (allocated > num2 && sizes.flexible > 0f)
		{
			num = allocated;
		}
		return num;
	}

	internal static float GetOffset(TextAnchor alignment, PanelDirection direction, float delta)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected I4, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		float result = 0f;
		if (direction == PanelDirection.Horizontal)
		{
			switch (alignment - 1)
			{
			case 0:
			case 3:
			case 6:
				result = delta * 0.5f;
				break;
			case 1:
			case 4:
			case 7:
				result = delta;
				break;
			}
		}
		else if (alignment - 3 > 2)
		{
			if (alignment - 6 <= 2)
			{
				result = delta;
			}
		}
		else
		{
			result = delta * 0.5f;
		}
		return result;
	}

	public static GameObject GetParent(this GameObject child)
	{
		GameObject result = null;
		if ((Object)(object)child != (Object)null)
		{
			Transform parent = child.transform.parent;
			GameObject gameObject;
			if ((Object)(object)parent != (Object)null && (Object)(object)(gameObject = ((Component)parent).gameObject) != (Object)null)
			{
				result = gameObject;
			}
		}
		return result;
	}

	public static GameObject GetSideScreenContent(DetailsScreen screen)
	{
		GameObject value = null;
		if ((Object)(object)screen != (Object)null)
		{
			if (UIDetours.SS_CONTENT_BODY != null)
			{
				value = UIDetours.SS_CONTENT_BODY.Get(screen);
			}
			else if (UIDetours.SS_GET_TAB != null)
			{
				object obj = UIDetours.SS_GET_TAB.Invoke(screen, new object[1] { 0 });
				if (obj != null)
				{
					PPatchTools.TryGetFieldValue<GameObject>(obj, "bodyInstance", out value);
				}
			}
		}
		return value;
	}

	internal static GameObject InsetChild(GameObject parent, GameObject child, Vector2 insets, Vector2 prefSize = default(Vector2))
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = Util.rectTransform(child);
		float x = insets.x;
		float y = insets.y;
		float x2 = prefSize.x;
		float y2 = prefSize.y;
		val.offsetMax = new Vector2(0f - x, 0f - y);
		val.offsetMin = insets;
		LayoutElement obj = EntityTemplateExtensions.AddOrGet<LayoutElement>(parent);
		float minWidth = (obj.preferredWidth = ((x2 <= 0f) ? LayoutUtility.GetPreferredWidth(val) : x2) + x * 2f);
		obj.minWidth = minWidth;
		minWidth = (obj.preferredHeight = ((y2 <= 0f) ? LayoutUtility.GetPreferredHeight(val) : y2) + y * 2f);
		obj.minHeight = minWidth;
		return parent;
	}

	private static void InsertSideScreenContent(IList<SideScreenRef> screens, SideScreenRef newScreen, string targetClassName, bool insertBefore)
	{
		if (screens == null)
		{
			throw new ArgumentNullException("screens");
		}
		if (newScreen == null)
		{
			throw new ArgumentNullException("newScreen");
		}
		if (string.IsNullOrEmpty(targetClassName))
		{
			screens.Add(newScreen);
			return;
		}
		int count = screens.Count;
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			SideScreenRef val = screens[i];
			SideScreenContent val2 = UIDetours.SS_PREFAB.Get(val);
			if (!((Object)(object)val2 != (Object)null))
			{
				continue;
			}
			SideScreenContent[] componentsInChildren = ((Component)val2).GetComponentsInChildren<SideScreenContent>();
			if (componentsInChildren == null || componentsInChildren.Length < 1)
			{
				LogUIWarning("Could not find SideScreenContent on side screen: " + val.name);
			}
			else if (((object)componentsInChildren[0]).GetType().FullName == targetClassName)
			{
				if (insertBefore)
				{
					screens.Insert(i, newScreen);
				}
				else if (i >= count - 1)
				{
					screens.Add(newScreen);
				}
				else
				{
					screens.Insert(i + 1, newScreen);
				}
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			LogUIWarning("No side screen with class name {0} found!".F(targetClassName));
			screens.Add(newScreen);
		}
	}

	public static Sprite LoadSprite(string path, Vector4 border = default(Vector4), bool log = true)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return LoadSprite(Assembly.GetCallingAssembly(), path, border, log);
	}

	internal static Sprite LoadSprite(Assembly assembly, string path, Vector4 border = default(Vector4), bool log = false)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			using Stream stream = assembly.GetManifestResourceStream(path);
			if (stream == null)
			{
				throw new ArgumentException("Could not load image: " + path);
			}
			int num = (int)stream.Length;
			byte[] array = new byte[num];
			Texture2D val = new Texture2D(2, 2);
			num = ReadAllBytes(stream, array);
			ImageConversion.LoadImage(val, array, false);
			int width = ((Texture)val).width;
			int height = ((Texture)val).height;
			if (log)
			{
				LogUIDebug("Loaded sprite: {0} ({1:D}x{2:D}, {3:D} bytes)".F(path, width, height, num));
			}
			return Sprite.Create(val, new Rect(0f, 0f, (float)width, (float)height), new Vector2(0.5f, 0.5f), 100f, 0u, (SpriteMeshType)0, border);
		}
		catch (IOException innerException)
		{
			throw new ArgumentException("Could not load image: " + path, innerException);
		}
	}

	public static Sprite LoadSpriteFile(string path, Vector4 border = default(Vector4))
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		Sprite result = null;
		try
		{
			using FileStream fileStream = new FileStream(path, FileMode.Open);
			byte[] array = new byte[(int)fileStream.Length];
			Texture2D val = new Texture2D(2, 2);
			ReadAllBytes(fileStream, array);
			ImageConversion.LoadImage(val, array, false);
			int width = ((Texture)val).width;
			int height = ((Texture)val).height;
			result = Sprite.Create(val, new Rect(0f, 0f, (float)width, (float)height), new Vector2(0.5f, 0.5f), 100f, 0u, (SpriteMeshType)0, border);
		}
		catch (IOException)
		{
		}
		return result;
	}

	internal static Sprite LoadSpriteLegacy(Assembly assembly, string path, int width, int height, Vector4 border = default(Vector4))
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			using Stream stream = assembly.GetManifestResourceStream(path);
			if (stream == null)
			{
				throw new ArgumentException("Could not load image: " + path);
			}
			int num = (int)stream.Length - 128;
			if (num < 0)
			{
				throw new ArgumentException("Image is too small: " + path);
			}
			byte[] array = new byte[num];
			stream.Seek(128L, SeekOrigin.Begin);
			num = ReadAllBytes(stream, array);
			Texture2D val = new Texture2D(width, height, (TextureFormat)12, false);
			val.LoadRawTextureData(array);
			val.Apply(true, true);
			LogUIDebug("Loaded sprite: {0} ({1:D}x{2:D}, {3:D} bytes)".F(path, width, height, num));
			return Sprite.Create(val, new Rect(0f, 0f, (float)width, (float)height), new Vector2(0.5f, 0.5f), 100f, 0u, (SpriteMeshType)0, border);
		}
		catch (IOException innerException)
		{
			throw new ArgumentException("Could not load image: " + path, innerException);
		}
	}

	private static int ReadAllBytes(Stream stream, byte[] data)
	{
		int num = 0;
		int num2 = data.Length;
		int num3;
		do
		{
			num3 = stream.Read(data, num, num2 - num);
			num += num3;
		}
		while (num3 > 0 && num < num2);
		return num;
	}

	internal static void LogUIDebug(string message)
	{
		Debug.LogFormat("[PLib/UI/{0}] {1}", new object[2]
		{
			Assembly.GetCallingAssembly().GetName().Name ?? "?",
			message
		});
	}

	internal static void LogUIWarning(string message)
	{
		Debug.LogWarningFormat("[PLib/UI/{0}] {1}", new object[2]
		{
			Assembly.GetCallingAssembly().GetName().Name ?? "?",
			message
		});
	}

	private static void PriValue(ref float value, float newValue, int newPri, ref int pri)
	{
		int num = pri;
		if (newValue >= 0f)
		{
			if (newPri > num)
			{
				pri = newPri;
				value = newValue;
			}
			else if (newValue > value && newPri == num)
			{
				value = newValue;
			}
		}
	}

	public static GameObject SetFlexUISize(this GameObject uiElement, Vector2 flexSize)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)uiElement == (Object)null)
		{
			throw new ArgumentNullException("uiElement");
		}
		ISettableFlexSize settableFlexSize = default(ISettableFlexSize);
		if (uiElement.TryGetComponent<ISettableFlexSize>(ref settableFlexSize))
		{
			settableFlexSize.flexibleWidth = flexSize.x;
			settableFlexSize.flexibleHeight = flexSize.y;
		}
		else
		{
			LayoutElement obj = EntityTemplateExtensions.AddOrGet<LayoutElement>(uiElement);
			obj.flexibleWidth = flexSize.x;
			obj.flexibleHeight = flexSize.y;
		}
		return uiElement;
	}

	public static GameObject SetMinUISize(this GameObject uiElement, Vector2 minSize)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)uiElement == (Object)null)
		{
			throw new ArgumentNullException("uiElement");
		}
		float x = minSize.x;
		float y = minSize.y;
		if (x > 0f || y > 0f)
		{
			LayoutElement val = EntityTemplateExtensions.AddOrGet<LayoutElement>(uiElement);
			if (x > 0f)
			{
				val.minWidth = x;
			}
			if (y > 0f)
			{
				val.minHeight = y;
			}
		}
		return uiElement;
	}

	public static GameObject SetUISize(this GameObject uiElement, Vector2 size, bool addLayout = false)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)uiElement == (Object)null)
		{
			throw new ArgumentNullException("uiElement");
		}
		RectTransform val = Util.rectTransform(uiElement);
		float x = size.x;
		float y = size.y;
		if ((Object)(object)val != (Object)null)
		{
			if (x >= 0f)
			{
				val.SetSizeWithCurrentAnchors((Axis)0, x);
			}
			if (y >= 0f)
			{
				val.SetSizeWithCurrentAnchors((Axis)1, y);
			}
		}
		if (addLayout)
		{
			LayoutElement obj = EntityTemplateExtensions.AddOrGet<LayoutElement>(uiElement);
			obj.minWidth = x;
			obj.minHeight = y;
			obj.preferredWidth = x;
			obj.preferredHeight = y;
			obj.flexibleHeight = 0f;
			obj.flexibleWidth = 0f;
		}
		return uiElement;
	}
}
