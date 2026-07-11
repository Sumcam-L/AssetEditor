using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.ATF;

public interface IPreviewerWidgetService
{
	IWidgetEditor ActiveWidgetEditor { get; }

	IEnumerable<string> AvailableWidgetEditors { get; }

	event EventHandler ActiveWidgetChanged;

	void ClearIfActive(IPreviewWindow wnd, IEntityDocument doc);

	void SetActiveEntity(IPreviewWindow wnd, IEntityDocument doc);

	void SetActiveWidgetEditor(string editorName);

	void StartProjectChange();

	void FinishProjectChange();

	IWidgetEditor FindWidgetEditor(string name);
}
