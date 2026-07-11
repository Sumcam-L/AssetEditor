using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Firaxis.Controls;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class LibraryEditorControl : UserControl
{
	private IContainer components;

	private Button _removePackagesButton;

	private Button _addPackagesButton;

	private Label _blpsLabel;

	private ListBox _packagesListBox;

	private TextBox _libraryNameTextBox;

	private Label _libraryNameLabel;

	private Button _changeNameButton;

	private ILibraryEditingContext EditingContext { get; set; }

	public string LibraryName
	{
		get
		{
			return _libraryNameTextBox.Text;
		}
		set
		{
			_libraryNameTextBox.Text = value;
		}
	}

	public IEnumerable<RelativePathAdapter> RelativePaths
	{
		get
		{
			return _packagesListBox.Items.OfType<RelativePathAdapter>();
		}
		set
		{
			_packagesListBox.SuspendLayout();
			_packagesListBox.Items.Clear();
			ListBox.ObjectCollection items = _packagesListBox.Items;
			object[] array = value.ToArray();
			object[] items2 = array;
			items.AddRange(items2);
			_packagesListBox.ResumeLayout(performLayout: true);
		}
	}

	private IEnumerable<RelativePathAdapter> SelectedRelativePaths => _packagesListBox.SelectedItems.OfType<RelativePathAdapter>();

	public LibraryEditorControl()
	{
		InitializeComponent();
	}

	public void Bind(ILibraryEditingContext context)
	{
		if (EditingContext != null)
		{
			EditingContext.DomNode.AttributeChanged -= HandleAttributeChanged;
			EditingContext.DomNode.ChildInserted -= HandleChildInserted;
			EditingContext.DomNode.ChildRemoved -= HandleChildRemoved;
			_packagesListBox.Items.Clear();
		}
		EditingContext = context;
		if (EditingContext != null)
		{
			LibraryName = EditingContext.LibraryName;
			RelativePaths = EditingContext.RelativePaths;
			bool isNewLibrary = EditingContext.IsNewLibrary;
			_libraryNameTextBox.Enabled = isNewLibrary;
			_changeNameButton.Enabled = !isNewLibrary;
			EditingContext.DomNode.AttributeChanged += HandleAttributeChanged;
			EditingContext.DomNode.ChildInserted += HandleChildInserted;
			EditingContext.DomNode.ChildRemoved += HandleChildRemoved;
		}
	}

	private void HandleAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == GameArtSpecificationSchema.GameLibraryType.LibraryNameAttribute)
		{
			LibraryName = (string)e.NewValue;
		}
	}

	private void HandleChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == GameArtSpecificationSchema.RelativePathContainerType.RelativePathsChild)
		{
			RelativePathAdapter relativePathAdapter = e.Child.As<RelativePathAdapter>();
			if (relativePathAdapter != null)
			{
				_packagesListBox.SafeInsertAndSelect(relativePathAdapter, e.Index);
			}
		}
	}

	private void HandleChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == GameArtSpecificationSchema.RelativePathContainerType.RelativePathsChild)
		{
			RelativePathAdapter relativePathAdapter = e.Child.As<RelativePathAdapter>();
			if (relativePathAdapter != null)
			{
				_packagesListBox.SafeRemove(relativePathAdapter, e.Index);
			}
		}
	}

	private void _addPackagesButton_Click(object sender, EventArgs e)
	{
		if (EditingContext != null)
		{
			EditingContext.AddRelativePath();
		}
	}

	private void _removePackagesButton_Click(object sender, EventArgs e)
	{
		if (EditingContext != null)
		{
			List<string> paths = SelectedRelativePaths.Select((RelativePathAdapter adp) => adp.RelativePath).ToList();
			EditingContext.RemoveRelativePaths(paths);
		}
	}

	private void _packagesListBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		_removePackagesButton.Enabled = _packagesListBox.SelectedIndex >= 0;
	}

	private void _libraryNameTextBox_Leave(object sender, EventArgs e)
	{
		if (EditingContext != null)
		{
			UpdateLibraryName();
		}
	}

	private void _libraryNameTextBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (EditingContext != null && e.KeyCode == Keys.Return)
		{
			UpdateLibraryName();
		}
	}

	private void UpdateLibraryName()
	{
		if (EditingContext.IsValidName(LibraryName))
		{
			EditingContext.LibraryName = LibraryName;
			return;
		}
		MessageBox.Show(this, "A library with that name already exists.  The name change is invalid.");
		LibraryName = EditingContext.LibraryName;
	}

	private void _changeNameButton_Click(object sender, EventArgs e)
	{
		string changeNameWarningMessage = GetChangeNameWarningMessage();
		if (MessageBox.Show(this, changeNameWarningMessage, "Change Name?", MessageBoxButtons.YesNo) == DialogResult.Yes)
		{
			_libraryNameTextBox.Enabled = true;
			_changeNameButton.Enabled = false;
		}
	}

	private string GetChangeNameWarningMessage()
	{
		IEnumerable<string> referencingConsumerNames = EditingContext.GetReferencingConsumerNames();
		StringBuilder stringBuilder = new StringBuilder("The following consumers will be affected:\n\n");
		stringBuilder.AppendLine(string.Format("{0}", string.Join(" ;", referencingConsumerNames)));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Additionally, this change requires code changes, changes to the project configuration, and changes to all assets that refer to this library.");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Are you sure you want to continue?");
		return stringBuilder.ToString();
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
		this._removePackagesButton = new System.Windows.Forms.Button();
		this._addPackagesButton = new System.Windows.Forms.Button();
		this._blpsLabel = new System.Windows.Forms.Label();
		this._packagesListBox = new System.Windows.Forms.ListBox();
		this._libraryNameTextBox = new System.Windows.Forms.TextBox();
		this._libraryNameLabel = new System.Windows.Forms.Label();
		this._changeNameButton = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this._removePackagesButton.Enabled = false;
		this._removePackagesButton.Location = new System.Drawing.Point(177, 65);
		this._removePackagesButton.Name = "_removePackagesButton";
		this._removePackagesButton.Size = new System.Drawing.Size(75, 23);
		this._removePackagesButton.TabIndex = 11;
		this._removePackagesButton.Text = "Remove";
		this._removePackagesButton.UseVisualStyleBackColor = true;
		this._removePackagesButton.Click += new System.EventHandler(_removePackagesButton_Click);
		this._addPackagesButton.Location = new System.Drawing.Point(94, 65);
		this._addPackagesButton.Name = "_addPackagesButton";
		this._addPackagesButton.Size = new System.Drawing.Size(75, 23);
		this._addPackagesButton.TabIndex = 10;
		this._addPackagesButton.Text = "Add...";
		this._addPackagesButton.UseVisualStyleBackColor = true;
		this._addPackagesButton.Click += new System.EventHandler(_addPackagesButton_Click);
		this._blpsLabel.AutoSize = true;
		this._blpsLabel.Location = new System.Drawing.Point(3, 75);
		this._blpsLabel.Name = "_blpsLabel";
		this._blpsLabel.Size = new System.Drawing.Size(55, 13);
		this._blpsLabel.TabIndex = 9;
		this._blpsLabel.Text = "Packages";
		this._packagesListBox.FormattingEnabled = true;
		this._packagesListBox.Location = new System.Drawing.Point(6, 94);
		this._packagesListBox.Name = "_packagesListBox";
		this._packagesListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
		this._packagesListBox.Size = new System.Drawing.Size(246, 303);
		this._packagesListBox.TabIndex = 8;
		this._packagesListBox.SelectedIndexChanged += new System.EventHandler(_packagesListBox_SelectedIndexChanged);
		this._libraryNameTextBox.Location = new System.Drawing.Point(94, 7);
		this._libraryNameTextBox.Name = "_libraryNameTextBox";
		this._libraryNameTextBox.Size = new System.Drawing.Size(158, 20);
		this._libraryNameTextBox.TabIndex = 7;
		this._libraryNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(_libraryNameTextBox_KeyDown);
		this._libraryNameTextBox.Leave += new System.EventHandler(_libraryNameTextBox_Leave);
		this._libraryNameLabel.AutoSize = true;
		this._libraryNameLabel.Location = new System.Drawing.Point(3, 7);
		this._libraryNameLabel.Name = "_libraryNameLabel";
		this._libraryNameLabel.Size = new System.Drawing.Size(69, 13);
		this._libraryNameLabel.TabIndex = 6;
		this._libraryNameLabel.Text = "Library Name";
		this._changeNameButton.Location = new System.Drawing.Point(177, 28);
		this._changeNameButton.Name = "_changeNameButton";
		this._changeNameButton.Size = new System.Drawing.Size(75, 23);
		this._changeNameButton.TabIndex = 12;
		this._changeNameButton.Text = "Change...";
		this._changeNameButton.UseVisualStyleBackColor = true;
		this._changeNameButton.Click += new System.EventHandler(_changeNameButton_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this._changeNameButton);
		base.Controls.Add(this._removePackagesButton);
		base.Controls.Add(this._addPackagesButton);
		base.Controls.Add(this._blpsLabel);
		base.Controls.Add(this._packagesListBox);
		base.Controls.Add(this._libraryNameTextBox);
		base.Controls.Add(this._libraryNameLabel);
		base.Name = "LibraryEditorControl";
		base.Size = new System.Drawing.Size(255, 398);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
