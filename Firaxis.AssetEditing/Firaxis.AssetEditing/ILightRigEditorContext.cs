using Firaxis.ATF;
using Sce.Atf;

namespace Firaxis.AssetEditing;

public interface ILightRigEditorContext : IEntityEditorContext, IObservableContext
{
	IPropertyEditingListContext AnalyticLightContext { get; }

	IPropertyEditingListContext AnimationSetContext { get; }

	IPropertyEditingListContext EnvironmentLightContext { get; }
}
