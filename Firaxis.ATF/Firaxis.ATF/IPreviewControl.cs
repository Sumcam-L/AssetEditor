using Firaxis.CivTech.AssetPreviewer;

namespace Firaxis.ATF;

public interface IPreviewControl : ISlotPreviewer
{
	IPreviewWindow PreviewerWindow { get; }

	string PreviewModuleName { get; }
}
