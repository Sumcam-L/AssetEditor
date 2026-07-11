using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Firaxis.AssetBrowser.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				ResourceManager resourceManager = new ResourceManager("Firaxis.AssetBrowser.Properties.Resources", typeof(Resources).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
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

	internal static Bitmap analytic_light
	{
		get
		{
			object obj = ResourceManager.GetObject("analytic_light", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap animation
	{
		get
		{
			object obj = ResourceManager.GetObject("animation", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap asset
	{
		get
		{
			object obj = ResourceManager.GetObject("asset", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap firefx
	{
		get
		{
			object obj = ResourceManager.GetObject("firefx", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap free_bsd
	{
		get
		{
			object obj = ResourceManager.GetObject("free_bsd", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap geometry
	{
		get
		{
			object obj = ResourceManager.GetObject("geometry", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap lightrig
	{
		get
		{
			object obj = ResourceManager.GetObject("lightrig", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap material
	{
		get
		{
			object obj = ResourceManager.GetObject("material", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Icon OpenSource
	{
		get
		{
			object obj = ResourceManager.GetObject("OpenSource", resourceCulture);
			return (Icon)obj;
		}
	}

	internal static Bitmap particle
	{
		get
		{
			object obj = ResourceManager.GetObject("particle", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Icon Reimport
	{
		get
		{
			object obj = ResourceManager.GetObject("Reimport", resourceCulture);
			return (Icon)obj;
		}
	}

	internal static Bitmap reimport_texture
	{
		get
		{
			object obj = ResourceManager.GetObject("reimport_texture", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap RenderFromSource
	{
		get
		{
			object obj = ResourceManager.GetObject("RenderFromSource", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap state_graph
	{
		get
		{
			object obj = ResourceManager.GetObject("state_graph", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap sun
	{
		get
		{
			object obj = ResourceManager.GetObject("sun", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap texture
	{
		get
		{
			object obj = ResourceManager.GetObject("texture", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
