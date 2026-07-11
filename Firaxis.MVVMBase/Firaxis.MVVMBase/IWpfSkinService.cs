using System.Windows;

namespace Firaxis.MVVMBase;

public interface IWpfSkinService
{
	void ApplySkin(FrameworkElement target);

	void RemoveSkin(FrameworkElement target);
}
