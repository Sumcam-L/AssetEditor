using System;
using System.ComponentModel.Composition;
using System.Threading;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.AssetPreviewing;

[Export(typeof(ITimelinePlaybackService))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TimelinePlaybackService : ITimelinePlaybackService, IInitializable, IDisposable
{
	private bool m_isPlaying = false;

	private float m_currentTime = 0f;

	private float m_frameTime = 0f;

	private float m_timeScale = 1f;

	private float m_lastAdvance = 0f;

	private Timer m_idleTimer;

	private float m_selectedStartTime = 0f;

	private float m_selectedEndTime = 0f;

	private StateTransitionInfo m_selectedTransition = null;

	public ITimelinePlayback Playback { get; set; }

	public bool Playing
	{
		get
		{
			return m_isPlaying;
		}
		set
		{
			if (m_isPlaying == value)
			{
				return;
			}
			if (value)
			{
				float currentTime = CurrentTime;
				if ((m_selectedEndTime != -1f && currentTime >= m_selectedEndTime) || currentTime < m_selectedStartTime)
				{
					CurrentTime = m_selectedStartTime;
				}
			}
			m_isPlaying = value;
		}
	}

	public bool Looping { get; set; }

	public float CurrentTime
	{
		get
		{
			return m_currentTime;
		}
		set
		{
			float num = Math.Max(value, 0f);
			if (m_currentTime == num)
			{
				return;
			}
			FrameTime = num - CurrentTime;
			m_currentTime = value;
			if (Playback != null)
			{
				if (!Playback.IsTimeScrubbingEnabled)
				{
					Playback.PlayTransition(m_selectedTransition, Looping);
				}
				Playback.CurrentTime = m_currentTime;
			}
		}
	}

	public float FrameTime
	{
		get
		{
			return m_frameTime;
		}
		set
		{
			m_frameTime = value;
			if (Playback != null)
			{
				Playback.FrameTime = m_frameTime;
			}
		}
	}

	public float TimeScale
	{
		get
		{
			return m_timeScale;
		}
		set
		{
			m_timeScale = value;
		}
	}

	[ImportingConstructor]
	public TimelinePlaybackService()
	{
	}

	public void StartIdleProcessExecution(int periodMS)
	{
		DestroyIdleTimer();
		m_idleTimer = new Timer(AdvancePlaybackTime, null, 0, periodMS);
	}

	public void StopIdleProcessExecution()
	{
		DestroyIdleTimer();
	}

	public bool SelectAnimation(StateTransitionInfo animInfo, float currentTime, float startTime, float endTime)
	{
		m_selectedTransition = animInfo;
		m_selectedStartTime = startTime;
		m_selectedEndTime = endTime;
		if (Playback == null)
		{
			return false;
		}
		Playback.FrameTime = currentTime;
		Playback.CurrentTime = currentTime;
		return true;
	}

	public bool PlayAnimation(StateTransitionInfo animInfo, float currentTime, float startTime, float endTime)
	{
		if (!SelectAnimation(animInfo, currentTime, startTime, endTime))
		{
			return false;
		}
		if (Playback == null)
		{
			return false;
		}
		Playback.PlayTransition(animInfo, Looping);
		return true;
	}

	private void AdvancePlaybackTime(object state)
	{
		float secondsCount = Ticks.SecondsCount;
		if (Playing)
		{
			float num = secondsCount - m_lastAdvance;
			float num2 = num * TimeScale;
			float num3 = CurrentTime + num2;
			float selectedStartTime = m_selectedStartTime;
			float selectedEndTime = m_selectedEndTime;
			if (num3 < selectedEndTime)
			{
				CurrentTime = num3;
			}
			else if (Looping)
			{
				CurrentTime = selectedStartTime;
			}
			else
			{
				CurrentTime = selectedEndTime;
				Playing = false;
			}
		}
		m_lastAdvance = secondsCount;
	}

	public void Initialize()
	{
	}

	public void Dispose()
	{
		DestroyIdleTimer();
	}

	private void DestroyIdleTimer()
	{
		if (m_idleTimer == null)
		{
			return;
		}
		using ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
		m_idleTimer.Dispose(manualResetEvent);
		m_idleTimer = null;
		manualResetEvent.WaitOne(30000);
	}
}
