using System;
using SharpDX.Multimedia;

namespace SharpDX.Serialization;

public class DynamicSerializerAttribute : Attribute
{
	private readonly FourCC id;

	public FourCC Id => id;

	public DynamicSerializerAttribute(int id)
	{
		this.id = id;
	}

	public DynamicSerializerAttribute(string id)
	{
		this.id = id;
	}
}
