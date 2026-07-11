using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class QueryTextInput : QueryNode
{
	private ToolStripTextBox m_toolStripSpringTextBox;

	private readonly bool m_numericalTextInput;

	public string InputText => ToolStripTextBox.Text;

	private ToolStripTextBox ToolStripTextBox
	{
		get
		{
			if (m_toolStripSpringTextBox == null)
			{
				m_toolStripSpringTextBox = new ToolStripTextBox();
				m_toolStripSpringTextBox.Name = "toolStripTextBox1";
				m_toolStripSpringTextBox.Size = new Size(100, 25);
				m_toolStripSpringTextBox.KeyDown += textBox_KeyDown;
				m_toolStripSpringTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
				m_toolStripSpringTextBox.BorderStyle = BorderStyle.FixedSingle;
				m_toolStripSpringTextBox.TextBoxTextAlign = HorizontalAlignment.Center;
				m_toolStripSpringTextBox.Margin = new Padding(6, 2, 6, 2);
				m_toolStripSpringTextBox.TextChanged += ToolStripSpringTextBoxOnTextChanged;
			}
			return m_toolStripSpringTextBox;
		}
	}

	public event EventHandler TextEntered;

	public event EventHandler TextChanged;

	private QueryTextInput()
	{
		m_numericalTextInput = false;
	}

	public QueryTextInput(bool numericalTextInput)
	{
		m_numericalTextInput = numericalTextInput;
	}

	private void ToolStripSpringTextBoxOnTextChanged(object sender, EventArgs eventArgs)
	{
		this.TextChanged.Raise(this, EventArgs.Empty);
	}

	public override void GetToolStripItems(List<ToolStripItem> items)
	{
		items.Add(ToolStripTextBox);
	}

	public override ToolStripItem GetToolStripItem()
	{
		return ToolStripTextBox;
	}

	private void textBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (m_numericalTextInput)
		{
			ValidateNumericalTextInput(e);
		}
		else
		{
			ValidateStringTextInput(e);
		}
	}

	private void ValidateStringTextInput(KeyEventArgs e)
	{
		Keys keyCode = e.KeyCode;
		if (keyCode == Keys.Return)
		{
			this.TextEntered.Raise(this, EventArgs.Empty);
		}
	}

	private void ValidateNumericalTextInput(KeyEventArgs e)
	{
		bool flag = false;
		switch (e.KeyCode)
		{
		case Keys.Return:
			this.TextEntered.Raise(this, EventArgs.Empty);
			flag = false;
			break;
		case Keys.Back:
		case Keys.Left:
		case Keys.Up:
		case Keys.Right:
		case Keys.Down:
		case Keys.Delete:
		case Keys.D0:
		case Keys.D1:
		case Keys.D2:
		case Keys.D3:
		case Keys.D4:
		case Keys.D5:
		case Keys.D6:
		case Keys.D7:
		case Keys.D8:
		case Keys.D9:
		case Keys.NumPad0:
		case Keys.NumPad1:
		case Keys.NumPad2:
		case Keys.NumPad3:
		case Keys.NumPad4:
		case Keys.NumPad5:
		case Keys.NumPad6:
		case Keys.NumPad7:
		case Keys.NumPad8:
		case Keys.NumPad9:
			flag = false;
			break;
		case Keys.Decimal:
		case Keys.OemPeriod:
			flag = m_toolStripSpringTextBox.Text.Contains(".");
			break;
		case Keys.Subtract:
		case Keys.OemMinus:
			flag = m_toolStripSpringTextBox.Text.Length != 0;
			break;
		default:
			flag = true;
			break;
		}
		e.SuppressKeyPress = flag;
	}
}
