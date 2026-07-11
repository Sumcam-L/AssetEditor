using System.ComponentModel;

namespace Firaxis.AssetPreviewing;

public class ControlPropertyEditorContext
{
	public readonly PropertyDescriptor Property;

	public readonly object Component;

	public ControlPropertyEditorContext(object comp, PropertyDescriptor prop)
	{
		Property = prop;
		Component = comp;
	}
}
