using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using KMod;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DevLoader;

public static class LiveLoader
{
	private static readonly List<string> _ourHarmonyIds = new List<string>();

	private static readonly HashSet<string> _foreignIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly List<Assembly> _loaded = new List<Assembly>();

	private static bool _isLoadingOrUnloading = false;

	public static void LoadAll()
	{
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Expected O, but got Unknown
		if (_isLoadingOrUnloading)
		{
			Debug.Log((object)"[DevLoader] LoadAll() ignorado: ya en curso.");
			return;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		_isLoadingOrUnloading = true;
		try
		{
			List<string> list = ResolveDevDirs();
			if (list.Count == 0)
			{
				Debug.Log((object)"[DevLoader] Mods/DEV no existe (ni en instalación ni en Documentos).");
				return;
			}
			int num = CountAllPatches();
			Stopwatch stopwatch2 = Stopwatch.StartNew();
			UnloadAll();
			stopwatch2.Stop();
			Debug.Log((object)$"[DevLoader] Pre-clean done en {stopwatch2.ElapsedMilliseconds} ms");
			_loaded.Clear();
			_ourHarmonyIds.Clear();
			_foreignIds.Clear();
			HashSet<string> hashSet = SnapshotHarmonyOwners();
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
                        foreach (string item in list)
                        {
                                string[] files = System.IO.Directory.GetFiles(item, "*.dll", SearchOption.AllDirectories);
				Debug.Log((object)$"[DevLoader] Buscando DLLs en: {item} -> {files.Length} archivos");
				string[] array = files;
                                foreach (string text in array)
                                {
                                        num2++;
                                        Stopwatch stopwatch3 = Stopwatch.StartNew();
                                        try
                                        {
                                                Assembly assembly2 = FindLoadedAssembly(text);
                                                if (assembly2 != null)
                                                {
                                                        Debug.Log((object)$"[DevLoader] Skipping {Path.GetFileName(text)} (already loaded as {assembly2.GetName().FullName})");
                                                        continue;
                                                }
                                                string modRoot = GetModRootForDll(text, out string modInfoPath);
                                                TryLoadModContent(modRoot, modInfoPath);
                                                byte[] rawAssembly = File.ReadAllBytes(text);
                                                Assembly assembly = Assembly.Load(rawAssembly);
						_loaded.Add(assembly);
						string text2 = SanitizeId(Path.GetFileNameWithoutExtension(text));
						string text3 = ("devloader.live." + text2).ToLowerInvariant();
						_ourHarmonyIds.Add(text3);
						Harmony val = new Harmony(text3);
						int num5 = 0;
						Type[] types = assembly.GetTypes();
						foreach (Type type in types)
						{
							try
							{
								if (typeof(UserMod2).IsAssignableFrom(type) && !type.IsAbstract)
								{
									object obj = Activator.CreateInstance(type);
									UserMod2 val2 = (UserMod2)((obj is UserMod2) ? obj : null);
									if (val2 != null)
									{
										val2.OnLoad(val);
									}
									num5++;
								}
							}
							catch (Exception ex)
							{
								Debug.LogWarning((object)("[DevLoader] OnLoad falló en " + type.FullName + ": " + ex.Message));
							}
						}
						int num6 = PatchAllInAssembly(val, assembly);
						num3 += num5;
						num4 += num6;
						stopwatch3.Stop();
						Debug.Log((object)$"[DevLoader] Loaded {Path.GetFileName(text)} → OnLoad={num5}, PatchedTypes={num6}, t={stopwatch3.ElapsedMilliseconds} ms");
					}
					catch (Exception ex2)
					{
						stopwatch3.Stop();
						Debug.LogWarning((object)("[DevLoader] Error cargando " + text + " (" + stopwatch3.ElapsedMilliseconds + " ms): " + ex2.Message));
					}
				}
			}
			HashSet<string> hashSet2 = SnapshotHarmonyOwners();
			int num7 = 0;
			foreach (string item2 in hashSet2)
			{
				if (!hashSet.Contains(item2) && !_ourHarmonyIds.Contains(item2) && _foreignIds.Add(item2))
				{
					num7++;
				}
			}
			int num8 = CountAllPatches();
			string arg = ((num7 > 0) ? (" [" + string.Join(", ", _foreignIds) + "]") : "");
			Debug.Log((object)($"[DevLoader] RESUMEN: DLLs={num2}, OnLoad ejecutados={num3}, " + $"Types parcheados={num4}, " + $"HarmonyIDs nuestros={_ourHarmonyIds.Count}, " + $"HarmonyIDs externos detectados={num7}{arg}, " + $"Parches totales before/after = {num}/{num8}"));
		}
		catch (Exception ex3)
		{
			Debug.LogWarning((object)("[DevLoader] LoadAll error: " + ex3));
		}
                finally
                {
                        stopwatch.Stop();
                        Debug.Log((object)$"[DevLoader] LoadAll() total = {stopwatch.ElapsedMilliseconds} ms");
                        _isLoadingOrUnloading = false;
                }
        }

        private static Assembly FindLoadedAssembly(string dllPath)
        {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(dllPath);
                if (string.IsNullOrEmpty(fileNameWithoutExtension))
                {
                        return null;
                }
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                        AssemblyName name = assembly.GetName();
                        if (string.Equals(name.Name, fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase) || string.Equals(name.FullName, fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase))
                        {
                                return assembly;
                        }
                }
                return null;
        }

