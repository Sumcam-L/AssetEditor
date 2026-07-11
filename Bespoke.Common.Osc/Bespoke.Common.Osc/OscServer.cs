using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Bespoke.Common.Net;

namespace Bespoke.Common.Osc;

public class OscServer : IPServer
{
	private TransportType mTransportType;

	private IPAddress mIPAddress;

	private int mPort;

	private IPEndPoint mIPEndPoint;

	private IPAddress mMulticastAddress;

	private UdpServer mUdpServer;

	private TcpServer mTcpServer;

	private Thread mTcpServerThread;

	private List<string> mRegisteredMethods;

	private bool mFilterRegisteredMethods;

	private TransmissionType mTransmissionType;

	private volatile bool mHandleMessages;

	private bool mConsumeParsingExceptions;

	public TransportType TransportType => mTransportType;

	public IPAddress IPAddress => mIPAddress;

	public int Port => mPort;

	public IPEndPoint IPEndPoint => mIPEndPoint;

	public IPAddress MulticastAddress => mMulticastAddress;

	public string[] RegisteredMethods => mRegisteredMethods.ToArray();

	public bool FilterRegisteredMethods
	{
		get
		{
			return mFilterRegisteredMethods;
		}
		set
		{
			mFilterRegisteredMethods = value;
		}
	}

	public TransmissionType TransmissionType => mTransmissionType;

	public bool IsRunning => mHandleMessages;

	public bool ConsumeParsingExceptions
	{
		get
		{
			return mConsumeParsingExceptions;
		}
		set
		{
			mConsumeParsingExceptions = value;
		}
	}

	public event EventHandler<OscPacketReceivedEventArgs> PacketReceived;

	public event EventHandler<OscBundleReceivedEventArgs> BundleReceived;

	public event EventHandler<OscMessageReceivedEventArgs> MessageReceived;

	public event EventHandler<ExceptionEventArgs> ReceiveErrored;

	public OscServer(TransportType transportType, IPAddress ipAddress, int port, bool consumeParsingExceptions = true)
		: this(transportType, ipAddress, port, null, TransmissionType.Unicast, consumeParsingExceptions)
	{
	}

	public OscServer(IPAddress multicastAddress, int port, bool consumeParsingExceptions = true)
		: this(TransportType.Udp, IPAddress.Loopback, port, multicastAddress, TransmissionType.Multicast, consumeParsingExceptions)
	{
	}

	private OscServer(TransportType transportType, IPAddress ipAddress, int port, IPAddress multicastAddress, TransmissionType transmissionType, bool consumeParsingExceptions = true)
	{
		Assert.IsTrue(transportType == TransportType.Udp || transportType == TransportType.Tcp);
		if (transportType == TransportType.Tcp && transmissionType != TransmissionType.Unicast)
		{
			throw new InvalidOperationException("TCP must be used with TransmissionType.Unicast.");
		}
		mTransportType = transportType;
		mIPAddress = ipAddress;
		mPort = port;
		mIPEndPoint = new IPEndPoint(ipAddress, port);
		mTransmissionType = transmissionType;
		if (mTransmissionType == TransmissionType.Multicast)
		{
			Assert.ParamIsNotNull(multicastAddress);
			mMulticastAddress = multicastAddress;
		}
		mRegisteredMethods = new List<string>();
		mFilterRegisteredMethods = true;
		mConsumeParsingExceptions = consumeParsingExceptions;
		switch (mTransportType)
		{
		case TransportType.Udp:
			mUdpServer = new UdpServer(mIPAddress, mPort, mMulticastAddress, mTransmissionType);
			mUdpServer.DataReceived += mUdpServer_DataReceived;
			break;
		case TransportType.Tcp:
			mTcpServer = new TcpServer(mIPAddress, mPort, receiveDataInline: true, OscPacket.LittleEndianByteOrder);
			mTcpServer.DataReceived += mTcpServer_DataReceived;
			break;
		default:
			throw new InvalidOperationException("Invalid transport type: " + mTransportType);
		}
	}

