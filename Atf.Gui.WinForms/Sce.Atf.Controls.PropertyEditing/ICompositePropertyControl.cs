using System;

namespace Sce.Atf.Controls.PropertyEditing;

public interface ICompositePropertyControl
{
	event EventHandler<CompositeOpenedEventArgs> CompositeOpened;
}
