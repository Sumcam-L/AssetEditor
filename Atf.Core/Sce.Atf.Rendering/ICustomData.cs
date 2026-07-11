using System.Collections.Generic;

namespace Sce.Atf.Rendering;

public interface ICustomData
{
	IList<ICustomDataAttribute> CustomAttributes { get; }
}
