using System.Windows;

namespace Firaxis.ATF;

public interface IWpfSkinStyleSetter
{
	DependencyProperty GetTargetProperty();

	void ApplyStyle(DependencyObject target);
}
