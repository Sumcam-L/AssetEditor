using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Firaxis.Asset.Properties;
using Firaxis.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.Asset;

public class AttachmentEditorControl : UserControl
{
	private readonly IAttachmentPointNameProvider _nameProvider;

	private readonly ITypeDescriptorContext _editingContext;

	private IContainer components = null;

	private ListBox _attachmentPointListBox;

	private ToolStrip filterToolbar;

	private ToolStripLabel _filterLabel;

	private ToolStripButton _clearButton;

	private ToolStripButton _clearFilterButton;

	private ToolStripSpringTextBox _filterTextBox;

	public bool UserPressedOK { get; set; } = false;

	public string SelectedAttachmentPoint => (_attachmentPointListBox.SelectedItem as string) ?? string.Empty;

	public AttachmentEditorControl(IAttachmentPointNameProvider nameProvider, ITypeDescriptorContext editingContext)
	{
		InitializeComponent();
		_nameProvider = nameProvider;
		_editingContext = editingContext;
		UpdateAttachmentPointListBox(_nameProvider.AttachementPointNames);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		ResizeAttachmentListBox();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		ResizeAttachmentListBox();
	}

	private IEnumerable<string> BuildFilteredAttachmentPointList()
	{
		List<string> list = new List<string>();
		string pattern = _filterTextBox.Text.Replace(" ", ".*");
		IEnumerable<string> attachementPointNames = _nameProvider.AttachementPointNames;
		foreach (string item in attachementPointNames)
		{
			if (Regex.Match(item, pattern, RegexOptions.IgnoreCase).Success)
			{
				list.Add(item);
			}
		}
		return list;
	}

	private void UpdateAttachmentPointListBox(IEnumerable<string> newValues)
	{
		SuspendLayout();
		_attachmentPointListBox.Items.Clear();
		_attachmentPointListBox.Items.AddRange(newValues.ToArray());
		ResumeLayout();
	}

	private void ResizeAttachmentListBox()
	{
		_attachmentPointListBox.Left = 0;
		_attachmentPointListBox.Top = filterToolbar.Height + 1;
		_attachmentPointListBox.Height = base.Height - filterToolbar.Height - 1;
		_attachmentPointListBox.Width = base.Width;
	}

	private void CloseDropDown()
	{
		if (_editingContext.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService windowsFormsEditorService)
		{
			windowsFormsEditorService.CloseDropDown();
		}
	}

	private void _clearButton_Click(object sender, EventArgs e)
	{
		_attachmentPointListBox.SelectedItem = null;
		UserPressedOK = true;
		CloseDropDown();
	}

	private void _clearFilterButton_Click(object sender, EventArgs e)
	{
		_filterTextBox.Text = string.Empty;
	}

	private void _attachmentPointListBox_Click(object sender, EventArgs e)
	{
		UserPressedOK = true;
		CloseDropDown();
	}

