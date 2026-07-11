using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Firaxis.AssetEditing;

public class TimelinePreviewCommands : ICommandClient
{
	private enum PlaybackCommands
	{
		GotoStart,
		StepBackward,
		TogglePlayPause,
		Stop,
		StepForward,
		GotoEnd,
		ToggleLooping
	}

	public static CommandInfo GotoStart = new CommandInfo(PlaybackCommands.GotoStart, StandardMenu.View, TimelineCommandGroup.PlaybackCommands, "Timeline\\Go to Start".Localize(), "Go to Start of timeline".Localize("Go to Start"), Sce.Atf.Input.Keys.None, Resources.GotoStartTimelineIcon, CommandVisibility.ControlDefault);

	public static CommandInfo StepBackward = new CommandInfo(PlaybackCommands.StepBackward, StandardMenu.View, TimelineCommandGroup.PlaybackCommands, "Timeline\\Step backward".Localize(), "Step backward a single frame's duration (as defined by authored framerate)".Localize("Step backward"), Sce.Atf.Input.Keys.None, Resources.StepBackwardTimelineIcon, CommandVisibility.ControlDefault);

	public static CommandInfo TogglePlayPause = new CommandInfo(PlaybackCommands.TogglePlayPause, StandardMenu.View, TimelineCommandGroup.PlaybackCommands, "Timeline\\Toggle Playing".Localize(), "Toggle previewing between playing and paused state".Localize("Toggle Playing"), Sce.Atf.Input.Keys.None, Resources.StartPlaybackTimelineIcon, CommandVisibility.ControlDefault);

	public static CommandInfo Stop = new CommandInfo(PlaybackCommands.Stop, StandardMenu.View, TimelineCommandGroup.PlaybackCommands, "Timeline\\Stop Playback".Localize(), "Stop playback and reset shuttle to the beginning of the currently playing track".Localize("Stop Playback"), Sce.Atf.Input.Keys.None, Resources.StopPlaybackTimelineIcon, CommandVisibility.ControlDefault);

	public static CommandInfo StepForward = new CommandInfo(PlaybackCommands.StepForward, StandardMenu.View, TimelineCommandGroup.PlaybackCommands, "Timeline\\Step forward".Localize(), "Step forward a single frame's duration (as defined by authored framerate)".Localize("Step forward"), Sce.Atf.Input.Keys.None, Resources.StepForwardTimelineIcon, CommandVisibility.ControlDefault);

	public static CommandInfo GotoEnd = new CommandInfo(PlaybackCommands.GotoEnd, StandardMenu.View, TimelineCommandGroup.PlaybackCommands, "Timeline\\Go to End".Localize(), "Go to End of timeline".Localize("Go to End"), Sce.Atf.Input.Keys.None, Resources.GotoEndTimelineIcon, CommandVisibility.ControlDefault);

	public static CommandInfo ToggleLooping = new CommandInfo(PlaybackCommands.ToggleLooping, StandardMenu.View, TimelineCommandGroup.PlaybackCommands, "Timeline\\Toggle Looping".Localize(), "Toggle preview looping".Localize("Toggle Looping"), Sce.Atf.Input.Keys.None, Resources.LoopPlaybackTimelineIcon, CommandVisibility.ControlDefault);

	private StateTransitionInfo _timelineTransition = new StateTransitionInfo();

	private bool _lastPlayingState;

	private ICommandService CommandService { get; set; }

	private CommandControl CommandControl { get; set; }

	private ISelectionContext SelectionContext { get; set; }

	private IAnimationKnobService AnimationKnobService { get; set; }

	private ITimelinePlaybackService TimelinePlaybackService { get; set; }

	public bool LoopPlayback { get; private set; }

	public IEnumerable<CommandInfo> Commands { get; } = new CommandInfo[7] { ToggleLooping, GotoEnd, StepForward, Stop, TogglePlayPause, StepBackward, GotoStart };

	private StateTransitionInfo SelectedTimelineTransition
	{
		get
		{
			IList<StateTransitionInfo> value = null;
			string selectedPlaybackSlot = GetSelectedPlaybackSlot();
			if (AnimationKnobService.TimelineStateTransitions.TryGetValue(selectedPlaybackSlot, out value))
			{
				_timelineTransition = value[0];
			}
			else
			{
				BugSubmitter.SilentReport("Failed to find playback slot named \"" + selectedPlaybackSlot + "\" @summary Failed to find playback slot @assign bwhitman");
				_timelineTransition.Source = "ANY";
				_timelineTransition.Destination = selectedPlaybackSlot;
				_timelineTransition.AnimationGraphIndex = 0;
				_timelineTransition.Duration = -1f;
			}
			return _timelineTransition;
		}
	}

	public TimelinePreviewCommands(ICommandService cmdSvc, IAnimationKnobService animKnobSvc, ITimelinePlaybackService tlPlyBckSvc, CommandControl cmdCtl)
	{
		CommandService = cmdSvc;
		AnimationKnobService = animKnobSvc;
		TimelinePlaybackService = tlPlyBckSvc;
		TimelinePlaybackService.Playback = AnimationKnobService;
		CommandControl = cmdCtl;
	}

	public void Bind(ISelectionContext context)
	{
		if (SelectionContext != null)
		{
			SelectionContext.SelectionChanged -= SelectionContext_SelectionChanged;
		}
		SelectionContext = context;
		if (SelectionContext != null)
		{
			SelectionContext.SelectionChanged += SelectionContext_SelectionChanged;
		}
	}

