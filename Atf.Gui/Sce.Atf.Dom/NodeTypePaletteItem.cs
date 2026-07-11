namespace Sce.Atf.Dom;

public class NodeTypePaletteItem
{
	public readonly DomNodeType NodeType;

	public readonly string Name;

	public readonly string Description;

	public readonly string ImageName;

	public readonly string Category;

	public readonly string MenuText;

	public NodeTypePaletteItem(DomNodeType nodeType, string name, string description, string imageName)
	{
		NodeType = nodeType;
		Name = name;
		Description = description;
		ImageName = imageName;
	}

	public NodeTypePaletteItem(DomNodeType nodeType, string name, string description, string imageName, string category, string menuText)
	{
		NodeType = nodeType;
		Name = name;
		Description = description;
		ImageName = imageName;
		Category = category;
		MenuText = menuText;
	}
}
