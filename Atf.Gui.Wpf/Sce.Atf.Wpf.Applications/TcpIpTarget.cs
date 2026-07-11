using System;
using System.ComponentModel;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications;

[Serializable]
public class TcpIpTarget : NotifyPropertyChangedBase, ITarget, IEquatable<ITarget>, IXmlSerializable
{
	private static readonly PropertyChangedEventArgs s_nameArgs = ObservableUtil.CreateArgs((TcpIpTarget x) => x.Name);

	private static readonly PropertyChangedEventArgs s_hostArgs = ObservableUtil.CreateArgs((TcpIpTarget x) => x.Host);

	private static readonly PropertyChangedEventArgs s_portArgs = ObservableUtil.CreateArgs((TcpIpTarget x) => x.Port);

	private static readonly PropertyChangedEventArgs s_ipAddressArgs = ObservableUtil.CreateArgs((TcpIpTarget x) => x.IpAddress);

	private string m_name;

	private uint m_port;

	private IPAddress m_ipAddress;

	[NonSerialized]
	private bool m_isConnected;

	public uint Port
	{
		get
		{
			return m_port;
		}
		set
		{
			if (value > 65535)
			{
				throw new ArgumentOutOfRangeException("Enter a port between 0 and 65535".Localize());
			}
			m_port = value;
			OnPropertyChanged(s_portArgs);
			UpdateHost();
		}
	}

	public string IpAddress
	{
		get
		{
			return m_ipAddress.ToString();
		}
		set
		{
			IPAddress address = null;
			if (!IPAddress.TryParse(value, out address))
			{
				throw new ArgumentException("Invalid Host");
			}
			m_ipAddress = address;
			OnPropertyChanged(s_ipAddressArgs);
			UpdateHost();
		}
	}

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
			OnPropertyChanged(s_nameArgs);
		}
	}

	public string Host { get; protected set; }

	public string HardwareId { get; protected set; }

	public bool IsConnected
	{
		get
		{
			return m_isConnected;
		}
		internal set
		{
			m_isConnected = value;
		}
	}

	public string ConnectionInfo { get; protected set; }

	public string Status { get; protected set; }

	public string ProtocolId { get; protected set; }

	public string ProtocolName { get; protected set; }

	public TcpIpTarget()
	{
	}

	public TcpIpTarget(string name, string protocol, string protocolName, string ip, uint port)
	{
		Name = name;
		ProtocolId = protocol;
		ProtocolName = protocolName;
		IpAddress = ip;
		Port = port;
	}

	public bool Equals(ITarget other)
	{
		return Host == other.Host && HardwareId == other.HardwareId;
	}

	public override bool Equals(object obj)
	{
		if (obj is ITarget other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 0;
		if (Host != null)
		{
			num ^= Host.GetHashCode();
		}
		if (HardwareId != null)
		{
			num ^= HardwareId.GetHashCode();
		}
		return num;
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		Name = reader.GetAttribute("name");
		ProtocolId = reader.GetAttribute("protocol");
		ProtocolName = reader.GetAttribute("protocolname");
		IpAddress = reader.GetAttribute("ip");
		Port = uint.Parse(reader.GetAttribute("port"));
	}

	public void WriteXml(XmlWriter writer)
	{
		writer.WriteAttributeString("name", Name);
		writer.WriteAttributeString("ip", IpAddress);
		writer.WriteAttributeString("port", Port.ToString());
		writer.WriteAttributeString("protocol", ProtocolId);
		writer.WriteAttributeString("protocolname", ProtocolName);
	}

	public override string ToString()
	{
		return Name;
	}

	private void UpdateHost()
	{
		Host = IpAddress + " : " + Port;
		OnPropertyChanged(s_hostArgs);
	}
}
