using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace WeifenLuo.WinFormsUI.ThemeVS2015;

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
				resourceMan = new ResourceManager("WeifenLuo.WinFormsUI.ThemeVS2015.Resources", typeof(Resources).Assembly);
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

	internal static byte[] vs2015blue_vstheme => (byte[])ResourceManager.GetObject("vs2015blue_vstheme", resourceCulture);

	internal static byte[] vs2015dark_vstheme => (byte[])ResourceManager.GetObject("vs2015dark_vstheme", resourceCulture);

	internal static byte[] vs2015light_vstheme => (byte[])ResourceManager.GetObject("vs2015light_vstheme", resourceCulture);

	internal Resources()
	{
	}
}
