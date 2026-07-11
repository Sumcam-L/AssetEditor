using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;

namespace Firaxis.ATF;

public class RegistryLoader<T> : IDisposable where T : ISerializable
{
	private volatile int m_numIOFilesLeft;

	private volatile int m_numLoadFilesLeft;

	private bool m_disposedValue;

	private object m_ioCountLocker = new object();

	private object m_loadCountLocker = new object();

	private readonly Queue<FileInfo> m_filesToLoad;

	private readonly Func<T> m_factoryFunction;

	private readonly Stopwatch m_ioStopWatch = new Stopwatch();

	private CancellationTokenSource m_asyncLoadCanceler = new CancellationTokenSource();

	private IList<Task> m_asyncTasks = new List<Task>();

	private readonly ConcurrentBag<long> m_loadTimes = new ConcurrentBag<long>();

	private readonly Thread m_readThread;

	private IDictionary<string, T> m_registry = new Dictionary<string, T>();

	public long LoadTimeInMS
	{
		get
		{
			long num = m_ioStopWatch.ElapsedMilliseconds;
			foreach (long loadTime in m_loadTimes)
			{
				num += loadTime;
			}
			return num;
		}
	}

	public IDictionary<string, T> Result => m_registry;

	public RegistryLoader(IEnumerable<string> filesToLoad, Func<T> factoryFunction)
	{
		m_factoryFunction = factoryFunction;
		List<FileInfo> list = new List<FileInfo>();
		foreach (string item2 in filesToLoad)
		{
			FileInfo item = new FileInfo(item2);
			list.Add(item);
		}
		m_filesToLoad = new Queue<FileInfo>(list.OrderByDescending((FileInfo x) => x.Length));
		m_numIOFilesLeft = m_filesToLoad.Count;
		m_readThread = new Thread(ThreadStartLoad);
		m_readThread.Name = "Registry Loader Read Thread";
		m_readThread.IsBackground = false;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public void StartLoad()
	{
		if (!m_readThread.IsAlive && m_filesToLoad.Count != 0)
		{
			m_readThread.Start();
		}
	}

	public void Wait()
	{
		Task.WaitAll(m_asyncTasks.ToArray());
		while (!IsDoneLoading())
		{
			Thread.Sleep(0);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_disposedValue)
		{
			return;
		}
		if (disposing)
		{
			m_asyncLoadCanceler.Cancel();
			Task.WaitAll(m_asyncTasks.ToArray());
			if (m_readThread.ThreadState != System.Threading.ThreadState.Unstarted)
			{
				m_readThread.Join();
			}
		}
		m_filesToLoad.Clear();
		m_registry.Clear();
		m_disposedValue = true;
	}

	private string GetXMLText(FileInfo info)
	{
		string result = string.Empty;
		try
		{
			using (StreamReader streamReader = info.OpenText())
			{
				result = streamReader.ReadToEnd();
			}
			lock (m_loadCountLocker)
			{
				m_numLoadFilesLeft++;
			}
		}
		catch (System.Exception)
		{
			result = string.Empty;
			Outputs.WriteLine(OutputMessageType.Error, "Unable to read text from the file {0}.  Ensure it is valid.", info.FullName);
		}
		lock (m_ioCountLocker)
		{
			m_numIOFilesLeft--;
			return result;
		}
	}

	private bool IsDoneLoading()
	{
		int num = 0;
		lock (m_ioCountLocker)
		{
			num = m_numIOFilesLeft;
		}
		if (num == 0)
		{
			int num2 = 0;
			lock (m_loadCountLocker)
			{
				num2 = m_numLoadFilesLeft;
			}
			if (num2 == 0)
			{
				return true;
			}
		}
		return false;
	}

	private void StartAsyncLoadTask(string xmlText, string filePath)
	{
		m_asyncTasks.Add(Task.Factory.StartNew(delegate
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			T value = m_factoryFunction();
			bool num = value.DeserializeFromXML(xmlText);
			stopwatch.Stop();
			m_loadTimes.Add(stopwatch.ElapsedMilliseconds);
			if (num)
			{
				lock (m_registry)
				{
					m_registry[filePath] = value;
				}
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Error, "Unable to deserialize the file {0}.  Ensure it is valid.", filePath);
			}
			lock (m_loadCountLocker)
			{
				m_numLoadFilesLeft--;
			}
		}, m_asyncLoadCanceler.Token));
	}

	private void ThreadStartLoad(object context)
	{
		m_ioStopWatch.Restart();
		while (m_filesToLoad.Count > 0)
		{
			FileInfo fileInfo = m_filesToLoad.Dequeue();
			string xMLText = GetXMLText(fileInfo);
			if (!string.IsNullOrEmpty(xMLText))
			{
				StartAsyncLoadTask(xMLText, fileInfo.FullName);
			}
		}
		Task.WaitAll(m_asyncTasks.ToArray());
		m_ioStopWatch.Stop();
	}
}
