using System;
using System.ComponentModel;
using System.Windows.Forms.Design;
using Firaxis.Controls;

namespace Firaxis.AssetEditing;

public class LightIntensitySliderEditor : SliderEditor
{
	private readonly float m_lightIntensityMultiplier;

	public LightIntensitySliderEditor()
	{
		m_lightIntensityMultiplier = 10f;
	}

	public LightIntensitySliderEditor(float multiplier)
	{
		m_lightIntensityMultiplier = multiplier;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService windowsFormsEditorService)
		{
			SliderEditorForm sliderEditorForm = new SliderEditorForm((int)(float.Parse(value.ToString()) * m_lightIntensityMultiplier));
			sliderEditorForm.WFES = windowsFormsEditorService;
			windowsFormsEditorService.DropDownControl(sliderEditorForm);
			value = (float)sliderEditorForm.BarValue / m_lightIntensityMultiplier;
		}
		return value;
	}
}
