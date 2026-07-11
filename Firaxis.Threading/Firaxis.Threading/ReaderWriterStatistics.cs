using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Firaxis.Threading;

public static class ReaderWriterStatistics
{
	private class LockerInfo
	{
		private class TimedLockOperation : IDisposable
		{
			private Action FinishAction;

			public TimedLockOperation(Action start, Action finish)
			{
				FinishAction = finish;
				start();
			}

			public void Dispose()
			{
				FinishAction();
			}
		}

		private long m_startReadEnter = 0L;

		private long m_startWriteEnter = 0L;

		private long m_startReadLock = 0L;

		private long m_startWriteLock = 0L;

		private long m_totalReadLockCount = 0L;

		private long m_timeReadLocked = 0L;

		private long m_timeWaitingForRead = 0L;

		private long m_totalWriteLockCount = 0L;

		private long m_timeWriteLocked = 0L;

		private long m_timeWaitingForWrite = 0L;

		public long TotalReadLockCount => m_totalReadLockCount;

		public long TimeWithReadLock => m_timeReadLocked;

		public long TimeWaitingForReadLock => m_timeWaitingForRead;

		public long TotalWriteLockCount => m_totalWriteLockCount;

		public long TimeWithWriteLock => m_timeWriteLocked;

		public long TimeWaitingForWriteLock => m_timeWaitingForWrite;

		public long UnitsPerSecond => Stopwatch.Frequency;

		private void StartEnteringRead()
		{
			m_startReadEnter = Stopwatch.GetTimestamp();
		}

		private void StopEnteringRead()
		{
			m_timeWaitingForRead += Stopwatch.GetTimestamp() - m_startReadEnter;
		}

		private void EnterReadLock()
		{
			Interlocked.Increment(ref m_totalReadLockCount);
			m_startReadLock = Stopwatch.GetTimestamp();
		}

		private void LeaveReadLock()
		{
			m_timeReadLocked += Stopwatch.GetTimestamp() - m_startReadLock;
		}

		private void StartEnteringWrite()
		{
			m_startWriteEnter = Stopwatch.GetTimestamp();
		}

		private void StopEnteringWrite()
		{
			m_timeWaitingForWrite += Stopwatch.GetTimestamp() - m_startWriteEnter;
		}

		private void EnterWriteLock()
		{
			Interlocked.Increment(ref m_totalWriteLockCount);
			m_startWriteLock = Stopwatch.GetTimestamp();
		}

		private void LeaveWriteLock()
		{
			m_timeWriteLocked += Stopwatch.GetTimestamp() - m_startWriteLock;
		}

		public IDisposable GetReadEnterTimer()
		{
			return new TimedLockOperation(StartEnteringRead, StopEnteringRead);
		}

		public IDisposable GetReadLockTimer()
		{
			return new TimedLockOperation(EnterReadLock, LeaveReadLock);
		}

		public IDisposable GetWriteEnterTimer()
		{
			return new TimedLockOperation(StartEnteringWrite, StopEnteringWrite);
		}

		public IDisposable GetWriteLockTimer()
		{
			return new TimedLockOperation(EnterWriteLock, LeaveWriteLock);
		}
	}

	private static ConcurrentDictionary<string, LockerInfo> s_statistics = new ConcurrentDictionary<string, LockerInfo>();

	public static IDisposable TimeEnteringReadLock(int framesToRemove)
	{
		return null;
	}

	public static IDisposable TimeReadLock(int framesToRemove)
	{
		return null;
	}

	public static IDisposable TimeEnteringWriteLock(int framesToRemove)
	{
		return null;
	}

	public static IDisposable TimeWriteLock(int framesToRemove)
	{
		return null;
	}

	private static string GetCallerName(int stackFramesToRemove)
	{
		StackTrace stackTrace = new StackTrace(stackFramesToRemove + 1, fNeedFileInfo: true);
		StackFrame frame = stackTrace.GetFrame(0);
		return $"{frame.GetFileName()}({frame.GetFileLineNumber()})";
	}

	public static void DumpStatistics(StringBuilder builder)
	{
	}
}
