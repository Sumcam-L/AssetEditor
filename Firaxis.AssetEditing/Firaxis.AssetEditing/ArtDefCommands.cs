using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Firaxis.AssetEditing;

[Export(typeof(ArtDefCommands))]
[Export(typeof(IInitializable))]
[Export(typeof(IContextMenuCommandProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ArtDefCommands : IArtDefService, ICommandClient, IInitializable, IContextMenuCommandProvider
{
	private enum Command
	{
		ReloadProjectConfig,
		AddElement,
		RemoveElement,
		DuplicateElement,
		FixupBLPReferences,
		SaveMergedArtDefs
	}

	private struct ArtDefCommandTag
	{
		public Command Command;

		public ArtDefCommandTag(Command command)
		{
			Command = command;
		}
	}

	private static ArtDefCommandTag ReloadProjectConfigCommandTag = new ArtDefCommandTag(Command.ReloadProjectConfig);

	private static ArtDefCommandTag AddArtDefCommandTag = new ArtDefCommandTag(Command.AddElement);

	private static ArtDefCommandTag RemoveArtDefCommandTag = new ArtDefCommandTag(Command.RemoveElement);

	private static ArtDefCommandTag DuplicateArtDefCommandTag = new ArtDefCommandTag(Command.DuplicateElement);

	private static ArtDefCommandTag FixupBLPReferencesCommandTag = new ArtDefCommandTag(Command.FixupBLPReferences);

	private static ArtDefCommandTag SaveMergedArtDefsCommandTag = new ArtDefCommandTag(Command.SaveMergedArtDefs);

	private readonly IArtDefRegistry m_artDefRegistry;

	private protected ICivTechService CivTechService { get; set; }

	private protected ICommandService CommandService { get; set; }

	private protected IDocumentRegistry DocumentRegistry { get; set; }

	private protected IFileDialogService FileDialogService { get; set; }

	private protected IProjectConfigService ProjectConfigService { get; set; }

	public virtual event EventHandler<ItemInsertedEventArgs<object>> ElementAdded;

	public virtual event EventHandler<ItemRemovedEventArgs<object>> ElementRemoved;

	public virtual event EventHandler<EventArgs> ProjectConfigChanged;

	[ImportingConstructor]
	public ArtDefCommands(ICommandService commandService, IDocumentRegistry documentRegistry, IFileDialogService fileDialogService, IProjectConfigService projCfgSvc, ICivTechService civTechSvc, IArtDefRegistry artDefRegistry)
	{
		CommandService = commandService;
		DocumentRegistry = documentRegistry;
		FileDialogService = fileDialogService;
		ProjectConfigService = projCfgSvc;
		CivTechService = civTechSvc;
		m_artDefRegistry = artDefRegistry;
	}

	public virtual void AddElement(ArtDefCollectionAdapter collection)
	{
		ArtDefDocument activeDocument = DocumentRegistry.GetActiveDocument<ArtDefDocument>();
		string uniqueName = ArtDefContext.GenerateUniqueName(collection.ArtDefCollection.Elements, CivTechService.PrimaryProject.Name + "_" + Path.GetFileNameWithoutExtension(activeDocument.GetPathName()) + "_" + collection.ArtDefElementsTemplate.Name);
		collection.DomNode.GetRoot().As<ArtDefContext>().DoTransaction(delegate
		{
			collection.AddElement(uniqueName, -1);
		}, "Add Element".Localize());
	}

	public virtual void RemoveElement(ArtDefCollectionAdapter collection, IArtDefElement element)
	{
		string elementName = element.Name;
		collection.DomNode.GetRoot().As<ArtDefContext>().DoTransaction(delegate
		{
			collection.RemoveElement(elementName);
		}, "Remove Collection Value".Localize());
	}

	public virtual void DuplicateElement(ArtDefCollectionAdapter collection, IArtDefElement element)
	{
		ArtDefContext artDefContext = collection.DomNode.GetRoot().As<ArtDefContext>();
		artDefContext.DoTransaction(delegate
		{
			object dataObject = artDefContext.Copy();
			artDefContext.Insert(dataObject);
		}, "Duplicate Collection Value".Localize());
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		if (!(commandTag is ArtDefCommandTag artDefCommandTag))
		{
			return false;
		}
		if (artDefCommandTag.Command == Command.ReloadProjectConfig || artDefCommandTag.Command == Command.SaveMergedArtDefs)
		{
			return true;
		}
		if (DocumentRegistry.ActiveDocument == null)
		{
			return false;
		}
		if (DocumentRegistry.ActiveDocument.IsReadOnly)
		{
			return false;
		}
		ArtDefDocument activeDocument = DocumentRegistry.GetActiveDocument<ArtDefDocument>();
		if (activeDocument == null)
		{
			return false;
		}
		if (artDefCommandTag.Command == Command.FixupBLPReferences)
		{
			return true;
		}
		ArtDefContext artDefContext = activeDocument.As<ArtDefContext>();
		ArtDefCollectionAdapter artDefCollectionAdapter = artDefContext.Selection.LastSelected.As<ArtDefCollectionAdapter>();
		if (artDefCollectionAdapter != null)
		{
			if (artDefCommandTag.Command == Command.AddElement)
			{
				return true;
			}
			if (artDefCommandTag.Command == Command.RemoveElement)
			{
				return artDefCollectionAdapter.As<IIndexSelectionContext>()?.SelectedIndices.Any() ?? false;
			}
			if (artDefCommandTag.Command == Command.DuplicateElement)
			{
				return artDefCollectionAdapter.As<IIndexSelectionContext>()?.SelectedIndices.Any() ?? false;
			}
		}
		if (artDefCommandTag.Command == Command.AddElement && artDefContext.Selection.LastSelected.As<CollectionFieldValueAdapter>() != null)
		{
			return true;
		}
		if (artDefCommandTag.Command == Command.RemoveElement)
		{
			FieldValueAdapter fieldValueAdapter = artDefContext.Selection.LastSelected.As<FieldValueAdapter>();
			if (fieldValueAdapter != null && fieldValueAdapter.DomNode != null && fieldValueAdapter.DomNode.Parent != null && fieldValueAdapter.DomNode.Parent.As<CollectionFieldValueAdapter>() != null)
			{
				return true;
			}
		}
		if (artDefCommandTag.Command == Command.DuplicateElement)
		{
			FieldValueAdapter fieldValueAdapter2 = artDefContext.Selection.LastSelected.As<FieldValueAdapter>();
			if (fieldValueAdapter2 != null && fieldValueAdapter2.DomNode != null && fieldValueAdapter2.DomNode.Parent != null && fieldValueAdapter2.DomNode.Parent.As<CollectionFieldValueAdapter>() != null)
			{
				return true;
			}
		}
		ArtDefElementAdapter artDefElementAdapter = artDefContext.Selection.LastSelected.As<ArtDefElementAdapter>();
		return (artDefElementAdapter == null || artDefCommandTag.Command == Command.RemoveElement || artDefCommandTag.Command == Command.DuplicateElement) && (artDefElementAdapter != null || artDefCollectionAdapter != null);
	}

	public virtual void DoCommand(object commandTag)
	{
		if (!(commandTag is ArtDefCommandTag artDefCommandTag))
		{
			return;
		}
		_ = DocumentRegistry.ActiveDocument;
		if (artDefCommandTag.Command == Command.AddElement)
		{
			ArtDefContext artDefContext = DocumentRegistry.ActiveDocument.As<ArtDefContext>();
			ArtDefCollectionAdapter artDefCollectionAdapter = artDefContext.Selection.LastSelected.As<ArtDefCollectionAdapter>();
			if (artDefCollectionAdapter != null)
			{
				AddElement(artDefCollectionAdapter);
				return;
			}
			CollectionFieldValueAdapter collectionFieldValueAdapter = artDefContext.Selection.LastSelected.As<CollectionFieldValueAdapter>();
			if (collectionFieldValueAdapter != null)
			{
				AddElement(collectionFieldValueAdapter);
			}
		}
		else if (artDefCommandTag.Command == Command.RemoveElement)
		{
			ArtDefContext artDefContext2 = DocumentRegistry.ActiveDocument.As<ArtDefContext>();
			ArtDefElementAdapter artDefElementAdapter = artDefContext2.Selection.LastSelected.As<ArtDefElementAdapter>();
			ArtDefCollectionAdapter artDefCollectionAdapter2 = ((artDefElementAdapter == null) ? artDefContext2.Selection.LastSelected.As<ArtDefCollectionAdapter>() : artDefElementAdapter.DomNode.Parent.As<ArtDefCollectionAdapter>());
			if (artDefCollectionAdapter2 != null)
			{
				if (artDefContext2.Selection.Where((object ao) => ao.As<ArtDefElementAdapter>()?.ArtDefElement != null).Any())
				{
					IEnumerable<IArtDefElement> elements = (from ao in artDefContext2.Selection
						select ao.As<ArtDefElementAdapter>()?.ArtDefElement into ade
						where ade != null
						select ade).ToArray();
					RemoveElements(artDefCollectionAdapter2, elements);
					return;
				}
				IIndexSelectionContext indexSelectionContext = artDefCollectionAdapter2.As<IIndexSelectionContext>();
				if (indexSelectionContext != null && indexSelectionContext.SelectedIndices.Any())
				{
					IEnumerable<IArtDefElement> elements2 = (from adp in indexSelectionContext.SelectedObjects.OfType<ArtDefElementAdapter>()
						select adp.ArtDefElement).ToList();
					RemoveElements(artDefCollectionAdapter2, elements2);
				}
				return;
			}
			FieldValueAdapter fieldValueAdapter = artDefContext2.Selection.LastSelected.As<FieldValueAdapter>();
			if (fieldValueAdapter != null && fieldValueAdapter.DomNode != null && fieldValueAdapter.DomNode.Parent != null)
			{
				CollectionFieldValueAdapter collectionFieldValueAdapter2 = fieldValueAdapter.DomNode.Parent.As<CollectionFieldValueAdapter>();
				if (collectionFieldValueAdapter2 != null)
				{
					RemoveElement(collectionFieldValueAdapter2, fieldValueAdapter.Value);
				}
			}
		}
		else if (artDefCommandTag.Command == Command.DuplicateElement)
		{
			ArtDefContext artDefContext3 = DocumentRegistry.ActiveDocument.As<ArtDefContext>();
			ArtDefElementAdapter artDefElementAdapter2 = artDefContext3.Selection.LastSelected.As<ArtDefElementAdapter>();
			ArtDefCollectionAdapter artDefCollectionAdapter3 = ((artDefElementAdapter2 == null) ? artDefContext3.Selection.LastSelected.As<ArtDefCollectionAdapter>() : artDefElementAdapter2.DomNode.Parent.As<ArtDefCollectionAdapter>());
			if (artDefCollectionAdapter3 != null)
			{
				List<IArtDefElement> list = new List<IArtDefElement>();
				foreach (object item in artDefContext3.Selection)
				{
					ArtDefElementAdapter artDefElementAdapter3 = item.As<ArtDefElementAdapter>();
					if (artDefElementAdapter3 != null)
					{
						list.Add(artDefElementAdapter3.ArtDefElement);
					}
				}
				if (list.Count > 0)
				{
					DuplicateElements(artDefCollectionAdapter3, list);
				}
				return;
			}
			FieldValueAdapter fieldValueAdapter2 = artDefContext3.Selection.LastSelected.As<FieldValueAdapter>();
			if (fieldValueAdapter2 != null && fieldValueAdapter2.DomNode != null && fieldValueAdapter2.DomNode.Parent != null)
			{
				CollectionFieldValueAdapter collectionFieldValueAdapter3 = fieldValueAdapter2.DomNode.Parent.As<CollectionFieldValueAdapter>();
				if (collectionFieldValueAdapter3 != null)
				{
					DuplicateElement(collectionFieldValueAdapter3, fieldValueAdapter2.Value);
				}
			}
		}
		else if (artDefCommandTag.Command == Command.ReloadProjectConfig)
		{
			ReloadProjectConfig();
		}
		else if (artDefCommandTag.Command == Command.FixupBLPReferences)
		{
			FixupBLPReferences();
		}
		else if (artDefCommandTag.Command == Command.SaveMergedArtDefs)
		{
			SaveMergedArtDefs();
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	public virtual void Initialize()
	{
		RegisterClientCommands();
	}

	IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object clicked)
	{
		if (context.As<ISelectionContext>() != null)
		{
			return new object[3] { AddArtDefCommandTag, RemoveArtDefCommandTag, DuplicateArtDefCommandTag };
		}
		return EmptyEnumerable<object>.Instance;
	}

	public virtual void AddElement(CollectionFieldValueAdapter collection)
	{
		collection.DomNode.Parent.As<ArtDefElementAdapter>().DoTransaction(delegate
		{
			collection.AddValue(-1);
		}, "Add Collection Value".Localize());
	}

	public virtual void AddElements(ArtDefCollectionAdapter collection, IEnumerable<string> ListOfElementNames)
	{
		DocumentRegistry.GetActiveDocument<ArtDefDocument>().As<ArtDefContext>().As<ArtDefSetAdapter>();
		new List<IArtDefElement>();
		collection.DoTransaction(delegate
		{
			foreach (string ListOfElementName in ListOfElementNames)
			{
				collection.AddElement(ListOfElementName, -1);
			}
		}, "Add Elements".Localize());
	}

	public virtual void RemoveElements(ArtDefCollectionAdapter collection, IEnumerable<IArtDefElement> elements)
	{
		DocumentRegistry.GetActiveDocument<ArtDefDocument>();
		string[] elementNames = elements.Select((IArtDefElement elem) => elem.Name).ToArray();
		collection.DoTransaction(delegate
		{
			string[] array = elementNames;
			foreach (string name in array)
			{
				collection.RemoveElement(name);
			}
		}, "Remove Elements".Localize());
	}

	public virtual void DuplicateElements(ArtDefCollectionAdapter collection, IEnumerable<IArtDefElement> elements)
	{
		ArtDefDocument activeDocument = DocumentRegistry.GetActiveDocument<ArtDefDocument>();
		ArtDefContext artDefContext = activeDocument.As<ArtDefContext>();
		collection.DoTransaction(delegate
		{
			foreach (object item in artDefContext.CopySelection())
			{
				artDefContext.Insert(item);
			}
		}, "Duplicate Collection Values".Localize());
	}

	public virtual void RemoveElement(CollectionFieldValueAdapter collection, IValue value)
	{
		collection.DomNode.Parent.As<ArtDefElementAdapter>().DoTransaction(delegate
		{
			collection.RemoveValue(collection.CollectionValue.Items.IndexOf(value));
		}, "Remove Collection Value".Localize());
	}

	public virtual void DuplicateElement(CollectionFieldValueAdapter collection, IValue value)
	{
		ArtDefDocument activeDocument = DocumentRegistry.GetActiveDocument<ArtDefDocument>();
		ArtDefContext artDefContext = activeDocument.As<ArtDefContext>();
		collection.DomNode.Parent.As<ArtDefElementAdapter>().DoTransaction(delegate
		{
			object dataObject = artDefContext.Copy();
			artDefContext.Insert(dataObject);
		}, "Duplicate Collection Value".Localize());
	}

	protected void RaiseElementAdded(int idx, object elm)
	{
		this.ElementAdded?.Invoke(this, new ItemInsertedEventArgs<object>(idx, elm));
	}

	protected void RaiseElementRemoved(int idx, object elm)
	{
		this.ElementRemoved?.Invoke(this, new ItemRemovedEventArgs<object>(idx, elm));
	}

	protected void RaiseProjectConfigChanged()
	{
		this.ProjectConfigChanged?.Invoke(this, new EventArgs());
	}

	private void FixupBLPReferences()
	{
		DocumentRegistry.ActiveDocument.As<ArtDefContext>()?.FixupBLPReferences();
	}

	private void RegisterClientCommands()
	{
		CommandService.RegisterCommand(new CommandInfo(AddArtDefCommandTag, StandardMenu.Edit, StandardCommandGroup.EditOther, "Add Element".Localize("Name of a command"), "Adds an element to an existing collection".Localize(), Keys.None, Resources.AddClassIcon, CommandVisibility.All), this);
		CommandService.RegisterCommand(new CommandInfo(RemoveArtDefCommandTag, StandardMenu.Edit, StandardCommandGroup.EditOther, "Remove Element".Localize("Name of a command"), "Removes an existing element from a collection".Localize(), Keys.None, Resources.DeleteClassIcon, CommandVisibility.All), this);
		CommandService.RegisterCommand(new CommandInfo(DuplicateArtDefCommandTag, StandardMenu.Edit, StandardCommandGroup.EditOther, "Duplicate Element".Localize("Name of a command"), "Duplicates an existing element from a collection".Localize(), Keys.None, Resources.DuplicateClassIcon, CommandVisibility.All), this);
		CommandService.RegisterCommand(new CommandInfo(FixupBLPReferencesCommandTag, StandardMenu.Edit, StandardCommandGroup.EditOther, "Fixup BLP References".Localize("Name of a command"), "Attempts to automatically find matches for missing BLP references.".Localize(), Keys.None, Resources.FixLinksIcon, CommandVisibility.All), this);
		CommandService.RegisterCommand(new CommandInfo(ReloadProjectConfigCommandTag, StandardMenu.Edit, StandardCommandGroup.EditPreferences, "Reload Project Config".Localize("Name of a command"), "Reloads the project's configuration XML".Localize(), Keys.None, Resources.ReloadProjectConfigIcon, CommandVisibility.ApplicationMenu), this);
		CommandInfo info = new CommandInfo(SaveMergedArtDefsCommandTag, StandardMenu.Edit, StandardCommandGroup.EditOther, "Save Merged ArtDefs".Localize(), "Saves all merged ArtDefs to a human-readable text format.".Localize(), Keys.None, null, CommandVisibility.Menu);
		CommandService.RegisterCommand(info, this);
	}

	private void ReloadProjectConfig()
	{
		ArtDefDocument activeDocument = DocumentRegistry.GetActiveDocument<ArtDefDocument>();
		if (activeDocument != null)
		{
			ArtDefContext artDefContext = activeDocument.As<ArtDefContext>();
			artDefContext.DoTransaction(delegate
			{
				artDefContext.AddOperation(new ProjectConfigOperation(CivTechService, ProjectConfigService));
			}, "Reload Project Config".Localize());
		}
		else
		{
			ProjectConfigService.LoadConfig();
		}
		RaiseProjectConfigChanged();
		Outputs.WriteLine(OutputMessageType.Info, "Loaded project config \"{0}\"", CivTechService.PrimaryProject.ActiveConfigPath);
	}

	private void SaveMergedArtDefs()
	{
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		folderPath = Path.Combine(folderPath, "My Games", "AssetCloud", "Merged ArtDefs", CivTechService.PrimaryProject.Name);
		if (!Directory.Exists(folderPath))
		{
			Directory.CreateDirectory(folderPath);
		}
		IEnumerable<string> relativeArtDefPaths = m_artDefRegistry.GetRelativeArtDefPaths();
		foreach (string item in relativeArtDefPaths)
		{
			string artDefString = m_artDefRegistry.GetArtDefString(item);
			if (!string.IsNullOrEmpty(artDefString))
			{
				using StreamWriter streamWriter = File.CreateText(Path.Combine(folderPath, item.Replace(".artdef", ".txt")));
				streamWriter.Write(artDefString);
				streamWriter.Flush();
				streamWriter.Close();
			}
		}
		MessageBoxes.Show($"Saved {relativeArtDefPaths.Count()} merged ArtDef files to {folderPath}.", "Success!", MessageBoxButton.OK, MessageBoxImage.None);
	}
}
