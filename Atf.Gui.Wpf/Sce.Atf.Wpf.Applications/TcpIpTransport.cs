using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sce.Atf.Wpf.Applications;

public class TcpIpTransport : ITransportLayer, IDisposable
{
	private class SocketReceiver
	{
		private readonly TcpIpTransport m_transport;

		private readonly string m_name;

		private readonly Socket m_socket;

		private readonly byte[] m_buffer = new byte[8192];

		private readonly object m_streamLock = new object();

		private MemoryStream m_stream = new MemoryStream();

		private readonly AsyncCallback m_callback = null;

		private readonly AutoResetEvent m_packetComposeEvent = new AutoResetEvent(initialState: false);

		private RegisteredWaitHandle m_packetComposeHandle;

		public SocketReceiver(TcpIpTransport transport, Socket socket, string name = "")
		{
			m_name = name;
			m_transport = transport;
			m_socket = socket;
			m_callback = SocketReceiveCallback;
			m_packetComposeHandle = ThreadPool.RegisterWaitForSingleObject(m_packetComposeEvent, PacketComposeTask, null, -1, executeOnlyOnce: false);
			BeginReceive();
		}

		public void BeginReceive()
		{
			if (m_callback != null && !m_transport.Closed)
			{
				m_socket.BeginReceive(m_buffer, 0, 8192, SocketFlags.None, m_callback, null);
			}
		}

		public void Stop()
		{
			if (m_packetComposeHandle != null)
			{
				m_packetComposeHandle.Unregister(null);
				m_packetComposeHandle = null;
			}
		}

		private void SocketReceiveCallback(IAsyncResult asyncResult)
		{
			try
			{
				int num = (m_socket.Connected ? m_socket.EndReceive(asyncResult) : 0);
				if (num > 0)
				{
					lock (m_streamLock)
					{
						long position = m_stream.Position;
						m_stream.Seek(0L, SeekOrigin.End);
						m_stream.Write(m_buffer, 0, num);
						m_stream.Position = position;
					}
					m_packetComposeEvent.Set();
					if (m_transport.CanAddMorePackets())
					{
						BeginReceive();
					}
				}
				else
				{
					PacketComposeTask(null, timedOut: false);
					if (m_stream.Length > 0)
					{
						m_transport.Exception = new TransportException("Broken Packet");
					}
					m_transport.DisconnectMessage = "Connection was closed";
					m_transport.EnqueuePacket(new byte[0]);
					m_transport.Exception = new Exception("Socket disconnected");
				}
			}
			catch (Exception exception)
			{
				m_transport.Exception = exception;
			}
		}

