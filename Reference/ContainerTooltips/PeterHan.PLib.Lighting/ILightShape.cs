namespace PeterHan.PLib.Lighting;

public interface ILightShape
{
	string Identifier { get; }

	LightShape KleiLightShape { get; }

	LightShape RayMode { get; }

	void FillLight(LightingArgs args);
}
