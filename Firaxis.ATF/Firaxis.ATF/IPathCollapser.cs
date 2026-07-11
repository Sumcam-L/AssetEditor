namespace Firaxis.ATF;

public interface IPathCollapser
{
	string RelativePath { get; }

	bool AddRootPantry(string pantry);

	void Reload();

	bool RemoveRootPantry(string pantry);
}
