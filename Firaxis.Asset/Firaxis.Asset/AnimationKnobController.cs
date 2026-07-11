using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;

namespace Firaxis.Asset;

public class AnimationKnobController : ITimelinePlayback
{
	private IDictionary<string, IList<StateTransitionInfo>> m_TimelineToStateTransitions;

	private IKnobSet m_knobSet;

	private bool m_lastIsAnimationPlaying = false;

	public IEnumerable<string> FromAnimationNames
	{
		get
		{
			IContainerKnob<string> fromStates = m_FromStates;
			if (fromStates != null)
			{
				return fromStates.Values;
			}
			return Enumerable.Empty<string>();
		}
	}

	public IEnumerable<string> ToAnimationNames
	{
		get
		{
			IContainerKnob<string> toStates = m_ToStates;
			if (toStates != null)
			{
				return toStates.Values;
			}
			return Enumerable.Empty<string>();
		}
	}

	private IValueKnob<bool> m_PreserveWeights => m_knobSet.FindKnobByName("m_bPreserveWeights") as IValueKnob<bool>;

	private IValueKnob<float> m_BaseWeight => m_knobSet.FindKnobByName("m_fBaseWeight") as IValueKnob<float>;

	private IValueKnob<float> m_AdditiveWeight => m_knobSet.FindKnobByName("m_fAdditiveWeight") as IValueKnob<float>;

	private IContainerKnob<string> m_ToStates => m_knobSet.FindKnobByName("m_sToState") as IContainerKnob<string>;

	private IContainerKnob<string> m_FromStates => m_knobSet.FindKnobByName("m_sFromState") as IContainerKnob<string>;

	private IValueKnob<bool> m_EnableOverrides => m_knobSet.FindKnobByName("m_bEnableOverrides") as IValueKnob<bool>;

	private IValueKnob<bool> m_LoopOverride => m_knobSet.FindKnobByName("m_bLoopOverride") as IValueKnob<bool>;

	private IValueKnob<bool> m_RandomOffsetOverride => m_knobSet.FindKnobByName("m_bRandomOffsetOverride") as IValueKnob<bool>;

	private IValueKnob<bool> m_ContinueOffsetOverride => m_knobSet.FindKnobByName("m_bContinueOffsetOverride") as IValueKnob<bool>;

	private IValueKnob<float> m_BlendDurationOverride => m_knobSet.FindKnobByName("m_fBlendDurationOverride") as IValueKnob<float>;

	private IValueKnob<int> m_AnimationGraphIndexOverride => m_knobSet.FindKnobByName("m_nAnimationGraphIndexOverride") as IValueKnob<int>;

	private IValueKnob<bool> m_IsAnimationPlaying => m_knobSet.FindKnobByName("m_bIsAnimationPlaying") as IValueKnob<bool>;

	private IValueKnob<bool> m_EnableTimeScrubbing => m_knobSet.FindKnobByName("m_bEnableTimeScrubbing") as IValueKnob<bool>;

	private IValueKnob<float> m_CurrentTime => m_knobSet.FindKnobByName("m_fCurrentTime") as IValueKnob<float>;

	private IValueKnob<float> m_FrameTime => m_knobSet.FindKnobByName("m_fFrameTime") as IValueKnob<float>;

	private IContainerKnob<string> m_TimelineBindings => m_knobSet.FindKnobByName("m_sTimelineBinding") as IContainerKnob<string>;

	private IFunctionKnob m_Play => m_knobSet.FindKnobByName("m_bPlayTransition") as IFunctionKnob;

	private IFunctionKnob m_Stop => m_knobSet.FindKnobByName("m_bStopTransition") as IFunctionKnob;

	private IFunctionKnob m_PlayInternal => m_knobSet.FindKnobByName("m_bInternalPlayTransition") as IFunctionKnob;

	private IFunctionKnob m_Reset => m_knobSet.FindKnobByName("m_bInternalResetTransition") as IFunctionKnob;

	public bool PreserveWeights
	{
		set
		{
			RegisterForTimeChanges();
			m_PreserveWeights?.SetUIValue(value);
		}
	}

	public float BaseWeight
	{
		set
		{
			RegisterForTimeChanges();
			m_BaseWeight?.SetUIValue(value);
		}
	}

	public float AdditiveWeight
	{
		set
		{
			RegisterForTimeChanges();
			m_AdditiveWeight?.SetUIValue(value);
		}
	}

	public string ToState
	{
		set
		{
			RegisterForTimeChanges();
			m_ToStates?.SetUIValue(value);
		}
	}

