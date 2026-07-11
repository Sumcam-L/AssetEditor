using System;

namespace Firaxis.ATF;

public class KeyFrameTimeChangeArgs : EventArgs
{
	public int SelectedIndex { get; internal set; }

	public float Time { get; internal set; }

	public KeyFrameTimeChangeArgs(int idx, float time)
	{
		SelectedIndex = idx;
		Time = time;
	}
}
