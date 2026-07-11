using System.Windows;

namespace Sce.Atf.Wpf.Controls;

public class TemplateSize
{
	public DataTemplate DataTemplate { get; set; }

	public Size? MinimumSize { get; set; }

	public Size? MaximumSize { get; set; }

	public bool IsRightSize(double width, double height)
	{
		return (!MinimumSize.HasValue || (MinimumSize.Value.Width <= width && MinimumSize.Value.Height <= height)) && (!MaximumSize.HasValue || (MaximumSize.Value.Width > width && MaximumSize.Value.Height > height));
	}
}
