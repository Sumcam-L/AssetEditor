using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Firaxis.AssetCloudFramework.Properties;

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
				ResourceManager resourceManager = new ResourceManager("Firaxis.AssetCloudFramework.Properties.Resources", typeof(Resources).Assembly);
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

	public static string AssetCloudSettings => ResourceManager.GetString("AssetCloudSettings", resourceCulture);

	public static string CloudServerAddress => ResourceManager.GetString("CloudServerAddress", resourceCulture);

	public static string x_AssetCloudCookSettings => ResourceManager.GetString("x_AssetCloudCookSettings", resourceCulture);

	public static string x_AssetCloudLog => ResourceManager.GetString("x_AssetCloudLog", resourceCulture);

	internal Resources()
	{
	}
}
