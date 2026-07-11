using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sce.Atf;

namespace Firaxis.AssetEditing;

public static class Resources
{
	[IconResource("Category.Animations.ico")]
	public static readonly string AnimationsCategoryIcon;

	[IconResource("Category.Attachments.ico")]
	public static readonly string AttachmentsCategoryIcon;

	[IconResource("Category.Base.ico")]
	public static readonly string BaseCategoryIcon;

	[IconResource("Category.Behavior.ico")]
	public static readonly string BehaviorCategoryIcon;

	[IconResource("Category.Camera.ico")]
	public static readonly string CameraCategoryIcon;

	[IconResource("Category.CookParameters.ico")]
	public static readonly string CookParametersCategoryIcon;

	[IconResource("Category.Geometry.ico")]
	public static readonly string GeometryCategoryIcon;

	[IconResource("Category.HUD.ico")]
	public static readonly string HUDCategoryIcon;

	[IconResource("Category.Lighting.ico")]
	public static readonly string LightingCategoryIcon;

	[IconResource("Category.Particles.ico")]
	public static readonly string ParticlesCategoryIcon;

	[IconResource("Category.Spline.ico")]
	public static readonly string SplineCategoryIcon;

	[ImageResource("AddClass16.png", "AddClass24.png", "AddClass48.png")]
	public static readonly string SelectWidgetIcon;

	[IconResource("Widgets.Move.ico")]
	public static readonly string MoveWidgetIcon;

	[IconResource("Widgets.Rotate.ico")]
	public static readonly string RotateWidgetIcon;

	[IconResource("Widgets.Scale.ico")]
	public static readonly string ScaleWidgetIcon;

	[IconResource("Widgets.None.ico")]
	public static readonly string NoneWidgetIcon;

	[IconResource("AssetCommands.AddItem.ico")]
	public static readonly string AddItemIcon;

	[IconResource("FileCommands.NewFile.ico")]
	public static readonly string NewFileIcon;

	[IconResource("AssetCommands.Reimport.ico")]
	public static readonly string ReimportFileIcon;

	[IconResource("AssetCommands.OpenSource.ico")]
	public static readonly string OpenSourceFileIcon;

	[IconResource("AssetCommands.RemoveItem.ico")]
	public static readonly string RemoveItemIcon;

	[IconResource("AssetCommands.FixReferences.ico")]
	public static readonly string FixLinksIcon;

	[IconResource("FileCommands.OpenXLP.ico")]
	public static readonly string OpenXLPIcon;

	[IconResource("FileCommands.OpenArtDef.ico")]
	public static readonly string OpenArtDefIcon;

	[IconResource("FileTypes.AnalyticLight.ico")]
	public static readonly string AnalyticLightFileIcon;

	[IconResource("FileTypes.Animation.ico")]
	public static readonly string AnimationFileIcon;

	[IconResource("FileTypes.ArtDef.ico")]
	public static readonly string ArtDefFileIcon;

	[IconResource("FileTypes.Asset.ico")]
	public static readonly string AssetFileIcon;

	[IconResource("FileTypes.Behavior.ico")]
	public static readonly string BehaviorFileIcon;

	[IconResource("FileTypes.EnvironmentMap.ico")]
	public static readonly string EnvironmentMapFileIcon;

	[IconResource("FileTypes.FireFX.ico")]
	public static readonly string FireFXFileIcon;

	[IconResource("FileTypes.Geometry.ico")]
	public static readonly string GeometryFileIcon;

	[IconResource("FileTypes.LightRig.ico")]
	public static readonly string LightRigFileIcon;

	[IconResource("FileTypes.Material.ico")]
	public static readonly string MaterialFileIcon;

	[IconResource("FileTypes.ParticleEffect.ico")]
	public static readonly string ParticleEffectFileIcon;

	[IconResource("FileTypes.Texture.ico")]
	public static readonly string TextureFileIcon;

	[IconResource("FileTypes.XLP.ico")]
	public static readonly string XLPFileIcon;

	[IconResource("Timeline.AddTimeline.ico")]
	public static readonly string AddTimelineTimelineIcon;

	[IconResource("Timeline.AddTrack.ico")]
	public static readonly string AddTrackTimelineIcon;

	[IconResource("Timeline.DeleteTrack.ico")]
	public static readonly string DeleteTrackTimelineIcon;

	[IconResource("Timeline.AddTrigger.ico")]
	public static readonly string AddTriggerTimelineIcon;

	[IconResource("Timeline.PasteAtTime.ico")]
	public static readonly string PasteAtTimeTimelineIcon;

	[IconResource("Timeline.ExpandAll.ico")]
	public static readonly string ExpandAllTimelineIcon;

	[IconResource("Timeline.ExpandAllWithTriggers.ico")]
	public static readonly string ExpandAllWithTriggersTimelineIcon;

	[IconResource("Timeline.CollapseAll.ico")]
	public static readonly string CollapseAllTimelineIcon;

