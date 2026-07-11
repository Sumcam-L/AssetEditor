using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Firaxis.MVVMBase.Views;

public class CloseableTab : TabItem
{
	public string Title
	{
		set
		{
			((CloseableHeader)base.Header).label_TabTitle.Content = value;
		}
	}

	public CloseableTab()
	{
		CloseableHeader closeableHeader = (CloseableHeader)(base.Header = new CloseableHeader());
		closeableHeader.button_close.MouseEnter += button_close_MouseEnter;
		closeableHeader.button_close.MouseLeave += button_close_MouseLeave;
		closeableHeader.button_close.Click += button_close_Click;
		closeableHeader.label_TabTitle.SizeChanged += label_TabTitle_SizeChanged;
	}

	protected override void OnSelected(RoutedEventArgs e)
	{
		base.OnSelected(e);
		((CloseableHeader)base.Header).button_close.Visibility = Visibility.Visible;
	}

	protected override void OnUnselected(RoutedEventArgs e)
	{
		base.OnUnselected(e);
		((CloseableHeader)base.Header).button_close.Visibility = Visibility.Hidden;
	}

	protected override void OnMouseEnter(MouseEventArgs e)
	{
		base.OnMouseEnter(e);
		((CloseableHeader)base.Header).button_close.Visibility = Visibility.Visible;
	}

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		if (!base.IsSelected)
		{
			((CloseableHeader)base.Header).button_close.Visibility = Visibility.Hidden;
		}
	}

	private void button_close_MouseEnter(object sender, MouseEventArgs e)
	{
		((CloseableHeader)base.Header).button_close.Foreground = Brushes.Red;
	}

	private void button_close_MouseLeave(object sender, MouseEventArgs e)
	{
		((CloseableHeader)base.Header).button_close.Foreground = Brushes.Black;
	}

	private void button_close_Click(object sender, RoutedEventArgs e)
	{
		((TabControl)base.Parent).Items.Remove(this);
	}

	private void label_TabTitle_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		((CloseableHeader)base.Header).button_close.Margin = new Thickness(((CloseableHeader)base.Header).label_TabTitle.ActualWidth + 5.0, 3.0, 4.0, 0.0);
	}
}
