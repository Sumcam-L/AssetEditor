using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace Firaxis.CivTech;

public class Paths
{
	[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
	public string ArtDev { get; set; }

	[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
	public string Pantry { get; set; }

	[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
	public string LooseAssetRoot { get; set; }

	[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
	public string ArtDefRoot { get; set; }

	[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
	public string ArtDefOutputRoot { get; set; }

	[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
	public string XLPRoot { get; set; }

	[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
	public string XLPOutputRoot { get; set; }

	[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
	public string GameFolder { get; set; }

	public Paths()
	{
		ArtDev = string.Empty;
		Pantry = string.Empty;
		LooseAssetRoot = string.Empty;
		ArtDefRoot = string.Empty;
		ArtDefOutputRoot = string.Empty;
		XLPRoot = string.Empty;
		XLPOutputRoot = string.Empty;
		GameFolder = string.Empty;
	}
}