	private void SelectionContext_SelectionChanged(object sender, EventArgs e)
	{
		if (IsTimelineSelected())
		{
			StateTransitionInfo selectedTimelineTransition = SelectedTimelineTransition;
			float currentTime = TimelinePlaybackService.CurrentTime;
			if (TimelinePlaybackService.Playing)
			{
				TimelinePlaybackService.Playing = false;
			}
			TimelinePlaybackService.Looping = LoopPlayback;
			TimelinePlaybackService.PlayAnimation(selectedTimelineTransition, currentTime, 0f, selectedTimelineTransition.Duration);
		}
	}

	private bool IsPlaying()
	{
		return TimelinePlaybackService.Playing;
	}

	private bool IsTimelineSelected()
	{
		if (SelectionContext == null)
		{
			return false;
		}
		if (SelectionContext.SelectionCount == 1)
		{
			if (!SelectionContext.LastSelected.Is<ITimelineBindingAdapter>())
			{
				return SelectionContext.LastSelected.Is<TrackAdapter>();
			}
			return true;
		}
		return false;
	}

	private string GetSelectedPlaybackSlot()
	{
		if (SelectionContext == null)
		{
			return string.Empty;
		}
		ITimelineBindingAdapter timelineBindingAdapter = SelectionContext.LastSelected?.As<ITimelineBindingAdapter>();
		if (timelineBindingAdapter != null)
		{
			return timelineBindingAdapter.SlotName;
		}
		TrackAdapter trackAdapter = SelectionContext.LastSelected?.As<TrackAdapter>();
		if (trackAdapter != null)
		{
			return trackAdapter.Timeline.Name;
		}
		TriggerAdapter triggerAdapter = SelectionContext.LastSelected?.As<TriggerAdapter>();
		if (triggerAdapter != null)
		{
			return triggerAdapter.TimelineAdapter.Name;
		}
		return string.Empty;
	}

	private bool IsAtStartOfPlayback()
	{
		return TimelinePlaybackService.CurrentTime == 0f;
	}

	private bool IsAtEndOfPlayback()
	{
		float num = SelectedTimelineTransition?.Duration ?? 0f;
		if (num >= 0f)
		{
			return TimelinePlaybackService.CurrentTime >= num;
		}
		return false;
	}

	public virtual bool CanDoCommand(object commandTag)
	{
		if (commandTag is PlaybackCommands)
		{
			if (string.IsNullOrEmpty(GetSelectedPlaybackSlot()))
			{
				return false;
			}
			switch ((PlaybackCommands)commandTag)
			{
			case PlaybackCommands.GotoStart:
			case PlaybackCommands.StepBackward:
				if (!IsPlaying() && IsTimelineSelected())
				{
					return !IsAtStartOfPlayback();
				}
				return false;
			case PlaybackCommands.TogglePlayPause:
				if (!IsPlaying())
				{
					return IsTimelineSelected();
				}
				return true;
			case PlaybackCommands.Stop:
				return IsPlaying();
			case PlaybackCommands.StepForward:
			case PlaybackCommands.GotoEnd:
				if (!IsPlaying() && IsTimelineSelected())
				{
					return !IsAtEndOfPlayback();
				}
				return false;
			case PlaybackCommands.ToggleLooping:
				return true;
			}
			BugSubmitter.Assert(condition: false, "Unknown TimelineTreeCommand enum! Be sure to update CanDoCommand, DoCommand, and UpdateCommand when you modify the enum");
		}
		return false;
	}

	public virtual void DoCommand(object commandTag)
	{
		if (commandTag is PlaybackCommands && !string.IsNullOrEmpty(GetSelectedPlaybackSlot()))
		{
			switch ((PlaybackCommands)commandTag)
			{
			case PlaybackCommands.GotoStart:
				TimelinePlaybackService.CurrentTime = 0f;
				break;
			case PlaybackCommands.StepBackward:
				TimelinePlaybackService.CurrentTime = AnimationKnobService.CurrentTime - 1f / 60f;
				break;
			case PlaybackCommands.TogglePlayPause:
				TimelinePlaybackService.Playing = !TimelinePlaybackService.Playing;
				break;
			case PlaybackCommands.Stop:
				TimelinePlaybackService.Playing = false;
				TimelinePlaybackService.CurrentTime = 0f;
				AnimationKnobService.Reset();
				break;
			case PlaybackCommands.StepForward:
				TimelinePlaybackService.CurrentTime = AnimationKnobService.CurrentTime + 1f / 60f;
				break;
			case PlaybackCommands.GotoEnd:
				TimelinePlaybackService.CurrentTime = SelectedTimelineTransition.Duration;
				break;
			case PlaybackCommands.ToggleLooping:
				TimelinePlaybackService.Looping = !TimelinePlaybackService.Looping;
				break;
			default:
				BugSubmitter.Assert(condition: false, "Unknown TimelineTreeCommand enum! Be sure to update CanDoCommand, DoCommand, and UpdateCommand when you modify the enum");
				break;
			}
		}
	}

	public virtual void UpdateCommand(object commandTag, CommandState commandState)
	{
		if (!(commandTag is PlaybackCommands playbackCommands))
		{
			return;
		}
		switch (playbackCommands)
		{
		case PlaybackCommands.ToggleLooping:
			if (commandState.Check != TimelinePlaybackService.Looping)
			{
				commandState.Check = TimelinePlaybackService.Looping;
			}
			break;
		case PlaybackCommands.TogglePlayPause:
		{
			bool playing = TimelinePlaybackService.Playing;
			if (_lastPlayingState != playing)
			{
				ToolStripButton button = null;
				CommandControl.GetButton(TogglePlayPause, out button);
				if (playing)
				{
					button.Image = ResourceUtil.GetImage24(Resources.PausePlaybackTimelineIcon);
				}
				else
				{
					button.Image = ResourceUtil.GetImage24(Resources.StartPlaybackTimelineIcon);
				}
				_lastPlayingState = playing;
			}
			break;
		}
		}
	}
}
