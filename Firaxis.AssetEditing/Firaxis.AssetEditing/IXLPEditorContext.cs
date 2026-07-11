using Firaxis.ATF;
using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

public interface IXLPEditorContext
{
	IPlatformSelectorContext PlatformSelectorContext { get; }

	IPropertyEditingContext XLPContext { get; }

	IPropertyEditingListContext XLPEntriesContext { get; }
}
