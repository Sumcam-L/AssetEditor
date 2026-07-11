using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Applications;

public class ValueInfo
{
	public Type Type;

	public TypeConverter Converter;

	public string Value;

	public readonly List<ValueInfo> ConstructorParams = new List<ValueInfo>();

	public readonly List<Setter> Setters = new List<Setter>();
}
