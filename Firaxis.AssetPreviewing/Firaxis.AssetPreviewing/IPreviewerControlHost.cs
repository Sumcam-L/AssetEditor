using System.Windows.Forms;

namespace Firaxis.AssetPreviewing;

public interface IPreviewerControlHost
{
	Control PreviewerControl { get; }
}
