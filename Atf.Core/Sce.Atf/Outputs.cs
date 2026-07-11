#define TRACE
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace Sce.Atf;

[Export(typeof(IInitializable))]
[Export(typeof(Outputs))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class Outputs : IInitializable, IPartImportsSatisfiedNotification
{
	[Import(AllowDefault = true)]
	private IScriptingService m_scriptingService = null;

	private static IOutputWriter[] s_outputWriters;

	private static TraceSource s_atfOutputTracer;

	[ImportMany(AllowRecomposition = true)]
	private IEnumerable<Lazy<IOutputWriter>> m_outputWriters;

	public static TraceSource TraceSource => s_atfOutputTracer;

	public static uint ErrorCount { get; private set; }

	public static uint WarningCount { get; private set; }

	static Outputs()
	{
		s_outputWriters = EmptyArray<IOutputWriter>.Instance;
		s_atfOutputTracer = new TraceSource("Sce.Atf.OutputsTracer");
	}

	void IInitializable.Initialize()
	{
		if (m_scriptingService != null)
		{
			m_scriptingService.LoadAssembly(GetType().Assembly);
			m_scriptingService.SetVariable("atfOutputs", this);
		}
	}

	void IPartImportsSatisfiedNotification.OnImportsSatisfied()
	{
		List<IOutputWriter> list = new List<IOutputWriter>(m_outputWriters.GetValues());
		if (list.Count > 0)
		{
			s_outputWriters = list.ToArray();
		}
		else
		{
			s_outputWriters = EmptyArray<IOutputWriter>.Instance;
		}
	}

	public static void Write(OutputMessageType type, string message)
	{
		Write(type, OutputMessageVerbosity.Normal, message);
	}

	public static void Write(OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		Write(type, verbosity, 0, message);
		switch (type)
		{
		case OutputMessageType.Error:
			ErrorCount++;
			break;
		case OutputMessageType.Warning:
			WarningCount++;
			break;
		}
	}

	public static void WriteDebug(OutputMessageType type, string message)
	{
		WriteDebug(type, OutputMessageVerbosity.Normal, message);
	}

	public static void WriteDebug(OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
		Write(type, verbosity, $"{stackTrace.GetFrame(2).ToString()} : {message}");
	}

	public static void Write(OutputMessageType type, int id, string message)
	{
		Write(type, OutputMessageVerbosity.Normal, id, message);
	}

	public static void Write(OutputMessageType type, OutputMessageVerbosity verbosity, int id, string message)
	{
		switch (type)
		{
		case OutputMessageType.Error:
			s_atfOutputTracer.TraceEvent(TraceEventType.Error, id, message);
			break;
		case OutputMessageType.Warning:
			s_atfOutputTracer.TraceEvent(TraceEventType.Warning, id, message);
			break;
		case OutputMessageType.Info:
			s_atfOutputTracer.TraceEvent(TraceEventType.Information, id, message);
			break;
		}
		IOutputWriter[] array = s_outputWriters;
		foreach (IOutputWriter outputWriter in array)
		{
			outputWriter.Write(type, verbosity, message);
		}
	}

	public static void Write(OutputMessageType type, string formatString, params object[] args)
	{
		Write(type, OutputMessageVerbosity.Normal, formatString, args);
	}

	public static void Write(OutputMessageType type, OutputMessageVerbosity verbosity, string formatString, params object[] args)
	{
		string message = string.Format(formatString, args);
		Write(type, verbosity, message);
	}

	public static void WriteLine(OutputMessageType type, string message)
	{
		WriteLine(type, OutputMessageVerbosity.Normal, message);
	}

	public static void WriteLine(OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		if (message.EndsWith("\r"))
		{
			message += "\n";
		}
		else if (!message.EndsWith(Environment.NewLine))
		{
			message += Environment.NewLine;
		}
		Write(type, verbosity, message);
	}

	public static void WriteLineDebug(OutputMessageType type, string message)
	{
		WriteLineDebug(type, OutputMessageVerbosity.Normal, message);
	}

	public static void WriteLineDebug(OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		if (message.EndsWith("\r"))
		{
			message += "\n";
		}
		else if (!message.EndsWith(Environment.NewLine))
		{
			message += Environment.NewLine;
		}
		WriteDebug(type, verbosity, message);
	}

	public static void WriteLine(OutputMessageType type, string formatString, params object[] args)
	{
		WriteLine(type, OutputMessageVerbosity.Normal, formatString, args);
	}

	public static void WriteLine(OutputMessageType type, OutputMessageVerbosity verbosity, string formatString, params object[] args)
	{
		string message = string.Format(formatString, args);
		WriteLine(type, verbosity, message);
	}

	public static void Clear()
	{
		IOutputWriter[] array = s_outputWriters;
		foreach (IOutputWriter outputWriter in array)
		{
			outputWriter.Clear();
		}
		ResetCounters();
	}

	public static void ResetCounters()
	{
		ErrorCount = 0u;
		WarningCount = 0u;
	}

	public static void Configure(params IOutputWriter[] outputWriters)
	{
		s_outputWriters = outputWriters;
	}
}
