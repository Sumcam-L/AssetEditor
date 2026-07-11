using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IInitializable))]
[Export(typeof(TimelineDockWindow))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TimelineDockWindow : IInitializable, IControlHostClient
{
	[Import(AllowDefault = false)]
	private IControlHostService ControlHostService { get; set; }

	[Import(AllowDefault = false)]
	private ISettingsService SettingsService { get; set; }

	private IAnimationKnobService AnimationKnobService { get; set; }

	private ITimelinePlaybackService TimelinePlaybackService { get; set; }

	private IDocumentRegistry DocumentRegistry { get; set; }

	private ICommandService CommandService { get; set; }

	private TimelineEditorControl TimelineEditorControl { get; set; }

	[ImportingConstructor]
	public TimelineDockWindow(IThemeService themeSvc, IDocumentRegistry docReg, ICommandService cmdSvc, ITimelineTrackCommands timeTrkCmds, IAnimationKnobService animKnobSvc, ITimelinePlaybackService playbackSvc, StandardEditCommands stdEditCmds)
	{
		DocumentRegistry = docReg;
		CommandService = cmdSvc;
		AnimationKnobService = animKnobSvc;
		TimelinePlaybackService = playbackSvc;
		DocumentRegistry.ActiveDocumentChanging += HandleActiveDocumentChanging;
		DocumentRegistry.ActiveDocumentChanged += HandleActiveDocumentChanged;
		TimelineEditorControl = new TimelineEditorControl("", cmdSvc, themeSvc, AnimationKnobService, TimelinePlaybackService, timeTrkCmds, stdEditCmds);
	}

	private void HandlePreviewerContextChanged(object sender, EventArgs e)
	{
		if (AnimationKnobService != null)
		{
			_ = AnimationKnobService.TimelineStateTransitions.Count;
		}
	}

	public virtual void Initialize()
	{
		BugSubmitter.SilentAssert(ControlHostService != null, "MEF Catalog must have a component that provides an IControlHostService export. TimelineDockWIndow disabled.");
		if (ControlHostService != null)
		{
			ControlInfo controlInfo = new ControlInfo("Timeline Editor", "Timeline editing utility view", StandardControlGroup.Bottom);
			controlInfo.ControlVisibility = ControlInitialVisibility.InitiallyHidden;
			controlInfo.MenuText = "Timeline Editor";
			controlInfo.MenuGroupOverride = StandardCommandGroup.UILayout;
			ControlHostService.RegisterControl(TimelineEditorControl, controlInfo, this);
			DocumentRegistry.ActiveDocumentChanged += DocumentRegistry_ActiveDocumentChanged;
		}
	}

	private void DocumentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
	{
		EditingContext context = DocumentRegistry?.ActiveDocument?.As<EditingContext>();
		TimelineEditorControl?.Bind(context);
	}

	public virtual void Activate(Control control)
	{
	}

	public virtual void Deactivate(Control control)
	{
	}

	public virtual bool Close(Control control)
	{
		return true;
	}

	private void HandleActiveDocumentChanging(object sender, EventArgs e)
	{
		if (ControlHostService != null && DocumentRegistry.GetActiveDocument<IBehaviorProviderAdapter>() != null && TimelinePlaybackService != null)
		{
			TimelinePlaybackService.Playing = false;
			TimelinePlaybackService.StopIdleProcessExecution();
		}
	}

	private void HandleActiveDocumentChanged(object sender, EventArgs e)
	{
		if (ControlHostService == null)
		{
			return;
		}
		IBehaviorProviderAdapter activeDocument = DocumentRegistry.GetActiveDocument<IBehaviorProviderAdapter>();
		if (activeDocument != null)
		{
			_ = activeDocument.TimelineSet;
			_ = activeDocument.As<BaseInstanceEntityDocument>().InstanceSet;
			if (TimelinePlaybackService != null)
			{
				TimelinePlaybackService.Playback = AnimationKnobService;
				TimelinePlaybackService.StartIdleProcessExecution(5);
			}
		}
	}
}
