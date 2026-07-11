using System;

namespace Sce.Atf.Applications;

public static class OscServices
{
	private static readonly char[] s_illegalOscAddressChars = new char[5] { ' ', '{', '}', '[', ']' };

	public static void Send(this IOscService service, string oscAddress, object data)
	{
		service.Send(new Tuple<string, object>[1]
		{
			new Tuple<string, object>(oscAddress, data)
		});
	}

	public static string FixPropertyAddress(string oscAddress)
	{
		if (!oscAddress.StartsWith("/"))
		{
			oscAddress = "/" + oscAddress;
		}
		int num = 0;
		while (true)
		{
			num = oscAddress.IndexOfAny(s_illegalOscAddressChars, num);
			if (num < 0)
			{
				break;
			}
			oscAddress = oscAddress.Remove(num, 1);
		}
		return oscAddress;
	}
}
