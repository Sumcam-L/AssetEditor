using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class GridControl : UserControl
{
	private readonly ToolStripDropDownButton m_propertyOrganization;

	private readonly ToolStripButton m_propertyShowHideButton;

	private readonly ToolStrip m_toolStrip;

	private readonly GridView m_gridView;

	private readonly ToolStripAutoFitLabel m_descriptionLabel;

	private readonly ToolStripAutoFitTextBox m_patternTextBox;

	private readonly ToolStripComboBox m_toolStripPatternTextBox;

	private static readonly Image s_categoryImage = ResourceUtil.GetImage16(Resources.ByCategoryImage);

	private static readonly Image s_showHidePropertiesImage = ResourceUtil.GetImage16(Resources.CheckedItemsImage);

	public ToolStrip ToolStrip => m_toolStrip;

	public GridView GridView => m_gridView;

	public PropertySorting PropertySorting
	{
		get
		{
			return m_gridView.PropertySorting;
		}
		set
		{
			m_gridView.PropertySorting = value;
			UpdateToolstripItems();
		}
	}

	public string Settings
	{
		get
		{
			return m_gridView.Settings;
		}
		set
		{
			m_gridView.Settings = value;
			UpdateToolstripItems();
		}
	}

	public bool CanResetCurrent => m_gridView.CanResetCurrent;

	public override bool AllowDrop
	{
		set
		{
			base.AllowDrop = value;
			m_gridView.AllowDrop = value;
		}
	}

	public GridControl()
		: this(PropertyGridMode.PropertySorting, new GridView())
	{
	}

	public GridControl(PropertyGridMode mode)
		: this(mode, new GridView())
	{
	}

	public GridControl(PropertyGridMode mode, PropertyCategorySettings catSet)
		: this(mode, new GridView(catSet))
	{
	}

	public GridControl(PropertyGridMode mode, GridView gridView)
	{
		m_gridView = gridView;
		m_gridView.BackColor = SystemColors.Window;
		m_gridView.Dock = DockStyle.Fill;
		m_gridView.EditingContextChanged += gridView_BindingChanged;
		m_gridView.MouseUp += gridView_MouseUp;
		m_gridView.DragOver += gridView_DragOver;
		m_gridView.DragDrop += gridView_DragDrop;
		m_gridView.MouseHover += gridView_MouseHover;
		m_gridView.MouseLeave += gridView_MouseLeave;
		m_gridView.SelectedPropertyChanged += gridView_SelectedPropertyChanged;
		m_toolStrip = new ToolStrip();
		m_toolStrip.GripStyle = ToolStripGripStyle.Hidden;
		m_toolStrip.Dock = DockStyle.Top;
		if ((mode & PropertyGridMode.PropertySorting) != 0)
		{
			m_propertyOrganization = new ToolStripDropDownButton(null, s_categoryImage);
			m_propertyOrganization.ToolTipText = "Property Organization".Localize("Could be rephrased as 'How do you want these properties to be organized?'");
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
		if ((mode & PropertyGridMode.ShowHideProperties) != 0)
		{
			m_propertyShowHideButton = new ToolStripButton(null, s_showHidePropertiesImage);
			m_propertyShowHideButton.ToolTipText = "Property Show / Hide".Localize();
			m_propertyShowHideButton.Click += propertyShowHide_Click;
			m_toolStrip.Items.Add(m_propertyShowHideButton);
			m_toolStrip.Items.Add(new ToolStripSeparator());
		}
		if ((mode & PropertyGridMode.DisableDragDropColumnHeaders) != 0)
		{
			m_gridView.DragDropColumnsEnabed = false;
		}
		if ((mode & PropertyGridMode.DisplayDescriptions) != 0)
		{
			m_descriptionLabel = new ToolStripAutoFitLabel();
			m_descriptionLabel.TextAlign = ContentAlignment.TopLeft;
			m_descriptionLabel.MaximumWidth = 5000;
			m_toolStrip.Items.Add(m_descriptionLabel);
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
			m_patternTextBox.KeyUp += m_filterTextBox_TextChanged;
			m_patternTextBox.TextBox.PreviewKeyDown += m_patternTextBox_PreviewKeyDown;
			m_toolStripPatternTextBox = new ToolStripComboBox();
			m_toolStripPatternTextBox.Dock = DockStyle.Left;
			m_toolStripPatternTextBox.TextChanged += m_toolStripPatternTextBox_SelectedValueChanged;
			ToolStripButton toolStripButton2 = new ToolStripButton
			{
				DisplayStyle = ToolStripItemDisplayStyle.Image,
				Image = ResourceUtil.GetImage16(Resources.DeleteImage)
			};
			toolStripButton.ImageTransparentColor = Color.Magenta;
			toolStripButton2.Name = "PropertyGridClearSearchButton";
			toolStripButton2.Size = new Size(29, 22);
			toolStripButton2.Text = "Clear Search".Localize("'Clear' is a verb");
			toolStripButton2.Click += ClearSearchButton_Click;
			m_toolStrip.Items.AddRange(new ToolStripItem[4] { toolStripButton, m_patternTextBox, m_toolStripPatternTextBox, toolStripButton2 });
		}
		SuspendLayout();
		base.Controls.Add(m_gridView);
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
		Font = new Font("Segoe UI", 9f);
		ResumeLayout(performLayout: false);
		PerformLayout();
	}

	private void ClearSearchButton_Click(object sender, EventArgs e)
	{
		m_patternTextBox.Text = string.Empty;
		FilteringContext filteringContext = m_gridView.EditingContext as FilteringContext;
		filteringContext.FilterValue = m_patternTextBox.Text;
	}

	private void m_toolStripPatternTextBox_SelectedValueChanged(object sender, EventArgs e)
	{
		FilteringContext filteringContext = m_gridView.EditingContext as FilteringContext;
		PropertyDescriptor filterProperty = filteringContext.PropertyDescriptors.FirstOrDefault((PropertyDescriptor pDesc) => pDesc.DisplayName == m_toolStripPatternTextBox.SelectedItem?.ToString());
		filteringContext.FilterProperty = filterProperty;
	}

	private void m_filterTextBox_TextChanged(object sender, EventArgs e)
	{
		FilteringContext filteringContext = m_gridView.EditingContext as FilteringContext;
		filteringContext.FilterValue = m_patternTextBox.Text;
	}

	private void m_patternTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		if (e.KeyData == Keys.Escape)
		{
			ClearSearchButton_Click(sender, e);
		}
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
		m_gridView.EditingContext = context;
		if (m_toolStripPatternTextBox != null && m_gridView.EditingContext is FilteringContext)
		{
			m_toolStripPatternTextBox.Items.Clear();
			foreach (PropertyDescriptor propertyDescriptor in context.PropertyDescriptors)
			{
				m_toolStripPatternTextBox.Items.Add(propertyDescriptor.DisplayName);
			}
			m_toolStripPatternTextBox.Visible = m_toolStripPatternTextBox.Items.Count > 0;
			(m_gridView.EditingContext as FilteringContext).FilterValue = m_patternTextBox.Text;
		}
		SkinService.ApplyActiveSkin(this);
	}

	public void ResetCurrent()
	{
		m_gridView.ResetCurrent();
	}

	public void ResetAll()
	{
		m_gridView.ResetAll();
	}

	public void RefreshProperties()
	{
		m_gridView.RefreshProperties();
	}

	public PropertyDescriptor GetDescriptorAt(Point clientPoint)
	{
		return m_gridView.GetDescriptorAt(clientPoint);
	}

	private void gridView_BindingChanged(object sender, EventArgs e)
	{
		UpdateDescription();
	}

	private void gridView_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			Point p = m_gridView.PointToScreen(new Point(e.X, e.Y));
			p = PointToClient(p);
			base.OnMouseUp(new MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta));
		}
	}

	private void gridView_DragDrop(object sender, DragEventArgs e)
	{
		OnDragDrop(e);
	}

	private void gridView_DragOver(object sender, DragEventArgs e)
	{
		OnDragOver(e);
	}

	private void gridView_MouseHover(object sender, EventArgs e)
	{
		OnMouseHover(e);
	}

	private void gridView_MouseLeave(object sender, EventArgs e)
	{
		OnMouseLeave(e);
	}

	private void organization_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
	{
		m_gridView.PropertySorting = (PropertySorting)e.ClickedItem.Tag;
		m_gridView.Invalidate();
		UpdateToolstripItems();
	}

	private void propertyShowHide_Click(object sender, EventArgs e)
	{
		if (m_gridView.GetColumnUserHiddenStates().Count > 0)
		{
			GridControlShowHidePropertiesDialog gridControlShowHidePropertiesDialog = new GridControlShowHidePropertiesDialog(m_gridView);
			gridControlShowHidePropertiesDialog.ShowIcon = false;
			gridControlShowHidePropertiesDialog.ShowInTaskbar = false;
			gridControlShowHidePropertiesDialog.MaximizeBox = false;
			gridControlShowHidePropertiesDialog.MinimizeBox = false;
			gridControlShowHidePropertiesDialog.ShowDialog();
		}
	}

	private void gridView_SelectedPropertyChanged(object sender, EventArgs e)
	{
		UpdateDescription();
	}

	private void UpdateDescription()
	{
		string text = string.Empty;
		PropertyDescriptor selectedPropertyDescriptor = m_gridView.SelectedPropertyDescriptor;
		if (selectedPropertyDescriptor != null)
		{
			text = selectedPropertyDescriptor.Description;
		}
		if (m_descriptionLabel != null)
		{
			m_descriptionLabel.Text = text;
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
			dropDownItem.Checked = (PropertySorting)dropDownItem.Tag == m_gridView.PropertySorting;
		}
	}
}
