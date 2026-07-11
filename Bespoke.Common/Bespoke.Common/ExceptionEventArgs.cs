using System;

namespace Bespoke.Common;

public class ExceptionEventArgs : EventArgs
{
	private Exception mException;

	public Exception Exception => mException;

	public ExceptionEventArgs(Exception ex)
	{
		mException = ex;
	}
}
