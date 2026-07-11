namespace Firaxis.CivTech.AssetObjects;

public class ProjectSpecificationDependency
{
	public string Name { get; set; }

	public string GUID { get; set; }

	public string Path { get; set; }

	public override string ToString()
	{
		return $"{Name} ({GUID}); {Path}";
	}
}
