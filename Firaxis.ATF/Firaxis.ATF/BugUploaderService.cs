using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using Firaxis.CivTech;
using Sce.Atf;

namespace Firaxis.ATF;

[Export(typeof(IInitializable))]
[Export(typeof(BugUploaderService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class BugUploaderService : IInitializable
{
	private ICivTechService m_civTechService;

	private string m_watchFolder;

	private IList<string> m_foldersToSubmit = new List<string>();

	private IDictionary<string, int> m_retryCount = new Dictionary<string, int>();

	private TaskFactory m_taskFactory = new TaskFactory();

	private Task m_uploaderTask;

	private FileSystemWatcher m_watcher;

	private readonly int kMaxRetries = 20;

	private const int kDefaultPort = 46181;

	public string ServerAddress { get; set; }

	public int ServerPort { get; set; }

	[ImportingConstructor]
	public BugUploaderService(ICivTechService civTechService)
	{
		ServerAddress = "2kgbalapp4";
		ServerPort = 46181;
		m_civTechService = civTechService;
	}

	public void Initialize()
	{
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		folderPath += "\\My Games";
		folderPath += "\\Sid Meier's Civilization 6\\Dumps";
		m_watchFolder = folderPath;
		if (!Directory.Exists(m_watchFolder))
		{
			Directory.CreateDirectory(m_watchFolder);
		}
		BuildSubmissionList();
		m_watcher = new FileSystemWatcher(m_watchFolder);
		m_watcher.Created += FileWatcher_Created;
		m_watcher.EnableRaisingEvents = true;
		m_uploaderTask = m_taskFactory.StartNew(delegate
		{
			SubmitExistingFolders();
		});
	}

	private void BuildSubmissionList()
	{
		lock (m_foldersToSubmit)
		{
			DirectoryInfo[] directories = new DirectoryInfo(m_watchFolder).GetDirectories();
			foreach (DirectoryInfo directoryInfo in directories)
			{
				m_foldersToSubmit.Add(directoryInfo.FullName);
			}
		}
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

	private void FileWatcher_Created(object sender, FileSystemEventArgs e)
	{
		if ((e.ChangeType & WatcherChangeTypes.Created) != WatcherChangeTypes.Created || !Directory.Exists(e.FullPath))
		{
			return;
		}
		m_taskFactory.StartNew(delegate
		{
			lock (m_foldersToSubmit)
			{
				m_foldersToSubmit.Add(e.FullPath);
			}
		});
	}

	private Stream GenerateAttactmentStreamAndUpdateMD5s(string folderPath, IEnumerable<Attachment> attachments)
	{
		MemoryStream memoryStream = new MemoryStream();
		foreach (Attachment attachment in attachments)
		{
			string text = folderPath + "/" + attachment.File;
			if (File.Exists(text))
			{
				attachment.MD5 = GenerateMD5(text);
				CopyFileToStream(text, memoryStream);
			}
		}
		memoryStream.Position = 0L;
		return memoryStream;
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

	private bool HeaderIsReady(string folderPath)
	{
		try
		{
			using (File.Open(folderPath + "\\header.json", FileMode.Open))
			{
			}
		}
		catch (FileNotFoundException)
		{
			return false;
		}
		catch (IOException e)
		{
			int num = Marshal.GetHRForException(e) & 0xFFFF;
			return num != 32 && num != 33;
		}
		return true;
	}

	private Stream PackageFolder(string fullPath, ref long headerLength)
	{
		new MemoryStream();
		string text = "";
		try
		{
			using StreamReader streamReader = new StreamReader(fullPath + "\\header.json");
			string text2;
			while ((text2 = streamReader.ReadLine()) != null)
			{
				text += text2;
			}
		}
		catch (System.Exception ex)
		{
			Console.WriteLine(ex.Message);
			return new MemoryStream();
		}
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		BugSubmissionHeader bugSubmissionHeader;
		try
		{
			bugSubmissionHeader = (BugSubmissionHeader)javaScriptSerializer.Deserialize(text, typeof(BugSubmissionHeader));
		}
		catch (System.Exception ex2)
		{
			Console.WriteLine(ex2.Message);
			return new MemoryStream();
		}
		if (bugSubmissionHeader == null)
		{
			return new MemoryStream();
		}
		string filePath = fullPath + "/" + bugSubmissionHeader.Catalog.Log.File;
		Stream log = GenerateStream(filePath);
		bugSubmissionHeader.Catalog.Log.MD5 = GenerateMD5(filePath);
		string filePath2 = fullPath + "/" + bugSubmissionHeader.Catalog.Dump.File;
		Stream dump = GenerateStream(filePath2);
		bugSubmissionHeader.Catalog.Dump.MD5 = GenerateMD5(filePath2);
		Stream attachments = GenerateAttactmentStreamAndUpdateMD5s(fullPath, bugSubmissionHeader.Catalog.Attachments);
		text = javaScriptSerializer.Serialize(bugSubmissionHeader);
		headerLength = text.Length;
		return GenerateSubmission(text, log, dump, attachments);
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

	private bool SendSubmission(long hdrLength, Stream stream)
	{
		if (hdrLength == 0L)
		{
			return true;
		}
		try
		{
			ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
			string text = $"{stream.Length:x8}";
			string text2 = GenerateMD5(stream);
			string text3 = $"{hdrLength:x8}";
			NetworkStream stream2 = new TcpClient(ServerAddress, ServerPort).GetStream();
			stream2.Write(aSCIIEncoding.GetBytes(text), 0, text.Length);
			stream2.Flush();
			stream2.Write(aSCIIEncoding.GetBytes(text2), 0, text2.Length);
			stream2.Flush();
			stream2.Write(aSCIIEncoding.GetBytes(text3), 0, text3.Length);
			stream2.Flush();
			stream.CopyTo(stream2);
			stream2.Flush();
			byte[] buffer = new byte[2];
			if (!ReceiveFromSocket(stream2, ref buffer, 2) || buffer[0] != 79 || buffer[1] != 75)
			{
				MessageBox.Show("Failed to receive acknowledgment from server for submission!");
				return false;
			}
		}
		catch (System.Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Info, "Failed to submit bug! Error=" + ex.Message);
			return false;
		}
		return true;
	}

	private void SubmitExistingFolders()
	{
		while (true)
		{
			string text = string.Empty;
			lock (m_foldersToSubmit)
			{
				if (m_foldersToSubmit.Count > 0)
				{
					text = m_foldersToSubmit[0];
					m_foldersToSubmit.RemoveAt(0);
				}
			}
			if (!string.IsNullOrEmpty(text) && !SubmitFolder(text))
			{
				lock (m_foldersToSubmit)
				{
					m_foldersToSubmit.Add(text);
				}
			}
			Thread.Sleep(50);
		}
	}

	private bool SubmitFolder(string folderPath)
	{
		if (!HeaderIsReady(folderPath))
		{
			if (m_retryCount.ContainsKey(folderPath))
			{
				m_retryCount[folderPath]++;
			}
			else
			{
				m_retryCount[folderPath] = 1;
			}
			if (m_retryCount[folderPath] > kMaxRetries)
			{
				m_retryCount.Remove(folderPath);
				Directory.Delete(folderPath, recursive: true);
				return true;
			}
			return false;
		}
		long headerLength = 0L;
		using (Stream stream = PackageFolder(folderPath, ref headerLength))
		{
			if (SendSubmission(headerLength, stream))
			{
				Directory.Delete(folderPath, recursive: true);
			}
		}
		return true;
	}
}
