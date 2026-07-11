using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Firaxis.AssetEditing;

[Export(typeof(AssetCommands))]
[Export(typeof(IInitializable))]
[Export(typeof(IContextMenuCommandProvider))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AssetCommands : ICommandClient, IInitializable, IContextMenuCommandProvider
{
	private enum Command
	{
		FixAsset
	}

	private struct AssetCommandTag
	{
		public Command Command;

		public AssetCommandTag(Command command)
		{
			Command = command;
		}
	}

	private static AssetCommandTag FixAssetCommandTag = new AssetCommandTag(Command.FixAsset);

	private ICommandService CommandService { get; set; }

	private IDocumentRegistry DocumentRegistry { get; set; }

	private IFileDialogService FileDialogService { get; set; }

	private ICivTechService CivTechService { get; set; }

	[ImportingConstructor]
	public AssetCommands(ICommandService commandService, IDocumentRegistry documentRegistry, IFileDialogService fileDialogService, ICivTechService civTechSvc)
	{
		CommandService = commandService;
		DocumentRegistry = documentRegistry;
		FileDialogService = fileDialogService;
		CivTechService = civTechSvc;
	}

	public virtual void Initialize()
	{
		RegisterClientCommands();
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		if (!(commandTag is AssetCommandTag assetCommandTag))
		{
			return false;
		}
		if (DocumentRegistry.ActiveDocument == null)
		{
			return false;
		}
		if (DocumentRegistry.ActiveDocument.IsReadOnly)
		{
			return false;
		}
		AssetDocument activeDocument = DocumentRegistry.GetActiveDocument<AssetDocument>();
		if (activeDocument == null)
		{
			return false;
		}
		activeDocument.As<AssetContext>();
		if (assetCommandTag.Command != Command.FixAsset)
		{
			return activeDocument != null;
		}
		return true;
	}

	public virtual void DoCommand(object commandTag)
	{
		if (commandTag is AssetCommandTag assetCommandTag)
		{
			IDocument activeDocument = DocumentRegistry.ActiveDocument;
			if (assetCommandTag.Command == Command.FixAsset)
			{
				DocumentRegistry.ActiveDocument.As<AssetContext>();
				SanitizeAndValidateAsset((AssetDocument)activeDocument);
			}
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object clicked)
	{
		if (context.As<ISelectionContext>() != null)
		{
			return new object[1] { FixAssetCommandTag };
		}
		return EmptyEnumerable<object>.Instance;
	}

	private void SanitizeAndValidateAsset(AssetDocument assetDocument)
	{
		assetDocument.DomNode.As<AssetContext>();
		AssetAdapter assetAdapter = assetDocument.As<AssetAdapter>();
		CivTechContext civTechContext = Context.EnsureCreated<CivTechContext>();
		_ = CivTechService.PrimaryProject.Paths.GamePantry;
		using IInstanceSet instanceSet = civTechContext.CreateInstance<IInstanceSet>(new object[1] { CivTechService.GetActivePantryPaths() });
		List<IGeometryInstance> list = new List<IGeometryInstance>();
		ModelInstanceAdapter[] array = assetAdapter.GeometrySet.ModelInstances.ToArray();
		foreach (ModelInstanceAdapter modelInstanceAdapter in array)
		{
			IGeometryInstance geometryInstance = instanceSet.LoadEntityIfUnique<IGeometryInstance>(modelInstanceAdapter.GeoName);
			if (geometryInstance != null)
			{
				list.Add(geometryInstance);
				assetDocument.UpdateAssetModels(geometryInstance);
				assetDocument.Dirty = true;
				assetAdapter.GeometrySet.Update();
				assetAdapter.Update();
			}
		}
		List<string> list2 = new List<string>();
		List<TimelineAdapter> list3 = new List<TimelineAdapter>();
		List<string> list4 = new List<string>();
		IDSGInstance iDSGInstance = instanceSet.LoadEntityIfUnique<IDSGInstance>(assetAdapter.DSG);
		if (iDSGInstance == null)
		{
			return;
		}
		List<string> list5 = iDSGInstance.GetTimelineSlots().ToList();
		List<string> list6 = iDSGInstance.GetAnimationSlots().ToList();
		foreach (IAnimationBinding binding in assetAdapter.BehaviorData.AnimationBindings.Bindings)
		{
			if (!list6.Contains(binding.SlotName))
			{
				list4.Add(binding.SlotName);
			}
		}
		foreach (TimelineAdapter timeline in assetAdapter.TimelineSet.Timelines)
		{
			if (!list5.Contains(timeline.Name))
			{
				list3.Add(timeline);
			}
		}
		foreach (ITimelineBinding binding2 in assetAdapter.BehaviorData.TimelineBindings.Bindings)
		{
			if (!list6.Contains(binding2.SlotName))
			{
				list2.Add(binding2.SlotName);
			}
		}
		foreach (TimelineAdapter item in list3)
		{
			assetAdapter.BehaviorData.Timelines.RemoveTimeline(item.Name);
		}
		foreach (string item2 in list4)
		{
			assetAdapter.BehaviorData.AnimationBindings.Unbind(item2);
		}
		foreach (string item3 in list2)
		{
			assetAdapter.BehaviorData.TimelineBindings.Unbind(item3);
		}
	}

	private void RegisterClientCommands()
	{
		CommandService.RegisterCommand(new CommandInfo(FixAssetCommandTag, StandardMenu.Edit, StandardCommandGroup.EditOther, "Fix My Asset".Localize("Name of a command"), "Attempts to do some cleanup some difficult to track down asset bugs".Localize(), Keys.None, Resources.AddClassIcon, CommandVisibility.Menu), this);
	}
}
