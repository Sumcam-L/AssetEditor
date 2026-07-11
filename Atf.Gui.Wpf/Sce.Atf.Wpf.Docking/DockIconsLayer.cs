using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Docking;

internal class DockIconsLayer : Window
{
	private Canvas m_canvas;

	public DockIconsLayer(FrameworkElement element)
	{
		base.ShowInTaskbar = false;
		base.ShowActivated = false;
		base.WindowStyle = WindowStyle.None;
		base.AllowsTransparency = true;
		base.Background = Brushes.Transparent;
		base.Topmost = true;
		m_canvas = new Canvas();
		base.Content = m_canvas;
		base.WindowStartupLocation = WindowStartupLocation.Manual;
		Point point = element.PointToScreen(new Point(0.0, 0.0));
		Matrix transformToDevice = PresentationSource.FromVisual(Window.GetWindow(element)).CompositionTarget.TransformToDevice;
		transformToDevice.Invert();
		point = transformToDevice.Transform(point);
		base.Left = point.X;
		base.Top = point.Y;
		base.Width = element.ActualWidth;
		base.Height = element.ActualHeight;
	}

	public void AddChild(FrameworkElement icon)
	{
		m_canvas.Children.Add(icon);
	}

	public void InsertChild(int index, FrameworkElement icon)
	{
		m_canvas.Children.Insert(index, icon);
	}

	public void ClearChildren()
	{
		m_canvas.Children.Clear();
	}

	public void RemoveChild(FrameworkElement icon)
	{
		m_canvas.Children.Remove(icon);
	}

	internal void CloseIfEmpty()
	{
		if (m_canvas.Children.Count == 0)
		{
			Close();
		}
	}
}
