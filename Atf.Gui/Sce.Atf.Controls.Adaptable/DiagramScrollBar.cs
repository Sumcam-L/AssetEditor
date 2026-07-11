namespace Sce.Atf.Controls.Adaptable;

public class DiagramScrollBar
{
	public readonly object Item;

	public Orientation BarOrientation { get; set; }

	public DiagramScrollBar(object item, Orientation orientation)
	{
		Item = item;
		BarOrientation = orientation;
	}
}
