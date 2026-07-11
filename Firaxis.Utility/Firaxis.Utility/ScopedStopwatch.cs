using System;
using System.Diagnostics;

namespace Firaxis.Utility;

public class ScopedStopwatch : IDisposable
{
	private Stopwatch m_stopWatch = new Stopwatch();

	private string m_formatString;

	private Action<string> m_outputDelegate;

	public ScopedStopwatch(string fmtStr, Action<string> outputDelegate)
	{
		m_stopWatch.Start();
		m_formatString = fmtStr;
		m_outputDelegate = outputDelegate;
	}

	public void Dispose()
	{
		m_stopWatch.Stop();
		if (!string.IsNullOrEmpty(m_formatString))
		{
			string obj = string.Format(m_formatString, (double)m_stopWatch.ElapsedMilliseconds / 1000.0);
			m_outputDelegate?.Invoke(obj);
		}
	}
}
