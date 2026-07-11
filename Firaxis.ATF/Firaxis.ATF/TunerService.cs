using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.Error;
using Firaxis.Net;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.ATF;

[Export(typeof(IHotLoadService))]
[Export(typeof(ITunerService))]
[Export(typeof(TunerService))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TunerService : SocketConnection, IInitializable, ITunerService, IHotLoadService, IDisposable
{
	private delegate void ListenForMessagesDelegate();

	private struct RequestResponse
	{
		public int Listener;

		public List<string> Messages;
	}

	public delegate void OutputMsgHandler(string sOutputMsg);

	public delegate void RequestListener(List<string> response);

	private enum ConnectionEventType
	{
		TryConnect,
		Connected,
		TryDisconnect,
		Disconnected
	}

	private int m_iReadBufferOffset;

	private byte[] m_ReadBuffer = new byte[1048576];

	private ListenForMessagesDelegate m_ListenForMessagesDelegate;

	private SocketAsyncEventArgs m_RecieveArgs;

	private bool m_bPendingRecieve;

	private byte[] m_Message;

	private int m_iMessageIndex;

	private object m_ResponseLock = new object();

	private List<RequestResponse> m_ResponsesToProcess = new List<RequestResponse>();

	private bool m_bRoutingMessages;

	private List<RequestResponse> m_Responses = new List<RequestResponse>();

	public OutputMsgHandler OnOutputMsgRecieved;

	private RequestListener m_DefaultRequestHandler;

	private IList<RequestListener> m_RequestListeners = new List<RequestListener>();

	private bool m_disposedValue;

	private ICollection<IHotLoadData> m_hotLoadDataCollection = new List<IHotLoadData>();

	private bool m_enabled = true;

	private FileSystemWatcher m_logsFileWatcher;

	private readonly ConcurrentQueue<ConnectionEventType> m_connectionRequestQueue = new ConcurrentQueue<ConnectionEventType>();

	private readonly TimeSpan m_connectionRetryDelay = TimeSpan.FromSeconds(10.0);

	private readonly Thread m_connectionThread;

	private readonly AutoResetEvent m_connectionThreadSignal = new AutoResetEvent(initialState: false);

	private readonly ICollection<IDocument> m_pendingHotLoads = new HashSet<IDocument>();

	private readonly System.Threading.Timer m_routingTimer;

	private volatile bool m_running = true;

	private readonly ISettingsService m_settingsService;

	private const int kTunerCommunicationPort = 4318;

	private const int kCommunicationPort = 4319;

	private volatile bool m_tryingToConnect;

	public bool IsHotLoading { get; private set; }

	public virtual bool Enabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			m_enabled = value;
			if (m_enabled)
			{
				m_connectionRequestQueue.Enqueue(ConnectionEventType.TryConnect);
				m_connectionThreadSignal.Set();
			}
			else if (base.Connected)
			{
				m_connectionRequestQueue.Enqueue(ConnectionEventType.TryDisconnect);
				m_connectionRequestQueue.Enqueue(ConnectionEventType.Disconnected);
				m_connectionThreadSignal.Set();
			}
		}
	}

	public bool HotLoadOnReimport { get; set; } = true;

	public virtual bool IsConnected => base.Connected;

	public event EventHandler HotLoadStarted;

	public event EventHandler HotLoadCompleted;

	protected virtual void OnHotLoadStarted()
	{
		IsHotLoading = true;
		this.HotLoadStarted?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnHotLoadCompleted()
	{
		IsHotLoading = false;
		this.HotLoadCompleted?.Invoke(this, EventArgs.Empty);
	}

	[ImportingConstructor]
	public TunerService(Form mainFrm, ISettingsService settingsService, ICivTechService civTechService)
		: base(mainFrm, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4319))
	{
		using (new ScopedStopwatch(GetType().Name + " construction took {0} seconds", delegate(string str)
		{
			Outputs.WriteLine(OutputMessageType.Info, OutputMessageVerbosity.Verbose, str);
		}))
		{
			Outputs.WriteLine(OutputMessageType.Info, "Starting up Tuner Service");
			base.RetryOnConnectionRefused = false;
			m_routingTimer = new System.Threading.Timer(RouteMessages);
			m_routingTimer.Change(-1, -1);
			m_settingsService = settingsService;
			string gameLogFolder = GetGameLogFolder(civTechService.PrimaryProject.Config.Name);
			try
			{
				m_logsFileWatcher = new FileSystemWatcher(gameLogFolder);
				m_logsFileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.CreationTime;
			}
			catch
			{
				m_logsFileWatcher = null;
			}
			m_connectionThread = new Thread(ConnectionThreadRun);
			m_connectionThread.IsBackground = true;
			m_connectionThread.Name = "Tuner connection thread";
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_disposedValue)
		{
			return;
		}
		if (disposing)
		{
			m_settingsService.Reloaded -= SettingsService_Reloaded;
			m_routingTimer.Change(-1, -1);
			m_routingTimer.Dispose();
			if (m_logsFileWatcher != null)
			{
				m_logsFileWatcher.EnableRaisingEvents = false;
				m_logsFileWatcher.Changed -= ConnectOnGameStart;
				m_logsFileWatcher.Dispose();
				m_logsFileWatcher = null;
			}
			ConnectionEventType result;
			while (m_connectionRequestQueue.TryDequeue(out result))
			{
			}
			m_connectionRequestQueue.Enqueue(ConnectionEventType.TryDisconnect);
			m_connectionThreadSignal.Set();
			Thread.Sleep(25);
			m_running = false;
			m_connectionThreadSignal.Set();
			if (m_connectionThread.IsAlive)
			{
				m_connectionThread.Join(TimeSpan.FromSeconds(10.0));
			}
			m_connectionThreadSignal.Dispose();
		}
		m_disposedValue = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public void BeginHotLoadRequest()
	{
		BugSubmitter.SilentAssert(!IsHotLoading && m_hotLoadDataCollection.Count == 0, "Starting hot-load request when there are already documents in the queue.  @assign bwhitman");
		OnHotLoadStarted();
	}

	public void AddHotLoadData(IHotLoadData hotLoadData)
	{
		BugSubmitter.SilentAssert(IsHotLoading, "Can only add hot load data when in a hot load request!  @assign bwhitman");
		m_hotLoadDataCollection.Add(hotLoadData);
	}

	public void EndHotLoadRequest()
	{
		BugSubmitter.SilentAssert(IsHotLoading, "Called EndHotLoadRequest without ever calling BeginHotLoadRequest!  @assign bwhitman");
		string text = BuildHotLoadMessage(m_hotLoadDataCollection);
		Outputs.WriteLine(OutputMessageType.Info, "Hot load requested: {0}", text);
		RequestListener listener = OnHotLoadRequestComplete;
		Request(text, listener);
		m_hotLoadDataCollection.Clear();
	}

	public virtual void RequestHotLoad(IDocument doc)
	{
		InitiateHotLoadDocumentReload(doc);
	}

	public void RequestHotLoad(string systemName, IEnumerable<string> consumerNames)
	{
		HotLoadData hotLoadData = new HotLoadData
		{
			SystemName = systemName,
			ConsumerNames = consumerNames
		};
		RequestHotLoad(new HotLoadData[1] { hotLoadData });
	}

	public void RequestHotLoad(IEnumerable<HotLoadData> hotLoadDataCollection)
	{
		string text = BuildHotLoadMessage(hotLoadDataCollection);
		Outputs.WriteLine(OutputMessageType.Info, "Hot load requested: {0}", text);
		Request(text, OnHotLoadRequestComplete);
	}

	private string BuildHotLoadMessage(IEnumerable<HotLoadData> hotLoadDataCollection)
	{
		StringBuilder stringBuilder = new StringBuilder("HOTLOAD:");
		foreach (HotLoadData item in hotLoadDataCollection)
		{
			stringBuilder.AppendFormat("{0}{1}", item.SystemName, ":");
			foreach (string consumerName in item.ConsumerNames)
			{
				stringBuilder.AppendFormat("{0}{1}", consumerName, ";");
			}
			stringBuilder.Append('#');
		}
		return stringBuilder.ToString();
	}

	private string BuildHotLoadMessage(IEnumerable<IHotLoadData> hotLoadDatas)
	{
		StringBuilder stringBuilder = new StringBuilder("HOTLOAD:");
		ISet<Uri> set = new HashSet<Uri>();
		ISet<string> set2 = new SortedSet<string>();
		ISet<string> set3 = new SortedSet<string>();
		foreach (IHotLoadData hotLoadData in hotLoadDatas)
		{
			set.UnionWith(hotLoadData.DependencyFileUris);
			set2.UnionWith(hotLoadData.RelativeArtDefPaths);
			set3.UnionWith(hotLoadData.RelativePackagePaths);
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		if (set.Any())
		{
			stringBuilder2.Clear();
			stringBuilder2.Append("DEPENDENCY:");
			foreach (Uri item in set)
			{
				string localPath = item.LocalPath;
				stringBuilder2.AppendFormat("{0}{1}", localPath, ";");
			}
			stringBuilder2.Append('#');
			stringBuilder.Append(stringBuilder2.ToString());
		}
		if (set2.Any())
		{
			stringBuilder2.Clear();
			stringBuilder2.Append("ARTDEF:");
			foreach (string item2 in set2)
			{
				stringBuilder2.AppendFormat("{0}{1}", item2, ";");
			}
			stringBuilder2.Append('#');
			stringBuilder.AppendFormat(stringBuilder2.ToString());
		}
		if (set3.Any())
		{
			stringBuilder2.Clear();
			stringBuilder2.Append("BLP:");
			foreach (string item3 in set3)
			{
				stringBuilder2.AppendFormat("{0}{1}", item3, ";");
			}
			stringBuilder2.Append('#');
			stringBuilder.AppendFormat(stringBuilder2.ToString());
		}
		return stringBuilder.ToString();
	}

	public void SendTunerCommand(string tunerCommand)
	{
		Request(tunerCommand, OnTunerRequestComplete);
	}

	public void Initialize()
	{
		RegisterSettings();
		RegisterEventHandlers();
		m_connectionThread.Start();
	}

	private void ConnectionThreadRun(object context)
	{
		while (m_running && m_connectionThreadSignal.WaitOne())
		{
			ConnectionEventType result;
			while (m_connectionRequestQueue.TryDequeue(out result))
			{
				switch (result)
				{
				case ConnectionEventType.TryConnect:
					StartConnection();
					break;
				case ConnectionEventType.TryDisconnect:
					EndConnection();
					break;
				case ConnectionEventType.Connected:
					HandleConnected();
					break;
				case ConnectionEventType.Disconnected:
					HandleDisconnect();
					break;
				default:
					BugSubmitter.SilentReport("Unknown connection request type detected!  Implement code here! @assign bwhitman");
					break;
				}
			}
		}
	}

	private void ConnectOnGameStart(object sender, FileSystemEventArgs e)
	{
		m_connectionRequestQueue.Enqueue(ConnectionEventType.TryConnect);
		m_connectionThreadSignal.Set();
	}

	private void EndConnection()
	{
		m_tryingToConnect = false;
		CloseConnection();
	}

	private string GetGameLogFolder(string projectName)
	{
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Sid Meier's Civilization VI", "Logs");
	}

	private void HandleConnected()
	{
		m_tryingToConnect = false;
		if (m_logsFileWatcher != null)
		{
			m_logsFileWatcher.EnableRaisingEvents = false;
		}
		Outputs.WriteLine(OutputMessageType.Info, "Connected to game: " + base.ConnectionTarget.Address.ToString() + ":" + base.ConnectionTarget.Port);
		m_iReadBufferOffset = 0;
		m_Message = null;
		m_iMessageIndex = 0;
		m_bPendingRecieve = false;
		m_routingTimer.Change(250, 250);
		ListenForMessages();
	}

	private void HandleDisconnect()
	{
		m_tryingToConnect = false;
		m_routingTimer.Change(-1, -1);
		if (m_logsFileWatcher != null)
		{
			m_logsFileWatcher.EnableRaisingEvents = true;
		}
		Outputs.WriteLine(OutputMessageType.Info, "Disconnected from game: " + base.ConnectionTarget.Address.ToString() + ":" + base.ConnectionTarget.Port);
	}

	private void InitiateHotLoadDocumentReload(IDocument changingDoc)
	{
		if (changingDoc is IHotLoadableDocument hotLoadableDocument)
		{
			RequestHotLoad(hotLoadableDocument.SubSystem, hotLoadableDocument.ConsumerNames);
		}
	}

	private void OnConnectionFailed(object sender, OpenConnectionFailedArgs e)
	{
		m_tryingToConnect = false;
		if (Enabled)
		{
			m_connectionRequestQueue.Enqueue(ConnectionEventType.TryConnect);
			m_connectionThreadSignal.Set();
		}
	}

	private void OnHotLoadRequestComplete(List<string> msgs)
	{
		Outputs.WriteLine(OutputMessageType.Info, "Hot load request complete.");
		OnHotLoadCompleted();
	}

	private void OnTunerRequestComplete(List<string> msgs)
	{
		foreach (string msg in msgs)
		{
			Outputs.WriteLine(OutputMessageType.Info, msg);
		}
	}

	private void RegisterEventHandlers()
	{
		m_settingsService.Reloaded += SettingsService_Reloaded;
		base.ConnectionEstablished += OnConnected;
		base.OpenConnectionFailed += OnConnectionFailed;
		base.ConnectionLost += OnConnectionLost;
		m_DefaultRequestHandler = DefaultRequestHandler;
		m_ListenForMessagesDelegate = ListenForMessages;
		if (m_logsFileWatcher != null)
		{
			m_logsFileWatcher.Changed += ConnectOnGameStart;
			m_logsFileWatcher.EnableRaisingEvents = true;
		}
	}

	private void RegisterSettings()
	{
		if (m_settingsService != null)
		{
			BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => Enabled, "Enable Tuner".Localize("Enable usage of tuner connection for reloading asset on the fly"), null, "Enable usage of tuner connection".Localize());
			BoundPropertyDescriptor boundPropertyDescriptor2 = new BoundPropertyDescriptor(this, () => HotLoadOnReimport, "Hotload On Reimport".Localize(), null, "If true, a hotload will be queued whenever an entity is reimported.".Localize());
			m_settingsService.RegisterSettings("Tuner".Localize(), boundPropertyDescriptor, boundPropertyDescriptor2);
			m_settingsService.RegisterUserSettings("Tuner".Localize(), boundPropertyDescriptor, boundPropertyDescriptor2);
		}
	}

	private void SettingsService_Reloaded(object sender, EventArgs e)
	{
		if (Enabled)
		{
			m_connectionRequestQueue.Enqueue(ConnectionEventType.TryConnect);
			m_connectionThreadSignal.Set();
		}
	}

	private void StartConnection()
	{
		if (!Enabled || m_tryingToConnect || base.Connected)
		{
			return;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location);
		m_tryingToConnect = true;
		try
		{
			OpenConnection();
		}
		catch (SocketException ex)
		{
			m_tryingToConnect = false;
			string text = "Could not open a connection to the Tuner (port is not in use).  Please restart " + fileNameWithoutExtension + " and try again.";
			MessageBox.Show(text, "Failed to connect", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			Outputs.WriteLine(OutputMessageType.Error, text);
			Outputs.WriteLine(OutputMessageType.Error, "Exception message: {0}", ex.ToString());
		}
		catch (System.Exception ex2)
		{
			m_tryingToConnect = false;
			string text2 = "Could not open a connection to the Tuner (system network resources exhausted for this application).  Please restart " + fileNameWithoutExtension + " and try again.";
			MessageBox.Show(text2, "Failed to connect", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			Outputs.WriteLine(OutputMessageType.Error, text2);
			Outputs.WriteLine(OutputMessageType.Error, "Exception message: {0}", ex2.ToString());
		}
	}

	private void OnConnected(object sender, EventArgs args)
	{
		m_connectionRequestQueue.Enqueue(ConnectionEventType.Connected);
		m_connectionThreadSignal.Set();
	}

	private void OnConnectionLost(object sender, EventArgs e)
	{
		m_connectionRequestQueue.Enqueue(ConnectionEventType.Disconnected);
		m_connectionThreadSignal.Set();
	}

	private void ListenForMessages()
	{
		if (m_RecieveArgs == null)
		{
			m_RecieveArgs = new SocketAsyncEventArgs();
			m_RecieveArgs.Completed += RecieveAsyncCompleted;
		}
		if (!m_bPendingRecieve)
		{
			m_bPendingRecieve = true;
			m_RecieveArgs.SetBuffer(m_ReadBuffer, m_iReadBufferOffset, m_ReadBuffer.Length - m_iReadBufferOffset);
			RecieveAsync(m_RecieveArgs);
		}
	}

	private void RecieveAsyncCompleted(object sender, SocketAsyncEventArgs e)
	{
		m_bPendingRecieve = false;
		if (e.SocketError == SocketError.Success)
		{
			if (e.BytesTransferred == 0)
			{
				SafeInvoke(base.OnConnectionLostDelegate);
				return;
			}
			OnRecievedData(e.Buffer, e.BytesTransferred);
			SafeInvoke(m_ListenForMessagesDelegate);
		}
	}

	private void OnRecievedData(byte[] data, int iTransfered)
	{
		int num = m_iReadBufferOffset + iTransfered;
		if (num > data.Length)
		{
			throw new System.Exception("Invalid transfered size");
		}
		m_iReadBufferOffset = 0;
		int num2 = 0;
		while (num2 < num)
		{
			if (m_Message == null)
			{
				int num3 = num - num2;
				if (num3 < 4)
				{
					for (int i = 0; i < num3; i++)
					{
						m_ReadBuffer[i] = m_ReadBuffer[num2 + i];
					}
					m_iReadBufferOffset = num3;
					break;
				}
				uint num4 = BitConverter.ToUInt32(m_ReadBuffer, num2);
				if (num4 > 524288)
				{
					SafeInvoke(base.OnConnectionLostDelegate);
				}
				uint num5 = num4 + 4;
				num2 += 4;
				m_Message = new byte[num5];
				m_iMessageIndex = 0;
			}
			else
			{
				while (m_iMessageIndex < m_Message.Length && num2 < num)
				{
					m_Message[m_iMessageIndex++] = data[num2++];
				}
				if (m_iMessageIndex == m_Message.Length)
				{
					OnMessageRecieved(m_Message);
					m_Message = null;
				}
			}
		}
	}

	private void OnMessageRecieved(byte[] message)
	{
		BinaryReader binaryReader = new BinaryReader(new MemoryStream(message));
		uint listener = binaryReader.ReadUInt32();
		char[] array = binaryReader.ReadChars(message.Length - 4);
		RequestResponse item = new RequestResponse
		{
			Listener = (int)listener,
			Messages = new List<string>()
		};
		StringBuilder stringBuilder = new StringBuilder();
		_ = string.Empty;
		char[] array2 = array;
		foreach (char c in array2)
		{
			if (c == '\0')
			{
				item.Messages.Add(stringBuilder.ToString());
				stringBuilder.Clear();
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		lock (m_ResponseLock)
		{
			m_ResponsesToProcess.Add(item);
		}
	}

	private void RouteMessages(object context)
	{
		if (m_bRoutingMessages)
		{
			return;
		}
		m_bRoutingMessages = true;
		lock (m_ResponseLock)
		{
			m_Responses.AddRange(m_ResponsesToProcess);
			m_ResponsesToProcess.Clear();
		}
		foreach (RequestResponse response in m_Responses)
		{
			GetRequestListener(response.Listener)?.Invoke(response.Messages);
		}
		m_Responses.Clear();
		m_bRoutingMessages = false;
	}

	private void DefaultRequestHandler(List<string> results)
	{
		if (results.Count <= 0)
		{
			return;
		}
		if (results[0] == "O")
		{
			results.RemoveAt(0);
			EnumerableUtil.ForEach(results, delegate(string str)
			{
				Outputs.WriteLine(OutputMessageType.Info, str);
			});
		}
		else if (results[0] == "Closing")
		{
			m_connectionRequestQueue.Enqueue(ConnectionEventType.TryDisconnect);
			m_connectionThreadSignal.Set();
		}
	}

	public int AddRequestListener(RequestListener listener)
	{
		int num = m_RequestListeners.IndexOf(listener);
		if (num < 0)
		{
			num = m_RequestListeners.Count;
			m_RequestListeners.Add(listener);
		}
		return num;
	}

	private RequestListener GetRequestListener(int iIndex)
	{
		if (iIndex < m_RequestListeners.Count && iIndex >= 0)
		{
			return m_RequestListeners[iIndex];
		}
		return m_DefaultRequestHandler;
	}

	public bool Request(string sMessage, RequestListener listener)
	{
		return Request(sMessage, AddRequestListener(listener));
	}

	public bool Request(string s, int iSender)
	{
		if (s == null)
		{
			return false;
		}
		bool result = false;
		try
		{
			uint num = (uint)(s.Length + 1);
			MemoryStream memoryStream = new MemoryStream((int)(num + 8));
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(num);
			binaryWriter.Write((uint)iSender);
			foreach (char ch in s)
			{
				binaryWriter.Write(ch);
			}
			binaryWriter.Write('\0');
			byte[] buffer = memoryStream.GetBuffer();
			int num2 = (int)memoryStream.Position;
			uint num3 = (uint)((ulong)num2 - 8uL);
			if (num != num3)
			{
				memoryStream.Position = 0L;
				binaryWriter.Write(num3);
			}
			SocketAsyncEventArgs e = new SocketAsyncEventArgs();
			e.Completed += SendAsyncCompleted;
			e.SetBuffer(buffer, 0, num2);
			result = SendAsync(e);
		}
		catch (System.Exception exception)
		{
			ErrorHandling.Error(exception, ErrorLevel.SendReport);
		}
		return result;
	}

	private void SendAsyncCompleted(object sender, SocketAsyncEventArgs e)
	{
		if (e.SocketError != SocketError.Success)
		{
			SafeInvoke(base.OnConnectionLostDelegate);
		}
	}
}
