using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Firaxis.AssetPreviewing;

public class PreviewModuleLayout
{
	private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

	private IDictionary<string, string> ClassSpecificLayout = new Dictionary<string, string>();

	public string GeneralLayout { get; set; } = string.Empty;

	public string GlobalLayout { get; set; } = string.Empty;

	public string SerializedClassLayouts
	{
		get
		{
			try
			{
				return Serializer.Serialize(ClassSpecificLayout);
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
				ClassSpecificLayout = ((IDictionary<string, string>)Serializer.Deserialize(value, typeof(IDictionary<string, string>))) ?? new Dictionary<string, string>();
			}
			catch (Exception)
			{
			}
		}
	}

	public string this[string entityClass]
	{
		get
		{
			string value = string.Empty;
			ClassSpecificLayout.TryGetValue(entityClass, out value);
			return value;
		}
		set
		{
			ClassSpecificLayout[entityClass] = value;
		}
	}
}
