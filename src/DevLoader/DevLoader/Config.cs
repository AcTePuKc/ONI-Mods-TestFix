namespace DevLoader;

public static class Config
{
	private static bool _enabled = true;

	public static bool Enabled => _enabled;

	public static void Set(bool value)
	{
		_enabled = value;
	}
}
