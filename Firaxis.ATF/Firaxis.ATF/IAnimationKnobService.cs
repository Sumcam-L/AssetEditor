using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.ATF;

public interface IAnimationKnobService : ITimelinePlayback
{
	float PlaybackTime { get; }

	PlaybackState CurrentPlaybackState { get; }

	IEnumerable<string> FromAnimationNames { get; }

	IEnumerable<string> ToAnimationNames { get; }

	event EventHandler KnobControllerDestroyed;

	event EventHandler KnobControllerCreated;

	void ClearIfActive(IInstanceEntity entity);

	void SetActiveEntity(IInstanceEntity entity);

	void StartPlaying(string startState, string endState, bool looping);

	void ResumePlaying();

	void StartScrubbing(StateTransitionInfo info);
}
