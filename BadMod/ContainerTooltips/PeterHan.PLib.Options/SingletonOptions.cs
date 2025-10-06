namespace PeterHan.PLib.Options;

public abstract class SingletonOptions<T> where T : class, new()
{
	protected static T instance;

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = POptions.ReadSettings<T>() ?? new T();
			}
			return instance;
		}
		protected set
		{
			if (value != null)
			{
				instance = value;
			}
		}
	}
}
