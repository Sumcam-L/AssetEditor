using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.Controls;

public class AutoCompletePopup : Form
{
	public class AutoCompleteEventArgs : EventArgs
	{
		private string sSelected;

		public string Selected => sSelected;

		public AutoCompleteEventArgs(string sSelected)
		{
			this.sSelected = sSelected;
		}
	}

	public delegate void AutoCompletePopupHandler(object sender, AutoCompleteEventArgs e);

	public delegate string FindStringToReplaceDelegate(string sFullText);

	public class Entry
	{
		private string m_sDisplayText;

		private string m_sReplaceText;

		private uint m_uiCategory;

		public string DisplayText => m_sDisplayText;

		public string ReplaceText => m_sReplaceText;

		public uint Category => m_uiCategory;

		public Entry(string sText)
		{
			m_sDisplayText = sText;
			m_sReplaceText = sText;
			m_uiCategory = 0u;
		}

		public Entry(string sDisplayText, string sReplaceText)
		{
			m_sDisplayText = sDisplayText;
			m_sReplaceText = sReplaceText;
			m_uiCategory = 0u;
		}

		public Entry(string sDisplayText, string sReplaceText, uint uiCategory)
		{
			m_sDisplayText = sDisplayText;
			m_sReplaceText = sReplaceText;
			m_uiCategory = uiCategory;
		}

		public override string ToString()
		{
			return m_sDisplayText;
		}
	}

	private bool m_bPopupOpen = false;

	private TextBoxBase m_TextBox = null;

	private List<Entry> m_EntryList = new List<Entry>();

	private bool m_bCaseInsensitive = true;

	private bool m_bInvertColorsForHighlight = false;

	private Brush m_BackcolorBrush = Brushes.White;

	private Dictionary<uint, Brush> m_CategoryBrushes = new Dictionary<uint, Brush>();

	public FindStringToReplaceDelegate FindReplacementSymbol = null;

	private IContainer components = null;

	private ListBox lstEntries;

	public bool PopupOpen
	{
		get
		{
			return m_bPopupOpen;
		}
		set
		{
			if (value)
			{
				if (!m_bPopupOpen)
				{
					OpenPopup();
				}
			}
			else if (m_bPopupOpen)
			{
				ClosePopup();
			}
		}
	}

	public TextBoxBase TextBox
	{
		set
		{
			DetachFromTextBox();
			m_TextBox = value;
			if (base.Visible)
			{
				AttachToTextBox();
				PositionToTextbox();
			}
		}
	}

	public List<Entry> Entries
	{
		set
		{
			m_EntryList = value;
			FilterList();
		}
	}

	public bool CaseInsensitive
	{
		get
		{
			return m_bCaseInsensitive;
		}
		set
		{
			m_bCaseInsensitive = value;
		}
	}

	public bool InvertColorsForHighlight
	{
		get
		{
			return m_bInvertColorsForHighlight;
		}
		set
		{
			m_bInvertColorsForHighlight = value;
		}
	}

	public event AutoCompletePopupHandler OnSelection;