        private static string GetModRootForDll(string dllPath, out string modInfoPath)
        {
                modInfoPath = string.Empty;
                string directoryName = Path.GetDirectoryName(dllPath) ?? string.Empty;
                if (string.IsNullOrEmpty(directoryName))
                {
                        return string.Empty;
                }
                string text = directoryName;
                while (!string.IsNullOrEmpty(text) && Directory.Exists(text))
                {
                        string text2 = Path.Combine(text, "mod_info.yaml");
                        if (File.Exists(text2))
                        {
                                modInfoPath = text2;
                                return text;
                        }
                        string directoryName2 = Path.GetDirectoryName(text);
                        if (string.IsNullOrEmpty(directoryName2) || string.Equals(directoryName2, text, StringComparison.OrdinalIgnoreCase))
                        {
                                break;
                        }
                        text = directoryName2;
                }
                if (File.Exists(Path.Combine(directoryName, "mod_info.yaml")))
                {
                        modInfoPath = Path.Combine(directoryName, "mod_info.yaml");
                }
                return directoryName;
        }

        private static void TryLoadModContent(string modRoot, string modInfoPath)
        {
                if (string.IsNullOrEmpty(modRoot) || !Directory.Exists(modRoot))
                {
                        Debug.Log((object)$"[DevLoader] Static content skipped: invalid mod root for dll ({modRoot})");
                        return;
                }
                if (string.IsNullOrEmpty(modInfoPath) || !File.Exists(modInfoPath))
                {
                        Debug.Log((object)$"[DevLoader] Static content skipped: no mod_info.yaml for {modRoot}");
                        return;
                }
                try
                {
                        Type type = AccessTools.TypeByName("KMod.Content");
                        if (type == null)
                        {
                                Debug.LogWarning((object)$"[DevLoader] Static content skipped: missing KMod.Content for {modRoot}");
                                return;
                        }
                        object obj = CreateContentInstance(type, modRoot, modInfoPath);
                        MethodInfo methodInfo = AccessTools.Method(type, "LoadAll") ?? AccessTools.Method(type, "Load");
                        if (methodInfo == null)
                        {
                                Debug.LogWarning((object)$"[DevLoader] Static content skipped: no Load/LoadAll on KMod.Content for {modRoot}");
                                return;
                        }
                        object target = methodInfo.IsStatic ? null : obj;
                        if (!methodInfo.IsStatic && obj == null)
                        {
                                Debug.LogWarning((object)$"[DevLoader] Static content skipped: unable to construct KMod.Content for {modRoot}");
                                return;
                        }
                        object[] array = BuildMethodArguments(methodInfo.GetParameters(), modRoot, modInfoPath);
                        if (array == null)
                        {
                                Debug.LogWarning((object)$"[DevLoader] Static content skipped: unsupported Load signature for {modRoot}");
                                return;
                        }
                        methodInfo.Invoke(target, array);
                        Debug.Log((object)$"[DevLoader] Static content preloaded for {modRoot}");
                }
                catch (Exception ex)
                {
                        Debug.LogWarning((object)$"[DevLoader] Static content preload failed for {modRoot}: {ex.Message}");
                }
        }

