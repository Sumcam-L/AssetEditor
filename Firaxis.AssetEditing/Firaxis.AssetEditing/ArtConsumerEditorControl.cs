using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.Controls;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtConsumerEditorControl : UserControl
{
	private IContainer components;

	private Label _consumerNameLabel;

	private TextBox _consumerNameTextBox;

	private ListBox _artDefsListBox;

	private Label _artDefLabel;

	private Button _addArtDefsButton;

	private Button _removeArtDefsButton;

	private Button _removeLibraryReference;

	private Label _libraryReferenceLabel;

	private ListBox _libraryReferences;

	private ComboBox _libraryComboBox;

	private CheckBox _loadsLibraries;

	private IArtConsumerEditingContext EditingContext { get; set; }

	public string ConsumerName
	{
		get
		{
			return _consumerNameTextBox.Text;
		}
		set
		{
			_consumerNameTextBox.Text = value;
		}
	}

	public bool LoadsLibraries
	{
		get
		{
			return _loadsLibraries.Checked;
		}
		set
		{
			_loadsLibraries.Checked = value;
		}
	}

	public IEnumerable<RelativePathAdapter> RelativePaths
	{
		get
		{
			return _artDefsListBox.Items.OfType<RelativePathAdapter>();
		}
		set
		{
			_artDefsListBox.Items.Clear();
			foreach (RelativePathAdapter item in value)
			{
				_artDefsListBox.Items.Add(item);
			}
		}
	}

	private IEnumerable<RelativePathAdapter> SelectedRelativePaths => _artDefsListBox.SelectedItems.OfType<RelativePathAdapter>();

	public IEnumerable<LibraryReferenceAdapter> LibraryReferences
	{
		get
		{
			return _libraryReferences.Items.OfType<LibraryReferenceAdapter>();
		}
		set
		{
			_libraryReferences.Items.Clear();
			foreach (LibraryReferenceAdapter item in value)
			{
				_libraryReferences.Items.Add(item);
			}
		}
	}

	private IEnumerable<LibraryReferenceAdapter> SelectedLibraryReferences => _libraryReferences.SelectedItems.OfType<LibraryReferenceAdapter>();

	public ArtConsumerEditorControl()
	{
		InitializeComponent();
	}

	public void Bind(IArtConsumerEditingContext context)
	{
		if (EditingContext != null)
		{
			EditingContext.DomNode.AttributeChanged -= HandleAttributeChanged;
			EditingContext.DomNode.ChildInserted -= HandleChildInserted;
			EditingContext.DomNode.ChildRemoved -= HandleChildRemoved;
			_artDefsListBox.Items.Clear();
			_libraryReferences.Items.Clear();
			_libraryComboBox.Items.Clear();
		}
		EditingContext = context;
		if (EditingContext != null)
		{
			ConsumerName = EditingContext.ConsumerName;
			RelativePaths = EditingContext.RelativePaths;
			LibraryReferences = EditingContext.LibraryReferences;
			LoadsLibraries = EditingContext.LoadsLibraries;
			ComboBox.ObjectCollection items = _libraryComboBox.Items;
			object[] libraryNames = EditingContext.LibraryNames;
			object[] items2 = libraryNames;
			items.AddRange(items2);
			EditingContext.DomNode.AttributeChanged += HandleAttributeChanged;
			EditingContext.DomNode.ChildInserted += HandleChildInserted;
			EditingContext.DomNode.ChildRemoved += HandleChildRemoved;
		}
	}

	private void HandleAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == GameArtSpecificationSchema.ArtConsumerType.ConsumerNameAttribute)
		{
			ConsumerName = (string)e.NewValue;
		}
		else if (e.AttributeInfo == GameArtSpecificationSchema.ArtConsumerType.LoadsLibrariesAttribute)
		{
			LoadsLibraries = (bool)e.NewValue;
		}
		else if (e.AttributeInfo == GameArtSpecificationSchema.RelativePathType.RelativePathAttribute)
		{
			RelativePathAdapter relativePathAdapter = e.DomNode.As<RelativePathAdapter>();
			if (relativePathAdapter != null)
			{
				_artDefsListBox.RefreshListBoxItem(relativePathAdapter);
			}
		}
		else if (e.AttributeInfo == GameArtSpecificationSchema.LibraryReferenceType.LibraryNameAttribute)
		{
			LibraryReferenceAdapter libraryReferenceAdapter = e.DomNode.As<LibraryReferenceAdapter>();
			if (libraryReferenceAdapter != null)
			{
				_libraryReferences.RefreshListBoxItem(libraryReferenceAdapter);
			}
		}
	}

	private void HandleChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == GameArtSpecificationSchema.RelativePathContainerType.RelativePathsChild)
		{
			RelativePathAdapter relativePathAdapter = e.Child.As<RelativePathAdapter>();
			if (relativePathAdapter != null)
			{
				_artDefsListBox.SafeInsertAndSelect(relativePathAdapter, e.Index);
			}
		}
		else if (e.ChildInfo == GameArtSpecificationSchema.LibraryReferenceContainerType.LibraryReferencesChild)
		{
			LibraryReferenceAdapter libraryReferenceAdapter = e.Child.As<LibraryReferenceAdapter>();
			if (libraryReferenceAdapter != null)
			{
				_libraryReferences.SafeInsertAndSelect(libraryReferenceAdapter, e.Index);
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
				_artDefsListBox.SafeRemove(relativePathAdapter, e.Index);
			}
		}
		else if (e.ChildInfo == GameArtSpecificationSchema.LibraryReferenceContainerType.LibraryReferencesChild)
		{
			LibraryReferenceAdapter libraryReferenceAdapter = e.Child.As<LibraryReferenceAdapter>();
			if (libraryReferenceAdapter != null)
			{
				_libraryReferences.SafeRemove(libraryReferenceAdapter, e.Index);
			}
		}
	}

	private void _addArtDefsButton_Click(object sender, EventArgs e)
	{
		if (EditingContext != null)
		{
			EditingContext.AddRelativeArtDefPath();
		}
	}

	private void _removeArtDefsButton_Click(object sender, EventArgs e)
	{
		if (EditingContext != null)
		{
			List<string> paths = SelectedRelativePaths.Select((RelativePathAdapter adp) => adp.RelativePath).ToList();
			EditingContext.RemoveRelativeArtDefPaths(paths);
		}
	}

	private void _artDefsListBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		_removeArtDefsButton.Enabled = _artDefsListBox.HasSelection();
	}

	private void _removeLibraryReference_Click(object sender, EventArgs e)
	{
		if (EditingContext != null)
		{
			List<string> libraryNames = SelectedLibraryReferences.Select((LibraryReferenceAdapter adp) => adp.LibraryName).ToList();
			EditingContext.RemoveLibraryReferences(libraryNames);
		}
	}

	private void _libraryReferences_SelectedIndexChanged(object sender, EventArgs e)
	{
		_removeLibraryReference.Enabled = _libraryReferences.HasSelection();
	}

	private void _libraryComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (EditingContext != null && _libraryComboBox.SelectedIndex >= 0)
		{
			string libraryName = _libraryComboBox.SelectedItem.ToString();
			EditingContext.AddLibraryReferences(libraryName);
			_libraryComboBox.SelectedIndex = -1;
		}
	}

	private void _loadsLibraries_CheckedChanged(object sender, EventArgs e)
	{
		if (EditingContext != null)
		{
			EditingContext.LoadsLibraries = _loadsLibraries.Checked;
		}
	}

	private void _consumerNameTextBox_Leave(object sender, EventArgs e)
	{
		if (EditingContext != null)
		{
			UpdateConsumerName();
		}
	}

	private void _consumerNameTextBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (EditingContext != null && e.KeyCode == Keys.Return)
		{
			UpdateConsumerName();
		}
	}

	private void UpdateConsumerName()
	{
		if (EditingContext.IsValidName(ConsumerName))
		{
			EditingContext.ConsumerName = ConsumerName;
			return;
		}
		MessageBox.Show(this, "A consumer with that name already exists.  The name change is invalid.");
		ConsumerName = EditingContext.ConsumerName;
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
		this._consumerNameLabel = new System.Windows.Forms.Label();
		this._consumerNameTextBox = new System.Windows.Forms.TextBox();
		this._artDefsListBox = new System.Windows.Forms.ListBox();
		this._artDefLabel = new System.Windows.Forms.Label();
		this._addArtDefsButton = new System.Windows.Forms.Button();
		this._removeArtDefsButton = new System.Windows.Forms.Button();
		this._removeLibraryReference = new System.Windows.Forms.Button();
		this._libraryReferenceLabel = new System.Windows.Forms.Label();
		this._libraryReferences = new System.Windows.Forms.ListBox();
		this._libraryComboBox = new System.Windows.Forms.ComboBox();
		this._loadsLibraries = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		this._consumerNameLabel.AutoSize = true;
		this._consumerNameLabel.Location = new System.Drawing.Point(3, 3);
		this._consumerNameLabel.Name = "_consumerNameLabel";
		this._consumerNameLabel.Size = new System.Drawing.Size(85, 13);
		this._consumerNameLabel.TabIndex = 0;
		this._consumerNameLabel.Text = "Consumer Name";
		this._consumerNameTextBox.Location = new System.Drawing.Point(94, 3);
		this._consumerNameTextBox.Name = "_consumerNameTextBox";
		this._consumerNameTextBox.Size = new System.Drawing.Size(158, 20);
		this._consumerNameTextBox.TabIndex = 1;
		this._consumerNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(_consumerNameTextBox_KeyDown);
		this._consumerNameTextBox.Leave += new System.EventHandler(_consumerNameTextBox_Leave);
		this._artDefsListBox.FormattingEnabled = true;
		this._artDefsListBox.Location = new System.Drawing.Point(6, 64);
		this._artDefsListBox.Name = "_artDefsListBox";
		this._artDefsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
		this._artDefsListBox.Size = new System.Drawing.Size(246, 108);
		this._artDefsListBox.TabIndex = 2;
		this._artDefsListBox.SelectedIndexChanged += new System.EventHandler(_artDefsListBox_SelectedIndexChanged);
		this._artDefLabel.AutoSize = true;
		this._artDefLabel.Location = new System.Drawing.Point(3, 45);
		this._artDefLabel.Name = "_artDefLabel";
		this._artDefLabel.Size = new System.Drawing.Size(42, 13);
		this._artDefLabel.TabIndex = 3;
		this._artDefLabel.Text = "ArtDefs";
		this._addArtDefsButton.Location = new System.Drawing.Point(94, 35);
		this._addArtDefsButton.Name = "_addArtDefsButton";
		this._addArtDefsButton.Size = new System.Drawing.Size(75, 23);
		this._addArtDefsButton.TabIndex = 4;
		this._addArtDefsButton.Text = "Add...";
		this._addArtDefsButton.UseVisualStyleBackColor = true;
		this._addArtDefsButton.Click += new System.EventHandler(_addArtDefsButton_Click);
		this._removeArtDefsButton.Enabled = false;
		this._removeArtDefsButton.Location = new System.Drawing.Point(177, 35);
		this._removeArtDefsButton.Name = "_removeArtDefsButton";
		this._removeArtDefsButton.Size = new System.Drawing.Size(75, 23);
		this._removeArtDefsButton.TabIndex = 5;
		this._removeArtDefsButton.Text = "Remove";
		this._removeArtDefsButton.UseVisualStyleBackColor = true;
		this._removeArtDefsButton.Click += new System.EventHandler(_removeArtDefsButton_Click);
		this._removeLibraryReference.Enabled = false;
		this._removeLibraryReference.Location = new System.Drawing.Point(177, 192);
		this._removeLibraryReference.Name = "_removeLibraryReference";
		this._removeLibraryReference.Size = new System.Drawing.Size(75, 23);
		this._removeLibraryReference.TabIndex = 9;
		this._removeLibraryReference.Text = "Remove";
		this._removeLibraryReference.UseVisualStyleBackColor = true;
		this._removeLibraryReference.Click += new System.EventHandler(_removeLibraryReference_Click);
		this._libraryReferenceLabel.AutoSize = true;
		this._libraryReferenceLabel.Location = new System.Drawing.Point(3, 202);
		this._libraryReferenceLabel.Name = "_libraryReferenceLabel";
		this._libraryReferenceLabel.Size = new System.Drawing.Size(46, 13);
		this._libraryReferenceLabel.TabIndex = 7;
		this._libraryReferenceLabel.Text = "Libraries";
		this._libraryReferences.FormattingEnabled = true;
		this._libraryReferences.Location = new System.Drawing.Point(6, 218);
		this._libraryReferences.Name = "_libraryReferences";
		this._libraryReferences.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
		this._libraryReferences.Size = new System.Drawing.Size(246, 108);
		this._libraryReferences.TabIndex = 6;
		this._libraryReferences.SelectedIndexChanged += new System.EventHandler(_libraryReferences_SelectedIndexChanged);
		this._libraryComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
		this._libraryComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
		this._libraryComboBox.FormattingEnabled = true;
		this._libraryComboBox.Location = new System.Drawing.Point(55, 194);
		this._libraryComboBox.Name = "_libraryComboBox";
		this._libraryComboBox.Size = new System.Drawing.Size(121, 21);
		this._libraryComboBox.TabIndex = 10;
		this._libraryComboBox.SelectedIndexChanged += new System.EventHandler(_libraryComboBox_SelectedIndexChanged);
		this._loadsLibraries.AutoSize = true;
		this._loadsLibraries.Location = new System.Drawing.Point(8, 332);
		this._loadsLibraries.Name = "_loadsLibraries";
		this._loadsLibraries.Size = new System.Drawing.Size(97, 17);
		this._loadsLibraries.TabIndex = 11;
		this._loadsLibraries.Text = "Loads Libraries";
		this._loadsLibraries.UseVisualStyleBackColor = true;
		this._loadsLibraries.CheckedChanged += new System.EventHandler(_loadsLibraries_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this._loadsLibraries);
		base.Controls.Add(this._libraryComboBox);
		base.Controls.Add(this._removeLibraryReference);
		base.Controls.Add(this._libraryReferenceLabel);
		base.Controls.Add(this._libraryReferences);
		base.Controls.Add(this._removeArtDefsButton);
		base.Controls.Add(this._addArtDefsButton);
		base.Controls.Add(this._artDefLabel);
		base.Controls.Add(this._artDefsListBox);
		base.Controls.Add(this._consumerNameTextBox);
		base.Controls.Add(this._consumerNameLabel);
		base.Name = "ArtConsumerEditorControl";
		base.Size = new System.Drawing.Size(255, 358);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
