using KMod;

namespace PeterHan.PLib.AVC;

public interface IModVersionChecker
{
	event PVersionCheck.OnVersionCheckComplete OnVersionCheckCompleted;

	bool CheckVersion(Mod mod);
}
