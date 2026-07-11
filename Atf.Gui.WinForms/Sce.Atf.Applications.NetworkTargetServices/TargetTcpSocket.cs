using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sce.Atf.Applications.NetworkTargetServices;

public class TargetTcpSocket
{
	public delegate void ConnectHandler(object sender, Target target);

	public delegate void DataReadyHandler(object sender, byte[] buffer);

	public delegate void ExceptonHandler(object sender, Exception ex);

	private volatile Socket m_theSocket;

	private volatile Target m_curTarget;

	private readonly AsyncCallback m_recieveClb;

	private readonly AsyncCallback m_connectClb;

	private readonly SynchronizationContext m_cctx;

	private volatile object m_syncSocket = new object();

	private int m_messageSize;

	private volatile bool m_ConnectionInProgress;

	public int MessageSize
	{
		get
		{
			return m_messageSize;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentException("Invalid arg");
			}
			lock (m_syncSocket)
			{
				m_messageSize = value;
			}
		}
	}

	public bool IsConnected
	{
		get
		{
			if (m_theSocket == null)
			{
				return false;
			}
			lock (m_syncSocket)
			{
				return m_theSocket != null && m_theSocket.Connected;
			}
		}
	}

	public event ConnectHandler Connected;

	public event ConnectHandler Disconnected;

	public event DataReadyHandler DataReady;

	public event ExceptonHandler UnHandledException;

	public TargetTcpSocket(Socket s)
		: this(5000)
	{
		if (s == null)
		{
			throw new ArgumentNullException();
		}
		m_theSocket = s;
		if (!IsConnected)
		{
			throw new Exception("only accept connected socket");
		}
		IPEndPoint iPEndPoint = (IPEndPoint)s.RemoteEndPoint;
		m_curTarget = new Target("client", iPEndPoint.Address.ToString(), iPEndPoint.Port);
		m_curTarget.IsConnected = true;
	}

	public TargetTcpSocket()
		: this(5000)
	{
	}

	public TargetTcpSocket(int maximumMessageSize)
	{
		m_cctx = SynchronizationContext.Current;
		if (m_cctx == null)
		{
			throw new Exception("The instance of this class can only be created on a threadthat has WindowsFormsSynchronizationContext, ie GUI thread");
		}
		m_theSocket = null;
		m_curTarget = null;
		m_recieveClb = ReceiveClb;
		m_connectClb = ConnectClb;
		m_ConnectionInProgress = false;
		MessageSize = maximumMessageSize;
	}

	public void Connect(Target target)
	{
		lock (m_syncSocket)
		{
			if (m_ConnectionInProgress)
			{
				return;
			}
		}
		if (IsConnected)
		{
			return;
		}
		lock (m_syncSocket)
		{
			if (m_theSocket != null)
			{
				m_theSocket.Close();
				m_theSocket = null;
				m_curTarget = null;
			}
		}
		try
		{
			lock (m_syncSocket)
			{
				IPAddress iPAddress = target.IPAddress;
				m_theSocket = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				m_theSocket.Blocking = true;
				m_theSocket.SendTimeout = 5000;
				m_theSocket.BeginConnect(iPAddress, target.Port, m_connectClb, target);
				m_ConnectionInProgress = true;
			}
		}
		catch (Exception ex)
		{
			lock (m_syncSocket)
			{
				if (m_theSocket != null)
				{
					m_theSocket.Close();
					m_theSocket = null;
					m_curTarget = null;
				}
			}
			OnUnHandledException(ex);
		}
	}

	public void Send(byte[] data)
	{
		Send(data, data.Length);
	}

	public void Send(byte[] data, int size)
	{
		try
		{
			lock (m_syncSocket)
			{
				if (m_theSocket == null || !m_theSocket.Connected)
				{
					throw new Exception("The socket is not connected. Please use Connect method to establish a connection");
				}
				int num = 0;
				for (num = 0; num <= size - m_messageSize; num += m_messageSize)
				{
					m_theSocket.Send(data, num, m_messageSize, SocketFlags.None);
				}
				int num2 = size - num;
				if (num2 > 0)
				{
					m_theSocket.Send(data, num, num2, SocketFlags.None);
				}
			}
		}
		catch (Exception ex)
		{
			lock (m_syncSocket)
			{
				if (m_theSocket != null && !m_theSocket.Connected)
				{
					m_theSocket.Close();
					m_theSocket = null;
					OnDisconnected();
					m_curTarget = null;
				}
			}
			OnUnHandledException(ex);
		}
	}

	public void Disconnect()
	{
		if (IsConnected)
		{
			lock (m_syncSocket)
			{
				m_theSocket.Shutdown(SocketShutdown.Both);
				m_theSocket.Close();
				m_theSocket = null;
				OnDisconnected();
				m_curTarget = null;
			}
		}
	}

	public void StartReceive()
	{
		try
		{
			byte[] array = new byte[m_messageSize];
			if (IsConnected)
			{
				lock (m_syncSocket)
				{
					m_theSocket.BeginReceive(array, 0, m_messageSize, SocketFlags.None, m_recieveClb, array);
					return;
				}
			}
		}
		catch (Exception ex)
		{
			lock (m_syncSocket)
			{
				if (m_theSocket != null && !m_theSocket.Connected)
				{
					m_theSocket.Close();
					m_theSocket = null;
					OnDisconnected();
					m_curTarget = null;
				}
			}
			OnUnHandledException(ex);
		}
	}

	private void ReceiveClb(IAsyncResult ar)
	{
		try
		{
			byte[] src = ar.AsyncState as byte[];
			int num = 0;
			if (IsConnected)
			{
				lock (m_syncSocket)
				{
					num = m_theSocket.EndReceive(ar);
				}
			}
			if (num > 0)
			{
				byte[] tmpBuf = new byte[num];
				Buffer.BlockCopy(src, 0, tmpBuf, 0, num);
				DataReadyHandler dr = this.DataReady;
				if (dr != null)
				{
					m_cctx.Post(delegate
					{
						dr(this, tmpBuf);
					}, null);
				}
			}
			StartReceive();
		}
		catch (Exception ex)
		{
			bool flag = false;
			lock (m_syncSocket)
			{
				if (m_theSocket != null && !m_theSocket.Connected)
				{
					m_theSocket.Close();
					m_theSocket = null;
					flag = true;
				}
			}
			if (flag)
			{
				OnDisconnected();
				m_curTarget = null;
			}
			OnUnHandledException(ex);
		}
	}

	private void ConnectClb(IAsyncResult ar)
	{
		try
		{
			lock (m_syncSocket)
			{
				m_theSocket.EndConnect(ar);
				m_ConnectionInProgress = false;
			}
			OnConnect((Target)ar.AsyncState);
			StartReceive();
		}
		catch (Exception ex)
		{
			lock (m_syncSocket)
			{
				m_ConnectionInProgress = false;
				if (m_theSocket != null)
				{
					m_theSocket.Close();
					m_theSocket = null;
					m_curTarget = null;
				}
			}
			OnUnHandledException(ex);
		}
	}

	private void OnUnHandledException(Exception ex)
	{
		ExceptonHandler handler = this.UnHandledException;
		if (handler != null)
		{
			m_cctx.Send(delegate
			{
				handler(this, ex);
			}, null);
		}
	}

	private void OnDisconnected()
	{
		ConnectHandler handler = this.Disconnected;
		m_curTarget.IsConnected = false;
		lock (m_syncSocket)
		{
			m_ConnectionInProgress = false;
		}
		if (handler != null)
		{
			m_cctx.Send(delegate
			{
				handler(this, m_curTarget);
			}, null);
		}
	}

	private void OnConnect(Target trg)
	{
		ConnectHandler handler = this.Connected;
		m_curTarget = trg;
		m_curTarget.IsConnected = true;
		if (handler != null)
		{
			m_cctx.Send(delegate
			{
				handler(this, trg);
			}, null);
		}
	}
}
