using System.ComponentModel.Composition;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IDocumentClient))]
[Export(typeof(LightRigEditor))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class LightRigEditor : BaseEntityEditor
{
	public static DocumentClientInfo DocumentClientInfo = new EntityDocumentClientInfo("Light Rig".Localize(), new string[1] { ".lrg" }, new InstanceType[1] { InstanceType.IT_LIGHT_RIG }, Sce.Atf.Resources.DocumentImage, Sce.Atf.Resources.FolderImage, "NewLightRig", multiDocument: true, Resources.LightRigFileIcon);

	public override DocumentClientInfo Info => DocumentClientInfo;

	protected override DomNodeType DomNodeEntityType => EntitySchema.LightRigEntityType.Type;

	protected override ChildInfo DomNodeRootElement => EntitySchema.LightRigEntityRootElement;

	[Import(AllowDefault = true)]
	private IAssetBrowserDialogService AssetBrowserService { get; set; }

	[ImportingConstructor]
	public LightRigEditor(IContextRegistry contextRegistry, EntitySchemaLoader importedEntitySchemaLoader, IDocumentRegistryMediator documentRegistryMediator)
		: base(contextRegistry, importedEntitySchemaLoader, documentRegistryMediator)
	{
	}

	protected override string GetEditorName()
	{
		return "lightRigInstanceEditor";
	}

	protected override BaseEntityPropertyContext InitializeContext(DomNode node, BaseInstanceEntityDocument document, ControlInfo controlInfo)
	{
		BaseEntityPropertyContext baseEntityPropertyContext = base.InitializeContext(node, document, controlInfo);
		baseEntityPropertyContext.As<LightRigContext>().AssetBrowserService = AssetBrowserService;
		return baseEntityPropertyContext;
	}
}