        private static object CreateContentInstance(Type contentType, string modRoot, string modInfoPath)
        {
                ConstructorInfo[] declaredConstructors = AccessTools.GetDeclaredConstructors(contentType);
                foreach (ConstructorInfo constructorInfo in declaredConstructors.OrderBy((ConstructorInfo c) => c.GetParameters().Length))
                {
                        object[] array = BuildConstructorArguments(constructorInfo.GetParameters(), modRoot, modInfoPath);
                        if (array == null)
                        {
                                continue;
                        }
                        try
                        {
                                return constructorInfo.Invoke(array);
                        }
                        catch
                        {
                        }
                }
                ConstructorInfo constructorInfo2 = AccessTools.Constructor(contentType, Type.EmptyTypes);
                if (constructorInfo2 != null)
                {
                        try
                        {
                                return constructorInfo2.Invoke(Array.Empty<object>());
                        }
                        catch
                        {
                        }
                }
                return null;
        }

        private static object[] BuildConstructorArguments(ParameterInfo[] parameters, string modRoot, string modInfoPath)
        {
                if (parameters.Length == 0)
                {
                        return Array.Empty<object>();
                }
                object[] array = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                        ParameterInfo parameterInfo = parameters[i];
                        Type parameterType = parameterInfo.ParameterType;
                        if (parameterType == typeof(string))
                        {
                                string text = parameterInfo.Name ?? string.Empty;
                                array[i] = (text.IndexOf("info", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("yaml", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("file", StringComparison.OrdinalIgnoreCase) >= 0) ? modInfoPath : modRoot;
                                continue;
                        }
                        if (parameterType == typeof(bool))
                        {
                                array[i] = false;
                                continue;
                        }
                        return null;
                }
                return array;
        }

        private static object[] BuildMethodArguments(ParameterInfo[] parameters, string modRoot, string modInfoPath)
        {
                if (parameters.Length == 0)
                {
                        return Array.Empty<object>();
                }
                object[] array = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                        ParameterInfo parameterInfo = parameters[i];
                        Type parameterType = parameterInfo.ParameterType;
                        if (parameterType == typeof(string))
                        {
                                string text = parameterInfo.Name ?? string.Empty;
                                array[i] = (text.IndexOf("info", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("yaml", StringComparison.OrdinalIgnoreCase) >= 0 || text.IndexOf("file", StringComparison.OrdinalIgnoreCase) >= 0) ? modInfoPath : modRoot;
                                continue;
                        }
                        if (parameterType == typeof(bool))
                        {
                                array[i] = true;
                                continue;
                        }
                        if (parameterType == typeof(string[]))
                        {
                                array[i] = Array.Empty<string>();
                                continue;
                        }
                        return null;
                }
                return array;
        }

