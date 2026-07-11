using System.Collections;

namespace IronPython.Runtime;

public interface IReversible
{
	IEnumerator __reversed__();
}
