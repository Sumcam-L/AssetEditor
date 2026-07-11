using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace Firaxis.Controls;

public class SliderEditor : UITypeEditor
{
	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.DropDown;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService windowsFormsEditorService)
		{
			SliderEditorForm sliderEditorForm = new SliderEditorForm(int.Parse(value.ToString()));
			sliderEditorForm.WFES = windowsFormsEditorService;
			windowsFormsEditorService.DropDownControl(sliderEditorForm);
			value = (float)sliderEditorForm.BarValue;
		}
		return value;
	}
}
