using System.Collections.Generic;
using System.Drawing;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Firaxis.AssetEditing;

public class CurveEditingCommands : ICommandClient
{
	private enum CurveCommands
	{
		InsertConstantSegment,
		InsertLinearSegment,
		DeleteSelectedSegments,
		NudgeRight,
		NudgeLeft,
		NudgeUp,
		NudgeDown,
		SelectNext,
		SelectPrevious
	}

	public enum CurveCommandGroup
	{
		CurveCommands,
		SegmentCommands
	}

	private readonly ICurveEditingContext EditingContext;

	private readonly ISelectionContext SelectionContext;

	public static CommandInfo InsertConstantSegment = new CommandInfo(CurveCommands.InsertConstantSegment, StandardMenu.Edit, CurveCommandGroup.CurveCommands, "Insert Constant Segment".Localize(), "Inserts a constant segment at this location".Localize("Insert Constant Segment"), Keys.None, Resources.AddNewIcon, CommandVisibility.ControlDefaultRight);

	public static CommandInfo InsertLinearSegment = new CommandInfo(CurveCommands.InsertLinearSegment, StandardMenu.Edit, CurveCommandGroup.CurveCommands, "Insert Control Point".Localize(), "Inserts a control point at this location.".Localize("Insert Control Point"), Keys.None, Resources.AddNewIcon, CommandVisibility.ControlDefaultRight);

	public static CommandInfo DeleteSelectedSegments = new CommandInfo(CurveCommands.DeleteSelectedSegments, StandardMenu.Edit, CurveCommandGroup.CurveCommands, "Delete Selected Segments".Localize(), "Deletes the selected segments from the curve.".Localize("Delete Selected Segments"), Keys.Delete, Resources.RemoveItemIcon, CommandVisibility.ControlDefaultRight);

	public static CommandInfo NudgeRight = new CommandInfo(CurveCommands.NudgeRight, StandardMenu.Edit, CurveCommandGroup.SegmentCommands, "Segment\\Nudge Right".Localize(), "Move selected points 0.01f to the right.".Localize("Nudge Right"), Keys.Right, Resources.StepForwardTimelineIcon, CommandVisibility.ControlDefaultRight);

	public static CommandInfo NudgeLeft = new CommandInfo(CurveCommands.NudgeLeft, StandardMenu.Edit, CurveCommandGroup.SegmentCommands, "Segment\\Nudge Left".Localize(), "Move selected points 0.01f to the left.".Localize("Nudge Left"), Keys.Left, Resources.StepBackwardTimelineIcon, CommandVisibility.ControlDefaultRight);

	public static CommandInfo NudgeUp = new CommandInfo(CurveCommands.NudgeUp, StandardMenu.Edit, CurveCommandGroup.SegmentCommands, "Segment\\Nudge Up".Localize(), "Move selected points 0.01f up.".Localize("Nudge Up"), Keys.Up, Resources.ArrowUpIcon, CommandVisibility.ControlDefaultRight);

	public static CommandInfo NudgeDown = new CommandInfo(CurveCommands.NudgeDown, StandardMenu.Edit, CurveCommandGroup.SegmentCommands, "Segment\\Nudge Down".Localize(), "Move selected points 0.01f down.".Localize("Nudge Down"), Keys.Down, Resources.ArrowDownIcon, CommandVisibility.ControlDefaultRight);

	public static CommandInfo SelectNextControlPoint = new CommandInfo(CurveCommands.SelectNext, StandardMenu.Edit, CurveCommandGroup.SegmentCommands, "Segment\\Select Next".Localize(), "Select the next point in the curve.".Localize("Select Next"), Keys.Tab, string.Empty, CommandVisibility.ControlDefaultRight);

	public static CommandInfo SelectPreviousControlPoint = new CommandInfo(CurveCommands.SelectPrevious, StandardMenu.Edit, CurveCommandGroup.SegmentCommands, "Segment\\Select Previous".Localize(), "Select the previous point in the curve.".Localize("Select Previous"), Keys.Tab | Keys.Shift, string.Empty, CommandVisibility.ControlDefaultRight);

	private ICommandService CommandService { get; set; }

	public IEnumerable<CommandInfo> Commands { get; } = new CommandInfo[9] { InsertConstantSegment, InsertLinearSegment, DeleteSelectedSegments, NudgeRight, NudgeLeft, NudgeUp, NudgeDown, SelectNextControlPoint, SelectPreviousControlPoint };

