using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using DatabaseWrapper;
using Firaxis.ATF.Properties;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Firaxis.Error;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Firaxis.ATF;

[Export(typeof(ImportServiceCommands))]
[Export(typeof(IImportService))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ImportServiceCommands : IImportService, ICommandClient, IInitializable
{
	private enum Command
	{
		Import,
		OpenSource
	}

	private struct ImportCommandTag
	{
		public Command Command;

		public ImportCommandTag(Command command)
		{
			Command = command;
		}
	}

	private string _importIconName;

	private ICivTechService m_civTechService;

	private AssetBrowserFileCommands m_fileCommands;

	[Import(AllowDefault = true)]
	private ScriptingService m_scriptingService;

	private BatchEntitySourceControlService m_batchSourceControlService;

	protected ICivTechService CivTechService { get; private set; }

	protected ICommandService CommandService { get; private set; }

	protected IDocumentRegistry DocumentRegistry { get; private set; }

	public string ImportIconName
	{
		get
		{
			return _importIconName;
		}
		set
		{
			_importIconName = value;
		}
	}

	public event EventHandler<DocumentImportedEventArgs> DocumentImported;

	public event EventHandler<DocumentImportingEventArgs> DocumentImporting;

	public event EventHandler ImportCompleted;

	[ImportingConstructor]
	public ImportServiceCommands(ICommandService commandService, IDocumentRegistry documentRegistry, ICivTechService civTechSvc, BatchEntitySourceControlService batchSourceControlService, AssetBrowserFileCommands fileCommands)
	{
		CommandService = commandService;
		DocumentRegistry = documentRegistry;
		CivTechService = civTechSvc;
		m_civTechService = civTechSvc;
		m_fileCommands = fileCommands;
		m_batchSourceControlService = batchSourceControlService;
		ResourceUtil.RegisterImage("file_refresh", Firaxis.ATF.Properties.Resources.file_refresh);
	}

	protected virtual void RaiseDocumentImported(IImportableDocument doc, bool success)
	{
		this.DocumentImported?.Invoke(this, new DocumentImportedEventArgs(doc, success));
	}

	protected virtual void RaiseDocumentImporting(IImportableDocument doc)
	{
		this.DocumentImporting?.Invoke(this, new DocumentImportingEventArgs(doc));
	}

	protected virtual void RaiseImportCompleted()
	{
		this.ImportCompleted?.Invoke(this, EventArgs.Empty);
	}

	public void Import(IImportableDocument document, bool force = false)
	{
		Import(new IImportableDocument[1] { document }, force);
	}

	private ResultCode PerformCheckout(IDocument document)
	{
		string localPath = document.Uri.LocalPath;
		string projectName = m_civTechService.GetProjectName(document.Uri);
		IVersionControlService versionControl = m_civTechService.ActiveProjectMap[projectName].VersionControl;
		bool flag = versionControl.IsVersionControlled(localPath);
		bool flag2 = versionControl.IsMarkedForDelete(localPath);
		string errMsg;
		if (!flag || flag2)
		{
			if (flag2)
			{
				Outputs.WriteLine(OutputMessageType.Info, "Readding marked for delete file {0} to version control", localPath);
			}
			else if (!flag)
			{
				Outputs.WriteLine(OutputMessageType.Info, "Adding new file {0} to version control. Workspace Root: {1}", localPath, versionControl.WorkspaceRoot);
			}
			if (!versionControl.AddFile(localPath, out errMsg))
			{
				return new ResultCode("Failed to open " + localPath + " for add in version control.\nYou will be unable to save this file!\n\n" + errMsg);
			}
		}
		else if (!versionControl.IsEditible(localPath))
		{
			if (!versionControl.GetLatest(localPath, out errMsg))
			{
				return new ResultCode("Failed to get latest " + document.Uri.LocalPath + " from version control.\nModifying this file in this state will remove someone's work!\n\nThe file will not be open for edit!\n\n" + errMsg);
			}
			if (!versionControl.EditFile(localPath, out errMsg))
			{
				return new ResultCode("Failed to open " + localPath + " for edit in version control.\nYou will be unable to save this file!\n\n" + errMsg);
			}
		}
		return ResultCode.Success;
	}

	public void Import(IEnumerable<IImportableDocument> documents, bool force = false)
	{
		IEnumerable<IImportableDocument> enumerable = documents.Where((IImportableDocument wc) => !CivTechRegistry.CivTechService.IsFromActiveProjectOrDependencies(wc.Uri));
		if (enumerable.Any())
		{
			MessageBoxes.Show("One or more documents being imported is not from the active project or one of its dependencies", "Cannot perform export", MessageBoxButton.OK, MessageBoxImage.Error);
			Outputs.WriteLine(OutputMessageType.Error, "Import failed, the following documents are not from the active project or one of its dependencies:");
			{
				foreach (IImportableDocument item2 in enumerable)
				{
					Outputs.WriteLine(OutputMessageType.Error, "\t{0}", item2.Uri);
				}
				return;
			}
		}
		IEnumerable<IImportableDocument> enumerable2 = documents.Where((IImportableDocument wc) => !File.Exists(wc.Uri.LocalPath) || File.GetAttributes(wc.Uri.LocalPath).HasFlag(FileAttributes.ReadOnly));
		if (enumerable2.Any())
		{
			IEnumerable<IImportableDocument> source = enumerable2.Where((IImportableDocument wc) => !CivTechRegistry.CivTechService.IsFromActiveProject(wc.Uri));
			if (source.Any())
			{
				IEnumerable<string> values = source.Select((IImportableDocument sc) => CivTechRegistry.CivTechService.GetProjectName(sc.Uri)).Distinct();
				if (MessageBoxes.Show(string.Format("Attempting to import documents from the following projects:\n\n{0}\n\nWould you like to continue?", string.Join("\n", values)), "Modifying non-active project files", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
				{
					return;
				}
			}
			IList<Tuple<IDocument, ResultCode>> results = new List<Tuple<IDocument, ResultCode>>();
			enumerable2.ForEach(delegate(IImportableDocument fe)
			{
				results.Add(new Tuple<IDocument, ResultCode>(fe, PerformCheckout(fe)));
			});
			IEnumerable<string> enumerable3 = from sc in results
				where !sc.Item2
				select sc.Item1.Uri.LocalPath;
			if (enumerable3.Any() && MessageBoxes.Show(string.Format("The following entities could not be prepared for edit:\n\n{0}\n\nThey will likely fail to export, continue?", string.Join("\n", enumerable3)), "Continue with Export?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
			{
				return;
			}
		}
		IList<IImportableDocument> list = new List<IImportableDocument>(documents);
		IDictionary<IImportableDocument, string> dictionary = new Dictionary<IImportableDocument, string>(list.Count);
		foreach (IImportableDocument document in documents)
		{
			IList<string> list2 = new List<string>();
			if (!document.IsReadyForExport(list2))
			{
				string value = string.Join("\n", list2);
				dictionary.Add(document, value);
			}
		}
		if (dictionary.Count > 0)
		{
			if (dictionary.Count == list.Count)
			{
				string message = BuildErrorMessage("Cannot perform export:\n\n", dictionary);
				MessageBoxes.Show(message, "Cannot perform export", MessageBoxButton.OK, MessageBoxImage.Error);
				Outputs.WriteLine(OutputMessageType.Error, message);
				return;
			}
			if (MessageBoxes.Show(BuildErrorMessage("The following documents cannot be exported for the following reasons.\nExport remaining documents?\n\n", dictionary), "Continue with Export?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
			{
				return;
			}
			list = list.Except(dictionary.Keys).ToList();
		}
		ISet<IImportedEntity> entities = new HashSet<IImportedEntity>();
		list.ForEach(delegate(IImportableDocument doc)
		{
			IImportedEntity item = doc.InstanceEntity as IImportedEntity;
			entities.Add(item);
		});
		ImportActionImpl(entities, documents);
	}

	private string BuildErrorMessage(string heading, IEnumerable<KeyValuePair<IImportableDocument, string>> failedDocuments)
	{
		StringBuilder stringBuilder = new StringBuilder(heading);
		foreach (KeyValuePair<IImportableDocument, string> failedDocument in failedDocuments)
		{
			stringBuilder.AppendFormat("Document '{0}' failed.\n\nReason: {1}", failedDocument.Key.Uri.LocalPath, failedDocument.Value);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString();
	}

	private void ImportActionImpl(IEnumerable<IImportedEntity> entitiesToImport, IEnumerable<IImportableDocument> documents)
	{
		if (!entitiesToImport.Any())
		{
			Outputs.WriteLine(OutputMessageType.Warning, "Export called but there were no entities to export.");
			RaiseImportCompleted();
			return;
		}
		foreach (IImportableDocument document in documents)
		{
			RaiseDocumentImporting(document);
		}
		IEnumerable<ImportOperationResult> allResults = global::DatabaseWrapper.DatabaseWrapper.ExportEntities(CivTechService, CivTechService.PrimaryProject.Name, entitiesToImport);
		IEnumerable<ImportOperationResult> failedResults = allResults.GetFailedResults();
		if (failedResults.Any())
		{
			string combinedFailureMessages = failedResults.GetCombinedFailureMessages();
			MessageBoxes.Show(combinedFailureMessages, "Export Failure", MessageBoxButton.OK, MessageBoxImage.Error);
			Outputs.WriteLine(OutputMessageType.Error, combinedFailureMessages);
		}
		foreach (ImportOperationResult validResult in allResults.GetValidResults())
		{
			Outputs.WriteLine(OutputMessageType.Info, "Successfully exported entity {0}.", validResult.Entity.Name);
		}
		foreach (IImportableDocument item in (IEnumerable<IImportableDocument>)new List<IImportableDocument>(documents))
		{
			bool success = !failedResults.Select((ImportOperationResult res) => res.Entity).Contains(item.InstanceEntity);
			RaiseDocumentImported(item, success);
		}
		RaiseImportCompleted();
	}

	public virtual void Initialize()
	{
		RegisterClientCommands();
		if (m_scriptingService != null)
		{
			m_scriptingService.LoadAssembly(GetType().Assembly);
			m_scriptingService.SetVariable("entityImporter", this);
		}
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		if (DocumentRegistry == null || DocumentRegistry.ActiveDocument == null)
		{
			return false;
		}
		if (!(DocumentRegistry.ActiveDocument is IImportableDocument))
		{
			return false;
		}
		if (commandTag == null)
		{
			return false;
		}
		if (!(commandTag is ImportCommandTag importCommandTag))
		{
			return false;
		}
		if (importCommandTag.Command != Command.OpenSource)
		{
			if (importCommandTag.Command == Command.Import)
			{
				return !DocumentRegistry.ActiveDocument.IsReadOnly;
			}
			return false;
		}
		return true;
	}

	public virtual void DoCommand(object commandTag)
	{
		if (commandTag is ImportCommandTag importCommandTag)
		{
			IImportableDocument document = DocumentRegistry.ActiveDocument as IImportableDocument;
			if (importCommandTag.Command == Command.Import)
			{
				Import(document);
			}
			if (importCommandTag.Command == Command.OpenSource)
			{
				PreviewSourceFile(document);
			}
		}
	}

	private void PreviewSourceFile(IImportableDocument document)
	{
		IInstanceEntity instanceEntity = document.InstanceEntity;
		using IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { CivTechService.GetActivePantryPaths() });
		IInstanceEntity instanceEntity2 = instanceSet.LoadEntityByName(instanceEntity.Name, instanceEntity.Type);
		if (instanceEntity2 != null)
		{
			if (!StaticMethods.IsImportableType(instanceEntity.Type))
			{
				ICollection<IImportedEntity> collection = global::DatabaseWrapper.DatabaseWrapper.GetImportableEntities(CivTechService, instanceEntity2, instanceSet, recursive: true).ToList();
				int count = collection.Count;
				if (count > 1 && MessageBoxes.Show($"This command is going to attempt to open {count} different source files, are you sure you want to proceed?", "Open multiple files?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
				{
					return;
				}
				{
					foreach (IImportedEntity item in collection)
					{
						CivTechHelper.OpenSourceFile(CivTechService, item);
					}
					return;
				}
			}
			if (instanceEntity2 is IImportedEntity entity)
			{
				CivTechHelper.OpenSourceFile(CivTechService, entity);
			}
		}
		else
		{
			Outputs.WriteLine(OutputMessageType.Error, "Could not load the instance entity with the name {0} and the type {1} for the purpose of getting its children to preview sources with.", document.InstanceEntity.Name, EnumToStringConverter.GetNameFromType(document.InstanceEntity.Type));
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private void RegisterClientCommands()
	{
		Keys shortcut = Keys.I | Keys.Shift | Keys.Control;
		CommandService.RegisterCommand(new CommandInfo(new ImportCommandTag(Command.Import), StandardMenu.File, StandardCommandGroup.FileNew, "Import".Localize("Name of a command"), "Imports an existing document".Localize(), shortcut, Resources.ReimportFileIcon, CommandVisibility.All), this);
		CommandService.RegisterCommand(new CommandInfo(new ImportCommandTag(Command.OpenSource), StandardMenu.File, StandardCommandGroup.FileNew, "Open Source file".Localize("Name of a command"), "Will open the source file for the document in its default editing program".Localize(), Keys.None, Resources.OpenSourceIcon, CommandVisibility.All), this);
	}
}
