using System.Windows;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors;

public class StyleBehaviorCollection : FreezableCollection<Behavior>
{
	protected override Freezable CreateInstanceCore()
	{
		return new StyleBehaviorCollection();
	}
}
