using System.ComponentModel.Composition;
using Firaxis.AssetEditing;
using Firaxis.ATF;
using Firaxis.CivTech;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetPreviewing;

[Export(typeof(IEntitySchemaExtender))]
[Export(typeof(EntitySchemaLoaderPreviewer))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class EntitySchemaLoaderPreviewer : IEntitySchemaExtender
{
	private ITimelinePlaybackService m_timelinePlaybackService;

	private IAnimationKnobService m_animationKnobService;

	private IAssetBrowserDialogService m_assetBrowserDialogService;

	private ICivTechService m_civTechService;

	private IFileDialogService m_fileDialogService;

	[ImportingConstructor]
	public EntitySchemaLoaderPreviewer(IEntitySchemaInitializer typeInitializer, IFileDialogService fileDialogService, IAssetBrowserDialogService assetBrowserDialogService, ICivTechService civTechSvc, ITimelinePlaybackService timelinePlaybackSvc, IAnimationKnobService animationKnobSvc)
	{
		m_fileDialogService = fileDialogService;
		m_assetBrowserDialogService = assetBrowserDialogService;
		m_civTechService = civTechSvc;
		m_timelinePlaybackService = timelinePlaybackSvc;
		m_animationKnobService = animationKnobSvc;
		DefineWidgetAdapters();
		DefineMaterialInstanceAdapters();
		DefineFireFXInstanceAdapters();
		DefineAnimationInstanceAdapters();
		DefineGeometryInstanceAdapters();
		DefineTextureInstanceAdapters();
		DefineAnalyticLightInstanceAdapters();
		DefineEnvironmentLightInstanceAdapters();
		DefineParticleEffectInstanceAdapters();
		DefineAssetInstanceAdapters();
		DefineLightRigInstanceAdapters();
		DefineBehaviorInstanceAdapters();
	}

	private void DefineAnalyticLightInstanceAdapters()
	{
		EntitySchema.AnalyticLightEntityType.Type.Define(new ExtensionInfo<EntityPreviewingSequencer>());
		EntitySchema.AnalyticLightEntityType.Type.Define(new ExtensionInfo<EntityPreviewComponent>());
	}

	private void DefineAnimationInstanceAdapters()
	{
		EntitySchema.AnimationEntityType.Type.Define(new ExtensionInfo<EntityPreviewingSequencer>());
		EntitySchema.AnimationEntityType.Type.Define(new ExtensionInfo<EntityPreviewComponent>());
	}

	private void DefineAssetInstanceAdapters()
	{
		EntitySchema.AssetType.Type.Define(new ExtensionInfo<EntityPreviewingSequencer>());
		EntitySchema.AssetType.Type.Define(new ExtensionInfo<EntityPreviewComponent>());
	}

	private void DefineWidgetAdapters()
	{
		EntitySchema.AttachmentPointSetType.Type.Define(new ExtensionInfo<AttachmentPointSetWidgetAdapter>());
		EntitySchema.SplineSetType.Type.Define(new ExtensionInfo<SplineSetWidgetAdapter>());
	}

	private void DefineBehaviorInstanceAdapters()
	{
		EntitySchema.BehaviorType.Type.Define(new ExtensionInfo<EntityPreviewingSequencer>());
		EntitySchema.BehaviorType.Type.Define(new ExtensionInfo<EntityPreviewComponent>());
	}

	private void DefineEnvironmentLightInstanceAdapters()
	{
		EntitySchema.EnvironmentLightEntityType.Type.Define(new ExtensionInfo<EntityPreviewingSequencer>());
		EntitySchema.EnvironmentLightEntityType.Type.Define(new ExtensionInfo<EntityPreviewComponent>());
	}

	private void DefineGeometryInstanceAdapters()
	{
		EntitySchema.GeometryEntityType.Type.Define(new ExtensionInfo<EntityPreviewingSequencer>());
		EntitySchema.GeometryEntityType.Type.Define(new ExtensionInfo<EntityPreviewComponent>());
	}

	private void DefineLightRigInstanceAdapters()
	{
		EntitySchema.LightRigEntityType.Type.Define(new ExtensionInfo<EntityPreviewingSequencer>());
		EntitySchema.LightRigEntityType.Type.Define(new ExtensionInfo<EntityPreviewComponent>());
	}

	private void DefineMaterialInstanceAdapters()
	{
		EntitySchema.MaterialInstanceType.Type.Define(new ExtensionInfo<EntityPreviewingSequencer>());
		EntitySchema.MaterialInstanceType.Type.Define(new ExtensionInfo<EntityPreviewComponent>());
	}

	private void DefineFireFXInstanceAdapters()
	{
		EntitySchema.FireFXInstanceType.Type.Define(new ExtensionInfo<EntityPreviewingSequencer>());
		EntitySchema.FireFXInstanceType.Type.Define(new ExtensionInfo<EntityPreviewComponent>());
	}

	private void DefineParticleEffectInstanceAdapters()
	{
		EntitySchema.ParticleEffectEntityType.Type.Define(new ExtensionInfo<EntityPreviewingSequencer>());
		EntitySchema.ParticleEffectEntityType.Type.Define(new ExtensionInfo<EntityPreviewComponent>());
	}

	private void DefineTextureInstanceAdapters()
	{
		EntitySchema.TextureEntityType.Type.Define(new ExtensionInfo<EntityPreviewingSequencer>());
		EntitySchema.TextureEntityType.Type.Define(new ExtensionInfo<EntityPreviewComponent>());
	}
}
