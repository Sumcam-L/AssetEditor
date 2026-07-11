using System;

namespace Sce.Atf.Applications;

public class CommandCount
{
	private int m_current;

	private int m_clean;

	private bool m_forceDirty;

	public int Current => m_current;

	public bool CanDecrement => m_current > 0;

	public bool Dirty
	{
		get
		{
			return m_forceDirty || m_clean != m_current;
		}
		set
		{
			bool dirty = Dirty;
			if (!value)
			{
				m_clean = m_current;
				m_forceDirty = false;
			}
			else
			{
				m_forceDirty = true;
			}
			CheckDirtyChanged(dirty);
		}
	}

	public event EventHandler DirtyChanged;

	public CommandCount()
	{
		Reset();
	}

	public void Reset()
	{
		bool dirty = Dirty;
		m_clean = (m_current = 0);
		m_forceDirty = false;
		CheckDirtyChanged(dirty);
	}

	public void Undo()
	{
		Decrement();
	}

	public void Redo()
	{
		Increment(checkCleanState: false);
	}

	public void Increment(bool checkCleanState = true)
	{
		bool dirty = Dirty;
		if (checkCleanState && m_current < m_clean)
		{
			m_clean = -1;
		}
		m_current++;
		CheckDirtyChanged(dirty);
	}

	public void Decrement()
	{
		if (!CanDecrement)
		{
			throw new InvalidOperationException("Can't decrement");
		}
		bool dirty = Dirty;
		m_current--;
		CheckDirtyChanged(dirty);
	}

	public void Revert(CommandHistory history)
	{
		while (m_clean < m_current && history.CanUndo)
		{
			history.Undo();
		}
		while (m_clean > m_current && history.CanRedo)
		{
			history.Redo();
		}
	}

	private void CheckDirtyChanged(bool oldState)
	{
		if (oldState != Dirty)
		{
			this.DirtyChanged.Raise(this, EventArgs.Empty);
		}
	}
}
