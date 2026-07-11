using System.ComponentModel.Composition;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IDocumentClient))]
[Export(typeof(MaterialEditor))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class MaterialEditor : BaseEntityEditor
{
	public static DocumentClientInfo DocumentClientInfo = new EntityDocumentClientInfo("Material".Localize(), new string[1] { ".mtl" }, new InstanceType[1] { InstanceType.IT_MATERIAL }, Sce.Atf.Resources.DocumentImage, Sce.Atf.Resources.FolderImage, "NewMaterial", multiDocument: true, Resources.MaterialFileIcon);

	public override DocumentClientInfo Info => DocumentClientInfo;

	protected override DomNodeType DomNodeEntityType => EntitySchema.MaterialInstanceType.Type;

	protected override ChildInfo DomNodeRootElement => EntitySchema.MaterialInstanceRootElement;

	[ImportingConstructor]
	public MaterialEditor(IContextRegistry contextRegistry, EntitySchemaLoader importedEntitySchemaLoader, IDocumentRegistryMediator documentRegistryMediator)
		: base(contextRegistry, importedEntitySchemaLoader, documentRegistryMediator)
	{
	}

	protected override string GetEditorName()
	{
		return "materialEditor";
	}
}