	public string FromState
	{
		set
		{
			RegisterForTimeChanges();
			m_FromStates?.SetUIValue(value);
		}
	}

	private bool EnableOverrides
	{
		set
		{
			RegisterForTimeChanges();
			m_EnableOverrides?.SetUIValue(value);
		}
	}

	public bool LoopOverride
	{
		set
		{
			RegisterForTimeChanges();
			m_LoopOverride?.SetUIValue(value);
		}
	}

	private bool RandomOffsetOverride
	{
		set
		{
			RegisterForTimeChanges();
			m_RandomOffsetOverride?.SetUIValue(value);
		}
	}

	private bool ContinueOffsetOverride
	{
		set
		{
			RegisterForTimeChanges();
			m_ContinueOffsetOverride?.SetUIValue(value);
		}
	}

	private float BlendDurationOverride
	{
		set
		{
			RegisterForTimeChanges();
			m_BlendDurationOverride?.SetUIValue(value);
		}
	}

	private int AnimationGraphIndexOverride
	{
		set
		{
			RegisterForTimeChanges();
			m_AnimationGraphIndexOverride?.SetUIValue(value);
		}
	}

	public float CurrentTime
	{
		get
		{
			RegisterForTimeChanges();
			return m_CurrentTime?.Value ?? 0f;
		}
		set
		{
			RegisterForTimeChanges();
			m_CurrentTime?.SetUIValue(value);
		}
	}

	public float CurrentTimeValueOnly
	{
		set
		{
			IValueKnob<float> currentTime = m_CurrentTime;
			if (currentTime != null)
			{
				UnregisterForTimeChanges();
				currentTime.Value = value;
				RegisterForTimeChanges();
			}
		}
	}

	public float FrameTime
	{
		get
		{
			return (m_FrameTime != null) ? m_FrameTime.Value : 0f;
		}
		set
		{
			RegisterForTimeChanges();
			m_FrameTime?.SetUIValue(value);
		}
	}

	public bool IsPlaying
	{
		get
		{
			RegisterForTimeChanges();
			return IsAnimationPlayingImpl;
		}
	}

	private bool IsAnimationPlayingImpl => m_IsAnimationPlaying?.Value ?? false;

	public bool IsTimeScrubbingEnabled
	{
		get
		{
			RegisterForTimeChanges();
			return m_EnableTimeScrubbing?.Value ?? false;
		}
		set
		{
			if (m_EnableTimeScrubbing != null)
			{
				RegisterForTimeChanges();
				if (m_EnableTimeScrubbing.Value != value)
				{
					m_EnableTimeScrubbing.Value = value;
				}
			}
		}
	}

	public IDictionary<string, IList<StateTransitionInfo>> TimelineStateTransitions => m_TimelineToStateTransitions;

	public event EventHandler CurrentTimeChanged;

	public event EventHandler StartedPlaying;

	public event EventHandler EndedPlaying;

	public event EventHandler ScrubbingChanged;

	public event EventHandler TimelineTransitionsChanged;

	public AnimationKnobController(IKnobSet knobSet)
	{
		m_knobSet = knobSet;
		InitializeTimelineToStateTransitions();
		RegisterForTimeChanges();
	}

	private void UnregisterForTimeChanges()
	{
		if (m_CurrentTime != null)
		{
			m_CurrentTime.HasUpdateEvent -= this.CurrentTimeChanged;
		}
	}

	private void RegisterForTimeChanges()
	{
		if (m_CurrentTime != null)
		{
			UnregisterForTimeChanges();
			m_CurrentTime.HasUpdateEvent += this.CurrentTimeChanged;
			m_lastIsAnimationPlaying = IsAnimationPlayingImpl;
			m_IsAnimationPlaying.HasUpdateEvent -= IsAnimationPlaying_HasUpdateEvent;
			m_IsAnimationPlaying.HasUpdateEvent += IsAnimationPlaying_HasUpdateEvent;
		}
	}

	private void TimelineBindings_HasUpdateEvent(object sender, EventArgs e)
	{
		InitializeTimelineToStateTransitions();
	}

	private void EnableTimeScrubbing_HasUpdateEvent(object sender, EventArgs e)
	{
		this.ScrubbingChanged.Raise(this, EventArgs.Empty);
	}