		private void PacketComposeTask(object state, bool timedOut)
		{
			try
			{
				bool flag = false;
				lock (m_streamLock)
				{
					m_stream.Position = 0L;
					byte[] array = new byte[4];
					while (m_stream.Length - m_stream.Position >= 4)
					{
						if (m_transport.Closed)
						{
							return;
						}
						long position = m_stream.Position;
						m_stream.Read(array, 0, 4);
						m_stream.Position = position;
						ushort num = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(array, 2));
						num += 4;
						if (m_stream.Length - position < num)
						{
							break;
						}
						byte[] array2 = new byte[num];
						if (m_stream.Read(array2, 0, num) != num)
						{
							throw new TransportException("Unable to read data");
						}
						m_transport.EnqueuePacket(array2);
						flag = true;
					}
					if (flag)
					{
						MemoryStream memoryStream = new MemoryStream();
						int num2 = (int)(m_stream.Length - m_stream.Position);
						if (num2 > 0)
						{
							byte[] buffer = new byte[num2];
							m_stream.Read(buffer, 0, num2);
							memoryStream.Write(buffer, 0, num2);
						}
						m_stream = memoryStream;
					}
				}
				if (flag)
				{
					m_transport.SignalTransportEvent();
				}
			}
			catch (Exception exception)
			{
				m_transport.Exception = exception;
			}
		}
	}

	private class OutPacket
	{
		public readonly byte[] Data;

		public readonly int Length;

		public int BytesSent = 0;

		public OutPacket(byte[] packet)
		{
			Data = packet;
			Length = packet.Length;
		}
	}

	private DateTime m_connectBeginTime;

	private TcpIpTarget m_target;

	private readonly IPEndPoint m_remoteEndPoint;

	private readonly Socket m_commandSocket;

	private readonly Socket m_eventDataSocket;

	private DateTime m_onlineStartTime = DateTime.Now;

	private readonly AutoResetEvent m_transportEvent = null;

	private readonly object m_stateLock = new object();

	private volatile bool m_closed = false;

	private bool m_disposed = false;

	private Exception m_exception = null;

	private const ushort KPacketHeaderSize = 4;

	private const int KPacketHeaderLengthOffset = 2;

	private const int ReceiveBufferSize = 8192;

	private const int InPacketMaxEnqueued = 10000;

	private SocketReceiver m_comandReceiver;

	private SocketReceiver m_eventDataReceiver;

	private Queue<byte[]> m_inPackets = new Queue<byte[]>();

	private object m_inPacketLock = new object();

	private bool m_inPacketStopped;

	private Thread m_sendThread;

	private AutoResetEvent m_outPacketNewEvent = new AutoResetEvent(initialState: false);

	private AutoResetEvent m_outPacketSentEvent = new AutoResetEvent(initialState: false);

	private AutoResetEvent m_sendThreadStopEvent = new AutoResetEvent(initialState: false);

	private Queue<OutPacket> m_outPackets = new Queue<OutPacket>();

	private object m_outPacketsLock = new object();

	public TimeSpan ConnectTimeout { get; set; }

	public bool Connected => m_commandSocket.Connected && m_eventDataSocket.Connected;

	public AutoResetEvent TransportEvent => m_transportEvent;

	internal bool Closed => m_closed;

	public string DisconnectMessage { get; internal set; }

	public Exception Exception
	{
		get
		{
			return m_exception;
		}
		set
		{
			if (m_closed)
			{
				return;
			}
			lock (m_stateLock)
			{
				if (m_exception == null)
				{
					if (value is SocketException)
					{
						value = new TransportException(value);
					}
					m_exception = value;
					m_transportEvent.Set();
				}
			}
		}
	}

	public void BeginConnect()
	{
		try
		{
			m_connectBeginTime = DateTime.Now;
			m_commandSocket.BeginConnect(m_remoteEndPoint, ConnectCallback, "CommandSocket");
		}
		catch (Exception exception)
		{
			Exception = exception;
			throw Exception;
		}
	}

	private void ConnectCallback(IAsyncResult asyncResult)
	{
		try
		{
			Exception ex = null;
			try
			{
				if (m_closed)
				{
					return;
				}
				if (asyncResult.AsyncState.Equals("CommandSocket"))
				{
					m_commandSocket.EndConnect(asyncResult);
					if (m_commandSocket.Connected)
					{
						m_eventDataSocket.BeginConnect(m_remoteEndPoint, ConnectCallback, "EventDataSocket");
						return;
					}
					ex = new TransportException("Unable to connect");
				}
				else if (asyncResult.AsyncState.Equals("EventDataSocket"))
				{
					m_eventDataSocket.EndConnect(asyncResult);
					if (m_eventDataSocket.Connected)
					{
						lock (m_stateLock)
						{
							SendThreadStart();
							ReceiveThreadStart();
						}
						if (m_target != null)
						{
							m_target.IsConnected = true;
						}
						m_transportEvent.Set();
						return;
					}
					ex = new TransportException("Unable to connect");
				}
			}
			catch (SocketException ex2)
			{
				ex = ex2;
			}
			bool flag = false;
			TimeSpan timeSpan = DateTime.Now - m_connectBeginTime;
			if (!(TimeSpan.Zero == ConnectTimeout))
			{
				if (ConnectTimeout == TimeSpan.MaxValue)
				{
					flag = true;
				}
				else if (timeSpan < ConnectTimeout)
				{
					flag = true;
				}
			}
			if (flag)
			{
				if (!m_closed)
				{
					m_commandSocket.BeginConnect(m_remoteEndPoint, ConnectCallback, "CommandSocket");
				}
				return;
			}
			Exception = new TransportException(string.Format("Connection Timeout", ex.Message));
		}
		catch (SocketException exception)
		{
			Exception = exception;
		}
		catch (ObjectDisposedException exception2)
		{
			Exception = exception2;
		}
		m_transportEvent.Set();
	}

	public TcpIpTransport(TcpIpTarget target)
	{
		DisconnectMessage = null;
		if (target == null)
		{
			throw new ArgumentException("target is null");
		}
		IPAddress address = null;
		if (!IPAddress.TryParse(target.IpAddress, out address))
		{
			try
			{
				IPHostEntry hostEntry = Dns.GetHostEntry(target.IpAddress);
				if (hostEntry.AddressList.Length != 0)
				{
					address = hostEntry.AddressList[0];
				}
			}
			catch
			{
				throw new ArgumentException("Invalid Host");
			}
		}
		m_target = target;
		m_remoteEndPoint = new IPEndPoint(address, (int)target.Port);
		ConnectTimeout = TimeSpan.MaxValue;
		m_transportEvent = new AutoResetEvent(initialState: false);
		m_commandSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		m_eventDataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	}

	~TcpIpTransport()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_disposed)
		{
			return;
		}
		m_disposed = true;
		if (disposing)
		{
			if (m_commandSocket != null)
			{
				m_commandSocket.Dispose();
			}
			if (m_eventDataSocket != null)
			{
				m_eventDataSocket.Dispose();
			}
		}
	}

	public void Close()
	{
		lock (m_stateLock)
		{
			m_closed = true;
			ConnectTimeout = TimeSpan.Zero;
			if (m_target != null)
			{
				m_target.IsConnected = false;
			}
			if (m_commandSocket != null && m_commandSocket.Connected)
			{
				m_commandSocket.Shutdown(SocketShutdown.Both);
				m_commandSocket.Disconnect(reuseSocket: false);
			}
			if (m_eventDataSocket != null && m_eventDataSocket.Connected)
			{
				m_eventDataSocket.Shutdown(SocketShutdown.Both);
				m_eventDataSocket.Disconnect(reuseSocket: false);
			}
			SendThreadStop();
			ReceiveThreadStop();
		}
	}

	internal void SignalTransportEvent()
	{
		m_transportEvent.Set();
	}

	internal bool CanAddMorePackets()
	{
		lock (m_inPacketLock)
		{
			if (m_inPackets.Count >= 10000)
			{
				m_inPacketStopped = true;
			}
		}
		return !m_inPacketStopped;
	}

	internal void EnqueuePacket(byte[] packet)
	{
		lock (m_inPacketLock)
		{
			m_inPackets.Enqueue(packet);
		}
	}

	public Queue<byte[]> GetIncomingPackets()
	{
		bool flag = false;
		Queue<byte[]> inPackets;
		lock (m_inPacketLock)
		{
			inPackets = m_inPackets;
			m_inPackets = new Queue<byte[]>();
			if (m_inPacketStopped)
			{
				flag = true;
				m_inPacketStopped = false;
			}
		}
		if (flag)
		{
			try
			{
				if (m_comandReceiver != null)
				{
					m_comandReceiver.BeginReceive();
				}
				if (m_eventDataReceiver != null)
				{
					m_eventDataReceiver.BeginReceive();
				}
			}
			catch (Exception exception)
			{
				Exception = exception;
			}
		}
		return inPackets;
	}

	private void ReceiveThreadStart()
	{
		lock (m_stateLock)
		{
			if (m_comandReceiver == null)
			{
				m_comandReceiver = new SocketReceiver(this, m_commandSocket, "Command");
			}
			if (m_eventDataReceiver == null)
			{
				m_eventDataReceiver = new SocketReceiver(this, m_eventDataSocket, "Event");
			}
		}
	}

	private void ReceiveThreadStop()
	{
		lock (m_stateLock)
		{
			if (m_comandReceiver != null)
			{
				m_comandReceiver.Stop();
			}
			if (m_eventDataReceiver != null)
			{
				m_eventDataReceiver.Stop();
			}
		}
	}

	public void BeginSend(byte[] data)
	{
		lock (m_outPacketsLock)
		{
			m_outPackets.Enqueue(new OutPacket(data));
		}
		m_outPacketNewEvent.Set();
	}

	private void SendThreadStart()
	{
		lock (m_stateLock)
		{
			if (m_sendThread == null)
			{
				m_sendThread = new Thread(SendThread);
				m_sendThread.IsBackground = true;
				m_sendThread.Start();
			}
		}
	}

	private void SendThreadStop()
	{
		lock (m_stateLock)
		{
			if (m_sendThread != null)
			{
				m_sendThreadStopEvent.Set();
				if (m_sendThread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
				{
					m_sendThread.Join();
				}
				m_sendThread = null;
			}
		}
	}

	private void SendThread()
	{
		try
		{
			WaitHandle[] waitHandles = new WaitHandle[2] { m_sendThreadStopEvent, m_outPacketNewEvent };
			WaitHandle[] waitHandles2 = new WaitHandle[2] { m_sendThreadStopEvent, m_outPacketSentEvent };
			while (WaitHandle.WaitAny(waitHandles) != 0)
			{
				Queue<OutPacket> outPackets;
				lock (m_outPacketsLock)
				{
					outPackets = m_outPackets;
					m_outPackets = new Queue<OutPacket>();
				}
				while (outPackets.Count > 0)
				{
					OutPacket outPacket = outPackets.Dequeue();
					do
					{
						m_commandSocket.BeginSend(outPacket.Data, outPacket.BytesSent, outPacket.Length - outPacket.BytesSent, SocketFlags.None, SendCallback, outPacket);
						if (WaitHandle.WaitAny(waitHandles2) == 0)
						{
							throw new TransportException("ErrProtocolConnectionClosed");
						}
					}
					while (outPacket.BytesSent < outPacket.Length);
				}
			}
		}
		catch (Exception exception)
		{
			Exception = exception;
		}
	}

	private void SendCallback(IAsyncResult ar)
	{
		OutPacket outPacket = (OutPacket)ar.AsyncState;
		try
		{
			int num = m_commandSocket.EndSend(ar);
			outPacket.BytesSent += num;
		}
		catch (Exception exception)
		{
			Exception = exception;
			outPacket.BytesSent = outPacket.Length;
		}
		finally
		{
			m_outPacketSentEvent.Set();
		}
	}
}
