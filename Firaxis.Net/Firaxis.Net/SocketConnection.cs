using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Firaxis.Threading;

namespace Firaxis.Net;

public abstract class SocketConnection
{
	public class OpenConnectionFailedArgs : EventArgs
	{
		public SocketError SocketError;
	}

	private delegate void ConnectAsyncCompletedHandler(SocketAsyncEventArgs e);

	protected delegate void OnConnectionLostHandler();

	private IPEndPoint m_ConnectionTarget;

	private Socket m_Socket;

	private ConnectAsyncCompletedHandler ConnectAsyncCompletedDelegate;

	private System.Timers.Timer m_tmrStatusUpdate = new System.Timers.Timer();

	public bool RetryOnConnectionRefused { get; set; }

	public IPEndPoint ConnectionTarget
	{
		get
		{
			return m_ConnectionTarget;
		}
		set
		{
			if (m_Socket == null)
			{
				m_ConnectionTarget = value;
				return;
			}
			throw new Exception("Cannot change connection target while connection is open.");
		}
	}

	public Control InvokeTarget
	{
		set
		{
			if (value != null)
			{
				ThreadSafeInvoker = new WinFormsThreadSafeInvoker(value);
			}
			else
			{
				ThreadSafeInvoker = null;
			}
		}
	}

	public IThreadSafeInvoker ThreadSafeInvoker { get; set; }

	public bool Connected => m_Socket != null && m_Socket.Connected;

	public bool Connecting => m_Socket != null && !m_Socket.Connected;

	protected OnConnectionLostHandler OnConnectionLostDelegate { get; private set; }

	public event EventHandler ConnectionEstablished;

	public event EventHandler ConnectionLost;

	public event EventHandler<OpenConnectionFailedArgs> OpenConnectionFailed;

	private bool IsAppRunning(string appName)
	{
		bool createdNew;
		using (new Mutex(initiallyOwned: false, appName, out createdNew))
		{
		}
		return !createdNew;
	}

	public bool FindTunerConnectionApp(string ignoreApp, ref string appName)
	{
		string[] array = new string[1] { "FireTuner2" };
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text != ignoreApp && IsAppRunning(text))
			{
				appName = text;
				return true;
			}
		}
		return false;
	}

	public SocketConnection(Control ctrlInvokeTarget, IPEndPoint connectionTarget)
		: this(connectionTarget)
	{
		InvokeTarget = ctrlInvokeTarget;
	}

	public SocketConnection(IThreadSafeInvoker threadSafeInvoker, IPEndPoint connectionTarget)
		: this(connectionTarget)
	{
		ThreadSafeInvoker = threadSafeInvoker;
	}

	protected SocketConnection(IPEndPoint connectionTarget)
	{
		ConnectionTarget = connectionTarget;
		RetryOnConnectionRefused = true;
		ConnectAsyncCompletedDelegate = ConnectAsyncCompleted;
		OnConnectionLostDelegate = OnConnectionLost;
		m_tmrStatusUpdate.Elapsed += m_tmrStatusUpdate_Elapsed;
		m_tmrStatusUpdate.Interval = 500.0;
	}

	public void OpenConnection()
	{
		if (m_Socket != null)
		{
			m_Socket.Dispose();
		}
		m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
		SocketAsyncEventArgs e = new SocketAsyncEventArgs();
		e.RemoteEndPoint = m_ConnectionTarget;
		e.Completed += ConnectAsyncCompleted;
		m_Socket.ConnectAsync(e);
	}

	private void ConnectAsyncCompleted(object sender, SocketAsyncEventArgs e)
	{
		if (e.SocketError != SocketError.OperationAborted)
		{
			SafeInvoke(ConnectAsyncCompletedDelegate, e);
		}
	}

	private void RaiseConnectionFailed(SocketError err)
	{
		EventHandler<OpenConnectionFailedArgs> eventHandler = this.OpenConnectionFailed;
		if (eventHandler != null)
		{
			OpenConnectionFailedArgs openConnectionFailedArgs = new OpenConnectionFailedArgs();
			openConnectionFailedArgs.SocketError = err;
			eventHandler(this, openConnectionFailedArgs);
		}
	}

	private void ConnectAsyncCompleted(SocketAsyncEventArgs e)
	{
		if (e.SocketError == SocketError.Success)
		{
			this.ConnectionEstablished?.Invoke(this, EventArgs.Empty);
			m_tmrStatusUpdate.Enabled = true;
		}
		else if (e.SocketError == SocketError.ConnectionRefused && RetryOnConnectionRefused)
		{
			if (m_Socket != null)
			{
				m_Socket.ConnectAsync(e);
			}
		}
		else
		{
			RaiseConnectionFailed(e.SocketError);
		}
	}

	public void CloseConnection()
	{
		if (m_Socket != null)
		{
			m_tmrStatusUpdate.Enabled = false;
			Socket socket = m_Socket;
			m_Socket = null;
			socket.Close();
		}
	}

	private void m_tmrStatusUpdate_Elapsed(object sender, ElapsedEventArgs e)
	{
		Socket socket = m_Socket;
		if (socket != null && !socket.Connected)
		{
			SafeInvoke(OnConnectionLostDelegate);
		}
	}

	private void OnConnectionLost()
	{
		if (m_Socket != null)
		{
			CloseConnection();
			this.ConnectionLost?.Invoke(this, EventArgs.Empty);
		}
	}

	protected bool SendAsync(SocketAsyncEventArgs args)
	{
		return m_Socket?.SendAsync(args) ?? false;
	}

	protected bool RecieveAsync(SocketAsyncEventArgs args)
	{
		return m_Socket?.ReceiveAsync(args) ?? false;
	}

	protected bool SafeInvoke(Delegate d)
	{
		if (ThreadSafeInvoker != null)
		{
			return ThreadSafeInvoker.Invoke(d);
		}
		return false;
	}

	protected bool SafeInvoke(Delegate d, params object[] args)
	{
		if (ThreadSafeInvoker != null)
		{
			return ThreadSafeInvoker.Invoke(d, args);
		}
		return false;
	}
}
