using System.IO;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class EnvironmentLightAdapter : ImportedEntityAdapter
{
	public override IImportedEntity ImportedEntity => Light;

	public IEnvironmentLightInstance Light => InstanceEntity as IEnvironmentLightInstance;

	public LightDirectionTagSetAdapter LightDirectionTagSet { get; private set; }

	protected override AttributeInfo ClassNameAttribute => EntitySchema.EnvironmentLightEntityType.ClassNameAttribute;

	protected override ChildInfo CookParametersChild => EntitySchema.EnvironmentLightEntityType.CookParametersChild;

	protected override ChildInfo DataFilesChild => EntitySchema.EnvironmentLightEntityType.DataFilesChild;

	protected override AttributeInfo DescriptionAttribute => EntitySchema.EnvironmentLightEntityType.DescriptionAttribute;

	protected override AttributeInfo ExportedTimeAttribute => EntitySchema.EnvironmentLightEntityType.ExportedTimeAttribute;

	protected override AttributeInfo ImportedTimeAttribute => EntitySchema.EnvironmentLightEntityType.ImportedTimeAttribute;

	protected override AttributeInfo NameAttribute => EntitySchema.EnvironmentLightEntityType.NameAttribute;

	protected override ChildInfo SourceFilePathChild => EntitySchema.EnvironmentLightEntityType.SourceFilePathChild;

	protected override AttributeInfo SourceObjectNameAttribute => EntitySchema.EnvironmentLightEntityType.SourceObjectNameAttribute;

	protected override ChildInfo TagsChild => EntitySchema.EnvironmentLightEntityType.TagsChild;

	public static LightDirectionTagAdapter CreateLightDirectionTag()
	{
		DomNode domNode = new DomNode(EntitySchema.LightDirectionTagType.Type);
		domNode.InitializeExtensions();
		return domNode.As<LightDirectionTagAdapter>();
	}

	public static LightDirectionTagAdapter CreateLightDirectionTag(float x, float y, float z)
	{
		DomNode domNode = new DomNode(EntitySchema.LightDirectionTagType.Type);
		domNode.InitializeExtensions();
		LightDirectionTagAdapter lightDirectionTagAdapter = domNode.As<LightDirectionTagAdapter>();
		lightDirectionTagAdapter.XPosition = x;
		lightDirectionTagAdapter.YPosition = y;
		lightDirectionTagAdapter.ZPosition = z;
		return lightDirectionTagAdapter;
	}

	protected override void AssignPropertiesFromEntity(bool updateUI)
	{
		base.AssignPropertiesFromEntity(updateUI);
		LightDirectionTagSet.Update();
		base.DomNode.As<EnvironmentLightContext>();
	}

	protected override bool CanPerformImport()
	{
		bool num = base.CivTechService.PrimaryProject.Config.Classes.FindForInstance(InstanceEntity) != null;
		bool flag = !string.IsNullOrEmpty(base.SourceFilePath) && File.Exists(base.SourceFilePath);
		if (!num)
		{
			Outputs.WriteLine(OutputMessageType.Warning, "Cannot perform an import because there is no valid class assigned to the entity.");
		}
		if (!flag)
		{
			Outputs.WriteLine(OutputMessageType.Warning, "Cannot perform an import because the source file is not set or does not exist on disk.");
		}
		return num && flag;
	}

	protected override void OnNodeSet()
	{
		LightDirectionTagSet = this.CreateComponentAdapter<LightDirectionTagSetAdapter>(EntitySchema.LightDirectionTagSetType.Type, EntitySchema.EnvironmentLightEntityType.LightDirectionTagsChild);
		base.OnNodeSet();
	}

	protected override void PerformImport()
	{
		if (CanPerformImport())
		{
			using (new WaitCursor())
			{
				base.PerformImport();
				EnvironmentLightContext environmentLightContext = base.DomNode.As<EnvironmentLightContext>();
				environmentLightContext.OpenSourceFile(base.SourceFilePath);
				environmentLightContext.ApplyChanges();
			}
		}
	}
}
