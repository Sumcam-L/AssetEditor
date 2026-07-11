using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class CollectionEditor : IPropertyEditor
{
	public Control GetEditingControl(PropertyEditorControlContext context)
	{
		CollectionEditingControl collectionEditingControl = new CollectionEditingControl(context);
		SkinService.ApplyActiveSkin(collectionEditingControl);
		return collectionEditingControl;
	}

	public SizeF GetDesiredSize(Graphics g, Font f)
	{
		return new SizeF(0f, 0f);
	}
}
