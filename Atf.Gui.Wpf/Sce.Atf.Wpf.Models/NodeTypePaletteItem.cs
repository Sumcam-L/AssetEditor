using Sce.Atf.Dom;

namespace Sce.Atf.Wpf.Models;

public class NodeTypePaletteItem
{
	public DomNodeType NodeType { get; private set; }

	public string Name { get; private set; }

	public string Description { get; private set; }

	public string Category { get; private set; }

	public object ImageKey { get; private set; }

	public DomNode Prototype { get; private set; }

	public NodeTypePaletteItem(DomNodeType nodeType, string name, string description, string category, object imageKey)
		: this(nodeType, name, description, category, imageKey, new DomNode(nodeType))
	{
	}

	public NodeTypePaletteItem(DomNodeType nodeType, string name, string description, string category, object imageKey, DomNode protoType)
	{
		NodeType = nodeType;
		Name = name;
		Category = category;
		Description = description;
		ImageKey = imageKey;
		Prototype = protoType;
	}
}
