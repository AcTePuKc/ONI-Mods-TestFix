using System.Collections.Generic;

namespace PeterHan.PLib.Options;

public interface IOptions
{
	IEnumerable<IOptionsEntry> CreateOptions();

	void OnOptionsChanged();
}
