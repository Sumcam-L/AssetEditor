using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Sce.Atf.GraphicsApplications;

public class PerformanceTimers
{
	private readonly Stopwatch m_stopWatch = new Stopwatch();

	private readonly SortedDictionary<string, long> m_times = new SortedDictionary<string, long>();

	private long m_startTime;

	private long m_cycleTime;

	private int m_startStopCycles;

	private bool m_started;

	public int StartStopCycles => m_startStopCycles;

	public long CycleTime => m_cycleTime;

	public void Start()
	{
		m_times.Clear();
		m_stopWatch.Reset();
		m_startTime = m_stopWatch.ElapsedMilliseconds;
		m_stopWatch.Start();
		m_started = true;
	}

	public void Stop()
	{
		m_stopWatch.Stop();
		long elapsedMilliseconds = m_stopWatch.ElapsedMilliseconds;
		m_cycleTime = elapsedMilliseconds - m_startTime;
		m_startStopCycles++;
		m_started = false;
	}

	public void Clear()
	{
		CheckNotStarted();
		m_startStopCycles = 0;
		m_times.Clear();
	}

	public void StartPhase(string name)
	{
		CheckStarted();
		long elapsedMilliseconds = m_stopWatch.ElapsedMilliseconds;
		m_times[name] = elapsedMilliseconds;
	}

	public void StopPhase(string id)
	{
		CheckStarted();
		if (!m_times.TryGetValue(id, out var value))
		{
			value = m_startTime;
		}
		long elapsedMilliseconds = m_stopWatch.ElapsedMilliseconds;
		elapsedMilliseconds -= value;
		m_times[id] = elapsedMilliseconds;
	}

	public long GetPhaseTime(string id)
	{
		m_times.TryGetValue(id, out var value);
		return value;
	}

	public string GetDisplayString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<KeyValuePair<string, long>> list = new List<KeyValuePair<string, long>>(m_times);
		foreach (KeyValuePair<string, long> item in list)
		{
			stringBuilder.Append(item.Key);
			stringBuilder.Append(": ");
			stringBuilder.Append(item.Value.ToString());
			stringBuilder.Append(" ms ");
		}
		return stringBuilder.ToString();
	}

	private void CheckStarted()
	{
		if (!m_started)
		{
			Start();
		}
	}

	private void CheckNotStarted()
	{
		if (m_started)
		{
			Stop();
		}
	}
}
