using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.ATF;

public class NullWidgetEditor : IWidgetEditor, IDisposable
{
	public static string WidgetName = "None";

	public string Name => WidgetName;

	public IEnumerable<IWidgetState> States => Enumerable.Empty<IWidgetState>();

	public void SetTarget(IPreviewWindow window, IEntityDocument target)
	{
	}

	public void Activate(bool isReadOnly)
	{
	}

	public void Deactivate()
	{
	}

	public void Dispose()
	{
	}

	public void OnKeyDown(KeyEventArgs evt)
	{
	}

	public void OnKeyUp(KeyEventArgs evt)
	{
	}

	public void OnMouseDown(MouseEventArgs evt)
	{
	}

	public void OnMouseMove(MouseEventArgs evt)
	{
	}

	public void OnMouseUp(MouseEventArgs evt)
	{
	}
}
