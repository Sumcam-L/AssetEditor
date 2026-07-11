using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications;

public interface IControlInfo
{
	string Name { get; set; }

	string Description { get; set; }

	object ImageSourceKey { get; set; }

	string Id { get; }

	StandardControlGroup Group { get; }

	IControlHostClient Client { get; }

	object Content { get; }
}
