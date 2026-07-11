using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Firaxis.ATF.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
public class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Firaxis.ATF.Properties.Resources", typeof(Resources).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	public static Bitmap arrow_left => (Bitmap)ResourceManager.GetObject("arrow_left", resourceCulture);

	public static Bitmap arrow_right => (Bitmap)ResourceManager.GetObject("arrow_right", resourceCulture);

	public static Bitmap file_refresh => (Bitmap)ResourceManager.GetObject("file_refresh", resourceCulture);

	public static Bitmap None => (Bitmap)ResourceManager.GetObject("None", resourceCulture);

	public static Bitmap ObjectClear16 => (Bitmap)ResourceManager.GetObject("ObjectClear16", resourceCulture);

	public static Bitmap Splashscreenvert => (Bitmap)ResourceManager.GetObject("Splashscreenvert", resourceCulture);

	internal Resources()
	{
	}
}
