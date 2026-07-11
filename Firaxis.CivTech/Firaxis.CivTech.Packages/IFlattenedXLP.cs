using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.Packages;

public interface IFlattenedXLP : IXLP, IAssemblyInstance, IDisposable, ISerializable, IVersionedData
{
	void AddXLP(IXLP xlp);

	void AddXLPs(IEnumerable<IXLP> xlps);

	void Reset();
}
