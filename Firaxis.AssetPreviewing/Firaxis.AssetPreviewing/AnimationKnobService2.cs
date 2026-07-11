using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Firaxis.Asset;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.AssetPreviewing;

[Export(typeof(IAnimationKnobService))]
[Export(typeof(AnimationKnobService2))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AnimationKnobService2 : IAnimationKnobService, ITimelinePlayback, IDisposable
{
	private string m_currentKnobSet = string.Empty;

	private float? m_startingTimeOffset;

	private IDocumentRegistry DocumentRegistry { get; set; }

	private IKnobManager KnobManager { get; set; }

	private AnimationKnobController KnobController { get; set; }

	public IEnumerable<string> FromAnimationNames => KnobController?.FromAnimationNames ?? Enumerable.Empty<string>();

	public IEnumerable<string> ToAnimationNames => KnobController?.ToAnimationNames ?? Enumerable.Empty<string>();

	public bool IsTimeScrubbingEnabled
	{
		get
		{
			return KnobController?.IsTimeScrubbingEnabled ?? false;
		}
		set
		{
			if (KnobController != null && KnobController.IsTimeScrubbingEnabled != value)
			{
				KnobController.IsTimeScrubbingEnabled = value;
			}
		}
	}

	public float FrameTime
	{
		get
		{
			return KnobController?.FrameTime ?? 0f;
		}
		set
		{
			if (KnobController != null)
			{
				KnobController.FrameTime = value;
			}
		}
	}

	public float CurrentTime
	{
		get
		{
			return KnobController?.CurrentTime ?? 0f;
		}
		set
		{
			if (KnobController != null)
			{
				KnobController.CurrentTime = value;
			}
		}
	}

	public float PlaybackTime => m_startingTimeOffset.HasValue ? (CurrentTime - m_startingTimeOffset.Value) : 0f;

	public IDictionary<string, IList<StateTransitionInfo>> TimelineStateTransitions => KnobController?.TimelineStateTransitions ?? new Dictionary<string, IList<StateTransitionInfo>>();

	public PlaybackState CurrentPlaybackState
	{
		get
		{
			if (IsPaused)
			{
				return PlaybackState.Paused;
			}
			if (IsPlaying)
			{
				return PlaybackState.Playing;
			}
			return PlaybackState.Stopped;
		}
	}

	public bool IsPlaying => KnobController?.IsPlaying ?? false;

	private bool IsPaused => IsPlaying && m_startingTimeOffset.HasValue && IsTimeScrubbingEnabled;

	public event EventHandler CurrentTimeChanged;

	public event EventHandler KnobControllerDestroyed;

	public event EventHandler KnobControllerCreated;

	public event EventHandler TimelineTransitionsChanged;

	[ImportingConstructor]
	public AnimationKnobService2(IKnobManager knobMgr, IDocumentRegistry docReg)
	{
		DocumentRegistry = docReg;
		KnobManager = knobMgr;
	}

	private void KnobManager_KnobGroupCleared(string groupName)
	{
		if (!(m_currentKnobSet != groupName))
		{
			ShutdownPreviousController();
		}
	}

	private void KnobManager_KnobGroupChanged(string groupName)
	{
		if (!string.IsNullOrEmpty(m_currentKnobSet) && !(groupName != m_currentKnobSet))
		{
			IKnobSet knobSet = KnobManager?.GetKnobSet(groupName);
			if (knobSet != null)
			{
				InitializeNewController(knobSet);
			}
		}
	}

	public void ClearIfActive(IInstanceEntity entity)
	{
		if (entity != null)
		{
			string knobSetName = GetKnobSetName(entity.Name);
			if (!(m_currentKnobSet != knobSetName))
			{
				SetActiveEntity(null);
			}
		}
	}

	public virtual void SetActiveEntity(IInstanceEntity entity)
	{
		ShutdownPreviousContext();
		InitializeNewContext(entity);
	}

	private void ActiveDocument_UriChanged(object sender, UriChangedEventArgs e)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(e.OldUri.LocalPath);
		string knobSetName = GetKnobSetName(fileNameWithoutExtension);
		if (m_currentKnobSet == knobSetName)
		{
			m_currentKnobSet = string.Empty;
			if (DocumentRegistry.ActiveDocument != null)
			{
				string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(DocumentRegistry.ActiveDocument.Uri.LocalPath);
				m_currentKnobSet = GetKnobSetName(fileNameWithoutExtension2);
			}
		}
	}

	private void DocumentRegistry_ActiveDocumentChanging(object sender, EventArgs e)
	{
		if (DocumentRegistry.ActiveDocument != null)
		{
			DocumentRegistry.ActiveDocument.UriChanged -= ActiveDocument_UriChanged;
		}
	}

	private void DocumentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
	{
		if (DocumentRegistry.ActiveDocument != null)
		{
			DocumentRegistry.ActiveDocument.UriChanged += ActiveDocument_UriChanged;
		}
	}

	public void Reset()
	{
		UnregisterForTimeEvents();
		KnobController?.Reset();
	}

	public void StartPlaying(string startState, string endState, bool looping)
	{
		if (KnobController != null)
		{
			KnobController.StartedPlaying += KnobController_StartedPlaying;
			KnobController.FromState = startState;
			KnobController.ToState = endState;
			KnobController.LoopOverride = looping;
			KnobController.Play();
		}
	}

	public void ResumePlaying()
	{
		if (KnobController != null)
		{
			BugSubmitter.SilentAssert(m_startingTimeOffset.HasValue, "We should only resume when paused. Being paused is defined by having a m_startingTimeOffset set!");
			KnobController.CurrentTime = m_startingTimeOffset.Value;
			KnobController.Play();
		}
	}

	private void KnobController_StartedPlaying(object sender, EventArgs e)
	{
		m_startingTimeOffset = CurrentTime;
		KnobController.StartedPlaying -= KnobController_StartedPlaying;
		KnobController.EndedPlaying -= KnobController_EndedPlaying;
		KnobController.EndedPlaying += KnobController_EndedPlaying;
	}

	private void KnobController_EndedPlaying(object sender, EventArgs e)
	{
		KnobController.EndedPlaying -= KnobController_EndedPlaying;
		m_startingTimeOffset = null;
	}

	public void StartScrubbing(StateTransitionInfo info)
	{
		m_startingTimeOffset = null;
		KnobController?.PlayTransition(info, looping: false);
	}

	public void PlayTransition(StateTransitionInfo info, bool looping)
	{
		KnobController?.PlayTransition(info, looping);
	}

	public void Dispose()
	{
		if (KnobController != null)
		{
			UnregisterForTimeEvents();
			KnobController.TimelineTransitionsChanged -= KnobController_TimelineTransitionsChanged;
			KnobController.StartedPlaying -= KnobController_StartedPlaying;
			KnobController.EndedPlaying -= KnobController_EndedPlaying;
			KnobController = null;
		}
		KnobManager = null;
	}

	private void RegisterForTimeEvents()
	{
		if (KnobController != null)
		{
			UnregisterForTimeEvents();
			KnobController.CurrentTimeChanged += KnobController_CurrentTimeChanged;
		}
	}

	private void KnobController_CurrentTimeChanged(object sender, EventArgs e)
	{
		this.CurrentTimeChanged.Raise(sender, e);
	}

	private void UnregisterForTimeEvents()
	{
		if (KnobController != null)
		{
			KnobController.CurrentTimeChanged -= KnobController_CurrentTimeChanged;
		}
	}

	private string GetKnobSetName(string assetName)
	{
		return $"{assetName}_{assetName}_{0}";
	}

	private void RegisterForDocumentEvents()
	{
		UnregisterForDocumentEvents();
		if (DocumentRegistry.ActiveDocument != null)
		{
			DocumentRegistry.ActiveDocument.UriChanged += ActiveDocument_UriChanged;
		}
		DocumentRegistry.ActiveDocumentChanging += DocumentRegistry_ActiveDocumentChanging;
		DocumentRegistry.ActiveDocumentChanged += DocumentRegistry_ActiveDocumentChanged;
	}

	private void UnregisterForDocumentEvents()
	{
		if (DocumentRegistry.ActiveDocument != null)
		{
			DocumentRegistry.ActiveDocument.UriChanged -= ActiveDocument_UriChanged;
		}
		DocumentRegistry.ActiveDocumentChanging -= DocumentRegistry_ActiveDocumentChanging;
		DocumentRegistry.ActiveDocumentChanged -= DocumentRegistry_ActiveDocumentChanged;
	}

	private void ShutdownPreviousController()
	{
		this.KnobControllerDestroyed.Raise(this, EventArgs.Empty);
		if (KnobController != null)
		{
			KnobController.TimelineTransitionsChanged -= KnobController_TimelineTransitionsChanged;
			KnobController = null;
		}
	}

	private void ShutdownPreviousContext()
	{
		ShutdownPreviousController();
		m_currentKnobSet = string.Empty;
	}

	private void InitializeNewController(IKnobSet knobSet)
	{
		KnobController = new AnimationKnobController(knobSet);
		KnobController.TimelineTransitionsChanged += KnobController_TimelineTransitionsChanged;
		KnobController.Enable();
		RegisterForTimeEvents();
		this.KnobControllerCreated.Raise(this, EventArgs.Empty);
	}

	private void KnobController_TimelineTransitionsChanged(object sender, EventArgs e)
	{
		this.TimelineTransitionsChanged.Raise(this, EventArgs.Empty);
	}

	private void InitializeNewContext(IInstanceEntity newPreviewEntity)
	{
		if (newPreviewEntity != null)
		{
			m_currentKnobSet = GetKnobSetName(newPreviewEntity.Name);
			IKnobSet knobSet = KnobManager.GetKnobSet(m_currentKnobSet);
			if (knobSet != null)
			{
				InitializeNewController(knobSet);
			}
		}
	}
}
