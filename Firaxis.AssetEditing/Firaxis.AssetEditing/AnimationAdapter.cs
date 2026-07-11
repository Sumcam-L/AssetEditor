using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class AnimationAdapter : ImportedEntityAdapter
{
	public IAnimationInstance Animation => InstanceEntity as IAnimationInstance;

	public override IImportedEntity ImportedEntity => Animation;

	protected override AttributeInfo ClassNameAttribute => EntitySchema.AnimationEntityType.ClassNameAttribute;

	protected override ChildInfo CookParametersChild => EntitySchema.AnimationEntityType.CookParametersChild;

	protected override ChildInfo DataFilesChild => EntitySchema.AnimationEntityType.DataFilesChild;

	protected override AttributeInfo DescriptionAttribute => EntitySchema.AnimationEntityType.DescriptionAttribute;

	protected override AttributeInfo ExportedTimeAttribute => EntitySchema.AnimationEntityType.ExportedTimeAttribute;

	protected override AttributeInfo ImportedTimeAttribute => EntitySchema.AnimationEntityType.ImportedTimeAttribute;

	protected override AttributeInfo NameAttribute => EntitySchema.AnimationEntityType.NameAttribute;

	protected override ChildInfo SourceFilePathChild => EntitySchema.AnimationEntityType.SourceFilePathChild;

	protected override AttributeInfo SourceObjectNameAttribute => EntitySchema.AnimationEntityType.SourceObjectNameAttribute;

	protected override ChildInfo TagsChild => EntitySchema.AnimationEntityType.TagsChild;
}
