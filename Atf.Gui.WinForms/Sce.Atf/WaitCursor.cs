using System;
using System.Windows.Forms;

namespace Sce.Atf;

public class WaitCursor : IDisposable
{
	private readonly Cursor m_oldCursor;

	public WaitCursor()
	{
		m_oldCursor = Cursor.Current;
		Cursor.Current = Cursors.WaitCursor;
	}

	public void Dispose()
	{
		Cursor.Current = m_oldCursor;
	}
}
