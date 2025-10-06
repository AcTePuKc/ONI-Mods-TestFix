using System.Collections.Generic;

namespace PeterHan.PLib.Core;

public interface IPLibRegistry
{
	IDictionary<string, object> ModData { get; }

	void AddCandidateVersion(PForwardedComponent instance);

	PForwardedComponent GetLatestVersion(string id);

	object GetSharedData(string id);

	IEnumerable<PForwardedComponent> GetAllComponents(string id);

	void SetSharedData(string id, object data);
}
