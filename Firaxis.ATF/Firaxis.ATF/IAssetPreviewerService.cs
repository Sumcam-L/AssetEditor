using System;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.ATF;

public interface IAssetPreviewerService : IPreviewSetService
{
	Control ActivePreviewerControl { get; }

	IAssetPreviewer AssetPreviewer { get; }

	IAudioPreviewer AudioPreviewer { get; }

	string PreviewerLayoutState { get; set; }

	event EventHandler KnobManagerDestroyed;

	event EventHandler KnobManagerCreated;

	event EventHandler<PreviewContextChangedEventArgs> PreviewContextChanged;

	void PreviewDocument(IPreviewableDocument document);

	bool RemovePreviewDocument(IPreviewableDocument document);

	bool IsActivePreviewModule(string previewModule);

	void SetActiveWidget(IWidgetEditor widget);

	void AddWidget(IWidgetEditor widget, string name, Image img, Action onClick);

	void RemoveWidget(IWidgetEditor widget);

	void HandleProjectChange();

	void ApplyGlobalKnobData(IPreviewContext context, KnobSetData data);

	KnobSetData GenerateGlobalKnobSetData(IPreviewContext previewContext);

	void SendChanges(IEntityChangeList changeList);
}
