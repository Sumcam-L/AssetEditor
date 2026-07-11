using System;
using System.Net;

namespace Bespoke.Common.Net;

public class UdpDataReceivedEventArgs : EventArgs
{
	private IPEndPoint mSourceEndPoint;

	private byte[] mData;

	public IPEndPoint SourceEndPoint => mSourceEndPoint;

	public byte[] Data => mData;

	public UdpDataReceivedEventArgs(IPEndPoint sourceEndPoint, byte[] data)
	{
		mSourceEndPoint = sourceEndPoint;
		mData = data;
	}
}
