using Firaxis.ATF;
using Sce.Atf;

namespace Firaxis.AssetEditing;

public interface IFireFXEditorContext : IEntityEditorContext, IObservableContext
{
	bool HasScript { get; }

	IFireFXScriptResource ScriptResource { get; }
}
