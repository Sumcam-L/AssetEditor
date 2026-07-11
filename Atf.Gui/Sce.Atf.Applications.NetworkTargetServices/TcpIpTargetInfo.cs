using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Sce.Atf.Applications.NetworkTargetServices;

[Group("TcpIpTargetInfo", Header = "TCP/IP Targets", ReadOnlyProperties = "Protocol")]
public class TcpIpTargetInfo : TargetInfo, IPropertyValueValidator
{
	public const string ProtocolName = "Tcp";

	public int FixedPort { get; set; }

	public IPEndPoint IPEndPoint
	{
		get
		{
			if (FixedPort > 0)
			{
				return TryParseEndPointUsingPort(base.Endpoint, FixedPort);
			}
			return TryParseIPEndPoint(base.Endpoint);
		}
		set
		{
			if (value != null)
			{
				if (FixedPort > 0 && FixedPort != value.Port)
				{
					throw new InvalidDataException("The port number does not match expected fixed value " + FixedPort);
				}
				base.Endpoint = value.Address.ToString() + ":" + value.Port;
			}
		}
	}

	public TcpIpTargetInfo()
	{
		base.Name = "LocalHost";
		base.Platform = "<undefined>";
		base.Endpoint = "127.0.0.1:12345";
		base.Protocol = "Tcp";
		base.Scope = TargetScope.PerApp;
	}

	public static IPEndPoint TryParseIPEndPoint(string endPoint)
	{
		string[] array = endPoint.Split(':');
		if (!int.TryParse(array[array.Length - 1], out var result))
		{
			return null;
		}
		if (result < 0 || result > 65535)
		{
			return null;
		}
		IPAddress iPAddress = TryParseIPAddress(endPoint);
		if (iPAddress != null)
		{
			return new IPEndPoint(iPAddress, result);
		}
		return null;
	}

	public static IPEndPoint TryParseEndPointUsingPort(string endPoint, int port)
	{
		if (port < 0 || port > 65535)
		{
			return null;
		}
		IPAddress iPAddress = TryParseIPAddress(endPoint);
		if (iPAddress != null)
		{
			return new IPEndPoint(iPAddress, port);
		}
		return null;
	}

	private static IPAddress TryParseIPAddress(string ipAddress)
	{
		string[] array = ipAddress.Split(':');
		IPAddress address;
		if (array.Length > 2)
		{
			if (!IPAddress.TryParse(string.Join(":", array, 0, array.Length - 1), out address))
			{
				return null;
			}
		}
		else if (!IPAddress.TryParse(array[0], out address))
		{
			try
			{
				address = Dns.GetHostEntry(array[0]).AddressList.FirstOrDefault((IPAddress x) => x.AddressFamily == AddressFamily.InterNetwork || x.AddressFamily == AddressFamily.InterNetworkV6);
				if (address == null)
				{
					return null;
				}
			}
			catch (Exception)
			{
				return null;
			}
		}
		return address;
	}

	public virtual bool Validate(string propertyName, object formattedValue, out string errorMessage)
	{
		errorMessage = string.Empty;
		bool flag;
		if (propertyName.Equals("Endpoint"))
		{
			flag = ((FixedPort <= 0) ? (TryParseIPEndPoint(formattedValue.ToString()) != null) : (TryParseEndPointUsingPort(formattedValue.ToString(), FixedPort) != null));
			if (!flag)
			{
				errorMessage = "The IP address format should be like \"192.168.0.1:12345\" or \"2001:740:8deb:0::1:12345\"".Localize();
			}
			return flag;
		}
		if (propertyName.Equals("Scope"))
		{
			flag = Enum.IsDefined(typeof(TargetScope), formattedValue.ToString());
			if (!flag)
			{
				errorMessage = "The scope type is unknown".Localize();
			}
		}
		else if (propertyName.Equals("Name"))
		{
			flag = !StringUtil.IsNullOrEmptyOrWhitespace(formattedValue.ToString());
			if (!flag)
			{
				errorMessage = "The name must not be empty or all whitespace".Localize();
			}
		}
		else
		{
			flag = false;
			errorMessage = "The property name is unknown: ".Localize() + propertyName;
		}
		return flag;
	}
}
