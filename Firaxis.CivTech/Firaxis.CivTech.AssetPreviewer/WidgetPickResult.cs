namespace Firaxis.CivTech.AssetPreviewer;

public class WidgetPickResult : PickResult, IWidgetPickComponent
{
	public IWidget Widget { get; private set; }

	public WidgetPickResult(float dist, IWidget wid)
		: base(dist)
	{
		Widget = wid;
	}
}