	public void Start()
	{
		mHandleMessages = true;
		switch (mTransportType)
		{
		case TransportType.Udp:
			mUdpServer.Start();
			break;
		case TransportType.Tcp:
			mTcpServerThread = new Thread(mTcpServer.Start);
			mTcpServerThread.Name = "OscServer Thread";
			mTcpServerThread.Start();
			break;
		default:
			throw new InvalidOperationException("Invalid transport type: " + mTransportType);
		}
	}

	public void Stop()
	{
		mHandleMessages = false;
		switch (mTransportType)
		{
		case TransportType.Udp:
			if (mUdpServer != null)
			{
				mUdpServer.Stop();
			}
			break;
		case TransportType.Tcp:
			if (mTcpServer != null)
			{
				mTcpServer.Stop();
				if (mTcpServerThread != null)
				{
					mTcpServerThread.Join();
					mTcpServerThread = null;
				}
			}
			break;
		default:
			throw new InvalidOperationException("Invalid transport type: " + mTransportType);
		}
	}

	public void RegisterMethod(string method)
	{
		if (!mRegisteredMethods.Contains(method))
		{
			mRegisteredMethods.Add(method);
		}
	}

	public void UnRegisterMethod(string method)
	{
		mRegisteredMethods.Remove(method);
	}

	public void ClearMethods()
	{
		mRegisteredMethods.Clear();
	}

	private void mUdpServer_DataReceived(object sender, UdpDataReceivedEventArgs e)
	{
		DataReceived(e.SourceEndPoint, e.Data);
	}

	private void mTcpServer_DataReceived(object sender, TcpDataReceivedEventArgs e)
	{
		DataReceived((IPEndPoint)e.Connection.Client.RemoteEndPoint, e.Data);
	}

	private void DataReceived(IPEndPoint sourceEndPoint, byte[] data)
	{
		if (!mHandleMessages)
		{
			return;
		}
		try
		{
			OscPacket oscPacket = OscPacket.FromByteArray(sourceEndPoint, data);
			OnPacketReceived(oscPacket);
			if (oscPacket.IsBundle)
			{
				OnBundleReceived(oscPacket as OscBundle);
			}
			else if (mFilterRegisteredMethods)
			{
				if (mRegisteredMethods.Contains(oscPacket.Address))
				{
					OnMessageReceived(oscPacket as OscMessage);
				}
			}
			else
			{
				OnMessageReceived(oscPacket as OscMessage);
			}
		}
		catch (Exception ex)
		{
			if (!mConsumeParsingExceptions)
			{
				OnReceiveErrored(ex);
			}
		}
	}

	private void OnPacketReceived(OscPacket packet)
	{
		if (this.PacketReceived != null)
		{
			this.PacketReceived(this, new OscPacketReceivedEventArgs(packet));
		}
	}

	private void OnBundleReceived(OscBundle bundle)
	{
		if (this.BundleReceived != null)
		{
			this.BundleReceived(this, new OscBundleReceivedEventArgs(bundle));
		}
		foreach (object datum in bundle.Data)
		{
			if (datum is OscBundle)
			{
				OnBundleReceived((OscBundle)datum);
			}
			else
			{
				if (!(datum is OscMessage))
				{
					continue;
				}
				OscMessage oscMessage = (OscMessage)datum;
				if (mFilterRegisteredMethods)
				{
					if (mRegisteredMethods.Contains(oscMessage.Address))
					{
						OnMessageReceived(oscMessage);
					}
				}
				else
				{
					OnMessageReceived(oscMessage);
				}
			}
		}
	}

	private void OnMessageReceived(OscMessage message)
	{
		if (this.MessageReceived != null)
		{
			this.MessageReceived(this, new OscMessageReceivedEventArgs(message));
		}
	}

	private void OnReceiveErrored(Exception ex)
	{
		if (this.ReceiveErrored != null)
		{
			this.ReceiveErrored(this, new ExceptionEventArgs(ex));
		}
	}
}
