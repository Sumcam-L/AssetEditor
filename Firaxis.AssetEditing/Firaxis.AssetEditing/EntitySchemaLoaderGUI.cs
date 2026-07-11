using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using DatabaseWrapper;
using Firaxis.Asset;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.TextureExport;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IEntitySchemaExtender))]
[Export(typeof(EntitySchemaLoaderGUI))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class EntitySchemaLoaderGUI : IEntitySchemaExtender, IPartImportsSatisfiedNotification
{
	[Import(AllowDefault = true)]
	private ITimelinePlaybackService TimelinePlaybackService;

	[Import(AllowDefault = true)]
	private IAnimationKnobService AnimationKnobService;

	private IAssetBrowserDialogService m_assetBrowserDialogService;

	private ICivTechService m_civTechService;

	private IFileDialogService m_fileDialogService;

	[ImportingConstructor]
	public EntitySchemaLoaderGUI(IEntitySchemaInitializer typeInitializer, IFileDialogService fileDialogService, IAssetBrowserDialogService assetBrowserDialogService, ICivTechService civTechSvc)
	{
		m_fileDialogService = fileDialogService;
		m_assetBrowserDialogService = assetBrowserDialogService;
		m_civTechService = civTechSvc;
	}

	public void OnImportsSatisfied()
	{
		DomNodeType type = EntitySchema.EntitySourceFilePathType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[1]
		{
			new AttributePropertyDescriptor("Path".Localize(), EntitySchema.EntitySourceFilePathType.PathAttribute, "Basic".Localize(), "File path.".Localize(), isReadOnly: false, new SourceFileTypeUITypeEditor(m_fileDialogService))
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
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

	private string[] GetSortedClassNames(InstanceType insType)
	{
		List<string> list = global::DatabaseWrapper.DatabaseWrapper.GetClassNames(m_civTechService.PrimaryProject.Name, insType).Cast<string>().ToList();
		list.Sort();
		return list.ToArray();
	}

	private void DefineAnalyticLightInstanceAdapters()
	{
		EmbeddedCollectionEditor embeddedCollectionEditor = new EmbeddedCollectionEditor();
		embeddedCollectionEditor.ShowCollectionToolbar = false;
		embeddedCollectionEditor.ShowItemLabels = false;
		embeddedCollectionEditor.ItemLabelsType = EmbeddedCollectionEditor.ItemLabelType.kNone;
		string[] sortedClassNames = GetSortedClassNames(InstanceType.IT_ANALYTIC_LIGHT);
		DomNodeType type = EntitySchema.AnalyticLightEntityType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[7]
		{
			new EntityNameAttributePropertyDescriptor("Name".Localize(), EntitySchema.AnalyticLightEntityType.NameAttribute, "Basic".Localize(), "Name of this entity".Localize(), isReadOnly: false),
			new EntityClassNameAttributePropertyDescriptor("Class Name".Localize(), EntitySchema.AnalyticLightEntityType.ClassNameAttribute, "Basic".Localize(), "Associated class".Localize(), isReadOnly: false, new EnumUITypeEditor(sortedClassNames), new ExclusiveEnumTypeConverter(sortedClassNames), onlyNewCanChange: false),
			new ChildAttributeFieldPropertyDescriptor("Source File Path".Localize(), EntitySchema.EntitySourceFilePathType.PathAttribute, EntitySchema.AnalyticLightEntityType.SourceFilePathChild, "Source".Localize(), "Source file for this entity.".Localize(), isReadOnly: false, new SourceFileTypeUITypeEditor(m_fileDialogService)),
			new AttributePropertyDescriptor("Source Object".Localize(), EntitySchema.AnalyticLightEntityType.SourceObjectNameAttribute, "Source".Localize(), "Source object that this texture came from.".Localize(), isReadOnly: false, new SourceObjectUITypeEditor(), new SourceObjectTypeConverter()),
			new AttributePropertyDescriptor("Imported Time".Localize(), EntitySchema.AnalyticLightEntityType.ImportedTimeAttribute, "Source".Localize(), "Last time that this texture was imported to Perforce.".Localize(), isReadOnly: true, new DateTimeEditor()),
			new AttributePropertyDescriptor("Exported Time".Localize(), EntitySchema.AnalyticLightEntityType.ExportedTimeAttribute, "Source".Localize(), "Last time that this texture was exported from its Source File.".Localize(), isReadOnly: true, new DateTimeEditor()),
			new ChildPropertyDescriptor("Data Files".Localize(), EntitySchema.AnalyticLightEntityType.DataFilesChild, "Data".Localize(), "Data files associated with entity".Localize(), isReadOnly: true, embeddedCollectionEditor, null)
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
	}

	private void DefineAnimationInstanceAdapters()
	{
		EmbeddedCollectionEditor embeddedCollectionEditor = new EmbeddedCollectionEditor();
		embeddedCollectionEditor.ShowCollectionToolbar = false;
		embeddedCollectionEditor.ShowItemLabels = false;
		embeddedCollectionEditor.ItemLabelsType = EmbeddedCollectionEditor.ItemLabelType.kNone;
		string[] sortedClassNames = GetSortedClassNames(InstanceType.IT_ANIMATION);
		DomNodeType type = EntitySchema.AnimationEntityType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[7]
		{
			new EntityNameAttributePropertyDescriptor("Name".Localize(), EntitySchema.AnimationEntityType.NameAttribute, "Basic".Localize(), "Name of this entity".Localize(), isReadOnly: false),
			new EntityClassNameAttributePropertyDescriptor("Class Name".Localize(), EntitySchema.AnimationEntityType.ClassNameAttribute, "Basic".Localize(), "Associated class".Localize(), isReadOnly: false, new EnumUITypeEditor(sortedClassNames), new ExclusiveEnumTypeConverter(sortedClassNames), onlyNewCanChange: false),
			new ChildAttributeFieldPropertyDescriptor("Source File Path".Localize(), EntitySchema.EntitySourceFilePathType.PathAttribute, EntitySchema.AnimationEntityType.SourceFilePathChild, "Source".Localize(), "Source file for this entity.".Localize(), isReadOnly: false, new SourceFileTypeUITypeEditor(m_fileDialogService)),
			new AttributePropertyDescriptor("Source Object".Localize(), EntitySchema.AnimationEntityType.SourceObjectNameAttribute, "Source".Localize(), "Source object that this texture came from.".Localize(), isReadOnly: false, new SourceObjectUITypeEditor(), new SourceObjectTypeConverter()),
			new AttributePropertyDescriptor("Imported Time".Localize(), EntitySchema.AnimationEntityType.ImportedTimeAttribute, "Source".Localize(), "Last time that this texture was imported to Perforce.".Localize(), isReadOnly: true, new DateTimeEditor()),
			new AttributePropertyDescriptor("Exported Time".Localize(), EntitySchema.AnimationEntityType.ExportedTimeAttribute, "Source".Localize(), "Last time that this texture was exported from its Source File.".Localize(), isReadOnly: true, new DateTimeEditor()),
			new ChildPropertyDescriptor("Data Files".Localize(), EntitySchema.AnimationEntityType.DataFilesChild, "Data".Localize(), "Data files associated with entity".Localize(), isReadOnly: true, embeddedCollectionEditor, null)
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
	}

	private void DefineTimelineAdapters()
	{
		DomNodeType type = EntitySchema.TrackType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[1]
		{
			new AttributePropertyDescriptor("Name".Localize(), EntitySchema.TrackType.NameAttribute, "Basic".Localize(), "Name for this track".Localize(), isReadOnly: false)
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type2 = EntitySchema.TriggerType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[2]
		{
			new AttributePropertyDescriptor("Name".Localize(), EntitySchema.TriggerType.NameAttribute, "Basic".Localize(), "Name for this trigger".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("StartTime".Localize(), EntitySchema.TriggerType.StartTimeAttribute, "Basic".Localize(), "Start time for this trigger".Localize(), isReadOnly: false)
		};
		properties = array;
		type2.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type3 = EntitySchema.SoundTriggerType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[2]
		{
			new AttributePropertyDescriptor("Audio Event".Localize(), EntitySchema.SoundTriggerType.AudioEventAttribute, "Audio".Localize(), "Audio event to trigger when this trigger is fired".Localize(), isReadOnly: false, new SoundEventTypeEditor()),
			new AttributePropertyDescriptor("Attachment Point".Localize(), EntitySchema.SoundTriggerType.AttachmentPointNameAttribute, "Audio".Localize(), "Attachment point this audio event should be attached to when this trigger is fired".Localize(), isReadOnly: false, new AttachmentPointNameEditor())
		};
		properties = array;
		type3.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type4 = EntitySchema.ActionTriggerType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[2]
		{
			new AttributePropertyDescriptor("Action".Localize(), EntitySchema.ActionTriggerType.ActionAttribute, "Action".Localize(), "Action to trigger when this trigger is fired".Localize(), isReadOnly: false),
			new AttributePropertyDescriptor("Attachment Point".Localize(), EntitySchema.ActionTriggerType.AttachmentPointNameAttribute, "Action".Localize(), "Attachment point this action should be attached to when this trigger is fired".Localize(), isReadOnly: false, new AttachmentPointNameEditor())
		};
		properties = array;
		type4.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type5 = EntitySchema.TransferTriggerType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[1]
		{
			new AttributePropertyDescriptor("Target Trigger".Localize(), EntitySchema.TransferTriggerType.TargetTriggerAttribute, "Transfer".Localize(), "Target trigger to transfer effect from when this trigger is fired".Localize(), isReadOnly: false, new TransferTargetTypeEditor(), new TransferTargetTypeConverter())
		};
		properties = array;
		type5.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type6 = EntitySchema.AssetFXTriggerType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[3]
		{
			new AttributePropertyDescriptor("VFX Asset".Localize(), EntitySchema.AssetFXTriggerType.VFXAssetAttribute, "Asset VFX".Localize(), "VFX asset to spawn when this trigger is fired".Localize(), isReadOnly: false, new AssetBrowserAssetNameEditor()),
			new AttributePropertyDescriptor("Attachment Point".Localize(), EntitySchema.AssetFXTriggerType.AttachmentPointNameAttribute, "Asset VFX".Localize(), "Attachment point this VFX should be attached to when this trigger is fired".Localize(), isReadOnly: false, new AttachmentPointNameEditor()),
			new AttributePropertyDescriptor("Duration".Localize(), EntitySchema.AssetFXTriggerType.DurationAttribute, "Asset VFX".Localize(), "Duration this effect should live".Localize(), isReadOnly: false, new NumericEditor(typeof(float)))
		};
		properties = array;
		type6.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type7 = EntitySchema.ArtDefFXTriggerType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[4]
		{
			new AttributePropertyDescriptor("VFX Collection".Localize(), EntitySchema.ArtDefFXTriggerType.VFXCollectionAttribute, "ArtDef VFX".Localize(), "ArtDef collection that contains VFX elements to pick from this trigger".Localize(), isReadOnly: false, new ArtDefFXTriggerCollectionTypeEditor()),
			new AttributePropertyDescriptor("VFX Element".Localize(), EntitySchema.ArtDefFXTriggerType.VFXElementAttribute, "ArtDef VFX".Localize(), "ArtDef element that points to the VFX asset to trigger when this trigger is fired".Localize(), isReadOnly: false, new ArtDefFXTriggerElementTypeEditor()),
			new AttributePropertyDescriptor("Attachment Point".Localize(), EntitySchema.ArtDefFXTriggerType.AttachmentPointNameAttribute, "ArtDef VFX".Localize(), "Attachment point this VFX should be attached to when this trigger is fired".Localize(), isReadOnly: false, new AttachmentPointNameEditor()),
			new AttributePropertyDescriptor("Duration".Localize(), EntitySchema.ArtDefFXTriggerType.DurationAttribute, "ArtDef VFX".Localize(), "Duration this effect should live".Localize(), isReadOnly: false, new NumericEditor(typeof(float)))
		};
		properties = array;
		type7.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type8 = EntitySchema.LightTriggerType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[3]
		{
			new AttributePropertyDescriptor("Light Asset".Localize(), EntitySchema.LightTriggerType.LightAssetAttribute, "Light".Localize(), "Light asset to spawn when this trigger is fired".Localize(), isReadOnly: false, new AssetBrowserLightNameEditor()),
			new AttributePropertyDescriptor("Attachment Point".Localize(), EntitySchema.LightTriggerType.AttachmentPointNameAttribute, "Light".Localize(), "Attachment point this light should be attached to when this trigger is fired".Localize(), isReadOnly: false, new AttachmentPointNameEditor()),
			new AttributePropertyDescriptor("Duration".Localize(), EntitySchema.LightTriggerType.DurationAttribute, "Light".Localize(), "Duration this light should exist".Localize(), isReadOnly: false, new NumericEditor(typeof(float)))
		};
		properties = array;
		type8.SetTag(new PropertyDescriptorCollection(properties));
	}

	private void DefineAssetInstanceAdapters()
	{
		DomNodeType type = EntitySchema.PrimGroupStateType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[3]
		{
			new AttributePropertyDescriptor("Mesh".Localize(), EntitySchema.PrimGroupStateType.MeshNameAttribute, "Basic".Localize(), "Mesh name for this state entry".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Group".Localize(), EntitySchema.PrimGroupStateType.GroupNameAttribute, "Basic".Localize(), "Group name for this state entry".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("State".Localize(), EntitySchema.PrimGroupStateType.StateNameAttribute, "Basic".Localize(), "State name of this entry".Localize(), isReadOnly: true)
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type2 = EntitySchema.TimelineBindingType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[3]
		{
			new AttributePropertyDescriptor("Slot Name".Localize(), EntitySchema.TimelineBindingType.SlotNameAttribute, "Basic".Localize(), "Slot name of this timeline binding".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Timeline Name".Localize(), EntitySchema.TimelineBindingType.TimelineNameAttribute, "Basic".Localize(), "Timeline name for this binding".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Duration".Localize(), EntitySchema.TimelineBindingType.DurationAttribute, "Timeline".Localize(), "Timeline duration".Localize(), isReadOnly: false, new NumericEditor(typeof(float)))
		};
		properties = array;
		type2.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type3 = EntitySchema.AnimationBindingType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[2]
		{
			new AttributePropertyDescriptor("Slot Name".Localize(), EntitySchema.AnimationBindingType.SlotNameAttribute, "Basic".Localize(), "Slot name of this animation binding".Localize(), isReadOnly: true),
			new TimelineFixupAttributePropertyDescriptor("Animation Name".Localize(), EntitySchema.AnimationBindingType.AnimationNameAttribute, "Basic".Localize(), "Animation name for this binding".Localize(), isReadOnly: false, new AnimationBindingEditor(m_civTechService, m_assetBrowserDialogService, TimelinePlaybackService, AnimationKnobService))
		};
		properties = array;
		type3.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type4 = EntitySchema.ParticleEffectReferenceType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[1]
		{
			new AttributePropertyDescriptor("Particle Name".Localize(), EntitySchema.ParticleEffectReferenceType.NameAttribute, "Basic".Localize(), "Particle name for this reference".Localize(), isReadOnly: false, new AssetBrowserNameEditor(m_civTechService, m_assetBrowserDialogService, new List<InstanceType> { InstanceType.IT_PARTICLE_EFFECT }))
		};
		properties = array;
		type4.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type5 = EntitySchema.ModelInstanceType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[4]
		{
			new AttributePropertyDescriptor("Name".Localize(), EntitySchema.ModelInstanceType.NameAttribute, "Basic".Localize(), "Name of this model".Localize(), isReadOnly: false),
			new AttributePropertyDescriptor("Vertex Count".Localize(), EntitySchema.ModelInstanceType.VertexCountAttribute, "Stats".Localize(), "Vertex Count of this model instance.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Primitive Count".Localize(), EntitySchema.ModelInstanceType.PrimitiveCountAttribute, "Stats".Localize(), "Primitive Count of this model instance.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Bone Count".Localize(), EntitySchema.ModelInstanceType.BoneCountAttribute, "Stats".Localize(), "Bone Count of this model instance.".Localize(), isReadOnly: true)
		};
		properties = array;
		type5.SetTag(new PropertyDescriptorCollection(properties));
		string[] sortedClassNames = GetSortedClassNames(InstanceType.IT_ASSET);
		DomNodeType type6 = EntitySchema.AssetType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[3]
		{
			new EntityNameAttributePropertyDescriptor("Name".Localize(), EntitySchema.AssetType.NameAttribute, "Basic".Localize(), "Name of this entity".Localize(), isReadOnly: false),
			new EntityClassNameAttributePropertyDescriptor("Class Name".Localize(), EntitySchema.AssetType.ClassNameAttribute, "Basic".Localize(), "Associated class".Localize(), isReadOnly: false, new EnumUITypeEditor(sortedClassNames), new ExclusiveEnumTypeConverter(sortedClassNames), onlyNewCanChange: false),
			new AttributePropertyDescriptor("DSG".Localize(), EntitySchema.AssetType.DSGAttribute, "Basic".Localize(), "State graph associated with this entity".Localize(), isReadOnly: false, new AssetBrowserNameEditor(m_civTechService, m_assetBrowserDialogService, new InstanceType[1] { InstanceType.IT_DSG }))
		};
		properties = array;
		type6.SetTag(new PropertyDescriptorCollection(properties));
	}

	private void DefineSplineAdapters()
	{
		DomNodeType type = EntitySchema.SplineType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[3]
		{
			new AttributePropertyDescriptor("Class Name".Localize(), EntitySchema.SplineType.ClassNameAttribute, "Basic".Localize(), "Name of this spline's spline class".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Spline Name".Localize(), EntitySchema.SplineType.NameAttribute, "Basic".Localize(), "Name of this spline".Localize(), isReadOnly: false),
			new AttributePropertyDescriptor("Closed Loop".Localize(), EntitySchema.SplineType.ClosedLoopAttribute, "Basic".Localize(), "Is spline a closed loop?".Localize(), isReadOnly: false)
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
	}

	private void DefineAttachmentPointAdapters()
	{
		FloatArrayConverter floatArrayConverter = new FloatArrayConverter();
		floatArrayConverter.Format = "0.####°";
		floatArrayConverter.ScaleFactor = 180.0 / Math.PI;
		NumericTupleEditor numericTupleEditor = new NumericTupleEditor(typeof(float), new string[3] { "X", "Y", "Z" });
		numericTupleEditor.ScaleFactor = 180.0 / Math.PI;
		DomNodeType type = EntitySchema.AttachmentPointType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[6]
		{
			new AttributePropertyDescriptor("Attachment Point Name".Localize(), EntitySchema.AttachmentPointType.NameAttribute, "ID".Localize(), "Name of this attachment".Localize(), isReadOnly: false),
			new AttributePropertyDescriptor("Model Instance".Localize(), EntitySchema.AttachmentPointType.ModelInstanceNameAttribute, "ID".Localize(), "Model instance associated with this attachment  point".Localize(), isReadOnly: false, new ModelInstanceUITypeEditor(), new ModelInstanceUITypeConverter()),
			new AttributePropertyDescriptor("Bone Name".Localize(), EntitySchema.AttachmentPointType.BoneNameAttribute, "ID".Localize(), "Bone name associated with this attachment point".Localize(), isReadOnly: false, new BoneUITypeEditor(), new BoneUITypeConverter()),
			new AttributePropertyDescriptor("Position".Localize(), EntitySchema.AttachmentPointType.PositionAttribute, "XForm".Localize(), "Position of this attachment".Localize(), isReadOnly: false, new NumericTupleEditor(), new FloatArrayConverter()),
			new AttributePropertyDescriptor("Rotation".Localize(), EntitySchema.AttachmentPointType.OrientationAttribute, "XForm".Localize(), "Rotation of this attachment".Localize(), isReadOnly: false, numericTupleEditor, floatArrayConverter),
			new AttributePropertyDescriptor("Scale".Localize(), EntitySchema.AttachmentPointType.ScaleAttribute, "XForm".Localize(), "Scale of this attachment".Localize(), isReadOnly: false, new BoundedFloatEditor(0.02f, 50f), new BoundedFloatConverter())
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
	}

	private void DefineBehaviorInstanceAdapters()
	{
		string[] sortedClassNames = GetSortedClassNames(InstanceType.IT_BEHAVIOR);
		DomNodeType type = EntitySchema.BehaviorType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[3]
		{
			new EntityNameAttributePropertyDescriptor("Name".Localize(), EntitySchema.BehaviorType.NameAttribute, "Basic".Localize(), "Name of this entity".Localize(), isReadOnly: false),
			new EntityClassNameAttributePropertyDescriptor("Class Name".Localize(), EntitySchema.BehaviorType.ClassNameAttribute, "Basic".Localize(), "Associated class".Localize(), isReadOnly: false, new EnumUITypeEditor(sortedClassNames), new ExclusiveEnumTypeConverter(sortedClassNames), onlyNewCanChange: false),
			new AttributePropertyDescriptor("DSG".Localize(), EntitySchema.BehaviorType.DSGAttribute, "Basic".Localize(), "State graph associated with this entity".Localize(), isReadOnly: false, new AssetBrowserNameEditor(m_civTechService, m_assetBrowserDialogService, new InstanceType[1] { InstanceType.IT_DSG }))
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type2 = EntitySchema.BehaviorReferenceType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[1]
		{
			new AttributePropertyDescriptor("Name".Localize(), EntitySchema.BehaviorReferenceType.NameAttribute, "Basic".Localize(), "Name of this entity".Localize(), isReadOnly: true)
		};
		properties = array;
		type2.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type3 = EntitySchema.GeometryReferenceType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[1]
		{
			new AttributePropertyDescriptor("Name".Localize(), EntitySchema.GeometryReferenceType.NameAttribute, "Basic".Localize(), "Name of this entity".Localize(), isReadOnly: true)
		};
		properties = array;
		type3.SetTag(new PropertyDescriptorCollection(properties));
	}

	private void DefineEnvironmentLightInstanceAdapters()
	{
		EmbeddedCollectionEditor embeddedCollectionEditor = new EmbeddedCollectionEditor();
		embeddedCollectionEditor.ShowCollectionToolbar = false;
		embeddedCollectionEditor.ShowItemLabels = false;
		embeddedCollectionEditor.ItemLabelsType = EmbeddedCollectionEditor.ItemLabelType.kNone;
		string[] sortedClassNames = GetSortedClassNames(InstanceType.IT_ENVIRONMENT_LIGHT);
		DomNodeType type = EntitySchema.EnvironmentLightEntityType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[7]
		{
			new EntityNameAttributePropertyDescriptor("Name".Localize(), EntitySchema.EnvironmentLightEntityType.NameAttribute, "Basic".Localize(), "Name of this entity".Localize(), isReadOnly: false),
			new EntityClassNameAttributePropertyDescriptor("Class Name".Localize(), EntitySchema.EnvironmentLightEntityType.ClassNameAttribute, "Basic".Localize(), "Associated class".Localize(), isReadOnly: false, new EnumUITypeEditor(sortedClassNames), new ExclusiveEnumTypeConverter(sortedClassNames), onlyNewCanChange: false),
			new ChildAttributePropertyDescriptor("Source File Path".Localize(), EntitySchema.EntitySourceFilePathType.PathAttribute, EntitySchema.EnvironmentLightEntityType.SourceFilePathChild, "Source".Localize(), "Source file for this entity.".Localize(), isReadOnly: false, new SourceFileTypeUITypeEditor(m_fileDialogService)),
			new AttributePropertyDescriptor("Source Object".Localize(), EntitySchema.EnvironmentLightEntityType.SourceObjectNameAttribute, "Source".Localize(), "Source object that this texture came from.".Localize(), isReadOnly: false, new SourceObjectUITypeEditor(), new SourceObjectTypeConverter()),
			new AttributePropertyDescriptor("Imported Time".Localize(), EntitySchema.EnvironmentLightEntityType.ImportedTimeAttribute, "Source".Localize(), "Last time that this texture was imported to Perforce.".Localize(), isReadOnly: true, new DateTimeEditor()),
			new AttributePropertyDescriptor("Exported Time".Localize(), EntitySchema.EnvironmentLightEntityType.ExportedTimeAttribute, "Source".Localize(), "Last time that this texture was exported from its Source File.".Localize(), isReadOnly: true, new DateTimeEditor()),
			new ChildPropertyDescriptor("Data Files".Localize(), EntitySchema.EnvironmentLightEntityType.DataFilesChild, "Data".Localize(), "Data files associated with entity".Localize(), isReadOnly: true, embeddedCollectionEditor, null)
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type2 = EntitySchema.LightDirectionTagType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[14]
		{
			new AttributePropertyDescriptor("Name".Localize(), EntitySchema.LightDirectionTagType.NameAttribute, "Basic".Localize(), "Name of this entity".Localize(), isReadOnly: false),
			new AttributePropertyDescriptor("U".Localize(), EntitySchema.LightDirectionTagType.UAttribute, "Mark Position".Localize(), "Horizontal position of the tag on the cube map in relative space.".Localize(), isReadOnly: false, new BoundedFloatEditor(0f, 1f), new BoundedFloatConverter()),
			new AttributePropertyDescriptor("V".Localize(), EntitySchema.LightDirectionTagType.VAttribute, "Mark Position".Localize(), "Vertical position of the tag on the cube map in relative space.".Localize(), isReadOnly: false, new BoundedFloatEditor(0f, 1f), new BoundedFloatConverter()),
			new AttributePropertyDescriptor("Diameter".Localize(), EntitySchema.LightDirectionTagType.DiameterAttribute, "Light Configuration".Localize(), "Diameter of the Light.".Localize(), isReadOnly: false, new BoundedFloatEditor(0f, 180f), new BoundedFloatConverter()),
			new AttributePropertyDescriptor("Angular Falloff".Localize(), EntitySchema.LightDirectionTagType.AngularFalloffAttribute, "Light Configuration".Localize(), "Angular Falloff of the Light.".Localize(), isReadOnly: false, new BoundedFloatEditor(0f, 1f), new BoundedFloatConverter()),
			new AttributePropertyDescriptor("Casts Shadows".Localize(), EntitySchema.LightDirectionTagType.CastsShadowsAttribute, "Light Configuration".Localize(), "Does this light cast shadows".Localize(), isReadOnly: false, new BoolEditor()),
			new AttributePropertyDescriptor("Light Intensity".Localize(), EntitySchema.LightDirectionTagType.LightIntensityAttribute, "Light Configuration".Localize(), "Intensity of the light.".Localize(), isReadOnly: false, new BoundedFloatEditor(0f, 20f), new BoundedFloatConverter()),
			new AttributePropertyDescriptor("Light Color".Localize(), EntitySchema.LightDirectionTagType.LightColorAttribute, "Light Configuration".Localize(), "Color of the light.".Localize(), isReadOnly: false, new ColorPickerEditor(), new IntColorConverter()),
			new AttributePropertyDescriptor("Red Intensity".Localize(), EntitySchema.LightDirectionTagType.RedAttribute, "Light Configuration".Localize(), "Red intensity of the light.".Localize(), isReadOnly: false, new NumericEditor(typeof(float))),
			new AttributePropertyDescriptor("Green Intensity".Localize(), EntitySchema.LightDirectionTagType.GreenAttribute, "Light Configuration".Localize(), "Green intensity of the light.".Localize(), isReadOnly: false, new NumericEditor(typeof(float))),
			new AttributePropertyDescriptor("Blue Intensity".Localize(), EntitySchema.LightDirectionTagType.BlueAttribute, "Light Configuration".Localize(), "Blue intensity of the light.".Localize(), isReadOnly: false, new NumericEditor(typeof(float))),
			new AttributePropertyDescriptor("X Position".Localize(), EntitySchema.LightDirectionTagType.XPositionAttribute, "Game-World Position".Localize(), "X position of the light.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Y Position".Localize(), EntitySchema.LightDirectionTagType.YPositionAttribute, "Game-World Position".Localize(), "Y position of the light.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Z Position".Localize(), EntitySchema.LightDirectionTagType.ZPositionAttribute, "Game-World Position".Localize(), "Z position of the light.".Localize(), isReadOnly: true)
		};
		properties = array;
		type2.SetTag(new PropertyDescriptorCollection(properties));
	}

	private void DefineGeometryInstanceAdapters()
	{
		EmbeddedCollectionEditor embeddedCollectionEditor = new EmbeddedCollectionEditor();
		embeddedCollectionEditor.ShowCollectionToolbar = false;
		embeddedCollectionEditor.ShowItemLabels = false;
		embeddedCollectionEditor.ItemLabelsType = EmbeddedCollectionEditor.ItemLabelType.kNone;
		string[] sortedClassNames = GetSortedClassNames(InstanceType.IT_GEOMETRY);
		DomNodeType type = EntitySchema.GeometryEntityType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[9]
		{
			new EntityNameAttributePropertyDescriptor("Name".Localize(), EntitySchema.GeometryEntityType.NameAttribute, "Basic".Localize(), "Name of this entity".Localize(), isReadOnly: false),
			new EntityClassNameAttributePropertyDescriptor("Class Name".Localize(), EntitySchema.GeometryEntityType.ClassNameAttribute, "Basic".Localize(), "Associated class".Localize(), isReadOnly: false, new EnumUITypeEditor(sortedClassNames), new ExclusiveEnumTypeConverter(sortedClassNames), onlyNewCanChange: false),
			new AttributePropertyDescriptor("Vertex Count".Localize(), EntitySchema.GeometryEntityType.VertexCountAttribute, "Stats".Localize(), "Vertex count of the model.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Primitive Count".Localize(), EntitySchema.GeometryEntityType.PrimitiveCountAttribute, "Stats".Localize(), "Primitive count of the model.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Bone Count".Localize(), EntitySchema.GeometryEntityType.BoneCountAttribute, "Stats".Localize(), "Bone Count of the model".Localize(), isReadOnly: true),
			new ChildAttributePropertyDescriptor("Source File Path".Localize(), EntitySchema.EntitySourceFilePathType.PathAttribute, EntitySchema.GeometryEntityType.SourceFilePathChild, "Source".Localize(), "Source file for this entity.".Localize(), isReadOnly: false, new SourceFileTypeUITypeEditor(m_fileDialogService)),
			new AttributePropertyDescriptor("Source Object".Localize(), EntitySchema.GeometryEntityType.SourceObjectNameAttribute, "Source".Localize(), "Source object that this texture came from.".Localize(), isReadOnly: false, new SourceObjectUITypeEditor(), new SourceObjectTypeConverter()),
			new AttributePropertyDescriptor("Exported Time".Localize(), EntitySchema.GeometryEntityType.ExportedTimeAttribute, "Source".Localize(), "Last time that this texture was exported from its Source File.".Localize(), isReadOnly: true, new DateTimeEditor()),
			new ChildPropertyDescriptor("Data Files".Localize(), EntitySchema.GeometryEntityType.DataFilesChild, "Data".Localize(), "Data files associated with entity".Localize(), isReadOnly: true, embeddedCollectionEditor, null)
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type2 = EntitySchema.GeoMeshType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[5]
		{
			new AttributePropertyDescriptor("Name".Localize(), EntitySchema.GeoMeshType.NameAttribute, "Traits".Localize(), "Name of the geometry mesh.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Primitive Count".Localize(), EntitySchema.GeoMeshType.PrimitiveCountAttribute, "Traits".Localize(), "Number of primitives in this mesh.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Vertex Count".Localize(), EntitySchema.GeoMeshType.VertexCountAttribute, "Traits".Localize(), "Number of vertices in this mesh.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Bound Bone Count".Localize(), EntitySchema.GeoMeshType.BoundBoneCountAttribute, "Traits".Localize(), "Number of bound bones in this mesh.".Localize(), isReadOnly: true),
			new ChildPropertyDescriptor("Triangle Groups".Localize(), EntitySchema.GeoMeshType.PrimGroupsChild, "Traits".Localize(), "Triangle Groups (aka materials in Max) in this mesh.".Localize(), isReadOnly: true, embeddedCollectionEditor, null)
		};
		properties = array;
		type2.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type3 = EntitySchema.GeoPrimGroupType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[3]
		{
			new AttributePropertyDescriptor("Name".Localize(), EntitySchema.GeoPrimGroupType.NameAttribute, "Traits".Localize(), "Name of the triangle group (aka material name).".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Primitive Count".Localize(), EntitySchema.GeoPrimGroupType.PrimCountAttribute, "Traits".Localize(), "Number of primitives in this triangle group.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("First Primitive Index".Localize(), EntitySchema.GeoPrimGroupType.FirstPrimIndexAttribute, "Traits".Localize(), "Index of the first primitive for this triangle group.".Localize(), isReadOnly: true)
		};
		properties = array;
		type3.SetTag(new PropertyDescriptorCollection(properties));
		DomNodeType type4 = EntitySchema.GeoModelType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[2]
		{
			new ChildPropertyDescriptor("Geometry Meshes".Localize(), EntitySchema.GeoModelType.GeometryMeshesChild, "Traits".Localize(), "Geometry meshes in this model.".Localize(), isReadOnly: true, embeddedCollectionEditor, null),
			new ChildPropertyDescriptor("Bones".Localize(), EntitySchema.GeoModelType.BonesChild, "Traits".Localize(), "Bone names in this model.".Localize(), isReadOnly: true, embeddedCollectionEditor, null)
		};
		properties = array;
		type4.SetTag(new PropertyDescriptorCollection(properties));
	}

	private void DefineLightRigInstanceAdapters()
	{
		DomNodeType type = EntitySchema.LightReferenceType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[1]
		{
			new AttributePropertyDescriptor("Light Name".Localize(), EntitySchema.LightReferenceType.NameAttribute, "Basic".Localize(), "Light name for this reference".Localize(), isReadOnly: true)
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
		string[] sortedClassNames = GetSortedClassNames(InstanceType.IT_LIGHT_RIG);
		DomNodeType type2 = EntitySchema.LightRigEntityType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[3]
		{
			new EntityNameAttributePropertyDescriptor("Name".Localize(), EntitySchema.LightRigEntityType.NameAttribute, "Basic".Localize(), "Name of this entity".Localize(), isReadOnly: false),
			new EntityClassNameAttributePropertyDescriptor("Class Name".Localize(), EntitySchema.LightRigEntityType.ClassNameAttribute, "Basic".Localize(), "Associated class".Localize(), isReadOnly: false, new EnumUITypeEditor(sortedClassNames), new ExclusiveEnumTypeConverter(sortedClassNames), onlyNewCanChange: false),
			new AttributePropertyDescriptor("DSG".Localize(), EntitySchema.LightRigEntityType.DSGAttribute, "Basic".Localize(), "State graph associated with this entity".Localize(), isReadOnly: false, new AssetBrowserNameEditor(m_civTechService, m_assetBrowserDialogService, new InstanceType[1] { InstanceType.IT_DSG }))
		};
		properties = array;
		type2.SetTag(new PropertyDescriptorCollection(properties));
	}

	private void DefineMaterialInstanceAdapters()
	{
		string[] sortedClassNames = GetSortedClassNames(InstanceType.IT_MATERIAL);
		DomNodeType type = EntitySchema.MaterialInstanceType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[2]
		{
			new EntityNameAttributePropertyDescriptor("Name".Localize(), EntitySchema.MaterialInstanceType.NameAttribute, "Basic".Localize(), "Name of this entity".Localize(), isReadOnly: false),
			new EntityClassNameAttributePropertyDescriptor("Class Name".Localize(), EntitySchema.MaterialInstanceType.ClassNameAttribute, "Basic".Localize(), "Associated class".Localize(), isReadOnly: false, new EnumUITypeEditor(sortedClassNames), new ExclusiveEnumTypeConverter(sortedClassNames), onlyNewCanChange: false)
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
	}

	private void DefineFireFXInstanceAdapters()
	{
		string[] sortedClassNames = GetSortedClassNames(InstanceType.IT_FIREFX);
		DomNodeType type = EntitySchema.FireFXInstanceType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[2]
		{
			new EntityNameAttributePropertyDescriptor("Name".Localize(), EntitySchema.FireFXInstanceType.NameAttribute, "Basic".Localize(), "Name of this entity".Localize(), isReadOnly: false),
			new EntityClassNameAttributePropertyDescriptor("Class Name".Localize(), EntitySchema.FireFXInstanceType.ClassNameAttribute, "Basic".Localize(), "Associated class".Localize(), isReadOnly: false, new EnumUITypeEditor(sortedClassNames), new ExclusiveEnumTypeConverter(sortedClassNames), onlyNewCanChange: false)
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
	}

	private void DefineParticleEffectInstanceAdapters()
	{
		EmbeddedCollectionEditor embeddedCollectionEditor = new EmbeddedCollectionEditor();
		embeddedCollectionEditor.ShowCollectionToolbar = false;
		embeddedCollectionEditor.ShowItemLabels = false;
		embeddedCollectionEditor.ItemLabelsType = EmbeddedCollectionEditor.ItemLabelType.kNone;
		string[] sortedClassNames = GetSortedClassNames(InstanceType.IT_PARTICLE_EFFECT);
		DomNodeType type = EntitySchema.ParticleEffectEntityType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[7]
		{
			new EntityNameAttributePropertyDescriptor("Name".Localize(), EntitySchema.ParticleEffectEntityType.NameAttribute, "Basic".Localize(), "Name of this entity".Localize(), isReadOnly: false),
			new EntityClassNameAttributePropertyDescriptor("Class Name".Localize(), EntitySchema.ParticleEffectEntityType.ClassNameAttribute, "Basic".Localize(), "Associated class".Localize(), isReadOnly: false, new EnumUITypeEditor(sortedClassNames), new ExclusiveEnumTypeConverter(sortedClassNames), onlyNewCanChange: false),
			new ChildAttributePropertyDescriptor("Source File Path".Localize(), EntitySchema.EntitySourceFilePathType.PathAttribute, EntitySchema.ParticleEffectEntityType.SourceFilePathChild, "Source".Localize(), "Source file for this entity.".Localize(), isReadOnly: false, new SourceFileTypeUITypeEditor(m_fileDialogService)),
			new AttributePropertyDescriptor("Source Object".Localize(), EntitySchema.ParticleEffectEntityType.SourceObjectNameAttribute, "Source".Localize(), "Source object that this texture came from.".Localize(), isReadOnly: false, new SourceObjectUITypeEditor(), new SourceObjectTypeConverter()),
			new AttributePropertyDescriptor("Imported Time".Localize(), EntitySchema.ParticleEffectEntityType.ImportedTimeAttribute, "Source".Localize(), "Last time that this texture was imported to Perforce.".Localize(), isReadOnly: true, new DateTimeEditor()),
			new AttributePropertyDescriptor("Exported Time".Localize(), EntitySchema.ParticleEffectEntityType.ExportedTimeAttribute, "Source".Localize(), "Last time that this texture was exported from its Source File.".Localize(), isReadOnly: true, new DateTimeEditor()),
			new ChildPropertyDescriptor("Data Files".Localize(), EntitySchema.ParticleEffectEntityType.DataFilesChild, "Data".Localize(), "Data files associated with entity".Localize(), isReadOnly: true, embeddedCollectionEditor, null)
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
	}

	private void DefineTextureInstanceAdapters()
	{
		EmbeddedCollectionEditor embeddedCollectionEditor = new EmbeddedCollectionEditor();
		embeddedCollectionEditor.ShowCollectionToolbar = false;
		embeddedCollectionEditor.ShowItemLabels = true;
		embeddedCollectionEditor.ItemLabelsType = EmbeddedCollectionEditor.ItemLabelType.kName;
		string[] sortedClassNames = GetSortedClassNames(InstanceType.IT_TEXTURE);
		DomNodeType type = EntitySchema.TextureEntityType.Type;
		System.ComponentModel.PropertyDescriptor[] array = new Sce.Atf.Dom.PropertyDescriptor[12]
		{
			new EntityNameAttributePropertyDescriptor("Name".Localize(), EntitySchema.TextureEntityType.NameAttribute, "Basic".Localize(), "Name of this entity".Localize(), isReadOnly: false),
			new EntityClassNameAttributePropertyDescriptor("Class Name".Localize(), EntitySchema.TextureEntityType.ClassNameAttribute, "Basic".Localize(), "Associated class".Localize(), isReadOnly: false, new EnumUITypeEditor(sortedClassNames), new ExclusiveEnumTypeConverter(sortedClassNames), onlyNewCanChange: false),
			new ChildAttributePropertyDescriptor("Source File Path".Localize(), EntitySchema.EntitySourceFilePathType.PathAttribute, EntitySchema.TextureEntityType.SourceFilePathChild, "Source".Localize(), "Source file for this entity.".Localize(), isReadOnly: false, new SourceFileTypeUITypeEditor(m_fileDialogService)),
			new AttributePropertyDescriptor("Source Object".Localize(), EntitySchema.TextureEntityType.SourceObjectNameAttribute, "Source".Localize(), "Source object that this texture came from.".Localize(), isReadOnly: false, new SourceObjectUITypeEditor(), new SourceObjectTypeConverter()),
			new AttributePropertyDescriptor("Imported Time".Localize(), EntitySchema.TextureEntityType.ImportedTimeAttribute, "Source".Localize(), "Last time that this texture was imported to Perforce.".Localize(), isReadOnly: true, new DateTimeEditor()),
			new AttributePropertyDescriptor("Exported Time".Localize(), EntitySchema.TextureEntityType.ExportedTimeAttribute, "Source".Localize(), "Last time that this texture was exported from its Source File.".Localize(), isReadOnly: true, new DateTimeEditor()),
			new ChildPropertyDescriptor("Data Files".Localize(), EntitySchema.TextureEntityType.DataFilesChild, "Data".Localize(), "Data files associated with entity".Localize(), isReadOnly: true, embeddedCollectionEditor, null),
			new ChildPropertyDescriptor("Export Settings".Localize(), EntitySchema.TextureEntityType.ExportSettingsChild, "Export Settings".Localize(), "Export settings used for this entity.".Localize(), isReadOnly: false, embeddedCollectionEditor, null),
			new AttributePropertyDescriptor("Height".Localize(), EntitySchema.TextureEntityType.HeightAttribute, "Data".Localize(), "Height of the data file.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Width".Localize(), EntitySchema.TextureEntityType.WidthAttribute, "Data".Localize(), "Width of the data file.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Depth".Localize(), EntitySchema.TextureEntityType.DepthAttribute, "Data".Localize(), "Depth of the data file.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("NumMips".Localize(), EntitySchema.TextureEntityType.NumMipsAttribute, "Data".Localize(), "Number of mip maps in the data file.".Localize(), isReadOnly: true)
		};
		System.ComponentModel.PropertyDescriptor[] properties = array;
		type.SetTag(new PropertyDescriptorCollection(properties));
		string[] names = Enum.GetNames(typeof(PixelFormat));
		Enum.GetNames(typeof(ExportMode));
		string[] names2 = Enum.GetNames(typeof(FilterType));
		DomNodeType type2 = EntitySchema.TextureExportSettingsType.Type;
		array = new Sce.Atf.Dom.PropertyDescriptor[17]
		{
			new AttributePropertyDescriptor("Pixel Format".Localize(), EntitySchema.TextureExportSettingsType.PixelFormatAttribute, "Export Settings".Localize(), "Format the DDS will be in.".Localize(), isReadOnly: true, new EnumUITypeEditor(names), new ExclusiveEnumTypeConverter(names)),
			new AttributePropertyDescriptor("Filter Type".Localize(), EntitySchema.TextureExportSettingsType.FilterTypeAttribute, "Export Settings".Localize(), "Filter to use during the export process to generate mip maps.  Not applicable in Fast Export Mode.".Localize(), isReadOnly: false, new EnumUITypeEditor(names2), new ExclusiveEnumTypeConverter(names2)),
			new AttributePropertyDescriptor("Sample from Top".Localize(), EntitySchema.TextureExportSettingsType.SampleFromTopLevelAttribute, "Export Settings".Localize(), "If set to true, the export process will sample from the top layer instead of from the previous mip when generating mips.".Localize(), isReadOnly: false, new BoolEditor()),
			new AttributePropertyDescriptor("Export Mode".Localize(), EntitySchema.TextureExportSettingsType.ExportModeAttribute, "Export Settings".Localize(), "The type of texture being exported.".Localize(), isReadOnly: false, new SourceObjectUITypeEditor(), new SourceObjectTypeConverter()),
			new AttributePropertyDescriptor("Use Mips".Localize(), EntitySchema.TextureExportSettingsType.UseMipsAttribute, "Export Settings".Localize(), "If set to true, the export process will generate mip maps.".Localize(), isReadOnly: false, new BoolEditor()),
			new AttributePropertyDescriptor("Number of Manual Mips".Localize(), EntitySchema.TextureExportSettingsType.NumManualMipsAttribute, "Export Settings".Localize(), "The number of mip maps created by the artist.  Only applicable in Manual Mips export.".Localize(), isReadOnly: false),
			new AttributePropertyDescriptor("Complete Mip Chain".Localize(), EntitySchema.TextureExportSettingsType.CompleteMipChainAttribute, "Export Settings".Localize(), "If set to true, the export process will use the last supplied mip to create the rest of the mip maps.  Only applicable in Manual Mips export.".Localize(), isReadOnly: false, new BoolEditor()),
			new AttributePropertyDescriptor("Value Clamp Max".Localize(), EntitySchema.TextureExportSettingsType.ValueClampMaxAttribute, "Export Settings".Localize(), "Clamps the maximum pixel value to this value.  Not applicable in Fast Export Mode.".Localize(), isReadOnly: false),
			new AttributePropertyDescriptor("Value Clamp Min".Localize(), EntitySchema.TextureExportSettingsType.ValueClampMinAttribute, "Export Settings".Localize(), "Clamps the minimum pixel value to this value.  Not applicable in Fast Export Mode.".Localize(), isReadOnly: false),
			new AttributePropertyDescriptor("Scale".Localize(), EntitySchema.TextureExportSettingsType.SupportScaleAttribute, "Export Settings".Localize(), "Amount to scale this texture by during the export process.  Not applicable in Fast Export Mode.".Localize(), isReadOnly: false),
			new AttributePropertyDescriptor("Gamma In".Localize(), EntitySchema.TextureExportSettingsType.GammaInAttribute, "Export Settings".Localize(), "The gamma value of the input texture.  Used to convert from gamma space to linear space.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Gamma Out".Localize(), EntitySchema.TextureExportSettingsType.GammaOutAttribute, "Export Settings".Localize(), "The gamma value of the output texture.  Used to convert from linear space to gamma space.".Localize(), isReadOnly: true),
			new AttributePropertyDescriptor("Slab Width".Localize(), EntitySchema.TextureExportSettingsType.SlabWidthAttribute, "Export Settings".Localize(), "Width of the resulting 3D Texture.  Only applicable in 3D Export mode.".Localize(), isReadOnly: false),
			new AttributePropertyDescriptor("Slab Height".Localize(), EntitySchema.TextureExportSettingsType.SlabHeightAttribute, "Export Settings".Localize(), "Height of the resulting 3D Texture.  Only applicable in 3D Export mode.".Localize(), isReadOnly: false),
			new AttributePropertyDescriptor("Color Key X".Localize(), EntitySchema.TextureExportSettingsType.ColorKeyXAttribute, "Export Settings".Localize(), "Number of horizontal pixels to sample for the color key.".Localize(), isReadOnly: false),
			new AttributePropertyDescriptor("Color Key Y".Localize(), EntitySchema.TextureExportSettingsType.ColorKeyYAttribute, "Export Settings".Localize(), "Number of vertical pixels to sample for the color key.".Localize(), isReadOnly: false),
			new AttributePropertyDescriptor("Color Key Z".Localize(), EntitySchema.TextureExportSettingsType.ColorKeyZAttribute, "Export Settings".Localize(), "Number of depth pixels to sample for the color key.".Localize(), isReadOnly: false)
		};
		properties = array;
		type2.SetTag(new PropertyDescriptorCollection(properties));
	}
}
