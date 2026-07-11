using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Firaxis.AssetPreviewing;

public class PreviewerLayout
{
	private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

	private IDictionary<string, PreviewModuleLayout> PreviewClassLayouts = new Dictionary<string, PreviewModuleLayout>();

	public string SerializedLayouts
	{
		get
		{
			try
			{
				return Serializer.Serialize(PreviewClassLayouts);
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}
		set
		{
			try
			{
				PreviewClassLayouts = ((IDictionary<string, PreviewModuleLayout>)Serializer.Deserialize(value, typeof(IDictionary<string, PreviewModuleLayout>))) ?? new Dictionary<string, PreviewModuleLayout>();
			}
			catch (Exception)
			{
			}
		}
	}

	public IEnumerable<string> ModuleNames => PreviewClassLayouts.Keys;

	public PreviewModuleLayout this[string moduleName]
	{
		get
		{
			if (!PreviewClassLayouts.ContainsKey(moduleName))
			{
				PreviewClassLayouts[moduleName] = new PreviewModuleLayout();
			}
			return PreviewClassLayouts[moduleName];
		}
	}
}
