using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class AnalyticLightAdapter : ImportedEntityAdapter
{
	public override IImportedEntity ImportedEntity => Light;

	public IAnalyticLightInstance Light => InstanceEntity as IAnalyticLightInstance;

	protected override AttributeInfo ClassNameAttribute => EntitySchema.AnalyticLightEntityType.ClassNameAttribute;

	protected override ChildInfo CookParametersChild => EntitySchema.AnalyticLightEntityType.CookParametersChild;

	protected override ChildInfo DataFilesChild => EntitySchema.AnalyticLightEntityType.DataFilesChild;

	protected override AttributeInfo DescriptionAttribute => EntitySchema.AnalyticLightEntityType.DescriptionAttribute;

	protected override AttributeInfo ExportedTimeAttribute => EntitySchema.AnalyticLightEntityType.ExportedTimeAttribute;

	protected override AttributeInfo ImportedTimeAttribute => EntitySchema.AnalyticLightEntityType.ImportedTimeAttribute;

	protected override AttributeInfo NameAttribute => EntitySchema.AnalyticLightEntityType.NameAttribute;

	protected override ChildInfo SourceFilePathChild => EntitySchema.AnalyticLightEntityType.SourceFilePathChild;

	protected override AttributeInfo SourceObjectNameAttribute => EntitySchema.AnalyticLightEntityType.SourceObjectNameAttribute;

	protected override ChildInfo TagsChild => EntitySchema.AnalyticLightEntityType.TagsChild;
}
