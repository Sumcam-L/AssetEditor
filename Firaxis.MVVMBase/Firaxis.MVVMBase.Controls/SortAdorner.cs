using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Firaxis.MVVMBase.Controls;

public class SortAdorner : Adorner
{
	private static readonly Geometry _AscGeometry = Geometry.Parse("M 0,0 L 10,0 L 5,5 Z");

	private static readonly Geometry _DescGeometry = Geometry.Parse("M 0,5 L 10,5 L 5,0 Z");

	public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register("Direction", typeof(ListSortDirection), typeof(SortAdorner), new FrameworkPropertyMetadata(ListSortDirection.Ascending, FrameworkPropertyMetadataOptions.AffectsRender));

	public static readonly DependencyProperty IsPrimarySortProperty = DependencyProperty.Register("IsPrimarySort", typeof(bool), typeof(SortAdorner), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

	public ListSortDirection Direction
	{
		get
		{
			return (ListSortDirection)GetValue(DirectionProperty);
		}
		set
		{
			SetValue(DirectionProperty, value);
		}
	}

	public bool IsPrimarySort
	{
		get
		{
			return (bool)GetValue(IsPrimarySortProperty);
		}
		set
		{
			SetValue(IsPrimarySortProperty, value);
		}
	}

	public SortAdorner(UIElement element, ListSortDirection dir)
		: base(element)
	{
		Direction = dir;
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		base.OnRender(drawingContext);
		if (!(base.AdornedElement.RenderSize.Width < 20.0))
		{
			drawingContext.PushTransform(new TranslateTransform(base.AdornedElement.RenderSize.Width - 15.0, (base.AdornedElement.RenderSize.Height - 5.0) / 2.0));
			drawingContext.DrawGeometry(IsPrimarySort ? Brushes.Green : Brushes.Black, null, (Direction == ListSortDirection.Ascending) ? _AscGeometry : _DescGeometry);
			drawingContext.Pop();
		}
	}
}
