using System.Reflection;

namespace PeterHan.PLib.PatchManager;

internal interface IPLibAnnotation
{
	uint Runtime { get; }

	IPatchMethodInstance CreateInstance(MethodInfo method);
}
