using System;

namespace Sce.Atf.Core;

public class ArgParserException : Exception
{
	public ArgParserException(string message)
		: base(message)
	{
	}
}
