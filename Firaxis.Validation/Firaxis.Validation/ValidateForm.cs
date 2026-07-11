using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Controls;
using Firaxis.Validation.Properties;

namespace Firaxis.Validation;

public class ValidateForm : Form
{
	private bool reportMode;

	private IContainer components = null;

	private ValidatePanel validatePanel1;

	private ToolStrip toolStrip;

	private ToolStripButton buttonRunTests;

	private ToolStripButton buttonExportResults;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton buttonShowTests;

	private SplitterControl splitterControl1;

	private ValidatorsPanel validatorsPanel1;

	private ToolStripButton buttonClearResults;

	private CaptionControl captionControl1;

	public bool AllowUserClose { get; set; }

	public bool ReportMode
	{
		get
		{
			return reportMode;
		}
		set
		{
			reportMode = value;
			buttonRunTests.Enabled = !reportMode;
			buttonClearResults.Enabled = !reportMode;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public object Sender { get; set; }

	[Browsable(false)]
	public ValidatePanel ValidatePanel => validatePanel1;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ValidatorProvider ValidatorProvider
	{
		get
		{
			return validatePanel1.ValidatorProvider;
		}
		set
		{
			validatePanel1.ValidatorProvider = value;
			validatorsPanel1.ValidatorProvider = value;
		}
	}

	public ValidateForm()
	{
		InitializeComponent();
		AllowUserClose = true;
		ReportMode = false;
	}

	public void RunTests()
	{
		if (ValidatorProvider != null)
		{
			Invalidate();
			Update();
			Cursor = Cursors.WaitCursor;
			ValidatorProvider.RunTests(Sender);
			Cursor = Cursors.Default;
		}
	}

	private void buttonRunTests_Click(object sender, EventArgs e)
	{
		RunTests();
	}

	private void buttonExportResults_Click(object sender, EventArgs e)
	{
	}

	private void buttonShowTests_Click(object sender, EventArgs e)
	{
		splitterControl1.Panel2Collapsed = !splitterControl1.Panel2Collapsed;
		buttonShowTests.Checked = !splitterControl1.Panel2Collapsed;
	}

	private void ValidateForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (!AllowUserClose && e.CloseReason == CloseReason.UserClosing)
		{
			e.Cancel = true;
			Hide();
		}
	}

	private void buttonClearResults_Click(object sender, EventArgs e)
	{
		if (ValidatorProvider != null)
		{
			ValidatorProvider.Results.Clear();
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
		this.toolStrip = new System.Windows.Forms.ToolStrip();
		this.buttonRunTests = new System.Windows.Forms.ToolStripButton();
		this.buttonClearResults = new System.Windows.Forms.ToolStripButton();
		this.buttonExportResults = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.buttonShowTests = new System.Windows.Forms.ToolStripButton();
		this.splitterControl1 = new Firaxis.Controls.SplitterControl();
		this.validatePanel1 = new Firaxis.Validation.ValidatePanel();
		this.validatorsPanel1 = new Firaxis.Validation.ValidatorsPanel();
		this.captionControl1 = new Firaxis.Controls.CaptionControl();
		this.toolStrip.SuspendLayout();
		this.splitterControl1.Panel1.SuspendLayout();
		this.splitterControl1.Panel2.SuspendLayout();
		this.splitterControl1.SuspendLayout();
		base.SuspendLayout();
		this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.buttonRunTests, this.buttonClearResults, this.buttonExportResults, this.toolStripSeparator1, this.buttonShowTests });
		this.toolStrip.Location = new System.Drawing.Point(0, 0);
		this.toolStrip.Name = "toolStrip";
		this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
		this.toolStrip.Size = new System.Drawing.Size(445, 25);
		this.toolStrip.TabIndex = 1;
		this.toolStrip.Text = "toolStrip";
		this.buttonRunTests.Image = Firaxis.Validation.Properties.Resources.graph_level;
		this.buttonRunTests.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonRunTests.Name = "buttonRunTests";
		this.buttonRunTests.Size = new System.Drawing.Size(69, 22);
		this.buttonRunTests.Text = "Validate";
		this.buttonRunTests.Click += new System.EventHandler(buttonRunTests_Click);
		this.buttonClearResults.Image = Firaxis.Validation.Properties.Resources.clear_results;
		this.buttonClearResults.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonClearResults.Name = "buttonClearResults";
		this.buttonClearResults.Size = new System.Drawing.Size(94, 22);
		this.buttonClearResults.Text = "Clear Results";
		this.buttonClearResults.Click += new System.EventHandler(buttonClearResults_Click);
		this.buttonExportResults.Enabled = false;
		this.buttonExportResults.Image = Firaxis.Validation.Properties.Resources.Control_SaveFileDialog;
		this.buttonExportResults.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonExportResults.Name = "buttonExportResults";
		this.buttonExportResults.Size = new System.Drawing.Size(100, 22);
		this.buttonExportResults.Text = "Export Results";
		this.buttonExportResults.ToolTipText = "Export Results";
		this.buttonExportResults.Click += new System.EventHandler(buttonExportResults_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.buttonShowTests.Image = Firaxis.Validation.Properties.Resources.version_task;
		this.buttonShowTests.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.buttonShowTests.Name = "buttonShowTests";
		this.buttonShowTests.Size = new System.Drawing.Size(86, 22);
		this.buttonShowTests.Text = "Show Tests";
		this.buttonShowTests.ToolTipText = "Show the list of tests that will be performed";
		this.buttonShowTests.Click += new System.EventHandler(buttonShowTests_Click);
		this.splitterControl1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitterControl1.Location = new System.Drawing.Point(0, 46);
		this.splitterControl1.Name = "splitterControl1";
		this.splitterControl1.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitterControl1.Panel1.Controls.Add(this.validatePanel1);
		this.splitterControl1.Panel2.Controls.Add(this.validatorsPanel1);
		this.splitterControl1.Panel2Collapsed = true;
		this.splitterControl1.Size = new System.Drawing.Size(445, 342);
		this.splitterControl1.SplitterDistance = 231;
		this.splitterControl1.TabIndex = 2;
		this.validatePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.validatePanel1.Location = new System.Drawing.Point(0, 0);
		this.validatePanel1.Name = "validatePanel1";
		this.validatePanel1.Size = new System.Drawing.Size(445, 342);
		this.validatePanel1.TabIndex = 0;
		this.validatorsPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.validatorsPanel1.Location = new System.Drawing.Point(0, 0);
		this.validatorsPanel1.Name = "validatorsPanel1";
		this.validatorsPanel1.Size = new System.Drawing.Size(150, 46);
		this.validatorsPanel1.TabIndex = 0;
		this.captionControl1.BackColor = System.Drawing.Color.FromArgb(54, 54, 54);
		this.captionControl1.Caption = "Double-Click error lines to view error location";
		this.captionControl1.Dock = System.Windows.Forms.DockStyle.Top;
		this.captionControl1.ForeColor = System.Drawing.Color.White;
		this.captionControl1.Image = Firaxis.Validation.Properties.Resources.appwindow_info_annotation;
		this.captionControl1.Location = new System.Drawing.Point(0, 25);
		this.captionControl1.Name = "captionControl1";
		this.captionControl1.Size = new System.Drawing.Size(445, 21);
		this.captionControl1.TabIndex = 3;
		this.captionControl1.Transparent = System.Drawing.Color.Magenta;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(445, 388);
		base.Controls.Add(this.splitterControl1);
		base.Controls.Add(this.captionControl1);
		base.Controls.Add(this.toolStrip);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		this.MinimumSize = new System.Drawing.Size(295, 196);
		base.Name = "ValidateForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "Validate";
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(ValidateForm_FormClosing);
		this.toolStrip.ResumeLayout(false);
		this.toolStrip.PerformLayout();
		this.splitterControl1.Panel1.ResumeLayout(false);
		this.splitterControl1.Panel2.ResumeLayout(false);
		this.splitterControl1.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
