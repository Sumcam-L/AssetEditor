namespace Firaxis.CivTech.AssetPreviewer;

public class AttachmentPickResult : PickResult
{
	public readonly string Name;

	public AttachmentPickResult(float dist, string name)
		: base(dist)
	{
		Name = name;
	}
}
