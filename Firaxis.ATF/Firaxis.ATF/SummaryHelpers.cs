using System;
using System.Collections.Generic;
using System.Linq;

namespace Firaxis.ATF;

public static class SummaryHelpers
{
	public static readonly string kAssignCommand = "@assign";

	public static readonly string kSummaryCommand = "@summary";

	public static readonly string kTagCommand = "@tag";

	public static readonly char[] kWhitespaceChars = new char[4] { ' ', '\r', '\n', '\t' };

	public static readonly char[] kNewlineChars = new char[2] { '\r', '\n' };

	private static int IndexOfNotAny(this string str, char[] notAny, int startIndex)
	{
		for (int i = startIndex; i < str.Length; i++)
		{
			char value = str[i];
			if (!notAny.Contains(value))
			{
				return i;
			}
		}
		return -1;
	}

	private static int IndexOf(this string str, Func<char, bool> predicate, int startIndex = 0)
	{
		for (int i = startIndex; i < str.Length; i++)
		{
			if (predicate(str[i]))
			{
				return i;
			}
		}
		return -1;
	}

	private static int LastIndexOf(this string str, Func<char, bool> predicate)
	{
		for (int num = str.Length - 1; num >= 0; num--)
		{
			if (predicate(str[num]))
			{
				return num;
			}
		}
		return -1;
	}

	public static string MakeAlphaNumeric(string str)
	{
		return new string(Array.FindAll(str.ToCharArray(), (char c) => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-'));
	}

	public static void ProcessAssignCommand(ref string message, ref string assignee)
	{
		int num = message.IndexOf(kAssignCommand);
		if (num < 0)
		{
			return;
		}
		int num2 = message.IndexOfAny(kWhitespaceChars, num);
		if (num2 < 0 || num2 != num + kAssignCommand.Length)
		{
			return;
		}
		int num3 = message.IndexOfNotAny(kWhitespaceChars, num + kAssignCommand.Length);
		if (num3 >= 0)
		{
			int num4 = message.IndexOfAny(kWhitespaceChars, num3);
			if (num4 >= 0)
			{
				assignee = message.Substring(num3, num4 - num3);
				int num5 = message.IndexOfNotAny(kWhitespaceChars, num4);
				message = message.Remove(num, num5 - num);
			}
			else
			{
				assignee = message.Substring(num3);
				message = message.Remove(num);
			}
		}
	}

	public static bool ProcessSummaryCommand(ref string message, ref string summary)
	{
		int num = message.IndexOf(kSummaryCommand);
		if (num < 0)
		{
			return false;
		}
		int num2 = message.IndexOfAny(kWhitespaceChars, num);
		if (num2 < 0)
		{
			return false;
		}
		if (num2 != num + kSummaryCommand.Length)
		{
			return false;
		}
		int num3 = message.IndexOfNotAny(kWhitespaceChars, num + kSummaryCommand.Length);
		if (num3 < 0)
		{
			return false;
		}
		int num4 = message.IndexOfAny(kNewlineChars, num3);
		if (num4 >= 0)
		{
			summary = message.Substring(num3, num4 - num3);
			message = message.Remove(num, num4 - num);
		}
		else
		{
			summary = message.Substring(num3);
			message = message.Remove(num);
		}
		return true;
	}

	public static void ProcessTagCommands(ref string message, ref IList<string> tags)
	{
		int num = message.IndexOf(kTagCommand);
		while (num >= 0)
		{
			int num2 = message.IndexOfAny(kWhitespaceChars, num);
			if (num2 >= 0 && num2 == num + kTagCommand.Length)
			{
				int num3 = message.IndexOfNotAny(kWhitespaceChars, num + kTagCommand.Length);
				if (num3 >= 0)
				{
					int num4 = message.IndexOfAny(kWhitespaceChars, num3);
					if (num4 >= 0)
					{
						tags.Add(message.Substring(num3, num4 - num3));
						int num5 = message.IndexOfNotAny(kWhitespaceChars, num4);
						message = message.Remove(num, num5 - num);
					}
					else
					{
						tags.Add(message.Substring(num3));
						message = message.Remove(num);
					}
					num = message.IndexOf(kTagCommand);
					continue;
				}
				break;
			}
			break;
		}
	}

	public static string TrimString(string str)
	{
		int num = str.IndexOf((char ch) => !char.IsWhiteSpace(ch));
		int num2 = str.LastIndexOf((char ch) => !char.IsWhiteSpace(ch));
		if (num2 <= num)
		{
			return string.Empty;
		}
		return str.Substring(num, num2 - num + 1);
	}
}
