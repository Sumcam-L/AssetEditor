using System.Net;
using System.Net.Sockets;
using Bespoke.Common.Net;

namespace Bespoke.Common.Osc;

public class OscClient
{
	private IPAddress mServerIPAddress;

	private int mServerPort;

	private TcpClient mClient;

	private TcpConnection mTcpConnection;

	public IPAddress ServerIPAddress => mServerIPAddress;

	public int ServerPort => mServerPort;

	public TcpClient Client => mClient;

	public OscClient()
	{
		mClient = new TcpClient();
	}

	public OscClient(IPEndPoint serverEndPoint)
		: this(serverEndPoint.Address, serverEndPoint.Port)
	{
	}

	public OscClient(IPAddress serverIPAddress, int serverPort)
		: this()
	{
		mServerIPAddress = serverIPAddress;
		mServerPort = serverPort;
	}

	public void Connect()
	{
		Connect(mServerIPAddress, mServerPort);
	}

	public void Connect(IPEndPoint serverEndPoint)
	{
		Connect(serverEndPoint.Address, serverEndPoint.Port);
	}

	public void Connect(IPAddress serverIPAddress, int serverPort)
	{
		mServerIPAddress = serverIPAddress;
		mServerPort = serverPort;
		mClient.Connect(mServerIPAddress, mServerPort);
		mTcpConnection = new TcpConnection(mClient.Client, OscPacket.LittleEndianByteOrder);
	}

	public void Close()
	{
		mTcpConnection.Dispose();
		mTcpConnection = null;
		mClient.Close();
	}

	public void Send(OscPacket packet)
	{
		byte[] value = packet.ToByteArray();
		mTcpConnection.Writer.Write(OscPacket.ValueToByteArray(value));
	}
}
