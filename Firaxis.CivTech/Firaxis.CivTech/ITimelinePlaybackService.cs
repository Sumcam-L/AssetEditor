using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.CivTech;

public interface ITimelinePlaybackService
{
	ITimelinePlayback Playback { get; set; }

	bool Playing { get; set; }

	bool Looping { get; set; }

	float CurrentTime { get; set; }

	float FrameTime { get; set; }

	float TimeScale { get; set; }

	void StartIdleProcessExecution(int periodMS);

	void StopIdleProcessExecution();

	bool SelectAnimation(StateTransitionInfo animInfo, float currentTime, float startTime, float endTime);

	bool PlayAnimation(StateTransitionInfo animInfo, float currentTime, float startTime, float endTime);
}
