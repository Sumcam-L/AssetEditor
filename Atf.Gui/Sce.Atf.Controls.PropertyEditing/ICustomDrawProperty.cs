using System.ComponentModel;
using System.Drawing;

namespace Sce.Atf.Controls.PropertyEditing;

public interface ICustomDrawProperty
{
	void DrawValue(Graphics g, Font cellFont, Brush cellBrush, Rectangle valueRect, PropertyDescriptor valueProp, object valueObj, bool isSelected);
}
