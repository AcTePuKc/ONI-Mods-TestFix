using PeterHan.PLib.UI;

namespace PeterHan.PLib.Options;

public interface IOptionsEntry : IOptionSpec
{
	void CreateUIEntry(PGridPanel parent, ref int row);

	void ReadFrom(object settings);

	void WriteTo(object settings);
}
