using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class PropertyGrid : ScrollableControl
{
	private class DescriptionControl : Control
	{
		private string m_name;

		private string m_description;

		private Brush m_textBrush;

		private Font m_boldFont;

		private PropertyGrid m_propertyGrid;

		public Brush TextBrush
		{
			get
			{
				return m_textBrush;
			}
			set
			{
				DisposeTextBrush();
				m_textBrush = value;
			}
		}

		public DescriptionControl(PropertyGrid propertyGrid)
		{
			m_propertyGrid = propertyGrid;
			m_textBrush = new SolidBrush(SystemColors.WindowText);
			CreateBoldFont();
		}

		public void SetDescription(string name, string description)
		{
			m_name = name;
			m_description = description;
			if (m_propertyGrid.AutoSizeDescription)
			{
				if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(description))
				{
					m_propertyGrid.m_splitContainer.Panel2Collapsed = true;
				}
				else
				{
					m_propertyGrid.m_splitContainer.Panel2Collapsed = false;
					Size proposedSize = new Size(base.ClientSize.Width, int.MaxValue);
					proposedSize = TextRenderer.MeasureText(m_description, Font, proposedSize, TextFormatFlags.WordBreak);
					if (!string.IsNullOrEmpty(m_name))
					{
						proposedSize.Height += m_boldFont.Height + 2 + 12;
					}
					int val = (int)Math.Max(m_propertyGrid.m_splitContainer.Height - proposedSize.Height, 0.5f * (float)m_propertyGrid.m_splitContainer.Height);
					val = Math.Max(val, m_propertyGrid.m_splitContainer.Panel1MinSize);
					val = Math.Min(val, m_propertyGrid.m_splitContainer.Height - m_propertyGrid.m_splitContainer.Panel2MinSize);
					if (val >= 0)
					{
						m_propertyGrid.m_splitContainer.SplitterDistance = val;
					}
				}
				m_propertyGrid.m_splitContainer.Invalidate();
			}
			Invalidate();
		}

		public void ClearDescription()
		{
			SetDescription(null, null);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposeBoldFont();
				DisposeTextBrush();
			}
			base.Dispose(disposing);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			DisposeBoldFont();
			CreateBoldFont();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			RectangleF layoutRectangle = base.ClientRectangle;
			if (m_name != null && m_description != null)
			{
				e.Graphics.DrawString(m_name, m_boldFont, TextBrush, layoutRectangle);
				int num = m_boldFont.Height + 2;
				layoutRectangle.Y += num;
				layoutRectangle.Height -= num;
				e.Graphics.DrawString(m_description, Font, TextBrush, layoutRectangle);
			}
		}

		private void CreateBoldFont()
		{
			if (m_boldFont == null)
			{
				m_boldFont = new Font(Font, FontStyle.Bold);
			}
		}

		private void DisposeBoldFont()
		{
			if (m_boldFont != null)
			{
				m_boldFont.Dispose();
				m_boldFont = null;
			}
		}

		private void DisposeTextBrush()
		{
			if (m_textBrush != null)
			{
				m_textBrush.Dispose();
				m_textBrush = null;
			}
		}
	}

	private readonly SplitContainer m_splitContainer = new SplitContainer();

	private readonly ToolStripDropDownButton m_propertyOrganization;

	private readonly ToolStripButton m_navigateOut;

	private readonly ToolStrip m_toolStrip;

	private readonly PropertyGridView m_propertyGridView;

	private readonly DescriptionControl m_descriptionTextBox;

	private static readonly Image s_categoryImage = ResourceUtil.GetImage16(Resources.ByCategoryImage);

	private static readonly Image s_navigateOutImage = ResourceUtil.GetImage16(Resources.NavLeftImage);

	private readonly ToolStripAutoFitTextBox m_patternTextBox;

	private const TextFormatFlags m_TextFormatFlags = TextFormatFlags.WordBreak;

	public bool AutoSizeDescription { get; set; }

	public PropertyGridView PropertyGridView => m_propertyGridView;

	public PropertySorting PropertySorting
	{
		get
		{
			return m_propertyGridView.PropertySorting;
		}
		set
		{
			m_propertyGridView.PropertySorting = value;
			UpdateToolstripItems();
		}
	}

	public string Settings
	{
		get
		{
			return m_propertyGridView.Settings;
		}
		set
		{
			m_propertyGridView.Settings = value;
			UpdateToolstripItems();
		}
	}

	public ToolStrip ToolStrip => m_toolStrip;

	public bool CanResetCurrent => m_propertyGridView.CanResetCurrent;

	public PropertyDescriptor SelectedPropertyDescriptor
	{
		get
		{
			return m_propertyGridView.SelectedPropertyDescriptor;
		}
		set
		{
			m_propertyGridView.SelectProperty(value);
			if (m_descriptionTextBox != null)
			{
				if (value != null)
				{
					m_descriptionTextBox.SetDescription(value.DisplayName, value.Description);
				}
				else
				{
					m_descriptionTextBox.ClearDescription();
				}
			}
		}
	}

	public override bool AllowDrop
	{
		set
		{
			base.AllowDrop = value;
			m_propertyGridView.AllowDrop = value;
		}
	}

	public event DragEventHandler PropertyGridDragDrop
	{
		add
		{
			m_propertyGridView.DragDrop += value;
		}
		remove
		{
			m_propertyGridView.DragDrop -= value;
		}
	}

	public event DragEventHandler PropertyGridDragOver
	{
		add
		{
			m_propertyGridView.DragOver += value;
		}
		remove
		{
			m_propertyGridView.DragOver -= value;
		}
	}

	public event MouseEventHandler PropertyGridMouseDown
	{
		add
		{
			m_propertyGridView.MouseDown += value;
		}
		remove
		{
			m_propertyGridView.MouseDown -= value;
		}
	}

	public event EventHandler PropertyGridMouseHover
	{
		add
		{
			m_propertyGridView.MouseHover += value;
		}
		remove
		{
			m_propertyGridView.MouseHover -= value;
		}
	}

	public event EventHandler PropertyGridMouseLeave
	{
		add
		{
			m_propertyGridView.MouseLeave += value;
		}
		remove
		{
			m_propertyGridView.MouseLeave -= value;
		}
	}

	public event MouseEventHandler PropertyGridMouseUp
	{
		add
		{
			m_propertyGridView.MouseUp += value;
		}
		remove
		{
			m_propertyGridView.MouseUp -= value;
		}
	}

	public PropertyGrid()
		: this(PropertyGridMode.PropertySorting | PropertyGridMode.DisplayDescriptions, new PropertyGridView())
	{
	}

	public PropertyGrid(PropertyGridMode mode)
		: this(mode, new PropertyGridView())
	{
	}

	public PropertyGrid(PropertyGridMode mode, PropertyGridView propertyGridView)
	{
		m_propertyGridView = propertyGridView;
		m_propertyGridView.BackColor = SystemColors.Window;
		m_propertyGridView.Dock = DockStyle.Fill;
		m_propertyGridView.EditingContextChanged += propertyGrid_EditingContextChanged;
		m_propertyGridView.MouseUp += propertyGrid_MouseUp;
		m_propertyGridView.DragOver += propertyGrid_DragOver;
		m_propertyGridView.DragDrop += propertyGrid_DragDrop;
		m_propertyGridView.MouseHover += propertyGrid_MouseHover;
		m_propertyGridView.MouseLeave += propertyGrid_MouseLeave;
		m_propertyGridView.DescriptionSetter = delegate(PropertyDescriptor p)
		{
			if (m_descriptionTextBox != null)
			{
				if (p != null)
				{
					m_descriptionTextBox.SetDescription(p.DisplayName, p.Description);
				}
				else
				{
					m_descriptionTextBox.ClearDescription();
				}
			}
		};
		m_toolStrip = new ToolStrip();
		m_toolStrip.GripStyle = ToolStripGripStyle.Hidden;
		m_toolStrip.Dock = DockStyle.Top;
		if ((mode & PropertyGridMode.PropertySorting) != 0)
		{
			m_propertyOrganization = new ToolStripDropDownButton(null, s_categoryImage);
			m_propertyOrganization.ToolTipText = "Property Organization".Localize("Could be rephrased as 'How do you want these properties to be organized?'");
			m_propertyOrganization.ImageTransparentColor = Color.Magenta;
			m_propertyOrganization.DropDownItemClicked += organization_DropDownItemClicked;
			ToolStripMenuItem value = new ToolStripMenuItem("Unsorted".Localize())
			{
				Tag = PropertySorting.None
			};
			ToolStripMenuItem value2 = new ToolStripMenuItem("Alphabetical".Localize())
			{
				Tag = PropertySorting.Alphabetical
			};
			ToolStripMenuItem value3 = new ToolStripMenuItem("Categorized".Localize())
			{
				Tag = PropertySorting.Categorized
			};
			ToolStripMenuItem value4 = new ToolStripMenuItem("Categorized Alphabetical Properties".Localize())
			{
				Tag = (PropertySorting.Categorized | PropertySorting.Alphabetical)
			};
			ToolStripMenuItem value5 = new ToolStripMenuItem("Alphabetical Categories".Localize())
			{
				Tag = (PropertySorting.Categorized | PropertySorting.CategoryAlphabetical)
			};
			ToolStripMenuItem value6 = new ToolStripMenuItem("Alphabetical Categories And Properties".Localize())
			{
				Tag = PropertySorting.ByCategory
			};
			m_propertyOrganization.DropDownItems.Add(value);
			m_propertyOrganization.DropDownItems.Add(value2);
			m_propertyOrganization.DropDownItems.Add(value3);
			m_propertyOrganization.DropDownItems.Add(value4);
			m_propertyOrganization.DropDownItems.Add(value5);
			m_propertyOrganization.DropDownItems.Add(value6);
			m_toolStrip.Items.Add(m_propertyOrganization);
			m_toolStrip.Items.Add(new ToolStripSeparator());
		}
		if ((mode & PropertyGridMode.DisableSearchControls) == 0)
		{
			ToolStripButton toolStripButton = new ToolStripButton
			{
				DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
				Image = ResourceUtil.GetImage16(Resources.SearchImage),
				ImageTransparentColor = Color.Magenta,
				Name = "PropertyGridSearchButton",
				Size = new Size(29, 22),
				Text = "Search".Localize("'Search' is a verb"),
				Enabled = false
			};
			m_patternTextBox = new ToolStripAutoFitTextBox();
			m_patternTextBox.Name = "patternTextBox";
			m_patternTextBox.MaximumWidth = 1080;
			m_patternTextBox.KeyUp += patternTextBox_KeyUp;
			m_patternTextBox.TextBox.PreviewKeyDown += patternTextBox_PreviewKeyDown;
			ToolStripButton toolStripButton2 = new ToolStripButton
			{
				DisplayStyle = ToolStripItemDisplayStyle.Image,
				Image = ResourceUtil.GetImage16(Resources.DeleteImage)
			};
			toolStripButton.ImageTransparentColor = Color.Magenta;
			toolStripButton2.Name = "PropertyGridClearSearchButton";
			toolStripButton2.Size = new Size(29, 22);
			toolStripButton2.Text = "Clear Search".Localize("'Clear' is a verb");
			toolStripButton2.Click += clearSearchButton_Click;
			m_toolStrip.Items.AddRange(new ToolStripItem[3] { toolStripButton, m_patternTextBox, toolStripButton2 });
		}
		if ((mode & PropertyGridMode.HideResetAllButton) == 0)
		{
			ToolStripButton toolStripButton3 = new ToolStripButton();
			toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Image;
			toolStripButton3.Image = ResourceUtil.GetImage16(Resources.ResetImage);
			toolStripButton3.ImageTransparentColor = Color.Magenta;
			toolStripButton3.Name = "PropertyGridResetAllButton";
			toolStripButton3.Size = new Size(29, 22);
			toolStripButton3.ToolTipText = "Reset all properties".Localize();
			toolStripButton3.Click += delegate
			{
				ITransactionContext context = m_propertyGridView.EditingContext.As<ITransactionContext>();
				context.DoTransaction(delegate
				{
					ResetAll();
				}, "Reset All Properties".Localize("'Reset' is a verb and this is the name of a command"));
			};
			m_toolStrip.Items.Add(toolStripButton3);
		}
		if ((mode & PropertyGridMode.AllowEditingComposites) != 0)
		{
			m_navigateOut = new ToolStripButton(null, s_navigateOutImage, navigateOut_Click);
			m_navigateOut.Enabled = true;
			m_navigateOut.ToolTipText = "Navigate back to parent of selected object".Localize();
			m_toolStrip.Items.Add(m_navigateOut);
			m_propertyGridView.AllowEditingComposites = true;
		}
		SuspendLayout();
		if ((mode & PropertyGridMode.DisplayTooltips) != 0)
		{
			m_propertyGridView.AllowTooltips = true;
		}
		if ((mode & PropertyGridMode.DisplayDescriptions) == 0)
		{
			base.Controls.Add(m_propertyGridView);
		}
		else
		{
			m_splitContainer.Orientation = Orientation.Horizontal;
			m_splitContainer.BackColor = SystemColors.InactiveBorder;
			m_splitContainer.FixedPanel = FixedPanel.Panel2;
			m_splitContainer.SplitterWidth = 8;
			m_splitContainer.Dock = DockStyle.Fill;
			m_splitContainer.Panel1.Controls.Add(m_propertyGridView);
			m_descriptionTextBox = new DescriptionControl(this);
			m_descriptionTextBox.BackColor = SystemColors.Window;
			m_descriptionTextBox.Dock = DockStyle.Fill;
			m_splitContainer.Panel2.Controls.Add(m_descriptionTextBox);
			base.Controls.Add(m_splitContainer);
			m_propertyGridView.SelectedPropertyChanged += propertyGrid_SelectedRowChanged;
			m_descriptionTextBox.ClearDescription();
		}
		if (m_toolStrip.Items.Count > 0)
		{
			UpdateToolstripItems();
			base.Controls.Add(m_toolStrip);
		}
		else
		{
			m_toolStrip.Dispose();
			m_toolStrip = null;
		}
		base.Name = "PropertyGrid";
		Font = m_propertyGridView.Font;
		base.FontChanged += delegate
		{
			m_propertyGridView.Font = Font;
		};
		ResumeLayout(performLayout: false);
		PerformLayout();
	}

	public void Bind(object selectedObject)
	{
		object[] array = selectedObject as object[];
		if (array == null)
		{
			array = new object[1] { selectedObject };
		}
		Bind(new PropertyEditingContext(array));
	}

	public void Bind(IPropertyEditingContext context)
	{
		m_propertyGridView.EditingContext = context;
		SkinService.ApplyActiveSkin(m_propertyGridView);
	}

	public void ResetCurrent()
	{
		m_propertyGridView.ResetCurrent();
	}

	public void ResetAll()
	{
		m_propertyGridView.ResetAll();
	}

	public void RefreshProperties()
	{
		m_propertyGridView.Refresh();
	}

	public PropertyDescriptor GetDescriptorAt(Point clientPoint)
	{
		return m_propertyGridView.GetDescriptorAt(clientPoint);
	}

	public PropertyDescriptor GetDescriptorAt(Point clientPoint, out IPropertyEditingContext editingContext)
	{
		return m_propertyGridView.GetDescriptorAt(clientPoint, out editingContext);
	}

	public PropertyDescriptor GetDescriptorAt(Point clientPoint, out int bottom)
	{
		return m_propertyGridView.GetDescriptorAt(clientPoint, out bottom);
	}

	public void SetDescription(string name, string description)
	{
		m_descriptionTextBox.SetDescription(name, description);
	}

	public override void Refresh()
	{
		m_propertyGridView.Refresh();
		base.Refresh();
	}

	private void propertyGrid_EditingContextChanged(object sender, EventArgs e)
	{
		if (m_navigateOut != null)
		{
			m_navigateOut.Enabled = m_propertyGridView.CanNavigateBack;
		}
	}

	private void propertyGrid_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			OnMouseUp(e);
		}
	}

	private void propertyGrid_DragDrop(object sender, DragEventArgs e)
	{
		OnDragDrop(e);
	}

	private void propertyGrid_DragOver(object sender, DragEventArgs e)
	{
		OnDragOver(e);
	}

	private void propertyGrid_MouseHover(object sender, EventArgs e)
	{
		OnMouseHover(e);
	}

	private void propertyGrid_MouseLeave(object sender, EventArgs e)
	{
		OnMouseLeave(e);
	}

	private void propertyGrid_SelectedRowChanged(object sender, EventArgs e)
	{
		PropertyDescriptor selectedPropertyDescriptor = m_propertyGridView.SelectedPropertyDescriptor;
		if (selectedPropertyDescriptor != null)
		{
			m_descriptionTextBox.SetDescription(selectedPropertyDescriptor.DisplayName, selectedPropertyDescriptor.Description);
		}
		else
		{
			m_descriptionTextBox.ClearDescription();
		}
	}

	private void organization_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
	{
		m_propertyGridView.PropertySorting = (PropertySorting)e.ClickedItem.Tag;
		UpdateToolstripItems();
	}

	private void navigateOut_Click(object sender, EventArgs e)
	{
		m_propertyGridView.NavigateBack();
	}

	private void clearSearchButton_Click(object sender, EventArgs e)
	{
		m_patternTextBox.Text = string.Empty;
		m_propertyGridView.FilterPattern = m_patternTextBox.Text;
	}

	private void patternTextBox_KeyUp(object sender, KeyEventArgs e)
	{
		m_propertyGridView.FilterPattern = m_patternTextBox.Text;
	}

	private void patternTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		if (e.KeyData == Keys.Escape)
		{
			clearSearchButton_Click(sender, e);
		}
	}

	private void UpdateToolstripItems()
	{
		if (m_propertyOrganization == null)
		{
			return;
		}
		foreach (ToolStripMenuItem dropDownItem in m_propertyOrganization.DropDownItems)
		{
			dropDownItem.Checked = (PropertySorting)dropDownItem.Tag == m_propertyGridView.PropertySorting;
		}
	}
}
