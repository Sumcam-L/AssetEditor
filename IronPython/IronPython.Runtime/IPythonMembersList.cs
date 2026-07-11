using System.Collections.Generic;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime;

public interface IPythonMembersList : IMembersList
{
	IList<object> GetMemberNames(CodeContext context);
}
