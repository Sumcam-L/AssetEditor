using System;

namespace Bespoke.Common.Net;

public class TcpDataReceivedEventArgs : EventArgs
{
	private TcpConnection mConnection;

	private byte[] mData;

	public TcpConnection Connection => mConnection;

	public byte[] Data => mData;

	public TcpDataReceivedEventArgs(TcpConnection connection, byte[] data)
	{
		mConnection = connection;
		mData = data;
	}
}
