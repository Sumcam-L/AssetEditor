using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using Firaxis.Reflection;

namespace Firaxis.Controls;

public class OpenFileEditor : UITypeEditor
{
	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (context == null || context.Instance == null || provider == null)
		{
			return base.EditValue(context, provider, value);
		}
		using (OpenFileDialog openFileDialog = new OpenFileDialog())
		{
			string text = value.ToString();
			if (!string.IsNullOrEmpty(text))
			{
				openFileDialog.InitialDirectory = Path.GetDirectoryName(text);
				openFileDialog.FileName = Path.GetFileName(text);
			}
			FilterAttribute filterAttribute = null;
			Type typeFromHandle = typeof(FilterAttribute);
			foreach (Attribute attribute in context.PropertyDescriptor.Attributes)
			{
				if (attribute.GetType() == typeFromHandle)
				{
					filterAttribute = (FilterAttribute)attribute;
					break;
				}
			}
			openFileDialog.Filter = ((filterAttribute != null) ? filterAttribute.Filter : "All Files (*.*)|*.*");
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				value = openFileDialog.FileName;
			}
		}
		return value;
	}
}
