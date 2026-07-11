using System.Collections.Generic;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IPreviewerSlotInfo
{
	int SlotID { get; }

	string XLPClass { get; }

	string DefaultAsset { get; }

	bool AllowsNull { get; }

	bool IsSettable { get; }

	IEnumerable<string> AttachmentXLPClasses { get; }
}
