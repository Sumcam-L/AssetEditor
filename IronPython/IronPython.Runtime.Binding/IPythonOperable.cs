using System.Dynamic;

namespace IronPython.Runtime.Binding;

internal interface IPythonOperable
{
	DynamicMetaObject BindOperation(PythonOperationBinder action, DynamicMetaObject[] args);
}
