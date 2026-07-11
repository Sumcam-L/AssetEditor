using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications;

public interface ISkin
{
	Uri Uri { get; }

	IList<SkinStyle> Styles { get; }
}
