namespace PeterHan.PLib.PatchManager;

public static class RunAt
{
	public const uint Immediately = 0u;

	public const uint AfterModsLoad = 1u;

	public const uint BeforeDbInit = 2u;

	public const uint AfterDbInit = 3u;

	public const uint InMainMenu = 4u;

	public const uint OnStartGame = 5u;

	public const uint OnEndGame = 6u;

	public const uint AfterLayerableLoad = 7u;

	public const uint BeforeDbPostProcess = 8u;

	public const uint AfterDbPostProcess = 9u;

	public const uint OnDetailsScreenInit = 10u;

	private static readonly string[] STRING_VALUES = new string[8] { "Immediately", "AfterModsLoad", "BeforeDbInit", "AfterDbInit", "InMainMenu", "OnStartGame", "OnEndGame", "AfterLayerableLoad" };

	public static string ToString(uint runtime)
	{
		if (runtime >= STRING_VALUES.Length)
		{
			return runtime.ToString();
		}
		return STRING_VALUES[runtime];
	}
}
