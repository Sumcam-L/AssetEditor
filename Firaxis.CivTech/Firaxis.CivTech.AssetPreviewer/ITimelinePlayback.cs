using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetPreviewer;

public interface ITimelinePlayback
{
	bool IsPlaying { get; }

	bool IsTimeScrubbingEnabled { get; set; }

	float FrameTime { get; set; }

	float CurrentTime { get; set; }

	IDictionary<string, IList<StateTransitionInfo>> TimelineStateTransitions { get; }

	event EventHandler CurrentTimeChanged;

	event EventHandler TimelineTransitionsChanged;

	void Reset();

	void PlayTransition(StateTransitionInfo info, bool looping);
}
