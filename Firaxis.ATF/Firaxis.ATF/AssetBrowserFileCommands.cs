using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DatabaseWrapper;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Microsoft.Win32;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Firaxis.ATF;

[Export(typeof(IDocumentService))]
[Export(typeof(AssetBrowserFileCommands))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AssetBrowserFileCommands : IDocumentService, ICommandClient, IInitializable, IDisposable
{
	private enum Command
	{
		FileNew,
		FileOpenEntity,
		FileOpenXLP,
		FileOpenArtDef,
		FileOpenArtSpecification,
		FileOpenXLPFromAssets,
		FileOpenArtDefFromAssets,
		CopyFileToProject,
		MoveFileToProject,
		DeleteFile
	}

	[Flags]
	public enum CommandRegister
	{
		None = 0,
		FileNew = 1,
		FileOpen = 2,
		FileSave = 4,
		FileClone = 8,
		FileSaveAll = 0x10,
		FileClose = 0x20,
		Default = 0x3F
	}

	private struct FileCommandTag
	{
		public Command Command;

		public readonly IDocumentClient Editor;

		public FileCommandTag(Command command, IDocumentClient client)
		{
			Command = command;
			Editor = client;
		}
	}

	private struct FileCommandMultiTag
	{
		public Command Command;

		public readonly IEnumerable<IDocumentClient> Editors;

		public FileCommandMultiTag(Command command, IEnumerable<IDocumentClient> clients)
		{
			Command = command;
			Editors = clients;
		}
	}

	private struct InterProjectCommandFileTag
	{
		public Command Command;

		public InterProjectCommandFileTag(Command command)
		{
			Command = command;
		}
	}

	private static string lastTargetProject = string.Empty;

	[ImportMany]
	private Lazy<IDocumentClient>[] m_documentClients;

	[Import(AllowDefault = true)]
	private IStatusService m_statusService;

	private readonly Dictionary<string, int> m_extensionSuffixes = new Dictionary<string, int>();

	private readonly HashSet<string> m_newDocumentPaths = new HashSet<string>();

	private CommandRegister m_registerCommands = CommandRegister.Default;

	private int m_shadowDocumentCount;

	private readonly Dictionary<string, IDocumentClient> m_typeToClientMap = new Dictionary<string, IDocumentClient>();

	private readonly IDictionary<string, string> m_unsaveableDocuments = new Dictionary<string, string>();

	private readonly HashSet<IDocument> m_untitledDocuments = new HashSet<IDocument>();

	private bool disposedValue;

	public FileSaveFailureBehavior FileSaveFailureBehavior { get; set; }

	public bool IsSaving { get; private set; }

	public CommandRegister RegisterCommands
	{
		get
		{
			return m_registerCommands;
		}
		set
		{
			m_registerCommands = value;
		}
	}

	public IEnumerable<IDocument> UntitledDocuments => m_untitledDocuments;

	protected ICivTechService CivTechService { get; private set; }

	protected IAssetBrowserDialogService AssetBrowserService { get; private set; }

	protected ICommandService CommandService { get; private set; }

	protected IDocumentRegistryProvider DocumentRegistryMediator { get; private set; }

	protected IFileDialogService FileDialogService { get; private set; }

	[Import(AllowDefault = true)]
	private IFileWatcherService FileWatcherService { get; set; }

	public event EventHandler<DocumentEventArgs> DocumentOpened;

	public event EventHandler<DocumentEventArgs> DocumentSaving;

	public event EventHandler<DocumentEventArgs> DocumentSaved;

	public event EventHandler<DocumentClosingEventArgs> DocumentClosing;

	public event EventHandler<DocumentEventArgs> DocumentClosed;

	[ImportingConstructor]
	public AssetBrowserFileCommands(ICivTechService civTechService, ICommandService commandService, IDocumentRegistryMediator documentRegistryMediator, IAssetBrowserDialogService assetBrowserService, IFileDialogService fileDialogService, IFileWatcherService fileWatcher)
	{
		CivTechService = civTechService;
		CommandService = commandService;
		DocumentRegistryMediator = documentRegistryMediator;
		FileDialogService = fileDialogService;
		AssetBrowserService = assetBrowserService;
		FileWatcherService = fileWatcher;
		FileSaveFailureBehavior = FileSaveFailureBehavior.Log;
	}

	void IInitializable.Initialize()
	{
		m_typeToClientMap.Clear();
		Lazy<IDocumentClient>[] documentClients = m_documentClients;
		for (int i = 0; i < documentClients.Length; i++)
		{
			IDocumentClient value = documentClients[i].Value;
			m_typeToClientMap.Add(value.Info.FileType, value);
		}
		if ((RegisterCommands & CommandRegister.FileSave) == CommandRegister.FileSave)
		{
			CommandService.RegisterCommand(CommandInfo.FileSave, this);
		}
		if ((RegisterCommands & CommandRegister.FileClone) == CommandRegister.FileClone)
		{
			CommandService.RegisterCommand(CommandInfo.FileSaveAs, this);
		}
		if ((RegisterCommands & CommandRegister.FileSaveAll) == CommandRegister.FileSaveAll)
		{
			CommandService.RegisterCommand(CommandInfo.FileSaveAll, this);
		}
		if ((RegisterCommands & CommandRegister.FileClose) == CommandRegister.FileClose)
		{
			CommandService.RegisterCommand(CommandInfo.FileClose, this);
		}
		RegisterClientCommands();
		Initialize();
	}

	private void AddToUntitledDocuments(IDocument document)
	{
		if (document != null && IsUntitledDocument(document))
		{
			m_untitledDocuments.Add(document);
		}
	}

	private bool IsUntitledDocument(IDocument document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		string localPath = document.Uri.LocalPath;
		if (!m_newDocumentPaths.Contains(localPath))
		{
			return !FileDialogService.PathExists(localPath);
		}
		return false;
	}

	public virtual IDocument OpenNewDocument(IDocumentClient client)
	{
		if (client == null)
		{
			throw new ArgumentNullException("client");
		}
		IDocument document = null;
		Uri newDocumentUri = GetNewDocumentUri(client);
		if (newDocumentUri != null)
		{
			document = DoOpen(client, newDocumentUri);
			AddToUntitledDocuments(document);
		}
		return document;
	}

	public virtual IEntityDocument OpenNewDocument(InstanceType entityType)
	{
		Lazy<IDocumentClient>[] documentClients = m_documentClients;
		foreach (Lazy<IDocumentClient> lazy in documentClients)
		{
			if (lazy.Value.Info is EntityDocumentClientInfo entityDocumentClientInfo && entityDocumentClientInfo.ExtensionTypes.Contains(entityType))
			{
				return OpenNewDocument(lazy.Value) as IEntityDocument;
			}
		}
		return null;
	}

	public virtual IDocument OpenNewShadowDocument(InstanceType entityType)
	{
		IDocumentClient documentClient = (from docClient in m_documentClients
			where docClient.Value.Info.FileType == "ShadowDocument"
			select docClient.Value).FirstOrDefault();
		if (documentClient == null)
		{
			return null;
		}
		return OpenNewShadowDocument(entityType, documentClient);
	}

	private Uri GetNewShadowDocumentUri(InstanceType entityType, IDocumentClient shadowDocumentClient)
	{
		Uri result = null;
		_ = shadowDocumentClient.Info.NewDocumentName;
		if (shadowDocumentClient.Info.FileType == "ShadowDocument")
		{
			EntityDocumentClientInfo entityDocumentClientInfo = shadowDocumentClient.Info as EntityDocumentClientInfo;
			string path = StaticMethods.PantryRootForInstanceType(CivTechService.PrimaryProject.Paths.GamePantry, entityType);
			int num = entityDocumentClientInfo.ExtensionTypes.IndexOf(entityType);
			result = new Uri(Path.Combine(path, $"NewShadowDocument_{m_shadowDocumentCount++}{entityDocumentClientInfo.Extensions[num]}").Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		}
		return result;
	}

	private IDocument OpenNewShadowDocument(InstanceType entityType, IDocumentClient client)
	{
		if (client == null)
		{
			throw new ArgumentNullException("client");
		}
		IDocument document = null;
		Uri newShadowDocumentUri = GetNewShadowDocumentUri(entityType, client);
		if (newShadowDocumentUri != null)
		{
			document = DoOpen(client, newShadowDocumentUri);
			AddToUntitledDocuments(document);
		}
		return document;
	}

	public virtual IDocument OpenExistingDocument(InstanceType entityType, string entityName)
	{
		Lazy<IDocumentClient>[] documentClients = m_documentClients;
		foreach (Lazy<IDocumentClient> lazy in documentClients)
		{
			if (lazy.Value.Info is EntityDocumentClientInfo entityDocumentClientInfo && entityDocumentClientInfo.ExtensionTypes.Contains(entityType))
			{
				string dataFile = global::DatabaseWrapper.DatabaseWrapper.GetDataFile(entityType, entityName);
				return OpenExistingDocument(lazy.Value, new Uri(dataFile));
			}
		}
		return null;
	}

	public virtual IDocument OpenExistingShadowDocument(InstanceType entityType, string entityName)
	{
		IDocumentClient documentClient = (from docClient in m_documentClients
			where docClient.Value.Info.FileType == "ShadowDocument"
			select docClient.Value).FirstOrDefault();
		if (documentClient == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(entityName))
		{
			return null;
		}
		string dataFile = global::DatabaseWrapper.DatabaseWrapper.GetDataFile(entityType, entityName);
		return OpenExistingDocument(documentClient, new Uri(dataFile));
	}

	public virtual IDocument OpenExistingDocument(IDocumentClient client, Uri uri)
	{
		return OpenExistingDocument(new IDocumentClient[1] { client }, uri);
	}

	private IEnumerable<InstanceType> GetSupportedInstanceTypes(IEnumerable<IDocumentClient> clients)
	{
		ISet<InstanceType> supportedInstanceTypes = new HashSet<InstanceType>();
		foreach (IDocumentClient client in clients)
		{
			if (client.Info is EntityDocumentClientInfo entityDocumentClientInfo)
			{
				entityDocumentClientInfo.ExtensionTypes.ForEach(delegate(InstanceType ext)
				{
					supportedInstanceTypes.Add(ext);
				});
			}
		}
		return supportedInstanceTypes;
	}

	private string BuildFilterString(IEnumerable<IDocumentClient> clients, int maxClts)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (maxClts > 1)
		{
			for (int i = 0; i < maxClts; i++)
			{
				string[] extensions = clients.ElementAt(i).Info.Extensions;
				stringBuilder.Append(FormatExtensionString(extensions));
				if (i < maxClts - 1)
				{
					stringBuilder.Append(";");
				}
			}
		}
		else if (maxClts == 1)
		{
			IDocumentClient documentClient = clients.First();
			stringBuilder.Append(FormatExtensionString(documentClient.Info.Extensions));
		}
		else
		{
			stringBuilder.Append("*.*");
		}
		return stringBuilder.ToString();
	}

	private void GetPathNames(IEnumerable<IDocumentClient> clients, ref string[] pathNames)
	{
		int num = clients.Count();
		string arg = BuildFilterString(clients, num);
		string empty = string.Empty;
		string forcedInitialDirectory = CivTechService.PrimaryProject.Paths.GamePantry;
		if (num > 1)
		{
			empty = "Various";
		}
		else if (num == 1)
		{
			IDocumentClient documentClient = clients.First();
			empty = documentClient.Info.FileType;
			forcedInitialDirectory = documentClient.Info.InitialDirectory;
		}
		else
		{
			empty = "All";
		}
		FileDialogService.ForcedInitialDirectory = forcedInitialDirectory;
		FileDialogService.OpenFileNames(ref pathNames, string.Format("{0} Files ({1})|{1}", empty, arg));
	}

	private IEnumerable<string> GetPathNames(IEnumerable<IDocumentClient> clients, Uri uri)
	{
		IEnumerable<string> result = Enumerable.Empty<string>();
		if (uri != null)
		{
			result = new string[1] { uri.ToString() };
		}
		else
		{
			string[] pathNames = null;
			IEnumerable<InstanceType> supportedInstanceTypes = GetSupportedInstanceTypes(clients);
			if (supportedInstanceTypes.Any())
			{
				AssetBrowserService.OpenFileNames(ref pathNames, supportedInstanceTypes);
			}
			else
			{
				GetPathNames(clients, ref pathNames);
			}
			if (pathNames != null)
			{
				result = pathNames;
			}
		}
		return result;
	}

	private IDocument FindOrOpenExistingDocument(IDocumentClient chosenClient, Uri docUri)
	{
		IDocument document = null;
		IDocument document2 = null;
		if (chosenClient.Info.FileType != "ShadowDocument")
		{
			document2 = FindOpenDocument(docUri);
		}
		if (document2 == null)
		{
			document = DoOpen(chosenClient, docUri);
		}
		else
		{
			document = document2;
			if (!(document is IInvisibleDocument))
			{
				chosenClient.Show(document2);
			}
		}
		return document;
	}

	public virtual IDocument OpenExistingDocument(IEnumerable<IDocumentClient> clients, Uri uri)
	{
		if (clients == null)
		{
			throw new ArgumentNullException("clients");
		}
		IEnumerable<string> pathNames = GetPathNames(clients, uri);
		IDocument result = null;
		if (pathNames.Any())
		{
			foreach (string item in pathNames)
			{
				IDocumentClient firstClientForPath = clients.GetFirstClientForPath(item);
				if (firstClientForPath != null)
				{
					Uri docUri = new Uri(item, UriKind.RelativeOrAbsolute);
					result = FindOrOpenExistingDocument(firstClientForPath, docUri);
				}
			}
		}
		return result;
	}

	private string GetAssetsInitialDirectory(string fallbackDirectory)
	{
		try
		{
			using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Firaxis\Civilization6_ModBuddy\2013\DialogPage\Firaxis.VisualStudio.Projects.Civ6.OptionsPages.OptionsDialogPage"))
			{
				if (registryKey != null && registryKey.GetValue("AssetsPath") is string assetsPath)
				{
					if (!string.IsNullOrWhiteSpace(assetsPath) && Directory.Exists(assetsPath))
					{
						return assetsPath;
					}
				}
			}
		}
		catch (System.Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Warning, "Failed to read AssetsPath from registry: {0}".Localize(), ex.Message);
		}
		return fallbackDirectory;
	}

	private void OpenExistingDocumentFromAssets(IDocumentClient client, string fallbackDirectory)
	{
		if (client == null)
		{
			throw new ArgumentNullException("client");
		}
		string initialDirectory = GetAssetsInitialDirectory(fallbackDirectory);
		string filter = string.Format("{0} ({1})|{1}", client.Info.FileType, FormatExtensionString(client.Info.Extensions));
		string previousForcedInitialDirectory = FileDialogService.ForcedInitialDirectory;
		try
		{
			FileDialogService.ForcedInitialDirectory = initialDirectory;
			string[] pathNames = null;
			if (FileDialogService.OpenFileNames(ref pathNames, filter) == FileDialogResult.OK && pathNames != null)
			{
				foreach (string pathName in pathNames)
				{
					IDocumentClient firstClientForPath = m_documentClients.Select((Lazy<IDocumentClient> lazy) => lazy.Value).GetFirstClientForPath(pathName);
					if (firstClientForPath != null)
					{
						Uri docUri = new Uri(pathName, UriKind.RelativeOrAbsolute);
						FindOrOpenExistingDocument(firstClientForPath, docUri);
					}
				}
			}
		}
		finally
		{
			FileDialogService.ForcedInitialDirectory = previousForcedInitialDirectory;
		}
	}

	public virtual bool Save(IDocument document)
	{
		if (IsUntitled(document))
		{
			return SaveAsImpl(document, forcePrompt: false);
		}
		return DoSave(document, document.Uri, DocumentEventType.Saved);
	}

	public virtual bool SaveAs(IDocument document)
	{
		return SaveAsImpl(document, forcePrompt: true);
	}

	private bool SaveAsImpl(IDocument document, bool forcePrompt)
	{
		IDocumentClient client = GetClient(document);
		string localPath = document.Uri.LocalPath;
		string newFilePath = GetNewFilePath(client, localPath, document, forcePrompt);
		if (newFilePath != null)
		{
			FileDialogService.InitialDirectory = Path.GetDirectoryName(newFilePath);
			Uri uri = new Uri(newFilePath);
			bool num = DoSave(document, uri, DocumentEventType.SavedAs);
			if (num)
			{
				document.Uri = uri;
				DocumentRegistryMediator.DocumentRegistry.ActiveDocument = document;
				m_untitledDocuments.Remove(document);
				m_newDocumentPaths.Remove(localPath);
			}
			return num;
		}
		return false;
	}

	public virtual bool SaveAs(IDocument document, Uri uri)
	{
		string localPath = document.Uri.LocalPath;
		FileDialogService.InitialDirectory = Path.GetDirectoryName(uri.LocalPath);
		bool num = DoSave(document, uri, DocumentEventType.SavedAs);
		if (num)
		{
			DocumentRegistryMediator.DocumentRegistry.Remove(document);
			document.Uri = uri;
			DocumentRegistryMediator.DocumentRegistry.ActiveDocument = document;
			m_untitledDocuments.Remove(document);
			m_newDocumentPaths.Remove(localPath);
		}
		return num;
	}

	public virtual bool SaveAll(bool cancelOnFail)
	{
		bool result = true;
		foreach (IDocument item in DocumentRegistryMediator.DocumentRegistry.Documents.Where((IDocument doc) => doc.Dirty))
		{
			if (!Save(item))
			{
				result = false;
				if (cancelOnFail)
				{
					break;
				}
			}
		}
		return result;
	}

	public virtual bool Close(IDocument document)
	{
		if (document == null)
		{
			return true;
		}
		DocumentClosingEventArgs e = new DocumentClosingEventArgs(document);
		this.DocumentClosing.Raise(this, e);
		IDocumentClient client = GetClient(document);
		int num;
		if (!e.Cancel)
		{
			if (client.AskWhenClosingDirtyDocument)
			{
				num = (ConfirmClose(document) ? 1 : 0);
				if (num == 0)
				{
					goto IL_009e;
				}
			}
			else
			{
				num = 1;
			}
			OnDocumentClosing(document);
			client.Close(document);
			DocumentRegistryMediator.DocumentRegistry.Remove(document);
			m_untitledDocuments.Remove(document);
			m_newDocumentPaths.Remove(document.Uri.LocalPath);
			OnDocumentClosed(document);
			this.DocumentClosed.Raise(this, new DocumentEventArgs(document, DocumentEventType.Closed));
		}
		else
		{
			num = 0;
		}
		goto IL_009e;
		IL_009e:
		return (byte)num != 0;
	}

	public virtual bool CloseAll(IDocument masterDocument)
	{
		foreach (IDocument item in new List<IDocument>(DocumentRegistryMediator.DocumentRegistry.Documents))
		{
			if (item != masterDocument && !Close(item))
			{
				return false;
			}
		}
		if (masterDocument != null)
		{
			return Close(masterDocument);
		}
		return true;
	}

	public virtual bool IsUntitled(IDocument document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (!m_untitledDocuments.Contains(document))
		{
			return IsUntitledDocument(document);
		}
		return true;
	}

	public void AddLockedDocumentPaths(IEnumerable<string> paths, string reason)
	{
		foreach (string path in paths)
		{
			if (!m_unsaveableDocuments.ContainsKey(path))
			{
				m_unsaveableDocuments.Add(path, reason);
			}
		}
	}

	public void RemoveLockedDocumentPaths(IEnumerable<string> paths)
	{
		foreach (string path in paths)
		{
			m_unsaveableDocuments.Remove(path);
		}
	}

	public void ClearLockedDocumentPaths()
	{
		m_unsaveableDocuments.Clear();
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		bool result = false;
		if (commandTag is StandardCommand standardCommand)
		{
			if ((uint)standardCommand <= 3u)
			{
				result = DocumentRegistryMediator.DocumentRegistry.ActiveDocument != null;
			}
		}
		else if (commandTag is FileCommandTag)
		{
			result = true;
		}
		else if (commandTag is FileCommandMultiTag)
		{
			result = true;
		}
		else if (commandTag is InterProjectCommandFileTag)
		{
			result = DocumentRegistryMediator.DocumentRegistry.ActiveDocument != null;
		}
		return result;
	}

	public void DiscardAll()
	{
		foreach (IDocument item in new List<IDocument>(DocumentRegistryMediator.DocumentRegistry.Documents))
		{
			item.Dirty = false;
			if (!Close(item))
			{
				throw new System.Exception("This code should never be reached");
			}
		}
	}

	public virtual void DoCommand(object commandTag)
	{
		if (commandTag is StandardCommand)
		{
			IDocument activeDocument = DocumentRegistryMediator.DocumentRegistry.ActiveDocument;
			switch ((StandardCommand)commandTag)
			{
			case StandardCommand.FileClose:
				Close(activeDocument);
				break;
			case StandardCommand.FileSave:
				if (Save(activeDocument))
				{
					ShowStatus("Document Saved".Localize());
				}
				break;
			case StandardCommand.FileSaveAs:
				if (SaveAs(activeDocument))
				{
					ShowStatus(string.Format("Document Saved As {0}".Localize(), activeDocument.Uri.LocalPath));
				}
				break;
			case StandardCommand.FileSaveAll:
			{
				bool flag = true;
				foreach (IDocument item in (IEnumerable<IDocument>)new List<IDocument>(DocumentRegistryMediator.DocumentRegistry.Documents.Where((IDocument doc) => doc.Dirty)))
				{
					if (!Save(item))
					{
						flag = false;
					}
				}
				ShowStatus(flag ? "All documents saved".Localize() : "Couldn't save all documents".Localize());
				break;
			}
			}
		}
	else if (commandTag is FileCommandTag { Editor: var editor } fileCommandTag)
	{
		if (fileCommandTag.Command == Command.FileNew)
		{
			OpenNewDocument(editor);
		}
		else if (fileCommandTag.Command == Command.FileOpenXLPFromAssets)
		{
			OpenExistingDocumentFromAssets(editor, CivTechService.PrimaryProject.Paths.XLPRoot);
		}
		else if (fileCommandTag.Command == Command.FileOpenArtDefFromAssets)
		{
			OpenExistingDocumentFromAssets(editor, CivTechService.PrimaryProject.Paths.ArtDefRoot);
		}
		else
		{
			OpenExistingDocument(editor, null);
		}
	}
		else if (commandTag is FileCommandMultiTag { Editors: var editors } fileCommandMultiTag)
		{
			if (fileCommandMultiTag.Command == Command.FileNew)
			{
				IDocumentClient documentClient = PromptUserForNewDocumentClient(editors);
				if (documentClient != null)
				{
					OpenNewDocument(documentClient);
				}
			}
			else if (fileCommandMultiTag.Command == Command.FileOpenEntity)
			{
				OpenExistingDocument(editors, null);
			}
		}
		else if (commandTag is InterProjectCommandFileTag interProjectCommandFileTag)
		{
			if (interProjectCommandFileTag.Command == Command.CopyFileToProject)
			{
				IDocument activeDocument2 = DocumentRegistryMediator.DocumentRegistry.ActiveDocument;
				DoCopy(activeDocument2);
			}
			else if (interProjectCommandFileTag.Command == Command.MoveFileToProject)
			{
				IDocument activeDocument3 = DocumentRegistryMediator.DocumentRegistry.ActiveDocument;
				DoMove(activeDocument3);
			}
		}
	}

	public IDocumentClient GetClient(IDocument document)
	{
		if (document == null)
		{
			throw new ArgumentNullException("document");
		}
		if (!m_typeToClientMap.TryGetValue(document.Type, out var value))
		{
			throw new InvalidOperationException("Document type without corresponding document client");
		}
		return value;
	}

	public bool IsDocumentOpen(IDocument doc)
	{
		return DocumentRegistryMediator.DocumentRegistry.Documents.Contains(doc);
	}

	public bool IsDocumentOpen(InstanceType entityType, string entityName)
	{
		if (string.IsNullOrEmpty(entityName))
		{
			return false;
		}
		Uri entityDocumentURI = GetEntityDocumentURI(entityType, entityName);
		if (entityDocumentURI == null)
		{
			return false;
		}
		foreach (IDocument item in new List<IDocument>(DocumentRegistryMediator.DocumentRegistry.Documents))
		{
			if (item.Uri == entityDocumentURI)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	protected virtual bool ConfirmClose(IDocument document)
	{
		if (document == null)
		{
			return true;
		}
		bool result = true;
		if (document.Dirty)
		{
			string message = string.Format("Save {0}?".Localize(), document.Uri.LocalPath);
			switch (FileDialogService.ConfirmFileClose(message))
			{
			case FileDialogResult.Yes:
				result = Save(document);
				break;
			case FileDialogResult.No:
				document.Dirty = false;
				break;
			case FileDialogResult.Cancel:
				result = false;
				break;
			}
		}
		return result;
	}

	protected bool DoCopy(IDocument document)
	{
		string projectName = ShowProjectPickingDialog("Copy To Project", "Pick the Project to make the copy to:");
		return CopyDocument(document, projectName);
	}

	protected bool CopyDocument(IDocument document, string ProjectName)
	{
		if (string.IsNullOrEmpty(ProjectName))
		{
			return false;
		}
		if (document is IInstanceEntityDocument instanceEntityDocument)
		{
			string errMsg = string.Empty;
			if (!global::DatabaseWrapper.DatabaseWrapper.OpenIfRequired(instanceEntityDocument.InstanceEntity, out errMsg))
			{
				MessageBoxes.Show("Entity " + instanceEntityDocument.InstanceEntity.Name + " could not be opened for edit.\n" + errMsg);
			}
			else
			{
				string directoryName = Path.GetDirectoryName(document.Uri.LocalPath);
				foreach (IInstanceDataFile dataFile in instanceEntityDocument.InstanceEntity.DataFiles)
				{
					string text = Path.Combine(directoryName, dataFile.RelativePath);
					string directoryName2 = Path.GetDirectoryName(StaticMethods.RemapPathToProject(CivTechService.ProjectMapService, document.Uri.LocalPath, ProjectName));
					directoryName2 = Path.Combine(directoryName2, dataFile.RelativePath);
					if (!global::DatabaseWrapper.DatabaseWrapper.OpenForEditOrAdd(directoryName2, out errMsg))
					{
						MessageBoxes.Show("The destination data file \"" + directoryName2 + "\" for entity \"" + instanceEntityDocument.InstanceEntity.Name + "\" could not be opened for edit in project \"" + ProjectName + "\".\n\n" + errMsg);
						continue;
					}
					if (!new FileInfo(text).Exists)
					{
						MessageBoxes.Show("The source data file \"" + text + "\" for entity \"" + instanceEntityDocument.InstanceEntity.Name + "\" could not be found on disk for project \"" + ProjectName + "\".\n\n" + errMsg);
						continue;
					}
					FileInfo fileInfo = new FileInfo(directoryName2);
					if (!fileInfo.Exists)
					{
						continue;
					}
					try
					{
						if (fileInfo.IsReadOnly)
						{
							fileInfo.IsReadOnly = false;
						}
						fileInfo.CopyTo(directoryName2, overwrite: true);
					}
					catch (SystemException ex)
					{
						MessageBoxes.Show("Failed to copy source data file \"" + text + "\" for entity \"" + instanceEntityDocument.InstanceEntity.Name + "\" to target location \"" + directoryName2 + "\".\n\n" + ex.Message);
					}
				}
			}
		}
		Uri uri = new Uri(StaticMethods.RemapPathToProject(CivTechService.ProjectMapService, document.Uri.LocalPath, ProjectName));
		return DoSave(document, uri, DocumentEventType.SavedAs, forceSave: true);
	}

	protected bool DoMove(IDocument document)
	{
		string projectName = ShowProjectPickingDialog("Move To Project", "Pick the Project to move the document to:");
		List<string> list = new List<string>();
		list.Add(document.Uri.LocalPath);
		if (document is IInstanceEntityDocument instanceEntityDocument)
		{
			string directoryName = Path.GetDirectoryName(document.Uri.LocalPath);
			foreach (IInstanceDataFile dataFile in instanceEntityDocument.InstanceEntity.DataFiles)
			{
				string item = Path.Combine(directoryName, dataFile.RelativePath);
				list.Add(item);
			}
		}
		if (CopyDocument(document, projectName))
		{
			string errMsg = string.Empty;
			foreach (string item2 in list)
			{
				global::DatabaseWrapper.DatabaseWrapper.DeleteFile(item2, out errMsg);
			}
		}
		return true;
	}

	protected bool deleteDocument(IDocument document)
	{
		string errMsg = string.Empty;
		IInstanceEntityDocument instanceEntityDocument = document as IInstanceEntityDocument;
		if (instanceEntityDocument != null)
		{
			global::DatabaseWrapper.DatabaseWrapper.RevertEntityIfOpen(instanceEntityDocument.InstanceEntity, out errMsg);
			global::DatabaseWrapper.DatabaseWrapper.DeleteFile(document.Uri.LocalPath, out errMsg);
			string directoryName = Path.GetDirectoryName(document.Uri.LocalPath);
			foreach (IInstanceDataFile dataFile in instanceEntityDocument.InstanceEntity.DataFiles)
			{
				global::DatabaseWrapper.DatabaseWrapper.DeleteFile(Path.Combine(directoryName, dataFile.RelativePath), out errMsg);
			}
		}
		else if (!global::DatabaseWrapper.DatabaseWrapper.DeleteFile(document.Uri.LocalPath, out errMsg))
		{
			MessageBoxes.Show("Entity " + instanceEntityDocument.InstanceEntity.Name + " could not be deleted.\n" + errMsg);
			return false;
		}
		return true;
	}

	private string ShowProjectPickingDialog(string title, string message)
	{
		if (string.IsNullOrEmpty(lastTargetProject))
		{
			lastTargetProject = CivTechService.PrimaryProject.Name;
		}
		Form form = new Form
		{
			StartPosition = FormStartPosition.CenterParent,
			Width = 480,
			Height = 150,
			Text = title,
			FormBorderStyle = FormBorderStyle.FixedDialog,
			MaximizeBox = false,
			MinimizeBox = false
		};
		Rectangle clientRectangle = form.ClientRectangle;
		string returnValue = string.Empty;
		Label label = new Label
		{
			Left = 9,
			Top = 10,
			AutoSize = true
		};
		label.Text = message;
		ComboBox projectComboBox = new ComboBox
		{
			Left = 10,
			Top = label.Top + label.Height + 2,
			Width = clientRectangle.Width - 20,
			Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right),
			DropDownStyle = ComboBoxStyle.DropDownList,
			TabIndex = 0
		};
		List<string> list = new List<string>(CivTechService.AllProjectsMap.ProjectNames);
		list.Sort();
		int selectedIndex = 0;
		foreach (string item in list)
		{
			if (item.Equals(lastTargetProject, StringComparison.CurrentCultureIgnoreCase))
			{
				selectedIndex = projectComboBox.Items.Count;
			}
			projectComboBox.Items.Add(item);
		}
		projectComboBox.SelectedIndex = selectedIndex;
		Button button = new Button
		{
			Text = "Cancel",
			Left = clientRectangle.Width - 100 - 10,
			Width = 100,
			Height = 25,
			Top = clientRectangle.Height - 25 - 10,
			Anchor = (AnchorStyles.Bottom | AnchorStyles.Right),
			DialogResult = DialogResult.Cancel,
			TabIndex = 2
		};
		Button button2 = new Button
		{
			Text = "OK",
			Left = button.Left - 10 - 100,
			Width = 100,
			Height = 25,
			Top = button.Top,
			Anchor = (AnchorStyles.Bottom | AnchorStyles.Right),
			DialogResult = DialogResult.OK,
			TabIndex = 1
		};
		button2.Click += delegate
		{
			lastTargetProject = (returnValue = projectComboBox.Text);
		};
		form.Controls.Add(button2);
		form.Controls.Add(button);
		form.Controls.Add(label);
		form.Controls.Add(projectComboBox);
		form.AcceptButton = button2;
		form.CancelButton = button;
		form.ShowDialog();
		return returnValue;
	}

	protected bool DoSave(IDocument document, Uri uri, DocumentEventType kind, bool forceSave = false)
	{
		if (FileWatcherService.UseSinglePantryMode)
		{
			string fileName = Path.GetFileName(uri.LocalPath);
			uri = new Uri(Path.Combine(CivTechService.GetBaseGamePantryPath(), "Assets", fileName));
		}
		IsSaving = true;
		if (document.Uri != uri)
		{
			document.Uri = uri;
		}
		if (document.IsReadOnly && !CivTechService.IsFromActiveProjectOrDependencies(document.Uri) && !FileWatcherService.UseSinglePantryMode && !forceSave)
		{
			MessageBoxes.Show("Can not modify assets that are not part of the active project \"" + CivTechService.PrimaryProject.Name + "\" or one of its dependencies", "File not saved", MessageBoxButton.OK, MessageBoxImage.Error);
			return false;
		}
		bool flag = false;
		if (forceSave || OnDocumentSaving(document))
		{
			this.DocumentSaving.Raise(this, new DocumentEventArgs(document, kind));
			using (new FileWatchSuspension(FileWatcherService, uri))
			{
				flag = GetClient(document).Save(document, uri);
				if (!flag)
				{
					Action<string> action = delegate(string msg)
					{
						switch (FileSaveFailureBehavior)
						{
						case FileSaveFailureBehavior.Log:
							Outputs.WriteLine(OutputMessageType.Error, msg);
							break;
						case FileSaveFailureBehavior.Prompt:
							MessageBoxes.Show(msg, "Failed to save", MessageBoxButton.OK, MessageBoxImage.Error);
							break;
						case FileSaveFailureBehavior.Quiet:
							break;
						}
					};
					if ((File.GetAttributes(uri.LocalPath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
					{
						action("Failed to write to file " + uri.LocalPath + " because it is read-only. Did the open for edit fail?");
					}
					else if (!Directory.Exists(Path.GetDirectoryName(document.Uri.LocalPath)))
					{
						action("Failed to write to file " + uri.LocalPath + " because the folder it lives in can not be found!");
					}
					else
					{
						action("Failed to write to file " + uri.LocalPath + " for unknown reason!");
					}
				}
			}
			flag &= OnDocumentSaved(document);
			if (flag)
			{
				document.Dirty = false;
				m_newDocumentPaths.Remove(document.Uri.LocalPath);
				if (document is IInstanceEntityDocument instanceEntityDocument && global::DatabaseWrapper.DatabaseWrapper.IsEntityNameAvailable(CivTechService.PrimaryProject.Name, instanceEntityDocument.InstanceEntity.Type, instanceEntityDocument.InstanceEntity.Name))
				{
					kind = DocumentEventType.SavedAs;
				}
				this.DocumentSaved.Raise(this, new DocumentEventArgs(document, kind));
			}
		}
		IsSaving = false;
		return flag;
	}

	protected IDocument FindOpenDocument(Uri uri)
	{
		return DocumentRegistryMediator.DocumentRegistry.Documents.FirstOrDefault((IDocument doc) => doc.Uri.Equals(uri));
	}

	protected IDocumentClient GetClient(Uri uri)
	{
		IDocument document = FindOpenDocument(uri);
		if (document != null)
		{
			return GetClient(document);
		}
		string extension = Path.GetExtension(uri.LocalPath);
		Lazy<IDocumentClient>[] documentClients = m_documentClients;
		for (int i = 0; i < documentClients.Length; i++)
		{
			IDocumentClient value = documentClients[i].Value;
			string[] extensions = value.Info.Extensions;
			for (int j = 0; j < extensions.Length; j++)
			{
				if (string.Compare(extensions[j], extension, ignoreCase: true) == 0)
				{
					return value;
				}
			}
		}
		return null;
	}

	protected virtual Uri GetNewDocumentUri(IDocumentClient client)
	{
		Uri result = null;
		if (client.Info.FileType == "ShadowDocument")
		{
			result = GetShadowDocumentUri(client);
		}
		else if (client.Info.Extensions.Length > 1)
		{
			result = GetNewDocumentUriFromUser(client);
		}
		else if (client.Info.Extensions.Length == 1)
		{
			string directoryName = GetInitialDirectoryName(client);
			if (FileWatcherService.UseSinglePantryMode)
			{
				directoryName = Path.Combine(CivTechService.GetBaseGamePantryPath(), "Assets");
			}
			string extension = client.Info.Extensions[0];
			result = GetNewDocumentUriUnique(client, directoryName, extension);
		}
		return result;
	}

	protected virtual void Initialize()
	{
	}

	protected virtual void OnDocumentClosed(IDocument document)
	{
		if (document is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}

	protected virtual void OnDocumentClosing(IDocument document)
	{
	}

	protected virtual void OnDocumentOpened(IDocument document)
	{
	}

	protected virtual bool OnDocumentOpening(Uri uri)
	{
		return true;
	}

	protected virtual bool OnDocumentSaved(IDocument document)
	{
		return true;
	}

	protected virtual bool OnDocumentSaving(IDocument document)
	{
		bool flag = !document.IsReadOnly;
		if (flag)
		{
			if (document is IInstanceEntityDocument instanceEntityDocument)
			{
				List<string> list = new List<string>();
				if (!instanceEntityDocument.MeetsSavePreconditions(list))
				{
					string text = string.Join("\n", list);
					string errMsg = "";
					if (global::DatabaseWrapper.DatabaseWrapper.RevertEntityIfOpen(instanceEntityDocument.InstanceEntity, out errMsg))
					{
						text = text + "\n" + errMsg;
					}
					MessageBoxes.Show(text, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
					flag = false;
				}
			}
		}
		else if (document is IVersionProvider)
		{
			flag = MessageBoxes.Show("Warning!  You are saving " + document.Uri.LocalPath + " with tools older than the last tools used to modify it.  There may be data loss.  Are you should you would like to continue?", "Save Prompt", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes;
		}
		if (flag)
		{
			string localPath = document.Uri.LocalPath;
			flag = !m_unsaveableDocuments.ContainsKey(localPath);
			if (!flag && !string.IsNullOrEmpty(m_unsaveableDocuments[localPath]))
			{
				MessageBoxes.Show(m_unsaveableDocuments[localPath], "Can't Save File", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		return flag;
	}

	protected virtual void OnOpenException(System.Exception ex)
	{
	}

	protected virtual void OnSaveException(System.Exception ex)
	{
		Outputs.WriteLine(OutputMessageType.Error, "There was a problem saving the file".Localize() + ": {0}", ex.Message);
	}

	protected void ShowStatus(string message)
	{
		if (m_statusService != null)
		{
			m_statusService.ShowStatus(message);
		}
	}

	private TextInputValidationForm CreateTextInputValidationForm(string projectName, string baseName, IInstanceEntity entity)
	{
		Predicate<string> forceActionResponse = delegate(string entityName2)
		{
			ProjectEnvironment projectEnvironment = CivTechService.ActiveProjectMap[projectName];
			string entityName3 = StaticMethods.SanitizeEntityName(entityName2);
			string entityPath = CivTechService.GetEntityPath(entityName3, entity.Type);
			if (Path.IsPathRooted(entityPath))
			{
				try
				{
					Uri item = new Uri(entityPath);
					IEnumerable<Uri> dependents = projectEnvironment.DependencyRegistry.GetDependents(item);
					if (dependents.Any())
					{
						List<EntityID> source = dependents.Select((Uri uri) => StaticMethods.GetEntityIDFromPath(CivTechService.ProjectMapService, uri.LocalPath)).ToList();
						return MessageBoxes.Show("The following entities will be affected by this change.  Continue?\n\n" + string.Join(";\t", source.Select((EntityID ent) => "(" + ent.ToString() + ")")), "Continue?", MessageBoxButton.YesNo, MessageBoxImage.None) == MessageBoxResult.Yes;
					}
				}
				catch (System.Exception)
				{
				}
			}
			return true;
		};
		Predicate<string> validationFunction = delegate(string text)
		{
			string text2 = StaticMethods.SanitizeEntityName(text);
			bool num = text2 != baseName;
			bool flag = global::DatabaseWrapper.DatabaseWrapper.IsEntityNameAvailable(projectName, entity.Type, text2);
			return num && flag;
		};
		string name = (entity.Name.Contains(baseName) ? entity.Name : SmartDefaultNameGuess(entity));
		return new TextInputValidationForm(validationFunction, name)
		{
			FormTitle = "Entity Name Selector",
			InputLabel = "Choose a new name for your entity.",
			InvalidInputLabel = "An entity with that name already exists, choose another.",
			ForceMessage = "Overwrite Existing",
			ForceActionResponse = forceActionResponse
		};
	}

	private IDocument DoOpen(IDocumentClient client, Uri uri)
	{
		IDocument document = null;
		if (client.CanOpen(uri) && OnDocumentOpening(uri))
		{
			document = client.Open(uri);
			if (document != null)
			{
				OnDocumentOpened(document);
				this.DocumentOpened.Raise(this, new DocumentEventArgs(document, DocumentEventType.Opened));
				IDocumentRegistry documentRegistry = null;
				documentRegistry = ((!(document is IShadowDocument)) ? DocumentRegistryMediator.DocumentRegistry : ((IShadowDocumentRegistryProvider)DocumentRegistryMediator).DocumentRegistry);
				documentRegistry.ActiveDocument = document;
			}
		}
		return document;
	}

	private static string FindStringIntersection(string str1, string str2)
	{
		string result = string.Empty;
		for (int i = 0; i < str1.Length; i++)
		{
			if (str2.Length >= i + 1)
			{
				if (str1[i] != str2[i])
				{
					return str1.Substring(0, i);
				}
				result = str1.Substring(0, i + 1);
				continue;
			}
			return str1.Substring(0, i);
		}
		return result;
	}

	private string FormatExtensionString(string[] exts)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (exts.Length != 0)
		{
			for (int i = 0; i < exts.Length - 1; i++)
			{
				stringBuilder.AppendFormat("*{0};", exts[i]);
			}
			stringBuilder.AppendFormat("*{0}", exts[exts.Length - 1]);
		}
		return stringBuilder.ToString();
	}

	private string GetCommonString(IList<string> stringList)
	{
		string text = ((stringList.Count >= 1) ? stringList[0] : string.Empty);
		if (stringList.Count > 1)
		{
			string text2 = text;
			for (int i = 1; i < stringList.Count; i++)
			{
				if (text2.Length <= 0)
				{
					break;
				}
				text2 = FindStringIntersection(text2, stringList[i]);
			}
			text = ((text2.Length > 0) ? text2 : stringList.Last());
		}
		return text.Trim('_');
	}

	private Uri GetEntityDocumentURI(InstanceType entityType, string entityName)
	{
		Uri result = null;
		Lazy<IDocumentClient>[] documentClients = m_documentClients;
		for (int i = 0; i < documentClients.Length; i++)
		{
			if (documentClients[i].Value.Info is EntityDocumentClientInfo entityDocumentClientInfo && entityDocumentClientInfo.ExtensionTypes.Contains(entityType))
			{
				result = new Uri(global::DatabaseWrapper.DatabaseWrapper.GetDataFile(entityType, entityName));
				break;
			}
		}
		return result;
	}

	private string GetInitialDirectoryName(IDocumentClient client)
	{
		if (client == null)
		{
			throw new ArgumentNullException("client");
		}
		if (client.Info.FileType == "ShadowDocument")
		{
			throw new ArgumentException("The client passed in cannot have the ShadowDocument file type.");
		}
		string text = client.Info.InitialDirectory;
		if (string.IsNullOrEmpty(text))
		{
			text = GetInitialPantryDirectory(client.Info);
		}
		return text;
	}

	private ProjectEnvironment GetBaseProject()
	{
		ProjectEnvironment projectEnvironment = CivTechService.ProjectMapService.ActiveProjectMap.Projects.Where((ProjectEnvironment proj) => proj.Dependencies.Count == 0).FirstOrDefault();
		if (projectEnvironment == null)
		{
			projectEnvironment = CivTechService.ProjectMapService.PrimaryProject;
		}
		return projectEnvironment;
	}

	private string GetInitialPantryDirectory(DocumentClientInfo info)
	{
		string text = CivTechService.PrimaryProject.Paths.GamePantry;
		if (FileWatcherService.UseSinglePantryMode)
		{
			text = GetBaseProject().Paths.GamePantry;
		}
		if (info is EntityDocumentClientInfo entityDocumentClientInfo && entityDocumentClientInfo.ExtensionTypes.Any())
		{
			text = StaticMethods.PantryRootForInstanceType(text, entityDocumentClientInfo.ExtensionTypes.First());
		}
		return text;
	}

	private Uri GetNewDocumentUriFromUser(IDocumentClient client)
	{
		if (client.Info.FileType == "ShadowDocument")
		{
			throw new ArgumentException("The client passed in cannot have the ShadowDocument file type.");
		}
		Uri result = null;
		string newDocumentName = client.Info.NewDocumentName;
		string newFilePath = GetNewFilePath(client, newDocumentName, null, forcePrompt: false);
		if (!string.IsNullOrEmpty(newFilePath))
		{
			try
			{
				if (File.Exists(newFilePath))
				{
					File.Delete(newFilePath);
				}
			}
			catch (System.Exception arg)
			{
				string message = $"Failed to delete: {newFilePath}. Exception: {arg}";
				Outputs.WriteLine(OutputMessageType.Warning, message);
			}
			m_newDocumentPaths.Add(newFilePath);
			result = new Uri(newFilePath, UriKind.RelativeOrAbsolute);
		}
		return result;
	}

	private Uri GetNewDocumentUriUnique(IDocumentClient client, string directoryName, string extension)
	{
		if (client.Info.FileType == "ShadowDocument")
		{
			throw new ArgumentException("The client passed in cannot have the ShadowDocument file type.");
		}
		Uri uri = null;
		string text = $"{client.Info.NewDocumentName}{DateTime.Now.ToBinary():X16}";
		if (!string.IsNullOrEmpty(directoryName) && !string.IsNullOrEmpty(extension))
		{
			int value = 0;
			m_extensionSuffixes.TryGetValue(extension, out value);
			string path = text + extension;
			string text2 = Path.Combine(directoryName, path);
			uri = new Uri(text2, UriKind.RelativeOrAbsolute);
			bool flag = FileDialogService.PathExists(text2) || DocumentRegistryMediator.DocumentRegistry.Documents.Any((IDocument doc) => doc.Uri.Equals(uri));
			while (flag)
			{
				path = $"{text}({++value}){extension}";
				text2 = Path.Combine(directoryName, path);
				uri = new Uri(text2, UriKind.RelativeOrAbsolute);
				flag = FileDialogService.PathExists(text2) || DocumentRegistryMediator.DocumentRegistry.Documents.Any((IDocument doc) => doc.Uri.Equals(uri));
			}
			m_extensionSuffixes[extension] = value;
		}
		return uri;
	}

	private string GetNewEntityFilePath(string projectName, IDocumentClient client, string filePath, IInstanceEntityDocument entDoc, bool forcePrompt)
	{
		EntityDocumentClientInfo entityDocumentClientInfo = client.Info as EntityDocumentClientInfo;
		if (!entDoc.HasNameSet(entityDocumentClientInfo.NewDocumentName) || forcePrompt)
		{
			TextInputValidationForm textInputValidationForm = CreateTextInputValidationForm(projectName, entityDocumentClientInfo.NewEntityName, entDoc.InstanceEntity);
			if (textInputValidationForm.ShowDialog() != DialogResult.OK)
			{
				return null;
			}
			entDoc.As<INamedAdapter>().Name = StaticMethods.SanitizeEntityName(textInputValidationForm.UserText);
			string errMsg = string.Empty;
			if (!global::DatabaseWrapper.DatabaseWrapper.OpenIfRequired(entDoc.InstanceEntity, out errMsg))
			{
				MessageBoxes.Show("Entity " + entDoc.InstanceEntity.Name + " could not be opened for edit.\n" + errMsg);
			}
			string path = Path.GetDirectoryName(filePath);
			if (FileWatcherService.UseSinglePantryMode)
			{
				path = Path.Combine(CivTechService.GetBaseGamePantryPath(), "Assets");
			}
			filePath = Path.Combine(path, textInputValidationForm.UserText);
			if (AssetBrowserService.SaveFileName(out filePath, entDoc.InstanceEntity) != DialogResult.OK)
			{
				return null;
			}
		}
		return filePath;
	}

	private string GetNewFilePath(IDocumentClient client, string fileName, IDocument existingDocument, bool forcePrompt)
	{
		if (existingDocument is IShadowEntityDocument)
		{
			return fileName;
		}
		string pathName = fileName;
		FileDialogService.InitialDirectory = client.Info.InitialDirectory;
		string filter = string.Format("{0} ({1})|{1}", client.Info.FileType, FormatExtensionString(client.Info.Extensions));
		if (existingDocument is IInstanceEntityDocument entDoc)
		{
			pathName = ((!FileWatcherService.UseSinglePantryMode) ? GetNewEntityFilePath(CivTechService.PrimaryProject.Name, client, pathName, entDoc, forcePrompt) : Path.Combine(CivTechService.GetBaseGamePantryPath(), "Assets", Path.GetFileName(pathName)));
			if (string.IsNullOrEmpty(pathName))
			{
				return null;
			}
		}
		else if (FileDialogService.SaveFileName(ref pathName, filter) != FileDialogResult.OK)
		{
			return null;
		}
		if (!client.Info.IsCompatiblePath(pathName))
		{
			Outputs.WriteLine(OutputMessageType.Error, "File extension not supported".Localize());
			return null;
		}
		Uri uri = new Uri(pathName);
		IDocument document = FindOpenDocument(uri);
		if (document != null && document != existingDocument)
		{
			Outputs.WriteLine(OutputMessageType.Error, "A file with that name is already open".Localize());
			return null;
		}
		return pathName;
	}

	private Uri GetShadowDocumentUri(IDocumentClient client)
	{
		if (client.Info.FileType != "ShadowDocument")
		{
			throw new ArgumentException("The client passed in must have the ShadowDocument file type.");
		}
		EntityDocumentClientInfo entityDocumentClientInfo = client.Info as EntityDocumentClientInfo;
		return new Uri(Path.Combine(StaticMethods.PantryRootForInstanceType(CivTechService.PrimaryProject.Paths.GamePantry, entityDocumentClientInfo.ExtensionTypes.First()), $"NewShadowDocument_{m_shadowDocumentCount++}{entityDocumentClientInfo.Extensions.First()}").Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
	}

	private IDocumentClient PromptUserForNewDocumentClient(IEnumerable<IDocumentClient> clients)
	{
		IDocumentClient result = null;
		IList<DocumentClientInfo> list = new List<DocumentClientInfo>();
		foreach (IDocumentClient client in clients)
		{
			list.Add(client.Info);
		}
		FileTypeSelectionForm entitySelector = new FileTypeSelectionForm(list);
		entitySelector.ShowDialog();
		if (!string.IsNullOrEmpty(entitySelector.SelectedEntityType))
		{
			result = clients.Where((IDocumentClient client) => client.Info.FileType == entitySelector.SelectedEntityType).FirstOrDefault();
		}
		return result;
	}

	private void RegisterClientCommands()
	{
		Sce.Atf.Input.Keys shortcut = Sce.Atf.Input.Keys.N | Sce.Atf.Input.Keys.Control;
		Sce.Atf.Input.Keys shortcut2 = Sce.Atf.Input.Keys.O | Sce.Atf.Input.Keys.Control;
		Sce.Atf.Input.Keys shortcut3 = Sce.Atf.Input.Keys.O | Sce.Atf.Input.Keys.Shift | Sce.Atf.Input.Keys.Control;
		Sce.Atf.Input.Keys shortcut4 = Sce.Atf.Input.Keys.O | Sce.Atf.Input.Keys.Shift | Sce.Atf.Input.Keys.Control | Sce.Atf.Input.Keys.Alt;
		Sce.Atf.Input.Keys shortcut5 = Sce.Atf.Input.Keys.O | Sce.Atf.Input.Keys.Control | Sce.Atf.Input.Keys.Alt;
		List<IDocumentClient> list = new List<IDocumentClient>();
		Lazy<IDocumentClient>[] documentClients = m_documentClients;
		foreach (Lazy<IDocumentClient> lazy in documentClients)
		{
			list.Add(lazy.Value);
		}
		if ((RegisterCommands & CommandRegister.FileNew) == CommandRegister.FileNew)
		{
			CommandService.RegisterCommand(new CommandInfo(new FileCommandMultiTag(Command.FileNew, list), StandardMenu.File, StandardCommandGroup.FileNew, "New".Localize("Name of a command"), "Creates a new document".Localize(), shortcut, Sce.Atf.Resources.DocumentImage, CommandVisibility.All), this);
		}
		if ((RegisterCommands & CommandRegister.FileOpen) == CommandRegister.FileOpen)
		{
			IEnumerable<IDocumentClient> enumerable = list.Where((IDocumentClient doc) => doc.Info is DocumentClientInfoEx);
			IDocumentClient documentClient = list.FirstOrDefault((IDocumentClient docClt) => docClt.Info.Extensions.Contains(".xlp"));
			IDocumentClient documentClient2 = list.FirstOrDefault((IDocumentClient docClt) => docClt.Info.Extensions.Contains(".artdef"));
			IDocumentClient documentClient3 = list.FirstOrDefault((IDocumentClient docClt) => docClt.Info.Extensions.Contains(".Art.xml"));
			if (enumerable.Any())
			{
				CommandService.RegisterCommand(new CommandInfo(new FileCommandMultiTag(Command.FileOpenEntity, enumerable), StandardMenu.File, StandardCommandGroup.FileNew, "Open Entity".Localize("Name of a command"), "Open an existing entity document".Localize(), shortcut2, Resources.OpenEntityIcon, CommandVisibility.All), this);
			}
			if (documentClient != null)
			{
				CommandService.RegisterCommand(new CommandInfo(new FileCommandTag(Command.FileOpenXLP, documentClient), StandardMenu.File, StandardCommandGroup.FileNew, "Open XLP".Localize("Name of a command"), "Open an existing XLP document".Localize(), shortcut3, documentClient.Info.OpenIconName, CommandVisibility.All), this);
			}
		if (documentClient2 != null)
		{
			CommandService.RegisterCommand(new CommandInfo(new FileCommandTag(Command.FileOpenArtDef, documentClient2), StandardMenu.File, StandardCommandGroup.FileNew, "Open Art Def".Localize("Name of a command"), "Open an existing art definition document".Localize(), shortcut4, documentClient2.Info.OpenIconName, CommandVisibility.All), this);
		}
		if (documentClient != null)
		{
			CommandService.RegisterCommand(new CommandInfo(new FileCommandTag(Command.FileOpenXLPFromAssets, documentClient), StandardMenu.File, StandardCommandGroup.FileNew, "Open XLP From Assets".Localize("Name of a command"), "Open an existing XLP document from Assets".Localize(), Sce.Atf.Input.Keys.None, documentClient.Info.OpenIconName, CommandVisibility.All), this);
		}
		if (documentClient2 != null)
		{
			CommandService.RegisterCommand(new CommandInfo(new FileCommandTag(Command.FileOpenArtDefFromAssets, documentClient2), StandardMenu.File, StandardCommandGroup.FileNew, "Open Art Def From Assets".Localize("Name of a command"), "Open an existing art definition document from Assets".Localize(), Sce.Atf.Input.Keys.None, documentClient2.Info.OpenIconName, CommandVisibility.All), this);
		}
		if (documentClient3 != null)
		{
			CommandService.RegisterCommand(new CommandInfo(new FileCommandTag(Command.FileOpenArtSpecification, documentClient3), StandardMenu.File, StandardCommandGroup.FileNew, "Open Art.xml".Localize("Name of a command"), "Open an existing art specification document".Localize(), shortcut5, documentClient3.Info.OpenIconName, CommandVisibility.All), this);
		}
			CommandService.RegisterCommand(new CommandInfo(new InterProjectCommandFileTag(Command.CopyFileToProject), StandardMenu.File, StandardCommandGroup.FileOther, "Copy To Project".Localize("Name of a command"), "Makes a duplicate of the active file in a given project".Localize(), Sce.Atf.Input.Keys.None, string.Empty, CommandVisibility.ApplicationMenu), this);
			CommandService.RegisterCommand(new CommandInfo(new InterProjectCommandFileTag(Command.MoveFileToProject), StandardMenu.File, StandardCommandGroup.FileOther, "Move To Project".Localize("Name of a command"), "Move a file from its current location to a given project".Localize(), Sce.Atf.Input.Keys.None, string.Empty, CommandVisibility.ApplicationMenu), this);
		}
	}

	private string SmartDefaultNameGuess(IInstanceEntity instanceEntity)
	{
		string result = instanceEntity.Name;
		if (instanceEntity.Type == InstanceType.IT_ASSET)
		{
			IList<string> stringList = (instanceEntity as IAssetInstance).GeometrySet.ModelInstances.Select((IModelInstance modelInstance) => modelInstance.GeoName).ToList();
			result = GetCommonString(stringList);
		}
		else if (instanceEntity.Type == InstanceType.IT_MATERIAL)
		{
			IList<string> stringList2 = (from val in instanceEntity.CookParameters.ItemsOfType<IObjectValue>()
				select val.GetBoundObjectName() into name
				where !string.IsNullOrEmpty(name)
				select name).ToList();
			result = GetCommonString(stringList2);
		}
		return result;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				CommandService.UnregisterCommand(StandardCommand.FileSave, this);
				CommandService.UnregisterCommand(StandardCommand.FileSaveAs, this);
				CommandService.UnregisterCommand(StandardCommand.FileSaveAll, this);
				CommandService.UnregisterCommand(StandardCommand.FileClose, this);
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
