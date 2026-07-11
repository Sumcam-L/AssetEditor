using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Firaxis.Asset;

public class SoundEventTypeEditor : UITypeEditor
{
	private SoundEventForm dialog;

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (dialog == null)
		{
			dialog = new SoundEventForm();
		}
		dialog.SoundEventName = (string)value;
		if (dialog.ShowDialog() == DialogResult.OK)
		{
			return dialog.SoundEventName;
		}
		return value;
	}
}
