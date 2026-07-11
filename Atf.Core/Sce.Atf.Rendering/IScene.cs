using System.Collections.Generic;

namespace Sce.Atf.Rendering;

public interface IScene : INameable
{
	IList<INode> Nodes { get; }
}
