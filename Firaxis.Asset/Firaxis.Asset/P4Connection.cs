using System;

namespace Firaxis.Asset;

public class P4Connection : IDisposable
{
	private DateTime Epoch = new DateTime(1970, 1, 1);

	public string User { get; set; }

	public string Port { get; set; }

	public string Client { get; set; }

	public string Password { get; set; }

	public P4RecordSet Run(string command, params string[] args)
	{
		return new P4RecordSet();
	}

	public void Connect()
	{
	}

	public void Disconnect()
	{
	}

	public DateTime ConvertDate(int p4Date)
	{
		DateTime time = Epoch.AddSeconds(p4Date);
		return TimeZone.CurrentTimeZone.ToLocalTime(time);
	}

	public DateTime ConvertDate(string p4Date)
	{
		return ConvertDate(int.Parse(p4Date));
	}

	public void Dispose()
	{
		Disconnect();
	}
}
