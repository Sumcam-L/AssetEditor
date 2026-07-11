using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Firaxis.CivTech;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[Export(typeof(CategorizedOutputs))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CategorizedOutputs : IInitializable, IPartImportsSatisfiedNotification
{
	private static ICategorizedOutputWriter[] s_outputWriters;

	[ImportMany(AllowRecomposition = true)]
	private IEnumerable<Lazy<ICategorizedOutputWriter>> m_outputWriters;

	[Import(AllowDefault = true)]
	private IScriptingService m_scriptingService;

	private static IOutputWriter s_logWriter;

	[Import(AllowDefault = true)]
	private Lazy<LogOutputWriter> m_logWriter;

	public static OutputMessageType ConvertEventType(LogEventType evtType)
	{
		return evtType switch
		{
			LogEventType.Error => OutputMessageType.Error, 
			LogEventType.Warning => OutputMessageType.Warning, 
			_ => OutputMessageType.Info, 
		};
	}

	public static LogEventType ConvertEventType(OutputMessageType evtType)
	{
		return evtType switch
		{
			OutputMessageType.Error => LogEventType.Error, 
			OutputMessageType.Warning => LogEventType.Warning, 
			_ => LogEventType.Info, 
		};
	}

	static CategorizedOutputs()
	{
		s_outputWriters = EmptyArray<ICategorizedOutputWriter>.Instance;
	}

	public static void Write(string category, OutputMessageType type, string message)
	{
		Write(category, type, OutputMessageVerbosity.Normal, message);
	}

	public static void Write(string category, OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		TaskUtil.FireAndForgetTask.Run(delegate
		{
			ICategorizedOutputWriter[] array = s_outputWriters;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Write(category, type, verbosity, message);
			}
			s_logWriter.Write(type, verbosity, message);
		});
	}

	public static void Write(string category, OutputMessageType type, string formatString, params object[] args)
	{
		Write(category, type, OutputMessageVerbosity.Normal, formatString, args);
	}

	public static void Write(string category, OutputMessageType type, OutputMessageVerbosity verbosity, string formatString, params object[] args)
	{
		Write(category, type, verbosity, string.Format(formatString, args));
	}

	public static void WriteLine(string category, OutputMessageType type, string message)
	{
		WriteLine(category, type, OutputMessageVerbosity.Normal, message);
	}

	public static void WriteLine(string category, OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		Write(category, type, verbosity, AddNewLineIfNeeded(message));
	}

	public static void WriteLine(string category, OutputMessageType type, string formatString, params object[] args)
	{
		WriteLine(category, type, OutputMessageVerbosity.Normal, formatString, args);
	}

	public static void WriteLine(string category, OutputMessageType type, OutputMessageVerbosity verbosity, string formatString, params object[] args)
	{
		WriteLine(category, type, verbosity, string.Format(formatString, args));
	}

	public static void Clear()
	{
		s_outputWriters.ForEach(delegate(ICategorizedOutputWriter writer)
		{
			writer.Clear();
		});
	}

	void IInitializable.Initialize()
	{
		if (m_scriptingService != null)
		{
			m_scriptingService.LoadAssembly(GetType().Assembly);
			m_scriptingService.SetVariable("fxsOutputs", this);
		}
	}

	void IPartImportsSatisfiedNotification.OnImportsSatisfied()
	{
		List<ICategorizedOutputWriter> list = new List<ICategorizedOutputWriter>(m_outputWriters.GetValues());
		if (list.Count > 0)
		{
			s_outputWriters = list.ToArray();
		}
		else
		{
			s_outputWriters = EmptyArray<ICategorizedOutputWriter>.Instance;
		}
		s_logWriter = m_logWriter?.Value;
	}

	private static string AddNewLineIfNeeded(string originalMessage)
	{
		if (originalMessage.EndsWith("\n"))
		{
			return originalMessage;
		}
		if (originalMessage.EndsWith("\n\r"))
		{
			return originalMessage.Substring(0, originalMessage.Length - 1);
		}
		return originalMessage + "\n";
	}
}
