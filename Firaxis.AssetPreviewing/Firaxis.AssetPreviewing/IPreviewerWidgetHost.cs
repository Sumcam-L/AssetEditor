using System;
using System.Drawing;
using Firaxis.ATF;

namespace Firaxis.AssetPreviewing;

public interface IPreviewerWidgetHost
{
	void SetActiveWidget(IWidgetEditor widget);

	void AddWidget(IWidgetEditor widget, string name, Image img, Action onClick);

	void RemoveWidget(IWidgetEditor widget);
}
