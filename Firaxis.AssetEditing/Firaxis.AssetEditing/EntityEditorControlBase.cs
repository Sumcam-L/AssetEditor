using System.Windows.Forms;

namespace Firaxis.AssetEditing;

public abstract class EntityEditorControlBase : UserControl
{
	public abstract void Bind(IEntityEditorContext context);
}
