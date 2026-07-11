using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Text;
using Firaxis.CivTech;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(IOutputWriter))]
[Export(typeof(ILogFileProvider))]
[Export(typeof(LogOutputWriter))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class LogOutputWriter : IOutputWriter, ILogFileProvider, IDisposable
{
	private int m_failCount;

	private FileStream m_logStream;

	private StreamWriter m_logWriter;

	private readonly int m_maxFailCount = 5;

	private string LogPath { get; set; }

	public bool AddFrameNumber { get; set; }

	public string LogFilePath => LogPath;

	public LogOutputWriter()
	{
		AssemblyName name = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName();
		string name2 = name.Name;
		Version version = name.Version;
		string text = version.Major + "." + version.Minor;
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		LogPath = folderPath + "\\" + name2 + "\\" + text + "\\Logs\\" + Guid.NewGuid().ToString() + ".log";
		string directoryName = Path.GetDirectoryName(LogPath);
		DateTime cutoffDate = DateTime.Now - TimeSpan.FromDays(30.0);
		CreateDirectory(directoryName);
		DeleteOldLogFiles(directoryName, cutoffDate);
		m_logStream = new FileStream(LogPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
		m_logWriter = new StreamWriter(m_logStream);
	}

	void IDisposable.Dispose()
	{
		DisposeStreams();
		GC.SuppressFinalize(this);
	}

	public void Clear()
	{
	}

	public void Write(OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		if (m_logWriter == null)
		{
			return;
		}
		string outputString = GetOutputString(type, message);
		try
		{
			lock (m_logWriter)
			{
				m_logWriter.Write(outputString);
				m_logWriter.Flush();
				m_failCount = 0;
			}
		}
		catch (IOException)
		{
			m_failCount++;
			if (m_failCount < m_maxFailCount)
			{
				return;
			}
			try
			{
				DisposeStreams();
			}
			catch (System.Exception)
			{
			}
			finally
			{
				m_logWriter = null;
				m_logStream = null;
			}
		}
	}

	private void CreateDirectory(string logDirectory)
	{
		if (!Directory.Exists(logDirectory))
		{
			Directory.CreateDirectory(logDirectory);
		}
	}

	private void DeleteOldLogFiles(string logDirectory, DateTime cutoffDate)
	{
		if (!Directory.Exists(logDirectory))
		{
			return;
		}
		string[] files = Directory.GetFiles(logDirectory);
		foreach (string text in files)
		{
			if (new FileInfo(text).LastWriteTimeUtc < cutoffDate.ToUniversalTime())
			{
				try
				{
					File.Delete(text);
				}
				catch
				{
				}
			}
		}
	}

	private void DisposeStreams()
	{
		if (m_logWriter != null && m_logStream != null)
		{
			m_logWriter.Flush();
			m_logStream.Flush();
		}
		if (m_logWriter != null)
		{
			m_logWriter.Close();
			m_logWriter.Dispose();
			m_logWriter = null;
		}
		if (m_logStream != null)
		{
			m_logStream.Close();
			m_logStream.Dispose();
			m_logStream = null;
		}
	}

	private string GetOutputString(OutputMessageType type, string message)
	{
		StringBuilder stringBuilder = new StringBuilder();
		switch (type)
		{
		case OutputMessageType.Diagnostic:
			stringBuilder.AppendFormat("{0,-5}", "DIAG");
			break;
		case OutputMessageType.Error:
			stringBuilder.AppendFormat("{0,-5}", "ERROR");
			break;
		case OutputMessageType.Warning:
			stringBuilder.AppendFormat("{0,-5}", "WARN");
			break;
		case OutputMessageType.Bug:
			stringBuilder.AppendFormat("{0,-5}", "BUG");
			break;
		default:
			stringBuilder.AppendFormat("{0,-5}", "INFO");
			break;
		}
		stringBuilder.AppendFormat(" {0:HH:mm:ss}Z", DateTime.Now.ToUniversalTime());
		if (AddFrameNumber)
		{
			uint num = (FiraxisATFRegistry.AssetPreviewerService?.AssetPreviewer?.FrameNumber).GetValueOrDefault() % 65535;
			stringBuilder.AppendFormat("({0:X5})", num);
		}
		stringBuilder.AppendFormat(" {0}", message);
		return stringBuilder.ToString();
	}
}
