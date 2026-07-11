using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class GameArtSpecificationContext : EditingContext, IGameArtSpecificationEditingContext, IObservableContext, IDisposable
{
	private ControlInfo m_controlInfo;

	private GameArtEditorControl m_gui;

	private GameArtSpecificationDocument m_document;

	private GameArtSpecificationAdapter _adapter;

	public ControlInfo ControlInfo
	{
		get
		{
			return m_controlInfo;
		}
		set
		{
			m_controlInfo = value;
		}
	}

	public Control GUI => m_gui;

	public GameArtSpecificationDocument Doc
	{
		get
		{
			return m_document;
		}
		set
		{
			if (m_document != value)
			{
				SafeDisposeGUI();
				m_document = value;
				m_gui = new GameArtEditorControl(ProjectMapService);
				m_gui.Bind(this);
			}
		}
	}

	public IFileDialogService FileDialogService { get; set; }

	public IProjectMapService ProjectMapService { get; set; }

	private GameArtSpecificationAdapter Adapter
	{
		get
		{
			if (_adapter == null)
			{
				_adapter = base.DomNode.As<GameArtSpecificationAdapter>();
			}
			return _adapter;
		}
	}

	public string GameName
	{
		get
		{
			return Adapter.Name;
		}
		set
		{
			if (Adapter.Name != value)
			{
				this.DoTransaction(delegate
				{
					Adapter.Name = value;
					Adapter.ID = ID;
				}, "Update Game Name.");
			}
		}
	}

	public string ID
	{
		get
		{
			return Adapter.ID;
		}
		set
		{
			if (Adapter.ID != value)
			{
				this.DoTransaction(delegate
				{
					Adapter.Name = GameName;
					Adapter.ID = ID;
				}, "Update Game Name.");
			}
		}
	}

	public bool IsNewGameArt => base.DomNode.As<GameArtSpecificationDocument>().IsNewGameArt;

	public IEnumerable<ArtConsumerAdapter> ArtConsumers => Adapter.ArtConsumers.ArtConsumers;

	public IEnumerable<GameLibraryAdapter> GameLibraries => Adapter.GameLibraries.GameLibraries;

	public IEnumerable<GameArtIDAdapter> GameDependencies => Adapter.RequiredGameArtIDs.GameArtIDs;

	public IEnumerable<string> ParentArtConsumers
	{
		get
		{
			SortedSet<string> sortedSet = new SortedSet<string>();
			string[] array = GameDependencies.Select((GameArtIDAdapter dep) => dep.GameName).ToArray();
			foreach (string name in array)
			{
				ProjectEnvironment project = null;
				if (!ProjectMapService.AllProjectsMap.GetProject(name, ref project))
				{
					continue;
				}
				foreach (IGameArtSpecification artSpecification in project.ArtSpecifications)
				{
					string[] other = artSpecification.ArtConsumers.Select((IArtConsumer consumer) => consumer.ConsumerName).ToArray();
					sortedSet.UnionWith(other);
				}
			}
			return sortedSet;
		}
	}

	public IEnumerable<string> ParentGameLibraries
	{
		get
		{
			SortedSet<string> sortedSet = new SortedSet<string>();
			string[] array = GameDependencies.Select((GameArtIDAdapter dep) => dep.GameName).ToArray();
			foreach (string name in array)
			{
				ProjectEnvironment project = null;
				if (!ProjectMapService.AllProjectsMap.GetProject(name, ref project))
				{
					continue;
				}
				foreach (IGameArtSpecification artSpecification in project.ArtSpecifications)
				{
					string[] other = artSpecification.GameLibraries.Select((IGameLibrary library) => library.LibraryName).ToArray();
					sortedSet.UnionWith(other);
				}
			}
			return sortedSet;
		}
	}

	public IEnumerable<IGameArtSpecification> AvailableArtSpecifications
	{
		get
		{
			foreach (ProjectEnvironment project in ProjectMapService.AllProjectsMap.Projects)
			{
				foreach (IGameArtSpecification artSpecification in project.ArtSpecifications)
				{
					if (!string.IsNullOrEmpty(artSpecification.ID.Name))
					{
						yield return artSpecification;
					}
				}
			}
		}
	}

	DomNode IGameArtSpecificationEditingContext.DomNode => base.DomNode;

	public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

	public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

	public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

	public event EventHandler Reloaded;

	public GameArtSpecificationContext()
	{
		_ = this.Reloaded;
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.DomNode.ChildInserted += DomNode_ChildInserted;
		base.DomNode.ChildRemoved += DomNode_ChildRemoved;
		base.OnNodeSet();
	}

	public void DisableGenerateIDButton()
	{
		if (m_gui != null)
		{
			m_gui.EnableGenerateIDButton = false;
		}
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		this.ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
	}

	private void DomNode_ChildInserted(object sender, ChildEventArgs e)
	{
		this.ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child));
	}

	private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
	{
		this.ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child));
	}

	public void Dispose()
	{
		Dispose(bDisposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool bDisposing)
	{
		if (bDisposing)
		{
			SafeDisposeGUI();
		}
	}

	private void SafeDisposeGUI()
	{
		if (m_gui != null)
		{
			m_gui.Dispose();
			m_gui = null;
		}
	}

	public void GenerateNewID()
	{
		ID = Guid.NewGuid().ToString();
	}

	public void AddArtConsumer()
	{
		string newConsumerName = Adapter.GenerateNewConsumerName();
		this.DoTransaction(delegate
		{
			Adapter.AddConsumer(newConsumerName);
		}, "Add new consumer.");
	}

	public void AddArtConsumerFromParent(string consumerName)
	{
		this.DoTransaction(delegate
		{
			Adapter.AddConsumer(consumerName);
		}, "Add consumer from parent.");
	}

	public void RemoveArtConsumer(IEnumerable<ArtConsumerAdapter> artConsumers)
	{
		this.DoTransaction(delegate
		{
			foreach (ArtConsumerAdapter artConsumer in artConsumers)
			{
				Adapter.RemoveConsumer(artConsumer.ConsumerName);
			}
		}, "Removing art consumers");
	}

	public void AddGameLibrary()
	{
		string newLibraryName = Adapter.GenerateNewLibraryName();
		this.DoTransaction(delegate
		{
			Adapter.AddGameLibrary(newLibraryName);
		}, "Add new library.");
	}

	public void AddGameLibraryFromParent(string libraryName)
	{
		this.DoTransaction(delegate
		{
			Adapter.AddGameLibrary(libraryName);
		}, "Add library from parent.");
	}

	public void RemoveGameLibraries(IEnumerable<GameLibraryAdapter> gameLibraries)
	{
		this.DoTransaction(delegate
		{
			foreach (GameLibraryAdapter gameLibrary in gameLibraries)
			{
				Adapter.RemoveGameLibrary(gameLibrary.LibraryName);
			}
		}, "Removing game libraries");
	}

	public void AddGameArtDependency(IEnumerable<IGameArtSpecification> gameArtSpecifications)
	{
		this.DoTransaction(delegate
		{
			foreach (IGameArtSpecification gameArtSpecification in gameArtSpecifications)
			{
				IGameArtID iD = gameArtSpecification.ID;
				if (iD.ID == ID)
				{
					MessageBox.Show("A Game Art Specification cannot depend on itself.");
				}
				else if (gameArtSpecification.RequiredGameArtIDs.Select((IGameArtID dependency) => dependency.ID).Any((string guid) => guid == ID))
				{
					MessageBox.Show("A Game Art Specification cannot depend on a project that depends on it  (no circular dependencies).");
				}
				else
				{
					Adapter.AddRequiredGameArtID(iD.Name, iD.ID);
				}
			}
		}, "Adding game dependencies.");
	}

	public void RemoveGameArtDependency(IEnumerable<GameArtIDAdapter> gameDependencies)
	{
		this.DoTransaction(delegate
		{
			GameArtIDAdapter[] array = gameDependencies.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				Adapter.RemoveRequiredGameArtID(array[i].GameArtID);
			}
		}, "Removing game dependencies.");
	}
}
