using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Firaxis.Utility.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
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
				ResourceManager resourceManager = new ResourceManager("Firaxis.Utility.Properties.Resources", typeof(Resources).Assembly);
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

	internal static string AssetsRegKey => ResourceManager.GetString("AssetsRegKey", resourceCulture);

	internal static string ConfigRegKey => ResourceManager.GetString("ConfigRegKey", resourceCulture);

	internal static string ConfigXmlName => ResourceManager.GetString("ConfigXmlName", resourceCulture);

	internal static string Enterprise => ResourceManager.GetString("Enterprise", resourceCulture);

	internal static string ProjectsRegKey => ResourceManager.GetString("ProjectsRegKey", resourceCulture);

	internal static Bitmap psgrid
	{
		get
		{
			object obj = ResourceManager.GetObject("psgrid", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static string ToolsRegKey => ResourceManager.GetString("ToolsRegKey", resourceCulture);

	internal Resources()
	{
	}
}
