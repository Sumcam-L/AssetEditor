using System.Collections.Generic;

namespace Firaxis.CivTech.Packages;

public interface IXLPClassSet
{
	IEnumerable<IXLPClass> Items { get; }

	void RemoveClass(IXLPClass cl);

	IXLPClass CreateClass(string name);
}
