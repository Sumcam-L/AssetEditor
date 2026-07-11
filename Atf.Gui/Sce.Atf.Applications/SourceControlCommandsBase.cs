using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Input;

namespace Sce.Atf.Applications;

public class SourceControlCommandsBase : ICommandClient, IContextMenuCommandProvider, IInitializable
{
	[Flags]
	public enum CommandRegister
	{
		None = 0,
		Add = 1,
		CheckIn = 2,
		CheckOut = 4,
		Sync = 8,
		Revert = 0x10,
		Refresh = 0x20,
		Reconcile = 0x40,
		Connection = 0x80,
		Enabled = 0x100,
		Default = 0x1FF
	}

	private class CommandUpdater : IDisposable
	{
		private SourceControlCommandsBase owner;

		public CommandUpdater(SourceControlCommandsBase srcCtlCmds)
		{
			owner = srcCtlCmds;
			owner.IsUpdatingCommands = true;
		}

		public void Dispose()
		{
			owner.IsUpdatingCommands = false;
		}
	}

	public enum Command
	{
		Invalid,
		Enabled,
		Add,
		CheckOut,
		CheckIn,
		Sync,
		Revert,
		Refresh,
		Reconcile,
		Connection
	}

	private enum SourceControlCommandGroup
	{
		OnOff
	}

	private delegate bool CommandFunction(bool doing);

	[Import(AllowDefault = true)]
	private IFileWatcherService m_fileWatcherService = null;

	private readonly ICommandService m_commandService;

	private readonly IDocumentService m_documentService;

	private readonly IDocumentRegistry m_documentRegistry;

	private SourceControlService m_sourceControlService;

	private CheckoutOnEditBehavior m_checkoutOnEditBehavior = CheckoutOnEditBehavior.Always;

	private FailedCheckoutBehavior m_failedCheckoutBehavior = FailedCheckoutBehavior.Prompt;

	private Command m_currentCommnd = Command.Invalid;

	private CommandRegister m_registerCommands = CommandRegister.Default;

	private CommandVisibility m_commandVisibility = CommandVisibility.Default;

	[ImportMany]
	private Lazy<IDocumentClient>[] m_documentClients = null;

	private readonly object[] m_commands = new object[8]
	{
		Command.Add,
		Command.CheckOut,
		Command.CheckIn,
		Command.Sync,
		Command.Revert,
		Command.Refresh,
		Command.Reconcile,
		Command.Connection
	};

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

	public CommandVisibility CommandVisibility
	{
		get
		{
			return m_commandVisibility;
		}
		set
		{
			m_commandVisibility = value;
		}
	}

	[Import(AllowDefault = true, AllowRecomposition = true)]
	public SourceControlService SourceControlService
	{
		get
		{
			return m_sourceControlService;
		}
		set
		{
			m_sourceControlService = value;
		}
	}

	[Obsolete("Use CheckoutOnEditBehavior instead", false)]
	public bool CheckoutOnEdit
	{
		get
		{
			return m_checkoutOnEditBehavior == CheckoutOnEditBehavior.Always;
		}
		set
		{
			m_checkoutOnEditBehavior = ((!value) ? CheckoutOnEditBehavior.Prompt : CheckoutOnEditBehavior.Always);
		}
	}

	public CheckoutOnEditBehavior CheckoutOnEditBehavior
	{
		get
		{
			return m_checkoutOnEditBehavior;
		}
		set
		{
			m_checkoutOnEditBehavior = value;
		}
	}

	public FailedCheckoutBehavior FailedCheckoutBehavior
	{
		get
		{
			return m_failedCheckoutBehavior;
		}
		set
		{
			m_failedCheckoutBehavior = value;
		}
	}

	public int Timeout { get; set; }

	public bool RefreshStatusOnSave { get; set; }

	protected bool IsUpdatingCommands { get; private set; }

	protected virtual ISourceControlContext SourceControlContext => (ContextRegistry != null) ? ContextRegistry.GetMostRecentContext<ISourceControlContext>() : null;

	protected Command CurrentCommand => m_currentCommnd;

