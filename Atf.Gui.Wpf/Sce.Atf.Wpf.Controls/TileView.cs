using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls;

public class TileView : ViewBase
{
	public static DependencyProperty ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(TileView), new PropertyMetadata(1.0, ScalePropertyChanged));

	public static DependencyProperty ItemHeightProperty = DependencyProperty.Register("ItemHeight", typeof(double), typeof(TileView), new PropertyMetadata(1.0, ScalePropertyChanged));

	private DataTemplate itemTemplate;

	private Brush selectedBackground = Brushes.Transparent;

	private Brush selectedBorderBrush = Brushes.Black;

	public double ItemWidth
	{
		get
		{
			return (double)GetValue(ItemWidthProperty);
		}
		set
		{
			SetValue(ItemWidthProperty, value);
		}
	}

	public double ItemHeight
	{
		get
		{
			return (double)GetValue(ItemHeightProperty);
		}
		set
		{
			SetValue(ItemHeightProperty, value);
		}
	}

	public DataTemplate ItemTemplate
	{
		get
		{
			return itemTemplate;
		}
		set
		{
			itemTemplate = value;
		}
	}

	public Brush SelectedBackground
	{
		get
		{
			return selectedBackground;
		}
		set
		{
			selectedBackground = value;
		}
	}

	public Brush SelectedBorderBrush
	{
		get
		{
			return selectedBorderBrush;
		}
		set
		{
			selectedBorderBrush = value;
		}
	}

	protected override object DefaultStyleKey => new ComponentResourceKey(GetType(), "TileView");

	protected override object ItemContainerDefaultStyleKey => new ComponentResourceKey(GetType(), "TileViewItem");

	public static void ScalePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
	{
	}
}
