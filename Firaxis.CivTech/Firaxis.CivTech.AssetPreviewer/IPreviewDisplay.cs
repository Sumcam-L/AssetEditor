using System;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IPreviewDisplay : IDisposable
{
	void MakeActiveDisplay();

	void BindWindow(IPreviewWindow pWindow);

	void UnbindWindow();

	void CaptureScreenshot(string imgPath);

	void OnResized(int nWidth, int nHeight);
}
