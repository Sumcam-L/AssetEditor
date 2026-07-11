using System;
using System.Collections.Generic;
using Firaxis.CivTech;

namespace Firaxis.ATF;

internal class RegisteredCookableInfo
{
	public bool CookEnabled { get; set; }

	public FileType FileType { get; private set; }

	public IList<Uri> Referrers { get; private set; }

	public Uri Uri { get; private set; }

	public RegisteredCookableInfo(FileType ft, Uri uri)
	{
		Uri = uri;
		CookEnabled = true;
		FileType = ft;
		Referrers = new List<Uri>();
	}

	public bool IsCookable()
	{
		if (FileType != FileType.ArtDef)
		{
			return FileType == FileType.XLP;
		}
		return true;
	}
}
