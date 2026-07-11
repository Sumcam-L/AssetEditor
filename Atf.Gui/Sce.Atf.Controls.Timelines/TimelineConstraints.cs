namespace Sce.Atf.Controls.Timelines;

public abstract class TimelineConstraints
{
	public abstract bool IsStartValid(IEvent _event, ref float start);

	public abstract bool IsLengthValid(IInterval interval, ref float length);

	public abstract bool IsIntervalValid(IInterval interval, ref float start, ref float length, IInterval other);
}
