using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.ATF;

public interface IPreviewContext
{
	IEnumerable<string> AvailablePreviewModuleNames { get; }

	string PreviewModule { get; set; }

	IPreviewWindow PreviewWindow { get; set; }

	IInstanceEntityAdapter EntityAdapter { get; }

	event EventHandler PreviewModuleChanged;
}
