namespace Firaxis.Asset;

public class Timeline
{
	public string TimelineID;

	public string AnimationID;

	public int TriggerStart;

	public int TriggerCount;

	public float Duration;

	public Timeline(string iTimelineID, string iAnimationID, int iTriggerStart, int nTriggerCount, float fDuration)
	{
		TimelineID = iTimelineID;
		AnimationID = iAnimationID;
		TriggerStart = iTriggerStart;
		TriggerCount = nTriggerCount;
		Duration = fDuration;
	}
}
