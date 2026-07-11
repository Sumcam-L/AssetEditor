using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public static class XLPAdapterExtensions
{
	public static void AddEntryFromCloud(this XLPAdapter adapter)
	{
		XLPContext mainContext = adapter.DomNode.As<XLPContext>();
		string[] files = null;
		IEnumerable<InstanceType> filter = new InstanceType[1] { adapter.XLPClass.InstanceType };
		IEnumerable<string> allowedEntityClasses = adapter.XLPClass.AllowedEntityClasses;
		if (DialogResult.OK != mainContext.AssetBrowserService.OpenFileNames(ref files, filter, allowedEntityClasses))
		{
			return;
		}
		Action action = delegate
		{
			_ = mainContext.CivTechService.PrimaryProject.Paths.GamePantry;
			string[] array = files;
			foreach (string entityPath in array)
			{
				string entityNameFromFilePath = StaticMethods.GetEntityNameFromFilePath(mainContext.CivTechService.ProjectMapService, entityPath, adapter.XLPClass.InstanceType);
				string entryName = GenerateUniqueEntryName(entityNameFromFilePath, adapter.XLP);
				adapter.AddEntry(entryName, entityNameFromFilePath);
			}
		};
		adapter.AddUndoOperation("Add Existing Entries", action);
	}

	public static void AddNewEntriesFromCloud(this XLPAdapter adapter)
	{
		XLPContext context = adapter.DomNode.As<XLPContext>();
		_ = context.CivTechService.PrimaryProject.Paths.GamePantry;
		using IInstanceSet entitySet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { context.CivTechService.GetActivePantryPaths() });
		IEnumerable<string> allowedEntityClasses = adapter.XLPClass.AllowedEntityClasses;
		InstanceType instanceType = adapter.XLPClass.InstanceType;
		IEnumerable<EntityID> entities = SetAdapterHelper.LaunchMiniImporter(context.CivTechService, context.FileWatchService, entitySet, context.EntityCacheService, allowedEntityClasses, instanceType);
		if (!entities.Any())
		{
			return;
		}
		adapter.AddUndoOperation("Add New Entries", delegate
		{
			foreach (EntityID item in entities)
			{
				adapter.AddUniqueXLPEntry(item.Name);
			}
		});
	}

	public static void AddUndoOperation(this XLPAdapter adapter, string operationName, Action action)
	{
		adapter.DomNode.GetRoot().As<XLPContext>().DoTransaction(action, operationName);
	}

	public static void AddUniqueXLPEntry(this XLPAdapter adapter, string entityName)
	{
		string entryName = GenerateUniqueEntryName(entityName, adapter.XLP);
		adapter.AddEntry(entryName, entityName);
	}

	public static bool CanAddEntryFromCloud(this XLPAdapter adapter)
	{
		if (adapter.XLP == null)
		{
			return false;
		}
		if (!adapter.DomNode.As<XLPDocument>().IsReadOnly && adapter.XLPClass != null)
		{
			return !string.IsNullOrEmpty(adapter.XLPClass.Name);
		}
		return false;
	}

	public static bool CanAddNewEntriesFromCloud(this XLPAdapter adapter)
	{
		XLPDocument xLPDocument = adapter.DomNode.As<XLPDocument>();
		if (xLPDocument.XLP != null && !xLPDocument.IsReadOnly && adapter.XLPClass != null && adapter.XLPClass.InstanceType != InstanceType.IT_ASSET)
		{
			return !string.IsNullOrEmpty(adapter.XLPClass.Name);
		}
		return false;
	}

	public static bool CanOpenSelectedEntries(this XLPAdapter adapter)
	{
		if (adapter.XLPClass != null)
		{
			return adapter.Selection.Any();
		}
		return false;
	}

	public static bool CanReimportSelectedEntries(this XLPAdapter adapter)
	{
		XLPDocument xLPDocument = adapter.DomNode.As<XLPDocument>();
		if (xLPDocument.XLP != null && !xLPDocument.IsReadOnly && adapter.XLPClass != null && StaticMethods.IsImportableType(adapter.XLPClass.InstanceType))
		{
			return adapter.Selection.Any();
		}
		return false;
	}

	public static bool CanRemoveSelectedEntries(this XLPAdapter adapter)
	{
		XLPDocument xLPDocument = adapter.DomNode.As<XLPDocument>();
		if (xLPDocument.XLP != null && !xLPDocument.IsReadOnly)
		{
			return adapter.Selection.Any();
		}
		return false;
	}

	public static string GenerateUniqueEntryName(string entityName, IXLP xlp)
	{
		int num = 0;
		string text = entityName;
		while (xlp.FindEntry(text) != null)
		{
			text = $"{entityName}{++num}";
		}
		return text;
	}

	public static void OpenSelectedEntries(this XLPAdapter adapter)
	{
		XLPContext xLPContext = adapter.DomNode.As<XLPContext>();
		object[] array = adapter.Selection.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			XLPEntryAdapter xLPEntryAdapter = array[i].As<XLPEntryAdapter>();
			xLPContext.AssetBrowserCommands.OpenExistingDocument(adapter.XLPClass.InstanceType, xLPEntryAdapter.ObjectName);
		}
	}

	public static void ReimportSelectedEntries(this XLPAdapter adapter)
	{
		XLPContext context = adapter.DomNode.As<XLPContext>();
		_ = context.CivTechService.PrimaryProject.Paths.GamePantry;
		IEnumerable<EntityID> selectedEntityIDs = adapter.GetSelectedEntityIDs();
		OutputEntityExportWarnings(selectedEntityIDs, context.CivTechService);
		foreach (EntityID item in selectedEntityIDs.Where((EntityID ent) => EntityExistsOnDisk(context.CivTechService, ent)))
		{
			bool alreadyOpen = context.AssetBrowserCommands.IsDocumentOpen(item.Type, item.Name);
			IImportableDocument importableDocument = context.AssetBrowserCommands.OpenExistingDocument(item.Type, item.Name) as IImportableDocument;
			IImportService importService = context.ImportService;
			AssetBrowserFileCommands assetBrowserCommands = context.AssetBrowserCommands;
			EventHandler<DocumentImportedEventArgs> value = BuildImportHandler(importableDocument.Uri, alreadyOpen, importService, assetBrowserCommands);
			importService.DocumentImported += value;
			importService.Import(importableDocument);
		}
	}

	public static void RemoveSelectedEntries(this XLPAdapter adapter)
	{
		AddUndoOperation(action: delegate
		{
			List<string> list = new List<string>();
			object[] array = adapter.Selection.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				XLPEntryAdapter xLPEntryAdapter = array[i].As<XLPEntryAdapter>();
				list.Add(xLPEntryAdapter.EntryID);
			}
			foreach (string item in list)
			{
				adapter.RemoveEntry(item);
			}
		}, adapter: adapter, operationName: "Remove Entries");
	}

	private static EventHandler<DocumentImportedEventArgs> BuildImportHandler(Uri documentUri, bool alreadyOpen, IImportService importService, AssetBrowserFileCommands commands)
	{
		EventHandler<DocumentImportedEventArgs> handler = null;
		handler = delegate(object s, DocumentImportedEventArgs docArg)
		{
			if (docArg.Document != null && docArg.Successful && docArg.Document.Uri == documentUri)
			{
				commands.Save(docArg.Document);
				if (!alreadyOpen)
				{
					commands.Close(docArg.Document);
				}
				importService.DocumentImported -= handler;
			}
		};
		return handler;
	}

	private static bool EntityExistsOnDisk(ICivTechService civTechSvc, EntityID entity)
	{
		return File.Exists(civTechSvc.GetEntityPath(entity.Name, entity.Type));
	}

	private static IEnumerable<EntityID> GetSelectedEntityIDs(this XLPAdapter adapter)
	{
		ISet<EntityID> set = new SortedSet<EntityID>();
		InstanceType instanceType = adapter.XLPClass.InstanceType;
		object[] array = adapter.Selection.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			EntityID item = new EntityID(array[i].As<XLPEntryAdapter>().ObjectName, instanceType);
			set.Add(item);
		}
		return set;
	}

	private static void OutputEntityExportWarnings(IEnumerable<EntityID> entitiesToImport, ICivTechService civTechSvc)
	{
		int num = 0;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (EntityID item in entitiesToImport.Where((EntityID ent) => !EntityExistsOnDisk(civTechSvc, ent)))
		{
			string text = $"{item};\t";
			stringBuilder.Append(text);
			num += text.Length;
			if (num > 120)
			{
				stringBuilder.AppendLine();
				num = 0;
			}
		}
		if (stringBuilder.Length > 0)
		{
			string text2 = "Unable to reimport the following entities because they do not exist on disk.\r\n" + stringBuilder.ToString();
			Outputs.WriteLine(OutputMessageType.Error, text2);
			MessageBox.Show(text2);
		}
	}
}
