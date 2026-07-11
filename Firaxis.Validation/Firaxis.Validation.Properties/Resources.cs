using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Firaxis.Validation.Properties;

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
				ResourceManager resourceManager = new ResourceManager("Firaxis.Validation.Properties.Resources", typeof(Resources).Assembly);
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

	internal static Bitmap appwindow_info_annotation
	{
		get
		{
			object obj = ResourceManager.GetObject("appwindow_info_annotation", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap clear_results
	{
		get
		{
			object obj = ResourceManager.GetObject("clear_results", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Control_Checkbox
	{
		get
		{
			object obj = ResourceManager.GetObject("Control_Checkbox", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Control_SaveFileDialog
	{
		get
		{
			object obj = ResourceManager.GetObject("Control_SaveFileDialog", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Control_Uncheckbox
	{
		get
		{
			object obj = ResourceManager.GetObject("Control_Uncheckbox", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap graph_level
	{
		get
		{
			object obj = ResourceManager.GetObject("graph_level", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static string ValidateError => ResourceManager.GetString("ValidateError", resourceCulture);

	internal static string ValidateNone => ResourceManager.GetString("ValidateNone", resourceCulture);

	internal static string ValidateSuccess => ResourceManager.GetString("ValidateSuccess", resourceCulture);

	internal static string ValidateWarning => ResourceManager.GetString("ValidateWarning", resourceCulture);

	internal static Bitmap ver_new
	{
		get
		{
			object obj = ResourceManager.GetObject("ver_new", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap ver_none
	{
		get
		{
			object obj = ResourceManager.GetObject("ver_none", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap ver_ok
	{
		get
		{
			object obj = ResourceManager.GetObject("ver_ok", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap ver_old
	{
		get
		{
			object obj = ResourceManager.GetObject("ver_old", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap version_task
	{
		get
		{
			object obj = ResourceManager.GetObject("version_task", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
