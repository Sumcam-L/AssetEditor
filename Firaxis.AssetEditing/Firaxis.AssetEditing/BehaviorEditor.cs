using System.ComponentModel.Composition;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IDocumentClient))]
[Export(typeof(BehaviorEditor))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class BehaviorEditor : BaseEntityEditor
{
	public static DocumentClientInfo DocumentClientInfo = new EntityDocumentClientInfo("Behavior".Localize(), new string[1] { ".bhv" }, new InstanceType[1] { InstanceType.IT_BEHAVIOR }, Sce.Atf.Resources.DocumentImage, Sce.Atf.Resources.FolderImage, "NewBehavior", multiDocument: true, Resources.BehaviorFileIcon);

	[Import(AllowDefault = true)]
	private IAssetBrowserDialogService AssetBrowserDialogService { get; set; }

	public override DocumentClientInfo Info => DocumentClientInfo;

	protected override DomNodeType DomNodeEntityType => EntitySchema.BehaviorType.Type;

	protected override ChildInfo DomNodeRootElement => EntitySchema.BehaviorEntityRootElement;

	[ImportingConstructor]
	public BehaviorEditor(IContextRegistry contextRegistry, EntitySchemaLoader importedEntitySchemaLoader, IDocumentRegistryMediator documentRegistryMediator)
		: base(contextRegistry, importedEntitySchemaLoader, documentRegistryMediator)
	{
	}

	protected override string GetEditorName()
	{
		return "behaviorInstanceEditor";
	}

	protected override BaseEntityPropertyContext InitializeContext(DomNode node, BaseInstanceEntityDocument document, ControlInfo controlInfo)
	{
		node.As<BehaviorContext>().AssetBrowserService = AssetBrowserDialogService;
		return base.InitializeContext(node, document, controlInfo);
	}
}
