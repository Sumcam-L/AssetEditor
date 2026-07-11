namespace IronPython.Runtime.Types;

internal abstract class InstanceCreator
{
	private readonly PythonType _type;

	protected PythonType Type => _type;

	protected InstanceCreator(PythonType type)
	{
		_type = type;
	}

	public static InstanceCreator Make(PythonType type)
	{
		if (type.IsSystemType)
		{
			return new SystemInstanceCreator(type);
		}
		return new UserInstanceCreator(type);
	}

	internal abstract object CreateInstance(CodeContext context);

	internal abstract object CreateInstance(CodeContext context, object arg0);

	internal abstract object CreateInstance(CodeContext context, object arg0, object arg1);

	internal abstract object CreateInstance(CodeContext context, object arg0, object arg1, object arg2);

	internal abstract object CreateInstance(CodeContext context, params object[] args);

	internal abstract object CreateInstance(CodeContext context, object[] args, string[] names);
}
