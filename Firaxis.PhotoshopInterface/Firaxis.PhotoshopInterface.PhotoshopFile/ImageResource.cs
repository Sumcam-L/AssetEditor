using System;
using System.Globalization;

namespace Firaxis.PhotoshopInterface.PhotoshopFile;

public abstract class ImageResource
{
	private string signature;

	public string Signature
	{
		get
		{
			return signature;
		}
		set
		{
			if (value.Length != 4)
			{
				throw new ArgumentException("Signature must have length of 4");
			}
			signature = value;
		}
	}

	public string Name { get; set; }

	public abstract ResourceID ID { get; }

	protected ImageResource(string name)
	{
		Signature = "8BIM";
		Name = name;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} {1}", new object[2] { ID, Name });
	}
}