        public static void UnloadAll()
        {
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (_isLoadingOrUnloading)
		{
			Debug.Log((object)"[DevLoader] UnloadAll() ignorado: ya en curso.");
			return;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		_isLoadingOrUnloading = true;
		try
		{
			int num = CountAllPatches();
			int num2 = 0;
			foreach (string ourHarmonyId in _ourHarmonyIds)
			{
				try
				{
					new Harmony(ourHarmonyId).UnpatchAll(ourHarmonyId);
					num2++;
				}
				catch (Exception ex)
				{
					Debug.LogWarning((object)("[DevLoader] Unpatch error (our " + ourHarmonyId + "): " + ex.Message));
				}
			}
			_ourHarmonyIds.Clear();
			int num3 = 0;
			foreach (string foreignId in _foreignIds)
			{
				try
				{
					new Harmony(foreignId).UnpatchAll(foreignId);
					num3++;
				}
				catch (Exception ex2)
				{
					Debug.LogWarning((object)("[DevLoader] Unpatch error (ext " + foreignId + "): " + ex2.Message));
				}
			}
			_foreignIds.Clear();
			int num4 = 0;
			Marker[] array = Object.FindObjectsOfType<Marker>();
			Marker[] array2 = array;
			foreach (Marker marker in array2)
			{
				if ((Object)marker != null && (Object)((Component)marker).gameObject != null)
				{
					num4++;
					Object.Destroy((Object)((Component)marker).gameObject);
				}
			}
			Resources.UnloadUnusedAssets();
			int num5 = CountAllPatches();
			Debug.Log((object)$"[DevLoader] UnloadAll: unpatch ours={num2}, unpatch externals={num3}, objectsDestroyed={num4}, patches before/after={num}/{num5}");
		}
		catch (Exception ex3)
		{
			Debug.LogWarning((object)("[DevLoader] UnloadAll error: " + ex3));
		}
		finally
		{
			stopwatch.Stop();
			Debug.Log((object)$"[DevLoader] UnloadAll() total = {stopwatch.ElapsedMilliseconds} ms");
			_isLoadingOrUnloading = false;
		}
	}

	private static int CountAllPatches()
	{
		int num = 0;
		try
		{
			foreach (MethodBase allPatchedMethod in Harmony.GetAllPatchedMethods())
			{
				num++;
			}
		}
		catch
		{
		}
		return num;
	}

	private static HashSet<string> SnapshotHarmonyOwners()
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		try
		{
			foreach (MethodBase allPatchedMethod in Harmony.GetAllPatchedMethods())
			{
				Patches patchInfo = Harmony.GetPatchInfo(allPatchedMethod);
				if (patchInfo == null)
				{
					continue;
				}
				foreach (Patch prefix in patchInfo.Prefixes)
				{
					if (!string.IsNullOrEmpty(prefix?.owner))
					{
						hashSet.Add(prefix.owner);
					}
				}
				foreach (Patch postfix in patchInfo.Postfixes)
				{
					if (!string.IsNullOrEmpty(postfix?.owner))
					{
						hashSet.Add(postfix.owner);
					}
				}
				foreach (Patch transpiler in patchInfo.Transpilers)
				{
					if (!string.IsNullOrEmpty(transpiler?.owner))
					{
						hashSet.Add(transpiler.owner);
					}
				}
				foreach (Patch finalizer in patchInfo.Finalizers)
				{
					if (!string.IsNullOrEmpty(finalizer?.owner))
					{
						hashSet.Add(finalizer.owner);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)("[DevLoader] SnapshotHarmonyOwners error: " + ex.Message));
		}
		return hashSet;
	}

	private static int PatchAllInAssembly(Harmony h, Assembly asm)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Type[] types = asm.GetTypes();
		foreach (Type type in types)
		{
			try
			{
				if (type.GetCustomAttributes(typeof(HarmonyPatch), inherit: true).Any())
				{
					new PatchClassProcessor(h, type).Patch();
					num++;
				}
			}
			catch (ReflectionTypeLoadException ex)
			{
				Debug.LogWarning((object)("[DevLoader] TypeLoad error en " + asm.FullName + ": " + ex));
			}
			catch (Exception ex2)
			{
				Debug.LogWarning((object)("[DevLoader] Patch falló en " + type.FullName + ": " + ex2.Message));
			}
		}
		return num;
	}

	private static List<string> ResolveDevDirs()
	{
		List<string> list = new List<string>();
		try
		{
			string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
			string parent = Path.Combine(fullPath, "mods");
			string text = FindSubdirCaseInsensitive(parent, "dev");
                        if (System.IO.Directory.Exists(text))
			{
				list.Add(text);
			}
		}
		catch
		{
		}
		try
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string parent2 = Path.Combine(folderPath, "Klei", "OxygenNotIncluded", "mods");
			string text2 = FindSubdirCaseInsensitive(parent2, "dev");
                        if (System.IO.Directory.Exists(text2))
			{
				list.Add(text2);
			}
		}
		catch
		{
		}
		try
		{
			string persistentDataPath = Application.persistentDataPath;
                        string path = System.IO.Directory.GetParent(persistentDataPath)?.FullName ?? persistentDataPath;
			string parent3 = Path.Combine(path, "mods");
			string text3 = FindSubdirCaseInsensitive(parent3, "dev");
                        if (System.IO.Directory.Exists(text3))
			{
				list.Add(text3);
			}
		}
		catch
		{
		}
		return list.Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
	}

	private static string FindSubdirCaseInsensitive(string parent, string name)
	{
		try
		{
                        if (string.IsNullOrEmpty(parent) || !System.IO.Directory.Exists(parent))
			{
				return "";
			}
                        string[] directories = System.IO.Directory.GetDirectories(parent);
			string[] array = directories;
			foreach (string text in array)
			{
				string fileName = Path.GetFileName(text);
				if (string.Equals(fileName, name, StringComparison.OrdinalIgnoreCase))
				{
					return text;
				}
			}
			return Path.Combine(parent, name);
		}
		catch
		{
			return "";
		}
	}

	private static string SanitizeId(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return "devdll";
		}
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		foreach (char oldChar in invalidFileNameChars)
		{
			name = name.Replace(oldChar, '_');
		}
		return name.Replace(' ', '_').Replace('.', '_');
	}
}
