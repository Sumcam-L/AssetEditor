using System;

namespace Sce.Atf;

public class InvalidTransactionException : Exception
{
	public readonly bool ReportError;

	public InvalidTransactionException(string message)
		: this(message, reportError: true, null)
	{
	}

	public InvalidTransactionException(string message, Exception innerException)
		: this(message, reportError: true, innerException)
	{
	}

	public InvalidTransactionException(string message, bool reportError)
		: this(message, reportError, null)
	{
	}

	public InvalidTransactionException(string message, bool reportError, Exception innerException)
		: base(message, innerException)
	{
		ReportError = reportError;
	}
}
