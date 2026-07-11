using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Controls;
using Firaxis.Utility;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class GameArtEditorControl : UserControl
{
	private ArtConsumerEditorControl _artConsumerEditorControl = new ArtConsumerEditorControl();

	private LibraryEditorControl _libraryEditorControl = new LibraryEditorControl();

	private IProjectMapService _projectMapService;

	private GameLibraryAdapter[] removeLibrariesParameter = new GameLibraryAdapter[1];

	private IContainer components;

	private SplitContainer _panelSplitter;

	private Button _removeArtConsumer;

	private Button _addArtConsumer;

	private Label _consumersLabel;

	private ListBox _artConsumerListBox;

	private Button _removeDependencyButton;

	private Button _addDependencyButton;

	private Label _dependenciesLabel;

	private ListBox _dependencyListBox;

	private TextBox _idTextbox;

	private Label _idLabel;

	private Button _generateIDButton;

	private ComboBox _nameTextBox;

	private Label _nameLabel;

	private Label _libraryLabel;

	private ListBox _libraryListBox;

	private Button _removeLibraryButton;

	private Button _addLibraryButton;

	private ComboBox _libraryComboBox;

	private ComboBox _consumerComboBox;

	private ListBox _availableProjectList;

	private IGameArtSpecificationEditingContext EditingContext { get; set; }

	public string GameName
	{
		get
		{
			return _nameTextBox.Text;
		}
		set
		{
			_nameTextBox.Text = value;
		}
	}

	public string ID
	{
		get
		{
			return _idTextbox.Text;
		}
		set
		{
			_idTextbox.Text = value;
		}
	}

	public IEnumerable<ArtConsumerAdapter> ArtConsumers
	{
		get
		{
			return _artConsumerListBox.Items.OfType<ArtConsumerAdapter>();
		}
		set
		{
			UpdateListBox(_artConsumerListBox, value, _systemListBox_SelectedIndexChanged);
		}
	}

	public IEnumerable<GameLibraryAdapter> GameLibraries
	{
		get
		{
			return _libraryListBox.Items.OfType<GameLibraryAdapter>();
		}
		set
		{
			UpdateListBox(_libraryListBox, value, _libraryListBox_SelectedIndexChanged);
		}
	}

	public IEnumerable<string> ParentArtConsumers
	{
		get
		{
			return _consumerComboBox.Items.OfType<string>();
		}
		set
		{
			UpdateComboBox(_consumerComboBox, value, _consumerComboBox_SelectedIndexChanged);
		}
	}

	public IEnumerable<string> ParentGameLibraries
	{
		get
		{
			return _libraryComboBox.Items.OfType<string>();
		}
		set
		{
			UpdateComboBox(_libraryComboBox, value, _libraryComboBox_SelectedIndexChanged);
		}
	}

	public IEnumerable<IGameArtSpecification> SelectedGameProjects => _availableProjectList.SelectedItems.OfType<IGameArtSpecification>();

	public ArtConsumerAdapter SelectedArtConsumer
	{
		get
		{
			return _artConsumerListBox.SelectedItem as ArtConsumerAdapter;
		}
		set
		{
			_artConsumerListBox.SelectedItem = value;
		}
	}

	public GameLibraryAdapter SelectedGameLibrary
	{
		get
		{
			return _libraryListBox.SelectedItem as GameLibraryAdapter;
		}
		set
		{
			_libraryListBox.SelectedItem = value;
		}
	}

	public IEnumerable<GameArtIDAdapter> RequiredGameIDs
	{
		get
		{
			return _dependencyListBox.Items.OfType<GameArtIDAdapter>();
		}
		set
		{
			_dependencyListBox.Items.Clear();
			foreach (GameArtIDAdapter item in value)
			{
				_dependencyListBox.Items.Add(item);
			}
		}
	}

	private IEnumerable<GameArtIDAdapter> SelectedGameIDs => _dependencyListBox.SelectedItems.OfType<GameArtIDAdapter>();

	public bool EnableGenerateIDButton
	{
		get
		{
			return _generateIDButton.Enabled;
		}
		set
		{
			_generateIDButton.Enabled = value;
		}
	}

	private void UpdateListBox(ListBox listBox, IEnumerable<object> newValues, EventHandler indexChangedHandler)
	{
		listBox.SuspendDrawing();
		listBox.SelectedIndexChanged -= indexChangedHandler;
		object selectedItem = listBox.SelectedItem;
		listBox.Items.Clear();
		listBox.Items.AddRange(newValues.ToArray());
		listBox.SelectedItem = selectedItem;
		listBox.SelectedIndexChanged += indexChangedHandler;
		listBox.ResumeDrawing();
	}

	private void UpdateComboBox(ComboBox comboBox, IEnumerable<object> newValues, EventHandler indexChangedHandler)
	{
		comboBox.SuspendDrawing();
		comboBox.SelectedIndexChanged -= indexChangedHandler;
		comboBox.Items.Clear();
		comboBox.Items.AddRange(newValues.ToArray());
		comboBox.SelectedIndexChanged += indexChangedHandler;
		comboBox.ResumeDrawing();
	}

	public GameArtEditorControl(IProjectMapService pms)
	{
		_projectMapService = pms;
		InitializeComponent();
		_panelSplitter.Panel2Collapsed = true;
		_removeDependencyButton.Enabled = false;
		_removeArtConsumer.Enabled = false;
		_libraryEditorControl.Dock = DockStyle.Fill;
		_artConsumerEditorControl.Dock = DockStyle.Fill;
		_dependencyListBox.SelectedIndexChanged += _dependencyListBox_SelectedIndexChanged;
	}

	private void PopulateGameNames()
	{
		string selectedText = _nameTextBox.SelectedText;
		_nameTextBox.SelectedText = "";
		_nameTextBox.Items.Clear();
		foreach (ProjectEnvironment project in _projectMapService.AllProjectsMap.Projects)
		{
			_nameTextBox.Items.Add(project.Name);
		}
		if (selectedText != null && _nameTextBox.Items.Contains(selectedText))
		{
			_nameTextBox.SelectedText = selectedText;
		}
	}

	public void Bind(IGameArtSpecificationEditingContext context)
	{
		EditingContext = context;
		PopulateGameNames();
		GameName = EditingContext.GameName;
		ID = EditingContext.ID;
		ArtConsumers = EditingContext.ArtConsumers;
		RequiredGameIDs = EditingContext.GameDependencies;
		GameLibraries = EditingContext.GameLibraries;
		ParentArtConsumers = EditingContext.ParentArtConsumers;
		ParentGameLibraries = EditingContext.ParentGameLibraries;
		_availableProjectList.Items.Clear();
		IGameArtSpecification[] array = EditingContext.AvailableArtSpecifications.ToArray();
		ListBox.ObjectCollection items = _availableProjectList.Items;
		object[] array2 = array;
		object[] items2 = array2;
		items.AddRange(items2);
		EnableGenerateIDButton = context.IsNewGameArt;
		context.DomNode.AttributeChanged += HandleAttributeChanged;
		context.DomNode.ChildInserted += HandleChildInserted;
		context.DomNode.ChildRemoved += HandleChildRemoved;
		bool hasDependencies = RequiredGameIDs.Any();
		UpdateParentDisplayVisibility(hasDependencies);
	}

	private void HandleAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == GameArtSpecificationSchema.GameArtSpecificationType.NameAttribute)
		{
			GameName = (string)e.NewValue;
		}
		else if (e.AttributeInfo == GameArtSpecificationSchema.GameArtSpecificationType.IDAttribute)
		{
			ID = (string)e.NewValue;
		}
		else if (e.AttributeInfo == GameArtSpecificationSchema.ArtConsumerType.ConsumerNameAttribute)
		{
			ArtConsumerAdapter artConsumerAdapter = e.DomNode.As<ArtConsumerAdapter>();
			if (artConsumerAdapter != null)
			{
				_artConsumerListBox.RefreshListBoxItem(artConsumerAdapter);
			}
		}
		else if (e.AttributeInfo == GameArtSpecificationSchema.GameLibraryType.LibraryNameAttribute)
		{
			GameLibraryAdapter gameLibraryAdapter = e.DomNode.As<GameLibraryAdapter>();
			if (gameLibraryAdapter != null)
			{
				_libraryListBox.RefreshListBoxItem(gameLibraryAdapter);
			}
		}
		else if (e.AttributeInfo == GameArtSpecificationSchema.GameArtIDType.GameArtIDAttribute || e.AttributeInfo == GameArtSpecificationSchema.GameArtIDType.GameNameAttribute)
		{
			GameArtIDAdapter gameArtIDAdapter = e.DomNode.As<GameArtIDAdapter>();
			if (gameArtIDAdapter != null)
			{
				_dependencyListBox.RefreshListBoxItem(gameArtIDAdapter);
			}
		}
	}

	private void HandleChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == GameArtSpecificationSchema.ArtConsumerContainerType.ArtConsumersChild)
		{
			ArtConsumerAdapter artConsumerAdapter = e.Child.As<ArtConsumerAdapter>();
			if (artConsumerAdapter != null)
			{
				_artConsumerListBox.SafeInsertAndSelect(artConsumerAdapter, e.Index);
			}
		}
		else if (e.ChildInfo == GameArtSpecificationSchema.GameLibraryContainerType.GameLibrariesChild)
		{
			GameLibraryAdapter gameLibraryAdapter = e.Child.As<GameLibraryAdapter>();
			if (gameLibraryAdapter != null)
			{
				_libraryListBox.SafeInsertAndSelect(gameLibraryAdapter, e.Index);
			}
		}
		else if (e.ChildInfo == GameArtSpecificationSchema.RequiredGameArtIDContainerType.RequiredGameArtIDsChild)
		{
			GameArtIDAdapter gameArtIDAdapter = e.Child.As<GameArtIDAdapter>();
			if (gameArtIDAdapter != null)
			{
				_dependencyListBox.SafeInsertAndSelect(gameArtIDAdapter, e.Index);
				bool hasDependencies = _dependencyListBox.Items.Count > 0;
				UpdateParentDisplayVisibility(hasDependencies);
				ParentArtConsumers = EditingContext.ParentArtConsumers;
				ParentGameLibraries = EditingContext.ParentGameLibraries;
			}
		}
	}

	private void HandleChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == GameArtSpecificationSchema.ArtConsumerContainerType.ArtConsumersChild)
		{
			ArtConsumerAdapter artConsumerAdapter = e.Child.As<ArtConsumerAdapter>();
			if (artConsumerAdapter != null)
			{
				_artConsumerListBox.SafeRemove(artConsumerAdapter, e.Index);
			}
		}
		else if (e.ChildInfo == GameArtSpecificationSchema.GameLibraryContainerType.GameLibrariesChild)
		{
			GameLibraryAdapter gameLibraryAdapter = e.Child.As<GameLibraryAdapter>();
			if (gameLibraryAdapter != null)
			{
				_libraryListBox.SafeRemove(gameLibraryAdapter, e.Index);
			}
		}
		else if (e.ChildInfo == GameArtSpecificationSchema.RequiredGameArtIDContainerType.RequiredGameArtIDsChild)
		{
			GameArtIDAdapter gameArtIDAdapter = e.Child.As<GameArtIDAdapter>();
			if (gameArtIDAdapter != null)
			{
				_dependencyListBox.SafeRemove(gameArtIDAdapter, e.Index);
				bool hasDependencies = _dependencyListBox.Items.Count > 0;
				UpdateParentDisplayVisibility(hasDependencies);
				ParentArtConsumers = EditingContext.ParentArtConsumers;
				ParentGameLibraries = EditingContext.ParentGameLibraries;
			}
		}
	}

	private void UpdateParentDisplayVisibility(bool hasDependencies)
	{
		_libraryComboBox.Visible = hasDependencies;
		_libraryComboBox.Enabled = hasDependencies;
		_consumerComboBox.Enabled = hasDependencies;
		_consumerComboBox.Visible = hasDependencies;
	}

	private void _generateIDButton_Click(object sender, EventArgs e)
	{
		if (EditingContext != null)
		{
			EditingContext.GenerateNewID();
		}
	}

	private void _addArtConsumer_Click(object sender, EventArgs e)
	{
		if (EditingContext != null)
		{
			EditingContext.AddArtConsumer();
		}
	}

	private void _removeArtConsumer_Click(object sender, EventArgs e)
	{
		if (EditingContext != null)
		{
			EditingContext.RemoveArtConsumer(new ArtConsumerAdapter[1] { SelectedArtConsumer });
		}
		bool flag = _artConsumerListBox.HasSelection();
		_panelSplitter.Panel2Collapsed = !flag;
		_removeArtConsumer.Enabled = flag;
	}

	private void _addDependencyButton_Click(object sender, EventArgs e)
	{
		if (EditingContext != null)
		{
			EditingContext.AddGameArtDependency(SelectedGameProjects);
		}
	}

	private void _removeDependencyButton_Click(object sender, EventArgs e)
	{
		if (EditingContext != null)
		{
			EditingContext.RemoveGameArtDependency(SelectedGameIDs);
		}
		_removeDependencyButton.Enabled = _dependencyListBox.HasSelection();
	}

	private void _nameTextBox_Click(object sender, EventArgs e)
	{
		if (EditingContext != null)
		{
			EditingContext.GameName = GameName;
		}
	}

	private void _systemListBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		bool flag = _artConsumerListBox.HasSelection();
		_panelSplitter.Panel2Collapsed = !flag;
		_removeArtConsumer.Enabled = flag;
		if (flag)
		{
			_artConsumerEditorControl.Bind(SelectedArtConsumer.As<IArtConsumerEditingContext>());
			RemoveControlFromPanel2(_libraryEditorControl);
			AddControlToPanel2(_artConsumerEditorControl);
			SetMinWidth();
		}
		else
		{
			_artConsumerEditorControl.Bind(null);
			RemoveControlFromPanel2(_artConsumerEditorControl);
		}
	}

	private void _dependencyListBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		_removeDependencyButton.Enabled = _dependencyListBox.HasSelection();
	}

	private void SetMinWidth()
	{
		Size minimumSize = MinimumSize;
		minimumSize.Width = (_panelSplitter.Panel2Collapsed ? _panelSplitter.Panel1.Width : _panelSplitter.Width);
		MinimumSize = minimumSize;
	}

	private void _addLibraryButton_Click(object sender, EventArgs e)
	{
		EditingContext.AddGameLibrary();
	}

	private void _removeLibraryButton_Click(object sender, EventArgs e)
	{
		removeLibrariesParameter[0] = SelectedGameLibrary;
		EditingContext.RemoveGameLibraries(removeLibrariesParameter);
	}

	private void _libraryListBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		bool flag = _libraryListBox.HasSelection();
		_panelSplitter.Panel2Collapsed = !flag;
		_removeLibraryButton.Enabled = flag;
		if (flag)
		{
			_libraryEditorControl.Bind(SelectedGameLibrary.As<ILibraryEditingContext>());
			RemoveControlFromPanel2(_artConsumerEditorControl);
			AddControlToPanel2(_libraryEditorControl);
			SetMinWidth();
		}
		else
		{
			_libraryEditorControl.Bind(null);
			RemoveControlFromPanel2(_libraryEditorControl);
		}
	}

	private void RemoveControlFromPanel2(Control control)
	{
		if (_panelSplitter.Panel2.Contains(control))
		{
			_panelSplitter.Panel2.Controls.Remove(control);
		}
	}

	private void AddControlToPanel2(Control control)
	{
		if (!_panelSplitter.Panel2.Contains(control))
		{
			_panelSplitter.Panel2.Controls.Add(control);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (EditingContext != null)
			{
				EditingContext.DomNode.AttributeChanged -= HandleAttributeChanged;
				EditingContext.DomNode.ChildInserted -= HandleChildInserted;
				EditingContext.DomNode.ChildRemoved -= HandleChildRemoved;
				EditingContext = null;
			}
			ArtConsumers = Enumerable.Empty<ArtConsumerAdapter>();
			GameLibraries = Enumerable.Empty<GameLibraryAdapter>();
			RequiredGameIDs = Enumerable.Empty<GameArtIDAdapter>();
			_artConsumerListBox.SelectedIndexChanged -= _systemListBox_SelectedIndexChanged;
			_dependencyListBox.SelectedIndexChanged -= _dependencyListBox_SelectedIndexChanged;
			_libraryListBox.SelectedIndexChanged -= _libraryListBox_SelectedIndexChanged;
			_libraryComboBox.SelectedIndexChanged -= _libraryComboBox_SelectedIndexChanged;
			_consumerComboBox.SelectedIndexChanged -= _consumerComboBox_SelectedIndexChanged;
			_artConsumerEditorControl.Dispose();
			_libraryEditorControl.Dispose();
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private void _libraryComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		int selectedIndex = _libraryComboBox.SelectedIndex;
		if (selectedIndex >= 0)
		{
			string libraryName = (string)_libraryComboBox.Items[selectedIndex];
			EditingContext.AddGameLibraryFromParent(libraryName);
			_libraryComboBox.SelectedIndex = -1;
		}
	}

	private void _consumerComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		int selectedIndex = _consumerComboBox.SelectedIndex;
		if (selectedIndex >= 0)
		{
			string consumerName = (string)_consumerComboBox.Items[selectedIndex];
			EditingContext.AddArtConsumerFromParent(consumerName);
			_consumerComboBox.SelectedIndex = -1;
		}
	}

	private void InitializeComponent()
	{
		this._panelSplitter = new System.Windows.Forms.SplitContainer();
		this._availableProjectList = new System.Windows.Forms.ListBox();
		this._libraryComboBox = new System.Windows.Forms.ComboBox();
		this._consumerComboBox = new System.Windows.Forms.ComboBox();
		this._removeLibraryButton = new System.Windows.Forms.Button();
		this._addLibraryButton = new System.Windows.Forms.Button();
		this._libraryLabel = new System.Windows.Forms.Label();
		this._libraryListBox = new System.Windows.Forms.ListBox();
		this._removeArtConsumer = new System.Windows.Forms.Button();
		this._addArtConsumer = new System.Windows.Forms.Button();
		this._consumersLabel = new System.Windows.Forms.Label();
		this._artConsumerListBox = new System.Windows.Forms.ListBox();
		this._removeDependencyButton = new System.Windows.Forms.Button();
		this._addDependencyButton = new System.Windows.Forms.Button();
		this._dependenciesLabel = new System.Windows.Forms.Label();
		this._dependencyListBox = new System.Windows.Forms.ListBox();
		this._idTextbox = new System.Windows.Forms.TextBox();
		this._idLabel = new System.Windows.Forms.Label();
		this._generateIDButton = new System.Windows.Forms.Button();
		this._nameTextBox = new System.Windows.Forms.ComboBox();
		this._nameLabel = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this._panelSplitter).BeginInit();
		this._panelSplitter.Panel1.SuspendLayout();
		this._panelSplitter.SuspendLayout();
		base.SuspendLayout();
		this._panelSplitter.Location = new System.Drawing.Point(3, 3);
		this._panelSplitter.Name = "_panelSplitter";
		this._panelSplitter.Panel1.Controls.Add(this._availableProjectList);
		this._panelSplitter.Panel1.Controls.Add(this._libraryComboBox);
		this._panelSplitter.Panel1.Controls.Add(this._consumerComboBox);
		this._panelSplitter.Panel1.Controls.Add(this._removeLibraryButton);
		this._panelSplitter.Panel1.Controls.Add(this._addLibraryButton);
		this._panelSplitter.Panel1.Controls.Add(this._libraryLabel);
		this._panelSplitter.Panel1.Controls.Add(this._libraryListBox);
		this._panelSplitter.Panel1.Controls.Add(this._removeArtConsumer);
		this._panelSplitter.Panel1.Controls.Add(this._addArtConsumer);
		this._panelSplitter.Panel1.Controls.Add(this._consumersLabel);
		this._panelSplitter.Panel1.Controls.Add(this._artConsumerListBox);
		this._panelSplitter.Panel1.Controls.Add(this._removeDependencyButton);
		this._panelSplitter.Panel1.Controls.Add(this._addDependencyButton);
		this._panelSplitter.Panel1.Controls.Add(this._dependenciesLabel);
		this._panelSplitter.Panel1.Controls.Add(this._dependencyListBox);
		this._panelSplitter.Panel1.Controls.Add(this._idTextbox);
		this._panelSplitter.Panel1.Controls.Add(this._idLabel);
		this._panelSplitter.Panel1.Controls.Add(this._generateIDButton);
		this._panelSplitter.Panel1.Controls.Add(this._nameTextBox);
		this._panelSplitter.Panel1.Controls.Add(this._nameLabel);
		this._panelSplitter.Size = new System.Drawing.Size(812, 426);
		this._panelSplitter.SplitterDistance = 545;
		this._panelSplitter.TabIndex = 9;
		this._availableProjectList.HorizontalScrollbar = true;
		this._availableProjectList.Location = new System.Drawing.Point(362, 293);
		this._availableProjectList.Name = "_availableProjectList";
		this._availableProjectList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
		this._availableProjectList.Size = new System.Drawing.Size(180, 121);
		this._availableProjectList.TabIndex = 27;
		this._libraryComboBox.Enabled = false;
		this._libraryComboBox.FormattingEnabled = true;
		this._libraryComboBox.Location = new System.Drawing.Point(331, 208);
		this._libraryComboBox.Name = "_libraryComboBox";
		this._libraryComboBox.Size = new System.Drawing.Size(156, 21);
		this._libraryComboBox.TabIndex = 26;
		this._libraryComboBox.Visible = false;
		this._libraryComboBox.SelectedIndexChanged += new System.EventHandler(_libraryComboBox_SelectedIndexChanged);
		this._consumerComboBox.Enabled = false;
		this._consumerComboBox.FormattingEnabled = true;
		this._consumerComboBox.Location = new System.Drawing.Point(331, 91);
		this._consumerComboBox.Name = "_consumerComboBox";
		this._consumerComboBox.Size = new System.Drawing.Size(156, 21);
		this._consumerComboBox.TabIndex = 25;
		this._consumerComboBox.Visible = false;
		this._consumerComboBox.SelectedIndexChanged += new System.EventHandler(_consumerComboBox_SelectedIndexChanged);
		this._removeLibraryButton.Enabled = false;
		this._removeLibraryButton.Location = new System.Drawing.Point(412, 179);
		this._removeLibraryButton.Name = "_removeLibraryButton";
		this._removeLibraryButton.Size = new System.Drawing.Size(75, 23);
		this._removeLibraryButton.TabIndex = 8;
		this._removeLibraryButton.Text = "Remove";
		this._removeLibraryButton.UseVisualStyleBackColor = true;
		this._removeLibraryButton.Click += new System.EventHandler(_removeLibraryButton_Click);
		this._addLibraryButton.Location = new System.Drawing.Point(331, 179);
		this._addLibraryButton.Name = "_addLibraryButton";
		this._addLibraryButton.Size = new System.Drawing.Size(75, 23);
		this._addLibraryButton.TabIndex = 7;
		this._addLibraryButton.Text = "Add...";
		this._addLibraryButton.UseVisualStyleBackColor = true;
		this._addLibraryButton.Click += new System.EventHandler(_addLibraryButton_Click);
		this._libraryLabel.AutoSize = true;
		this._libraryLabel.Location = new System.Drawing.Point(5, 179);
		this._libraryLabel.Name = "_libraryLabel";
		this._libraryLabel.Size = new System.Drawing.Size(46, 13);
		this._libraryLabel.TabIndex = 24;
		this._libraryLabel.Text = "Libraries";
		this._libraryListBox.HorizontalScrollbar = true;
		this._libraryListBox.Location = new System.Drawing.Point(87, 179);
		this._libraryListBox.Name = "_libraryListBox";
		this._libraryListBox.Size = new System.Drawing.Size(238, 108);
		this._libraryListBox.TabIndex = 6;
		this._libraryListBox.SelectedIndexChanged += new System.EventHandler(_libraryListBox_SelectedIndexChanged);
		this._removeArtConsumer.Location = new System.Drawing.Point(412, 62);
		this._removeArtConsumer.Name = "_removeArtConsumer";
		this._removeArtConsumer.Size = new System.Drawing.Size(75, 23);
		this._removeArtConsumer.TabIndex = 5;
		this._removeArtConsumer.Text = "Remove";
		this._removeArtConsumer.UseVisualStyleBackColor = true;
		this._removeArtConsumer.Click += new System.EventHandler(_removeArtConsumer_Click);
		this._addArtConsumer.Location = new System.Drawing.Point(331, 62);
		this._addArtConsumer.Name = "_addArtConsumer";
		this._addArtConsumer.Size = new System.Drawing.Size(75, 23);
		this._addArtConsumer.TabIndex = 4;
		this._addArtConsumer.Text = "Add...";
		this._addArtConsumer.UseVisualStyleBackColor = true;
		this._addArtConsumer.Click += new System.EventHandler(_addArtConsumer_Click);
		this._consumersLabel.AutoSize = true;
		this._consumersLabel.Location = new System.Drawing.Point(5, 62);
		this._consumersLabel.Name = "_consumersLabel";
		this._consumersLabel.Size = new System.Drawing.Size(75, 13);
		this._consumersLabel.TabIndex = 20;
		this._consumersLabel.Text = "Art Consumers";
		this._artConsumerListBox.HorizontalScrollbar = true;
		this._artConsumerListBox.Location = new System.Drawing.Point(87, 60);
		this._artConsumerListBox.Name = "_artConsumerListBox";
		this._artConsumerListBox.Size = new System.Drawing.Size(238, 108);
		this._artConsumerListBox.TabIndex = 3;
		this._artConsumerListBox.SelectedIndexChanged += new System.EventHandler(_systemListBox_SelectedIndexChanged);
		this._removeDependencyButton.Location = new System.Drawing.Point(331, 324);
		this._removeDependencyButton.Name = "_removeDependencyButton";
		this._removeDependencyButton.Size = new System.Drawing.Size(25, 23);
		this._removeDependencyButton.TabIndex = 11;
		this._removeDependencyButton.Text = "->";
		this._removeDependencyButton.UseVisualStyleBackColor = true;
		this._removeDependencyButton.Click += new System.EventHandler(_removeDependencyButton_Click);
		this._addDependencyButton.Location = new System.Drawing.Point(331, 295);
		this._addDependencyButton.Name = "_addDependencyButton";
		this._addDependencyButton.Size = new System.Drawing.Size(25, 23);
		this._addDependencyButton.TabIndex = 10;
		this._addDependencyButton.Text = "<-";
		this._addDependencyButton.UseVisualStyleBackColor = true;
		this._addDependencyButton.Click += new System.EventHandler(_addDependencyButton_Click);
		this._dependenciesLabel.AutoSize = true;
		this._dependenciesLabel.Location = new System.Drawing.Point(5, 295);
		this._dependenciesLabel.Name = "_dependenciesLabel";
		this._dependenciesLabel.Size = new System.Drawing.Size(76, 13);
		this._dependenciesLabel.TabIndex = 16;
		this._dependenciesLabel.Text = "Dependencies";
		this._dependencyListBox.HorizontalScrollbar = true;
		this._dependencyListBox.Location = new System.Drawing.Point(87, 293);
		this._dependencyListBox.Name = "_dependencyListBox";
		this._dependencyListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
		this._dependencyListBox.Size = new System.Drawing.Size(238, 121);
		this._dependencyListBox.TabIndex = 9;
		this._idTextbox.Enabled = false;
		this._idTextbox.Location = new System.Drawing.Point(44, 34);
		this._idTextbox.Name = "_idTextbox";
		this._idTextbox.Size = new System.Drawing.Size(281, 20);
		this._idTextbox.TabIndex = 1;
		this._idLabel.AutoSize = true;
		this._idLabel.Location = new System.Drawing.Point(5, 37);
		this._idLabel.Name = "_idLabel";
		this._idLabel.Size = new System.Drawing.Size(18, 13);
		this._idLabel.TabIndex = 13;
		this._idLabel.Text = "ID";
		this._generateIDButton.Location = new System.Drawing.Point(331, 32);
		this._generateIDButton.Name = "_generateIDButton";
		this._generateIDButton.Size = new System.Drawing.Size(75, 23);
		this._generateIDButton.TabIndex = 2;
		this._generateIDButton.Text = "Generate ID";
		this._generateIDButton.UseVisualStyleBackColor = true;
		this._generateIDButton.Click += new System.EventHandler(_generateIDButton_Click);
		this._nameTextBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this._nameTextBox.Location = new System.Drawing.Point(44, 8);
		this._nameTextBox.Name = "_nameTextBox";
		this._nameTextBox.Size = new System.Drawing.Size(281, 21);
		this._nameTextBox.TabIndex = 0;
		this._nameTextBox.Click += new System.EventHandler(_nameTextBox_Click);
		this._nameLabel.AutoSize = true;
		this._nameLabel.Location = new System.Drawing.Point(5, 11);
		this._nameLabel.Name = "_nameLabel";
		this._nameLabel.Size = new System.Drawing.Size(35, 13);
		this._nameLabel.TabIndex = 9;
		this._nameLabel.Text = "Name";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this._panelSplitter);
		this.MinimumSize = new System.Drawing.Size(818, 432);
		base.Name = "GameArtEditorControl";
		base.Size = new System.Drawing.Size(818, 432);
		this._panelSplitter.Panel1.ResumeLayout(false);
		this._panelSplitter.Panel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this._panelSplitter).EndInit();
		this._panelSplitter.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
