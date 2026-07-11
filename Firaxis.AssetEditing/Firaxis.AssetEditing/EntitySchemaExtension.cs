using System.ComponentModel.Composition;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IEntitySchemaExtender))]
[Export(typeof(EntitySchemaExtension))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class EntitySchemaExtension : IEntitySchemaExtender
{
	[ImportingConstructor]
	public EntitySchemaExtension(IEntitySchemaInitializer typeInitializer)
	{
		EntitySchema.TextureEntityType.Type.Define(new ExtensionInfo<TextureEntityDocument>());
		EntitySchema.TextureEntityType.Type.Define(new ExtensionInfo<TextureEntityPropertyContext>());
		EntitySchema.TextureEntityType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
		EntitySchema.TextureEntityType.Type.Define(new ExtensionInfo<ReferenceValidator>());
		EntitySchema.TextureEntityType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
		EntitySchema.TextureEntityType.Type.Define(new ExtensionInfo<DomNodeQueryable>());
		EntitySchema.MaterialInstanceType.Type.Define(new ExtensionInfo<MaterialDocument>());
		EntitySchema.MaterialInstanceType.Type.Define(new ExtensionInfo<BaseEntityPropertyContext>());
		EntitySchema.MaterialInstanceType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
		EntitySchema.MaterialInstanceType.Type.Define(new ExtensionInfo<ReferenceValidator>());
		EntitySchema.MaterialInstanceType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
		EntitySchema.MaterialInstanceType.Type.Define(new ExtensionInfo<DomNodeQueryable>());
		EntitySchema.FireFXInstanceType.Type.Define(new ExtensionInfo<FireFXDocument>());
		EntitySchema.FireFXInstanceType.Type.Define(new ExtensionInfo<FireFXContext>());
		EntitySchema.FireFXInstanceType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
		EntitySchema.FireFXInstanceType.Type.Define(new ExtensionInfo<ReferenceValidator>());
		EntitySchema.FireFXInstanceType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
		EntitySchema.FireFXInstanceType.Type.Define(new ExtensionInfo<DomNodeQueryable>());
		EntitySchema.AnimationEntityType.Type.Define(new ExtensionInfo<AnimationDocument>());
		EntitySchema.AnimationEntityType.Type.Define(new ExtensionInfo<BaseEntityPropertyContext>());
		EntitySchema.AnimationEntityType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
		EntitySchema.AnimationEntityType.Type.Define(new ExtensionInfo<ReferenceValidator>());
		EntitySchema.AnimationEntityType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
		EntitySchema.AnimationEntityType.Type.Define(new ExtensionInfo<DomNodeQueryable>());
		EntitySchema.AnalyticLightEntityType.Type.Define(new ExtensionInfo<AnalyticLightDocument>());
		EntitySchema.AnalyticLightEntityType.Type.Define(new ExtensionInfo<BaseEntityPropertyContext>());
		EntitySchema.AnalyticLightEntityType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
		EntitySchema.AnalyticLightEntityType.Type.Define(new ExtensionInfo<ReferenceValidator>());
		EntitySchema.AnalyticLightEntityType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
		EntitySchema.AnalyticLightEntityType.Type.Define(new ExtensionInfo<DomNodeQueryable>());
		EntitySchema.EnvironmentLightEntityType.Type.Define(new ExtensionInfo<EnvironmentLightDocument>());
		EntitySchema.EnvironmentLightEntityType.Type.Define(new ExtensionInfo<EnvironmentLightContext>());
		EntitySchema.EnvironmentLightEntityType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
		EntitySchema.EnvironmentLightEntityType.Type.Define(new ExtensionInfo<ReferenceValidator>());
		EntitySchema.EnvironmentLightEntityType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
		EntitySchema.EnvironmentLightEntityType.Type.Define(new ExtensionInfo<DomNodeQueryable>());
		EntitySchema.ParticleEffectEntityType.Type.Define(new ExtensionInfo<ParticleEffectDocument>());
		EntitySchema.ParticleEffectEntityType.Type.Define(new ExtensionInfo<BaseEntityPropertyContext>());
		EntitySchema.ParticleEffectEntityType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
		EntitySchema.ParticleEffectEntityType.Type.Define(new ExtensionInfo<ReferenceValidator>());
		EntitySchema.ParticleEffectEntityType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
		EntitySchema.ParticleEffectEntityType.Type.Define(new ExtensionInfo<DomNodeQueryable>());
		EntitySchema.GeometryEntityType.Type.Define(new ExtensionInfo<GeometryDocument>());
		EntitySchema.GeometryEntityType.Type.Define(new ExtensionInfo<BaseEntityPropertyContext>());
		EntitySchema.GeometryEntityType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
		EntitySchema.GeometryEntityType.Type.Define(new ExtensionInfo<ReferenceValidator>());
		EntitySchema.GeometryEntityType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
		EntitySchema.GeometryEntityType.Type.Define(new ExtensionInfo<DomNodeQueryable>());
		EntitySchema.EntitySourceFilePathType.Type.Define(new ExtensionInfo<EntitySourceFilePathAdapter>());
		DefineSplineAdapters();
		DefineAttachmentPointAdapters();
		DefineMaterialInstanceAdapters();
		DefineFireFXInstanceAdapters();
		DefineAnimationInstanceAdapters();
		DefineGeometryInstanceAdapters();
		DefineTextureInstanceAdapters();
		DefineAnalyticLightInstanceAdapters();
		DefineEnvironmentLightInstanceAdapters();
		DefineParticleEffectInstanceAdapters();
		DefineTimelineAdapters();
		DefineAssetInstanceAdapters();
		DefineLightRigInstanceAdapters();
		DefineBehaviorInstanceAdapters();
	}

	private void DefineAnalyticLightInstanceAdapters()
	{
		EntitySchema.AnalyticLightEntityType.Type.Define(new ExtensionInfo<AnalyticLightAdapter>());
	}

	private void DefineAnimationInstanceAdapters()
	{
		EntitySchema.AnimationEntityType.Type.Define(new ExtensionInfo<AnimationAdapter>());
	}

	private void DefineTimelineAdapters()
	{
		EntitySchema.LegacyTriggerType.Type.Define(new ExtensionInfo<LegacyTriggerAdapter>());
		EntitySchema.TrackType.Type.Define(new ExtensionInfo<TrackAdapter>());
		EntitySchema.TriggerType.Type.Define(new ExtensionInfo<TriggerAdapter>());
		EntitySchema.SoundTriggerType.Type.Define(new ExtensionInfo<SoundTriggerAdapter>());
		EntitySchema.ActionTriggerType.Type.Define(new ExtensionInfo<ActionTriggerAdapter>());
		EntitySchema.TransferTriggerType.Type.Define(new ExtensionInfo<TransferTriggerAdapter>());
		EntitySchema.AssetFXTriggerType.Type.Define(new ExtensionInfo<AssetFXTriggerAdapter>());
		EntitySchema.ArtDefFXTriggerType.Type.Define(new ExtensionInfo<ArtDefFXTriggerAdapter>());
		EntitySchema.LightTriggerType.Type.Define(new ExtensionInfo<LightTriggerAdapter>());
	}

	private void DefineAssetInstanceAdapters()
	{
		EntitySchema.AssetType.Type.Define(new ExtensionInfo<AssetDocument>());
		EntitySchema.AssetType.Type.Define(new ExtensionInfo<AssetContext>());
		EntitySchema.AssetType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
		EntitySchema.AssetType.Type.Define(new ExtensionInfo<AssetAdapter>());
		EntitySchema.TimelineType.Type.Define(new ExtensionInfo<TimelineAdapter>());
		EntitySchema.TimelineBindingType.Type.Define(new ExtensionInfo<TimelineBindingAdapter>());
		EntitySchema.AnimationBindingType.Type.Define(new ExtensionInfo<AnimationBindingAdapter>());
		EntitySchema.ParticleEffectReferenceType.Type.Define(new ExtensionInfo<ParticleEffectReferenceAdapter>());
		EntitySchema.PrimGroupStateType.Type.Define(new ExtensionInfo<PrimGroupStateAdapter>());
		EntitySchema.ModelInstanceType.Type.Define(new ExtensionInfo<ModelInstanceAdapter>());
		EntitySchema.AnimationBindingSetType.Type.Define(new ExtensionInfo<AnimationBindingSetAdapter>());
		EntitySchema.TimelineSetType.Type.Define(new ExtensionInfo<TimelineSetAdapter>());
		EntitySchema.TimelineBindingSetType.Type.Define(new ExtensionInfo<TimelineBindingSetAdapter>());
		EntitySchema.GeometrySetType.Type.Define(new ExtensionInfo<GeometrySetAdapter>());
		EntitySchema.ParticleEffectSetType.Type.Define(new ExtensionInfo<ParticleEffectSetAdapter>());
		EntitySchema.CookParametersSetType.Type.Define(new ExtensionInfo<CookParameterSetAdapter>());
	}

	private void DefineSplineAdapters()
	{
		EntitySchema.SplineSetType.Type.Define(new ExtensionInfo<SplineSetAdapter>());
		EntitySchema.SplineVertexSetType.Type.Define(new ExtensionInfo<SplineVertexSetAdapter>());
		EntitySchema.SplineVertexType.Type.Define(new ExtensionInfo<SplineVertexAdapter>());
		EntitySchema.SplineType.Type.Define(new ExtensionInfo<SplineAdapter>());
	}

	private void DefineAttachmentPointAdapters()
	{
		EntitySchema.AttachmentPointSetType.Type.Define(new ExtensionInfo<AttachmentPointSetAdapter>());
		EntitySchema.AttachmentPointType.Type.Define(new ExtensionInfo<AttachmentPointAdapter>());
	}

	private void DefineBehaviorInstanceAdapters()
	{
		EntitySchema.BehaviorType.Type.Define(new ExtensionInfo<BehaviorDocument>());
		EntitySchema.BehaviorType.Type.Define(new ExtensionInfo<BehaviorContext>());
		EntitySchema.BehaviorType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
		EntitySchema.BehaviorType.Type.Define(new ExtensionInfo<ReferenceValidator>());
		EntitySchema.BehaviorType.Type.Define(new ExtensionInfo<UniqueIdValidator>());
		EntitySchema.BehaviorType.Type.Define(new ExtensionInfo<DomNodeQueryable>());
		EntitySchema.BehaviorType.Type.Define(new ExtensionInfo<BehaviorAdapter>());
		EntitySchema.BehaviorReferenceType.Type.Define(new ExtensionInfo<BehaviorReferenceAdapter>());
		EntitySchema.BehaviorSetType.Type.Define(new ExtensionInfo<BehaviorSetAdapter>());
		EntitySchema.GeometryReferenceType.Type.Define(new ExtensionInfo<GeometryReferenceAdapter>());
		EntitySchema.GeometryReferenceSetType.Type.Define(new ExtensionInfo<GeometryReferenceSetAdapter>());
	}

	private void DefineEnvironmentLightInstanceAdapters()
	{
		EntitySchema.LightDirectionTagSetType.Type.Define(new ExtensionInfo<LightDirectionTagSetAdapter>());
		EntitySchema.EnvironmentLightEntityType.Type.Define(new ExtensionInfo<EnvironmentLightAdapter>());
		EntitySchema.LightDirectionTagType.Type.Define(new ExtensionInfo<LightDirectionTagAdapter>());
	}

	private void DefineGeometryInstanceAdapters()
	{
		EntitySchema.GeoModelType.Type.Define(new ExtensionInfo<GeometryModelAdapter>());
		EntitySchema.GeoMeshType.Type.Define(new ExtensionInfo<GeometryMeshAdapter>());
		EntitySchema.GeoPrimGroupType.Type.Define(new ExtensionInfo<GeometryPrimGroupAdapter>());
		EntitySchema.GeometryEntityType.Type.Define(new ExtensionInfo<GeometryAdapter>());
	}

	private void DefineLightRigInstanceAdapters()
	{
		EntitySchema.LightRigEntityType.Type.Define(new ExtensionInfo<LightRigDocument>());
		EntitySchema.LightRigEntityType.Type.Define(new ExtensionInfo<LightRigContext>());
		EntitySchema.LightRigEntityType.Type.Define(new ExtensionInfo<MultipleHistoryContext>());
		EntitySchema.LightRigEntityType.Type.Define(new ExtensionInfo<LightRigAdapter>());
		EntitySchema.LightSetType.Type.Define(new ExtensionInfo<LightSetAdapter>());
		EntitySchema.LightReferenceType.Type.Define(new ExtensionInfo<LightReferenceAdapter>());
	}

	private void DefineMaterialInstanceAdapters()
	{
		EntitySchema.MaterialInstanceType.Type.Define(new ExtensionInfo<MaterialInstanceAdapter>());
	}

	private void DefineFireFXInstanceAdapters()
	{
		EntitySchema.FireFXInstanceType.Type.Define(new ExtensionInfo<FireFXInstanceAdapter>());
	}

	private void DefineParticleEffectInstanceAdapters()
	{
		EntitySchema.ParticleEffectEntityType.Type.Define(new ExtensionInfo<ParticleEffectAdapter>());
	}

	private void DefineTextureInstanceAdapters()
	{
		EntitySchema.TextureEntityType.Type.Define(new ExtensionInfo<TextureEntityAdapter>());
		EntitySchema.TextureExportSettingsType.Type.Define(new ExtensionInfo<TextureExportSettingsAdapter>());
	}
}
