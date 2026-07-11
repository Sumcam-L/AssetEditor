using System;

namespace IronPython.Runtime.Exceptions;

[Serializable]
[PythonType("traceback")]
public class TraceBack
{
	private readonly TraceBack _next;

	private readonly TraceBackFrame _frame;

	private int _line;

	public TraceBack tb_next => _next;

	public TraceBackFrame tb_frame => _frame;

	public int tb_lineno => _line;

	public int tb_lasti => 0;

	public TraceBack(TraceBack nextTraceBack, TraceBackFrame fromFrame)
	{
		_next = nextTraceBack;
		_frame = fromFrame;
	}

	internal void SetLine(int lineNumber)
	{
		_line = lineNumber;
	}
}
