namespace Firaxis.Controls;

public interface IExploreFileTree
{
	string BaseDirectory { get; set; }

	void RebuildTree();
}
