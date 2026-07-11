using System;
using System.ComponentModel.Composition;
using System.Text;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(IOutputWriter))]
[Export(typeof(IFramedOutputWriter))]
[Export(typeof(TeamCityOutputWriter))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TeamCityOutputWriter : IOutputWriter, IFramedOutputWriter
{
	private string m_errorDetails = string.Empty;

	public void StartFrame(string message)
	{
		string arg = EscapeMessage(message);
		Console.WriteLine("##teamcity[compilationStarted compiler='{0}']", arg);
	}

	public void EndFrame(string message)
	{
		string arg = EscapeMessage(message);
		Console.WriteLine("##teamcity[compilationFinished compiler='{0}']", arg);
	}

	public void SetErrorDetails(string errorDetails)
	{
		m_errorDetails = EscapeMessage(errorDetails);
	}

	public void Write(OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		string arg = EscapeMessage(message);
		switch (type)
		{
		case OutputMessageType.Error:
			Console.WriteLine("##teamcity[message text='{0}' errorDetails='{1}' status='ERROR']", arg, m_errorDetails);
			break;
		case OutputMessageType.Warning:
			Console.WriteLine("##teamcity[message text='{0}' status='WARNING']", arg);
			break;
		default:
			Console.WriteLine("##teamcity[message text='{0}']", arg);
			break;
		}
	}

	public void Clear()
	{
	}

	private bool NeedsEscaping(char ch)
	{
		if (ch != '\'' && ch != '\n' && ch != '\r' && ch != '|' && ch != '[')
		{
			return ch == ']';
		}
		return true;
	}

	private string EscapeMessage(string msg)
	{
		StringBuilder stringBuilder = new StringBuilder(msg.Length);
		foreach (char c in msg)
		{
			if (!NeedsEscaping(c))
			{
				stringBuilder.Append(c);
				continue;
			}
			switch (c)
			{
			case '\n':
				stringBuilder.Append("|n");
				break;
			case '\r':
				stringBuilder.Append("|r");
				break;
			default:
				stringBuilder.Append('|');
				stringBuilder.Append(c);
				break;
			}
		}
		return stringBuilder.ToString();
	}
}
