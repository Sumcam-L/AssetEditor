namespace Firaxis.AssetEditing;

public interface ITimelineBindingAdapter
{
	string SlotName { get; }

	TimelineBindingType BindingType { get; }

	void ClearBinding();
}
