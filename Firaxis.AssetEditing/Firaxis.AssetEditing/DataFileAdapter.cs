using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class DataFileAdapter : DomNodeAdapter
{
	public string ID
	{
		get
		{
			return GetAttribute<string>(BaseSchema.DataFileType.IDAttribute);
		}
		set
		{
			SetAttribute(BaseSchema.DataFileType.IDAttribute, value);
		}
	}

	public string RelativePath
	{
		get
		{
			return GetAttribute<string>(BaseSchema.DataFileType.RelativePathAttribute);
		}
		set
		{
			SetAttribute(BaseSchema.DataFileType.RelativePathAttribute, value);
		}
	}
}
