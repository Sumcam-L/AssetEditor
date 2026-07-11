using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public class StringSearchInputUI : ToolStrip
{
	private readonly ToolStripAutoFitTextBox m_patternTextBox;

	private string m_patternTextRegex;

	private bool m_textBoxEmpty = true;

	public string SearchPattern => m_patternTextBox.Text;

	public event EventHandler Updated;

	public StringSearchInputUI()
	{
		m_patternTextRegex = string.Empty;
		base.Visible = true;
		base.GripStyle = ToolStripGripStyle.Hidden;
		base.RenderMode = ToolStripRenderMode.System;
		ToolStripDropDownButton toolStripDropDownButton = new ToolStripDropDownButton();
		toolStripDropDownButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
		toolStripDropDownButton.Image = ResourceUtil.GetImage16(Resources.SearchImage);
		toolStripDropDownButton.ImageTransparentColor = Color.Magenta;
		toolStripDropDownButton.Name = "SearchButton";
		toolStripDropDownButton.Size = new Size(29, 22);
		toolStripDropDownButton.Text = "Search".Localize("'Search' is a verb");
		ToolStripButton toolStripButton = new ToolStripButton
		{
			DisplayStyle = ToolStripItemDisplayStyle.Image,
			Image = ResourceUtil.GetImage16(Resources.DeleteImage)
		};
		toolStripDropDownButton.ImageTransparentColor = Color.Magenta;
		toolStripButton.Name = "ClearSearchButton";
		toolStripButton.Size = new Size(29, 22);
		toolStripButton.Text = "Clear Search".Localize("'Clear' is a verb");
		toolStripButton.Click += clearSearchButton_Click;
		m_patternTextBox = new ToolStripAutoFitTextBox();
		m_patternTextBox.KeyUp += patternTextBox_KeyUp;
		m_patternTextBox.TextChanged += patternTextBox_TextChanged;
		m_patternTextBox.TextBox.PreviewKeyDown += textBox_PreviewKeyDown;
		m_patternTextBox.MaximumWidth = 1080;
		Items.AddRange(new ToolStripItem[3] { toolStripDropDownButton, m_patternTextBox, toolStripButton });
	}

	public bool IsNullOrEmpty()
	{
		return m_textBoxEmpty;
	}

	public bool Matches(string inputString)
	{
		return Regex.Match(inputString, m_patternTextRegex, RegexOptions.IgnoreCase).Success;
	}

	public void ClearSearch()
	{
		m_patternTextBox.Text = string.Empty;
		m_patternTextRegex = string.Empty;
		this.Updated.Raise(this, null);
	}

	private void clearSearchButton_Click(object sender, EventArgs e)
	{
		ClearSearch();
	}

	private void patternTextBox_KeyUp(object sender, KeyEventArgs e)
	{
		m_patternTextRegex = string.Empty;
		if (!string.IsNullOrEmpty(m_patternTextBox.Text))
		{
			bool flag = true;
			m_patternTextRegex = m_patternTextBox.Text.Replace("*", "[\\w\\s]+");
			try
			{
				Regex.Match(string.Empty, m_patternTextRegex);
			}
			catch (ArgumentException)
			{
				flag = false;
			}
			m_patternTextRegex = (flag ? m_patternTextRegex : Regex.Escape(m_patternTextBox.Text));
		}
		this.Updated.Raise(this, null);
	}

	private void patternTextBox_TextChanged(object sender, EventArgs e)
	{
		m_textBoxEmpty = string.IsNullOrEmpty(m_patternTextBox.Text);
	}

	private void textBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		if (e.KeyData == Keys.Escape)
		{
			clearSearchButton_Click(sender, e);
		}
	}
}
