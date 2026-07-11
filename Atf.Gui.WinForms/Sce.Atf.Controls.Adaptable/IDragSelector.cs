using System;

namespace Sce.Atf.Controls.Adaptable;

public interface IDragSelector
{
	event EventHandler<DragSelectionEventArgs> Selected;
}