	private void IsAnimationPlaying_HasUpdateEvent(object sender, EventArgs e)
	{
		bool isAnimationPlayingImpl = IsAnimationPlayingImpl;
		if (m_lastIsAnimationPlaying != isAnimationPlayingImpl)
		{
			if (isAnimationPlayingImpl)
			{
				this.StartedPlaying.Raise(this, EventArgs.Empty);
			}
			else
			{
				this.EndedPlaying.Raise(this, EventArgs.Empty);
			}
			m_lastIsAnimationPlaying = isAnimationPlayingImpl;
		}
	}

	public void Play()
	{
		RegisterForTimeChanges();
		m_Play?.CallFunction();
	}

	public void Stop()
	{
		RegisterForTimeChanges();
		m_Stop?.CallFunction();
	}

	public void PlayInternal()
	{
		RegisterForTimeChanges();
		m_PlayInternal?.CallFunction();
	}

	public void Reset()
	{
		RegisterForTimeChanges();
		m_Reset?.CallFunction();
	}

	public void PlayTransition(StateTransitionInfo info, bool looping)
	{
		if (info != null)
		{
			PreserveWeights = true;
			BaseWeight = 0.5f;
			AdditiveWeight = 0.5f;
			EnableOverrides = true;
			RandomOffsetOverride = false;
			ContinueOffsetOverride = false;
			BlendDurationOverride = 0f;
			LoopOverride = looping;
			AnimationGraphIndexOverride = info.AnimationGraphIndex;
			FromState = info.Source;
			ToState = info.Destination;
			PlayInternal();
		}
	}

	public void Enable()
	{
		Stop();
		if (m_EnableTimeScrubbing != null)
		{
			m_EnableTimeScrubbing.HasUpdateEvent += EnableTimeScrubbing_HasUpdateEvent;
		}
	}

	public void Disable()
	{
		if (m_EnableTimeScrubbing != null)
		{
			m_EnableTimeScrubbing.HasUpdateEvent -= EnableTimeScrubbing_HasUpdateEvent;
		}
		Stop();
	}

	private void InitializeTimelineToStateTransitions()
	{
		m_TimelineToStateTransitions = new Dictionary<string, IList<StateTransitionInfo>>();
		if (m_TimelineBindings != null)
		{
			m_TimelineBindings.HasUpdateEvent -= TimelineBindings_HasUpdateEvent;
			m_TimelineBindings.HasUpdateEvent += TimelineBindings_HasUpdateEvent;
			foreach (string value in m_TimelineBindings.Values)
			{
				float result = 0f;
				int result2 = 0;
				string[] array = value.Split(new string[6] { ":", "->", "(", ")", "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length != 5)
				{
					BugSubmitter.SilentReport($"Result contained {array.Length} of 5 expected elements when splitting \"{value}\" into transition info @summary Failed to parse timeline binding transition info @assign bwhitman");
					continue;
				}
				string text = array[1].Trim();
				if (!int.TryParse(text, out result2))
				{
					BugSubmitter.SilentReport($"Failed to parse AnimationGraphIndex from {text} that was split from \"{value}\" for transition info @summary Failed to parse timeline binding transition info @assign bwhitman");
					continue;
				}
				string text2 = array[4].Trim();
				if (!text2.Equals("nan", StringComparison.CurrentCultureIgnoreCase))
				{
					if (!float.TryParse(text2, out result))
					{
						BugSubmitter.SilentReport($"Failed to parse Duration from {text2} that was split from \"{value}\" for transition info @summary Failed to parse timeline binding transition info @assign bwhitman");
						continue;
					}
				}
				else
				{
					result = 1f;
					BugSubmitter.SilentReport($"Failed to parse NaN Duration that was split from \"{value}\" for transition info @summary Fixing NaN found in timeline binding transition info @assign bwhitman");
				}
				string key = array[0].Trim();
				StateTransitionInfo stateTransitionInfo = new StateTransitionInfo();
				stateTransitionInfo.Source = array[2].Trim();
				stateTransitionInfo.Destination = array[3].Trim();
				stateTransitionInfo.AnimationGraphIndex = result2;
				stateTransitionInfo.Duration = result;
				stateTransitionInfo.IsReadOnly = value.Contains('[');
				if (!m_TimelineToStateTransitions.ContainsKey(key))
				{
					m_TimelineToStateTransitions.Add(key, new List<StateTransitionInfo> { stateTransitionInfo });
				}
				else
				{
					m_TimelineToStateTransitions[key].Add(stateTransitionInfo);
				}
			}
			this.TimelineTransitionsChanged.Raise(this, EventArgs.Empty);
		}
		RegisterForTimeChanges();
	}
}
