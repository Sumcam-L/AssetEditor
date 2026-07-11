using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.PropertyEditing;

public interface IPropertyEditor
{
	Control GetEditingControl(PropertyEditorControlContext context);

	SizeF GetDesiredSize(Graphics g, Font f);
}
