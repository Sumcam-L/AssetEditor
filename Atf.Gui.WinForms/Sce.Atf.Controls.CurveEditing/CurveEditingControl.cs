using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing;

public class CurveEditingControl : Control
{
	private enum MathOperation
	{
		Assign,
		Add,
		Subtract,
		Multiply,
		Divide
	}

	private class HelpForm : Form
	{
		private readonly TextBox m_textBox;

		public HelpForm()
		{
			base.Size = new Size(700, 450);
			Text = "Quick startup guide";
			Font = new Font(Font.Name, 12f);
			m_textBox = new TextBox();
			m_textBox.ReadOnly = true;
			m_textBox.BackColor = SystemColors.Info;
			m_textBox.Multiline = true;
			m_textBox.WordWrap = true;
			m_textBox.Dock = DockStyle.Fill;
			m_textBox.ScrollBars = ScrollBars.Vertical;
			m_textBox.Font = new Font("Lucida Console", 11.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			Assembly assembly = Assembly.GetAssembly(typeof(CurveUtils));
			Stream manifestResourceStream = assembly.GetManifestResourceStream(typeof(CurveUtils), "Resources.QuickHelp.txt");
			StreamReader streamReader = new StreamReader(manifestResourceStream);
			m_textBox.Text = streamReader.ReadToEnd();
			streamReader.Close();
			manifestResourceStream.Close();
			m_textBox.Select(0, 0);
			base.Controls.Add(m_textBox);
			base.ShowInTaskbar = false;
			base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = true;
			base.OnClosing(e);
			Hide();
		}
	}

	private class CustomToolStripRenderer : ToolStripSystemRenderer
	{
		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			if (e.Item.Enabled)
			{
				base.OnRenderMenuItemBackground(e);
			}
		}
	}

	private class AddPointDialog : Form
	{
		private PointF point;

		private readonly Label label1;

		private readonly Label label2;

		private readonly TextBox txtBoxX;

		private readonly TextBox textBoxY;

		private readonly Button cancelBtn;

		private readonly Button OkBtn;

		public PointF PointPosition => point;

		public AddPointDialog()
		{
			label1 = new Label();
			label2 = new Label();
			txtBoxX = new TextBox();
			textBoxY = new TextBox();
			cancelBtn = new Button();
			OkBtn = new Button();
			SuspendLayout();
			label1.AutoSize = true;
			label1.Location = new Point(12, 13);
			label1.Name = "label1";
			label1.Size = new Size(18, 18);
			label1.TabIndex = 0;
			label1.Text = "X";
			label2.AutoSize = true;
			label2.Location = new Point(13, 43);
			label2.Name = "label2";
			label2.Size = new Size(17, 18);
			label2.TabIndex = 1;
			label2.Text = "Y";
			txtBoxX.Location = new Point(36, 7);
			txtBoxX.Name = "txtBoxX";
			txtBoxX.Size = new Size(197, 24);
			txtBoxX.TabIndex = 2;
			txtBoxX.Text = "0";
			textBoxY.Location = new Point(36, 37);
			textBoxY.Name = "textBoxY";
			textBoxY.Size = new Size(197, 24);
			textBoxY.TabIndex = 3;
			textBoxY.Text = "0";
			cancelBtn.DialogResult = DialogResult.Cancel;
			cancelBtn.Location = new Point(158, 77);
			cancelBtn.Name = "cancelBtn";
			cancelBtn.Size = new Size(75, 29);
			cancelBtn.TabIndex = 5;
			cancelBtn.Text = "&Cancel";
			cancelBtn.UseVisualStyleBackColor = true;
			OkBtn.Location = new Point(77, 77);
			OkBtn.Name = "OkBtn";
			OkBtn.Size = new Size(75, 29);
			OkBtn.TabIndex = 6;
			OkBtn.Text = "&Ok";
			OkBtn.UseVisualStyleBackColor = true;
			OkBtn.Click += OkBtn_Click;
			base.AutoScaleMode = AutoScaleMode.None;
			base.CancelButton = cancelBtn;
			base.ClientSize = new Size(248, 111);
			base.Controls.Add(OkBtn);
			base.Controls.Add(cancelBtn);
			base.Controls.Add(textBoxY);
			base.Controls.Add(txtBoxX);
			base.Controls.Add(label2);
			base.Controls.Add(label1);
			Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
			base.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			base.Margin = new Padding(4, 4, 4, 4);
			base.Name = "Form1";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			Text = "Add Point";
			base.StartPosition = FormStartPosition.Manual;
			ResumeLayout(performLayout: false);
			PerformLayout();
		}

		private void OkBtn_Click(object sender, EventArgs e)
		{
			if (ValidateAndParseInput())
			{
				base.DialogResult = DialogResult.OK;
			}
		}