	protected ICommandService CommandService => m_commandService;

	protected IDocumentService DocumentService => m_documentService;

	protected IDocumentRegistry DocumentRegistry => m_documentRegistry;

	[Import(AllowDefault = true)]
	protected IContextRegistry ContextRegistry { get; private set; }

	protected SourceControlCommandsBase(ICommandService commandService, IDocumentRegistry documentRegistry, IDocumentService documentService)
	{
		Timeout = 30000;
		m_commandService = commandService;
		m_documentService = documentService;
		m_documentRegistry = documentRegistry;
		documentRegistry.DocumentAdded += OnDocumentAdded;
		documentService.DocumentSaved += OnDocumentSaved;
	}

	public virtual void Initialize()
	{
		if ((RegisterCommands & CommandRegister.Enabled) == CommandRegister.Enabled)
		{
			m_commandService.RegisterCommand(Command.Enabled, StandardMenu.File, SourceControlCommandGroup.OnOff, "Source Control/Enable".Localize(), "Enable source control".Localize(), Keys.None, Resources.SourceControlEnableImage, m_commandVisibility, this);
		}
		if ((RegisterCommands & CommandRegister.Connection) == CommandRegister.Connection)
		{
			m_commandService.RegisterCommand(Command.Connection, StandardMenu.File, SourceControlCommandGroup.OnOff, "Source Control/Open Connection...".Localize(), "Source control connection".Localize(), Keys.None, Resources.SourceControlConnectionImage, m_commandVisibility, this);
		}
		if ((RegisterCommands & CommandRegister.Add) == CommandRegister.Add)
		{
			m_commandService.RegisterCommand(Command.Add, StandardMenu.File, SourceControlCommandGroup.OnOff, "Source Control/Add".Localize(), "Add to source control".Localize(), Keys.None, Resources.DocumentAddImage, m_commandVisibility, this);
		}
		if ((RegisterCommands & CommandRegister.CheckIn) == CommandRegister.CheckIn)
		{
			m_commandService.RegisterCommand(Command.CheckIn, StandardMenu.File, SourceControlCommandGroup.OnOff, "Source Control/Check In".Localize(), "Check in to source control".Localize(), Keys.None, Resources.DocumentLockImage, m_commandVisibility, this);
		}
		if ((RegisterCommands & CommandRegister.CheckOut) == CommandRegister.CheckOut)
		{
			m_commandService.RegisterCommand(Command.CheckOut, StandardMenu.File, SourceControlCommandGroup.OnOff, "Source Control/Check Out".Localize(), "Check out from source control".Localize(), Keys.None, Resources.DocumentCheckOutImage, m_commandVisibility, this);
		}
		if ((RegisterCommands & CommandRegister.Sync) == CommandRegister.Sync)
		{
			m_commandService.RegisterCommand(Command.Sync, StandardMenu.File, SourceControlCommandGroup.OnOff, "Source Control/Get Latest Version".Localize(), "Get latest version from source control".Localize(), Keys.None, Resources.DocumentGetLatestImage, m_commandVisibility, this);
		}
		if ((RegisterCommands & CommandRegister.Revert) == CommandRegister.Revert)
		{
			m_commandService.RegisterCommand(Command.Revert, StandardMenu.File, SourceControlCommandGroup.OnOff, "Source Control/Revert".Localize("Revert add or check out from source control"), "Revert add or check out from source control".Localize(), Keys.None, Resources.DocumentRevertImage, m_commandVisibility, this);
		}
		if ((RegisterCommands & CommandRegister.Refresh) == CommandRegister.Refresh)
		{
			m_commandService.RegisterCommand(Command.Refresh, StandardMenu.File, SourceControlCommandGroup.OnOff, "Source Control/Refresh Status".Localize(), "Refresh status in source control".Localize(), Keys.None, Resources.DocumentRefreshImage, m_commandVisibility, this);
		}
		if ((RegisterCommands & CommandRegister.Reconcile) == CommandRegister.Reconcile)
		{
			m_commandService.RegisterCommand(Command.Reconcile, StandardMenu.File, SourceControlCommandGroup.OnOff, "Source Control/Reconcile Offline Work...".Localize(), "Reconcile Offline Work".Localize(), Keys.None, Resources.SourceControlReconcileImage, CommandVisibility.ApplicationMenu, this);
		}
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		if (IsUpdatingCommands)
		{
			return false;
		}
		using (new CommandUpdater(this))
		{
			if (!(commandTag is Command))
			{
				return false;
			}
			if (SourceControlService == null)
			{
				return false;
			}
			if ((Command)commandTag == Command.Enabled)
			{
				return true;
			}
			if (!SourceControlService.Enabled)
			{
				return false;
			}
			if ((Command)commandTag == Command.Connection)
			{
				return WrapCommandFunction((Command)commandTag, DoConnection, doing: false);
			}
			if (!CanDoSourceControlCommand())
			{
				return false;
			}
			if ((Command)commandTag == Command.Add)
			{
				return WrapCommandFunction((Command)commandTag, DoAdd, doing: false);
			}
			bool result = false;
			using (IEnumerator<IResource> enumerator = SourceControlContext.Resources.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					IResource current = enumerator.Current;
					switch ((Command)commandTag)
					{
					case Command.Refresh:
						result = WrapCommandFunction((Command)commandTag, DoRefresh, doing: false);
						break;
					case Command.Reconcile:
						result = WrapCommandFunction((Command)commandTag, DoReconcile, doing: false);
						break;
					case Command.CheckOut:
						result = WrapCommandFunction((Command)commandTag, DoCheckOut, doing: false);
						break;
					case Command.CheckIn:
						result = WrapCommandFunction((Command)commandTag, DoCheckIn, doing: false);
						break;
					case Command.Sync:
						result = WrapCommandFunction((Command)commandTag, DoSync, doing: false);
						break;
					case Command.Revert:
						result = WrapCommandFunction((Command)commandTag, DoRevert, doing: false);
						break;
					}
				}
			}
			return result;
		}
	}

	public virtual void DoCommand(object commandTag)
	{
		if (IsUpdatingCommands)
		{
			return;
		}
		using (new CommandUpdater(this))
		{
			if (!(commandTag is Command) || SourceControlService == null)
			{
				return;
			}
			if ((Command)commandTag == Command.Enabled)
			{
				SourceControlService.Enabled = !SourceControlService.Enabled;
			}
			else if ((Command)commandTag == Command.Connection)
			{
				WrapCommandFunction((Command)commandTag, DoConnection, doing: true);
			}
			else if (CanDoSourceControlCommand())
			{
				switch ((Command)commandTag)
				{
				case Command.Refresh:
					WrapCommandFunction((Command)commandTag, DoRefresh, doing: true);
					break;
				case Command.Reconcile:
					WrapCommandFunction((Command)commandTag, DoReconcile, doing: true);
					break;
				case Command.Add:
					WrapCommandFunction((Command)commandTag, DoAdd, doing: true);
					break;
				case Command.CheckOut:
					WrapCommandFunction((Command)commandTag, DoCheckOut, doing: true);
					break;
				case Command.CheckIn:
					WrapCommandFunction((Command)commandTag, DoCheckIn, doing: true);
					break;
				case Command.Sync:
					WrapCommandFunction((Command)commandTag, DoSync, doing: true);
					break;
				case Command.Revert:
					WrapCommandFunction((Command)commandTag, DoRevert, doing: true);
					break;
				}
			}
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
		if (commandTag is Command && (Command)commandTag == Command.Enabled && SourceControlService != null)
		{
			commandState.Text = (SourceControlService.Enabled ? "Disable Source Control".Localize() : "Enable Source Control".Localize());
		}
	}

	public virtual IEnumerable<object> GetCommands(object context, object target)
	{
		IResource resource = target.As<IResource>();
		IEnumerable<object> result;
		if (resource == null)
		{
			result = EmptyEnumerable<object>.Instance;
		}
		else
		{
			IEnumerable<object> commands = m_commands;
			result = commands;
		}
		return result;
	}

	protected virtual void OnDocumentAdded(object sender, ItemInsertedEventArgs<IDocument> e)
	{
		IDocument activeDocument = m_documentRegistry.ActiveDocument;
		if (activeDocument != null && !m_documentService.IsUntitled(activeDocument))
		{
			activeDocument.DirtyChanged += OnDocumentDirtyChanged;
			TransactionContext transactionContext = activeDocument.As<TransactionContext>();
			transactionContext.Beginning += OnDocumentDirtyChanging;
		}
	}

	protected virtual void OnDocumentSaved(object sender, DocumentEventArgs e)
	{
		if (SourceControlService == null)
		{
			return;
		}
		if (RefreshStatusOnSave)
		{
			SourceControlService.RefreshStatus(e.Document.Uri);
		}
		if (e.Kind == DocumentEventType.SavedAs && SourceControlService.GetStatus(e.Document.Uri) == SourceControlStatus.NotControlled)
		{
			string message = string.Format("Add document {0} to version control?".Localize(), e.Document.Uri.AbsolutePath);
			MessageBoxResult messageBoxResult = MessageBoxes.Show(message, "Add document to Version Control".Localize(), MessageBoxButton.YesNo, MessageBoxImage.None);
			if (messageBoxResult == MessageBoxResult.Yes)
			{
				SourceControlService.Add(e.Document.Uri);
			}
		}
	}

	protected virtual void OnDocumentDirtyChanging(object sender, EventArgs e)
	{
		IDocument document = sender.As<IDocument>();
		if (SourceControlService == null || document == null)
		{
			return;
		}
		SyncFileIfOutOfDate(document, delegate(SourceControlResultCodeEventArgs res)
		{
			if ((bool)res.SourceControlResult && CheckoutOnEditBehavior != CheckoutOnEditBehavior.Never)
			{
				FileInfo fileInfo = new FileInfo(document.Uri.LocalPath);
				if (fileInfo.IsReadOnly && SourceControlService.GetStatus(document.Uri) == SourceControlStatus.CheckedIn)
				{
					if (CheckoutOnEditBehavior == CheckoutOnEditBehavior.Always)
					{
						TestCheckedIn(sender);
					}
					else
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.AppendLine("Check out this file to be able to save the changes?".Localize());
						stringBuilder.AppendLine(document.Uri.LocalPath);
						MessageBoxResult messageBoxResult = MessageBoxes.Show(stringBuilder.ToString(), "Check Out File".Localize(), MessageBoxButton.YesNo, MessageBoxImage.None);
						if (messageBoxResult == MessageBoxResult.Yes)
						{
							TestCheckedIn(sender);
						}
					}
				}
			}
		});
	}

	protected virtual void OnDocumentDirtyChanged(object sender, EventArgs e)
	{
		IDocument document = sender as IDocument;
		if (SourceControlService == null || document == null || !document.Dirty || CheckoutOnEditBehavior == CheckoutOnEditBehavior.Never || SourceControlService.GetStatus(document.Uri) != SourceControlStatus.CheckedIn)
		{
			return;
		}
		if (CheckoutOnEditBehavior == CheckoutOnEditBehavior.Always)
		{
			TestCheckedIn(sender);
			return;
		}
		string message = string.Format("Check out the file\r\n\r\n{0}\r\n\r\nto be able to save the changes?".Localize(), document.Uri.LocalPath);
		MessageBoxResult messageBoxResult = MessageBoxes.Show(message, "Check Out File".Localize("this is the title of a dialog box that is asking a question"), MessageBoxButton.YesNo, MessageBoxImage.Question);
		if (messageBoxResult == MessageBoxResult.Yes)
		{
			TestCheckedIn(sender);
		}
	}

	protected virtual bool DoRefresh(bool doing)
	{
		if (SourceControlService == null || SourceControlContext == null)
		{
			return false;
		}
		if (!doing)
		{
			using (IEnumerator<IResource> enumerator = SourceControlContext.Resources.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					IResource current = enumerator.Current;
					return true;
				}
			}
			return false;
		}
		List<Uri> list = new List<Uri>();
		foreach (IResource resource in SourceControlContext.Resources)
		{
			GetSourceControlledUri(resource, list);
		}
		SourceControlService.RefreshStatus(list);
		return list.Count > 0;
	}

	protected virtual bool DoReconcile(bool doing)
	{
		return false;
	}

	protected static void GetSourceControlledUri(object path, List<Uri> uris)
	{
		IResource resource = path.As<IResource>();
		if (resource != null)
		{
			uris.Add(resource.Uri);
		}
	}

	protected virtual bool DoAdd(bool doing)
	{
		if (SourceControlService == null || SourceControlContext == null)
		{
			return false;
		}
		int num = 0;
		foreach (IResource resource in SourceControlContext.Resources)
		{
			SourceControlStatus status = GetStatus(resource);
			if (status != SourceControlStatus.NotControlled && status != SourceControlStatus.FileDoesNotExist)
			{
				return false;
			}
			num++;
			if (doing)
			{
				SourceControlService.Add(resource.Uri);
			}
		}
		return num != 0;
	}

	protected virtual bool DoCheckOut(bool doing)
	{
		if (!CanDoSourceControlCommand())
		{
			return false;
		}
		int checkedOutCount = 0;
		Task<bool> task = Task.Factory.StartNew(delegate
		{
			foreach (IResource resource in SourceControlContext.Resources)
			{
				if (GetStatus(resource) != SourceControlStatus.CheckedIn)
				{
					return false;
				}
				if (SourceControlService.IsLocked(resource.Uri))
				{
					return false;
				}
				checkedOutCount++;
				if (doing)
				{
					EventHandler<SourceControlResultCodeEventArgs> callback = null;
					callback = delegate(object sender, SourceControlResultCodeEventArgs eventArgs)
					{
						SourceControlService.OperationCompleted -= callback;
						bool flag = eventArgs.SourceControlResult;
						if (flag)
						{
							flag = GetStatus(resource) == SourceControlStatus.CheckedOut;
						}
						if (!flag)
						{
							int num = checkedOutCount;
							checkedOutCount = num - 1;
							DisplaySourceControlError(eventArgs);
						}
					};
					SourceControlService.OperationCompleted += callback;
					SourceControlService.CheckOut(resource.Uri);
				}
			}
			return checkedOutCount != 0;
		});
		task.Wait(Timeout);
		return task.Result;
	}

	protected virtual bool DoCheckIn(bool doing)
	{
		if (SourceControlService == null || ContextRegistry == null || SourceControlContext == null || !SourceControlService.AllowCheckIn)
		{
			return false;
		}
		bool result = false;
		List<IResource> list = new List<IResource>();
		foreach (IResource resource in SourceControlContext.Resources)
		{
			SourceControlStatus status = GetStatus(resource);
			if (status == SourceControlStatus.CheckedOut || status == SourceControlStatus.Added || status == SourceControlStatus.Deleted)
			{
				result = true;
				if (!doing)
				{
					break;
				}
				list.Add(resource);
			}
		}
		if (doing)
		{
			if (m_documentService != null)
			{
				foreach (IResource resource2 in SourceControlContext.Resources)
				{
					IDocument document = resource2.As<IDocument>();
					if (document != null && document.Dirty)
					{
						m_documentService.Save(document);
					}
				}
			}
			ShowCheckInDialog(list);
		}
		return result;
	}

	protected virtual void ShowCheckInDialog(IList<IResource> toCheckIns)
	{
	}

	protected virtual bool DoSync(bool doing)
	{
		if (!CanDoSourceControlCommand())
		{
			return false;
		}
		int syncCount = 0;
		Task<bool> task = Task.Factory.StartNew(delegate
		{
			foreach (IResource resource in SourceControlContext.Resources)
			{
				if (GetStatus(resource) != SourceControlStatus.CheckedIn)
				{
					return false;
				}
				if (SourceControlService.IsLocked(resource.Uri))
				{
					return false;
				}
				syncCount++;
				if (doing)
				{
					EventHandler<SourceControlResultCodeEventArgs> callback = null;
					callback = delegate(object sender, SourceControlResultCodeEventArgs eventArgs)
					{
						SourceControlService.OperationCompleted -= callback;
						bool flag = eventArgs.SourceControlResult;
						while (flag)
						{
							foreach (Uri uri in eventArgs.SourceControlResult.Uris)
							{
								flag = IsFileSynced(uri);
							}
						}
						if (flag)
						{
							Reload(resource);
						}
						else
						{
							int num = syncCount;
							syncCount = num - 1;
							DisplaySourceControlError(eventArgs);
						}
					};
					SourceControlService.OperationCompleted += callback;
					SourceControlService.GetLatestVersion(resource.Uri);
				}
			}
			return syncCount != 0;
		});
		task.Wait(Timeout);
		return task.Result;
	}

	private bool IsFileSynced(Uri fileUri)
	{
		bool flag = SourceControlService.IsSynched(fileUri);
		if (!flag)
		{
			SourceControlFileInfo fileInfo = SourceControlService.GetFileInfo(fileUri);
			flag = fileInfo.HeadStatus == SourceControlStatus.Deleted;
		}
		return flag;
	}

	protected virtual bool DoRevert(bool doing)
	{
		if (SourceControlService == null || SourceControlContext == null)
		{
			return false;
		}
		foreach (IResource resource2 in SourceControlContext.Resources)
		{
			SourceControlStatus status = GetStatus(resource2);
			if (status != SourceControlStatus.CheckedOut && status != SourceControlStatus.Added && status != SourceControlStatus.Deleted)
			{
				return false;
			}
		}
		if (doing)
		{
			MessageBoxResult messageBoxResult = MessageBoxes.Show("All Changes will be lost. Do you want to proceed?".Localize(), "Proceed with Revert?".Localize(), MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (messageBoxResult == MessageBoxResult.Yes)
			{
				IResource[] array = SourceControlContext.Resources.ToArray();
				foreach (IResource resource in array)
				{
					SourceControlStatus status2 = GetStatus(resource);
					if (status2 == SourceControlStatus.CheckedOut || status2 == SourceControlStatus.Added || status2 == SourceControlStatus.Deleted)
					{
						SourceControlService.Revert(resource.Uri);
						Reload(resource);
					}
				}
			}
		}
		return true;
	}

	protected virtual bool DoConnection(bool doing)
	{
		if (SourceControlService == null)
		{
			return false;
		}
		return doing ? SourceControlService.Connect() : SourceControlService.CanConfigure;
	}

	protected virtual SourceControlStatus GetStatus(IResource resource)
	{
		Uri uri = resource.Uri;
		if (uri != null)
		{
			return SourceControlService.GetStatus(new Uri(uri.LocalPath));
		}
		return SourceControlStatus.FileDoesNotExist;
	}

	protected virtual void Reload(IResource resource)
	{
		if (!(resource is IDocument))
		{
			return;
		}
		IDocumentClient documentClient = GetDocumentClient(resource.Uri.LocalPath);
		if (documentClient != null)
		{
			IDocument document2 = m_documentRegistry.Documents.FirstOrDefault((IDocument d) => d.Uri == resource.Uri);
			if (document2 != null)
			{
				documentClient.Reload(document2);
			}
		}
	}

	private IDocumentClient GetDocumentClient(string pathName)
	{
		IEnumerable<IDocumentClient> clients = m_documentClients.Select((Lazy<IDocumentClient> lazy) => lazy.Value);
		return clients.GetFirstClientForPath(pathName);
	}

	protected void TestCheckedIn(object obj)
	{
		if (SourceControlService == null)
		{
			return;
		}
		IResource resource = obj.As<IResource>();
		if (resource == null)
		{
			return;
		}
		CheckOutIfNeeded(resource.Uri, delegate(SourceControlResultCodeEventArgs args)
		{
			bool flag = args.SourceControlResult;
			if (flag)
			{
				flag = GetStatus(resource) == SourceControlStatus.CheckedOut;
			}
			if (!flag)
			{
				DisplaySourceControlError(args);
			}
		});
	}

	private bool WrapCommandFunction(Command command, CommandFunction function, bool doing)
	{
		try
		{
			m_currentCommnd = command;
			return function(doing);
		}
		finally
		{
			m_currentCommnd = Command.Invalid;
		}
	}

	protected virtual bool ShouldTrackChanges(IDocument doc)
	{
		if (doc == null)
		{
			return false;
		}
		if (m_documentService.IsUntitled(doc))
		{
			return false;
		}
		if (doc.IsReadOnly)
		{
			return false;
		}
		return true;
	}

	protected virtual bool ShouldCheckForAdd(DocumentEventType kind)
	{
		if (kind != DocumentEventType.SavedAs)
		{
			return false;
		}
		if (SourceControlService == null)
		{
			return false;
		}
		return true;
	}

	protected void SyncFileIfOutOfDate(IDocument document, Action<SourceControlResultCodeEventArgs> completionAction)
	{
		Uri uri = document.Uri;
		if (IsFileSynced(uri))
		{
			return;
		}
		bool flag = true;
		if (SourceControlService.AllowOutOfDateEdit)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("This file is out of date.  Sync this file in Perforce?".Localize());
			stringBuilder.AppendLine(uri.LocalPath);
			MessageBoxResult messageBoxResult = MessageBoxes.Show(stringBuilder.ToString(), "Sync File".Localize(), MessageBoxButton.YesNo, MessageBoxImage.None);
			flag = messageBoxResult == MessageBoxResult.Yes;
			if (!flag)
			{
				completionAction(new SourceControlResultCodeEventArgs(new SourceControlResultCode(uri, "Sync canceled by user")));
				return;
			}
		}
		if (!flag)
		{
			return;
		}
		if (m_fileWatcherService != null)
		{
			m_fileWatcherService.Suspend(uri.LocalPath);
		}
		EventHandler<SourceControlResultCodeEventArgs> callback = null;
		callback = delegate(object sender, SourceControlResultCodeEventArgs eventArgs)
		{
			bool flag2 = eventArgs.SourceControlResult;
			int num = 50;
			while (!flag2 && num > 0)
			{
				num--;
				foreach (Uri uri2 in eventArgs.SourceControlResult.Uris)
				{
					flag2 = IsFileSynced(uri2);
				}
			}
			if (flag2)
			{
				Reload(document);
			}
			else
			{
				DisplaySourceControlError(eventArgs);
			}
			if (m_fileWatcherService != null)
			{
				foreach (Uri uri3 in eventArgs.SourceControlResult.Uris)
				{
					m_fileWatcherService.Unsuspend(uri3.LocalPath);
				}
			}
			SourceControlService.OperationCompleted -= callback;
			completionAction(eventArgs);
		};
		SourceControlService.OperationCompleted += callback;
		SourceControlService.GetLatestVersion(uri);
	}

	protected void DisplaySourceControlError(SourceControlResultCodeEventArgs e)
	{
		string message = $"{e.SourceControlResult.Message}\n\n{e.SourceControlResult.ResultInformation}";
		MessageBoxes.Show(message, "Source control operation failed", MessageBoxButton.OK, MessageBoxImage.Error);
	}

	protected void CheckOutIfNeeded(Uri uri, Action<SourceControlResultCodeEventArgs> completionAction)
	{
		if (SourceControlService.GetStatus(uri) == SourceControlStatus.CheckedIn)
		{
			EventHandler<SourceControlResultCodeEventArgs> callback = null;
			callback = delegate(object sender, SourceControlResultCodeEventArgs eventArgs)
			{
				SourceControlService.OperationCompleted -= callback;
				completionAction(eventArgs);
			};
			SourceControlService.OperationCompleted += callback;
			SourceControlService.CheckOut(uri);
		}
	}

	private bool CanDoSourceControlCommand()
	{
		return SourceControlService != null && SourceControlContext != null && SourceControlService.IsConnected;
	}
}
