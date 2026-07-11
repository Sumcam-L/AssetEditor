using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IEntitySchemaAdapterCreator))]
[Export(typeof(EntitySchemaAdapterCreator))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class EntitySchemaAdapterCreator : IEntitySchemaAdapterCreator, IPartImportsSatisfiedNotification
{
	[ImportMany]
	private IEnumerable<IEntitySchemaExtender> m_adapterCreatorProviders;

	public void OnImportsSatisfied()
	{
		AdapterCreator<CustomTypeDescriptorNodeAdapter> creator = new AdapterCreator<CustomTypeDescriptorNodeAdapter>();
		EntitySchema.TextureExportSettingsType.Type.AddAdapterCreator(creator);
		EntitySchema.TextureEntityType.Type.AddAdapterCreator(creator);
		EntitySchema.MaterialInstanceType.Type.AddAdapterCreator(creator);
		EntitySchema.FireFXInstanceType.Type.AddAdapterCreator(creator);
		EntitySchema.AnimationEntityType.Type.AddAdapterCreator(creator);
		EntitySchema.GeometryEntityType.Type.AddAdapterCreator(creator);
		EntitySchema.GeoModelType.Type.AddAdapterCreator(creator);
		EntitySchema.GeoMeshType.Type.AddAdapterCreator(creator);
		EntitySchema.GeoPrimGroupType.Type.AddAdapterCreator(creator);
		EntitySchema.AssetType.Type.AddAdapterCreator(creator);
		EntitySchema.AnalyticLightEntityType.Type.AddAdapterCreator(creator);
		EntitySchema.EnvironmentLightEntityType.Type.AddAdapterCreator(creator);
		EntitySchema.LightDirectionTagSetType.Type.AddAdapterCreator(creator);
		EntitySchema.LightDirectionTagType.Type.AddAdapterCreator(creator);
		EntitySchema.LightRigEntityType.Type.AddAdapterCreator(creator);
		EntitySchema.ParticleEffectEntityType.Type.AddAdapterCreator(creator);
		EntitySchema.AttachmentPointType.Type.AddAdapterCreator(creator);
		EntitySchema.SplineType.Type.AddAdapterCreator(creator);
		EntitySchema.SplineVertexType.Type.AddAdapterCreator(creator);
		EntitySchema.TimelineType.Type.AddAdapterCreator(creator);
		EntitySchema.TimelineBindingType.Type.AddAdapterCreator(creator);
		EntitySchema.AnimationBindingType.Type.AddAdapterCreator(creator);
		EntitySchema.TriggerType.Type.AddAdapterCreator(creator);
	}
}
