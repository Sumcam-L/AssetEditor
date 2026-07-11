using System.Collections.Generic;

namespace Sce.Atf.Rendering;

public interface IAnim
{
	IList<IAnimChannel> Channels { get; }

	IList<IAnim> Animations { get; }
}
