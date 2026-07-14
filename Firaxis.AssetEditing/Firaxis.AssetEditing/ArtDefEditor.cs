using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IDocumentClient))]
[Export(typeof(ArtDefEditor))]
[Export(typeof(IProjectChangeWatcher))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ArtDefEditor : IDocumentClient, IControlHostClient, IInitializable, IDisposable, IProjectChangeWatcher
{
	private readonly IContextRegistry m_contextRegistry;

	private readonly ArtDefSchemaLoader m_artDefSchemaLoader;

	private readonly ICivTechService m_civTechService;

	private readonly IVersionService m_versionService;

	private readonly IXLPRegistry m_xlpRegistry;

	private readonly IArtDefRegistry m_artDefRegistry;

	[Import(AllowDefault = true)]
	private ICookService m_cookService;

	[Import(AllowDefault = true)]
	private IControlHostService m_controlHostService;

	[Import(AllowDefault = true)]
	private ICommandService m_commandService;

	[Import(AllowDefault = true)]
	private ScriptingService m_scriptingService;

	[Import(AllowDefault = true)]
	private DomExplorer m_domExplorer;

	[ImportMany]
	private Lazy<IDocumentClient>[] m_lazyDocumentClients;

	private IEnumerable<IDocumentClient> m_documentClients;

	public static DocumentClientInfo DocumentClientInfo = new DocumentClientInfoEx("ArtDef".Localize(), ".artdef", Resources.NewFileIcon, Resources.OpenArtDefIcon, multiDocument: true, isHidden: false, "Other");

	[Import(AllowDefault = true)]
	private ITunerQueueService TunerQueueService { get; set; }

	[Import(AllowDefault = true)]
	private IDocumentService DocumentService { get; set; }

	[Import(AllowDefault = true)]
	private IDocumentRegistry DocumentRegistry { get; set; }

	[Import(AllowDefault = true)]
	private ArtDefCommands ArtDefCommands { get; set; }

	[Import(AllowDefault = true)]
	private IFileDialogService FileDialogService { get; set; }

	public bool AskWhenClosingDirtyDocument => true;

	public DocumentClientInfo Info => DocumentClientInfo;

	[ImportingConstructor]
	public ArtDefEditor(IContextRegistry contextRegistry, ArtDefSchemaLoader artDefSchemaLoader, ICivTechService civTechSvc, IVersionService versionService, IXLPRegistry xlpRegistry, IArtDefRegistry artDefRegistry)
	{
		m_contextRegistry = contextRegistry;
		m_artDefSchemaLoader = artDefSchemaLoader;
		m_civTechService = civTechSvc;
		m_versionService = versionService;
		m_xlpRegistry = xlpRegistry;
		m_artDefRegistry = artDefRegistry;
	}

	private void ArtDefCommands_ProjectConfigChanged(object sender, EventArgs e)
	{
		IDocument[] array = DocumentRegistry.Documents.ToArray();
		foreach (IDocument document in array)
		{
			if (document is ArtDefDocument artDefDocument)
			{
				ArtDefSetAdapter artDefSetAdapter = artDefDocument.As<ArtDefSetAdapter>();
				if (!artDefDocument.ValidateTemplate(artDefSetAdapter.ArtDef.ArtDefTemplate) || !artDefDocument.MigrateTemplate())
				{
					Close(document);
				}
			}
		}
	}

	public virtual void HandleProjectChange(Action<string> statusMessagePrinter)
	{
		Info.InitialDirectory = m_civTechService.PrimaryProject.Paths.ArtDefRoot;
	}

	void IInitializable.Initialize()
	{
		Info.InitialDirectory = m_civTechService.PrimaryProject.Paths.ArtDefRoot;
		if (DocumentRegistry != null)
		{
			DocumentRegistry.ActiveDocumentChanged += DocumentRegistry_ActiveDocumentChanged;
		}
		if (ArtDefCommands != null)
		{
			ArtDefCommands.ProjectConfigChanged += ArtDefCommands_ProjectConfigChanged;
		}
		if (m_scriptingService != null)
		{
			m_scriptingService.LoadAssembly(GetType().Assembly);
			m_scriptingService.SetVariable("artDefEditor", this);
			m_contextRegistry.ActiveContextChanged += delegate
			{
				EditingContext activeContext = m_contextRegistry.GetActiveContext<EditingContext>();
				IHistoryContext activeContext2 = m_contextRegistry.GetActiveContext<IHistoryContext>();
				m_scriptingService.SetVariable("editingContext", activeContext);
				m_scriptingService.SetVariable("hist", activeContext2);
			};
		}
		if (m_lazyDocumentClients != null)
		{
			List<IDocumentClient> list = new List<IDocumentClient>();
			Lazy<IDocumentClient>[] lazyDocumentClients = m_lazyDocumentClients;
			foreach (Lazy<IDocumentClient> lazy in lazyDocumentClients)
			{
				list.Add(lazy.Value);
			}
			m_documentClients = list;
		}
	}

	private void UpdateLibraryNames(IArtDef artDef)
	{
		Action<IValue> act = delegate(IValue value)
		{
			if (value is IBLPEntryValue iBLPEntryValue)
			{
				iBLPEntryValue.LibraryName = iBLPEntryValue.XLPClass;
			}
		};
		artDef.VisitAllValues(act);
	}

	public bool CanOpen(Uri uri)
	{
		return DocumentClientInfo.IsCompatibleUri(uri);
	}

	public IDocument Open(Uri uri)
	{
		Outputs.WriteLine(OutputMessageType.Info, "Opening art def file \"{0}\"", uri.LocalPath);
		string localPath = uri.LocalPath;
		string fileName = Path.GetFileName(localPath);
		IArtDef artDef;
		if (File.Exists(localPath))
		{
			artDef = global::DatabaseWrapper.DatabaseWrapper.LoadArtDef(uri, m_civTechService.PrimaryProject.Config);
		}
		else
		{
			artDef = Context.EnsureCreated<CivTechContext>().CreateInstance<IArtDef>(new object[1] { m_civTechService.PrimaryProject.Config });
			IArtDefTemplate artDefTemplate = m_civTechService.PrimaryProject.Config.ArtDefTemplates.Items.First();
			artDef.UpdateRootCollectionsFromTemplate(artDefTemplate);
			artDef.ArtDefTemplate = artDefTemplate.Name;
		}
		if (artDef == null)
		{
			return null;
		}
		localPath.Replace(m_civTechService.PrimaryProject.Paths.ArtDefRoot, "").TrimStart(Path.DirectorySeparatorChar);
		ArtDefDocument document = null;
		DomNode domNode = new DomNode(ArtDefSchema.ArtDefType.Type);
		domNode.InitializeExtensions();
		ArtDefContext artDefContext = domNode.As<ArtDefContext>();
		artDefContext.DocumentRegistry = DocumentRegistry;
		artDefContext.CommandService = m_commandService;
		artDefContext.ControlHostService = m_controlHostService;
		artDefContext.ContextRegistry = m_contextRegistry;
		artDefContext.Commands = ArtDefCommands;
		ControlInfo controlInfo = new ControlInfo(fileName + "(" + m_civTechService.GetProjectName(uri) + ")", localPath, StandardControlGroup.Center, ResourceUtil.GetIcon(Resources.ArtDefFileIcon), null);
		controlInfo.IsDocument = true;
		artDefContext.ControlInfo = controlInfo;
		document = domNode.As<ArtDefDocument>();
		document.Uri = uri;
		document.VersionService = m_versionService;
		document.CookService = m_cookService;
		document.CivTechService = m_civTechService;
		document.FileDialogService = FileDialogService;
		document.FileCommands = DocumentService;
		document.DocumentClients = m_documentClients;
		artDefContext.Doc = document;
		document.RegisterForDirtyNotifications(artDefContext);
		if (string.IsNullOrEmpty(artDef.ArtDefTemplate))
		{
			return null;
		}
		if (!document.ValidateTemplate(artDef.ArtDefTemplate))
		{
			return null;
		}
		IArtDefTemplate artDefTemplate2 = ArtDefContext.FindArtDefTemplate(m_civTechService.PrimaryProject.Config, artDef.ArtDefTemplate);
		artDef.UpdateBLPReferences(artDefTemplate2);
		artDef.UpdateAppendMergedParameterCollection(artDefTemplate2);
		UpdateLibraryNames(artDef);
		ArtDefSetAdapter artDefSetAdapter = domNode.As<ArtDefSetAdapter>();
		artDefSetAdapter.Update(artDef, artDefTemplate2, initialUpdate: true);
		controlInfo.IsDirtyDocument = () => document.Dirty;
		controlInfo.IsReadOnlyDocument = () => document.IsReadOnly;
		document.UpdateControlInfo();
		if (m_controlHostService != null)
		{
			artDefContext.GUI = new ArtDefSetTreeLister(m_commandService, document, artDefContext.Commands, document.FileDialogService, document.CivTechService);
			artDefContext.GUI.MainControl.Tag = document;
			artDefContext.GUI.MainControl.TemplateReadOnly = document.IsReadOnly;
			artDefContext.GUI.MainControl.Bind(artDefSetAdapter);
			m_controlHostService?.RegisterControl(artDefContext.GUI.MainControl, controlInfo, this);
		}
		if (!document.IsReadOnly && !string.IsNullOrEmpty(artDef.ArtDefTemplate) && !document.MigrateTemplate())
		{
			return null;
		}
		Outputs.WriteLine(OutputMessageType.Info, "Opened art def file \"{0}\"", uri.LocalPath);
		if (!m_versionService.IsLocalBuild() && artDef.Version > m_versionService.ApplicationVersion)
		{
			MessageBoxes.Show("The file " + localPath + " was saved with newer tools. You will be unable to make changes to the file.\n\nPlease update your content tools.", "Error Opening File", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		return document;
	}

	public void Show(IDocument document)
	{
		ArtDefContext artDefContext = document.As<ArtDefContext>();
		if (!artDefContext.GUI.MainControl.IsDisposed)
		{
			m_controlHostService?.Show(artDefContext.GUI.MainControl);
			return;
		}
		BugSubmitter.SilentAssert(false, "Attempting to show a disposed control for {0} @summary Showing disposed control @assign nicholai.wojtowycz", document.Uri);
	}

	public bool Save(IDocument document, Uri uri)
	{
		IArtDef artDef = ((ArtDefDocument)document).As<ArtDefSetAdapter>().ArtDef;
		artDef.RemoveDuplicateEntries();
		Version applicationVersion = m_versionService.ApplicationVersion;
		artDef.SetVersion(applicationVersion.Major, applicationVersion.Minor, applicationVersion.Build, applicationVersion.Revision);
		bool num = artDef.SerializeIntoFile(uri.LocalPath);
		ArtDefContext artDefContext = document.As<ArtDefContext>();
		if (artDefContext.GUI != null)
		{
			artDefContext.GUI.MainControl.TemplateReadOnly = document.IsReadOnly;
		}
		if (num)
		{
			ITunerQueueService tunerQueueService = TunerQueueService;
			if (tunerQueueService == null)
			{
				return num;
			}
			tunerQueueService.AddDocumentToQueue(document);
		}
		return num;
	}

	public void Close(IDocument document)
	{
		document.Uri.LocalPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		_ = document.Uri;
		ArtDefContext artDefContext = document.As<ArtDefContext>();
		m_controlHostService?.UnregisterControl(artDefContext.GUI.MainControl);
		artDefContext.ControlInfo = null;
		foreach (DomNode item in artDefContext.DomNode.Subtree)
		{
			foreach (EditingContext item2 in item.AsAll<EditingContext>())
			{
				m_contextRegistry.RemoveContext(item2);
			}
		}
		DocumentRegistry?.Remove(document);
	}

	void IControlHostClient.Activate(Control control)
	{
		if (control.Tag is ArtDefDocument artDefDocument)
		{
			DocumentRegistry.ActiveDocument = artDefDocument;
			ArtDefContext activeContext = artDefDocument.As<ArtDefContext>();
			m_contextRegistry.ActiveContext = activeContext;
		}
	}

	void IControlHostClient.Deactivate(Control control)
	{
	}

	bool IControlHostClient.Close(Control control)
	{
		bool flag = true;
		if (control.Tag is ArtDefDocument artDefDocument)
		{
			flag = DocumentService?.Close(artDefDocument) ?? true;
			if (flag)
			{
				m_contextRegistry.RemoveContext(artDefDocument);
			}
		}
		return flag;
	}

	private void DocumentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
	{
		if (m_domExplorer != null)
		{
			m_domExplorer.Root = DocumentRegistry.GetActiveDocument<DomNode>();
		}
	}

	public void Reload(IDocument document)
	{
		ArtDefSetAdapter artDefSetAdapter = document.As<ArtDefSetAdapter>();
		if (!artDefSetAdapter.ArtDef.DeserializeFromFile(document.Uri.LocalPath))
		{
			throw new InvalidTransactionException("Failed to reload document " + document.Uri.LocalPath + "!");
		}
		artDefSetAdapter.Update(artDefSetAdapter.ArtDef, artDefSetAdapter.ArtDefTemplate, initialUpdate: true);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (ArtDefCommands != null)
			{
				ArtDefCommands.ProjectConfigChanged -= ArtDefCommands_ProjectConfigChanged;
			}
			if (DocumentRegistry != null)
			{
				DocumentRegistry.ActiveDocumentChanged -= DocumentRegistry_ActiveDocumentChanged;
			}
		}
	}
}
