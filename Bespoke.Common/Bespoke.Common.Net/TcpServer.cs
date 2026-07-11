using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Bespoke.Common.Net;

public class TcpServer : IPServer, IDisposable
{
	public static readonly int MaxPendingConnections = 3;

	private IPAddress mIPAddress;

	private int mPort;

	private List<TcpConnection> mClientConnections;

	private List<TcpConnection> mConnectionsToClose;

	private bool mReceiveDataInline;

	private volatile bool mIsShuttingDown;

	private volatile bool mAcceptingConnections;

	private ManualResetEvent mListenerReady;

	private bool mLittleEndianByteOrder;

	public IPAddress IPAddress => mIPAddress;

	public int Port => mPort;

	public bool IsRunning => mAcceptingConnections;

	public int ActiveConnectionCount => mClientConnections.Count;

	public ReadOnlyCollection<TcpConnection> ActiveConnections => mClientConnections.AsReadOnly();

	public bool ReceiveDataInline => mReceiveDataInline;

	public bool LittleEndianByteOrder
	{
		get
		{
			return mLittleEndianByteOrder;
		}
		set
		{
			mLittleEndianByteOrder = value;
		}
	}

	public event EventHandler<TcpConnectionEventArgs> Connected;

	public event EventHandler<TcpConnectionEventArgs> Disconnected;

	public event EventHandler<TcpDataReceivedEventArgs> DataReceived;

	public TcpServer(int port)
		: this(IPAddress.Loopback, port)
	{
	}

	public TcpServer(IPAddress ipAddress, int port, bool receiveDataInline = true, bool littleEndianByteOrder = true)
	{
		mPort = port;
		mIPAddress = ipAddress;
		mReceiveDataInline = receiveDataInline;
		mClientConnections = new List<TcpConnection>();
		mConnectionsToClose = new List<TcpConnection>();
		mIsShuttingDown = false;
		mListenerReady = new ManualResetEvent(initialState: false);
		mLittleEndianByteOrder = littleEndianByteOrder;
	}

	public void Dispose()
	{
		lock (this)
		{
			if (!mIsShuttingDown)
			{
				Stop();
			}
			foreach (TcpConnection mClientConnection in mClientConnections)
			{
				mClientConnection.Dispose();
			}
			mClientConnections.Clear();
			mClientConnections = null;
			mConnectionsToClose.Clear();
			mConnectionsToClose = null;
			if (mListenerReady != null)
			{
				mListenerReady.Close();
				mListenerReady = null;
			}
		}
	}

	public void Start()
	{
		TcpListener tcpListener = null;
		try
		{
			mIsShuttingDown = false;
			mAcceptingConnections = true;
			tcpListener = new TcpListener(mIPAddress, mPort);
			tcpListener.Start(MaxPendingConnections);
			while (mAcceptingConnections)
			{
				mListenerReady.Reset();
				tcpListener.BeginAcceptSocket(EndAcceptSocket, tcpListener);
				mListenerReady.WaitOne();
			}
		}
		finally
		{
			tcpListener?.Stop();
			lock (mClientConnections)
			{
				foreach (TcpConnection mClientConnection in mClientConnections)
				{
					MarkConnectionForClose(mClientConnection);
				}
				CloseMarkedConnections();
			}
			mIsShuttingDown = true;
		}
	}

	public void Stop()
	{
		mAcceptingConnections = false;
		mIsShuttingDown = true;
		mListenerReady.Set();
	}

	public void CloseConnection(TcpConnection connection)
	{
		try
		{
			connection.Dispose();
		}
		catch
		{
		}
		finally
		{
			mClientConnections.Remove(connection);
		}
	}

	private void EndAcceptSocket(IAsyncResult asyncResult)
	{
		try
		{
			TcpListener tcpListener = (TcpListener)asyncResult.AsyncState;
			Socket client = tcpListener.EndAcceptSocket(asyncResult);
			TcpConnection tcpConnection = new TcpConnection(client, mLittleEndianByteOrder);
			tcpConnection.Disconnected += OnDisconnected;
			tcpConnection.DataReceived += OnDataReceived;
			if (mReceiveDataInline)
			{
				tcpConnection.ReceiveDataAsynchronously();
			}
			mClientConnections.Add(tcpConnection);
			OnConnected(new TcpConnectionEventArgs(tcpConnection));
		}
		catch (ObjectDisposedException)
		{
		}
		finally
		{
			mListenerReady.Set();
		}
	}

	private void OnConnected(TcpConnectionEventArgs e)
	{
		if (this.Connected != null)
		{
			this.Connected(this, e);
		}
	}

	private void OnDisconnected(object sender, TcpConnectionEventArgs e)
	{
		if (this.Disconnected != null)
		{
			this.Disconnected(this, e);
		}
	}

	private void OnDataReceived(object sender, TcpDataReceivedEventArgs e)
	{
		if (this.DataReceived != null)
		{
			this.DataReceived(this, e);
		}
	}

	private void MarkConnectionForClose(TcpConnection connection)
	{
		mConnectionsToClose.Add(connection);
	}

	private void CloseMarkedConnections()
	{
		foreach (TcpConnection item in mConnectionsToClose)
		{
			CloseConnection(item);
		}
		mConnectionsToClose.Clear();
	}
}
