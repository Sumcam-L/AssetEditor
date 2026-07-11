using System;
using System.ComponentModel;
using System.Drawing.Design;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.AssetPreviewing;

public class FunctionUITypeEditor : UITypeEditor
{
	private readonly IFunctionKnob FunctionKnob;

	public FunctionUITypeEditor(IFunctionKnob functionKnob)
	{
		FunctionKnob = functionKnob;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		FunctionKnob.CallFunction();
		return value;
	}
}
