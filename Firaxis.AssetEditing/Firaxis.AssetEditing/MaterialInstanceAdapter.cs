using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class MaterialInstanceAdapter : InstanceEntityAdapter
{
	public IMaterialInstance Material => InstanceEntity as IMaterialInstance;

	protected override AttributeInfo ClassNameAttribute => EntitySchema.MaterialInstanceType.ClassNameAttribute;

	protected override ChildInfo CookParametersChild => EntitySchema.MaterialInstanceType.CookParametersChild;

	protected override ChildInfo DataFilesChild => EntitySchema.MaterialInstanceType.DataFilesChild;

	protected override AttributeInfo DescriptionAttribute => EntitySchema.MaterialInstanceType.DescriptionAttribute;

	protected override AttributeInfo NameAttribute => EntitySchema.MaterialInstanceType.NameAttribute;

	protected override ChildInfo TagsChild => EntitySchema.MaterialInstanceType.TagsChild;
}
