using System;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public class LayerSectionInfo : LayerInfo
{
	private string key;

	private LayerSectionSubtype? subtype;

	private string blendModeKey;

	public override string Key => key;

	public LayerSectionType SectionType { get; set; }

	public LayerSectionSubtype Subtype
	{
		get
		{
			return subtype ?? LayerSectionSubtype.Normal;
		}
		set
		{
			subtype = value;
		}
	}

	public string BlendModeKey
	{
		get
		{
			return blendModeKey;
		}
		set
		{
			if (value.Length != 4)
			{
				throw new ArgumentException("Blend mode key must have a length of 4.");
			}
			blendModeKey = value;
		}
	}

	public LayerSectionInfo(PsdBinaryReader reader, string key, int dataLength)
	{
		this.key = key;
		SectionType = (LayerSectionType)reader.ReadInt32();
		if (dataLength >= 12)
		{
			string text = reader.ReadAsciiChars(4);
			if (text != "8BIM")
			{
				throw new PsdInvalidException("Invalid section divider signature.");
			}
			BlendModeKey = reader.ReadAsciiChars(4);
			if (dataLength >= 16)
			{
				Subtype = (LayerSectionSubtype)reader.ReadInt32();
			}
		}
	}
}
