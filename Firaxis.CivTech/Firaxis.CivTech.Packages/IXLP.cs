using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.Packages;

public interface IXLP : IAssemblyInstance, IDisposable, ISerializable, IVersionedData
{
	string Package { get; set; }

	string ClassName { get; set; }

	IList<IXLPEntry> XLPEntries { get; }

	IEnumerable<Platforms> AllowedPlatforms { get; }

	void RemoveEntry(string ID);

	IXLPEntry AddEntry(string ID, string objectName);

	IXLPEntry FindEntry(string ID);

	bool IsPlatformAllowed(Platforms ePlatform);

	void AllowPlatform(Platforms ePlatform);

	void ClearAllowedPlatforms();
}
