using System;
using System.Runtime.Serialization;

namespace Sce.Atf.Dom;

public class AnnotationException : Exception
{
	public AnnotationException()
	{
	}

	public AnnotationException(string message)
		: base(message)
	{
	}

	public AnnotationException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected AnnotationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
