using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;

namespace Firaxis.Controls;

public class DictionaryEditor : UITypeEditor
{
	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (value is IDictionary)
		{
			object obj = context.Instance;
			if (context.Instance is PropertyGridProxy)
			{
				obj = ((PropertyGridProxy)context.Instance).Object;
			}
			IDictionary descriptions = null;
			if (obj is IDictionaryDescriptionProvider)
			{
				descriptions = ((IDictionaryDescriptionProvider)obj).DescriptionValues;
			}
			DictionaryPropertyGridForm dictionaryPropertyGridForm = new DictionaryPropertyGridForm((IDictionary)value, descriptions);
			dictionaryPropertyGridForm.ShowDialog();
		}
		return value;
	}
}
