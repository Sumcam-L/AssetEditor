using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications;

public class ControlDef
{
	public string Name { get; set; }

	public string Description { get; set; }

	public object ImageSourceKey { get; set; }

	public string Id { get; set; }

	public StandardControlGroup Group { get; set; }
}
