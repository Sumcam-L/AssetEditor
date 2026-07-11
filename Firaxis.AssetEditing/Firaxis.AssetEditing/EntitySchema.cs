using System;
using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public static class EntitySchema
{
	public static class EntitySourceFilePathType
	{
		public static DomNodeType Type;

		public static AttributeInfo PathAttribute;
	}

	public static class MaterialInstanceType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static AttributeInfo ClassNameAttribute;

		public static ChildInfo TagsChild;

		public static ChildInfo CookParametersChild;

		public static ChildInfo DataFilesChild;
	}

	public static class FireFXInstanceType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static AttributeInfo ClassNameAttribute;

		public static ChildInfo TagsChild;

		public static ChildInfo CookParametersChild;

		public static ChildInfo DataFilesChild;

		public static AttributeInfo ScriptTextAttribute;
	}

	public static class TextureEntityType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static AttributeInfo ClassNameAttribute;

		public static AttributeInfo SourceObjectNameAttribute;

		public static AttributeInfo ImportedTimeAttribute;

		public static AttributeInfo ExportedTimeAttribute;

		public static AttributeInfo HeightAttribute;

		public static AttributeInfo WidthAttribute;

		public static AttributeInfo DepthAttribute;

		public static AttributeInfo NumMipsAttribute;

		public static ChildInfo TagsChild;

		public static ChildInfo CookParametersChild;

		public static ChildInfo DataFilesChild;

		public static ChildInfo SourceFilePathChild;

		public static ChildInfo ExportSettingsChild;
	}

	public static class TextureExportSettingsType
	{
		public static DomNodeType Type;

		public static AttributeInfo PixelFormatAttribute;

		public static AttributeInfo FilterTypeAttribute;

		public static AttributeInfo UseMipsAttribute;

		public static AttributeInfo NumManualMipsAttribute;

		public static AttributeInfo CompleteMipChainAttribute;

		public static AttributeInfo ValueClampMinAttribute;

		public static AttributeInfo ValueClampMaxAttribute;

		public static AttributeInfo SupportScaleAttribute;

		public static AttributeInfo GammaInAttribute;

		public static AttributeInfo GammaOutAttribute;

		public static AttributeInfo SlabWidthAttribute;

		public static AttributeInfo SlabHeightAttribute;

		public static AttributeInfo ColorKeyXAttribute;

		public static AttributeInfo ColorKeyYAttribute;

		public static AttributeInfo ColorKeyZAttribute;

		public static AttributeInfo ExportModeAttribute;

		public static AttributeInfo SampleFromTopLevelAttribute;
	}

	public static class AnimationEntityType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static AttributeInfo ClassNameAttribute;

		public static AttributeInfo SourceObjectNameAttribute;

		public static AttributeInfo ImportedTimeAttribute;

		public static AttributeInfo ExportedTimeAttribute;

		public static ChildInfo TagsChild;

		public static ChildInfo CookParametersChild;

		public static ChildInfo DataFilesChild;

		public static ChildInfo SourceFilePathChild;
	}

	public static class GeometryEntityType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static AttributeInfo ClassNameAttribute;

		public static AttributeInfo SourceObjectNameAttribute;

		public static AttributeInfo ImportedTimeAttribute;

		public static AttributeInfo ExportedTimeAttribute;

		public static AttributeInfo VertexCountAttribute;

		public static AttributeInfo PrimitiveCountAttribute;

		public static AttributeInfo BoneCountAttribute;

		public static ChildInfo TagsChild;

		public static ChildInfo CookParametersChild;

		public static ChildInfo DataFilesChild;

		public static ChildInfo SourceFilePathChild;

		public static ChildInfo GeoModelTypeChild;
	}

	public static class GeoModelType
	{
		public static DomNodeType Type;

		public static ChildInfo GeometryMeshesChild;

		public static ChildInfo BonesChild;
	}

	public static class GeoMeshType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo BoundBoneCountAttribute;

		public static AttributeInfo PrimitiveCountAttribute;

		public static AttributeInfo VertexCountAttribute;

		public static ChildInfo PrimGroupsChild;
	}

	public static class GeoPrimGroupType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo FirstPrimIndexAttribute;

		public static AttributeInfo PrimCountAttribute;
	}

	public static class AnalyticLightEntityType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static AttributeInfo ClassNameAttribute;

		public static AttributeInfo SourceObjectNameAttribute;

		public static AttributeInfo ImportedTimeAttribute;

		public static AttributeInfo ExportedTimeAttribute;

		public static ChildInfo TagsChild;

		public static ChildInfo CookParametersChild;

		public static ChildInfo DataFilesChild;

		public static ChildInfo SourceFilePathChild;
	}

	public static class ParticleEffectEntityType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static AttributeInfo ClassNameAttribute;

		public static AttributeInfo SourceObjectNameAttribute;

		public static AttributeInfo ImportedTimeAttribute;

		public static AttributeInfo ExportedTimeAttribute;

		public static ChildInfo TagsChild;

		public static ChildInfo CookParametersChild;

		public static ChildInfo DataFilesChild;

		public static ChildInfo SourceFilePathChild;
	}

	public static class DSGEntityType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static AttributeInfo ClassNameAttribute;

		public static ChildInfo TagsChild;

		public static ChildInfo CookParametersChild;

		public static ChildInfo DataFilesChild;

		public static ChildInfo AnimationSlotsChild;

		public static ChildInfo TimelineSlotsChild;
	}

	public static class AnimationBindingSetType
	{
		public static DomNodeType Type;

		public static ChildInfo AnimationBindingChild;
	}

	public static class AttachmentPointSetType
	{
		public static DomNodeType Type;

		public static ChildInfo AttachmentPointChild;
	}

	public static class SplineSetType
	{
		public static DomNodeType Type;

		public static ChildInfo SplineChild;
	}

	public static class TimelineSetType
	{
		public static DomNodeType Type;

		public static ChildInfo TimelineChild;
	}

	public static class TimelineBindingSetType
	{
		public static DomNodeType Type;

		public static ChildInfo TimelineBindingChild;
	}

	public static class GeometrySetType
	{
		public static DomNodeType Type;

		public static ChildInfo ModelInstanceChild;
	}

	public static class ParticleEffectSetType
	{
		public static DomNodeType Type;

		public static ChildInfo ParticleEffectChild;
	}

	public static class CookParametersSetType
	{
		public static DomNodeType Type;

		public static ChildInfo CookParameterChild;
	}

	public static class BehaviorSetType
	{
		public static DomNodeType Type;

		public static ChildInfo BehaviorChild;
	}

	public static class GeometryReferenceSetType
	{
		public static DomNodeType Type;

		public static ChildInfo GeometryReferencesChild;
	}

	public static class LightDirectionTagSetType
	{
		public static DomNodeType Type;

		public static ChildInfo LightDirectionTagsChild;
	}

	public static class LightSetType
	{
		public static DomNodeType Type;

		public static ChildInfo LightChild;
	}

	public static class AssetType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static AttributeInfo ClassNameAttribute;

		public static AttributeInfo DSGAttribute;

		public static ChildInfo TagsChild;

		public static ChildInfo CookParameterSetChild;

		public static ChildInfo DataFilesChild;

		public static ChildInfo BehaviorSetChild;

		public static ChildInfo SplineSetChild;

		public static ChildInfo TimelineSetChild;

		public static ChildInfo AnimationBindingSetChild;

		public static ChildInfo TimelineBindingSetChild;

		public static ChildInfo GeometrySetChild;

		public static ChildInfo AttachmentPointSetChild;

		public static ChildInfo ParticleEffectsSetChild;
	}

	public static class BehaviorType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static AttributeInfo ClassNameAttribute;

		public static AttributeInfo DSGAttribute;

		public static ChildInfo TagsChild;

		public static ChildInfo CookParameterSetChild;

		public static ChildInfo DataFilesChild;

		public static ChildInfo BehaviorSetChild;

		public static ChildInfo GeometryReferenceSetChild;

		public static ChildInfo TimelineSetChild;

		public static ChildInfo AnimationBindingSetChild;

		public static ChildInfo TimelineBindingSetChild;

		public static ChildInfo AttachmentPointSetChild;
	}

	public static class TimelineType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static AttributeInfo AnimationNameAttribute;

		public static AttributeInfo DurationAttribute;

		public static ChildInfo TriggersChild;

		public static ChildInfo TracksChild;

		public static ChildInfo LegacyTriggersChild;
	}

	public static class TrackType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo TriggerTypeAttribute;

		public static AttributeInfo IndexAttribute;

		public static ChildInfo TriggersChild;
	}

	public static class TriggerType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo StartTimeAttribute;

		public static AttributeInfo TrackIndexAttribute;
	}

	public static class SoundTriggerType
	{
		public static DomNodeType Type;

		public static AttributeInfo AudioEventAttribute;

		public static AttributeInfo AttachmentPointNameAttribute;
	}

	public static class ActionTriggerType
	{
		public static DomNodeType Type;

		public static AttributeInfo ActionAttribute;

		public static AttributeInfo AttachmentPointNameAttribute;
	}

	public static class AssetFXTriggerType
	{
		public static DomNodeType Type;

		public static AttributeInfo VFXAssetAttribute;

		public static AttributeInfo AttachmentPointNameAttribute;

		public static AttributeInfo DurationAttribute;
	}

	public static class ArtDefFXTriggerType
	{
		public static DomNodeType Type;

		public static AttributeInfo VFXElementAttribute;

		public static AttributeInfo VFXCollectionAttribute;

		public static AttributeInfo AttachmentPointNameAttribute;

		public static AttributeInfo DurationAttribute;
	}

	public static class LightTriggerType
	{
		public static DomNodeType Type;

		public static AttributeInfo LightAssetAttribute;

		public static AttributeInfo AttachmentPointNameAttribute;

		public static AttributeInfo DurationAttribute;
	}

	public static class TransferTriggerType
	{
		public static DomNodeType Type;

		public static AttributeInfo TargetTriggerAttribute;
	}

	public static class LegacyTriggerType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo FXNameAttribute;

		public static AttributeInfo AttachmentPointNameAttribute;

		public static AttributeInfo StartTimeAttribute;

		public static AttributeInfo DurationAttribute;

		public static AttributeInfo CollectionNameAttribute;

		public static AttributeInfo TrackIndexAttribute;
	}

	public static class AnimationBindingType
	{
		public static DomNodeType Type;

		public static AttributeInfo SlotNameAttribute;

		public static AttributeInfo AnimationNameAttribute;
	}

	public static class ParticleEffectReferenceType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;
	}

	public static class BehaviorReferenceType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;
	}

	public static class GeometryReferenceType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;
	}

	public static class LightReferenceType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;
	}

	public static class TimelineBindingType
	{
		public static DomNodeType Type;

		public static AttributeInfo SlotNameAttribute;

		public static AttributeInfo TimelineNameAttribute;

		public static AttributeInfo DurationAttribute;
	}

	public static class ModelInstanceType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo GeoNameAttribute;

		public static AttributeInfo VertexCountAttribute;

		public static AttributeInfo PrimitiveCountAttribute;

		public static AttributeInfo BoneCountAttribute;

		public static ChildInfo PrimGroupStatesChild;
	}

	public static class PrimGroupStateType
	{
		public static DomNodeType Type;

		public static AttributeInfo MeshNameAttribute;

		public static AttributeInfo GroupNameAttribute;

		public static AttributeInfo StateNameAttribute;

		public static ChildInfo CookParametersChild;
	}

	public static class FloatVector3Type
	{
		public static DomNodeType Type;
	}

	public static class AttachmentPointType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo BoneNameAttribute;

		public static AttributeInfo ModelInstanceNameAttribute;

		public static AttributeInfo PositionAttribute;

		public static AttributeInfo OrientationAttribute;

		public static AttributeInfo ScaleAttribute;

		public static ChildInfo CookParametersChild;
	}

	public static class SplineType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo ClassNameAttribute;

		public static AttributeInfo ClosedLoopAttribute;

		public static ChildInfo VerticesChild;

		public static ChildInfo CookParametersChild;
	}

	public static class SplineVertexType
	{
		public static DomNodeType Type;

		public static AttributeInfo PositionAttribute;

		public static AttributeInfo LeftUserTangentAttribute;

		public static AttributeInfo RightUserTangentAttribute;

		public static AttributeInfo SharpCornerAttribute;

		public static AttributeInfo UseUserTangentsAttribute;
	}

	public static class SplineVertexSetType
	{
		public static DomNodeType Type;

		public static ChildInfo SplineVertexChild;
	}

	public static class EnvironmentLightEntityType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static AttributeInfo ClassNameAttribute;

		public static AttributeInfo SourceObjectNameAttribute;

		public static AttributeInfo ImportedTimeAttribute;

		public static AttributeInfo ExportedTimeAttribute;

		public static ChildInfo TagsChild;

		public static ChildInfo CookParametersChild;

		public static ChildInfo DataFilesChild;

		public static ChildInfo SourceFilePathChild;

		public static ChildInfo LightDirectionTagsChild;
	}

	public static class LightDirectionTagType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo UAttribute;

		public static AttributeInfo VAttribute;

		public static AttributeInfo LightIntensityAttribute;

		public static AttributeInfo LightColorAttribute;

		public static AttributeInfo CastsShadowsAttribute;

		public static AttributeInfo XPositionAttribute;

		public static AttributeInfo YPositionAttribute;

		public static AttributeInfo ZPositionAttribute;

		public static AttributeInfo DiameterAttribute;

		public static AttributeInfo AngularFalloffAttribute;

		public static AttributeInfo RedAttribute;

		public static AttributeInfo GreenAttribute;

		public static AttributeInfo BlueAttribute;
	}

	public static class LightRigEntityType
	{
		public static DomNodeType Type;

		public static AttributeInfo NameAttribute;

		public static AttributeInfo DescriptionAttribute;

		public static AttributeInfo ClassNameAttribute;

		public static AttributeInfo DSGAttribute;

		public static ChildInfo TagsChild;

		public static ChildInfo CookParametersChild;

		public static ChildInfo DataFilesChild;

		public static ChildInfo AnimationBindingSetChild;

		public static ChildInfo EnvironmentLightSetChild;

		public static ChildInfo AnalyticLightSetChild;
	}

	public static ChildInfo MaterialInstanceRootElement;

	public static ChildInfo TextureEntityRootElement;

	public static ChildInfo AnimationEntityRootElement;

	public static ChildInfo GeometryEntityRootElement;

	public static ChildInfo AnalyticLightEntityRootElement;

	public static ChildInfo ParticleEffectEntityRootElement;

	public static ChildInfo DSGEntityRootElement;

	public static ChildInfo AssetEntityRootElement;

	public static ChildInfo BehaviorEntityRootElement;

	public static ChildInfo EnvironmentLightEntityRootElement;

	public static ChildInfo LightRigEntityRootElement;

	public static ChildInfo FireFXInstanceRootElement;

	public const string NS = "Entities";

	public static void Initialize(XmlSchemaTypeCollection typeCollection)
	{
		Initialize((string ns, string name) => typeCollection.GetNodeType(ns, name), (string ns, string name) => typeCollection.GetRootElement(ns, name));
	}

	public static void Initialize(IDictionary<string, XmlSchemaTypeCollection> typeCollections)
	{
		Initialize((string ns, string name) => typeCollections[ns].GetNodeType(name), (string ns, string name) => typeCollections[ns].GetRootElement(name));
	}

	private static void Initialize(Func<string, string, DomNodeType> getNodeType, Func<string, string, ChildInfo> getRootElement)
	{
		FloatVector3Type.Type = getNodeType("Entities", "FloatVector3Type");
		EntitySourceFilePathType.Type = getNodeType("Entities", "EntitySourceFilePathType");
		EntitySourceFilePathType.PathAttribute = EntitySourceFilePathType.Type.GetAttributeInfo("Path");
		MaterialInstanceType.Type = getNodeType("Entities", "MaterialInstanceType");
		MaterialInstanceType.NameAttribute = MaterialInstanceType.Type.GetAttributeInfo("Name");
		MaterialInstanceType.DescriptionAttribute = MaterialInstanceType.Type.GetAttributeInfo("Description");
		MaterialInstanceType.ClassNameAttribute = MaterialInstanceType.Type.GetAttributeInfo("ClassName");
		MaterialInstanceType.TagsChild = MaterialInstanceType.Type.GetChildInfo("Tags");
		MaterialInstanceType.CookParametersChild = MaterialInstanceType.Type.GetChildInfo("CookParameters");
		MaterialInstanceType.DataFilesChild = MaterialInstanceType.Type.GetChildInfo("DataFiles");
		FireFXInstanceType.Type = getNodeType("Entities", "FireFXInstanceType");
		FireFXInstanceType.NameAttribute = FireFXInstanceType.Type.GetAttributeInfo("Name");
		FireFXInstanceType.DescriptionAttribute = FireFXInstanceType.Type.GetAttributeInfo("Description");
		FireFXInstanceType.ClassNameAttribute = FireFXInstanceType.Type.GetAttributeInfo("ClassName");
		FireFXInstanceType.TagsChild = FireFXInstanceType.Type.GetChildInfo("Tags");
		FireFXInstanceType.CookParametersChild = FireFXInstanceType.Type.GetChildInfo("CookParameters");
		FireFXInstanceType.DataFilesChild = FireFXInstanceType.Type.GetChildInfo("DataFiles");
		FireFXInstanceType.ScriptTextAttribute = FireFXInstanceType.Type.GetAttributeInfo("ScriptText");
		TextureEntityType.Type = getNodeType("Entities", "TextureEntityType");
		TextureEntityType.NameAttribute = TextureEntityType.Type.GetAttributeInfo("Name");
		TextureEntityType.DescriptionAttribute = TextureEntityType.Type.GetAttributeInfo("Description");
		TextureEntityType.ClassNameAttribute = TextureEntityType.Type.GetAttributeInfo("ClassName");
		TextureEntityType.SourceObjectNameAttribute = TextureEntityType.Type.GetAttributeInfo("SourceObjectName");
		TextureEntityType.ImportedTimeAttribute = TextureEntityType.Type.GetAttributeInfo("ImportedTime");
		TextureEntityType.ExportedTimeAttribute = TextureEntityType.Type.GetAttributeInfo("ExportedTime");
		TextureEntityType.HeightAttribute = TextureEntityType.Type.GetAttributeInfo("Height");
		TextureEntityType.WidthAttribute = TextureEntityType.Type.GetAttributeInfo("Width");
		TextureEntityType.DepthAttribute = TextureEntityType.Type.GetAttributeInfo("Depth");
		TextureEntityType.NumMipsAttribute = TextureEntityType.Type.GetAttributeInfo("NumMips");
		TextureEntityType.TagsChild = TextureEntityType.Type.GetChildInfo("Tags");
		TextureEntityType.CookParametersChild = TextureEntityType.Type.GetChildInfo("CookParameters");
		TextureEntityType.DataFilesChild = TextureEntityType.Type.GetChildInfo("DataFiles");
		TextureEntityType.SourceFilePathChild = TextureEntityType.Type.GetChildInfo("SourceFilePath");
		TextureEntityType.ExportSettingsChild = TextureEntityType.Type.GetChildInfo("ExportSettings");
		TextureExportSettingsType.Type = getNodeType("Entities", "TextureExportSettingsType");
		TextureExportSettingsType.PixelFormatAttribute = TextureExportSettingsType.Type.GetAttributeInfo("PixelFormat");
		TextureExportSettingsType.FilterTypeAttribute = TextureExportSettingsType.Type.GetAttributeInfo("FilterType");
		TextureExportSettingsType.UseMipsAttribute = TextureExportSettingsType.Type.GetAttributeInfo("UseMips");
		TextureExportSettingsType.NumManualMipsAttribute = TextureExportSettingsType.Type.GetAttributeInfo("NumManualMips");
		TextureExportSettingsType.CompleteMipChainAttribute = TextureExportSettingsType.Type.GetAttributeInfo("CompleteMipChain");
		TextureExportSettingsType.ValueClampMinAttribute = TextureExportSettingsType.Type.GetAttributeInfo("ValueClampMin");
		TextureExportSettingsType.ValueClampMaxAttribute = TextureExportSettingsType.Type.GetAttributeInfo("ValueClampMax");
		TextureExportSettingsType.SupportScaleAttribute = TextureExportSettingsType.Type.GetAttributeInfo("SupportScale");
		TextureExportSettingsType.GammaInAttribute = TextureExportSettingsType.Type.GetAttributeInfo("GammaIn");
		TextureExportSettingsType.GammaOutAttribute = TextureExportSettingsType.Type.GetAttributeInfo("GammaOut");
		TextureExportSettingsType.SlabWidthAttribute = TextureExportSettingsType.Type.GetAttributeInfo("SlabWidth");
		TextureExportSettingsType.SlabHeightAttribute = TextureExportSettingsType.Type.GetAttributeInfo("SlabHeight");
		TextureExportSettingsType.ColorKeyXAttribute = TextureExportSettingsType.Type.GetAttributeInfo("ColorKeyX");
		TextureExportSettingsType.ColorKeyYAttribute = TextureExportSettingsType.Type.GetAttributeInfo("ColorKeyY");
		TextureExportSettingsType.ColorKeyZAttribute = TextureExportSettingsType.Type.GetAttributeInfo("ColorKeyZ");
		TextureExportSettingsType.ExportModeAttribute = TextureExportSettingsType.Type.GetAttributeInfo("ExportMode");
		TextureExportSettingsType.SampleFromTopLevelAttribute = TextureExportSettingsType.Type.GetAttributeInfo("SampleFromTopLevel");
		AnimationEntityType.Type = getNodeType("Entities", "AnimationEntityType");
		AnimationEntityType.NameAttribute = AnimationEntityType.Type.GetAttributeInfo("Name");
		AnimationEntityType.DescriptionAttribute = AnimationEntityType.Type.GetAttributeInfo("Description");
		AnimationEntityType.ClassNameAttribute = AnimationEntityType.Type.GetAttributeInfo("ClassName");
		AnimationEntityType.SourceObjectNameAttribute = AnimationEntityType.Type.GetAttributeInfo("SourceObjectName");
		AnimationEntityType.ImportedTimeAttribute = AnimationEntityType.Type.GetAttributeInfo("ImportedTime");
		AnimationEntityType.ExportedTimeAttribute = AnimationEntityType.Type.GetAttributeInfo("ExportedTime");
		AnimationEntityType.TagsChild = AnimationEntityType.Type.GetChildInfo("Tags");
		AnimationEntityType.CookParametersChild = AnimationEntityType.Type.GetChildInfo("CookParameters");
		AnimationEntityType.DataFilesChild = AnimationEntityType.Type.GetChildInfo("DataFiles");
		AnimationEntityType.SourceFilePathChild = AnimationEntityType.Type.GetChildInfo("SourceFilePath");
		GeometryEntityType.Type = getNodeType("Entities", "GeometryEntityType");
		GeometryEntityType.NameAttribute = GeometryEntityType.Type.GetAttributeInfo("Name");
		GeometryEntityType.DescriptionAttribute = GeometryEntityType.Type.GetAttributeInfo("Description");
		GeometryEntityType.ClassNameAttribute = GeometryEntityType.Type.GetAttributeInfo("ClassName");
		GeometryEntityType.SourceObjectNameAttribute = GeometryEntityType.Type.GetAttributeInfo("SourceObjectName");
		GeometryEntityType.ImportedTimeAttribute = GeometryEntityType.Type.GetAttributeInfo("ImportedTime");
		GeometryEntityType.ExportedTimeAttribute = GeometryEntityType.Type.GetAttributeInfo("ExportedTime");
		GeometryEntityType.VertexCountAttribute = GeometryEntityType.Type.GetAttributeInfo("VertexCount");
		GeometryEntityType.PrimitiveCountAttribute = GeometryEntityType.Type.GetAttributeInfo("PrimitiveCount");
		GeometryEntityType.BoneCountAttribute = GeometryEntityType.Type.GetAttributeInfo("BoneCount");
		GeometryEntityType.TagsChild = GeometryEntityType.Type.GetChildInfo("Tags");
		GeometryEntityType.CookParametersChild = GeometryEntityType.Type.GetChildInfo("CookParameters");
		GeometryEntityType.DataFilesChild = GeometryEntityType.Type.GetChildInfo("DataFiles");
		GeometryEntityType.SourceFilePathChild = GeometryEntityType.Type.GetChildInfo("SourceFilePath");
		GeometryEntityType.GeoModelTypeChild = GeometryEntityType.Type.GetChildInfo("GeoModelType");
		GeoModelType.Type = getNodeType("Entities", "GeoModelType");
		GeoModelType.GeometryMeshesChild = GeoModelType.Type.GetChildInfo("GeometryMeshes");
		GeoModelType.BonesChild = GeoModelType.Type.GetChildInfo("Bones");
		GeoMeshType.Type = getNodeType("Entities", "GeoMeshType");
		GeoMeshType.NameAttribute = GeoMeshType.Type.GetAttributeInfo("Name");
		GeoMeshType.BoundBoneCountAttribute = GeoMeshType.Type.GetAttributeInfo("BoundBoneCount");
		GeoMeshType.PrimitiveCountAttribute = GeoMeshType.Type.GetAttributeInfo("PrimitiveCount");
		GeoMeshType.VertexCountAttribute = GeoMeshType.Type.GetAttributeInfo("VertexCount");
		GeoMeshType.PrimGroupsChild = GeoMeshType.Type.GetChildInfo("PrimGroups");
		GeoPrimGroupType.Type = getNodeType("Entities", "GeoPrimGroupType");
		GeoPrimGroupType.NameAttribute = GeoPrimGroupType.Type.GetAttributeInfo("Name");
		GeoPrimGroupType.FirstPrimIndexAttribute = GeoPrimGroupType.Type.GetAttributeInfo("FirstPrimIndex");
		GeoPrimGroupType.PrimCountAttribute = GeoPrimGroupType.Type.GetAttributeInfo("PrimCount");
		AnalyticLightEntityType.Type = getNodeType("Entities", "AnalyticLightEntityType");
		AnalyticLightEntityType.NameAttribute = AnalyticLightEntityType.Type.GetAttributeInfo("Name");
		AnalyticLightEntityType.DescriptionAttribute = AnalyticLightEntityType.Type.GetAttributeInfo("Description");
		AnalyticLightEntityType.ClassNameAttribute = AnalyticLightEntityType.Type.GetAttributeInfo("ClassName");
		AnalyticLightEntityType.SourceObjectNameAttribute = AnalyticLightEntityType.Type.GetAttributeInfo("SourceObjectName");
		AnalyticLightEntityType.ImportedTimeAttribute = AnalyticLightEntityType.Type.GetAttributeInfo("ImportedTime");
		AnalyticLightEntityType.ExportedTimeAttribute = AnalyticLightEntityType.Type.GetAttributeInfo("ExportedTime");
		AnalyticLightEntityType.TagsChild = AnalyticLightEntityType.Type.GetChildInfo("Tags");
		AnalyticLightEntityType.CookParametersChild = AnalyticLightEntityType.Type.GetChildInfo("CookParameters");
		AnalyticLightEntityType.DataFilesChild = AnalyticLightEntityType.Type.GetChildInfo("DataFiles");
		AnalyticLightEntityType.SourceFilePathChild = AnalyticLightEntityType.Type.GetChildInfo("SourceFilePath");
		ParticleEffectEntityType.Type = getNodeType("Entities", "ParticleEffectEntityType");
		ParticleEffectEntityType.NameAttribute = ParticleEffectEntityType.Type.GetAttributeInfo("Name");
		ParticleEffectEntityType.DescriptionAttribute = ParticleEffectEntityType.Type.GetAttributeInfo("Description");
		ParticleEffectEntityType.ClassNameAttribute = ParticleEffectEntityType.Type.GetAttributeInfo("ClassName");
		ParticleEffectEntityType.SourceObjectNameAttribute = ParticleEffectEntityType.Type.GetAttributeInfo("SourceObjectName");
		ParticleEffectEntityType.ImportedTimeAttribute = ParticleEffectEntityType.Type.GetAttributeInfo("ImportedTime");
		ParticleEffectEntityType.ExportedTimeAttribute = ParticleEffectEntityType.Type.GetAttributeInfo("ExportedTime");
		ParticleEffectEntityType.TagsChild = ParticleEffectEntityType.Type.GetChildInfo("Tags");
		ParticleEffectEntityType.CookParametersChild = ParticleEffectEntityType.Type.GetChildInfo("CookParameters");
		ParticleEffectEntityType.DataFilesChild = ParticleEffectEntityType.Type.GetChildInfo("DataFiles");
		ParticleEffectEntityType.SourceFilePathChild = ParticleEffectEntityType.Type.GetChildInfo("SourceFilePath");
		DSGEntityType.Type = getNodeType("Entities", "DSGEntityType");
		DSGEntityType.NameAttribute = DSGEntityType.Type.GetAttributeInfo("Name");
		DSGEntityType.DescriptionAttribute = DSGEntityType.Type.GetAttributeInfo("Description");
		DSGEntityType.ClassNameAttribute = DSGEntityType.Type.GetAttributeInfo("ClassName");
		DSGEntityType.TagsChild = DSGEntityType.Type.GetChildInfo("Tags");
		DSGEntityType.CookParametersChild = DSGEntityType.Type.GetChildInfo("CookParameters");
		DSGEntityType.DataFilesChild = DSGEntityType.Type.GetChildInfo("DataFiles");
		DSGEntityType.AnimationSlotsChild = DSGEntityType.Type.GetChildInfo("AnimationSlots");
		DSGEntityType.TimelineSlotsChild = DSGEntityType.Type.GetChildInfo("TimelineSlots");
		AnimationBindingSetType.Type = getNodeType("Entities", "AnimationBindingSetType");
		AnimationBindingSetType.AnimationBindingChild = AnimationBindingSetType.Type.GetChildInfo("AnimationBinding");
		AttachmentPointSetType.Type = getNodeType("Entities", "AttachmentPointSetType");
		AttachmentPointSetType.AttachmentPointChild = AttachmentPointSetType.Type.GetChildInfo("AttachmentPoint");
		SplineSetType.Type = getNodeType("Entities", "SplineSetType");
		SplineSetType.SplineChild = SplineSetType.Type.GetChildInfo("Spline");
		TimelineSetType.Type = getNodeType("Entities", "TimelineSetType");
		TimelineSetType.TimelineChild = TimelineSetType.Type.GetChildInfo("Timeline");
		TimelineBindingSetType.Type = getNodeType("Entities", "TimelineBindingSetType");
		TimelineBindingSetType.TimelineBindingChild = TimelineBindingSetType.Type.GetChildInfo("TimelineBinding");
		GeometrySetType.Type = getNodeType("Entities", "GeometrySetType");
		GeometrySetType.ModelInstanceChild = GeometrySetType.Type.GetChildInfo("ModelInstance");
		ParticleEffectSetType.Type = getNodeType("Entities", "ParticleEffectSetType");
		ParticleEffectSetType.ParticleEffectChild = ParticleEffectSetType.Type.GetChildInfo("ParticleEffect");
		LightSetType.Type = getNodeType("Entities", "LightSetType");
		LightSetType.LightChild = LightSetType.Type.GetChildInfo("LightBinding");
		CookParametersSetType.Type = getNodeType("Entities", "CookParametersSetType");
		CookParametersSetType.CookParameterChild = CookParametersSetType.Type.GetChildInfo("CookParameter");
		SplineVertexSetType.Type = getNodeType("Entities", "SplineVertexSetType");
		SplineVertexSetType.SplineVertexChild = SplineVertexSetType.Type.GetChildInfo("SplineVertex");
		BehaviorSetType.Type = getNodeType("Entities", "BehaviorSetType");
		BehaviorSetType.BehaviorChild = BehaviorSetType.Type.GetChildInfo("Behavior");
		GeometryReferenceSetType.Type = getNodeType("Entities", "GeometryReferenceSetType");
		GeometryReferenceSetType.GeometryReferencesChild = GeometryReferenceSetType.Type.GetChildInfo("GeometryReferences");
		AssetType.Type = getNodeType("Entities", "AssetType");
		AssetType.NameAttribute = AssetType.Type.GetAttributeInfo("Name");
		AssetType.DescriptionAttribute = AssetType.Type.GetAttributeInfo("Description");
		AssetType.ClassNameAttribute = AssetType.Type.GetAttributeInfo("ClassName");
		AssetType.DSGAttribute = AssetType.Type.GetAttributeInfo("DSG");
		AssetType.TagsChild = AssetType.Type.GetChildInfo("Tags");
		AssetType.CookParameterSetChild = AssetType.Type.GetChildInfo("CookParameterSet");
		AssetType.DataFilesChild = AssetType.Type.GetChildInfo("DataFiles");
		AssetType.BehaviorSetChild = AssetType.Type.GetChildInfo("BehaviorSet");
		AssetType.SplineSetChild = AssetType.Type.GetChildInfo("SplineSet");
		AssetType.TimelineSetChild = AssetType.Type.GetChildInfo("TimelineSet");
		AssetType.AnimationBindingSetChild = AssetType.Type.GetChildInfo("AnimationBindingSet");
		AssetType.TimelineBindingSetChild = AssetType.Type.GetChildInfo("TimelineBindingSet");
		AssetType.GeometrySetChild = AssetType.Type.GetChildInfo("GeometrySet");
		AssetType.AttachmentPointSetChild = AssetType.Type.GetChildInfo("AttachmentPointSet");
		AssetType.ParticleEffectsSetChild = AssetType.Type.GetChildInfo("ParticleEffectSet");
		BehaviorType.Type = getNodeType("Entities", "BehaviorType");
		BehaviorType.NameAttribute = BehaviorType.Type.GetAttributeInfo("Name");
		BehaviorType.DescriptionAttribute = BehaviorType.Type.GetAttributeInfo("Description");
		BehaviorType.ClassNameAttribute = BehaviorType.Type.GetAttributeInfo("ClassName");
		BehaviorType.DSGAttribute = BehaviorType.Type.GetAttributeInfo("DSG");
		BehaviorType.TagsChild = BehaviorType.Type.GetChildInfo("Tags");
		BehaviorType.CookParameterSetChild = BehaviorType.Type.GetChildInfo("CookParameterSet");
		BehaviorType.DataFilesChild = BehaviorType.Type.GetChildInfo("DataFiles");
		BehaviorType.BehaviorSetChild = BehaviorType.Type.GetChildInfo("BehaviorSet");
		BehaviorType.GeometryReferenceSetChild = BehaviorType.Type.GetChildInfo("GeometryReferenceSet");
		BehaviorType.TimelineSetChild = BehaviorType.Type.GetChildInfo("TimelineSet");
		BehaviorType.AnimationBindingSetChild = BehaviorType.Type.GetChildInfo("AnimationBindingSet");
		BehaviorType.TimelineBindingSetChild = BehaviorType.Type.GetChildInfo("TimelineBindingSet");
		BehaviorType.AttachmentPointSetChild = BehaviorType.Type.GetChildInfo("AttachmentPointSet");
		TimelineType.Type = getNodeType("Entities", "TimelineType");
		TimelineType.NameAttribute = TimelineType.Type.GetAttributeInfo("Name");
		TimelineType.DescriptionAttribute = TimelineType.Type.GetAttributeInfo("Description");
		TimelineType.AnimationNameAttribute = TimelineType.Type.GetAttributeInfo("AnimationName");
		TimelineType.DurationAttribute = TimelineType.Type.GetAttributeInfo("Duration");
		TimelineType.TriggersChild = TimelineType.Type.GetChildInfo("Triggers");
		TimelineType.TracksChild = TimelineType.Type.GetChildInfo("Tracks");
		TimelineType.LegacyTriggersChild = TimelineType.Type.GetChildInfo("LegacyTriggers");
		TrackType.Type = getNodeType("Entities", "TrackType");
		TrackType.NameAttribute = TrackType.Type.GetAttributeInfo("Name");
		TrackType.TriggerTypeAttribute = TrackType.Type.GetAttributeInfo("TriggerType");
		TrackType.IndexAttribute = TrackType.Type.GetAttributeInfo("Index");
		LegacyTriggerType.Type = getNodeType("Entities", "LegacyTriggerType");
		LegacyTriggerType.NameAttribute = LegacyTriggerType.Type.GetAttributeInfo("Name");
		LegacyTriggerType.FXNameAttribute = LegacyTriggerType.Type.GetAttributeInfo("FXName");
		LegacyTriggerType.AttachmentPointNameAttribute = LegacyTriggerType.Type.GetAttributeInfo("AttachmentPointName");
		LegacyTriggerType.StartTimeAttribute = LegacyTriggerType.Type.GetAttributeInfo("StartTime");
		LegacyTriggerType.DurationAttribute = LegacyTriggerType.Type.GetAttributeInfo("Duration");
		LegacyTriggerType.CollectionNameAttribute = LegacyTriggerType.Type.GetAttributeInfo("CollectionName");
		LegacyTriggerType.TrackIndexAttribute = LegacyTriggerType.Type.GetAttributeInfo("TrackIndex");
		TriggerType.Type = getNodeType("Entities", "TriggerType");
		TriggerType.NameAttribute = TriggerType.Type.GetAttributeInfo("Name");
		TriggerType.StartTimeAttribute = TriggerType.Type.GetAttributeInfo("StartTime");
		TriggerType.TrackIndexAttribute = TriggerType.Type.GetAttributeInfo("TrackIndex");
		SoundTriggerType.Type = getNodeType("Entities", "SoundTriggerType");
		SoundTriggerType.AudioEventAttribute = SoundTriggerType.Type.GetAttributeInfo("AudioEvent");
		SoundTriggerType.AttachmentPointNameAttribute = SoundTriggerType.Type.GetAttributeInfo("AttachmentPointName");
		ActionTriggerType.Type = getNodeType("Entities", "ActionTriggerType");
		ActionTriggerType.ActionAttribute = ActionTriggerType.Type.GetAttributeInfo("Action");
		ActionTriggerType.AttachmentPointNameAttribute = ActionTriggerType.Type.GetAttributeInfo("AttachmentPointName");
		AssetFXTriggerType.Type = getNodeType("Entities", "AssetFXTriggerType");
		AssetFXTriggerType.VFXAssetAttribute = AssetFXTriggerType.Type.GetAttributeInfo("VFXAsset");
		AssetFXTriggerType.AttachmentPointNameAttribute = AssetFXTriggerType.Type.GetAttributeInfo("AttachmentPointName");
		AssetFXTriggerType.DurationAttribute = AssetFXTriggerType.Type.GetAttributeInfo("Duration");
		ArtDefFXTriggerType.Type = getNodeType("Entities", "ArtDefFXTriggerType");
		ArtDefFXTriggerType.VFXElementAttribute = ArtDefFXTriggerType.Type.GetAttributeInfo("VFXElement");
		ArtDefFXTriggerType.VFXCollectionAttribute = ArtDefFXTriggerType.Type.GetAttributeInfo("VFXCollection");
		ArtDefFXTriggerType.AttachmentPointNameAttribute = ArtDefFXTriggerType.Type.GetAttributeInfo("AttachmentPointName");
		ArtDefFXTriggerType.DurationAttribute = ArtDefFXTriggerType.Type.GetAttributeInfo("Duration");
		LightTriggerType.Type = getNodeType("Entities", "LightTriggerType");
		LightTriggerType.LightAssetAttribute = LightTriggerType.Type.GetAttributeInfo("LightAsset");
		LightTriggerType.AttachmentPointNameAttribute = LightTriggerType.Type.GetAttributeInfo("AttachmentPointName");
		LightTriggerType.DurationAttribute = LightTriggerType.Type.GetAttributeInfo("Duration");
		TransferTriggerType.Type = getNodeType("Entities", "TransferTriggerType");
		TransferTriggerType.TargetTriggerAttribute = TransferTriggerType.Type.GetAttributeInfo("TargetTrigger");
		TransferTriggerType.TargetTriggerAttribute.DefaultValue = "0";
		AnimationBindingType.Type = getNodeType("Entities", "AnimationBindingType");
		AnimationBindingType.SlotNameAttribute = AnimationBindingType.Type.GetAttributeInfo("SlotName");
		AnimationBindingType.AnimationNameAttribute = AnimationBindingType.Type.GetAttributeInfo("AnimationName");
		ParticleEffectReferenceType.Type = getNodeType("Entities", "ParticleEffectReferenceType");
		ParticleEffectReferenceType.NameAttribute = ParticleEffectReferenceType.Type.GetAttributeInfo("Name");
		BehaviorReferenceType.Type = getNodeType("Entities", "BehaviorReferenceType");
		BehaviorReferenceType.NameAttribute = BehaviorReferenceType.Type.GetAttributeInfo("Name");
		GeometryReferenceType.Type = getNodeType("Entities", "GeometryReferenceType");
		GeometryReferenceType.NameAttribute = GeometryReferenceType.Type.GetAttributeInfo("Name");
		LightReferenceType.Type = getNodeType("Entities", "LightReferenceType");
		LightReferenceType.NameAttribute = LightReferenceType.Type.GetAttributeInfo("Name");
		TimelineBindingType.Type = getNodeType("Entities", "TimelineBindingType");
		TimelineBindingType.SlotNameAttribute = TimelineBindingType.Type.GetAttributeInfo("SlotName");
		TimelineBindingType.TimelineNameAttribute = TimelineBindingType.Type.GetAttributeInfo("TimelineName");
		TimelineBindingType.DurationAttribute = TimelineBindingType.Type.GetAttributeInfo("Duration");
		ModelInstanceType.Type = getNodeType("Entities", "ModelInstanceType");
		ModelInstanceType.NameAttribute = ModelInstanceType.Type.GetAttributeInfo("Name");
		ModelInstanceType.GeoNameAttribute = ModelInstanceType.Type.GetAttributeInfo("GeoName");
		ModelInstanceType.PrimGroupStatesChild = ModelInstanceType.Type.GetChildInfo("PrimGroupStates");
		ModelInstanceType.VertexCountAttribute = ModelInstanceType.Type.GetAttributeInfo("VertexCount");
		ModelInstanceType.PrimitiveCountAttribute = ModelInstanceType.Type.GetAttributeInfo("PrimitiveCount");
		ModelInstanceType.BoneCountAttribute = ModelInstanceType.Type.GetAttributeInfo("BoneCount");
		PrimGroupStateType.Type = getNodeType("Entities", "PrimGroupStateType");
		PrimGroupStateType.MeshNameAttribute = PrimGroupStateType.Type.GetAttributeInfo("MeshName");
		PrimGroupStateType.GroupNameAttribute = PrimGroupStateType.Type.GetAttributeInfo("GroupName");
		PrimGroupStateType.StateNameAttribute = PrimGroupStateType.Type.GetAttributeInfo("StateName");
		PrimGroupStateType.CookParametersChild = PrimGroupStateType.Type.GetChildInfo("CookParameters");
		AttachmentPointType.Type = getNodeType("Entities", "AttachmentPointType");
		AttachmentPointType.NameAttribute = AttachmentPointType.Type.GetAttributeInfo("Name");
		AttachmentPointType.BoneNameAttribute = AttachmentPointType.Type.GetAttributeInfo("BoneName");
		AttachmentPointType.ModelInstanceNameAttribute = AttachmentPointType.Type.GetAttributeInfo("ModelInstanceName");
		AttachmentPointType.PositionAttribute = AttachmentPointType.Type.GetAttributeInfo("Position");
		AttachmentPointType.OrientationAttribute = AttachmentPointType.Type.GetAttributeInfo("Orientation");
		AttachmentPointType.ScaleAttribute = AttachmentPointType.Type.GetAttributeInfo("Scale");
		AttachmentPointType.ScaleAttribute.DefaultValue = 1f;
		AttachmentPointType.CookParametersChild = AttachmentPointType.Type.GetChildInfo("CookParameters");
		SplineType.Type = getNodeType("Entities", "SplineType");
		SplineType.NameAttribute = SplineType.Type.GetAttributeInfo("Name");
		SplineType.ClassNameAttribute = SplineType.Type.GetAttributeInfo("ClassName");
		SplineType.ClosedLoopAttribute = SplineType.Type.GetAttributeInfo("ClosedLoop");
		SplineType.VerticesChild = SplineType.Type.GetChildInfo("Vertices");
		SplineType.CookParametersChild = SplineType.Type.GetChildInfo("CookParameters");
		SplineVertexType.Type = getNodeType("Entities", "SplineVertexType");
		SplineVertexType.PositionAttribute = SplineVertexType.Type.GetAttributeInfo("Position");
		SplineVertexType.LeftUserTangentAttribute = SplineVertexType.Type.GetAttributeInfo("LeftUserTangent");
		SplineVertexType.RightUserTangentAttribute = SplineVertexType.Type.GetAttributeInfo("RightUserTangent");
		SplineVertexType.SharpCornerAttribute = SplineVertexType.Type.GetAttributeInfo("SharpCorner");
		SplineVertexType.UseUserTangentsAttribute = SplineVertexType.Type.GetAttributeInfo("UseUserTangents");
		EnvironmentLightEntityType.Type = getNodeType("Entities", "EnvironmentLightEntityType");
		EnvironmentLightEntityType.NameAttribute = EnvironmentLightEntityType.Type.GetAttributeInfo("Name");
		EnvironmentLightEntityType.DescriptionAttribute = EnvironmentLightEntityType.Type.GetAttributeInfo("Description");
		EnvironmentLightEntityType.ClassNameAttribute = EnvironmentLightEntityType.Type.GetAttributeInfo("ClassName");
		EnvironmentLightEntityType.SourceObjectNameAttribute = EnvironmentLightEntityType.Type.GetAttributeInfo("SourceObjectName");
		EnvironmentLightEntityType.ImportedTimeAttribute = EnvironmentLightEntityType.Type.GetAttributeInfo("ImportedTime");
		EnvironmentLightEntityType.ExportedTimeAttribute = EnvironmentLightEntityType.Type.GetAttributeInfo("ExportedTime");
		EnvironmentLightEntityType.TagsChild = EnvironmentLightEntityType.Type.GetChildInfo("Tags");
		EnvironmentLightEntityType.CookParametersChild = EnvironmentLightEntityType.Type.GetChildInfo("CookParameters");
		EnvironmentLightEntityType.DataFilesChild = EnvironmentLightEntityType.Type.GetChildInfo("DataFiles");
		EnvironmentLightEntityType.SourceFilePathChild = EnvironmentLightEntityType.Type.GetChildInfo("SourceFilePath");
		EnvironmentLightEntityType.LightDirectionTagsChild = EnvironmentLightEntityType.Type.GetChildInfo("LightDirectionTags");
		LightDirectionTagType.Type = getNodeType("Entities", "LightDirectionTagType");
		LightDirectionTagType.NameAttribute = LightDirectionTagType.Type.GetAttributeInfo("Name");
		LightDirectionTagType.CastsShadowsAttribute = LightDirectionTagType.Type.GetAttributeInfo("CastsShadows");
		LightDirectionTagType.XPositionAttribute = LightDirectionTagType.Type.GetAttributeInfo("XPosition");
		LightDirectionTagType.YPositionAttribute = LightDirectionTagType.Type.GetAttributeInfo("YPosition");
		LightDirectionTagType.ZPositionAttribute = LightDirectionTagType.Type.GetAttributeInfo("ZPosition");
		LightDirectionTagType.DiameterAttribute = LightDirectionTagType.Type.GetAttributeInfo("Diameter");
		LightDirectionTagType.AngularFalloffAttribute = LightDirectionTagType.Type.GetAttributeInfo("AngularFalloff");
		LightDirectionTagType.RedAttribute = LightDirectionTagType.Type.GetAttributeInfo("Red");
		LightDirectionTagType.GreenAttribute = LightDirectionTagType.Type.GetAttributeInfo("Green");
		LightDirectionTagType.BlueAttribute = LightDirectionTagType.Type.GetAttributeInfo("Blue");
		LightDirectionTagType.UAttribute = LightDirectionTagType.Type.GetAttributeInfo("U");
		LightDirectionTagType.VAttribute = LightDirectionTagType.Type.GetAttributeInfo("V");
		LightDirectionTagType.LightIntensityAttribute = LightDirectionTagType.Type.GetAttributeInfo("LightIntensity");
		LightDirectionTagType.LightColorAttribute = LightDirectionTagType.Type.GetAttributeInfo("LightColor");
		LightDirectionTagSetType.Type = getNodeType("Entities", "LightDirectionTagSetType");
		LightDirectionTagSetType.LightDirectionTagsChild = LightDirectionTagSetType.Type.GetChildInfo("LightDirectionTags");
		LightRigEntityType.Type = getNodeType("Entities", "LightRigEntityType");
		LightRigEntityType.NameAttribute = LightRigEntityType.Type.GetAttributeInfo("Name");
		LightRigEntityType.DescriptionAttribute = LightRigEntityType.Type.GetAttributeInfo("Description");
		LightRigEntityType.ClassNameAttribute = LightRigEntityType.Type.GetAttributeInfo("ClassName");
		LightRigEntityType.DSGAttribute = LightRigEntityType.Type.GetAttributeInfo("DSG");
		LightRigEntityType.TagsChild = LightRigEntityType.Type.GetChildInfo("Tags");
		LightRigEntityType.CookParametersChild = LightRigEntityType.Type.GetChildInfo("CookParameters");
		LightRigEntityType.DataFilesChild = LightRigEntityType.Type.GetChildInfo("DataFiles");
		LightRigEntityType.AnimationBindingSetChild = LightRigEntityType.Type.GetChildInfo("AnimationBindingSet");
		LightRigEntityType.EnvironmentLightSetChild = LightRigEntityType.Type.GetChildInfo("EnvironmentLightSet");
		LightRigEntityType.AnalyticLightSetChild = LightRigEntityType.Type.GetChildInfo("AnalyticLightSet");
		MaterialInstanceRootElement = getRootElement("Entities", "MaterialInstance");
		FireFXInstanceRootElement = getRootElement("Entities", "FireFXInstance");
		TextureEntityRootElement = getRootElement("Entities", "TextureEntity");
		AnimationEntityRootElement = getRootElement("Entities", "AnimationEntity");
		GeometryEntityRootElement = getRootElement("Entities", "GeometryEntity");
		AnalyticLightEntityRootElement = getRootElement("Entities", "AnalyticLightEntity");
		ParticleEffectEntityRootElement = getRootElement("Entities", "ParticleEffectEntity");
		DSGEntityRootElement = getRootElement("Entities", "DSGEntity");
		AssetEntityRootElement = getRootElement("Entities", "AssetEntity");
		BehaviorEntityRootElement = getRootElement("Entities", "BehaviorEntity");
		EnvironmentLightEntityRootElement = getRootElement("Entities", "EnvironmentLightEntity");
		LightRigEntityRootElement = getRootElement("Entities", "LightRigEntity");
	}
}
