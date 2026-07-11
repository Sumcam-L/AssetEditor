using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Sce.Atf.Controls.PropertyEditing;

public class FlagsUITypeEditor : UITypeEditor, IAnnotatedParams
{
	private string[] m_names;

	private string[] m_displayNames;

	private int[] m_values;

	private IWindowsFormsEditorService m_editorService;

	private static readonly string NoFlags = "(none)".Localize("No flags");

	public FlagsUITypeEditor()
	{
	}

	public FlagsUITypeEditor(string[] names)
	{
		DefineFlags(names);
	}

	public FlagsUITypeEditor(string[] names, int[] values)
	{
		DefineFlags(names, values);
	}

	public void DefineFlags(string[] definitions)
	{
		EnumUtil.ParseFlagDefinitions(definitions, out m_names, out m_displayNames, out m_values);
	}

	public void DefineFlags(string[] names, int[] values)
	{
		if (names == null || values == null || names.Length != values.Length)
		{
			throw new ArgumentException("names and/or values null, or of unequal length");
		}
		m_names = names;
		m_displayNames = names;
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
			CheckedListBox checkedListBox = new CheckedListBox();
			checkedListBox.CheckOnClick = true;
			string[] displayNames = m_displayNames;
			foreach (string item in displayNames)
			{
				checkedListBox.Items.Add(item);
			}
			using (Graphics graphics = checkedListBox.CreateGraphics())
			{
				float num = 0f;
				string[] displayNames2 = m_displayNames;
				foreach (string text in displayNames2)
				{
					float width = graphics.MeasureString(text, checkedListBox.Font).Width;
					num = Math.Max(num, width);
				}
				float num2 = m_displayNames.Length * checkedListBox.ItemHeight;
				int verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
				if (num2 > (float)(checkedListBox.Height - 4))
				{
					num += (float)SystemInformation.VerticalScrollBarWidth;
				}
				if (num > (float)checkedListBox.Width)
				{
					checkedListBox.Width = (int)num + 31;
				}
			}
			if (value is string)
			{
				FillCheckedListBoxFromString(value, checkedListBox);
			}
			else if (value is int)
			{
				FillCheckedListBoxFromInt(value, checkedListBox);
			}
			m_editorService.DropDownControl(checkedListBox);
			object obj = ((!(value is string)) ? ExtractIntFromCheckedListBox(checkedListBox) : ExtractStringFromCheckedListBox(checkedListBox));
			if (!obj.Equals(value))
			{
				value = obj;
			}
		}
		return value;
	}

	public void Initialize(string[] parameters)
	{
		DefineFlags(parameters);
	}

	private object ExtractIntFromCheckedListBox(CheckedListBox checkedListBox)
	{
		CheckedListBox.CheckedIndexCollection checkedIndices = checkedListBox.CheckedIndices;
		int num = 0;
		foreach (int item in checkedIndices)
		{
			num |= m_values[item];
		}
		return num;
	}

	private object ExtractStringFromCheckedListBox(CheckedListBox checkedListBox)
	{
		CheckedListBox.CheckedIndexCollection checkedIndices = checkedListBox.CheckedIndices;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (int item in checkedIndices)
		{
			stringBuilder.Append(m_names[item]);
			stringBuilder.Append("|");
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Length--;
		}
		else
		{
			stringBuilder.Append(NoFlags);
		}
		return stringBuilder.ToString();
	}

	private void FillCheckedListBoxFromInt(object value, CheckedListBox listBox)
	{
		int num = (int)value;
		for (int i = 0; i < m_values.Length; i++)
		{
			bool value2 = (num & m_values[i]) == m_values[i];
			listBox.SetItemChecked(i, value2);
		}
	}

	private void FillCheckedListBoxFromString(object obj, CheckedListBox listBox)
	{
		string text = obj.ToString();
		if (text == NoFlags)
		{
			return;
		}
		char[] separator = new char[4] { '|', ';', ',', ':' };
		string[] array = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			for (int j = 0; j < m_names.Length; j++)
			{
				if (text2 == m_names[j])
				{
					listBox.SetItemChecked(j, value: true);
					break;
				}
			}
		}
	}
}
