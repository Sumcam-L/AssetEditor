namespace Sce.Atf.Controls.Adaptable;

public class DiagramBorder
{
	public enum BorderType
	{
		None,
		Left,
		Right,
		Top,
		Bottom,
		UpperLeftCorner,
		UpperRightCorner,
		LowerLeftCorner,
		LowerRightCorner
	}

	public readonly object Item;

	public BorderType Border { get; set; }

	public DiagramBorder(object item)
	{
		Item = item;
		Border = BorderType.None;
	}

	public DiagramBorder(object item, BorderType border)
	{
		Item = item;
		Border = border;
	}
}
