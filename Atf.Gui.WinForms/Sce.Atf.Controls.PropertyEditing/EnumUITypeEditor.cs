using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Sce.Atf.Controls.PropertyEditing;

public class EnumUITypeEditor : UITypeEditor, IAnnotatedParams
{
	private string[] m_names = EmptyArray<string>.Instance;

	private int[] m_values = EmptyArray<int>.Instance;

	private IWindowsFormsEditorService m_editorService;

	private bool m_listBoxMouseDown;

	public EnumUITypeEditor()
	{
	}

	public EnumUITypeEditor(string[] names)
	{
		DefineEnum(names);
	}

	public EnumUITypeEditor(string[] names, int[] values)
	{
		DefineEnum(names, values);
	}

	public void DefineEnum(string[] names)
	{
		EnumUtil.ParseEnumDefinitions(names, out m_names, out m_values);
	}

	public void DefineEnum(string[] names, int[] values)
	{
		if (names == null || values == null || names.Length != values.Length)
		{
			throw new ArgumentException("names and/or values null, or of unequal length");
		}
		m_names = names;
		m_values = values;
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
			ListBox listBox = new HotTrackListBox();
			for (int i = 0; i < m_names.Length; i++)
			{
				listBox.Items.Add(m_names[i]);
				if (m_names[i].Equals(value) || m_values[i].Equals(value))
				{
					listBox.SelectedIndex = i;
				}
			}
			using (Graphics graphics = listBox.CreateGraphics())
			{
				float num = 0f;
				string[] names = m_names;
				foreach (string text in names)
				{
					float width = graphics.MeasureString(text, listBox.Font).Width;
					num = Math.Max(num, width);
				}
				float num2 = m_names.Length * listBox.ItemHeight;
				if (num2 > (float)(listBox.Height - 4))
				{
					num += (float)SystemInformation.VerticalScrollBarWidth;
				}
				if (num > (float)listBox.Width)
				{
					listBox.Width = (int)num + 6;
				}
				int num3 = listBox.ItemHeight + listBox.Margin.Vertical * 2;
				int num4 = num3 * listBox.Items.Count;
				int num5 = num3 * 8;
				int num6 = num3 * 3;
				if (num4 > num5)
				{
					num4 = num5;
				}
				else if (num4 < num6)
				{
					num4 = num6;
				}
				listBox.Height = num4;
			}
			listBox.SelectedIndexChanged += listBox_SelectedIndexChanged;
			listBox.MouseDown += listBox_OnMouseDown;
			listBox.MouseUp += listBox_OnMouseUp;
			listBox.MouseLeave += listBox_OnMouseLeave;
			listBox.PreviewKeyDown += listBox_OnPreviewKeyDown;
			m_editorService.DropDownControl(listBox);
			int selectedIndex = listBox.SelectedIndex;
			if (selectedIndex >= 0)
			{
				object obj = null;
				obj = ((!(value is int)) ? m_names[selectedIndex] : ((object)m_values[selectedIndex]));
				if (!obj.Equals(value))
				{
					return obj;
				}
			}
		}
		return value;
	}

	public void Initialize(string[] parameters)
	{
		DefineEnum(parameters);
	}

	private void listBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_listBoxMouseDown)
		{
			m_listBoxMouseDown = false;
			m_editorService.CloseDropDown();
		}
	}

	private void listBox_OnMouseLeave(object sender, EventArgs e)
	{
		m_listBoxMouseDown = false;
	}

	private void listBox_OnMouseUp(object sender, MouseEventArgs e)
	{
		m_listBoxMouseDown = false;
	}

	private void listBox_OnMouseDown(object sender, MouseEventArgs e)
	{
		m_listBoxMouseDown = true;
	}

	private void listBox_OnPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		if (e.KeyData == Keys.Return || e.KeyData == Keys.Tab || e.KeyData == (Keys.Tab | Keys.Shift))
		{
			m_editorService.CloseDropDown();
		}
		else if (e.KeyData == Keys.Escape)
		{
			((ListBox)sender).ClearSelected();
			m_editorService.CloseDropDown();
		}
	}
}
