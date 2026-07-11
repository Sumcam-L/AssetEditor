using System;
using System.Net;
using System.Net.Sockets;

namespace Bespoke.Common.Net;

public class UdpServer : IPServer
{
	private class UdpState
	{
		private UdpClient mClient;

		private IPEndPoint mIPEndPoint;

		public UdpClient Client => mClient;

		public IPEndPoint IPEndPoint => mIPEndPoint;

		public UdpState(UdpClient client, IPEndPoint ipEndPoint)
		{
			mClient = client;
			mIPEndPoint = ipEndPoint;
		}
	}

	private IPAddress mIPAddress;

	private int mPort;

	private IPAddress mMulticastAddress;

	private TransmissionType mTransmissionType;

	private UdpClient mUdpClient;

	private AsyncCallback mAsynCallback;

	private volatile bool mAcceptingConnections;

	public IPAddress IPAddress => mIPAddress;

	public int Port => mPort;

	public IPAddress MulticastAddress => mMulticastAddress;

	public bool IsRunning => mAcceptingConnections;

	public TransmissionType TransmissionType => mTransmissionType;

	public event EventHandler<UdpDataReceivedEventArgs> DataReceived;

	public UdpServer(int port)
		: this(IPAddress.Loopback, port, null, TransmissionType.LocalBroadcast)
	{
	}

	public UdpServer(int port, IPAddress multicastAddress)
		: this(IPAddress.Loopback, port, multicastAddress, TransmissionType.Multicast)
	{
	}

	public UdpServer(IPAddress ipAddress, int port)
		: this(ipAddress, port, null, TransmissionType.Unicast)
	{
	}

	public UdpServer(IPAddress ipAddress, int port, IPAddress multicastAddress, TransmissionType transmissionType)
	{
		mPort = port;
		mIPAddress = ipAddress;
		mTransmissionType = transmissionType;
		if (mTransmissionType == TransmissionType.Multicast)
		{
			Assert.ParamIsNotNull(multicastAddress);
			mMulticastAddress = multicastAddress;
		}
		mAsynCallback = EndReceive;
	}

	public void Start()
	{
		IPEndPoint iPEndPoint;
		switch (mTransmissionType)
		{
		case TransmissionType.Unicast:
			iPEndPoint = new IPEndPoint(mIPAddress, mPort);
			mUdpClient = new UdpClient(iPEndPoint);
			break;
		case TransmissionType.Multicast:
		{
			iPEndPoint = new IPEndPoint(IPAddress.Any, mPort);
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
			socket.Bind(iPEndPoint);
			mUdpClient = new UdpClient();
			mUdpClient.Client = socket;
			mUdpClient.JoinMulticastGroup(mMulticastAddress);
			break;
		}
		case TransmissionType.Broadcast:
		case TransmissionType.LocalBroadcast:
			iPEndPoint = new IPEndPoint(IPAddress.Any, mPort);
			mUdpClient = new UdpClient(iPEndPoint);
			break;
		default:
			throw new Exception();
		}
		UdpState state = new UdpState(mUdpClient, iPEndPoint);
		mAcceptingConnections = true;
		mUdpClient.BeginReceive(mAsynCallback, state);
	}

	public void Stop()
	{
		mAcceptingConnections = false;
		if (mUdpClient != null)
		{
			if (mTransmissionType == TransmissionType.Multicast)
			{
				mUdpClient.DropMulticastGroup(mMulticastAddress);
			}
			mUdpClient.Close();
		}
	}

	private void EndReceive(IAsyncResult asyncResult)
	{
		try
		{
			if (mAcceptingConnections)
			{
				UdpState udpState = (UdpState)asyncResult.AsyncState;
				UdpClient client = udpState.Client;
				IPEndPoint remoteEP = udpState.IPEndPoint;
				byte[] array = client.EndReceive(asyncResult, ref remoteEP);
				if (array != null && array.Length > 0)
				{
					OnDataReceived(new UdpDataReceivedEventArgs(remoteEP, array));
				}
				client.BeginReceive(mAsynCallback, udpState);
			}
		}
		catch (ObjectDisposedException)
		{
		}
	}

	private void OnDataReceived(UdpDataReceivedEventArgs e)
	{
		if (this.DataReceived != null)
		{
			this.DataReceived(this, e);
		}
	}
}
