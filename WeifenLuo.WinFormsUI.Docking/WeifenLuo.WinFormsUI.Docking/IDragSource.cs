using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

public interface IDragSource
{
	Control DragControl { get; }
}
