using System.Collections.Generic;

namespace Sce.Atf.Rendering;

public interface IPose : INameable
{
	IList<IPoseElement> Elements { get; }

	bool BindPose { get; set; }
}
