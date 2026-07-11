using System;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IWidget : IDisposable
{
	object BoundObject { get; set; }

	bool Visible { get; set; }

	string WidgetType { get; }

	event EventHandler OnStartEdit;

	event EventHandler OnEdit;

	event EventHandler OnFinishEdit;

	event EventHandler OnCancelEdit;

	void CancelPendingEdits();

	void Alter(IValueSet NewParametrs);
}
