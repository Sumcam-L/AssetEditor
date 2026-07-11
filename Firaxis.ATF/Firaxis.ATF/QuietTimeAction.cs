using System;
using System.Diagnostics;
using System.Threading;
using Firaxis.MathEx;
using Sce.Atf;

namespace Firaxis.ATF;

public class QuietTimeAction : IQuietTimeAction, IDisposable
{
	private const int kDefaultInitialQuietTimeInMS = 100;

	private const int kDefaultMaxBackedOffQuietTimeInMS = 15000;

	private readonly int m_initialQuietTimeInMS;

	private readonly int m_maxQuietTimeInMS;

	private QuietTimeWaitBehavior m_quietTimeWaitBehavior;

	private readonly string m_operationMessage;

	private Action m_actionOperation;

	private Timer m_actionTimer;

	private Stopwatch m_quietTimeClock;

	private int m_quietTimeInMS;

	private int m_updatesSinceLastCallback;

	public int UpdatesSinceLastAction => m_updatesSinceLastCallback;

	public QuietTimeAction(Action operation)
		: this(100, operation)
	{
	}

	public QuietTimeAction(QuietTimeWaitBehavior waitBehavior, Action operation)
		: this(100, waitBehavior, operation)
	{
	}

	public QuietTimeAction(int initialQuietTimeInMS, QuietTimeWaitBehavior waitBehavior, Action operation)
		: this(initialQuietTimeInMS, 15000, waitBehavior, operation, string.Empty)
	{
	}

	public QuietTimeAction(int initialQuietTimeInMS, int maxQuietTimeInMS, QuietTimeWaitBehavior waitBehavior, Action operation)
		: this(initialQuietTimeInMS, maxQuietTimeInMS, waitBehavior, operation, string.Empty)
	{
	}

	public QuietTimeAction(int initialQuietTimeInMS, Action operation)
		: this(initialQuietTimeInMS, operation, string.Empty)
	{
	}

	public QuietTimeAction(QuietTimeWaitBehavior waitBehavior, Action operation, string operationMessage)
		: this(100, 15000, waitBehavior, operation, operationMessage)
	{
	}

	public QuietTimeAction(int initialQuietTimeInMS, Action operation, string operationMessage)
		: this(initialQuietTimeInMS, 15000, QuietTimeWaitBehavior.FixedDuration, operation, operationMessage)
	{
	}

	public QuietTimeAction(int initialQuietTimeInMS, int maxQuietTimeInMS, QuietTimeWaitBehavior waitBehavior, Action operation, string operationMessage)
	{
		m_initialQuietTimeInMS = initialQuietTimeInMS;
		m_maxQuietTimeInMS = maxQuietTimeInMS;
		m_quietTimeInMS = m_initialQuietTimeInMS;
		m_quietTimeWaitBehavior = waitBehavior;
		m_actionOperation = operation;
		m_operationMessage = operationMessage;
		m_quietTimeClock = new Stopwatch();
		m_quietTimeClock.Reset();
		m_actionTimer = new Timer(OperationCallback);
	}

	public void Dispose()
	{
		using ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
		m_actionTimer.Dispose(manualResetEvent);
		m_actionTimer = null;
		manualResetEvent.WaitOne(30000);
	}

	public void UpdateLastChangeTime()
	{
		if (m_actionTimer != null)
		{
			m_updatesSinceLastCallback++;
			ScheduleAction(m_quietTimeWaitBehavior);
		}
	}

	private int ComputeQuietTimeDuration(QuietTimeWaitBehavior waitBehavior)
	{
		switch (waitBehavior)
		{
		case QuietTimeWaitBehavior.ExponentialBackoff:
			m_quietTimeInMS <<= 1;
			if (m_quietTimeInMS > m_maxQuietTimeInMS)
			{
				m_quietTimeInMS = m_maxQuietTimeInMS;
			}
			break;
		case QuietTimeWaitBehavior.SigmoidBackoff:
			m_quietTimeInMS = m_initialQuietTimeInMS + (int)(MathExtension.LogisticFn(30f, -1f, 50f, m_updatesSinceLastCallback) * 1000f);
			break;
		case QuietTimeWaitBehavior.Adaptive:
			if (m_quietTimeClock.IsRunning)
			{
				long num = (long)((double)((float)m_quietTimeInMS / 2f) + 0.5);
				long elapsedMilliseconds = m_quietTimeClock.ElapsedMilliseconds;
				if (elapsedMilliseconds > num)
				{
					m_quietTimeInMS -= (int)((float)m_quietTimeInMS / 4f);
				}
				else if (elapsedMilliseconds < num)
				{
					m_quietTimeInMS <<= 1;
				}
			}
			m_quietTimeClock.Restart();
			m_quietTimeInMS = MathUtil.Clamp(m_quietTimeInMS, m_initialQuietTimeInMS, m_maxQuietTimeInMS);
			break;
		}
		return m_quietTimeInMS;
	}

	private void ScheduleAction(QuietTimeWaitBehavior waitBehavior)
	{
		int dueTime = ComputeQuietTimeDuration(waitBehavior);
		m_actionTimer.Change(dueTime, -1);
	}

	private void OperationCallback(object timerObj)
	{
		if (!string.IsNullOrEmpty(m_operationMessage))
		{
			Outputs.WriteLine(OutputMessageType.Info, m_operationMessage);
		}
		m_actionOperation();
		m_quietTimeClock.Reset();
		m_updatesSinceLastCallback = 0;
		m_quietTimeInMS = m_initialQuietTimeInMS;
	}
}