		private bool ValidateAndParseInput()
		{
			string s = txtBoxX.Text.Trim();
			string s2 = textBoxY.Text.Trim();
			float num = 0f;
			float num2 = 0f;
			point = new PointF(0f, 0f);
			try
			{
				num = float.Parse(s);
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message);
				txtBoxX.Focus();
				return false;
			}
			try
			{
				num2 = float.Parse(s2);
			}
			catch (Exception ex2)
			{
				MessageBox.Show(this, ex2.Message);
				textBoxY.Focus();
				return false;
			}
			point = new PointF(num, num2);
			return true;
		}
	}

	private bool m_showTangentEditing = true;

	private ToolStripButton m_undoBtn;

	private ToolStripButton m_redoBtn;

	private ToolStripButton m_delBtn;

	private ToolStripSeparator m_tanSeparator1;

	private ToolStripSeparator m_tanSeparator2;

	private readonly Color m_multiPointColor = Color.FromArgb(186, 150, 190);

	private CurveCanvas m_curveControl;

	private MenuStrip m_menu;

	private ToolStrip m_topStrip;

	private ToolStrip m_bottomStrip;

	private ToolStripMenuItem m_helpMenuItem;

	private ToolStripMenuItem m_optionsMenu;

	private ToolStripMenuItem m_curveMenuItem;

	private ToolStripMenuItem m_preInfinityMenuItem;

	private ToolStripMenuItem m_postInfinityMenuItem;

	private ToolStripMenuItem m_tangentsMenuItem;

	private ToolStripMenuItem m_InTangentMenuItem;

	private ToolStripMenuItem m_outTangentMenuItem;

	private ToolStripLabel m_MousePos;

	private ToolStripTextBox m_xTxtBox;

	private ToolStripLabel m_PointLabel;

	private ToolStripTextBox m_yTxtBox;

	private ToolStripSeparator m_TangentsSep1;

	private SplitContainer splitContainer1;

	private ListView m_curvesListView;

	private HelpForm m_helpForm;

	private ToolStripButton[] m_editModeButtons;

	private ToolStripButton m_fitBtn;

	private ToolStripButton[] m_tangentBtns;

	private ToolStripButton m_breakTangent;

	private ToolStripButton m_unifyTangent;

	private ToolStripButton[] m_infinityBtns;

	private bool m_firstPaint = true;

	private ToolStripMenuItem m_basicMenuItem;

	private ToolStripMenuItem m_flipYMenuItem;

	private ToolStripMenuItem m_advancedInputMenuItem;

	private ToolStripLabel m_curveTypeLabel;

	private ToolStripDropDownButton m_curveTypeSelector;

	public bool OnlyEditSelectedCurves
	{
		get
		{
			return m_curveControl.OnlyEditSelectedCurves;
		}
		set
		{
			m_curveControl.OnlyEditSelectedCurves = value;
		}
	}

	public bool AllowResizeCurveLimits
	{
		get
		{
			return m_curveControl.AllowResizeCurveLimits;
		}
		set
		{
			m_curveControl.AllowResizeCurveLimits = value;
		}
	}

	public bool AutoComputeCurveLimitsEnabled
	{
		get
		{
			return m_curveControl.AutoComputeCurveLimitsEnabled;
		}
		set
		{
			m_curveControl.AutoComputeCurveLimitsEnabled = value;
		}
	}

	public bool RestrictedTranslationEnabled
	{
		get
		{
			return m_curveControl.RestrictedTranslationEnabled;
		}
		set
		{
			m_curveControl.RestrictedTranslationEnabled = value;
		}
	}

	public object Context
	{
		set
		{
			m_curveControl.Context = value;
		}
	}

	public virtual ReadOnlyCollection<ICurve> Curves
	{
		set
		{
			SetUI(value != null);
			m_curveControl.Curves = value;
			PopulateListView(value);
			UpdateCurveTypeSelector();
		}
	}

	public OriginLockMode LockOrigin
	{
		get
		{
			return m_curveControl.LockOrigin;
		}
		set
		{
			m_curveControl.LockOrigin = value;
		}
	}

	public InputModes InputMode
	{
		get
		{
			return m_curveControl.InputMode;
		}
		set
		{
			m_curveControl.InputMode = value;
			switch (value)
			{
			case InputModes.Basic:
			{
				ToolStripButton[] editModeButtons2 = m_editModeButtons;
				foreach (ToolStripButton toolStripButton2 in editModeButtons2)
				{
					toolStripButton2.Visible = false;
				}
				break;
			}
			case InputModes.Advanced:
			{
				ToolStripButton[] editModeButtons = m_editModeButtons;
				foreach (ToolStripButton toolStripButton in editModeButtons)
				{
					toolStripButton.Visible = true;
				}
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
			m_advancedInputMenuItem.Checked = value == InputModes.Advanced;
			m_basicMenuItem.Checked = value == InputModes.Basic;
		}
	}

	protected bool ShowTangentEditing
	{
		get
		{
			return m_showTangentEditing;
		}
		set
		{
			m_showTangentEditing = value;
			ToolStripButton[] tangentBtns = m_tangentBtns;
			foreach (ToolStripButton toolStripButton in tangentBtns)
			{
				toolStripButton.Visible = value;
			}
			m_breakTangent.Visible = value;
			m_unifyTangent.Visible = value;
			m_tangentsMenuItem.Visible = value;
			m_tanSeparator1.Visible = value;
			m_tanSeparator2.Visible = value;
		}
	}

	public bool FlipY
	{
		get
		{
			return m_curveControl.FlipY;
		}
		set
		{
			m_curveControl.FlipY = value;
			Invalidate();
		}
	}

	protected CurveCanvas CurveCanvas => m_curveControl;

	protected MenuStrip MainMenu => m_menu;

	protected ToolStrip ToolBar => m_topStrip;

	public event EventHandler CurvesChanged
	{
		add
		{
			m_curveControl.CurvesChanged += value;
		}
		remove
		{
			m_curveControl.CurvesChanged -= value;
		}
	}

	public CurveEditingControl()
	{
		Init(new CurveCanvas());
	}

	public CurveEditingControl(CurveCanvas curveCanvas)
	{
		Init(curveCanvas);
	}

	public void FitAll()
	{
		m_curveControl.FitAll();
	}

	public override void Refresh()
	{
		if (!CurveCanvas.Editing)
		{
			base.Refresh();
			UpdateCurveTypeSelector();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (m_firstPaint)
		{
			PopulateListView(m_curveControl.Curves);
			m_curveControl.PanToOrigin();
			m_firstPaint = false;
		}
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.Delete)
		{
			return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void Init(CurveCanvas curveCanvas)
	{
		int[] array = (int[])Enum.GetValues(typeof(OriginLockMode));
		int[] array2 = (int[])Enum.GetValues(typeof(CurveTangentTypes));
		int[] array3 = (int[])Enum.GetValues(typeof(CurveLoopTypes));
		splitContainer1 = new SplitContainer();
		m_curvesListView = new ListView();
		m_curveControl = curveCanvas;
		m_menu = new MenuStrip();
		m_tangentsMenuItem = new ToolStripMenuItem();
		m_InTangentMenuItem = new ToolStripMenuItem();
		m_outTangentMenuItem = new ToolStripMenuItem();
		m_topStrip = new ToolStrip();
		m_PointLabel = new ToolStripLabel();
		m_xTxtBox = new ToolStripTextBox();
		m_yTxtBox = new ToolStripTextBox();
		m_bottomStrip = new ToolStrip();
		m_MousePos = new ToolStripLabel();
		m_helpForm = new HelpForm();
		m_TangentsSep1 = new ToolStripSeparator();
		m_editModeButtons = new ToolStripButton[4];
		for (int i = 0; i < m_editModeButtons.Length; i++)
		{
			m_editModeButtons[i] = new ToolStripButton();
		}
		m_tangentBtns = new ToolStripButton[5];
		for (int j = 0; j < m_tangentBtns.Length; j++)
		{
			m_tangentBtns[j] = new ToolStripButton();
		}
		m_infinityBtns = new ToolStripButton[4];
		for (int k = 0; k < m_infinityBtns.Length; k++)
		{
			m_infinityBtns[k] = new ToolStripButton();
		}
		m_helpMenuItem = new ToolStripMenuItem();
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
		m_fitBtn = new ToolStripButton();
		m_breakTangent = new ToolStripButton();
		m_unifyTangent = new ToolStripButton();
		ToolStripButton snapToX = new ToolStripButton();
		ToolStripButton snapToY = new ToolStripButton();
		ToolStripButton snapToPoint = new ToolStripButton();
		ToolStripButton snapToCurve = new ToolStripButton();
		m_undoBtn = new ToolStripButton();
		m_redoBtn = new ToolStripButton();
		m_delBtn = new ToolStripButton();
		splitContainer1.Panel1.SuspendLayout();
		splitContainer1.Panel2.SuspendLayout();
		splitContainer1.SuspendLayout();
		SuspendLayout();
		m_preInfinityMenuItem = new ToolStripMenuItem();
		m_preInfinityMenuItem.Name = "PreInfinity";
		m_preInfinityMenuItem.Text = "Pre-Infinity".Localize();
		int[] array4 = array3;
		foreach (int num in array4)
		{
			string name = Enum.GetName(typeof(CurveLoopTypes), num);
			ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem();
			toolStripMenuItem2.Name = "PreInfinity" + name;
			toolStripMenuItem2.Text = name;
			toolStripMenuItem2.Tag = (CurveLoopTypes)num;
			toolStripMenuItem2.Click += curveLoopMenu_Click;
			m_preInfinityMenuItem.DropDownItems.Add(toolStripMenuItem2);
		}
		m_postInfinityMenuItem = new ToolStripMenuItem();
		m_postInfinityMenuItem.Name = "PostInfinity";
		m_postInfinityMenuItem.Text = "Post-Infinity".Localize();
		int[] array5 = array3;
		foreach (int num2 in array5)
		{
			string name2 = Enum.GetName(typeof(CurveLoopTypes), num2);
			ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem();
			toolStripMenuItem3.Name = "PostInfinity" + name2;
			toolStripMenuItem3.Text = name2;
			toolStripMenuItem3.Tag = (CurveLoopTypes)num2;
			toolStripMenuItem3.Click += curveLoopMenu_Click;
			m_postInfinityMenuItem.DropDownItems.Add(toolStripMenuItem3);
		}
		m_curveMenuItem = new ToolStripMenuItem();
		m_curveMenuItem.DropDownItems.AddRange(new ToolStripItem[2] { m_preInfinityMenuItem, m_postInfinityMenuItem });
		m_curveMenuItem.Name = "Curve";
		m_curveMenuItem.Text = "Curve".Localize();
		ToolStripMenuItem toolStripMenuItem4 = new ToolStripMenuItem("Edit".Localize());
		toolStripMenuItem4.DropDown = m_curveControl.ContextMenuStrip;
		m_menu.Location = new Point(0, 0);
		m_menu.Name = "m_menu";
		m_menu.RenderMode = ToolStripRenderMode.System;
		m_menu.Size = new Size(898, 31);
		m_menu.TabIndex = 0;
		m_menu.Text = "menuStrip1";
		m_menu.Renderer = new CustomToolStripRenderer();
		int[] array6 = array2;
		foreach (int num3 in array6)
		{
			CurveTangentTypes curveTangentTypes = (CurveTangentTypes)num3;
			if (IsImplemented(curveTangentTypes))
			{
				string name3 = Enum.GetName(typeof(CurveTangentTypes), num3);
				ToolStripMenuItem toolStripMenuItem5 = new ToolStripMenuItem();
				toolStripMenuItem5.Name = name3;
				toolStripMenuItem5.Text = name3;
				toolStripMenuItem5.Tag = curveTangentTypes;
				toolStripMenuItem5.Click += TanMenuItem_Click;
				m_tangentsMenuItem.DropDownItems.Add(toolStripMenuItem5);
			}
		}
		m_tangentsMenuItem.DropDownItems.AddRange(new ToolStripItem[3] { m_TangentsSep1, m_InTangentMenuItem, m_outTangentMenuItem });
		m_tangentsMenuItem.Name = "m_tangentsMenuItem";
		m_tangentsMenuItem.Size = new Size(100, 27);
		m_tangentsMenuItem.Text = "Tangents".Localize();
		int[] array7 = array2;
		foreach (int num5 in array7)
		{
			CurveTangentTypes curveTangentTypes2 = (CurveTangentTypes)num5;
			if (IsImplemented(curveTangentTypes2) && curveTangentTypes2 != CurveTangentTypes.Stepped && curveTangentTypes2 != CurveTangentTypes.SteppedNext)
			{
				string name4 = Enum.GetName(typeof(CurveTangentTypes), num5);
				ToolStripMenuItem toolStripMenuItem6 = new ToolStripMenuItem();
				toolStripMenuItem6.Name = "InTan" + name4;
				toolStripMenuItem6.Text = name4;
				toolStripMenuItem6.Tag = curveTangentTypes2;
				toolStripMenuItem6.Click += TanMenuItem_Click;
				m_InTangentMenuItem.DropDownItems.Add(toolStripMenuItem6);
			}
		}
		m_InTangentMenuItem.Name = "m_InTangentMenuItem";
		m_InTangentMenuItem.Size = new Size(205, 28);
		m_InTangentMenuItem.Text = "In Tangent".Localize();
		int[] array8 = array2;
		foreach (int num7 in array8)
		{
			CurveTangentTypes curveTangentTypes3 = (CurveTangentTypes)num7;
			if (IsImplemented(curveTangentTypes3) && curveTangentTypes3 != CurveTangentTypes.Stepped && curveTangentTypes3 != CurveTangentTypes.SteppedNext)
			{
				string name5 = Enum.GetName(typeof(CurveTangentTypes), num7);
				ToolStripMenuItem toolStripMenuItem7 = new ToolStripMenuItem();
				toolStripMenuItem7.Name = "OutTan" + name5;
				toolStripMenuItem7.Text = name5;
				toolStripMenuItem7.Tag = curveTangentTypes3;
				toolStripMenuItem7.Click += TanMenuItem_Click;
				m_outTangentMenuItem.DropDownItems.Add(toolStripMenuItem7);
			}
		}
		m_outTangentMenuItem.Name = "m_outTangentMenuItem";
		m_outTangentMenuItem.Size = new Size(205, 28);
		m_outTangentMenuItem.Text = "Out Tangent".Localize();
		m_helpMenuItem.Name = "helpMenuItem";
		m_helpMenuItem.Text = "Help".Localize();
		m_helpMenuItem.DropDownItems.Add(toolStripMenuItem);
		toolStripMenuItem.Name = "quickHelpMenuItem";
		toolStripMenuItem.Text = "Quick Help...".Localize();
		toolStripMenuItem.Click += delegate
		{
			if (m_helpForm.Visible)
			{
				m_helpForm.Activate();
			}
			else
			{
				m_helpForm.Show(this);
			}
		};
		m_optionsMenu = new ToolStripMenuItem("Options".Localize());
		ToolStripMenuItem toolStripMenuItem8 = new ToolStripMenuItem("Input Mode".Localize());
		m_basicMenuItem = new ToolStripMenuItem("Basic".Localize());
		m_basicMenuItem.Name = "basic";
		m_basicMenuItem.Click += delegate
		{
			InputMode = InputModes.Basic;
		};
		m_advancedInputMenuItem = new ToolStripMenuItem("Advanced".Localize());
		m_advancedInputMenuItem.Click += delegate
		{
			InputMode = InputModes.Advanced;
		};
		InputMode = m_curveControl.InputMode;
		m_flipYMenuItem = new ToolStripMenuItem("Flip Y-Axis".Localize());
		m_flipYMenuItem.Click += delegate
		{
			FlipY = !FlipY;
		};
		m_optionsMenu.DropDownOpening += delegate
		{
			m_flipYMenuItem.Checked = FlipY;
		};
		toolStripMenuItem8.DropDownItems.Add(m_basicMenuItem);
		toolStripMenuItem8.DropDownItems.Add(m_advancedInputMenuItem);
		ToolStripMenuItem lockmenu = new ToolStripMenuItem("Lock Origin".Localize("This is the name of a command. Lock is a verb. Origin is like the origin of a graph."));
		int[] array9 = array;
		foreach (int num9 in array9)
		{
			string name6 = Enum.GetName(typeof(OriginLockMode), num9);
			ToolStripMenuItem toolStripMenuItem9 = new ToolStripMenuItem();
			toolStripMenuItem9.Name = name6;
			toolStripMenuItem9.Text = name6;
			toolStripMenuItem9.Tag = (OriginLockMode)num9;
			toolStripMenuItem9.Click += delegate(object sender, EventArgs e)
			{
				ToolStripMenuItem toolStripMenuItem12 = (ToolStripMenuItem)sender;
				m_curveControl.LockOrigin = (OriginLockMode)toolStripMenuItem12.Tag;
			};
			lockmenu.DropDownItems.Add(toolStripMenuItem9);
		}
		lockmenu.DropDownOpening += delegate
		{
			foreach (ToolStripMenuItem dropDownItem in lockmenu.DropDownItems)
			{
				dropDownItem.Checked = m_curveControl.LockOrigin == (OriginLockMode)dropDownItem.Tag;
			}
		};
		m_optionsMenu.DropDownItems.Add(toolStripMenuItem8);
		m_optionsMenu.DropDownItems.Add(lockmenu);
		m_optionsMenu.DropDownItems.Add(m_flipYMenuItem);
		m_curveTypeLabel = new ToolStripLabel();
		m_curveTypeLabel.Name = "CurveTypeLabel";
		m_curveTypeLabel.AutoSize = true;
		m_curveTypeLabel.Text = "Type".Localize("curve types");
		m_curveTypeSelector = new ToolStripDropDownButton();
		m_curveTypeSelector.Name = "CurveTypeSelector";
		m_curveTypeSelector.AutoSize = false;
		m_curveTypeSelector.Width = 70;
		m_curveTypeSelector.ToolTipText = "Type of Selected Curve(s)".Localize();
		m_curveTypeSelector.DisplayStyle = ToolStripItemDisplayStyle.Text;
		ToolStripMenuItem toolStripMenuItem10 = new ToolStripMenuItem("Linear".Localize());
		toolStripMenuItem10.Tag = InterpolationTypes.Linear;
		toolStripMenuItem10.Name = toolStripMenuItem10.Text;
		ToolStripMenuItem toolStripMenuItem11 = new ToolStripMenuItem("Smooth".Localize());
		toolStripMenuItem11.Tag = InterpolationTypes.Hermite;
		toolStripMenuItem11.Name = toolStripMenuItem11.Text;
		toolStripMenuItem11.Checked = true;
		m_curveTypeSelector.DropDownItems.Add(toolStripMenuItem10);
		m_curveTypeSelector.DropDownItems.Add(toolStripMenuItem11);
		m_curveTypeSelector.Text = toolStripMenuItem11.Text;
		m_curveTypeSelector.DropDownItemClicked += curveTypeSelector_DropDownItemClicked;
		m_menu.Items.AddRange(new ToolStripItem[5] { toolStripMenuItem4, m_curveMenuItem, m_tangentsMenuItem, m_optionsMenu, m_helpMenuItem });
		m_topStrip.Items.AddRange(m_editModeButtons);
		m_topStrip.Items.Add(new ToolStripSeparator());
		m_topStrip.Items.AddRange(new ToolStripItem[4] { m_PointLabel, m_xTxtBox, m_yTxtBox, m_fitBtn });
		m_tanSeparator1 = new ToolStripSeparator();
		m_tanSeparator2 = new ToolStripSeparator();
		m_topStrip.Items.Add(m_tanSeparator1);
		m_topStrip.Items.Add(m_curveTypeLabel);
		m_topStrip.Items.Add(m_curveTypeSelector);
		m_topStrip.Items.AddRange(m_tangentBtns);
		m_topStrip.Items.Add(m_tanSeparator2);
		m_topStrip.Items.Add(m_breakTangent);
		m_topStrip.Items.Add(m_unifyTangent);
		m_topStrip.Items.Add(new ToolStripSeparator());
		m_topStrip.Items.Add(snapToX);
		m_topStrip.Items.Add(snapToY);
		m_topStrip.Items.Add(snapToPoint);
		m_topStrip.Items.Add(snapToCurve);
		m_topStrip.Items.Add(new ToolStripSeparator());
		m_topStrip.Items.AddRange(m_infinityBtns);
		m_topStrip.Items.Add(new ToolStripSeparator());
		m_topStrip.Items.Add(m_undoBtn);
		m_topStrip.Items.Add(m_redoBtn);
		m_topStrip.Items.Add(m_delBtn);
		m_topStrip.Items.Add(new ToolStripSeparator());
		m_topStrip.Location = new Point(0, 31);
		m_topStrip.Name = "m_topStrip";
		m_topStrip.RenderMode = ToolStripRenderMode.System;
		m_topStrip.Size = new Size(898, 32);
		m_topStrip.TabIndex = 1;
		m_topStrip.Stretch = true;
		m_topStrip.Text = "topstrip";
		m_topStrip.GripStyle = ToolStripGripStyle.Hidden;
		m_topStrip.MinimumSize = new Size(32, 32);
		m_topStrip.CausesValidation = true;
		for (int num10 = 0; num10 < m_editModeButtons.Length; num10++)
		{
			m_editModeButtons[num10].DisplayStyle = ToolStripItemDisplayStyle.Image;
			m_editModeButtons[num10].Click += EditModeClick;
			m_editModeButtons[num10].Alignment = ToolStripItemAlignment.Left;
			m_editModeButtons[num10].ImageScaling = ToolStripItemImageScaling.None;
		}
		m_editModeButtons[0].Name = "ScalePoint";
		m_editModeButtons[0].Tag = EditModes.Scale;
		m_editModeButtons[0].Image = new Bitmap(typeof(CurveUtils), "Resources.ScaleKeysTool.png");
		m_editModeButtons[0].ToolTipText = "Scale selected control points   " + KeysUtil.KeysToString(CurveCanvas.ShortcutKeys.Scale, digitOnly: true);
		m_editModeButtons[1].Checked = true;
		m_editModeButtons[1].Name = "MovePoint";
		m_editModeButtons[1].Tag = EditModes.Move;
		m_editModeButtons[1].Image = new Bitmap(typeof(CurveUtils), "Resources.MoveKeysTool.png");
		m_editModeButtons[1].ToolTipText = "Move selected control points   " + KeysUtil.KeysToString(CurveCanvas.ShortcutKeys.Move, digitOnly: true);
		m_editModeButtons[2].Name = "InsertPoint";
		m_editModeButtons[2].Tag = EditModes.InsertPoint;
		m_editModeButtons[2].Image = new Bitmap(typeof(CurveUtils), "Resources.InsertKeysTool.png");
		m_editModeButtons[2].ToolTipText = "Insert control point";
		m_editModeButtons[3].Name = "AddPoint";
		m_editModeButtons[3].Tag = EditModes.AddPoint;
		m_editModeButtons[3].Image = new Bitmap(typeof(CurveUtils), "Resources.AddKeysTool.png");
		m_editModeButtons[3].ToolTipText = "Add control point";
		m_PointLabel.Name = "m_PointLabel";
		m_PointLabel.AutoSize = true;
		m_PointLabel.Text = "Stats".Localize();
		m_xTxtBox.Name = "m_XtxtBox";
		m_xTxtBox.Size = new Size(100, 30);
		m_xTxtBox.Validating += InputBoxValidating;
		m_xTxtBox.KeyUp += m_TxtBox_KeyUp;
		m_xTxtBox.ReadOnly = true;
		m_yTxtBox.Name = "m_yTxtBox";
		m_yTxtBox.Size = new Size(100, 30);
		m_yTxtBox.Validating += InputBoxValidating;
		m_yTxtBox.KeyUp += m_TxtBox_KeyUp;
		m_yTxtBox.ReadOnly = true;
		m_fitBtn.Name = "m_fitBtn";
		m_fitBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
		m_fitBtn.Alignment = ToolStripItemAlignment.Left;
		m_fitBtn.Tag = null;
		m_fitBtn.Image = new Bitmap(typeof(CurveUtils), "Resources.FrameAll.png");
		m_fitBtn.ToolTipText = "Fit " + KeysUtil.KeysToString(CurveCanvas.ShortcutKeys.Fit, digitOnly: true);
		m_fitBtn.Click += delegate
		{
			m_curveControl.Fit();
		};
		m_fitBtn.ImageScaling = ToolStripItemImageScaling.None;
		for (int num11 = 0; num11 < m_tangentBtns.Length; num11++)
		{
			m_tangentBtns[num11].DisplayStyle = ToolStripItemDisplayStyle.Image;
			m_tangentBtns[num11].Alignment = ToolStripItemAlignment.Left;
			m_tangentBtns[num11].Name = "m_tangentBtns" + num11;
			m_tangentBtns[num11].ImageScaling = ToolStripItemImageScaling.None;
			m_tangentBtns[num11].Click += delegate(object sender, EventArgs e)
			{
				ToolStripButton toolStripButton = sender as ToolStripButton;
				m_curveControl.SetTangent(TangentSelection.TangentInOut, (CurveTangentTypes)toolStripButton.Tag);
			};
		}
		m_tangentBtns[0].Tag = CurveTangentTypes.Spline;
		m_tangentBtns[0].Image = new Bitmap(typeof(CurveUtils), "Resources.SplineTangents.png");
		m_tangentBtns[0].ToolTipText = "Spline";
		m_tangentBtns[1].Tag = CurveTangentTypes.Clamped;
		m_tangentBtns[1].Image = new Bitmap(typeof(CurveUtils), "Resources.ClampedTangents.png");
		m_tangentBtns[1].ToolTipText = "Clamped";
		m_tangentBtns[2].Tag = CurveTangentTypes.Linear;
		m_tangentBtns[2].Image = new Bitmap(typeof(CurveUtils), "Resources.LinearTangents.png");
		m_tangentBtns[2].ToolTipText = "Linear";
		m_tangentBtns[3].Tag = CurveTangentTypes.Flat;
		m_tangentBtns[3].Image = new Bitmap(typeof(CurveUtils), "Resources.FlatTangents.png");
		m_tangentBtns[3].ToolTipText = "Flat";
		m_tangentBtns[4].Tag = CurveTangentTypes.Stepped;
		m_tangentBtns[4].Image = new Bitmap(typeof(CurveUtils), "Resources.StepTangents.png");
		m_tangentBtns[4].ToolTipText = "Step";
		m_breakTangent.Name = "m_breakTangent";
		m_breakTangent.DisplayStyle = ToolStripItemDisplayStyle.Image;
		m_breakTangent.Alignment = ToolStripItemAlignment.Left;
		m_breakTangent.Image = new Bitmap(typeof(CurveUtils), "Resources.BreakTangents.png");
		m_breakTangent.ImageScaling = ToolStripItemImageScaling.None;
		m_breakTangent.ToolTipText = "Break Tangents";
		m_breakTangent.Click += delegate
		{
			m_curveControl.BreakTangents(breaktan: true);
		};
		m_unifyTangent.Name = "m_unifyTangent";
		m_unifyTangent.DisplayStyle = ToolStripItemDisplayStyle.Image;
		m_unifyTangent.Alignment = ToolStripItemAlignment.Left;
		m_unifyTangent.Image = new Bitmap(typeof(CurveUtils), "Resources.UnifyTangents.png");
		m_unifyTangent.ImageScaling = ToolStripItemImageScaling.None;
		m_unifyTangent.ToolTipText = "Unify Tangents";
		m_unifyTangent.Click += delegate
		{
			m_curveControl.BreakTangents(breaktan: false);
		};
		snapToX.Checked = m_curveControl.AutoSnapToX;
		snapToX.Name = "snapToX";
		snapToX.DisplayStyle = ToolStripItemDisplayStyle.Image;
		snapToX.Alignment = ToolStripItemAlignment.Left;
		snapToX.Image = new Bitmap(typeof(CurveUtils), "Resources.TimeSnap.png");
		snapToX.ImageScaling = ToolStripItemImageScaling.None;
		snapToX.ToolTipText = "Auto snap to major X tick";
		snapToX.Click += delegate
		{
			snapToX.Checked = !snapToX.Checked;
			m_curveControl.AutoSnapToX = snapToX.Checked;
		};
		snapToY.Checked = m_curveControl.AutoSnapToY;
		snapToY.Name = "snapToY";
		snapToY.DisplayStyle = ToolStripItemDisplayStyle.Image;
		snapToY.Alignment = ToolStripItemAlignment.Left;
		snapToY.Image = new Bitmap(typeof(CurveUtils), "Resources.ValueSnap.png");
		snapToY.ImageScaling = ToolStripItemImageScaling.None;
		snapToY.ToolTipText = "Auto snap to major Y tick";
		snapToY.Click += delegate
		{
			snapToY.Checked = !snapToY.Checked;
			m_curveControl.AutoSnapToY = snapToY.Checked;
		};
		snapToPoint.Checked = m_curveControl.AutoPointSnap;
		snapToPoint.Name = "snapToPoint";
		snapToPoint.DisplayStyle = ToolStripItemDisplayStyle.Image;
		snapToPoint.Alignment = ToolStripItemAlignment.Left;
		snapToPoint.Image = new Bitmap(typeof(CurveUtils), "Resources.PointSnap.png");
		snapToPoint.ImageScaling = ToolStripItemImageScaling.None;
		snapToPoint.ToolTipText = "Auto snap to point";
		snapToPoint.Click += delegate
		{
			snapToPoint.Checked = !snapToPoint.Checked;
			m_curveControl.AutoPointSnap = snapToPoint.Checked;
		};
		snapToCurve.Checked = m_curveControl.AutoCurveSnap;
		snapToCurve.Name = "snapToCurve";
		snapToCurve.DisplayStyle = ToolStripItemDisplayStyle.Image;
		snapToCurve.Alignment = ToolStripItemAlignment.Left;
		snapToCurve.Image = new Bitmap(typeof(CurveUtils), "Resources.CurveSnap.png");
		snapToCurve.ImageScaling = ToolStripItemImageScaling.None;
		snapToCurve.ToolTipText = "Auto snap to curve";
		snapToCurve.Click += delegate
		{
			snapToCurve.Checked = !snapToCurve.Checked;
			m_curveControl.AutoCurveSnap = snapToCurve.Checked;
		};
		for (int num12 = 0; num12 <= 1; num12++)
		{
			m_infinityBtns[num12].DisplayStyle = ToolStripItemDisplayStyle.Image;
			m_infinityBtns[num12].Alignment = ToolStripItemAlignment.Left;
			m_infinityBtns[num12].Name = "m_infinityBtns" + num12;
			m_infinityBtns[num12].ImageScaling = ToolStripItemImageScaling.None;
			m_infinityBtns[num12].Click += delegate(object sender, EventArgs e)
			{
				ToolStripButton toolStripButton = sender as ToolStripButton;
				m_curveControl.SetPreInfinity((CurveLoopTypes)toolStripButton.Tag);
			};
		}
		for (int num13 = 2; num13 <= 3; num13++)
		{
			m_infinityBtns[num13].DisplayStyle = ToolStripItemDisplayStyle.Image;
			m_infinityBtns[num13].Alignment = ToolStripItemAlignment.Left;
			m_infinityBtns[num13].Name = "m_infinityBtns" + num13;
			m_infinityBtns[num13].ImageScaling = ToolStripItemImageScaling.None;
			m_infinityBtns[num13].Click += delegate(object sender, EventArgs e)
			{
				ToolStripButton toolStripButton = sender as ToolStripButton;
				m_curveControl.SetPostInfinity((CurveLoopTypes)toolStripButton.Tag);
			};
		}
		m_infinityBtns[0].Tag = CurveLoopTypes.Cycle;
		m_infinityBtns[0].Image = new Bitmap(typeof(CurveUtils), "Resources.CycleBefore.png");
		m_infinityBtns[0].ToolTipText = "Cycle Before";
		m_infinityBtns[1].Tag = CurveLoopTypes.CycleWithOffset;
		m_infinityBtns[1].Image = new Bitmap(typeof(CurveUtils), "Resources.CycleBeforewithOffset.png");
		m_infinityBtns[1].ToolTipText = "Cycle Before with Offset";
		m_infinityBtns[2].Tag = CurveLoopTypes.Cycle;
		m_infinityBtns[2].Image = new Bitmap(typeof(CurveUtils), "Resources.CycleAfter.png");
		m_infinityBtns[2].ToolTipText = "Cycle After";
		m_infinityBtns[3].Tag = CurveLoopTypes.CycleWithOffset;
		m_infinityBtns[3].Image = new Bitmap(typeof(CurveUtils), "Resources.CycleAfterwithOffset.png");
		m_infinityBtns[3].ToolTipText = "Cycle After with Offset";
		m_undoBtn.Name = "m_undoBtn";
		m_undoBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
		m_undoBtn.Alignment = ToolStripItemAlignment.Left;
		m_undoBtn.Image = ResourceUtil.GetImage24(Resources.UndoImage);
		m_undoBtn.ImageScaling = ToolStripItemImageScaling.None;
		m_undoBtn.ToolTipText = "Undo";
		m_undoBtn.Click += delegate
		{
			m_curveControl.Undo();
		};
		m_redoBtn.Name = "m_redoBtn";
		m_redoBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
		m_redoBtn.Alignment = ToolStripItemAlignment.Left;
		m_redoBtn.Image = ResourceUtil.GetImage24(Resources.RedoImage);
		m_redoBtn.ImageScaling = ToolStripItemImageScaling.None;
		m_redoBtn.ToolTipText = "Redo";
		m_redoBtn.Click += delegate
		{
			m_curveControl.Redo();
		};
		m_delBtn.Name = "delBtn";
		m_delBtn.DisplayStyle = ToolStripItemDisplayStyle.Image;
		m_delBtn.Alignment = ToolStripItemAlignment.Left;
		m_delBtn.Image = ResourceUtil.GetImage24(Resources.DeleteImage);
		m_delBtn.ImageScaling = ToolStripItemImageScaling.None;
		m_delBtn.ToolTipText = "Delete selected points";
		m_delBtn.Click += delegate
		{
			m_curveControl.Delete();
		};
		m_bottomStrip.Dock = DockStyle.Bottom;
		m_bottomStrip.Items.AddRange(new ToolStripItem[1] { m_MousePos });
		m_bottomStrip.Location = new Point(0, 549);
		m_bottomStrip.Name = "m_bottomStrip";
		m_bottomStrip.RenderMode = ToolStripRenderMode.System;
		m_bottomStrip.Size = new Size(898, 26);
		m_bottomStrip.TabIndex = 2;
		m_bottomStrip.Text = "toolStrip2";
		m_bottomStrip.GripStyle = ToolStripGripStyle.Hidden;
		m_MousePos.Alignment = ToolStripItemAlignment.Left;
		m_MousePos.AutoSize = true;
		m_MousePos.Name = "m_MousePos";
		m_MousePos.Size = new Size(250, 27);
		m_MousePos.Text = "Mouse Position".Localize();
		m_curveControl.Dock = DockStyle.Fill;
		m_curveControl.Location = new Point(24, 61);
		m_curveControl.Name = "m_curveControl";
		m_curveControl.Size = new Size(900, 600);
		m_curveControl.TabIndex = 0;
		m_curveControl.TabStop = false;
		m_curveControl.MouseMove += delegate(object sender, MouseEventArgs e)
		{
			PointD pointD = m_curveControl.ClientToGraph_d(e.X, e.Y);
			m_MousePos.Text = $"{Math.Round(pointD.X, 4)}, {Math.Round(pointD.Y, 4)}";
		};
		m_curveControl.MouseLeave += delegate
		{
			m_MousePos.Text = "";
		};
		m_curveControl.MouseUp += delegate
		{
			UpdateInputBoxes();
		};
		m_curveControl.EditMode = EditModes.Move;
		m_curveControl.EditModeChanged += delegate
		{
			ToolStripButton[] editModeButtons = m_editModeButtons;
			foreach (ToolStripButton toolStripButton in editModeButtons)
			{
				toolStripButton.Checked = (EditModes)toolStripButton.Tag == m_curveControl.EditMode;
			}
		};
		m_curveControl.SelectionChanged += SelectionChanged;
		m_TangentsSep1.Name = "m_TangentsSep1";
		m_TangentsSep1.Size = new Size(202, 6);
		splitContainer1.Dock = DockStyle.Fill;
		splitContainer1.ForeColor = SystemColors.Control;
		splitContainer1.Location = new Point(0, 48);
		splitContainer1.Name = "splitContainer1";
		splitContainer1.Panel1MinSize = 30;
		splitContainer1.Panel2MinSize = 30;
		splitContainer1.Panel1.Controls.Add(m_curvesListView);
		splitContainer1.Panel2.Controls.Add(m_curveControl);
		splitContainer1.Size = new Size(898, 520);
		splitContainer1.SplitterDistance = 180;
		splitContainer1.SplitterIncrement = 5;
		splitContainer1.TabIndex = 0;
		splitContainer1.TabStop = false;
		splitContainer1.Text = "splitContainer1";
		splitContainer1.BorderStyle = BorderStyle.None;
		splitContainer1.SplitterWidth = 4;
		splitContainer1.FixedPanel = FixedPanel.Panel1;
		splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;
		splitContainer1.SplitterMoving += splitContainer1_SplitterMoving;
		m_curvesListView.CheckBoxes = true;
		m_curvesListView.Dock = DockStyle.Fill;
		m_curvesListView.HideSelection = false;
		m_curvesListView.LabelEdit = false;
		m_curvesListView.Location = new Point(0, 0);
		m_curvesListView.Name = "m_curvesListView";
		m_curvesListView.Size = new Size(300, 300);
		m_curvesListView.TabIndex = 0;
		m_curvesListView.TabStop = false;
		m_curvesListView.UseCompatibleStateImageBehavior = false;
		m_curvesListView.View = View.Details;
		m_curvesListView.ItemChecked += m_curvesListView_ItemChecked;
		m_curvesListView.SelectedIndexChanged += m_curvesListView_SelectedIndexChanged;
		m_curvesListView.Scrollable = true;
		m_curvesListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
		m_curvesListView.Columns.Add("Curves", 250);
		m_curvesListView.AllowColumnReorder = false;
		m_curvesListView.BackColor = m_curveControl.BackColor;
		ToolStripMenuItem addMenuItem = new ToolStripMenuItem("Add Point".Localize());
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
		m_curvesListView.ContextMenuStrip = contextMenuStrip;
		contextMenuStrip.Opening += delegate
		{
			addMenuItem.Enabled = m_curvesListView.SelectedItems.Count > 0;
		};
		contextMenuStrip.Items.Add(addMenuItem);
		addMenuItem.Click += delegate
		{
			if (m_curvesListView.SelectedItems.Count != 0)
			{
				AddPointDialog addPointDialog = new AddPointDialog();
				addPointDialog.Location = new Point(Control.MousePosition.X, Control.MousePosition.Y);
				addPointDialog.ShowDialog(this);
				if (addPointDialog.DialogResult == DialogResult.OK)
				{
					PointF pt = addPointDialog.PointPosition;
					m_curveControl.TransactionContext.DoTransaction(delegate
					{
						foreach (ListViewItem selectedItem in m_curvesListView.SelectedItems)
						{
							ICurve curve = (ICurve)selectedItem.Tag;
							IControlPoint controlPoint = curve.CreateControlPoint();
							float num14 = pt.X;
							int validInsertionIndex;
							for (validInsertionIndex = CurveUtils.GetValidInsertionIndex(curve, num14); validInsertionIndex == -1; validInsertionIndex = CurveUtils.GetValidInsertionIndex(curve, num14))
							{
								num14 += CurveUtils.Epsilone;
							}
							controlPoint.X = num14;
							controlPoint.Y = pt.Y;
							curve.InsertControlPoint(validInsertionIndex, controlPoint);
							CurveUtils.ComputeTangent(curve);
						}
					}, "Add Point".Localize());
					m_curveControl.Invalidate();
				}
			}
		};
		base.ClientSize = new Size(898, 575);
		Dock = DockStyle.Fill;
		base.Controls.Add(splitContainer1);
		base.Controls.Add(m_bottomStrip);
		base.Controls.Add(m_topStrip);
		base.Controls.Add(m_menu);
		splitContainer1.Panel1.ResumeLayout(performLayout: false);
		splitContainer1.Panel2.ResumeLayout(performLayout: false);
		splitContainer1.ResumeLayout(performLayout: false);
		ResumeLayout(performLayout: false);
		PerformLayout();
		base.Invalidated += CurveEditorControl_Invalidated;
		SetUI(enable: false);
		Application.Idle += Application_Idle;
	}

	private void m_curvesListView_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_curveControl.OnlyEditSelectedCurves)
		{
			int[] array = new int[m_curvesListView.SelectedIndices.Count];
			m_curvesListView.SelectedIndices.CopyTo(array, 0);
			m_curveControl.EditableCurves = array;
		}
	}

	private void curveTypeSelector_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
	{
		ToolStripMenuItem toolStripMenuItem = e.ClickedItem as ToolStripMenuItem;
		if (toolStripMenuItem.Checked)
		{
			return;
		}
		foreach (ToolStripMenuItem dropDownItem in m_curveTypeSelector.DropDownItems)
		{
			dropDownItem.Checked = false;
		}
		toolStripMenuItem.Checked = true;
		m_curveTypeSelector.Text = toolStripMenuItem.Text;
		foreach (ICurve curf in m_curveControl.Curves)
		{
			curf.CurveInterpolation = (InterpolationTypes)toolStripMenuItem.Tag;
		}
		Invalidate();
	}

	private void Application_Idle(object sender, EventArgs e)
	{
		bool enabled = m_curveControl.Selection.Count > 0;
		m_undoBtn.Enabled = m_curveControl.HistoryContext.CanUndo;
		m_redoBtn.Enabled = m_curveControl.HistoryContext.CanRedo;
		m_delBtn.Enabled = enabled;
	}

	private void splitContainer1_SplitterMoving(object sender, SplitterCancelEventArgs e)
	{
		Control control = sender as Control;
		control.Cursor = Cursors.VSplit;
	}

	private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
	{
		Control control = sender as Control;
		control.Cursor = Cursors.Default;
	}

	private void EditModeClick(object sender, EventArgs e)
	{
		ToolStripButton toolStripButton = (ToolStripButton)sender;
		ToolStripButton[] editModeButtons = m_editModeButtons;
		foreach (ToolStripButton toolStripButton2 in editModeButtons)
		{
			toolStripButton2.Checked = toolStripButton2 == toolStripButton;
		}
		m_curveControl.EditMode = (EditModes)toolStripButton.Tag;
	}

	private void m_curvesListView_ItemChecked(object sender, ItemCheckedEventArgs e)
	{
		ICurve curve = e.Item.Tag as ICurve;
		curve.Visible = e.Item.Checked;
		if (!curve.Visible)
		{
			m_curveControl.RemoveCurveFromSelection(curve);
		}
		Invalidate();
	}

	private void PopulateListView(ReadOnlyCollection<ICurve> curves)
	{
		m_curvesListView.Items.Clear();
		if (curves == null || curves.Count == 0)
		{
			return;
		}
		m_curvesListView.BeginUpdate();
		foreach (ICurve curf in curves)
		{
			string text = (string.IsNullOrWhiteSpace(curf.DisplayName) ? curf.Name : curf.DisplayName);
			if (text.Length > 250)
			{
				text = text.Substring(0, 250);
			}
			ListViewItem listViewItem = new ListViewItem(text);
			listViewItem.ForeColor = curf.CurveColor;
			listViewItem.Checked = curf.Visible;
			listViewItem.Tag = curf;
			m_curvesListView.Items.Add(listViewItem);
		}
		m_curvesListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		m_curvesListView.EndUpdate();
	}

	private void SetUI(bool enable)
	{
		foreach (ToolStripItem item in m_topStrip.Items)
		{
			item.Enabled = enable;
		}
		foreach (ToolStripItem item2 in m_menu.Items)
		{
			item2.Enabled = enable;
		}
		m_optionsMenu.Enabled = true;
		m_helpMenuItem.Enabled = true;
	}

	private void CurveEditorControl_Invalidated(object sender, InvalidateEventArgs e)
	{
		m_curveControl.Invalidate();
	}

	private void TanMenuItem_Click(object sender, EventArgs e)
	{
		if (m_curveControl.Selection.Count != 0)
		{
			ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
			if (toolStripMenuItem.OwnerItem == null)
			{
				throw new InvalidOperationException("Improper UI initialization");
			}
			TangentSelection tangentSelection = TangentSelection.None;
			if (toolStripMenuItem.OwnerItem == m_tangentsMenuItem)
			{
				tangentSelection = TangentSelection.TangentInOut;
			}
			else if (toolStripMenuItem.OwnerItem == m_InTangentMenuItem)
			{
				tangentSelection = TangentSelection.TangentIn;
			}
			else if (toolStripMenuItem.OwnerItem == m_outTangentMenuItem)
			{
				tangentSelection = TangentSelection.TangentOut;
			}
			if (tangentSelection != TangentSelection.None)
			{
				m_curveControl.SetTangent(tangentSelection, (CurveTangentTypes)toolStripMenuItem.Tag);
			}
		}
	}

	private void curveLoopMenu_Click(object sender, EventArgs e)
	{
		ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
		if (toolStripMenuItem.OwnerItem == null)
		{
			throw new InvalidOperationException("Improper UI initialization");
		}
		if (toolStripMenuItem.OwnerItem == m_preInfinityMenuItem)
		{
			m_curveControl.SetPreInfinity((CurveLoopTypes)toolStripMenuItem.Tag);
			return;
		}
		if (toolStripMenuItem.OwnerItem == m_postInfinityMenuItem)
		{
			m_curveControl.SetPostInfinity((CurveLoopTypes)toolStripMenuItem.Tag);
			return;
		}
		throw new InvalidOperationException(toolStripMenuItem.OwnerItem.Text + " is not valid parent for " + toolStripMenuItem.Text);
	}

	private void m_TxtBox_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			e.Handled = true;
			m_curveControl.Focus();
		}
	}

	private void InputBoxValidating(object sender, CancelEventArgs e)
	{
		ToolStripTextBox txtBox = sender as ToolStripTextBox;
		if (m_curveControl.Selection.Count == 0)
		{
			SetInputBoxes(m_xTxtBox, active: false);
			SetInputBoxes(m_yTxtBox, active: false);
			return;
		}
		try
		{
			string text = txtBox.Text.Trim();
			MathOperation op = MathOperation.Assign;
			if (text.StartsWith("+="))
			{
				op = MathOperation.Add;
			}
			else if (text.StartsWith("-="))
			{
				op = MathOperation.Subtract;
			}
			else if (text.StartsWith("*="))
			{
				op = MathOperation.Multiply;
			}
			else if (text.StartsWith("/="))
			{
				op = MathOperation.Divide;
			}
			if (op != MathOperation.Assign)
			{
				text = text.Substring(2);
			}
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			HashSet<ICurve> curveset = new HashSet<ICurve>();
			List<Vec2F> orglist = new List<Vec2F>();
			foreach (IControlPoint item in m_curveControl.Selection)
			{
				orglist.Add(new Vec2F(item.X, item.Y));
			}
			float f = float.Parse(text);
			m_curveControl.TransactionContext.DoTransaction(delegate
			{
				if (txtBox == m_xTxtBox)
				{
					if ((bool)m_xTxtBox.Tag || (op != MathOperation.Assign && (op != MathOperation.Multiply || f != 0f)))
					{
						foreach (IControlPoint item2 in m_curveControl.Selection)
						{
							item2.X = Operate(item2.X, f, op);
							if (!CurveUtils.IsSorted(item2))
							{
								curveset.Add(item2.Parent);
							}
						}
						if (m_curveControl.RestrictedTranslationEnabled && curveset.Count > 0)
						{
							for (int i = 0; i < m_curveControl.Selection.Count; i++)
							{
								IControlPoint controlPoint = m_curveControl.Selection[i];
								Vec2F vec2F = orglist[i];
								controlPoint.X = vec2F.X;
								controlPoint.Y = vec2F.Y;
							}
						}
					}
				}
				else
				{
					foreach (IControlPoint item3 in m_curveControl.Selection)
					{
						item3.Y = Operate(item3.Y, f, op);
					}
				}
				ICurve[] selectedCurves = m_curveControl.SelectedCurves;
				foreach (ICurve curve in selectedCurves)
				{
					CurveUtils.Sort(curve);
					CurveUtils.ForceMinDistance(curve);
					CurveUtils.ComputeTangent(curve);
				}
				CurveCanvas.UpdateCurveLimits();
			}, "Edit Point".Localize());
			m_curveControl.Invalidate();
		}
		catch (Exception ex)
		{
			string text2 = txtBox.Text;
			bool flag = true;
			float num = 0f;
			if (txtBox == m_xTxtBox)
			{
				num = m_curveControl.Selection[0].X;
				foreach (IControlPoint item4 in m_curveControl.Selection)
				{
					if (item4.X != num)
					{
						flag = false;
						break;
					}
				}
			}
			else
			{
				num = m_curveControl.Selection[0].Y;
				foreach (IControlPoint item5 in m_curveControl.Selection)
				{
					if (item5.Y != num)
					{
						flag = false;
						break;
					}
				}
			}
			txtBox.Text = (flag ? num.ToString() : "");
			MessageBox.Show(this, text2 + " " + ex.Message, "CurveEditor");
		}
	}

	private float Operate(float val1, float val2, MathOperation operation)
	{
		float result = 0f;
		switch (operation)
		{
		case MathOperation.Add:
			result = val1 + val2;
			break;
		case MathOperation.Subtract:
			result = val1 - val2;
			break;
		case MathOperation.Multiply:
			result = val1 * val2;
			break;
		case MathOperation.Divide:
			result = val1 / val2;
			break;
		case MathOperation.Assign:
			result = val2;
			break;
		}
		return result;
	}

	private void SelectionChanged(object sender, EventArgs e)
	{
		UpdateInputBoxes();
	}

	private void UpdateInputBoxes()
	{
		if (m_curveControl.Selection.Count > 0)
		{
			SetInputBoxes(m_yTxtBox, active: true);
			SetInputBoxes(m_xTxtBox, active: true);
			bool flag = true;
			float num = m_curveControl.Selection[0].Y;
			foreach (IControlPoint item in m_curveControl.Selection)
			{
				if (item.Y != num)
				{
					flag = false;
					break;
				}
			}
			m_yTxtBox.Text = (flag ? num.ToString() : "");
			m_yTxtBox.BackColor = (flag ? SystemColors.Window : m_multiPointColor);
			bool flag2 = true;
			Dictionary<ICurve, object> dictionary = new Dictionary<ICurve, object>();
			foreach (IControlPoint item2 in m_curveControl.Selection)
			{
				if (item2.Parent != null && dictionary.ContainsKey(item2.Parent))
				{
					flag2 = false;
					break;
				}
				dictionary.Add(item2.Parent, null);
			}
			m_xTxtBox.Tag = flag2;
			flag = true;
			num = m_curveControl.Selection[0].X;
			foreach (IControlPoint item3 in m_curveControl.Selection)
			{
				if (item3.X != num)
				{
					flag = false;
					break;
				}
			}
			m_xTxtBox.Text = (flag ? num.ToString() : "");
			m_xTxtBox.BackColor = (flag ? SystemColors.Window : m_multiPointColor);
		}
		else
		{
			SetInputBoxes(m_yTxtBox, active: false);
			SetInputBoxes(m_xTxtBox, active: false);
		}
	}

	private void SetInputBoxes(ToolStripTextBox txtBox, bool active)
	{
		txtBox.Text = "";
		txtBox.Tag = null;
		if (active)
		{
			txtBox.BackColor = SystemColors.Window;
			txtBox.ReadOnly = false;
		}
		else
		{
			txtBox.BackColor = SystemColors.Control;
			txtBox.ReadOnly = true;
		}
	}

	private void UpdateCurveTypeSelector()
	{
		bool flag = false;
		bool flag2 = false;
		foreach (ICurve curf in m_curveControl.Curves)
		{
			if (curf.CurveInterpolation == InterpolationTypes.Linear)
			{
				flag = true;
			}
			else if (curf.CurveInterpolation == InterpolationTypes.Hermite)
			{
				flag2 = true;
			}
			if (flag && flag2)
			{
				break;
			}
		}
		InterpolationTypes curveTypeSelector = InterpolationTypes.None;
		if (flag && !flag2)
		{
			curveTypeSelector = InterpolationTypes.Linear;
		}
		else if (flag2 && !flag)
		{
			curveTypeSelector = InterpolationTypes.Hermite;
		}
		SetCurveTypeSelector(curveTypeSelector);
	}

	private void SetCurveTypeSelector(InterpolationTypes interpolationTypes)
	{
		if (interpolationTypes == InterpolationTypes.None)
		{
			foreach (ToolStripMenuItem dropDownItem in m_curveTypeSelector.DropDownItems)
			{
				dropDownItem.Checked = false;
			}
			m_curveTypeSelector.Text = "(Multiple)".Localize("CurveTypeSelector");
			return;
		}
		foreach (ToolStripMenuItem dropDownItem2 in m_curveTypeSelector.DropDownItems)
		{
			if ((InterpolationTypes)dropDownItem2.Tag == interpolationTypes)
			{
				dropDownItem2.Checked = true;
				m_curveTypeSelector.Text = dropDownItem2.Text;
			}
			else
			{
				dropDownItem2.Checked = false;
			}
		}
	}

	private bool IsImplemented(CurveTangentTypes tanType)
	{
		bool result = false;
		switch (tanType)
		{
		case CurveTangentTypes.Spline:
		case CurveTangentTypes.Linear:
		case CurveTangentTypes.Clamped:
		case CurveTangentTypes.Stepped:
		case CurveTangentTypes.Flat:
			result = true;
			break;
		}
		return result;
	}
}
