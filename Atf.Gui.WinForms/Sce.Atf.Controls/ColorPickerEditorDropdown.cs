using System.ComponentModel;
using System.Drawing.Design;

namespace Sce.Atf.Controls;

public class ColorPickerEditorDropdown : ColorPickerEditor
{
	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.DropDown;
	}
}
