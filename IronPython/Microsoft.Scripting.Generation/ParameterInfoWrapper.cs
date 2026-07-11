using System;
using System.Reflection;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Generation;

public class ParameterInfoWrapper : ParameterInfo
{
	private Type _type;

	private string _name;

	public override Type ParameterType => _type;

	public override string Name
	{
		get
		{
			if (_name != null)
			{
				return _name;
			}
			return base.Name;
		}
	}

	public ParameterInfoWrapper(Type parameterType)
	{
		_type = parameterType;
	}

	public ParameterInfoWrapper(Type parameterType, string parameterName)
	{
		_type = parameterType;
		_name = parameterName;
	}

	public override object[] GetCustomAttributes(bool inherit)
	{
		return ArrayUtils.EmptyObjects;
	}

	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		return ArrayUtils.EmptyObjects;
	}
}
