namespace Firaxis.AssetEditing;

public interface ITimelineBoundItem : IDomNodeTreeItem
{
	TimelineAdapter Timeline { get; }
}