	public CurveEditingCommands(ICommandService commandService, CurveEditingContext editingContext)
	{
		RegisterCommands(commandService);
		EditingContext = editingContext.As<ICurveEditingContext>();
		SelectionContext = editingContext.As<ISelectionContext>();
	}

	public void RegisterCommands(ICommandService commandService)
	{
		if (CommandService != commandService)
		{
			if (CommandService != null)
			{
				UnregisterCommands(CommandService);
			}
			CommandService = commandService;
			if (CommandService != null)
			{
				CommandService.RegisterCommand(InsertLinearSegment, this);
				CommandService.RegisterCommand(DeleteSelectedSegments, this);
				CommandService.RegisterCommand(NudgeRight, this);
				CommandService.RegisterCommand(NudgeLeft, this);
				CommandService.RegisterCommand(NudgeUp, this);
				CommandService.RegisterCommand(NudgeDown, this);
				CommandService.RegisterCommand(SelectNextControlPoint, this);
				CommandService.RegisterCommand(SelectPreviousControlPoint, this);
			}
		}
	}

	private void UnregisterCommands(ICommandService commandService)
	{
		commandService.UnregisterCommand(InsertLinearSegment, this);
		commandService.UnregisterCommand(DeleteSelectedSegments, this);
		commandService.UnregisterCommand(NudgeRight, this);
		commandService.UnregisterCommand(NudgeLeft, this);
		commandService.UnregisterCommand(NudgeUp, this);
		commandService.UnregisterCommand(NudgeDown, this);
		commandService.UnregisterCommand(SelectNextControlPoint, this);
		commandService.UnregisterCommand(SelectPreviousControlPoint, this);
	}

	public bool CanDoCommand(object commandTag)
	{
		if (SelectionContext == null)
		{
			return false;
		}
		if (!(commandTag is CurveCommands curveCommands))
		{
			return false;
		}
		switch (curveCommands)
		{
		case CurveCommands.InsertConstantSegment:
		case CurveCommands.InsertLinearSegment:
			return true;
		case CurveCommands.DeleteSelectedSegments:
		case CurveCommands.NudgeRight:
		case CurveCommands.NudgeLeft:
		case CurveCommands.NudgeUp:
		case CurveCommands.NudgeDown:
		case CurveCommands.SelectNext:
		case CurveCommands.SelectPrevious:
			return SelectionContext.SelectionCount > 0;
		default:
			BugSubmitter.Assert(condition: false, "Unknown CurveCommand enum!  Be sure to update CanDoCommand, DoCommand, and UpdateCommand when you modify the enum.");
			return false;
		}
	}

	public void DoCommand(object commandTag)
	{
		if (EditingContext != null && commandTag is CurveCommands)
		{
			switch ((CurveCommands)commandTag)
			{
			case CurveCommands.InsertConstantSegment:
			{
				PointF menuCurvePosition2 = EditingContext.MenuCurvePosition;
				EditingContext.InsertConstantCurveSegmentDefinition(menuCurvePosition2.X, menuCurvePosition2.Y);
				break;
			}
			case CurveCommands.InsertLinearSegment:
			{
				PointF menuCurvePosition = EditingContext.MenuCurvePosition;
				EditingContext.InsertLinearCurveSegmentDefinition(menuCurvePosition.X, menuCurvePosition.Y);
				break;
			}
			case CurveCommands.DeleteSelectedSegments:
				EditingContext.RemoveSelectedCurveSegments();
				break;
			case CurveCommands.NudgeRight:
				EditingContext.NudgeSelectedPointsRight();
				break;
			case CurveCommands.NudgeLeft:
				EditingContext.NudgeSelectedPointsLeft();
				break;
			case CurveCommands.NudgeUp:
				EditingContext.NudgeSelectedPointsUp();
				break;
			case CurveCommands.NudgeDown:
				EditingContext.NudgeSelectedPointsDown();
				break;
			case CurveCommands.SelectNext:
				EditingContext.SelectNextPoint();
				break;
			case CurveCommands.SelectPrevious:
				EditingContext.SelectPreviousPoint();
				break;
			default:
				BugSubmitter.Assert(condition: false, "Unknown CurveCommand enum!  Be sure to update CanDoCommand, DoCommand, and UpdateCommand when you modify the enum.");
				break;
			}
		}
	}

	public void UpdateCommand(object commandTag, CommandState commandState)
	{
	}
}
