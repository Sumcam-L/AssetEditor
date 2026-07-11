namespace Firaxis.ATF;

public enum QuietTimeWaitBehavior
{
	FixedDuration,
	ExponentialBackoff,
	SigmoidBackoff,
	Adaptive
}
