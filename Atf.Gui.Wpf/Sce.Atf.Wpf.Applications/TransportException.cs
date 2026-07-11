using System;

namespace Sce.Atf.Wpf.Applications;

public class TransportException : Exception
{
	public TransportException()
	{
	}

	public TransportException(string message)
		: base(message)
	{
	}

	public TransportException(Exception ex)
		: base(ex.Message, ex)
	{
	}
}
