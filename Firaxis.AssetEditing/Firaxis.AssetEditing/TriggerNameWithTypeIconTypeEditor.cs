using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class TriggerNameWithTypeIconTypeEditor : IPropertyEditor
{
	public SizeF GetDesiredSize(Graphics g, Font f)
	{
		return SizeF.Empty;
	}

	public Control GetEditingControl(PropertyEditorControlContext context)
	{
		TriggerNameAndTypeControl result = new TriggerNameAndTypeControl(context);
		SkinService.ApplyActiveSkin(context);
		return result;
	}
}
