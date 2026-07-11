using System.Collections;
using System.Collections.Generic;

namespace IronPython.Runtime;

internal interface IPythonArray : IList<object>, ICollection<object>, IEnumerable<object>, IEnumerable
{
	string tostring();
}
