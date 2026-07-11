using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public class BLPEntryBrowserLauncher : UserControl
{
	private BLPData m_selectedData;

	private bool m_userPressedOK;

	private readonly BLPEntryBrowser m_blpEntryBrowser;

	private readonly ICivTechService m_civTechService;

	private readonly ITypeDescriptorContext m_editingContext;

	private readonly AssetBrowserFileCommands m_fileCommands;

	private readonly object m_valueToEdit;

	private readonly IXLPRegistry m_xlpRegistry;

	private IContainer components;

	private Button _browseButton;

	private Button _openEntryButton;

	private Button _clearButton;

	public BLPData SelectedData
	{
		get
		{
			return m_selectedData;
		}
		private set
		{
			m_selectedData = value;
		}
	}

	public bool UserPressedOK
	{
		get
		{
			return m_userPressedOK;
		}
		private set
		{
			m_userPressedOK = value;
		}
	}

	public BLPEntryBrowserLauncher(ITypeDescriptorContext context, object valueToEdit, BLPEntryBrowser blpEntryBrowser, ICivTechService civTechService, AssetBrowserFileCommands fileCommands, IXLPRegistry xlpRegistry)
	{
		m_editingContext = context;
		m_valueToEdit = valueToEdit;
		m_blpEntryBrowser = blpEntryBrowser;
		m_civTechService = civTechService;
		m_fileCommands = fileCommands;
		m_xlpRegistry = xlpRegistry;
		InitializeComponent();
		_openEntryButton.Image = ResourceUtil.GetImage16(Resources.GotoFileIcon);
		_clearButton.Image = ResourceUtil.GetImage16(Resources.DeleteIcon);
		_openEntryButton.Enabled = IsEntrySet();
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		base.Size = new Size(1 + _clearButton.Width + 1 + _openEntryButton.Width + 1 + _browseButton.Width + 1, _browseButton.Height + 2);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		_clearButton.Left = base.ClientSize.Width - _clearButton.Width - 1;
		_openEntryButton.Left = _clearButton.Left - _openEntryButton.Width - 1;
		_browseButton.Left = 1;
		_browseButton.Width = _openEntryButton.Left - _browseButton.Left - 1;
	}

	private void _browseButton_Click(object sender, EventArgs e)
	{
		BLPData bLPEntry = GetBLPEntry();
		IXLPBrowserTypeProvider iXLPBrowserTypeProvider = GetDomNodeAdapter() as IXLPBrowserTypeProvider;
		if (m_blpEntryBrowser.OpenBLPEntry(ref bLPEntry.Name, ref bLPEntry.XLPPath, ref bLPEntry.BLPPath, iXLPBrowserTypeProvider.ValidTypes) == DialogResult.OK)
		{
			SelectedData = bLPEntry;
			UserPressedOK = true;
		}
	}

	private void _openEntryButton_Click(object sender, EventArgs e)
	{
		if (GetDomNodeAdapter() is IFieldValueAdapter { Value: IBLPEntryValue value })
		{
			EntityID entityID = m_xlpRegistry.GetEntityID(value.XLPPath, value.EntryName);
			if (entityID.Type != InstanceType.IT_INVALID)
			{
				m_fileCommands.OpenExistingDocument(entityID.Type, entityID.Name);
			}
		}
	}

	private void btnClear_Click(object sender, EventArgs e)
	{
		SelectedData = default(BLPData);
		UserPressedOK = true;
		if (m_editingContext.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService windowsFormsEditorService)
		{
			windowsFormsEditorService.CloseDropDown();
		}
	}

	private BLPData GetBLPEntry()
	{
		if (m_valueToEdit is string)
		{
			return new BLPData
			{
				Name = (m_valueToEdit as string)
			};
		}
		return (BLPData)m_valueToEdit;
	}

	private DomNodeAdapter GetDomNodeAdapter()
	{
		AdaptablePath<object> adaptablePath = m_editingContext.Instance as AdaptablePath<object>;
		if (adaptablePath != null)
		{
			return adaptablePath.Last.As<DomNodeAdapter>();
		}
		return ((Sce.Atf.Dom.PropertyDescriptor)m_editingContext.PropertyDescriptor).GetNode(m_editingContext.Instance).As<DomNodeAdapter>();
	}

	private bool IsEntrySet()
	{
		if (m_valueToEdit == null)
		{
			return false;
		}
		return !string.IsNullOrEmpty(GetBLPEntry().Name);
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
		this._browseButton = new System.Windows.Forms.Button();
		this._openEntryButton = new System.Windows.Forms.Button();
		this._clearButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this._browseButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom;
		this._browseButton.Location = new System.Drawing.Point(0, 1);
		this._browseButton.Name = "_browseButton";
		this._browseButton.Size = new System.Drawing.Size(80, 23);
		this._browseButton.TabIndex = 0;
		this._browseButton.Text = "Browse...";
		this._browseButton.UseVisualStyleBackColor = true;
		this._browseButton.Click += new System.EventHandler(_browseButton_Click);
		this._openEntryButton.Location = new System.Drawing.Point(82, 1);
		this._openEntryButton.Name = "_openEntryButton";
		this._openEntryButton.Size = new System.Drawing.Size(23, 23);
		this._openEntryButton.TabIndex = 1;
		this._openEntryButton.UseVisualStyleBackColor = true;
		this._openEntryButton.Click += new System.EventHandler(_openEntryButton_Click);
		this._clearButton.Location = new System.Drawing.Point(106, 1);
		this._clearButton.Name = "_clearButton";
		this._clearButton.Size = new System.Drawing.Size(23, 23);
		this._clearButton.TabIndex = 2;
		this._clearButton.UseVisualStyleBackColor = true;
		this._clearButton.Click += new System.EventHandler(btnClear_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this._clearButton);
		base.Controls.Add(this._openEntryButton);
		base.Controls.Add(this._browseButton);
		this.MinimumSize = new System.Drawing.Size(130, 25);
		base.Name = "BLPEntryBrowserLauncher";
		base.Size = new System.Drawing.Size(130, 25);
		base.ResumeLayout(false);
	}
}