	public AutoCompletePopup()
	{
		InitializeComponent();
		m_BackcolorBrush = new SolidBrush(lstEntries.BackColor);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			components?.Dispose();
			m_BackcolorBrush?.Dispose();
		}
		base.Dispose(disposing);
	}

	private void OpenPopup()
	{
		PositionToTextbox();
		Show();
		AttachToTextBox();
		m_bPopupOpen = true;
	}

	private void ClosePopup()
	{
		DetachFromTextBox();
		Hide();
		m_bPopupOpen = false;
	}

	public void SetCategoryBrush(uint uiCategory, Brush brush)
	{
		m_CategoryBrushes[uiCategory] = brush;
	}

	private void FilterList()
	{
		if (m_TextBox == null)
		{
			return;
		}
		string text = m_TextBox.Text;
		if (FindReplacementSymbol != null)
		{
			text = FindReplacementSymbol(m_TextBox.Text);
		}
		object selectedItem = lstEntries.SelectedItem;
		Graphics graphics = lstEntries.CreateGraphics();
		int num = 50;
		lstEntries.BeginUpdate();
		lstEntries.Items.Clear();
		if (text.Equals(string.Empty))
		{
			foreach (Entry entry in m_EntryList)
			{
				lstEntries.Items.Add(entry);
				SizeF sizeF = graphics.MeasureString(entry.ToString(), lstEntries.Font);
				if (sizeF.Width > (float)num)
				{
					num = (int)sizeF.Width;
				}
			}
		}
		else
		{
			foreach (Entry entry2 in m_EntryList)
			{
				if (entry2.ReplaceText.StartsWith(text, m_bCaseInsensitive, null))
				{
					lstEntries.Items.Add(entry2);
					SizeF sizeF2 = graphics.MeasureString(entry2.ToString(), lstEntries.Font, new SizeF(1000f, lstEntries.ItemHeight), StringFormat.GenericDefault);
					if (sizeF2.Width > (float)num)
					{
						num = (int)sizeF2.Width + 1;
					}
				}
			}
		}
		int num2 = lstEntries.Items.Count * lstEntries.ItemHeight + lstEntries.Margin.Vertical + 4;
		if (num2 > 256)
		{
			int verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
			base.Width = num + verticalScrollBarWidth + 12;
			base.Height = 256;
		}
		else
		{
			base.Width = num + 12;
			base.Height = num2;
		}
		if (selectedItem != null && lstEntries.Items.Contains(selectedItem))
		{
			lstEntries.SelectedItem = selectedItem;
		}
		else if (lstEntries.Items.Count > 0)
		{
			lstEntries.SelectedIndex = 0;
		}
		lstEntries.EndUpdate();
	}

	private void SelectionMade()
	{
		AutoCompletePopupHandler autoCompletePopupHandler = this.OnSelection;
		if (autoCompletePopupHandler != null)
		{
			Entry entry = (Entry)lstEntries.SelectedItem;
			if (entry != null)
			{
				autoCompletePopupHandler(this, new AutoCompleteEventArgs(entry.ReplaceText));
			}
		}
		ClosePopup();
	}

	public void TextBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode.Equals(Keys.Return))
		{
			SelectionMade();
		}
		else
		{
			if (lstEntries.Items.Count <= 0)
			{
				return;
			}
			if (e.KeyCode.Equals(Keys.Up))
			{
				if (lstEntries.SelectedIndex == 0)
				{
					lstEntries.SelectedIndex = lstEntries.Items.Count - 1;
				}
				else
				{
					lstEntries.SelectedIndex--;
				}
			}
			else if (e.KeyCode.Equals(Keys.Down))
			{
				if (lstEntries.SelectedIndex == lstEntries.Items.Count - 1)
				{
					lstEntries.SelectedIndex = 0;
				}
				else
				{
					lstEntries.SelectedIndex++;
				}
			}
		}
	}

	private void lstEntries_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode.Equals(Keys.Return))
		{
			SelectionMade();
		}
	}

	private void lstEntries_DoubleClick(object sender, EventArgs e)
	{
		SelectionMade();
	}

	private void TextBox_LostFocus(object sender, EventArgs e)
	{
		if (!lstEntries.Focused)
		{
			ClosePopup();
		}
	}

	private void lstEntries_LostFocus(object sender, EventArgs e)
	{
		if (m_TextBox == null || !m_TextBox.Focused)
		{
			ClosePopup();
		}
	}

	private void AttachToTextBox()
	{
		if (m_TextBox != null)
		{
			m_TextBox.Focus();
			m_TextBox.LostFocus += TextBox_LostFocus;
			m_TextBox.KeyDown += TextBox_KeyDown;
			m_TextBox.TextChanged += TextBox_TextChanged;
		}
	}

	private void TextBox_TextChanged(object sender, EventArgs e)
	{
		FilterList();
	}

	private void PositionToTextbox()
	{
		if (m_TextBox != null)
		{
			Point positionFromCharIndex = m_TextBox.GetPositionFromCharIndex(0);
			positionFromCharIndex.Y += m_TextBox.Height;
			base.Location = m_TextBox.PointToScreen(positionFromCharIndex);
		}
	}

	private void DetachFromTextBox()
	{
		if (m_TextBox != null)
		{
			m_TextBox.LostFocus -= TextBox_LostFocus;
			m_TextBox.KeyDown -= TextBox_KeyDown;
			m_TextBox.TextChanged -= TextBox_TextChanged;
		}
	}

	private void lstEntries_DrawItem(object sender, DrawItemEventArgs e)
	{
		if (e.Index == -1)
		{
			return;
		}
		object obj = lstEntries.Items[e.Index];
		Brush value = Brushes.Black;
		if (obj is Entry)
		{
			Entry entry = (Entry)obj;
			if (!m_CategoryBrushes.TryGetValue(entry.Category, out value))
			{
				value = Brushes.Black;
				m_CategoryBrushes[entry.Category] = Brushes.Black;
			}
		}
		if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
		{
			if (m_bInvertColorsForHighlight)
			{
				e.Graphics.FillRectangle(value, e.Bounds);
				e.Graphics.DrawString(obj.ToString(), e.Font, m_BackcolorBrush, e.Bounds, StringFormat.GenericDefault);
			}
			else
			{
				e.DrawBackground();
				e.Graphics.DrawString(obj.ToString(), e.Font, value, e.Bounds, StringFormat.GenericDefault);
			}
		}
		else
		{
			e.Graphics.FillRectangle(m_BackcolorBrush, e.Bounds);
			e.Graphics.DrawString(obj.ToString(), e.Font, value, e.Bounds, StringFormat.GenericDefault);
		}
	}

	private void InitializeComponent()
	{
		this.lstEntries = new System.Windows.Forms.ListBox();
		base.SuspendLayout();
		this.lstEntries.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lstEntries.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
		this.lstEntries.FormattingEnabled = true;
		this.lstEntries.IntegralHeight = false;
		this.lstEntries.Location = new System.Drawing.Point(0, 0);
		this.lstEntries.Name = "lstEntries";
		this.lstEntries.Size = new System.Drawing.Size(330, 327);
		this.lstEntries.Sorted = true;
		this.lstEntries.TabIndex = 0;
		this.lstEntries.DrawItem += new System.Windows.Forms.DrawItemEventHandler(lstEntries_DrawItem);
		this.lstEntries.Leave += new System.EventHandler(lstEntries_LostFocus);
		this.lstEntries.DoubleClick += new System.EventHandler(lstEntries_DoubleClick);
		this.lstEntries.KeyDown += new System.Windows.Forms.KeyEventHandler(lstEntries_KeyDown);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(330, 327);
		base.ControlBox = false;
		base.Controls.Add(this.lstEntries);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "AutoCompletePopup";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		base.ResumeLayout(false);
	}
}
