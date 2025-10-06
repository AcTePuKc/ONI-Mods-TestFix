namespace PeterHan.PLib.Options;

public interface IOptionSpec
{
	string Category { get; }

	string Format { get; }

	string Title { get; }

	string Tooltip { get; }
}
