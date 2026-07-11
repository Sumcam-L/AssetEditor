using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sce.Atf;

namespace Firaxis.ATF;

public static class Resources
{
	[IconResource("EnableTuner.ico")]
	public static readonly string EnableTunerIcon;

	[IconResource("Hotload.ico")]
	public static readonly string HotloadIcon;

	[IconResource("DisableTuner.ico")]
	public static readonly string DisableTunerIcon;

	[IconResource("OpenSource.ico")]
	public static readonly string OpenSourceIcon;

	[IconResource("GotoFile.ico")]
	public static readonly string GotoFileIcon;

	[IconResource("Animations.ico")]
	public static readonly string AnimationsCategoryIcon;

	[IconResource("Attachments.ico")]
	public static readonly string AttachmentsCategoryIcon;

	[IconResource("Base.ico")]
	public static readonly string BaseCategoryIcon;

	[IconResource("Behavior.ico")]
	public static readonly string BehaviorCategoryIcon;

	[IconResource("Camera.ico")]
	public static readonly string CameraCategoryIcon;

	[IconResource("CookParameters.ico")]
	public static readonly string CookParametersCategoryIcon;

	[IconResource("Geometry.ico")]
	public static readonly string GeometryCategoryIcon;

	[IconResource("HUD.ico")]
	public static readonly string HUDCategoryIcon;

	[IconResource("Lighting.ico")]
	public static readonly string LightingCategoryIcon;

	[IconResource("Particles.ico")]
	public static readonly string ParticlesCategoryIcon;

	[IconResource("Spline.ico")]
	public static readonly string SplineCategoryIcon;

	[IconResource("Move.ico")]
	public static readonly string MoveWidgetIcon;

	[IconResource("Rotate.ico")]
	public static readonly string RotateWidgetIcon;

	[IconResource("Scale.ico")]
	public static readonly string ScaleWidgetIcon;

	[IconResource("AddItem.ico")]
	public static readonly string AddItemIcon;

	[IconResource("AddExistingEntity.ico")]
	public static readonly string AddExistingEntityIcon;

	[IconResource("AddNewEntity.ico")]
	public static readonly string AddNewEntityIcon;

	[IconResource("NewFile.ico")]
	public static readonly string NewFileIcon;

	[IconResource("Reimport.ico")]
	public static readonly string ReimportFileIcon;

	[IconResource("OpenSource.ico")]
	public static readonly string OpenSourceFileIcon;

	[IconResource("Delete.ico")]
	public static readonly string DeleteIcon;

	[IconResource("RemoveItem.ico")]
	public static readonly string RemoveItemIcon;

	[IconResource("FixReferences.ico")]
	public static readonly string FixLinksIcon;

	[IconResource("OpenXLP.ico")]
	public static readonly string OpenXLPIcon;

	[IconResource("OpenArtDef.ico")]
	public static readonly string OpenArtDefIcon;

	[IconResource("OpenEntity.ico")]
	public static readonly string OpenEntityIcon;

	[IconResource("ShowGrid.ico")]
	public static readonly string ShowGridOutput;

	[IconResource("ShowText.ico")]
	public static readonly string ShowTextOutput;

	[IconResource("VerboseTool.ico")]
	public static readonly string ToggleVerboseToolOuput;

	[IconResource("VerboseEngine.ico")]
	public static readonly string ToggleVerboseEngineOuput;

	[IconResource("Civ6.ico")]
	public static readonly string Civ6Game;

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
