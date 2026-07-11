using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DatabaseWrapper;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.Packages;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IDocumentClient))]
[Export(typeof(XLPEditor))]
[Export(typeof(IProjectChangeWatcher))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class XLPEditor : IDocumentClient, IControlHostClient, IInitializable, IProjectChangeWatcher
{
	private readonly IContextRegistry m_contextRegistry;

	private readonly XLPSchemaLoader m_xlpSchemaLoader;

	private readonly ICivTechService m_civTechService;

	private readonly IFileWatcherService m_fileWatchService;

	private readonly IVersionService m_versionService;

	private readonly IXLPRegistry m_XLPRegistry;

	[Import(AllowDefault = true)]
	private ScriptingService m_scriptingService;

	[Import(AllowDefault = true)]
	private IDocumentService m_documentService;

	[Import(AllowDefault = true)]
	private IDocumentRegistry m_documentRegistry;

	[Import(AllowDefault = true)]
	private IControlHostService m_controlHostService;

	public static DocumentClientInfo DocumentClientInfo = new DocumentClientInfoEx("XLP".Localize(), ".xlp", Resources.NewFileIcon, Resources.OpenXLPIcon, multiDocument: true, isHidden: false, "Other");

	[Import(AllowDefault = true)]
	private IEntityCacheService EntityCacheService { get; set; }

	[Import(AllowDefault = true)]
	private ITunerQueueService TunerQueueService { get; set; }

	[Import(AllowDefault = true)]
	private IImportService ImportService { get; set; }

	[Import(AllowDefault = true)]
	private AssetBrowserFileCommands AssetBrowserFileCommands { get; set; }

	[Import(AllowDefault = true)]
	private IAssetBrowserDialogService AssetBrowserFileService { get; set; }

	public bool AskWhenClosingDirtyDocument => true;

	public DocumentClientInfo Info => DocumentClientInfo;

	[ImportingConstructor]
	public XLPEditor(IContextRegistry contextRegistry, XLPSchemaLoader xlpSchemaLoader, ICivTechService civTechSvc, IFileWatcherService fileWatchSvc, IVersionService versionService, IXLPRegistry xlpRegistry)
	{
		m_contextRegistry = contextRegistry;
		m_xlpSchemaLoader = xlpSchemaLoader;
		m_civTechService = civTechSvc;
		m_fileWatchService = fileWatchSvc;
		m_versionService = versionService;
		m_XLPRegistry = xlpRegistry;
	}

	public virtual void HandleProjectChange(Action<string> statusMessagePrinter)
	{
		Info.InitialDirectory = m_civTechService.PrimaryProject.Paths.XLPRoot;
	}

	public void SetXLPClass(IDocument doc, string clsName)
	{
		if (doc is XLPDocument)
		{
			(doc as DomNodeAdapter).As<XLPAdapter>().ClassName = clsName;
		}
		else
		{
			Outputs.WriteLine(OutputMessageType.Error, "Incorrect document type \"" + doc.GetType().ToString() + "\" passed to XLPEditor.SetXLPClass");
		}
	}

	public void SetPackageName(IDocument doc, string pkgName)
	{
		if (doc is XLPDocument)
		{
			(doc as DomNodeAdapter).As<XLPAdapter>().PackageName = pkgName;
		}
		else
		{
			Outputs.WriteLine(OutputMessageType.Error, "Incorrect document type \"" + doc.GetType().ToString() + "\" passed to XLPEditor.SetPackageName");
		}
	}

	public void AddEntry(IDocument doc, string entryId, string asset)
	{
		if (doc is XLPDocument xLPDocument)
		{
			string iD = entryId;
			if (xLPDocument.XLP.FindEntry(iD) != null)
			{
				int i;
				for (i = 1; xLPDocument.XLP.FindEntry($"{entryId}{i}") != null; i++)
				{
				}
				iD = $"{entryId}{i}";
			}
			xLPDocument.XLP.AddEntry(iD, asset);
			xLPDocument.As<XLPAdapter>().Update(xLPDocument.XLP);
		}
		else
		{
			Outputs.WriteLine(OutputMessageType.Error, "Incorrect document type \"" + doc.GetType().ToString() + "\" passed to XLPEditor.SetPackageName");
		}
	}

	void IInitializable.Initialize()
	{
		string gamePantry = m_civTechService.PrimaryProject.Paths.GamePantry;
		Info.InitialDirectory = Path.Combine(gamePantry, "XLPs");
		if (m_scriptingService != null)
		{
			m_scriptingService.LoadAssembly(GetType().Assembly);
			m_scriptingService.SetVariable("xlpEditor", this);
			m_contextRegistry.ActiveContextChanged += delegate
			{
				EditingContext activeContext = m_contextRegistry.GetActiveContext<EditingContext>();
				IHistoryContext activeContext2 = m_contextRegistry.GetActiveContext<IHistoryContext>();
				m_scriptingService.SetVariable("editingContext", activeContext);
				m_scriptingService.SetVariable("hist", activeContext2);
			};
		}
	}

	public bool CanOpen(Uri uri)
	{
		return DocumentClientInfo.IsCompatibleUri(uri);
	}

	public IDocument Open(Uri uri)
	{
		Outputs.WriteLine(OutputMessageType.Info, "Opening XLP file \"{0}\"", uri.LocalPath);
		string localPath = uri.LocalPath;
		string fileName = Path.GetFileName(localPath);
		IXLP iXLP;
		if (File.Exists(localPath))
		{
			iXLP = global::DatabaseWrapper.DatabaseWrapper.LoadXLP(uri);
			if (iXLP == null)
			{
				Outputs.WriteLine(OutputMessageType.Error, "Failed to open XLP file at \"{0}\"", uri.LocalPath);
				return null;
			}
		}
		else
		{
			iXLP = Context.EnsureCreated<CivTechContext>().CreateInstance<IXLP>();
		}
		DomNode domNode = new DomNode(XLPSchema.XLPType.Type);
		domNode.InitializeExtensions();
		XLPContext xLPContext = domNode.As<XLPContext>();
		ControlInfo controlInfo = new ControlInfo(fileName + "(" + m_civTechService.GetProjectName(uri) + ")", localPath, StandardControlGroup.Center, ResourceUtil.GetIcon(Resources.XLPFileIcon), null);
		controlInfo.IsDocument = true;
		xLPContext.ControlInfo = controlInfo;
		xLPContext.AssetBrowserCommands = AssetBrowserFileCommands;
		xLPContext.AssetBrowserService = AssetBrowserFileService;
		xLPContext.CivTechService = m_civTechService;
		xLPContext.FileWatchService = m_fileWatchService;
		xLPContext.ImportService = ImportService;
		xLPContext.EntityCacheService = EntityCacheService;
		XLPDocument document = domNode.As<XLPDocument>();
		document.CivTechService = m_civTechService;
		document.VersionService = m_versionService;
		document.Uri = uri;
		XLPAdapter xLPAdapter = domNode.As<XLPAdapter>();
		xLPAdapter.Update(iXLP);
		document.UpdateControlInfo();
		if (!document.IsReadOnly)
		{
			xLPAdapter.AssignDefaultPlatforms();
		}
		xLPContext.Doc = document;
		controlInfo.IsDirtyDocument = () => document.Dirty;
		controlInfo.IsReadOnlyDocument = () => document.IsReadOnly;
		if (m_controlHostService != null)
		{
			xLPContext.GUI = new XLPEditorControl();
			xLPContext.GUI.Bind(xLPContext);
			xLPContext.GUI.Tag = document;
			m_controlHostService.RegisterControl(xLPContext.GUI, controlInfo, this);
		}
		Outputs.WriteLine(OutputMessageType.Info, "Opened XLP file \"{0}\"", uri.LocalPath);
		if (!m_versionService.IsLocalBuild() && iXLP.Version > m_versionService.ApplicationVersion)
		{
			MessageBoxes.Show("The file " + localPath + " was saved with newer tools. You will be unable to make changes to the file.\n\nPlease update your content tools.", "Error Opening File", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		return document;
	}

	public void Show(IDocument document)
	{
		XLPContext xLPContext = document.As<XLPContext>();
		m_controlHostService?.Show(xLPContext.GUI);
	}

	public bool Save(IDocument document, Uri uri)
	{
		XLPDocument xLPDocument = document.As<XLPDocument>();
		IXLP xLP = xLPDocument.XLP;
		Version applicationVersion = m_versionService.ApplicationVersion;
		xLP.SetVersion(applicationVersion.Major, applicationVersion.Minor, applicationVersion.Build, applicationVersion.Revision);
		if (string.IsNullOrWhiteSpace(xLP.Package))
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(uri.LocalPath);
			if (!string.IsNullOrWhiteSpace(fileNameWithoutExtension))
			{
				xLPDocument.DomNode.SetAttribute(XLPSchema.XLPType.PackageNameAttribute, fileNameWithoutExtension);
			}
		}
		if (string.IsNullOrWhiteSpace(xLP.ClassName))
		{
			string text = m_civTechService.PrimaryProject.Config.XLPClasses.Items.FirstOrDefault()?.Name;
			if (text != null)
			{
				xLPDocument.DomNode.SetAttribute(XLPSchema.XLPType.ClassNameAttribute, text);
			}
		}
		bool num = xLP.SerializeIntoFile(uri.LocalPath);
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
		XLPContext xLPContext = document.As<XLPContext>();
		m_controlHostService?.UnregisterControl(xLPContext.GUI);
		xLPContext.ControlInfo = null;
		foreach (DomNode item in xLPContext.DomNode.Subtree)
		{
			foreach (EditingContext item2 in item.AsAll<EditingContext>())
			{
				m_contextRegistry.RemoveContext(item2);
			}
		}
		m_documentRegistry?.Remove(document);
	}

	void IControlHostClient.Activate(Control control)
	{
		if (control.Tag is XLPDocument xLPDocument)
		{
			m_documentRegistry.ActiveDocument = xLPDocument;
			XLPContext activeContext = xLPDocument.As<XLPContext>();
			m_contextRegistry.ActiveContext = activeContext;
		}
	}

	void IControlHostClient.Deactivate(Control control)
	{
	}

	bool IControlHostClient.Close(Control control)
	{
		bool flag = true;
		if (control.Tag is XLPDocument xLPDocument)
		{
			flag = m_documentService?.Close(xLPDocument) ?? true;
			if (flag)
			{
				m_contextRegistry.RemoveContext(xLPDocument);
			}
		}
		return flag;
	}

	public void Reload(IDocument document)
	{
		XLPAdapter xLPAdapter = document.As<XLPAdapter>();
		IXLP xLP = xLPAdapter.XLP;
		if ((bool)xLP.DeserializeFromFile(document.Uri.LocalPath))
		{
			xLPAdapter.Update(xLP);
		}
	}
}
