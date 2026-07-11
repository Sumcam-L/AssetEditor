using System;
using System.Net;

namespace Sce.Atf.Applications.NetworkTargetServices;

public class Target : ICloneable
{
	private int m_port = -1;

	private string m_name;

	private string m_host;

	private object m_tag;

	private string m_protocol = "none";

	private bool m_selected;

	private bool m_connected;

	public string Name => m_name;

	public bool Selected
	{
		get
		{
			return m_selected;
		}
		set
		{
			m_selected = value;
		}
	}

	public bool IsConnected
	{
		get
		{
			return m_connected;
		}
		set
		{
			m_connected = value;
		}
	}

	public string Protocol
	{
		get
		{
			return m_protocol;
		}
		set
		{
			m_protocol = value;
		}
	}

	public object Tag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
		}
	}

	public string Host => m_host;

	public IPAddress IPAddress
	{
		get
		{
			IPAddress address = null;
			if (!IPAddress.TryParse(m_host, out address))
			{
				IPHostEntry hostEntry = Dns.GetHostEntry(m_host);
				IPAddress[] addressList = hostEntry.AddressList;
				int num = 0;
				if (num < addressList.Length)
				{
					IPAddress iPAddress = addressList[num];
					address = iPAddress;
				}
			}
			return address;
		}
	}

	public int Port => m_port;

	public Target(string name, string host, int port)
	{
		Set(name, host, port);
	}

	internal void Set(string name, string host, int port)
	{
		if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(host))
		{
			throw new ArgumentNullException();
		}
		if (port < 0 || port > 65535)
		{
			throw new ArgumentOutOfRangeException();
		}
		m_name = name;
		m_host = host;
		m_port = port;
	}

	public override string ToString()
	{
		return $"{m_name} ({m_host} : {m_port})";
	}

	public object Clone()
	{
		Target target = new Target(m_name, m_host, m_port);
		target.m_connected = m_connected;
		target.m_selected = m_selected;
		target.m_tag = m_tag;
		target.m_protocol = m_protocol;
		return target;
	}
}
