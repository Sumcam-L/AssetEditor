using System;

namespace Sce.Atf;

public sealed class ReentryGuard
{
	private class UsingBlock : IDisposable
	{
		private readonly ReentryGuard m_owner;

		public UsingBlock(ReentryGuard guard)
		{
			m_owner = guard;
			if (!m_owner.m_allowReentry && m_owner.m_entryCount > 0)
			{
				throw new InvalidOperationException("This function or statement block is not allowed to be reentered.\nMake sure that CanEnter is checked first and that EnterAndExit() is\ncalled within a 'using' block.");
			}
			m_owner.m_entryCount++;
		}

		public void Dispose()
		{
			m_owner.m_entryCount--;
		}
	}

	private int m_entryCount;

	private bool m_allowReentry;

	public bool CanEnter => m_entryCount == 0;

	public bool HasEntered => m_entryCount > 0;

	public IDisposable EnterAndExit()
	{
		m_allowReentry = false;
		return new UsingBlock(this);
	}

	public IDisposable EnterAndExitMultiple()
	{
		m_allowReentry = true;
		return new UsingBlock(this);
	}
}
