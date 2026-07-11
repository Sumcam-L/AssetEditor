using System.Windows;

namespace Sce.Atf.Wpf.Controls;

public class RangeSelectionChangedEventArgs : RoutedEventArgs
{
	public double NewRangeStart { get; set; }

	public double NewRangeStop { get; set; }

	internal RangeSelectionChangedEventArgs(double newRangeStart, double newRangeStop)
	{
		NewRangeStart = newRangeStart;
		NewRangeStop = newRangeStop;
	}

	internal RangeSelectionChangedEventArgs(RangeSlider slider)
		: this(slider.RangeStart, slider.RangeStop)
	{
	}
}
