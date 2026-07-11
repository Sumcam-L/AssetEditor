using System;

namespace Sce.Atf.Wpf.Controls.Adaptable;

public interface IDragSelector
{
	event EventHandler<DragSelectionEventArgs> Selected;
}
