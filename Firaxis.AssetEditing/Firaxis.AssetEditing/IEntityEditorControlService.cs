using Firaxis.ATF;

namespace Firaxis.AssetEditing;

public interface IEntityEditorControlService
{
	EntityEditorControlBase CreateControl(IEntityDocument entDoc);
}
