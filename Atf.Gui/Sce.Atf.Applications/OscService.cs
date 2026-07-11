using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using Bespoke.Common;
using Bespoke.Common.Osc;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications.NetworkTargetServices;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(IOscService))]
[Export(typeof(OscService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class OscService : IOscService, IInitializable
{
	private class ValueSettingInfo
	{
		public readonly string OscAddress;

		public readonly object Addressable;

		public readonly object ConvertedData;

		public readonly OscAddressInfo OscAddressInfo;

		public ValueSettingInfo(string oscAddress, object addressable, object convertedData, OscAddressInfo oscAddressInfo)
		{
			OscAddress = oscAddress;
			Addressable = addressable;
			ConvertedData = convertedData;
			OscAddressInfo = oscAddressInfo;
		}
	}

	public class OscAddressInfo
	{
		public readonly string Address;

		public readonly Type CompatibleType;

		public readonly Type PropertyType;

		internal readonly List<object> Addressable = new List<object>();

		private readonly System.ComponentModel.PropertyDescriptor m_descriptor;

		private readonly PropertyInfo m_propertyInfo;

		public string PropertyName => (m_descriptor != null) ? m_descriptor.Name : m_propertyInfo.Name;

		public System.ComponentModel.PropertyDescriptor PropertyDescriptor => m_descriptor;

		internal OscAddressInfo(string oscAddress, PropertyInfo propertyInfo)
		{
			Address = oscAddress;
			m_propertyInfo = propertyInfo;
			CompatibleType = propertyInfo.DeclaringType;
			PropertyType = propertyInfo.PropertyType;
		}

		internal OscAddressInfo(string oscAddress, System.ComponentModel.PropertyDescriptor descriptor)
		{
			Address = oscAddress;
			m_descriptor = descriptor;
			CompatibleType = descriptor.ComponentType;
			PropertyType = descriptor.PropertyType;
		}

		internal object CommonToAddressable(object common)
		{
			return common.As(CompatibleType);
		}

		internal object GetValue(object compatible)
		{
			if (m_descriptor != null)
			{
				return m_descriptor.GetValue(compatible);
			}
			return m_propertyInfo.GetValue(compatible, null);
		}

		internal void SetValue(object compatible, object data)
		{
			if (m_descriptor != null)
			{
				PropertyUtils.SetProperty(compatible, m_descriptor, data);
			}
			else
			{
				m_propertyInfo.SetValue(compatible, data, null);
			}
		}
	}

	private class EndPointConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string endPoint)
			{
				IPEndPoint iPEndPoint = TcpIpTargetInfo.TryParseIPEndPoint(endPoint);
				if (iPEndPoint != null)
				{
					return iPEndPoint;
				}
				throw new NotSupportedException("The format for an IP end point is like \"192.168.0.120:9000\"".Localize());
			}
			return base.ConvertFrom(context, culture, value);
		}
	}

	internal long NumMessagesEverReceived;

	internal long NumMessagesEverSent;

	private bool m_dirtySelectionInfo;

	private readonly Multimap<object, Tuple<OscAddressInfo, object>> m_commonToInfo = new Multimap<object, Tuple<OscAddressInfo, object>>();

	private readonly Dictionary<string, OscAddressInfo> m_oscAddressToInfo = new Dictionary<string, OscAddressInfo>();

	private readonly Dictionary<string, OscAddressInfo> m_wildCards = new Dictionary<string, OscAddressInfo>();

	private bool m_domCacheHasBeenQueried;

	private readonly Dictionary<DomNodeType, List<OscAddressInfo>> m_domTypeToInfo = new Dictionary<DomNodeType, List<OscAddressInfo>>();

	private readonly HashSet<IPEndPoint> m_inputDevices = new HashSet<IPEndPoint>();

	private readonly HashSet<IPEndPoint> m_autoInputDevices = new HashSet<IPEndPoint>();

	private OscServer m_udpServer;

	private IPEndPoint m_receivingEndPoint = new IPEndPoint(IPAddress.Loopback, 8000);

	private IPEndPoint m_destinationEndPoint = new IPEndPoint(IPAddress.None, 8000);

	private bool m_consumingMsg;

	private readonly UniqueNamer m_namer = new UniqueNamer();

	private Thread m_sendingThread;

	private readonly AutoResetEvent m_outgoingDataAvailableEvent = new AutoResetEvent(initialState: false);

	private string m_serverStatusMsg = "Not yet initialized";

	private readonly Dictionary<string, OscMessage> m_incomingQueue = new Dictionary<string, OscMessage>();

	private readonly Dictionary<string, Tuple<string, object>> m_outgoingQueue = new Dictionary<string, Tuple<string, object>>();

	private List<EventHandler<OscMessageReceivedArgs>> m_messageReceivedDelegates = new List<EventHandler<OscMessageReceivedArgs>>();

	private IContextRegistry m_contextRegistry;

	private IObservableContext m_observableContext;

	private ISelectionContext m_selectionContext;

	private ITransactionContext m_transactionContext;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService = null;

	public int ReceivingPort
	{
		get
		{
			return m_receivingEndPoint.Port;
		}
		set
		{
			if (value != m_receivingEndPoint.Port)
			{
				if (value < 0 || value > 65535)
				{
					throw new ArgumentOutOfRangeException("ReceivingPort", value, "The IP port number must be between 0 and 65535");
				}
				m_receivingEndPoint.Port = value;
				InitializeServer();
			}
		}
	}

	public IPAddress ReceivingIPAddress
	{
		get
		{
			return m_receivingEndPoint.Address;
		}
		set
		{
			if (!value.Equals(m_receivingEndPoint.Address))
			{
				m_receivingEndPoint.Address = value;
				InitializeServer();
			}
		}
	}

	public IPAddress PreferredReceivingIPAddress
	{
		get
		{
			return ReceivingIPAddress;
		}
		set
		{
			foreach (IPAddress localIPAddress in GetLocalIPAddresses())
			{
				if (localIPAddress.Equals(value))
				{
					ReceivingIPAddress = value;
					break;
				}
			}
		}
	}

	public IPEndPoint ReceivingEndpoint
	{
		get
		{
			return m_receivingEndPoint;
		}
		set
		{
			if (!value.Equals(m_receivingEndPoint))
			{
				m_receivingEndPoint = value;
				InitializeServer();
			}
		}
	}

	public IPEndPoint DestinationEndpoint
	{
		get
		{
			return m_destinationEndPoint;
		}
		set
		{
			m_destinationEndPoint = value;
			m_inputDevices.Clear();
			if (!value.Address.Equals(IPAddress.None))
			{
				m_inputDevices.Add(value);
			}
			foreach (IPEndPoint autoInputDevice in m_autoInputDevices)
			{
				m_inputDevices.Add(autoInputDevice);
			}
		}
	}

	public IEnumerable<IPEndPoint> DestinationEndpoints => m_inputDevices;

	[Import(AllowDefault = true)]
	public IContextRegistry ContextRegistry
	{
		get
		{
			return m_contextRegistry;
		}
		set
		{
			if (IsInitialized)
			{
				throw new InvalidOperationException("OscService is already initialized");
			}
			m_contextRegistry = value;
		}
	}

	[Import(AllowDefault = true)]
	public ISynchronizeInvoke MainForm { get; set; }

	public IObservableContext ObservableContext
	{
		get
		{
			return m_observableContext;
		}
		set
		{
			if (m_observableContext != value)
			{
				if (m_observableContext != null)
				{
					m_observableContext.ItemChanged -= m_observableContext_ItemChanged;
					m_observableContext.ItemInserted -= m_observableContext_ItemInserted;
					m_observableContext.ItemRemoved -= m_observableContext_ItemRemoved;
				}
				m_observableContext = value;
				if (m_observableContext != null)
				{
					m_observableContext.ItemChanged += m_observableContext_ItemChanged;
					m_observableContext.ItemInserted += m_observableContext_ItemInserted;
					m_observableContext.ItemRemoved += m_observableContext_ItemRemoved;
				}
				ClearAddressableCache();
			}
		}
	}

	public ISelectionContext SelectionContext
	{
		get
		{
			return m_selectionContext;
		}
		set
		{
			if (m_selectionContext != value)
			{
				if (m_selectionContext != null)
				{
					m_selectionContext.SelectionChanged -= m_selectionContext_SelectionChanged;
				}
				m_selectionContext = value;
				if (m_selectionContext != null)
				{
					m_selectionContext.SelectionChanged += m_selectionContext_SelectionChanged;
				}
				ClearAddressableCache();
			}
		}
	}

	public bool IsInitialized => m_udpServer != null;

	public string StatusMessage
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(m_serverStatusMsg);
			stringBuilder.AppendLine("IP addresses to broadcast to: " + m_inputDevices.Count);
			stringBuilder.AppendLine("Number of OSC messages received: " + NumMessagesEverReceived);
			stringBuilder.AppendLine("Number of OSC messages sent: " + NumMessagesEverSent);
			return stringBuilder.ToString();
		}
	}

	public IEnumerable<OscAddressInfo> AddressInfos => m_oscAddressToInfo.Values;

	private bool CanUpdateConnectedDevices
	{
		get
		{
			if (m_inputDevices.Count == 0)
			{
				return false;
			}
			if (m_consumingMsg)
			{
				return false;
			}
			return true;
		}
	}

	public event EventHandler<OscMessageReceivedArgs> MessageReceived
	{
		add
		{
			m_messageReceivedDelegates.Add(value);
		}
		remove
		{
			m_messageReceivedDelegates.Remove(value);
		}
	}

	public virtual void Initialize()
	{
		InitializeServer();
		m_receivingEndPoint.Address = GetLocalIPAddresses().First();
		if (m_contextRegistry != null)
		{
			m_contextRegistry.ActiveContextChanged += m_contextRegistry_ActiveContextChanged;
		}
		if (m_settingsService != null)
		{
			System.ComponentModel.PropertyDescriptor[] properties = new System.ComponentModel.PropertyDescriptor[3]
			{
				new BoundPropertyDescriptor(this, () => ReceivingPort, "OSC Receiving Port Number".Localize(), null, "The IP Port number that this app listens to for receiving Open Sound Control messages".Localize()),
				new BoundPropertyDescriptor(this, () => DestinationEndpoint, "Primary Destination IP Endpoint".Localize(), null, "The primary IP address and port number that this app sends Open Sound Control messages to. Additional destinations can be added due to auto-configuration".Localize(), null, new EndPointConverter()),
				new BoundPropertyDescriptor(this, () => PreferredReceivingIPAddress, "Preferred Receiving IP Address".Localize(), null, "The preferred IP address that this app listens to for receiving Open Sound Control messages".Localize())
			};
			m_settingsService.RegisterUserSettings("Open Sound Control", properties);
			m_settingsService.RegisterSettings(this, properties);
		}
		m_sendingThread = new Thread(SendingThreadStart);
		m_sendingThread.Name = "OSC sending thread";
		m_sendingThread.IsBackground = true;
		m_sendingThread.SetApartmentState(ApartmentState.STA);
		m_sendingThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
		m_sendingThread.Start();
	}

	public static IEnumerable<IPAddress> GetLocalIPAddresses()
	{
		IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
		IPAddress[] addressList = host.AddressList;
		foreach (IPAddress ip in addressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				yield return ip;
			}
		}
		yield return IPAddress.Loopback;
	}

	public string AddPropertyAddress(PropertyInfo propertyInfo, string oscAddress)
	{
		oscAddress = OscServices.FixPropertyAddress(oscAddress);
		oscAddress = m_namer.Name(oscAddress);
		OscAddressInfo info = new OscAddressInfo(oscAddress, propertyInfo);
		AddOscAddress(oscAddress, info);
		return oscAddress;
	}

	public string AddPropertyAddress(Type classType, string propertyName, string oscAddress)
	{
		return AddPropertyAddress(classType.GetProperty(propertyName), oscAddress);
	}

	public string AddPropertyAddress(string typeName, string propertyName, string oscAddress)
	{
		Type typeFromString = GetTypeFromString(typeName);
		if (typeFromString == null)
		{
			return oscAddress;
		}
		PropertyInfo property = typeFromString.GetProperty(propertyName);
		if (property == null)
		{
			throw new InvalidOperationException(string.Concat("OscService was called with bad data. '", typeFromString, "' does not contain property ", propertyName));
		}
		return AddPropertyAddress(property, oscAddress);
	}

	public string AddPropertyAddress(System.ComponentModel.PropertyDescriptor descriptor, string oscAddress)
	{
		oscAddress = OscServices.FixPropertyAddress(oscAddress);
		oscAddress = m_namer.Name(oscAddress);
		OscAddressInfo info = new OscAddressInfo(oscAddress, descriptor);
		AddOscAddress(oscAddress, info);
		if (descriptor is AttributePropertyDescriptor attributePropertyDescriptor)
		{
			DomNodeType owningDomNodeType = ((!(attributePropertyDescriptor is ChildAttributePropertyDescriptor childAttributePropertyDescriptor)) ? attributePropertyDescriptor.AttributeInfo.OwningType : childAttributePropertyDescriptor.Path.First().OwningType);
			AddToDomTypeCache(owningDomNodeType, info);
		}
		return oscAddress;
	}

	public IEnumerable<OscAddressInfo> GetInfos(object selected)
	{
		object common = SelectedToCommon(selected);
		if (common == null)
		{
			yield break;
		}
		foreach (Tuple<OscAddressInfo, object> infoAddressablePair in GetAddressables(selected))
		{
			yield return infoAddressablePair.Item1;
		}
	}

	public void Send(IEnumerable<Tuple<string, object>> addressesAndData)
	{
		lock (m_outgoingQueue)
		{
			foreach (Tuple<string, object> addressesAndDatum in addressesAndData)
			{
				m_outgoingQueue[addressesAndDatum.Item1] = addressesAndDatum;
			}
		}
		m_outgoingDataAvailableEvent.Set();
	}

	protected virtual object ObservableToCommon(object observable)
	{
		return observable;
	}

	protected virtual object SelectedToCommon(object selected)
	{
		return selected;
	}

	protected virtual IEnumerable<Tuple<string, object>> GetCustomDataToSend(object common)
	{
		yield break;
	}

	protected virtual object PrepareDataForSending(object data, object common, OscAddressInfo info)
	{
		return data;
	}

	protected virtual void SendSynchronously(IList<Tuple<string, object>> addressesAndData)
	{
		SendPacket(addressesAndData, 0, addressesAndData.Count);
	}

	protected virtual void OnMessageRecieved(OscMessageReceivedArgs args)
	{
		foreach (EventHandler<OscMessageReceivedArgs> messageReceivedDelegate in m_messageReceivedDelegates)
		{
			if (args.Handled)
			{
				break;
			}
			messageReceivedDelegate(this, args);
		}
	}

	protected void SendPacket(IList<Tuple<string, object>> addressesAndData, int first, int count)
	{
		if (addressesAndData.Count < first + count)
		{
			return;
		}
		OscPacket oscPacket;
		if (count == 1)
		{
			oscPacket = new OscMessage(ReceivingEndpoint, addressesAndData[first].Item1, addressesAndData[first].Item2);
		}
		else
		{
			OscBundle oscBundle = new OscBundle(ReceivingEndpoint);
			for (int i = first; i < first + count; i++)
			{
				Tuple<string, object> tuple = addressesAndData[i];
				OscMessage value = new OscMessage(ReceivingEndpoint, tuple.Item1, tuple.Item2);
				oscBundle.Append(value);
			}
			oscPacket = oscBundle;
		}
		foreach (IPEndPoint destinationEndpoint in DestinationEndpoints)
		{
			oscPacket.Send(destinationEndpoint);
			NumMessagesEverSent++;
		}
	}

	private void SendingThreadStart()
	{
		while (m_outgoingDataAvailableEvent.WaitOne())
		{
			List<Tuple<string, object>> addressesAndData;
			lock (m_outgoingQueue)
			{
				addressesAndData = new List<Tuple<string, object>>(m_outgoingQueue.Values);
				m_outgoingQueue.Clear();
			}
			SendSynchronously(addressesAndData);
		}
	}

	private void m_oscUdpServer_MessageReceived(object sender, OscMessageReceivedEventArgs e)
	{
		NumMessagesEverReceived++;
		bool flag = false;
		lock (m_incomingQueue)
		{
			int count = m_incomingQueue.Count;
			m_incomingQueue[e.Message.Address] = e.Message;
			if (m_incomingQueue.Count > count)
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (MainForm != null)
			{
				MainForm.BeginInvoke(new Action(ConsumeMessages), null);
			}
			else
			{
				ConsumeMessages();
			}
		}
	}

	private void ConsumeMessages()
	{
		List<OscMessage> list;
		lock (m_incomingQueue)
		{
			list = new List<OscMessage>(m_incomingQueue.Values);
			m_incomingQueue.Clear();
		}
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			OscMessage oscMessage = list[i];
			m_inputDevices.Add(oscMessage.SourceEndPoint);
			m_autoInputDevices.Add(oscMessage.SourceEndPoint);
			OscMessageReceivedArgs oscMessageReceivedArgs = new OscMessageReceivedArgs(oscMessage.Address, oscMessage.Data);
			OnMessageRecieved(oscMessageReceivedArgs);
			if (oscMessageReceivedArgs.Handled)
			{
				num++;
				list[i] = null;
			}
		}
		if (num == list.Count)
		{
			return;
		}
		CheckAddressableCache();
		m_consumingMsg = true;
		try
		{
			List<ValueSettingInfo> valuesToSet = new List<ValueSettingInfo>();
			foreach (OscMessage item in list)
			{
				if (item == null || item.Data.Count <= 0)
				{
					continue;
				}
				foreach (OscAddressInfo matchingInfo in GetMatchingInfos(item.Address))
				{
					foreach (object item2 in matchingInfo.Addressable)
					{
						object obj = ConvertOscData(item.Data, matchingInfo.PropertyType);
						if (obj != null)
						{
							valuesToSet.Add(new ValueSettingInfo(item.Address, item2, obj, matchingInfo));
						}
					}
				}
			}
			if (valuesToSet.Count <= 0)
			{
				return;
			}
			m_transactionContext.DoTransaction(delegate
			{
				foreach (ValueSettingInfo item3 in valuesToSet)
				{
					SetValue(item3.OscAddress, item3.Addressable, item3.ConvertedData, item3.OscAddressInfo);
				}
			}, "OSC Input".Localize("The name of a command"));
		}
		finally
		{
			m_consumingMsg = false;
		}
	}

	private void AddOscAddress(string oscAddress, OscAddressInfo info)
	{
		m_oscAddressToInfo[oscAddress] = info;
		if (oscAddress.EndsWith("*"))
		{
			m_wildCards[oscAddress] = info;
		}
	}

	private IEnumerable<OscAddressInfo> GetMatchingInfos(string oscAddress)
	{
		if (m_oscAddressToInfo.TryGetValue(oscAddress, out var info))
		{
			yield return info;
		}
		foreach (KeyValuePair<string, OscAddressInfo> pair in m_wildCards)
		{
			string ourOscAddress = pair.Key;
			if (string.Compare(ourOscAddress, 0, oscAddress, 0, ourOscAddress.Length - 1) == 0)
			{
				yield return pair.Value;
			}
		}
	}

	private object ConvertOscData(IList<object> messageData, Type propertyType)
	{
		object obj = messageData[0];
		if (propertyType == typeof(float[]) && messageData[0] is float)
		{
			float[] array = new float[messageData.Count];
			for (int i = 0; i < messageData.Count; i++)
			{
				array[i] = (float)messageData[i];
			}
			obj = array;
		}
		Type type = obj.GetType();
		if (propertyType.IsAssignableFrom(type))
		{
			return obj;
		}
		if (propertyType.IsAssignableFrom(typeof(bool)))
		{
			if (type == typeof(float))
			{
				return (float)obj == 1f;
			}
		}
		else if (propertyType.IsAssignableFrom(typeof(int)) && type == typeof(float))
		{
			return (int)Math.Round((float)obj);
		}
		return null;
	}

	protected virtual void SetValue(string oscAddress, object addressable, object value, OscAddressInfo info)
	{
		info.SetValue(addressable, value);
	}

	private void m_observableContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		ObservableContextChanged(e.Item);
	}

	private void m_observableContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		ClearAddressableCache();
		ObservableContextChanged(e.Item);
	}

	private void m_observableContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		ObservableContextChanged(e.Item);
	}

	private void ObservableContextChanged(object observable)
	{
		if (CanUpdateConnectedDevices)
		{
			object obj = ObservableToCommon(observable);
			if (obj != null)
			{
				UpdateConnectedDevices(obj);
			}
		}
	}

	private void UpdateConnectedDevices(object common)
	{
		if (common == null)
		{
			return;
		}
		if (m_selectionContext != null)
		{
			object obj = SelectedToCommon(m_selectionContext.LastSelected);
			if (obj != common)
			{
				return;
			}
		}
		List<Tuple<string, object>> list = GetCustomDataToSend(common).ToList();
		CheckAddressableCache();
		foreach (Tuple<OscAddressInfo, object> item3 in m_commonToInfo[common])
		{
			OscAddressInfo item = item3.Item1;
			object item2 = item3.Item2;
			object value = item.GetValue(item2);
			value = PrepareDataForSending(value, common, item);
			if (value != null)
			{
				list.Add(new Tuple<string, object>(item.Address, value));
			}
		}
		if (list.Count > 0)
		{
			Send(list);
		}
	}

	private void ClearAddressableCache()
	{
		m_dirtySelectionInfo = true;
	}

	private void CheckAddressableCache()
	{
		if (!m_dirtySelectionInfo)
		{
			return;
		}
		m_dirtySelectionInfo = false;
		if (m_consumingMsg)
		{
			throw new InvalidOperationException("CheckAddressableCache() is being called at a bad time.OscAddressInfo.Addressable might be modified while it's being enumerated.");
		}
		m_commonToInfo.Clear();
		foreach (OscAddressInfo value in m_oscAddressToInfo.Values)
		{
			value.Addressable.Clear();
		}
		if (m_selectionContext == null)
		{
			return;
		}
		foreach (object item in m_selectionContext.Selection)
		{
			object obj = SelectedToCommon(item);
			if (obj == null)
			{
				continue;
			}
			foreach (Tuple<OscAddressInfo, object> addressable in GetAddressables(item))
			{
				addressable.Item1.Addressable.Add(addressable.Item2);
				m_commonToInfo.Add(obj, addressable);
			}
		}
	}

	private IEnumerable<Tuple<OscAddressInfo, object>> GetAddressables(object common)
	{
		DomNode domNode = common.As<DomNode>();
		if (domNode != null)
		{
			IEnumerable<OscAddressInfo> infos = QueryDomTypeCache(domNode.Type);
			foreach (OscAddressInfo info in infos)
			{
				yield return new Tuple<OscAddressInfo, object>(info, domNode);
			}
			yield break;
		}
		foreach (OscAddressInfo info2 in m_oscAddressToInfo.Values)
		{
			object addressable = info2.CommonToAddressable(common);
			if (addressable != null)
			{
				yield return new Tuple<OscAddressInfo, object>(info2, addressable);
			}
		}
	}

	private IEnumerable<OscAddressInfo> QueryDomTypeCache(DomNodeType domNodeType)
	{
		if (!m_domTypeToInfo.TryGetValue(domNodeType, out var value))
		{
			value = new List<OscAddressInfo>();
			for (DomNodeType baseType = domNodeType.BaseType; baseType != null; baseType = baseType.BaseType)
			{
				if (m_domTypeToInfo.TryGetValue(baseType, out var value2))
				{
					value.AddRange(value2);
					break;
				}
			}
			m_domCacheHasBeenQueried = true;
			m_domTypeToInfo[domNodeType] = value;
		}
		return value;
	}

	private void AddToDomTypeCache(DomNodeType owningDomNodeType, OscAddressInfo info)
	{
		if (m_domCacheHasBeenQueried)
		{
			throw new InvalidOperationException("New OSC addresses can't currently be added after start-up");
		}
		if (!m_domTypeToInfo.TryGetValue(owningDomNodeType, out var value))
		{
			value = new List<OscAddressInfo>();
			m_domTypeToInfo[owningDomNodeType] = value;
		}
		value.Add(info);
	}

	private void oscServer_ReceiveErrored(object sender, ExceptionEventArgs e)
	{
		Outputs.WriteLine(OutputMessageType.Warning, "OscService received an error: " + e.Exception.ToString());
	}

	private void m_selectionContext_SelectionChanged(object sender, EventArgs e)
	{
		ClearAddressableCache();
		if (CanUpdateConnectedDevices)
		{
			CheckAddressableCache();
			object common = SelectedToCommon(m_selectionContext.LastSelected);
			UpdateConnectedDevices(common);
		}
	}

	private void m_contextRegistry_ActiveContextChanged(object sender, EventArgs e)
	{
		ObservableContext = m_contextRegistry.GetActiveContext<IObservableContext>();
		SelectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
		m_transactionContext = m_contextRegistry.GetActiveContext<ITransactionContext>();
	}

	private static Type GetTypeFromString(string typeString)
	{
		if (string.IsNullOrEmpty(typeString))
		{
			return null;
		}
		Type type = null;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			type = assembly.GetType(typeString);
			if (type != null)
			{
				break;
			}
		}
		return type;
	}

	private void InitializeServer()
	{
		if (m_udpServer != null)
		{
			m_udpServer.ReceiveErrored -= oscServer_ReceiveErrored;
			m_udpServer.MessageReceived -= m_oscUdpServer_MessageReceived;
			m_udpServer.Stop();
			m_udpServer = null;
		}
		m_udpServer = new OscServer(TransportType.Udp, IPAddress.Any, ReceivingPort);
		m_udpServer.FilterRegisteredMethods = false;
		m_udpServer.MessageReceived += m_oscUdpServer_MessageReceived;
		m_udpServer.ReceiveErrored += oscServer_ReceiveErrored;
		m_udpServer.ConsumeParsingExceptions = false;
		try
		{
			m_udpServer.Start();
			m_serverStatusMsg = "Running";
		}
		catch (SocketException ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, ex.Message);
			m_serverStatusMsg = "Not running. Could be two apps using the same port #. Exception text: \"" + ex.Message + "\"";
		}
	}
}
