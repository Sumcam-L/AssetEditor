using System;

namespace Sce.Atf.Wpf.Applications;

public interface ILabelEditingContext
{
	event EventHandler<BeginLabelEditEventArgs> BeginLabelEdit;

	bool CanEditLabel(object item);

	void EditLabel(object item);

	string GetLabel(object item);

	void SetLabel(object item, string value);
}
