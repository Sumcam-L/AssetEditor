using System.Collections.Generic;

namespace Sce.Atf.Rendering;

public interface IAnimClip : INameable
{
	IList<IAnim> Anims { get; }
}
