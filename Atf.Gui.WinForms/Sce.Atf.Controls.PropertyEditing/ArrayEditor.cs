using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class ArrayEditor : IPropertyEditor
{
	public Control GetEditingControl(PropertyEditorControlContext context)
	{
		if (context.LastSelectedObject == null)
		{
			return null;
		}
		ArrayEditingControl arrayEditingControl = new ArrayEditingControl(context);
		SkinService.ApplyActiveSkin(arrayEditingControl);
		return arrayEditingControl;
	}

	public SizeF GetDesiredSize(Graphics g, Font f)
	{
		return new SizeF(0f, 0f);
	}
}
