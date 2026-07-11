using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Sce.Atf.Input;

namespace Sce.Atf.Applications;

[Export(typeof(IDocumentService))]
[Export(typeof(StandardFileCommands))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StandardFileCommands : IDocumentService, ICommandClient, IInitializable
{
	[Flags]
	public enum CommandRegister
	{
		None = 0,
		FileNew = 1,
		FileOpen = 2,
		FileSave = 4,
		FileSaveAs = 8,
		FileSaveAll = 0x10,
		FileClose = 0x20,
		Default = 0x3F
	}

	private enum Command
	{
		FileNew,
		FileOpen
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

	[Import(AllowDefault = true)]
	private IStatusService m_statusService = null;

	[ImportMany]
	private Lazy<IDocumentClient>[] m_documentClients = null;

	private readonly IDictionary<string, string> m_unsaveableDocuments = new Dictionary<string, string>();

	private readonly HashSet<IDocument> m_untitledDocuments = new HashSet<IDocument>();

	private readonly HashSet<string> m_newDocumentPaths = new HashSet<string>();

	private readonly Dictionary<string, IDocumentClient> m_typeToClientMap = new Dictionary<string, IDocumentClient>();

	private readonly Dictionary<string, int> m_extensionSuffixes = new Dictionary<string, int>();

	private CommandRegister m_registerCommands = CommandRegister.Default;

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

	public bool IsSaving { get; private set; }

	protected ICommandService CommandService { get; private set; }

	protected IDocumentRegistry DocumentRegistry { get; private set; }

	protected IFileDialogService FileDialogService { get; private set; }

	public FileSaveFailureBehavior FileSaveFailureBehavior { get; set; }

	public event EventHandler<DocumentEventArgs> DocumentOpened;

	public event EventHandler<DocumentEventArgs> DocumentSaving;

	public event EventHandler<DocumentEventArgs> DocumentSaved;

	public event EventHandler<DocumentClosingEventArgs> DocumentClosing;

	public event EventHandler<DocumentEventArgs> DocumentClosed;

	[ImportingConstructor]
	public StandardFileCommands(ICommandService commandService, IDocumentRegistry documentRegistry, IFileDialogService fileDialogService)
	{
		CommandService = commandService;
		DocumentRegistry = documentRegistry;
		documentRegistry.ActiveDocumentChanging += ActiveDocumentChanging;
		documentRegistry.ActiveDocumentChanged += ActiveDocumentChanged;
		FileDialogService = fileDialogService;
	}

	void IInitializable.Initialize()
	{
		m_typeToClientMap.Clear();
		Lazy<IDocumentClient>[] documentClients = m_documentClients;
		foreach (Lazy<IDocumentClient> lazy in documentClients)
		{
			IDocumentClient value = lazy.Value;
			m_typeToClientMap.Add(value.Info.FileType, value);
		}
		CommandInfo.FileSave.EnableCheckCanDoEvent(this);
		CommandInfo.FileSaveAs.EnableCheckCanDoEvent(this);
		CommandInfo.FileSaveAll.EnableCheckCanDoEvent(this);
		CommandInfo.FileClose.EnableCheckCanDoEvent(this);
		if ((RegisterCommands & CommandRegister.FileSave) == CommandRegister.FileSave)
		{
			CommandService.RegisterCommand(CommandInfo.FileSave, this);
		}
		if ((RegisterCommands & CommandRegister.FileSaveAs) == CommandRegister.FileSaveAs)
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
			document = SafeOpen(client, newDocumentUri);
			if (document != null && !m_newDocumentPaths.Contains(document.Uri.LocalPath) && !FileDialogService.PathExists(document.Uri.LocalPath))
			{
				m_untitledDocuments.Add(document);
			}
		}
		return document;
	}

	public virtual IDocument OpenExistingDocument(IDocumentClient client, Uri uri)
	{
		if (client == null)
		{
			throw new ArgumentNullException("client");
		}
		string filterString = client.Info.GetFilterString();
		string[] pathNames = null;
		if (uri != null)
		{
			pathNames = new string[1] { uri.ToString() };
		}
		else
		{
			FileDialogService.InitialDirectory = client.Info.InitialDirectory;
			FileDialogService.OpenFileNames(ref pathNames, filterString);
		}
		IDocument result = null;
		if (pathNames != null)
		{
			string[] array = pathNames;
			foreach (string uriString in array)
			{
				Uri uri2 = new Uri(uriString, UriKind.RelativeOrAbsolute);
				IDocument document = FindOpenDocument(uri2);
				if (document != null)
				{
					result = document;
					client.Show(document);
				}
				else
				{
					result = SafeOpen(client, uri2);
				}
			}
		}
		return result;
	}

	public virtual bool Save(IDocument document)
	{
		if (IsUntitled(document))
		{
			return SaveAs(document);
		}
		return SafeSave(document, DocumentEventType.Saved);
	}

	public virtual bool SaveAs(IDocument document)
	{
		IDocumentClient client = GetClient(document);
		string localPath = document.Uri.LocalPath;
		string text = PromptUserForNewFilePath(client, localPath, document);
		if (text != null)
		{
			FileDialogService.InitialDirectory = Path.GetDirectoryName(text);
			Uri uri = document.Uri;
			Uri uri2 = new Uri(text);
			document.Uri = uri2;
			bool flag = SafeSave(document, DocumentEventType.SavedAs);
			if (flag)
			{
				DocumentRegistry.Remove(document);
				DocumentRegistry.ActiveDocument = document;
				m_untitledDocuments.Remove(document);
				m_newDocumentPaths.Remove(localPath);
			}
			else
			{
				document.Uri = uri;
			}
			return flag;
		}
		return false;
	}

	public virtual bool SaveAs(IDocument document, Uri uri)
	{
		string localPath = document.Uri.LocalPath;
		FileDialogService.InitialDirectory = Path.GetDirectoryName(uri.LocalPath);
		Uri uri2 = document.Uri;
		document.Uri = uri;
		bool flag = SafeSave(document, DocumentEventType.SavedAs);
		if (flag)
		{
			DocumentRegistry.Remove(document);
			DocumentRegistry.ActiveDocument = document;
			m_untitledDocuments.Remove(document);
			m_newDocumentPaths.Remove(localPath);
		}
		else
		{
			document.Uri = uri2;
		}
		return flag;
	}

	public virtual bool SaveAll(bool cancelOnFail)
	{
		bool result = true;
		foreach (IDocument document in DocumentRegistry.Documents)
		{
			if (!Save(document))
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
		bool flag = !e.Cancel && ConfirmClose(document);
		if (flag)
		{
			IDocumentClient client = GetClient(document);
			OnDocumentClosing(document);
			client.Close(document);
			DocumentRegistry.Remove(document);
			m_untitledDocuments.Remove(document);
			m_newDocumentPaths.Remove(document.Uri.LocalPath);
			OnDocumentClosed(document);
			this.DocumentClosed.Raise(this, new DocumentEventArgs(document, DocumentEventType.Closed));
		}
		return flag;
	}

	public virtual bool CloseAll(IDocument masterDocument)
	{
		List<IDocument> list = new List<IDocument>(DocumentRegistry.Documents);
		foreach (IDocument item in list)
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
		return m_untitledDocuments.Contains(document);
	}

	public void DiscardAll()
	{
		List<IDocument> list = new List<IDocument>(DocumentRegistry.Documents);
		foreach (IDocument item in list)
		{
			item.Dirty = false;
			if (!Close(item))
			{
				throw new Exception("This code should never be reached");
			}
		}
	}

	protected virtual void Initialize()
	{
	}

	protected virtual bool OnDocumentOpening(Uri uri)
	{
		return true;
	}

	protected virtual void OnDocumentOpened(IDocument document)
	{
	}

	protected virtual void OnOpenException(Exception ex)
	{
		Outputs.WriteLine(OutputMessageType.Error, "There was a problem opening the file".Localize() + ": {0}", ex.Message);
	}

	protected virtual bool OnDocumentSaving(IDocument document)
	{
		string localPath = document.Uri.LocalPath;
		bool flag = !m_unsaveableDocuments.ContainsKey(localPath);
		if (!flag && !string.IsNullOrEmpty(m_unsaveableDocuments[localPath]))
		{
			MessageBoxes.Show(m_unsaveableDocuments[localPath], "Can't Save File", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		return flag;
	}

	protected virtual bool OnDocumentSaved(IDocument document)
	{
		return true;
	}

	protected virtual void OnSaveException(Exception ex)
	{
		Outputs.WriteLine(OutputMessageType.Error, "There was a problem saving the file".Localize() + ": {0}", ex.Message);
	}

	protected virtual void OnDocumentClosing(IDocument document)
	{
	}

	protected virtual void OnDocumentClosed(IDocument document)
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
			string message = "Save".Localize() + " " + document.Uri.LocalPath + "?";
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

	protected virtual Uri GetNewDocumentUri(IDocumentClient client)
	{
		Uri result = null;
		string newDocumentName = client.Info.NewDocumentName;
		if (client.Info.Extensions.Length > 1 && string.IsNullOrEmpty(client.Info.DefaultExtension))
		{
			string text = PromptUserForNewFilePath(client, newDocumentName, null);
			if (text != null)
			{
				try
				{
					if (File.Exists(text))
					{
						File.Delete(text);
					}
				}
				catch (Exception arg)
				{
					string message = $"Failed to delete: {text}. Exception: {arg}";
					Outputs.WriteLine(OutputMessageType.Warning, message);
				}
				m_newDocumentPaths.Add(text);
				return new Uri(text, UriKind.RelativeOrAbsolute);
			}
		}
		if (client.Info.Extensions.Length >= 1)
		{
			string initialDirectory = client.Info.InitialDirectory;
			if (initialDirectory == null)
			{
				initialDirectory = FileDialogService.InitialDirectory;
			}
			string text2 = client.Info.DefaultExtension;
			if (string.IsNullOrEmpty(text2))
			{
				text2 = client.Info.Extensions[0];
			}
			if (initialDirectory != null && text2 != null)
			{
				result = GenerateUniqueUri(initialDirectory, newDocumentName, text2);
			}
		}
		return result;
	}

	protected virtual Uri GenerateUniqueUri(string directoryName, string fileName, string extension)
	{
		m_extensionSuffixes.TryGetValue(extension, out var value);
		string text2;
		do
		{
			string text = fileName;
			if (value > 0)
			{
				text = text + "(" + (value + 1) + ")";
			}
			value++;
			text += extension;
			text2 = Path.Combine(directoryName, text);
		}
		while (FileDialogService.PathExists(text2));
		Uri result = new Uri(text2, UriKind.RelativeOrAbsolute);
		m_extensionSuffixes[extension] = value;
		return result;
	}

	protected IDocument FindOpenDocument(Uri uri)
	{
		foreach (IDocument document in DocumentRegistry.Documents)
		{
			if (document.Uri.Equals(uri))
			{
				return document;
			}
		}
		return null;
	}

	protected IDocumentClient GetClient(IDocument document)
	{
		if (!m_typeToClientMap.TryGetValue(document.Type, out var value))
		{
			throw new InvalidOperationException("Document type without corresponding document client");
		}
		return value;
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
		foreach (Lazy<IDocumentClient> lazy in documentClients)
		{
			IDocumentClient value = lazy.Value;
			string[] extensions = value.Info.Extensions;
			foreach (string strA in extensions)
			{
				if (string.Compare(strA, extension, ignoreCase: true) == 0)
				{
					return value;
				}
			}
		}
		return null;
	}

	protected void ShowStatus(string message)
	{
		if (m_statusService != null)
		{
			m_statusService.ShowStatus(message);
		}
	}

	private string PromptUserForNewFilePath(IDocumentClient client, string fileName, IDocument existingDocument)
	{
		string pathName = Path.GetFileName(fileName);
		FileFilterBuilder fileFilterBuilder = new FileFilterBuilder();
		string[] extensions = client.Info.Extensions;
		string[] array = extensions;
		foreach (string text in array)
		{
			fileFilterBuilder.AddFileType(client.Info.FileType, text);
		}
		if (extensions.Length > 1)
		{
			fileFilterBuilder.AddAllFilesWithExtensions();
		}
		string filter = fileFilterBuilder.ToString();
		FileDialogService.InitialDirectory = client.Info.InitialDirectory;
		if (FileDialogService.SaveFileName(ref pathName, filter) != FileDialogResult.OK)
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

	private IDocument SafeOpen(IDocumentClient client, Uri uri)
	{
		IDocument document = null;
		try
		{
			if (client.CanOpen(uri) && OnDocumentOpening(uri))
			{
				document = client.Open(uri);
				if (document != null)
				{
					OnDocumentOpened(document);
					this.DocumentOpened.Raise(this, new DocumentEventArgs(document, DocumentEventType.Opened));
					DocumentRegistry.ActiveDocument = document;
				}
			}
		}
		catch (Exception ex)
		{
			if (DocumentRegistry.ActiveDocument == document)
			{
				DocumentRegistry.Remove(document);
			}
			document = null;
			OnOpenException(ex);
		}
		return document;
	}

	protected bool SafeSave(IDocument document, DocumentEventType kind)
	{
		IsSaving = true;
		bool flag = false;
		try
		{
			if (OnDocumentSaving(document))
			{
				this.DocumentSaving.Raise(this, new DocumentEventArgs(document, kind));
				IDocumentClient client = GetClient(document);
				flag = client.Save(document, document.Uri) && OnDocumentSaved(document);
				if (flag)
				{
					document.Dirty = false;
					m_newDocumentPaths.Remove(document.Uri.LocalPath);
					this.DocumentSaved.Raise(this, new DocumentEventArgs(document, kind));
				}
			}
		}
		catch (Exception ex)
		{
			OnSaveException(ex);
		}
		IsSaving = false;
		return flag;
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		bool result = false;
		if (commandTag is StandardCommand)
		{
			switch ((StandardCommand)commandTag)
			{
			case StandardCommand.FileClose:
			case StandardCommand.FileSave:
			case StandardCommand.FileSaveAs:
			case StandardCommand.FileSaveAll:
				result = DocumentRegistry.ActiveDocument != null;
				break;
			}
		}
		else if (commandTag is FileCommandTag)
		{
			result = true;
		}
		return result;
	}

	public virtual void DoCommand(object commandTag)
	{
		if (commandTag is StandardCommand)
		{
			IDocument activeDocument = DocumentRegistry.ActiveDocument;
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
				else
				{
					MessageBoxes.Show($"Unable to save document: {activeDocument.Uri.LocalPath}");
				}
				break;
			case StandardCommand.FileSaveAs:
				if (SaveAs(activeDocument))
				{
					ShowStatus("Document Saved As".Localize() + " " + activeDocument.Uri.LocalPath);
				}
				else
				{
					MessageBoxes.Show($"Unable to save document: {activeDocument.Uri.LocalPath}");
				}
				break;
			case StandardCommand.FileSaveAll:
			{
				bool flag = true;
				List<IDocument> list = new List<IDocument>(DocumentRegistry.Documents);
				foreach (IDocument item in list)
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
			else
			{
				OpenExistingDocument(editor, null);
			}
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private void RegisterClientCommands()
	{
		List<IDocumentClient> list = (from l in m_documentClients
			select l.Value into dc
			where dc.Info.AllowStandardFileCommands
			select dc).ToList();
		int num = 0;
		foreach (IDocumentClient item in list)
		{
			Keys shortcut = Keys.None;
			Keys shortcut2 = Keys.None;
			if (num == 0)
			{
				shortcut = Keys.N | Keys.Control;
				shortcut2 = Keys.O | Keys.Control;
			}
			object newIconKey = item.Info.NewIconKey;
			string newIconName = item.Info.NewIconName;
			if ((RegisterCommands & CommandRegister.FileNew) == CommandRegister.FileNew)
			{
				CommandService.RegisterCommand(new CommandInfo(new FileCommandTag(Command.FileNew, item), StandardMenu.File, StandardCommandGroup.FileNew, (list.Count > 1) ? ("New".Localize("Name of a command") + "/" + item.Info.FileType) : string.Format("New {0}".Localize(), item.Info.FileType), string.Format("Creates a new {0} document".Localize("{0} is the type of document to create"), item.Info.FileType), shortcut, (newIconKey != null) ? newIconKey : newIconName, (!string.IsNullOrEmpty(newIconName) || newIconKey != null) ? CommandVisibility.All : CommandVisibility.Menu), this);
			}
			object openIconKey = item.Info.OpenIconKey;
			string openIconName = item.Info.OpenIconName;
			if ((RegisterCommands & CommandRegister.FileOpen) == CommandRegister.FileOpen)
			{
				CommandService.RegisterCommand(new CommandInfo(new FileCommandTag(Command.FileOpen, item), StandardMenu.File, StandardCommandGroup.FileNew, (list.Count > 1) ? ("Open".Localize("Name of a command") + "/" + item.Info.FileType) : string.Format("Open {0}".Localize(), item.Info.FileType), string.Format("Open an existing {0} document".Localize(), item.Info.FileType), shortcut2, (openIconKey != null) ? openIconKey : openIconName, (!string.IsNullOrEmpty(openIconName) || openIconKey != null) ? CommandVisibility.All : CommandVisibility.Menu), this);
			}
			num++;
		}
	}

	private void ActiveDocumentChanging(object sender, EventArgs e)
	{
		IDocument activeDocument = DocumentRegistry.ActiveDocument;
		if (activeDocument != null)
		{
			activeDocument.DirtyChanged -= ActiveDocumentDirtyChanged;
		}
	}

	private void ActiveDocumentChanged(object sender, EventArgs eventArgs)
	{
		IDocument activeDocument = DocumentRegistry.ActiveDocument;
		if (activeDocument != null)
		{
			activeDocument.DirtyChanged += ActiveDocumentDirtyChanged;
		}
		CommandInfo.FileSave.OnCheckCanDo(this);
		CommandInfo.FileSaveAs.OnCheckCanDo(this);
		CommandInfo.FileSaveAll.OnCheckCanDo(this);
		CommandInfo.FileClose.OnCheckCanDo(this);
	}

	private void ActiveDocumentDirtyChanged(object sender, EventArgs eventArgs)
	{
		CommandInfo.FileSave.OnCheckCanDo(this);
	}
}
