using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Sce.Atf.Controls.PropertyEditing;

public static class WinFormsPropertyUtils
{
	private class StandardValuesUIEditor : UITypeEditor
	{
		private class StandardValuesListBox : ListBox
		{
			private readonly StandardValuesUIEditor m_editor;

			public StandardValuesListBox(StandardValuesUIEditor editor)
			{
				m_editor = editor;
				base.BorderStyle = BorderStyle.None;
				base.DrawMode = DrawMode.OwnerDrawVariable;
			}

			protected override void OnDrawItem(DrawItemEventArgs e)
			{
				e.DrawBackground();
				if (e.Index >= 0 && e.Index < base.Items.Count)
				{
					object value = base.Items[e.Index];
					string valueAsText = m_editor.GetValueAsText(value);
					Brush brush = ((e.Index == SelectedIndex) ? SystemBrushes.HighlightText : new SolidBrush(ForeColor));
					e.Graphics.DrawString(valueAsText, Font, brush, e.Bounds);
					if (brush != SystemBrushes.HighlightText)
					{
						brush.Dispose();
					}
				}
			}

			protected override void OnMeasureItem(MeasureItemEventArgs e)
			{
				e.ItemHeight++;
			}
		}

		private readonly TypeConverter m_converter;

		private IWindowsFormsEditorService m_editorService;

		public static bool CanCreateStandardValues(PropertyDescriptor descriptor, ITypeDescriptorContext context)
		{
			return descriptor.Converter?.GetStandardValuesSupported(context) ?? false;
		}

		public StandardValuesUIEditor(PropertyDescriptor descriptor)
		{
			m_converter = descriptor.Converter;
			if (m_converter == null)
			{
				throw new ArgumentException("descriptor has no Converter");
			}
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			m_editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
			if (m_editorService != null)
			{
				StandardValuesListBox standardValuesListBox = new StandardValuesListBox(this);
				standardValuesListBox.SelectedIndexChanged += listbox_SelectedIndexChanged;
				ICollection standardValues = m_converter.GetStandardValues(context);
				foreach (object item in standardValues)
				{
					if (!standardValuesListBox.Items.Contains(item))
					{
						standardValuesListBox.Items.Add(item);
					}
				}
				standardValuesListBox.SelectedItem = value;
				m_editorService.DropDownControl(standardValuesListBox);
				if (standardValuesListBox.SelectedItem != null)
				{
					return standardValuesListBox.SelectedItem;
				}
			}
			return value;
		}

		private void listbox_SelectedIndexChanged(object sender, EventArgs e)
		{
			m_editorService.CloseDropDown();
		}

		private string GetValueAsText(object value)
		{
			if (value == null)
			{
				return string.Empty;
			}
			if (value is string)
			{
				return (string)value;
			}
			if (m_converter != null && m_converter.CanConvertTo(typeof(string)))
			{
				return m_converter.ConvertToString(value);
			}
			return value.ToString();
		}
	}

	public static UITypeEditor GetUITypeEditor(PropertyDescriptor descriptor, ITypeDescriptorContext context)
	{
		UITypeEditor uITypeEditor = descriptor.GetEditor(typeof(UITypeEditor)) as UITypeEditor;
		if (uITypeEditor == null)
		{
			if (StandardValuesUIEditor.CanCreateStandardValues(descriptor, context))
			{
				uITypeEditor = new StandardValuesUIEditor(descriptor);
			}
			else
			{
				Type propertyType = descriptor.PropertyType;
				uITypeEditor = TypeDescriptor.GetEditor(propertyType, typeof(UITypeEditor)) as UITypeEditor;
			}
		}
		return uITypeEditor;
	}
}
