using System.Windows;

namespace Sce.Atf.Wpf.Controls.Adaptable;

public interface IDragDropAdapter
{
	Point? MousePosition { get; set; }
}
