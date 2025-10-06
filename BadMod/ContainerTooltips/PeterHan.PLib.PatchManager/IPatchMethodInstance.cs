using HarmonyLib;

namespace PeterHan.PLib.PatchManager;

public interface IPatchMethodInstance
{
	void Run(Harmony instance);
}
