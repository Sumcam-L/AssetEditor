using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Firaxis.AssetPreviewing.Properties;

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
				ResourceManager resourceManager = new ResourceManager("Firaxis.ATF.Properties.Resources", typeof(Resources).Assembly);
				resourceMan = resourceManager;
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

	public static Bitmap arrow_left
	{
		get
		{
			object obj = ResourceManager.GetObject("arrow_left", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap arrow_right
	{
		get
		{
			object obj = ResourceManager.GetObject("arrow_right", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap file_refresh
	{
		get
		{
			object obj = ResourceManager.GetObject("file_refresh", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap None
	{
		get
		{
			object obj = ResourceManager.GetObject("None", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap ObjectClear16
	{
		get
		{
			object obj = ResourceManager.GetObject("ObjectClear16", resourceCulture);
			return (Bitmap)obj;
		}
	}

	public static Bitmap Splashscreenvert
	{
		get
		{
			object obj = ResourceManager.GetObject("Splashscreenvert", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
