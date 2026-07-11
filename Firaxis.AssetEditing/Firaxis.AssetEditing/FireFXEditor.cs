using System;
using System.ComponentModel.Composition;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IDocumentClient))]
[Export(typeof(FireFXEditor))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class FireFXEditor : BaseEntityEditor
{
	public static DocumentClientInfo DocumentClientInfo = new EntityDocumentClientInfo("FireFX".Localize(), new string[1] { ".ffx" }, new InstanceType[1] { InstanceType.IT_FIREFX }, Sce.Atf.Resources.DocumentImage, Sce.Atf.Resources.FolderImage, "NewFireFX", multiDocument: true, Resources.FireFXFileIcon);

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	public override DocumentClientInfo Info => DocumentClientInfo;

	protected override DomNodeType DomNodeEntityType => EntitySchema.FireFXInstanceType.Type;

	protected override ChildInfo DomNodeRootElement => EntitySchema.FireFXInstanceRootElement;

	[Import(AllowDefault = true)]
	private AssetBrowserFileCommands AssetBrowserFileCommands { get; set; }

	[Import(AllowDefault = true)]
	private IFireFXService FireFXService { get; set; }

	public string FireFXEditorlayoutState { get; set; }

	[ImportingConstructor]
	public FireFXEditor(IContextRegistry contextRegistry, EntitySchemaLoader importedEntitySchemaLoader, IDocumentRegistryMediator documentRegistryMediator)
		: base(contextRegistry, importedEntitySchemaLoader, documentRegistryMediator)
	{
	}

	private void AssetBrowserFileCommands_DocumentClosing(object sender, DocumentClosingEventArgs e)
	{
		if (e.Document is FireFXDocument adaptable)
		{
			FireFXEditorControl fireFXEditorControl = adaptable.As<FireFXContext>().GUI as FireFXEditorControl;
			FireFXEditorlayoutState = fireFXEditorControl.EditorLayoutState;
		}
	}

	protected override string GetEditorName()
	{
		return "fireFXEditor";
	}

	protected override BaseInstanceEntityDocument InitializeDocument(DomNode node, Uri uri, IInstanceEntity entity, IInstanceSet instances)
	{
		node.As<FireFXDocument>().FireFXService = FireFXService;
		return base.InitializeDocument(node, uri, entity, instances);
	}

	protected override BaseEntityPropertyContext InitializeContext(DomNode node, BaseInstanceEntityDocument document, ControlInfo controlInfo)
	{
		node.As<FireFXContext>().FireFXEditor = this;
		return base.InitializeContext(node, document, controlInfo);
	}

	public override void Initialize()
	{
		base.Initialize();
		if (AssetBrowserFileCommands != null)
		{
			AssetBrowserFileCommands.DocumentClosing += AssetBrowserFileCommands_DocumentClosing;
		}
		if (m_settingsService != null)
		{
			BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => FireFXEditorlayoutState, "FireFXEditorlayoutState", null, null);
			m_settingsService.RegisterSettings(this, boundPropertyDescriptor);
		}
	}
}
