using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public class GridViewPropertyEditor : IPropertyEditor
{
	public SizeF GetDesiredSize(Graphics g, Font f)
	{
		return new SizeF(0f, 0f);
	}

	public Control GetEditingControl(PropertyEditorControlContext context)
	{
		if (context.LastSelectedObject == null)
		{
			return null;
		}
		if (!(context.GetValue() is IList<DomNode> source))
		{
			return null;
		}
		GridView gridView = new GridView();
		object[] selection = source.ToArray();
		gridView.EditingContext = new PropertyEditingContext(selection);
		SkinService.ApplyActiveSkin(gridView);
		return gridView;
	}
}
