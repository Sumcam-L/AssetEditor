namespace Firaxis.Utility;

public static class TimeCode
{
	public static float ToValue(string time, TimeCodeFormat format)
	{
		if (string.IsNullOrEmpty(time))
		{
			return 0f;
		}
		float num = 0f;
		foreach (char c in time)
		{
			if (!char.IsDigit(c) && c != ':' && c != '.')
			{
				return 0f;
			}
		}
		if (!time.Contains(":"))
		{
			return Transpose.FromString<float>(time);
		}
		int num2 = 0;
		string text = "";
		int num3 = time.Length - 1;
		while (num3 >= 0 && num2 < 4)
		{
			char c2 = time[num3];
			if (c2 == ':' || c2 == '.')
			{
				int num4 = Transpose.FromString<int>(text);
				text = "";
				switch (num2)
				{
				case 0:
					num += (float)num4 / 30f;
					break;
				case 1:
					num += (float)num4;
					break;
				case 2:
					num += (float)(num4 * 60);
					break;
				case 3:
					num += (float)((format == TimeCodeFormat.Frame) ? (num4 * 3600) : (num4 * 100));
					break;
				}
				num2++;
			}
			else
			{
				string text2 = text;
				text = time.Substring(num3, 1);
				text += text2;
			}
			num3--;
		}
		if (num2 < 4)
		{
			if (num3 >= 0)
			{
				string text3 = text;
				text = time.Substring(num3, 1);
				text += text3;
			}
			int num5 = Transpose.FromString<int>(text);
			switch (num2)
			{
			case 0:
				num += (float)num5 / 30f;
				break;
			case 1:
				num += (float)num5;
				break;
			case 2:
				num += (float)(num5 * 60);
				break;
			case 3:
				num += (float)((format == TimeCodeFormat.Frame) ? (num5 * 3600) : (num5 * 100));
				break;
			}
		}
		return num;
	}

	public static string ToString(float time)
	{
		return ToString(time, TimeCodeFormat.Frame);
	}

	public static string ToString(float time, TimeCodeFormat format)
	{
		int num = (int)(time / 3600f);
		int num2 = (int)(time / 60f - (float)(num * 60));
		int num3 = (int)(time - (float)(num2 * 60)) % 60;
		string result = "";
		switch (format)
		{
		case TimeCodeFormat.Seconds:
			result = $"{time:f2}";
			break;
		case TimeCodeFormat.MS:
		{
			int num6 = (int)(time - (float)(num3 * 100)) % 100;
			result = $"{num}:{num2:d2}:{num3:d2}:{num6:d2}";
			break;
		}
		case TimeCodeFormat.HMS:
			result = $"{num}:{num2:d2}:{num3:d2}";
			break;
		case TimeCodeFormat.Frame:
		{
			float num4 = time - (float)((int)time / 30 * 30);
			int num5 = (int)(num4 * 30f) % 30;
			result = $"{num}:{num2:d2}:{num3:d2}:{num5:d2}";
			break;
		}
		}
		return result;
	}
}
