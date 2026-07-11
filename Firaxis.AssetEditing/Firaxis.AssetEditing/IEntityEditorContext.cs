using Firaxis.ATF;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

public interface IEntityEditorContext : IObservableContext
{
	IPropertyEditingListContext CookParametersContext { get; }

	IPropertyEditingContext EntityContext { get; }

	bool HasCookParameters { get; }
}
