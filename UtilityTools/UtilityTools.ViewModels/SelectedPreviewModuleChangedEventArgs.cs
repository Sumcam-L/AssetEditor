using System;

namespace UtilityTools.ViewModels;

public class SelectedPreviewModuleChangedEventArgs : EventArgs
{
	public string PreviewModuleName { get; private set; }

	public SelectedPreviewModuleChangedEventArgs(string previewModuleName)
	{
		PreviewModuleName = previewModuleName;
	}
}
