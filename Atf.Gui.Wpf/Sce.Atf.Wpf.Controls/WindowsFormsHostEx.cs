using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Sce.Atf.Wpf.Controls;

public class WindowsFormsHostEx : WindowsFormsHost
{
	public static readonly DependencyProperty BoundChildProperty = DependencyProperty.Register("BoundChild", typeof(Control), typeof(WindowsFormsHostEx), new PropertyMetadata(BoundChildPropertyChanged));

	public Control BoundChild
	{
		get
		{
			return (Control)GetValue(BoundChildProperty);
		}
		set
		{
			SetValue(BoundChildProperty, value);
		}
	}

	private static void BoundChildPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		((WindowsFormsHostEx)sender).Child = e.NewValue as Control;
	}
}
