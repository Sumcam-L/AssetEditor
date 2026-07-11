using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.ATF;

public interface IWidgetEditor : IDisposable
{
	string Name { get; }

	IEnumerable<IWidgetState> States { get; }

	void Activate(bool isReadOnly);

	void Deactivate();

	void SetTarget(IPreviewWindow window, IEntityDocument target);

	void OnKeyDown(KeyEventArgs evt);

	void OnKeyUp(KeyEventArgs evt);

	void OnMouseMove(MouseEventArgs evt);

	void OnMouseDown(MouseEventArgs evt);

	void OnMouseUp(MouseEventArgs evt);
}