	private void _attachmentPointListBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		Keys keyCode = e.KeyCode;
		if (keyCode == Keys.Tab || keyCode == Keys.Return || keyCode == Keys.Escape)
		{
			e.IsInputKey = true;
		}
	}

	private void _attachmentPointListBox_KeyDown(object sender, KeyEventArgs e)
	{
		switch (e.KeyCode)
		{
		case Keys.Escape:
			UserPressedOK = false;
			CloseDropDown();
			break;
		case Keys.Return:
			UserPressedOK = true;
			CloseDropDown();
			break;
		case Keys.Up:
			if (_attachmentPointListBox.Items.Count == 0 || _attachmentPointListBox.SelectedIndex == 0)
			{
				_filterTextBox.Focus();
			}
			break;
		case Keys.Tab:
			if (e.Shift)
			{
				_filterTextBox.Focus();
			}
			break;
		}
	}

	private void _filterTextBox_TextChanged(object sender, EventArgs e)
	{
		string selectedAttachmentPoint = SelectedAttachmentPoint;
		IEnumerable<string> newValues = ((!string.IsNullOrEmpty(_filterTextBox.Text)) ? BuildFilteredAttachmentPointList() : _nameProvider.AttachementPointNames);
		UpdateAttachmentPointListBox(newValues);
		_attachmentPointListBox.SelectedItem = selectedAttachmentPoint;
		_clearFilterButton.Enabled = _filterTextBox.Text.Length > 0;
	}

	private void _filterTextBox_KeyDown(object sender, KeyEventArgs e)
	{
		switch (e.KeyCode)
		{
		case Keys.Escape:
			UserPressedOK = false;
			CloseDropDown();
			break;
		case Keys.Return:
		case Keys.Down:
			_attachmentPointListBox.Focus();
			if (_attachmentPointListBox.SelectedIndex < 0 && _attachmentPointListBox.Items.Count > 0)
			{
				_attachmentPointListBox.SelectedIndex = 0;
			}
			break;
		}
	}

	private void _filterTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		Keys keyCode = e.KeyCode;
		if (keyCode == Keys.Return || keyCode == Keys.Escape || keyCode == Keys.Down)
		{
			e.IsInputKey = true;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_clearButton.Click -= _clearButton_Click;
			_attachmentPointListBox.Click -= _attachmentPointListBox_Click;
			_attachmentPointListBox.PreviewKeyDown -= _attachmentPointListBox_PreviewKeyDown;
			_attachmentPointListBox.KeyDown -= _attachmentPointListBox_KeyDown;
			_filterTextBox.TextChanged -= _filterTextBox_TextChanged;
			_filterTextBox.KeyDown -= _filterTextBox_KeyDown;
			_filterTextBox.Control.PreviewKeyDown -= _filterTextBox_PreviewKeyDown;
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this._attachmentPointListBox = new Sce.Atf.Controls.PropertyEditing.HotTrackListBox();
		this.filterToolbar = new System.Windows.Forms.ToolStrip();
		this._filterLabel = new System.Windows.Forms.ToolStripLabel();
		this._filterTextBox = new Firaxis.Controls.ToolStripSpringTextBox();
		this._clearButton = new System.Windows.Forms.ToolStripButton();
		this._clearFilterButton = new System.Windows.Forms.ToolStripButton();
		this.filterToolbar.SuspendLayout();
		base.SuspendLayout();
		this._attachmentPointListBox.BackColor = System.Drawing.SystemColors.Control;
		this._attachmentPointListBox.FormattingEnabled = true;
		this._attachmentPointListBox.Location = new System.Drawing.Point(0, 28);
		this._attachmentPointListBox.Name = "_attachmentPointListBox";
		this._attachmentPointListBox.Size = new System.Drawing.Size(387, 316);
		this._attachmentPointListBox.TabIndex = 2;
		this._attachmentPointListBox.Click += new System.EventHandler(_attachmentPointListBox_Click);
		this._attachmentPointListBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(_attachmentPointListBox_PreviewKeyDown);
		this._attachmentPointListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(_attachmentPointListBox_KeyDown);
		this.filterToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.filterToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[4] { this._filterLabel, this._filterTextBox, this._clearButton, this._clearFilterButton });
		this.filterToolbar.Location = new System.Drawing.Point(0, 0);
		this.filterToolbar.Name = "filterToolbar";
		this.filterToolbar.Size = new System.Drawing.Size(387, 25);
		this.filterToolbar.TabIndex = 4;
		this.filterToolbar.Text = "toolStrip1";
		this._filterLabel.Name = "_filterLabel";
		this._filterLabel.Size = new System.Drawing.Size(36, 22);
		this._filterLabel.Text = "Filter:";
		this._filterTextBox.AcceptsReturn = true;
		this._filterTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
		this._filterTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
		this._filterTextBox.BackColor = System.Drawing.SystemColors.Control;
		this._filterTextBox.Name = "_filterTextBox";
		this._filterTextBox.Size = new System.Drawing.Size(285, 25);
		this._filterTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(_filterTextBox_KeyDown);
		this._filterTextBox.TextChanged += new System.EventHandler(_filterTextBox_TextChanged);
		this._filterTextBox.Control.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(_filterTextBox_PreviewKeyDown);
		this._clearButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
		this._clearButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this._clearButton.Image = Firaxis.Asset.Properties.Resources.tool_delete;
		this._clearButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this._clearButton.Name = "_clearButton";
		this._clearButton.Size = new System.Drawing.Size(23, 22);
		this._clearButton.ToolTipText = "Clear Attachment";
		this._clearButton.Click += new System.EventHandler(_clearButton_Click);
		this._clearFilterButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
		this._clearFilterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this._clearFilterButton.Image = System.Drawing.Bitmap.FromHicon(Firaxis.Asset.Properties.Resources.ClearFilter.Handle);
		this._clearFilterButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this._clearFilterButton.Enabled = false;
		this._clearFilterButton.Name = "_clearFilterButton";
		this._clearFilterButton.Size = new System.Drawing.Size(23, 22);
		this._clearFilterButton.ToolTipText = "Clear Filter";
		this._clearFilterButton.Click += new System.EventHandler(_clearFilterButton_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.filterToolbar);
		base.Controls.Add(this._attachmentPointListBox);
		base.Name = "AttachmentEditorControl";
		base.Size = new System.Drawing.Size(387, 347);
		this.filterToolbar.ResumeLayout(false);
		this.filterToolbar.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
