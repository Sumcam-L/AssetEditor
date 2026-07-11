namespace Firaxis.CivTech.AssetPreviewer;

public class StateTransitionInfo
{
	public string Source { get; set; }

	public string Destination { get; set; }

	public int AnimationGraphIndex { get; set; }

	public float Duration { get; set; }

	public bool IsReadOnly { get; set; }

	public StateTransitionInfo()
	{
		Source = string.Empty;
		Destination = string.Empty;
		AnimationGraphIndex = 0;
		Duration = 1f;
		IsReadOnly = false;
	}
}
