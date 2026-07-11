using System;

namespace Firaxis.Utility.Converters;

public class DateTimeConverter
{
	public static DateTime ConvertFromUnixTimestamp(double timestamp)
	{
		return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(timestamp);
	}

	public static double ConvertToUnixTimestamp(DateTime date)
	{
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
		return Math.Floor((date - dateTime).TotalSeconds);
	}
}
