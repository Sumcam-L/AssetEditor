using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IDocumentClient))]
[Export(typeof(BaseEntityEditor))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public abstract class BaseEntityEditor : IDocumentClient, IControlHostClient, IInitializable, IDocumentRegistryProvider
{
	private IContextRegistry m_contextRegistry;

	private EntitySchemaLoader m_entitySchemaLoader;

	private IDocumentRegistryMediator m_documentRegistryMediator;

	private static readonly IDictionary<InstanceType, string> InstanceTypeImageMap;

	[Import(AllowDefault = true)]
	public IDocumentService DocumentService { get; private set; }

	[Import(AllowDefault = true)]
	public ISettingsService SettingsService { get; set; }

	[Import(AllowDefault = true)]
	public IDocumentRegistry DocumentRegistry { get; private set; }

	[Import(AllowDefault = true)]
	private IEntityCacheService EntityCacheService { get; set; }

	[Import(AllowDefault = true)]
	private IEntityEditorControlService EntityEditorControlService { get; set; }

	[Import(AllowDefault = true)]
	private IAssetPreviewer PreviewerService { get; set; }

	[Import(AllowDefault = true)]
	private IPreviewerWidgetService PreviewerWidgetService { get; set; }

	[Import(AllowDefault = true)]
	private IScriptingService ScriptingService { get; set; }

	[Import(AllowDefault = true)]
	private IImportService ImportService { get; set; }

	[Import(AllowDefault = true)]
	private AssetBrowserFileCommands AssetBrowserCommands { get; set; }

	[Import(AllowDefault = true)]
	public ICivTechService CivTechService { get; set; }

	[Import(AllowDefault = true)]
	private IVersionService VersionService { get; set; }

	[Import(AllowDefault = true)]
	private IFileWatcherService FileWatchService { get; set; }

	[Import(AllowDefault = true)]
	private IPreviewerCacheService PreviewerCacheService { get; set; }

	[Import(AllowDefault = true)]
	private ITunerQueueService TunerQueueService { get; set; }

	[Import(AllowDefault = true)]
	private IHotLoadService HotLoadService { get; set; }

	[Import(AllowDefault = true)]
	private IControlHostService ControlHostService { get; set; }

	[Import(AllowDefault = true)]
	private protected BatchEntitySourceControlService SourceControlService { get; set; }

	public string EditorLayoutState { get; set; }

	public bool AskWhenClosingDirtyDocument => !m_documentRegistryMediator.ShadowMode;

	public abstract DocumentClientInfo Info { get; }

	public EntityDocumentClientInfo InfoEx => Info as EntityDocumentClientInfo;

	protected abstract DomNodeType DomNodeEntityType { get; }

	protected abstract ChildInfo DomNodeRootElement { get; }

	static BaseEntityEditor()
	{
		InstanceTypeImageMap = new Dictionary<InstanceType, string>();
		InstanceTypeImageMap[InstanceType.IT_ANALYTIC_LIGHT] = Resources.AnalyticLightFileIcon;
		InstanceTypeImageMap[InstanceType.IT_ANIMATION] = Resources.AnimationFileIcon;
		InstanceTypeImageMap[InstanceType.IT_ASSET] = Resources.AssetFileIcon;
		InstanceTypeImageMap[InstanceType.IT_BEHAVIOR] = Resources.BehaviorFileIcon;
		InstanceTypeImageMap[InstanceType.IT_ENVIRONMENT_LIGHT] = Resources.EnvironmentMapFileIcon;
		InstanceTypeImageMap[InstanceType.IT_GEOMETRY] = Resources.GeometryFileIcon;
		InstanceTypeImageMap[InstanceType.IT_LIGHT_RIG] = Resources.LightRigFileIcon;
		InstanceTypeImageMap[InstanceType.IT_MATERIAL] = Resources.MaterialFileIcon;
		InstanceTypeImageMap[InstanceType.IT_PARTICLE_EFFECT] = Resources.ParticleEffectFileIcon;
		InstanceTypeImageMap[InstanceType.IT_TEXTURE] = Resources.TextureFileIcon;
		InstanceTypeImageMap[InstanceType.IT_FIREFX] = Resources.ParticleEffectFileIcon;
	}

	public BaseEntityEditor(IContextRegistry contextRegistry, EntitySchemaLoader importedEntitySchemaLoader, IDocumentRegistryMediator documentRegistryMediator)
	{
		m_contextRegistry = contextRegistry;
		m_entitySchemaLoader = importedEntitySchemaLoader;
		m_documentRegistryMediator = documentRegistryMediator;
	}

	public IEnumerable<string> EnumerateDataFiles(IDocument doc)
	{
		if (doc is IInstanceEntityDocument)
		{
			InstanceEntityAdapter instanceEntityAdapter = (doc as DomNodeAdapter).As<InstanceEntityAdapter>();
			foreach (DataFileAdapter dataFile in instanceEntityAdapter.DataFiles)
			{
				yield return dataFile.RelativePath;
			}
		}
		else
		{
			Outputs.WriteLine(OutputMessageType.Error, $"Incorrect document type \"{doc.GetType()}\" passed to BaseEntityEditor.EnumerateDataFiles");
		}
	}

	public void SetEntityName(IDocument doc, string entityName)
	{
		if (doc is IInstanceEntityDocument)
		{
			(doc as DomNodeAdapter).As<INamedAdapter>().Name = entityName;
		}
		else
		{
			Outputs.WriteLine(OutputMessageType.Error, "Incorrect document type \"" + doc.GetType().ToString() + "\" passed to BaseEntityEditor.SetEntityName");
		}
	}

	public void SetClassName(IDocument doc, string clsName)
	{
		if (doc is IInstanceEntityDocument)
		{
			(doc as DomNodeAdapter).As<InstanceEntityAdapter>().ClassName = clsName;
		}
		else
		{
			Outputs.WriteLine(OutputMessageType.Error, "Incorrect document type \"" + doc.GetType().ToString() + "\" passed to BaseEntityEditor.SetClassName");
		}
	}

	protected abstract string GetEditorName();

	public virtual void Initialize()
	{
		if (ScriptingService != null)
		{
			ScriptingService.LoadAssembly(GetType().Assembly);
			ScriptingService.SetVariable(GetEditorName(), this);
			m_contextRegistry.ActiveContextChanged += delegate
			{
				EditingContext activeContext = m_contextRegistry.GetActiveContext<EditingContext>();
				IHistoryContext activeContext2 = m_contextRegistry.GetActiveContext<IHistoryContext>();
				ScriptingService.SetVariable("editingContext", activeContext);
				ScriptingService.SetVariable("hist", activeContext2);
			};
		}
		if (SettingsService != null)
		{
			BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => EditorLayoutState, "EditorLayoutState", null, null);
			SettingsService.RegisterSettings(this, boundPropertyDescriptor);
		}
	}

	public bool CanOpen(Uri uri)
	{
		return Info.IsCompatibleUri(uri);
	}

	protected virtual InstanceEntityAdapter InitializeAdapter(DomNode node, IInstanceEntity entity)
	{
		InstanceEntityAdapter instanceEntityAdapter = node.As<InstanceEntityAdapter>();
		instanceEntityAdapter.CivTechService = CivTechService;
		instanceEntityAdapter.FileWatcherService = FileWatchService;
		instanceEntityAdapter.PreviewerService = PreviewerService;
		instanceEntityAdapter.PreviewerWidgetService = PreviewerWidgetService;
		instanceEntityAdapter.DocumentRegistry = DocumentRegistry;
		instanceEntityAdapter.InstanceEntity = entity;
		instanceEntityAdapter.Update();
		return instanceEntityAdapter;
	}

	protected virtual BaseInstanceEntityDocument InitializeDocument(DomNode node, Uri uri, IInstanceEntity entity, IInstanceSet instances)
	{
		BaseInstanceEntityDocument baseInstanceEntityDocument = node.As<BaseInstanceEntityDocument>();
		baseInstanceEntityDocument.Uri = uri;
		baseInstanceEntityDocument.VersionService = VersionService;
		baseInstanceEntityDocument.InstanceEntity = entity;
		baseInstanceEntityDocument.InstanceSet = instances;
		baseInstanceEntityDocument.CivTechService = CivTechService;
		baseInstanceEntityDocument.DocumentClient = this;
		baseInstanceEntityDocument.ImportService = ImportService;
		return baseInstanceEntityDocument;
	}

	protected virtual BaseEntityPropertyContext InitializeContext(DomNode node, BaseInstanceEntityDocument document, ControlInfo controlInfo)
	{
		BaseEntityPropertyContext baseEntityPropertyContext = node.As<BaseEntityPropertyContext>();
		baseEntityPropertyContext.ControlInfo = controlInfo;
		baseEntityPropertyContext.CivTechService = CivTechService;
		baseEntityPropertyContext.FileWatchService = FileWatchService;
		baseEntityPropertyContext.ImportService = ImportService;
		baseEntityPropertyContext.AssetDocumentCommands = AssetBrowserCommands;
		baseEntityPropertyContext.Doc = document;
		long tCreate = 0, tBind = 0;
		{
			var sw = Stopwatch.StartNew();
			baseEntityPropertyContext.GUI = EntityEditorControlService?.CreateControl(document);
			tCreate = sw.ElapsedMilliseconds;
		}
		if (baseEntityPropertyContext.GUI != null)
		{
			var sw = Stopwatch.StartNew();
			baseEntityPropertyContext.GUI.Tag = document;
			baseEntityPropertyContext.GUI.Layout += EntityEditor_Layout;
			baseEntityPropertyContext.GUI.Bind(baseEntityPropertyContext);
			Control gUI = baseEntityPropertyContext.GUI;
			if (gUI != null)
			{
				gUI.Layout += EntityEditor_Layout;
			}
			tBind = sw.ElapsedMilliseconds;
		}
		if (tCreate + tBind > 1)
			PaintTimingLog.Write("InitializeContext: create={0}ms, bind={1}ms", tCreate, tBind);
		baseEntityPropertyContext.DocumentRegistryMediator = m_documentRegistryMediator;
		baseEntityPropertyContext.SourceControl = SourceControlService;
		baseEntityPropertyContext.EntityCacheService = EntityCacheService;
		baseEntityPropertyContext.TunerQueueService = TunerQueueService;
		baseEntityPropertyContext.HotLoadService = HotLoadService;
		return baseEntityPropertyContext;
	}

	public IDocument Open(Uri uri)
	{
		var sw = Stopwatch.StartNew();
		string localPath = uri.LocalPath;
		IInstanceEntity instanceEntity = null;
		string instanceName = string.Empty;
		InstanceType type = InstanceType.IT_COUNT;
		if (!StaticMethods.GetInstanceNameAndType(CivTechService.ProjectMapService, localPath, out instanceName, out type))
		{
			BugSubmitter.SilentReport("Unable to get instance name and type for path " + localPath + "!  @assign bwhitman  @summary BadUri");
			instanceName = InfoEx.NewDocumentName;
			type = InfoEx.ExtensionTypes.First();
		}
		IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { CivTechService.GetActivePantryPaths() });
		var tLoad = Stopwatch.StartNew();
		if (File.Exists(localPath))
		{
			instanceEntity = instanceSet.LoadEntityByPath(localPath, type);
			if (instanceEntity != null)
			{
				BugSubmitter.SilentAssert(PathComparer.PathCompareWithCase.Equals(instanceEntity.Name, instanceName) || !PathComparer.PathCompareIgnoreCase.Equals(instanceEntity.Name, instanceName), "Opening an entity from file \"{0}\" resulted in an entity with the name of \"{1}\" instead of the expected name of \"{2}\" @summary Opening an entity with a given URI did not result in opening an entity of the same name @assign bwhitman", localPath, instanceEntity.Name, instanceName);
			}
		}
		tLoad.Stop();
		if (instanceEntity == null)
		{
			instanceEntity = instanceSet.CreateEntityByName(instanceName, type);
		}
		var tInit = Stopwatch.StartNew();
		DomNode domNode = new DomNode(DomNodeEntityType, DomNodeRootElement);
		domNode.InitializeExtensions();
		string value = string.Empty;
		InstanceTypeImageMap.TryGetValue(type, out value);
		ControlInfo controlInfo = new ControlInfo(instanceEntity.Name + instanceEntity.XMLExtension + "(" + CivTechService.GetProjectName(uri) + ")", localPath, StandardControlGroup.Center, ResourceUtil.GetIcon(value), null);
		controlInfo.IsDocument = true;
		BaseInstanceEntityDocument document = InitializeDocument(domNode, uri, instanceEntity, instanceSet);
		controlInfo.IsDirtyDocument = () => document.Dirty;
		controlInfo.IsReadOnlyDocument = () => document.IsReadOnly;
		tInit.Stop();
		var tAdapter = Stopwatch.StartNew();
		InitializeAdapter(domNode, instanceEntity);
		tAdapter.Stop();
		var tCtx = Stopwatch.StartNew();
		BaseEntityPropertyContext baseEntityPropertyContext = InitializeContext(domNode, document, controlInfo);
		tCtx.Stop();
		OnDocumentOpening(document);
		long tReg = 0;
		if (baseEntityPropertyContext.GUI != null)
		{
			var t = Stopwatch.StartNew();
			ControlHostService?.RegisterControl(baseEntityPropertyContext.GUI, controlInfo, this);
			tReg = t.ElapsedMilliseconds;
		}
		PaintTimingLog.Write("Open doc step: load={0}ms, init={1}ms, adapter={2}ms, ctx={3}ms, reg={4}ms", tLoad.ElapsedMilliseconds, tInit.ElapsedMilliseconds, tAdapter.ElapsedMilliseconds, tCtx.ElapsedMilliseconds, tReg);
		PaintTimingLog.Write("Opened document {0} in {1}ms", uri, sw?.ElapsedMilliseconds ?? 0);
		if (VersionService != null && !VersionService.IsLocalBuild() && instanceEntity.Version > VersionService.ApplicationVersion)
		{
			MessageBoxes.Show("The file " + localPath + " was saved with newer tools. You will be unable to make changes to the file.\n\nPlease update your content tools.", "Error Opening File", MessageBoxButton.OK, MessageBoxImage.Error);
		}
		return document;
	}

	protected virtual void OnDocumentOpening(IEntityDocument document)
	{
	}

	public void Show(IDocument document)
	{
		BugSubmitter.SilentAssert(!(document is IShadowEntityDocument), "Trying to show shadow document \"{0}\" with {1} @summary Trying to show a shadow document @assign bwhitman", document.Uri.LocalPath, GetType().Name);
		if (!document.As<BaseEntityPropertyContext>().GUI.IsDisposed)
		{
			ControlHostService?.Show(document.As<BaseEntityPropertyContext>().GUI);
			return;
		}
		BugSubmitter.SilentAssert(false, "Attempting to show a disposed control for {0} @summary Showing disposed control @assign nicholai.wojtowycz", document.Uri);
	}

	protected virtual void OnDocumentSaving(IEntityDocument document, Uri uri)
	{
		document.InstanceEntity.Name = StaticMethods.SanitizeEntityName(document.InstanceEntity.Name);
	}

	protected virtual void OnDocumentSaved(IEntityDocument document, Uri uri)
	{
		TunerQueueService?.AddDocumentToQueue(document);
	}

	public bool Save(IDocument document, Uri uri)
	{
		bool result = false;
		using (new FileWatchSuspension(FileWatchService, uri))
		{
			IEntityDocument entityDocument = document.As<IEntityDocument>();
			if (entityDocument != null)
			{
				IInstanceEntity instanceEntity = entityDocument.InstanceEntity;
				if (instanceEntity != null)
				{
					OnDocumentSaving(entityDocument, uri);
					if (VersionService != null)
					{
						Version applicationVersion = VersionService.ApplicationVersion;
						instanceEntity.SetVersion(applicationVersion.Major, applicationVersion.Minor, applicationVersion.Build, applicationVersion.Revision);
					}
					result = instanceEntity.SerializeIntoFile(uri.LocalPath);
					OnDocumentSaved(entityDocument, uri);
					Outputs.WriteLine(OutputMessageType.Info, $"Saved document {document.Uri} to {uri}");
				}
			}
		}
		return result;
	}

	public void Close(IDocument document)
	{
		BaseEntityPropertyContext baseEntityPropertyContext = document.As<BaseEntityPropertyContext>();
		ControlHostService?.UnregisterControl(baseEntityPropertyContext.GUI);
		baseEntityPropertyContext.ControlInfo = null;
		foreach (DomNode item in baseEntityPropertyContext.DomNode.Subtree)
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
		if (control.Tag is IInstanceEntityDocument instanceEntityDocument)
		{
			long tDoc = 0, tCtx = 0;
			if (DocumentRegistry != null)
			{
				var sw = Stopwatch.StartNew();
				DocumentRegistry.ActiveDocument = instanceEntityDocument;
				tDoc = sw.ElapsedMilliseconds;
			}
			{
				var sw = Stopwatch.StartNew();
				BaseEntityPropertyContext activeContext = instanceEntityDocument.As<BaseEntityPropertyContext>();
				m_contextRegistry.ActiveContext = activeContext;
				tCtx = sw.ElapsedMilliseconds;
			}
			if (tDoc + tCtx > 0)
				PaintTimingLog.Write("Activate: doc={0}ms, ctx={1}ms", tDoc, tCtx);
		}
	}

	void IControlHostClient.Deactivate(Control control)
	{
	}

	bool IControlHostClient.Close(Control control)
	{
		bool flag = true;
		if (control.Tag is IInstanceEntityDocument instanceEntityDocument)
		{
			flag = DocumentService?.Close(instanceEntityDocument) ?? true;
			if (flag)
			{
				m_contextRegistry.RemoveContext(instanceEntityDocument);
			}
		}
		return flag;
	}

	protected virtual void EntityEditor_Layout(object sender, LayoutEventArgs e)
	{
	}

	public void Reload(IDocument document)
	{
		BaseInstanceEntityDocument baseInstanceEntityDocument = document.As<BaseInstanceEntityDocument>();
		BaseEntityPropertyContext baseEntityPropertyContext = document.As<BaseEntityPropertyContext>();
		if (!baseInstanceEntityDocument.InstanceEntity.DeserializeFromFile(document.Uri.LocalPath))
		{
			Outputs.WriteLine(OutputMessageType.Error, "Failed to reload document {0}!", document.Uri.LocalPath);
		}
		baseEntityPropertyContext.PerformReload();
		baseInstanceEntityDocument.UpdateControlInfo();
	}
}
