using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows;
using Firaxis.CivTech;
using Firaxis.CivTech.Properties;
using Firaxis.MathEx;
using Firaxis.Utility;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(ICrashSubmissionService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CrashSubmissionService : ICrashSubmissionService
{
	private string m_bugHelperFolderSuffix = string.Empty;

	private const int kDefaultPort = 46181;

	public IList<string> Attachments { get; set; }

	public uint Changelist { get; set; }

	public string Log { get; set; }

	public string ServerAddress { get; set; }

	public int ServerPort { get; set; }

	public ulong SessionHash { get; set; }

	public bool UseBugHelper { get; private set; }

	private ILogFileProvider LogFileProvider { get; set; }

	[ImportingConstructor]
	public CrashSubmissionService(ILogFileProvider logFileProvider)
	{
		Changelist = 0u;
		LogFileProvider = logFileProvider;
		object[] customAttributes = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), inherit: false);
		if (customAttributes.Length != 0 && customAttributes[0] is AssemblyInformationalVersionAttribute assemblyInformationalVersionAttribute)
		{
			Changelist = Convert.ToUInt32(assemblyInformationalVersionAttribute.InformationalVersion);
		}
		SessionHash = FNV1a.HashValue64(Environment.MachineName, Changelist, DateTime.Now);
		Attachments = new List<string>();
		ServerAddress = "2kgbalapp4";
		ServerPort = 46181;
		UseBugHelper = false;
		m_bugHelperFolderSuffix = "My Games\\Sid Meier's Civilization VI\\PackagedDumps";
		if (string.IsNullOrEmpty(LogFileProvider.LogFilePath))
		{
			return;
		}
		try
		{
			AddAttachment(new Uri(LogFileProvider.LogFilePath));
		}
		catch (System.Exception ex)
		{
			try
			{
				Outputs.WriteLine(OutputMessageType.Error, OutputMessageVerbosity.Normal, "Failed to obtain valid log path from log writer!\n\n{0}", ex.ToString());
			}
			catch (System.Exception)
			{
			}
		}
	}

	public void EnableBugHelper(string gameBasePath)
	{
		string path = (Firaxis.CivTech.Properties.Resources.ModTools ? gameBasePath : Path.Combine(gameBasePath, "Binaries\\Win64"));
		string text = Path.Combine(path, "bughelper_Win64_DX11_Release.exe");
		if (!File.Exists(text))
		{
			text = Path.Combine(path, "FiraxisBugReporter.exe");
		}
		if (!File.Exists(text))
		{
			text = Path.Combine(path, "BugReporter.exe");
		}
		if (!File.Exists(text))
		{
			path = Path.Combine(gameBasePath, "Binaries\\Win64Steam");
			text = Path.Combine(path, "FiraxisBugReporter.exe");
		}
		if (!File.Exists(text))
		{
			path = Path.Combine(gameBasePath, "Binaries\\Win64Steam");
			text = Path.Combine(path, "BugReporter.exe");
		}
		if (!File.Exists(text))
		{
			Outputs.Write(OutputMessageType.Error, "Failed to start bug submission agent");
			return;
		}
		Assembly entryAssembly = Assembly.GetEntryAssembly();
		if (entryAssembly == null)
		{
			Outputs.Write(OutputMessageType.Error, "Failed to find entry point, not starting bug submission agent");
			return;
		}
		UseBugHelper = true;
		Process.Start(text, "-process=" + Path.GetFileName(entryAssembly.Location));
	}

	public void SubmitIssue(SubmissionType issueType, string message)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(Environment.StackTrace);
		SubmitCrashInternal(message, stringBuilder.ToString(), issueType);
	}

	public void SubmitIssue(SubmissionType issueType, System.Exception excObj)
	{
		string text = null;
		string text2 = null;
		string text3 = null;
		int num = 0;
		bool flag = false;
		Queue<System.Exception> queue = new Queue<System.Exception>();
		queue.Enqueue(excObj);
		while (queue.Count > 0)
		{
			System.Exception ex = queue.Dequeue();
			text = ex.Message;
			if (text2 == null)
			{
				text2 = ex.StackTrace ?? "";
				text3 = ((ex.StackTrace == null) ? text : ("(" + ex.GetType().Name + ") " + text + "\n" + text2));
			}
			else if (ex.StackTrace == null)
			{
				text3 = "(" + ex.GetType().Name + ") " + text + "\n\n" + text3;
			}
			else
			{
				text2 = ex.StackTrace + "\n\n----------\n\n" + text2;
				text3 = "(" + ex.GetType().Name + ") " + text + "\n" + ex.StackTrace + "\n\n" + text3;
			}
			if (ex is AggregateException ex2)
			{
				num += ex2.InnerExceptions.Count;
				foreach (System.Exception innerException in ex2.InnerExceptions)
				{
					if (innerException != ex2.InnerException)
					{
						queue.Enqueue(innerException);
						flag = true;
					}
				}
			}
			if (ex.InnerException != null)
			{
				queue.Enqueue(ex.InnerException);
				flag = true;
			}
		}
		if (num > 1)
		{
			text = $"Multiple Aggregated Exceptions ({num})";
		}
		if (flag)
		{
			using (TempFileHandler tempFileHandler = new TempFileHandler("FullStack.txt"))
			{
				if (File.Exists(tempFileHandler.FilePath))
				{
					File.Delete(tempFileHandler.FilePath);
				}
				File.WriteAllText(tempFileHandler.FilePath, text3);
				AddAttachment(new Uri(tempFileHandler.FilePath));
				SubmitCrashInternal(text, text2, issueType);
				return;
			}
		}
		SubmitCrashInternal(text, text2, issueType);
	}

	public void AddAttachment(Uri attachment)
	{
		lock (Attachments)
		{
			Attachments.Add(attachment.LocalPath);
		}
	}

	public void RemoveAttachment(Uri attachment)
	{
		lock (Attachments)
		{
			Attachments.Remove(attachment.LocalPath);
		}
	}

	private string GetIssueTypeDescription(SubmissionType issueType)
	{
		return issueType switch
		{
			SubmissionType.kAssert => "ASSERT", 
			SubmissionType.kCrash => "CRASH", 
			SubmissionType.kSilentAssert => "SILENT", 
			_ => "UNKNOWN", 
		};
	}

	private string GetSubmissionSummary(string message)
	{
		string summary = string.Empty;
		IList<string> tags = new List<string>();
		string assignee = string.Empty;
		SummaryHelpers.ProcessAssignCommand(ref message, ref assignee);
		SummaryHelpers.ProcessTagCommands(ref message, ref tags);
		if (!SummaryHelpers.ProcessSummaryCommand(ref message, ref summary))
		{
			summary = message;
		}
		return SummaryHelpers.TrimString(summary);
	}

	private void SubmitCrashInternal(string message, string stackTrace, SubmissionType subType)
	{
		string issueTypeDescription = GetIssueTypeDescription(subType);
		string submissionSummary = GetSubmissionSummary(message);
		Outputs.WriteLine(OutputMessageType.Bug, OutputMessageVerbosity.ExtremelyVerbose, "{0}: {1}\n", issueTypeDescription, submissionSummary);
		if ((subType != SubmissionType.kCrash && Firaxis.CivTech.Properties.Resources.ModTools) || NativeMethods.IsDebuggerPresent())
		{
			return;
		}
		IList<Attachment> list = new List<Attachment>();
		IEnumerable<string> attachments = Enumerable.Empty<string>();
		lock (Attachments)
		{
			attachments = Attachments.ToArray();
		}
		Stream stream = GenerateAttachmentStream(attachments, list);
		BugSubmissionHeader bugSubmissionHeader = new BugSubmissionHeader(stream.Length);
		bugSubmissionHeader.Catalog.Log.File = Path.GetFileName(Log);
		bugSubmissionHeader.Catalog.Log.MD5 = GenerateMD5(Log);
		Stream log = GenerateStream(Log);
		string text = Path.GetTempPath() + Path.GetFileName((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location) + "." + Guid.NewGuid().ToString() + ".dmp";
		using (FileStream fileStream = new FileStream(text, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			if (DebugHelp.WriteMiniDump(fileStream, DebugHelp.Option.Normal))
			{
				fileStream.Close();
				bugSubmissionHeader.Catalog.Dump.File = Path.GetFileName(text);
				bugSubmissionHeader.Catalog.Dump.MD5 = GenerateMD5(text);
			}
		}
		Stream dump = GenerateStream(text);
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		bugSubmissionHeader.Exception.Message = message;
		bugSubmissionHeader.Exception.StackTrace = stackTrace;
		bugSubmissionHeader.Line = 0;
		bugSubmissionHeader.File = string.Empty;
		bugSubmissionHeader.SubmissionType = issueTypeDescription;
		bugSubmissionHeader.Catalog.Attachments = list;
		try
		{
			AssemblyName name = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName();
			bugSubmissionHeader.Version.Build = (uint)name.Version.Build;
			bugSubmissionHeader.Version.Major = (uint)name.Version.Major;
			bugSubmissionHeader.Version.Minor = (uint)name.Version.Minor;
			bugSubmissionHeader.Version.Rev = (uint)name.Version.Revision;
		}
		catch
		{
		}
		bugSubmissionHeader.Session.Hash = SessionHash;
		bugSubmissionHeader.Changelist = Changelist;
		string text2 = new JavaScriptSerializer().Serialize(bugSubmissionHeader);
		Stream stream2 = GenerateSubmission(text2, log, dump, stream);
		if (UseBugHelper)
		{
			string submissionFolder = GenerateBugHelperSubmissionPath(stream2);
			WriteSubmission(text2.Length, stream2, submissionFolder);
		}
		else
		{
			SendSubmission(text2.Length, stream2);
		}
	}

	private string GenerateBugHelperBasePath()
	{
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), m_bugHelperFolderSuffix);
	}

	private string GenerateBugHelperSubmissionPath(Stream file)
	{
		return Path.Combine(GenerateBugHelperBasePath(), GenerateMD5(file));
	}

	private string GenerateMD5(Stream file)
	{
		long position = file.Position;
		byte[] array = MD5.Create().ComputeHash(file);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		file.Position = position;
		return stringBuilder.ToString();
	}

	private string GenerateMD5(string filePath)
	{
		string empty = string.Empty;
		if (!File.Exists(filePath))
		{
			return empty;
		}
		using FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		return GenerateMD5(file);
	}

	private void CopyFileToStream(string filePath, Stream stream)
	{
		ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
		using MemoryStream memoryStream = new MemoryStream();
		try
		{
			using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			using (GZipStream destination = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
			{
				fileStream.CopyTo(destination);
			}
			fileStream.Close();
		}
		catch (System.Exception ex)
		{
			MessageBox.Show(ex.Message);
		}
		memoryStream.Position = 0L;
		string text = $"{memoryStream.Length:x8}";
		string text2 = GenerateMD5(memoryStream);
		memoryStream.Position = 0L;
		stream.Write(aSCIIEncoding.GetBytes(text), 0, text.Length);
		stream.Write(aSCIIEncoding.GetBytes(text2), 0, text2.Length);
		memoryStream.CopyTo(stream);
	}

	private Stream GenerateStream(string filePath)
	{
		if (!File.Exists(filePath))
		{
			return new MemoryStream();
		}
		MemoryStream memoryStream = new MemoryStream();
		CopyFileToStream(filePath, memoryStream);
		memoryStream.Position = 0L;
		return memoryStream;
	}

	private Stream GenerateAttachmentStream(IEnumerable<string> attachments, IList<Attachment> attachmentInfo)
	{
		MemoryStream memoryStream = new MemoryStream();
		foreach (string attachment in attachments)
		{
			if (File.Exists(attachment))
			{
				string md = GenerateMD5(attachment);
				CopyFileToStream(attachment, memoryStream);
				attachmentInfo.Add(new Attachment(Path.GetFileName(attachment), md));
			}
		}
		memoryStream.Position = 0L;
		return memoryStream;
	}

	private Stream GenerateSubmission(string header, Stream log, Stream dump, Stream attachments)
	{
		ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
		MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(aSCIIEncoding.GetBytes(header), 0, header.Length);
		log.CopyTo(memoryStream);
		dump.CopyTo(memoryStream);
		attachments.CopyTo(memoryStream);
		memoryStream.Position = 0L;
		return memoryStream;
	}

	private bool ReceiveFromSocket(NetworkStream socketStream, ref byte[] buffer, int bufSize)
	{
		int num = bufSize;
		while (num > 0)
		{
			if (socketStream.CanRead)
			{
				int num2 = socketStream.Read(buffer, bufSize - num, num);
				if (num2 == 0)
				{
					return false;
				}
				num -= num2;
			}
		}
		return true;
	}

	private Stream PackageSubmission(int hdrLength, Stream payload)
	{
		ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
		string text = $"{payload.Length:x8}";
		string text2 = GenerateMD5(payload);
		string text3 = $"{hdrLength:x8}";
		Stream stream = new MemoryStream(hdrLength + 64);
		stream.Write(aSCIIEncoding.GetBytes(text), 0, text.Length);
		stream.Write(aSCIIEncoding.GetBytes(text2), 0, text2.Length);
		stream.Write(aSCIIEncoding.GetBytes(text3), 0, text3.Length);
		payload.CopyTo(stream);
		stream.Flush();
		stream.Seek(0L, SeekOrigin.Begin);
		return stream;
	}

	private void WriteSubmission(int hdrLength, Stream stream, string submissionFolder)
	{
		if (!Directory.Exists(submissionFolder))
		{
			Directory.CreateDirectory(submissionFolder);
		}
		string path = Path.Combine(submissionFolder, "submission.pkg");
		using (Stream destination = File.Create(path, hdrLength + 64, FileOptions.WriteThrough))
		{
			PackageSubmission(hdrLength, stream).CopyTo(destination);
		}
		File.SetAttributes(path, FileAttributes.ReadOnly);
	}

	private void SendSubmission(int hdrLength, Stream stream)
	{
		TcpClient tcpClient;
		try
		{
			tcpClient = new TcpClient(ServerAddress, ServerPort);
		}
		catch (System.Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Info, "Failed to connect to bug submission server " + ServerAddress + "! Error=" + ex.Message);
			return;
		}
		Stream stream2 = PackageSubmission(hdrLength, stream);
		try
		{
			NetworkStream stream3 = tcpClient.GetStream();
			stream2.CopyTo(stream3);
			stream3.Flush();
			byte[] buffer = new byte[2];
			if (!ReceiveFromSocket(stream3, ref buffer, 2) || buffer[0] != 79 || buffer[1] != 75)
			{
				MessageBox.Show("Failed to receive acknowledgment from server for submission!");
			}
		}
		catch (System.Exception ex2)
		{
			Outputs.WriteLine(OutputMessageType.Info, "Failed to transmit bug to submission server! Error=" + ex2.Message);
		}
	}
}
