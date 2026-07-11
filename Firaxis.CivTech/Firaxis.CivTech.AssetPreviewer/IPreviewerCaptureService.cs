using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IPreviewerCaptureService
{
	int InterCaptureDelay { get; set; }

	int TargetFPS { get; }

	bool CaptureData { get; }

	Size CaptureSize { get; set; }

	IEnumerable<string> AvailableCodecs { get; }

	string PreferredCodec { get; set; }

	int CompressionLevel { get; set; }

	event EventHandler DataCaptureStarted;

	event EventHandler DataCaptureEnded;

	bool Start(string filePath);

	bool CaptureScreen(Control control);

	bool Stop(string filePath);
}
