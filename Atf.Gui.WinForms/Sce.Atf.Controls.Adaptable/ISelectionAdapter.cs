using System;

namespace Sce.Atf.Controls.Adaptable;

public interface ISelectionAdapter
{
	event EventHandler<DiagramHitEventArgs> SelectedItemHit;
}
