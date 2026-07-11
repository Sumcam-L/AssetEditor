using System;
using System.ComponentModel.Composition;
using System.IO;
using Firaxis.CivTech;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[Export(typeof(VersionControlDocuments))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class VersionControlDocuments : IInitializable
{
	private ICivTechService m_civTechService;

	private IDocumentRegistry m_documentRegistry;

	private IDocumentService m_documentService;

	private IFileWatcherService m_fileWatchService;

	private IDocumentRegistry m_shadowDocumentRegistry;

	[Import(AllowDefault = true)]
	private IShadowDocumentRegistryProvider m_shadowDocumentRegistryProvider;

	public ICivTechService CivTechService => m_civTechService;

	public IDocumentRegistry DocumentRegistry => m_documentRegistry;

	public IDocumentRegistry ShadowDocumentRegistry => m_shadowDocumentRegistry;

	private IDocumentService DocumentService => m_documentService;

	private IFileWatcherService FileWatchService => m_fileWatchService;

	private IContextRegistry ContextRegistry { get; set; }

	[ImportingConstructor]
	public VersionControlDocuments(IDocumentRegistry documentRegistry, IDocumentService documentService, IContextRegistry contextReg, ICivTechService civTechSvc, IFileWatcherService fileWatchSvc)
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			ContextRegistry = contextReg;
			ContextRegistry.ActiveContextChanging += ContextRegistry_ActiveContextChanging;
			ContextRegistry.ActiveContextChanged += ContextRegistry_ActiveContextChanged;
			m_documentRegistry = documentRegistry;
			m_documentRegistry.UriChanged += DocumentRegistry_UriChanged;
			m_documentRegistry.DocumentAdded += DocumentRegistry_DocumentAdded;
			m_documentRegistry.DocumentRemoved += DocumentRegistry_DocumentRemoved;
			m_civTechService = civTechSvc;
			m_fileWatchService = fileWatchSvc;
			m_documentService = documentService;
			m_documentService.DocumentSaving += DocumentService_DocumentSaving;
			m_documentService.DocumentSaved += DocumentService_DocumentSaved;
		}
	}

	private void DocumentRegistry_DocumentAdded(object sender, ItemInsertedEventArgs<IDocument> e)
	{
		IInstanceEntityAdapter instanceEntityAdapter = e.Item.As<IInstanceEntityAdapter>();
		if (instanceEntityAdapter != null)
		{
			instanceEntityAdapter.DataFilesChanging -= EntityDocument_DataFilesChanging;
			instanceEntityAdapter.DataFilesChanging += EntityDocument_DataFilesChanging;
			instanceEntityAdapter.DataFilesChanged -= EntityAdapter_DataFilesChanged;
			instanceEntityAdapter.DataFilesChanged += EntityAdapter_DataFilesChanged;
		}
	}

	private void DocumentRegistry_DocumentRemoved(object sender, ItemRemovedEventArgs<IDocument> e)
	{
		IInstanceEntityAdapter instanceEntityAdapter = e.Item.As<IInstanceEntityAdapter>();
		if (instanceEntityAdapter != null)
		{
			instanceEntityAdapter.DataFilesChanging -= EntityDocument_DataFilesChanging;
			instanceEntityAdapter.DataFilesChanged -= EntityAdapter_DataFilesChanged;
		}
	}

	private void ContextRegistry_ActiveContextChanged(object sender, EventArgs e)
	{
		TransactionContext transactionContext = ContextRegistry.ActiveContext.As<TransactionContext>();
		if (transactionContext != null)
		{
			transactionContext.Beginning -= TransactionContext_Beginning;
			transactionContext.Beginning += TransactionContext_Beginning;
		}
	}

	private void ContextRegistry_ActiveContextChanging(object sender, EventArgs e)
	{
		TransactionContext transactionContext = ContextRegistry.ActiveContext.As<TransactionContext>();
		if (transactionContext != null)
		{
			transactionContext.Beginning -= TransactionContext_Beginning;
		}
	}

	public void Initialize()
	{
		if (m_shadowDocumentRegistryProvider != null)
		{
			m_shadowDocumentRegistry = m_shadowDocumentRegistryProvider.DocumentRegistry;
		}
	}

	private void EntityAdapter_DataFilesChanged(object sender, DataFilesEventArgs e)
	{
		foreach (DataFileInfo dataFileInfo in e.DataFileInfos)
		{
			string projectName = m_civTechService.GetProjectName(dataFileInfo.FullPath);
			_ = m_civTechService.PrimaryProject.Name;
			if (!m_civTechService.AllProjectsMap.ContainsProject(projectName))
			{
				break;
			}
			IVersionControlService versionControl = m_civTechService.AllProjectsMap[projectName].VersionControl;
			string errMsg = string.Empty;
			if (!versionControl.AddFile(dataFileInfo.FullPath.LocalPath, out errMsg))
			{
				MessageBoxes.Show("Failed to open " + dataFileInfo.FullPath.LocalPath + " for add in version control.\nYou will have to manually add this file to use P4V!\n\n" + errMsg, "Failed to open for add in version control".Localize(), MessageBoxButton.OK, MessageBoxImage.None);
			}
		}
	}

	private void EntityDocument_DataFilesChanging(object sender, DataFilesEventArgs e)
	{
		foreach (DataFileInfo dataFileInfo in e.DataFileInfos)
		{
			string projectName = m_civTechService.GetProjectName(dataFileInfo.FullPath);
			_ = m_civTechService.PrimaryProject.Name;
			if (!m_civTechService.AllProjectsMap.ContainsProject(projectName))
			{
				break;
			}
			IVersionControlService versionControl = m_civTechService.AllProjectsMap[projectName].VersionControl;
			string errMsg = string.Empty;
			if (versionControl.IsMarkedForAdd(dataFileInfo.FullPath.LocalPath))
			{
				if (!versionControl.RevertFile(dataFileInfo.FullPath.LocalPath, out errMsg))
				{
					MessageBoxes.Show("Failed to revert open for add status of " + dataFileInfo.FullPath.LocalPath + " in version control.\nYou will have to P4V to fix your environment!\n\n" + errMsg, "Failed to revert open for add in version control".Localize(), MessageBoxButton.OK, MessageBoxImage.None);
					break;
				}
			}
			else if (versionControl.IsVersionControlled(dataFileInfo.FullPath.LocalPath) && !versionControl.EditFile(dataFileInfo.FullPath.LocalPath, out errMsg))
			{
				MessageBoxes.Show("Failed to open " + dataFileInfo.FullPath.LocalPath + " for edit in version control.\nYou will have to manually edit this file in P4V!\n\n" + errMsg, "Failed to open for edit in version control".Localize(), MessageBoxButton.OK, MessageBoxImage.None);
			}
		}
	}

	private void DocumentRegistry_UriChanged(object sender, UriChangedEventArgs e)
	{
		string projectName = m_civTechService.GetProjectName(e.OldUri);
		string projectName2 = m_civTechService.GetProjectName(e.NewUri);
		_ = m_civTechService.PrimaryProject.Name;
		if (m_civTechService.AllProjectsMap.ContainsProject(projectName) || m_civTechService.AllProjectsMap.ContainsProject(projectName2))
		{
			IVersionControlService versionControl = m_civTechService.AllProjectsMap[projectName].VersionControl;
			IVersionControlService versionControl2 = m_civTechService.AllProjectsMap[projectName2].VersionControl;
			string errMsg = string.Empty;
			if (versionControl.IsMarkedForAdd(e.OldUri.LocalPath) && !versionControl.RevertFile(e.OldUri.LocalPath, out errMsg))
			{
				MessageBoxes.Show("Failed to revert open for add status of " + e.OldUri.LocalPath + " in version control.\nYou will have to use P4V to fix your environment!\n\n" + errMsg, "Failed to revert open for add in version control".Localize(), MessageBoxButton.OK, MessageBoxImage.None);
			}
			else if (!versionControl2.AddFile(e.NewUri.LocalPath, out errMsg))
			{
				MessageBoxes.Show("Failed to open " + e.NewUri.LocalPath + " for add in version control.\nYou will have to manually add this file to P4V!\n\n" + errMsg, "Failed to open for add in version control".Localize(), MessageBoxButton.OK, MessageBoxImage.None);
			}
		}
	}

	private bool IsAlreadyEditable(IDocument document)
	{
		if (File.Exists(document.Uri.LocalPath))
		{
			return !File.GetAttributes(document.Uri.LocalPath).HasFlag(FileAttributes.ReadOnly);
		}
		return false;
	}

	private bool IsFromActiveProjectStack(ICivTechService civTechSvc, IDocument document)
	{
		string projectName = civTechSvc.GetProjectName(document.Uri);
		string name = civTechSvc.PrimaryProject.Name;
		if (!civTechSvc.ActiveProjectMap.ContainsProject(projectName) && !FileWatchService.UseSinglePantryMode)
		{
			BugSubmitter.SilentReport("Attempted to modify document \"" + document.Uri.LocalPath + "\" from project \"" + projectName + "\" that is not part of the active project \"" + name + "\" dependency tree @summary Attempted to modify a document outside the active project's dependency tree @assign bwhitman");
			return false;
		}
		return true;
	}

	private bool IsFromActiveProject(ICivTechService civTechSvc, IDocument document)
	{
		return civTechSvc.IsFromActiveProject(document.Uri);
	}

	private bool UserWantsToEditDependentProjectDocument(ICivTechService civTechSvc, IDocument document)
	{
		string projectName = civTechSvc.GetProjectName(document.Uri);
		if (!civTechSvc.ActiveProjectMap[projectName].VersionControl.IsEditible(document.Uri.LocalPath) && !FileWatchService.UseSinglePantryMode && MessageBoxResult.No == MessageBoxes.Show("Modifying a document from project \"" + projectName + "\" but the active project is \"" + civTechSvc.PrimaryProject.Name + "\"\n\nAre you sure?", "Modifying " + civTechSvc.PrimaryProject.Name + " document", MessageBoxButton.YesNo, MessageBoxImage.Exclamation))
		{
			return false;
		}
		return true;
	}

	private void PerformCheckout(ICivTechService civTechSvc, IDocument document)
	{
		string localPath = document.Uri.LocalPath;
		string projectName = civTechSvc.GetProjectName(document.Uri);
		IVersionControlService versionControl = civTechSvc.AllProjectsMap[projectName].VersionControl;
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
				MessageBoxes.Show("Failed to open " + localPath + " for add in version control.\nYou will be unable to save this file!\n\n" + errMsg, "Failed to open for add in version control".Localize(), MessageBoxButton.OK, MessageBoxImage.None);
			}
		}
		else if (!versionControl.IsEditible(localPath))
		{
			if (!versionControl.GetLatest(localPath, out errMsg))
			{
				MessageBoxes.Show("Failed to get latest " + document.Uri.LocalPath + " from version control.\nModifying this file in this state will remove someone's work!\n\nThe file will not be open for edit!\n\n" + errMsg, "Failed to get latest from version control".Localize(), MessageBoxButton.OK, MessageBoxImage.None);
			}
			else if (!versionControl.EditFile(localPath, out errMsg))
			{
				MessageBoxes.Show("Failed to open " + localPath + " for edit in version control.\nYou will be unable to save this file!\n\n" + errMsg, "Failed to open for edit in version control".Localize(), MessageBoxButton.OK, MessageBoxImage.None);
			}
		}
	}

	private bool IsProjectSpecificDocument(IDocument document)
	{
		return document.Is<IProjectSpecificDocument>();
	}

	private void TransactionContext_Beginning(object sender, EventArgs e)
	{
		IDocument document = sender.As<IDocument>();
		if (document == null || IsAlreadyEditable(document))
		{
			return;
		}
		if (IsProjectSpecificDocument(document))
		{
			if (!IsFromActiveProjectStack(m_civTechService, document))
			{
				throw new InvalidTransactionException("Attempted to modify a document outside the active project's dependency tree", reportError: true);
			}
			if (!IsFromActiveProject(m_civTechService, document) && !UserWantsToEditDependentProjectDocument(m_civTechService, document))
			{
				throw new InvalidTransactionException("Canceled edit at user's request", reportError: false);
			}
		}
		PerformCheckout(m_civTechService, document);
	}

	private void DocumentService_DocumentSaving(object sender, DocumentEventArgs e)
	{
		if (e.Kind != DocumentEventType.SavedAs)
		{
			return;
		}
		IDocument document = e.Document;
		if (document == null)
		{
			return;
		}
		string localPath = document.Uri.LocalPath;
		string projectName = m_civTechService.GetProjectName(document.Uri);
		_ = m_civTechService.PrimaryProject.Name;
		if (!m_civTechService.AllProjectsMap.ContainsProject(projectName))
		{
			return;
		}
		IVersionControlService versionControl = m_civTechService.AllProjectsMap[projectName].VersionControl;
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
				MessageBoxes.Show("Failed to open " + localPath + " for add in version control.\nYou will be unable to save this file!\n\n" + errMsg, "Failed to open for add in version control".Localize(), MessageBoxButton.OK, MessageBoxImage.None);
			}
		}
		else if (!versionControl.IsEditible(localPath))
		{
			if (!versionControl.GetLatest(localPath, out errMsg))
			{
				MessageBoxes.Show("Failed to get latest " + document.Uri.LocalPath + " from version control.\nModifying this file in this state will remove someone's work!\n\nThe file will not be open for edit!\n\n" + errMsg, "Failed to get latest from version control".Localize(), MessageBoxButton.OK, MessageBoxImage.None);
			}
			else if (!versionControl.EditFile(localPath, out errMsg))
			{
				MessageBoxes.Show("Failed to open " + localPath + " for edit in version control.\nYou will be unable to save this file!\n\n" + errMsg, "Failed to open for edit in version control".Localize(), MessageBoxButton.OK, MessageBoxImage.None);
			}
		}
	}

	protected virtual void DocumentService_DocumentSaved(object sender, DocumentEventArgs e)
	{
		if (e.Kind == DocumentEventType.SavedAs)
		{
			IVersionControlService versionControl = m_civTechService.PrimaryProject.VersionControl;
			string localPath = e.Document.Uri.LocalPath;
			if ((!versionControl.IsVersionControlled(localPath) || versionControl.IsMarkedForDelete(localPath)) && !versionControl.AddFile(localPath, out var errMsg))
			{
				MessageBoxes.Show("Failed to add " + localPath + " to version control.\n\n" + errMsg, "Failed to add to version control".Localize(), MessageBoxButton.OK, MessageBoxImage.None);
			}
		}
	}
}
