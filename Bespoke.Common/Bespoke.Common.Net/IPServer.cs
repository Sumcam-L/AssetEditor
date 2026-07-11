using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Bespoke.Common.Net;

public abstract class IPServer
{
	public static IPAddress[] GetLocalIPAddress()
	{
		IPAddress[] hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
		if (hostAddresses.Length == 0)
		{
			throw new Exception("No local IP Address address found.");
		}
		return hostAddresses;
	}

	public static bool IsIPEndPointAvailable(IPAddress ipAddress, int port)
	{
		IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
		return IsIPEndPointAvailable(ipEndPoint);
	}

	public static bool IsIPEndPointAvailable(IPEndPoint ipEndPoint)
	{
		bool result = true;
		IPGlobalProperties iPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
		IPEndPoint[] activeUdpListeners = iPGlobalProperties.GetActiveUdpListeners();
		IPEndPoint[] array = activeUdpListeners;
		foreach (IPEndPoint iPEndPoint in array)
		{
			if (iPEndPoint.Address == ipEndPoint.Address && iPEndPoint.Port == ipEndPoint.Port)
			{
				result = false;
				break;
			}
		}
		return result;
	}
}
