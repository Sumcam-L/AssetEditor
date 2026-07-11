using System.Collections.Generic;

namespace Sce.Atf.Rendering;

public interface ISceneGraphHierarchy
{
	IEnumerable<object> GetChildren();
}
