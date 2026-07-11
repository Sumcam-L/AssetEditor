using System.Dynamic;

namespace IronPython.Runtime.Binding;

internal interface IPythonGetable
{
	DynamicMetaObject GetMember(PythonGetMemberBinder member, DynamicMetaObject codeContext);
}
