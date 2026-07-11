using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public class BoundEnumUITypeEditor : UITypeEditor
{
	private IWindowsFormsEditorService m_editorService;

	private bool m_listBoxMouseDown;

	private IEnumParameter EnumParameter { get; set; }

	private string[] EnumTextArray { get; set; }

	public BoundEnumUITypeEditor(IEnumParameter enumParam)
	{
		EnumParameter = enumParam;
		EnumTextArray = Enumerable.Empty<string>().ToArray();
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
			IInstanceEntityAdapter enumeratorInstance = context.Instance.As<DomNodeAdapter>()?.DomNode?.GetRoot()?.As<IInstanceEntityAdapter>();
			using ((EnumParameter is IDynamicEnum dynamicEnum) ? dynamicEnum.SetEnumeratorInstance(enumeratorInstance) : null)
			{
				EnumTextArray = EnumParameter.GetEnumerations().ToArray();
			}
			int num = 0;
			string[] enumTextArray = EnumTextArray;
			foreach (string text in enumTextArray)
			{
				listBox.Items.Add(text);
				if (text.Equals(value) || num.Equals(value))
				{
					listBox.SelectedIndex = num;
				}
				num++;
			}
			using (Graphics graphics = listBox.CreateGraphics())
			{
				float num2 = 0f;
				enumTextArray = EnumTextArray;
				foreach (string text2 in enumTextArray)
				{
					float width = graphics.MeasureString(text2, listBox.Font).Width;
					num2 = System.Math.Max(num2, width);
				}
				if ((float)(EnumTextArray.Length * listBox.ItemHeight) > (float)(listBox.Height - 4))
				{
					num2 += (float)SystemInformation.VerticalScrollBarWidth;
				}
				if (num2 > (float)listBox.Width)
				{
					listBox.Width = (int)num2 + 6;
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
				obj = ((!(value is int)) ? EnumTextArray[selectedIndex] : ((object)selectedIndex));
				if (!obj.Equals(value))
				{
					value = obj;
				}
			}
		}
		return value;
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
