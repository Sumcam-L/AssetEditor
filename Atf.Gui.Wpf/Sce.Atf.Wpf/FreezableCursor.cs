using System.Windows;
using System.Windows.Input;

namespace Sce.Atf.Wpf;

public class FreezableCursor : Freezable
{
	public Cursor Cursor { get; set; }

	protected override Freezable CreateInstanceCore()
	{
		return new FreezableCursor();
	}
}
