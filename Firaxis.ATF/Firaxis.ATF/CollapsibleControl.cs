using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.ATF;

public class CollapsibleControl : UserControl
{
	private Control m_childControl;

	private const int kClosed = 0;

	private const int kOpened = 1;

	private IContainer components;

	private Panel pnlTitle;

	private Button btnExpand;

	private ImageList imgList;

	private Label lblTitle;

	private Panel pnlEditor;

	private Panel pnlGrid;

	public bool Expanded
	{
		get
		{
			return btnExpand.ImageIndex == 1;
		}
		set
		{
			if (value != Expanded)
			{
				if (value)
				{
					Expand();
				}
				else
				{
					Collapse();
				}
			}
		}
	}

	public string Title
	{
		get
		{
			return lblTitle.Text;
		}
		set
		{
			lblTitle.Text = value;
		}
	}

	protected Control ChildControl
	{
		get
		{
			return m_childControl;
		}
		set
		{
			if (m_childControl != null)
			{
				pnlGrid.Controls.Remove(m_childControl);
			}
			m_childControl = value;
			pnlGrid.Controls.Add(m_childControl);
		}
	}

	public event EventHandler ExpansionChanged;

	public CollapsibleControl()
	{
		InitializeComponent();
		pnlEditor.Visible = false;
		pnlEditor.AutoSize = true;
		pnlEditor.AutoSizeMode = AutoSizeMode.GrowAndShrink;
	}

	public CollapsibleControl(Control child)
		: this()
	{
		ChildControl = child;
	}

	protected void RaiseExpansionChanged()
	{
		this.ExpansionChanged?.Invoke(this, new EventArgs());
	}

	private void btnExpand_Click(object sender, EventArgs e)
	{
		if (!Expanded)
		{
			Expand();
		}
		else
		{
			Collapse();
		}
	}

	private void Collapse()
	{
		btnExpand.ImageIndex = 0;
		pnlEditor.Visible = false;
		RaiseExpansionChanged();
	}

	private void CollapsibleControl_SizeChanged(object sender, EventArgs e)
	{
		UpdateChildControlSizes();
	}

	private void Expand()
	{
		btnExpand.ImageIndex = 1;
		pnlEditor.Visible = true;
		RaiseExpansionChanged();
	}

	private void UpdateChildControlSizes()
	{
		int num = base.ClientSize.Width;
		int num2 = base.ClientSize.Height;
		pnlEditor.Width = num;
		pnlGrid.Width = num;
		pnlTitle.Width = num;
		pnlGrid.Top = 0;
		if (!AutoSize)
		{
			pnlEditor.Height = num2 - pnlTitle.Height;
			pnlGrid.Height = pnlEditor.Height;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Firaxis.ATF.CollapsiblePropertyEditingListControl));
		this.pnlTitle = new System.Windows.Forms.Panel();
		this.lblTitle = new System.Windows.Forms.Label();
		this.btnExpand = new System.Windows.Forms.Button();
		this.imgList = new System.Windows.Forms.ImageList(this.components);
		this.pnlEditor = new System.Windows.Forms.Panel();
		this.pnlGrid = new System.Windows.Forms.Panel();
		this.pnlTitle.SuspendLayout();
		this.pnlEditor.SuspendLayout();
		base.SuspendLayout();
		this.pnlTitle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.pnlTitle.BackColor = System.Drawing.SystemColors.Control;
		this.pnlTitle.Controls.Add(this.lblTitle);
		this.pnlTitle.Controls.Add(this.btnExpand);
		this.pnlTitle.Location = new System.Drawing.Point(0, 0);
		this.pnlTitle.Margin = new System.Windows.Forms.Padding(0);
		this.pnlTitle.Name = "pnlTitle";
		this.pnlTitle.Size = new System.Drawing.Size(445, 30);
		this.pnlTitle.TabIndex = 0;
		this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.lblTitle.Location = new System.Drawing.Point(26, 6);
		this.lblTitle.Name = "lblTitle";
		this.lblTitle.Size = new System.Drawing.Size(406, 18);
		this.lblTitle.TabIndex = 1;
		this.lblTitle.Text = "Title";
		this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.btnExpand.FlatAppearance.BorderSize = 0;
		this.btnExpand.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnExpand.ImageIndex = 0;
		this.btnExpand.ImageList = this.imgList;
		this.btnExpand.Location = new System.Drawing.Point(3, 4);
		this.btnExpand.Name = "btnExpand";
		this.btnExpand.Size = new System.Drawing.Size(23, 23);
		this.btnExpand.TabIndex = 0;
		this.btnExpand.UseVisualStyleBackColor = true;
		this.btnExpand.Click += new System.EventHandler(btnExpand_Click);
		this.imgList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imgList.ImageStream");
		this.imgList.TransparentColor = System.Drawing.Color.Transparent;
		this.imgList.Images.SetKeyName(0, "closedChevron");
		this.imgList.Images.SetKeyName(1, "openedChevron");
		this.pnlEditor.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.pnlEditor.Controls.Add(this.pnlGrid);
		this.pnlEditor.Location = new System.Drawing.Point(0, 31);
		this.pnlEditor.Name = "pnlEditor";
		this.pnlEditor.Size = new System.Drawing.Size(445, 259);
		this.pnlEditor.TabIndex = 1;
		this.pnlGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.pnlGrid.BackColor = System.Drawing.SystemColors.Window;
		this.pnlGrid.Location = new System.Drawing.Point(0, 28);
		this.pnlGrid.Name = "pnlGrid";
		this.pnlGrid.Size = new System.Drawing.Size(445, 231);
		this.pnlGrid.TabIndex = 1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.Window;
		base.Controls.Add(this.pnlEditor);
		base.Controls.Add(this.pnlTitle);
		base.Name = "CollapsibleControl";
		base.Size = new System.Drawing.Size(445, 290);
		base.SizeChanged += new System.EventHandler(CollapsibleControl_SizeChanged);
		this.pnlTitle.ResumeLayout(false);
		this.pnlEditor.ResumeLayout(false);
		this.pnlEditor.PerformLayout();
		base.ResumeLayout(false);
	}
}