	[IconResource("Timeline.CollapseAllButThis.ico")]
	public static readonly string CollapseAllButSelectedTimelineIcon;

	[IconResource("Timeline.StartPlayback.ico")]
	public static readonly string StartPlaybackTimelineIcon;

	[IconResource("Timeline.StopPlayback.ico")]
	public static readonly string StopPlaybackTimelineIcon;

	[IconResource("Timeline.PausePlayback.ico")]
	public static readonly string PausePlaybackTimelineIcon;

	[IconResource("Timeline.LoopPlayback.ico")]
	public static readonly string LoopPlaybackTimelineIcon;

	[IconResource("Timeline.GotoStart.ico")]
	public static readonly string GotoStartTimelineIcon;

	[IconResource("Timeline.GotoEnd.ico")]
	public static readonly string GotoEndTimelineIcon;

	[IconResource("Timeline.StepForward.ico")]
	public static readonly string StepForwardTimelineIcon;

	[IconResource("Timeline.StepBackward.ico")]
	public static readonly string StepBackwardTimelineIcon;

	[IconResource("FireFX.Compile.ico")]
	public static readonly string FireFXCompileIcon;

	[ImageResource("AddClass16.png", "AddClass24.png", "AddClass48.png")]
	public static readonly string AddClassIcon;

	[ImageResource("DeleteClass16.png", "DeleteClass24.png", "DeleteClass48.png")]
	public static readonly string DeleteClassIcon;

	[ImageResource("DuplicateClass16.png", "DuplicateClass24.png", "DuplicateClass48.png")]
	public static readonly string DuplicateClassIcon;

	[ImageResource("ReloadProject16.png", "ReloadProject24.png", "ReloadProject48.png")]
	public static readonly string ReloadProjectConfigIcon;

	[ImageResource("ObjectAdd16.png")]
	public static readonly string ObjectAddIcon;

	[ImageResource("ObjectNew16.png")]
	public static readonly string ObjectNewIcon;

	[ImageResource("ObjectOpen16.png")]
	public static readonly string ObjectOpenIcon;

	[ImageResource("ObjectReImport16.png")]
	public static readonly string ObjectReImportIcon;

	[ImageResource("ObjectClear16.png")]
	public static readonly string ObjectClearIcon;

	[ImageResource("AddNew16.png", "AddNew24.png", "AddNew48.png")]
	public static readonly string AddNewIcon;

	[ImageResource("AddExisting16.png", "AddExisting24.png", "AddExisting48.png")]
	public static readonly string AddExistingIcon;

	[IconResource("Misc.ArrowUp.ico")]
	public static readonly string ArrowUpIcon;

	[IconResource("Misc.ArrowDown.ico")]
	public static readonly string ArrowDownIcon;

	[IconResource("EditCommands.Delete.ico")]
	public static readonly string DeleteIcon;

	[IconResource("EditCommands.Copy.ico")]
	public static readonly string CopyIcon;

	[IconResource("EditCommands.Cut.ico")]
	public static readonly string CutIcon;

	[IconResource("EditCommands.Paste.ico")]
	public static readonly string PasteIcon;

	[ImageResource("ClearMaterials16.png")]
	public static readonly string ClearMaterialsIcon;

	[ImageResource("FillMaterials16.png")]
	public static readonly string FillMaterialsIcon;

	[ImageResource("BindAttachments.png")]
	public static readonly string BindAttachments;

	[IconResource("General.ChevronOpen.ico")]
	public static readonly string GeneralChevronOpenIcon;

	[IconResource("General.ChevronOpenHover.ico")]
	public static readonly string GeneralChevronOpenHoverIcon;

	[IconResource("General.ChevronClosed.ico")]
	public static readonly string GeneralChevronClosedIcon;

	[IconResource("General.ChevronClosedHover.ico")]
	public static readonly string GeneralChevronClosedHoverIcon;

	static Resources()
	{
		IEnumerable<MethodInfo> enumerable = from assembly in AppDomain.CurrentDomain.GetAssemblies()
			where assembly.FullName.StartsWith("Atf.Gui.WinForms")
			from type in assembly.GetExportedTypes()
			where type.Name == "ResourceUtil"
			from methodInfo in type.GetMethods()
			where methodInfo.Name == "Register" && methodInfo.IsStatic && methodInfo.IsPublic && methodInfo.ReturnType == typeof(void) && methodInfo.GetParameters().Length == 1 && methodInfo.GetParameters()[0].ParameterType == typeof(Type)
			select methodInfo;
		bool flag = false;
		foreach (MethodInfo item in enumerable)
		{
			if (flag)
			{
				throw new InvalidOperationException("More than one implementation of ResourceUtil.Register(Type type) has been found.  Only the first one will be called.");
			}
			Queue queue = new Queue(1);
			queue.Enqueue(typeof(Resources));
			item.Invoke(null, queue.ToArray());
			flag = true;
		}
	}
}
