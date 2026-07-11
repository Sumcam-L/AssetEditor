using System;

namespace Bespoke.Common.Net;

public class TcpConnectionEventArgs : EventArgs
{
	private TcpConnection mConnection;

	public TcpConnection Connection => mConnection;

	public TcpConnectionEventArgs(TcpConnection connection)
	{
		mConnection = connection;
	}
}
