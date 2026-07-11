using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetPreviewing;

[Export(typeof(IAnimationRecorderService))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AnimationRecorderService : IAnimationRecorderService, IInitializable, IDisposable
{
	private string _animationVideoRoot;

	public string BoundAssetName { get; set; }

	public string AnimationVideoRoot
	{
		get
		{
			return _animationVideoRoot;
		}
		set
		{
			if (!(_animationVideoRoot == value) && CreateDirectoryOrFail(value))
			{
				_animationVideoRoot = value;
			}
		}
	}

	public IEnumerable<string> AvailableCodecs => PreviewerCaptureService.AvailableCodecs;

	public string SelectedCodec
	{
		get
		{
			return PreviewerCaptureService.PreferredCodec;
		}
		set
		{
			PreviewerCaptureService.PreferredCodec = value;
		}
	}

	public int CompressionLevel
	{
		get
		{
			return PreviewerCaptureService.CompressionLevel;
		}
		set
		{
			PreviewerCaptureService.CompressionLevel = value;
		}
	}

	public IEnumerable<string> FromAnimationStates => AnimationKnobService.FromAnimationNames;

	public IEnumerable<string> ToAnimationStates => AnimationKnobService.ToAnimationNames;

	private IPreviewerControlHost PreviewerControlHost { get; set; }

	private IPreviewerCaptureService PreviewerCaptureService { get; set; }

	private IAnimationKnobService AnimationKnobService { get; set; }

	private ISettingsService SettingsService { get; set; }

	public event EventHandler BoundAnimationsChanged;

	protected virtual void OnBoundAnimationsChanged()
	{
		this.BoundAnimationsChanged?.Invoke(this, EventArgs.Empty);
	}

	[ImportingConstructor]
	public AnimationRecorderService(IPreviewerControlHost previewControlHost, IPreviewerCaptureService previewerCaptureService, IAnimationKnobService animKnobSvc, ISettingsService settingsService)
	{
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		AnimationVideoRoot = Path.Combine(folderPath, "My Games", "AssetCloud", "Animation Videos");
		PreviewerControlHost = previewControlHost;
		AnimationKnobService = animKnobSvc;
		PreviewerCaptureService = previewerCaptureService;
		SettingsService = settingsService;
		AnimationKnobService.KnobControllerCreated += AnimationKnobService_KnobControllerCreated;
		AnimationKnobService.KnobControllerDestroyed += AnimationKnobService_KnobControllerDestroyed;
	}

	public void SetActiveEntity(InstanceType insType, string entityName)
	{
		BoundAssetName = entityName;
	}

	private void AnimationKnobService_KnobControllerDestroyed(object sender, EventArgs e)
	{
		OnBoundAnimationsChanged();
	}

	private void AnimationKnobService_KnobControllerCreated(object sender, EventArgs e)
	{
		OnBoundAnimationsChanged();
	}

	public void Initialize()
	{
		BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => AnimationVideoRoot, "Saved Animation Root".Localize(), "Previewer Animation Capturing".Localize(), "The root directory to save recorded animations to.".Localize());
		SettingsService.RegisterSettings("Previewer".Localize(), boundPropertyDescriptor);
		SettingsService.RegisterUserSettings("Previewer".Localize(), boundPropertyDescriptor);
	}

	public void Record(string fromAnimationState, string toAnimationState)
	{
		if (!AnimationKnobService.FromAnimationNames.Contains(fromAnimationState))
		{
			string message = $"Tried to record a transition that is invalid.  From Animation State '{fromAnimationState}' does not exist on the current asset.  @assign bwhitman @summary Record Animation From Error";
			BugSubmitter.SilentReport(message);
			return;
		}
		if (!AnimationKnobService.ToAnimationNames.Contains(toAnimationState))
		{
			string message2 = $"Tried to record a transition that is invalid.  To Animation State '{toAnimationState}' does not exist on the current asset.  @assign bwhitman @summary Record Animation To Error";
			BugSubmitter.SilentReport(message2);
			return;
		}
		IDictionary<string, IList<StateTransitionInfo>> timelineStateTransitions = AnimationKnobService.TimelineStateTransitions;
		IEnumerable<StateTransitionInfo> source = timelineStateTransitions.Values.SelectMany((IList<StateTransitionInfo> val) => val);
		StateTransitionInfo stateTransitionInfo = source.FirstOrDefault((StateTransitionInfo info) => info.Source == fromAnimationState && info.Destination == toAnimationState);
		if (stateTransitionInfo == null)
		{
			return;
		}
		if (stateTransitionInfo.Duration == 0f)
		{
			Outputs.WriteLine(OutputMessageType.Info, "Ignoring animation from {0} to {1} because it has no duration", fromAnimationState, toAnimationState);
			return;
		}
		string text = Path.Combine(AnimationVideoRoot, BoundAssetName, $"from-{fromAnimationState}-to-{toAnimationState}.avi");
		string directoryName = Path.GetDirectoryName(text);
		if (CreateDirectoryOrFail(directoryName))
		{
			float duration = stateTransitionInfo.Duration;
			float num = PreviewerCaptureService.TargetFPS;
			float num2 = 1f / 30f;
			if (num > 0f)
			{
				num2 = 1f / num;
			}
			AnimationKnobService.Reset();
			AnimationKnobService.IsTimeScrubbingEnabled = true;
			AnimationKnobService.PlayTransition(stateTransitionInfo, looping: false);
			Thread.Sleep(PreviewerCaptureService?.InterCaptureDelay ?? 0);
			Control previewerControl = PreviewerControlHost.PreviewerControl;
			PreviewerCaptureService.Start(text);
			float num3 = 0f;
			bool flag = true;
			do
			{
				AnimationKnobService.CurrentTime = num3;
				AnimationKnobService.FrameTime = num2;
				Thread.Sleep((int)((double)(1000f * num2) * 1.5));
				flag = PreviewerCaptureService.CaptureScreen(previewerControl);
				num3 += num2;
			}
			while (num3 < duration && flag);
			AnimationKnobService.Reset();
			PreviewerCaptureService.Stop(text);
		}
	}

	private bool CreateDirectoryOrFail(string directory)
	{
		if (Directory.Exists(directory))
		{
			return true;
		}
		try
		{
			Directory.CreateDirectory(directory);
		}
		catch (System.Exception)
		{
			string message = $"Unable to create the directory {directory}.  The Animation Video Root has not been changed.";
			MessageBoxes.Show(message);
			return false;
		}
		return true;
	}

	public void Dispose()
	{
		if (AnimationKnobService != null)
		{
			AnimationKnobService.KnobControllerCreated -= AnimationKnobService_KnobControllerCreated;
			AnimationKnobService.KnobControllerDestroyed -= AnimationKnobService_KnobControllerDestroyed;
		}
	}
}
