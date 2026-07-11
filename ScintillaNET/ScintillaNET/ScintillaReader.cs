using System;
using System.IO;

namespace ScintillaNET;

public class ScintillaReader : TextReader
{
	private const int DefaultBufferSize = 256;

	private Scintilla _scintilla;

	private int _bufferSize;

	private string _data;

	private int _dataIndex;

	private int _nextData;

	private int _lastData;

	private int BufferRemaining => (_data != null) ? (_data.Length - _dataIndex) : 0;

	private int UnbufferedRemaining => _lastData - _nextData;

	private int TotalRemaining => BufferRemaining + UnbufferedRemaining;

	public ScintillaReader(Scintilla scintilla)
		: this(scintilla, 0, scintilla.TextLength)
	{
	}

	public ScintillaReader(Scintilla scintilla, int bufferSize)
		: this(scintilla, 0, scintilla.TextLength, bufferSize)
	{
	}

	public ScintillaReader(Scintilla scintilla, int start, int end)
		: this(scintilla, start, end, 256)
	{
	}

	public ScintillaReader(Scintilla scintilla, int start, int end, int bufferSize)
	{
		_scintilla = scintilla;
		_bufferSize = ((bufferSize > 0) ? bufferSize : 256);
		_nextData = start;
		_lastData = end;
		BufferNextRegion();
	}

	public override int Peek()
	{
		return (_data != null) ? _data[_dataIndex] : (-1);
	}

	public override int Read()
	{
		if (_data != null)
		{
			char result = _data[_dataIndex++];
			if (_dataIndex >= _data.Length)
			{
				BufferNextRegion();
			}
			return result;
		}
		return -1;
	}

	public override int Read(char[] buffer, int index, int count)
	{
		return ReadBlock(buffer, index, count);
	}

	public override int ReadBlock(char[] buffer, int index, int count)
	{
		if (_data != null)
		{
			int bufferRemaining = BufferRemaining;
			if (count < bufferRemaining)
			{
				_data.CopyTo(_dataIndex, buffer, index, count);
				return count;
			}
			_data.CopyTo(_dataIndex, buffer, index, bufferRemaining);
			if (count > bufferRemaining)
			{
				string textRange = _scintilla.GetTextRange(_nextData, Math.Min(count - bufferRemaining, UnbufferedRemaining));
				textRange.CopyTo(0, buffer, index + bufferRemaining, textRange.Length);
				count = bufferRemaining + textRange.Length;
				_nextData += textRange.Length;
			}
			BufferNextRegion();
			return count;
		}
		return 0;
	}

	private void BufferNextRegion()
	{
		if (_nextData < _lastData)
		{
			int length = Math.Min(_lastData - _nextData, _bufferSize);
			_data = _scintilla.GetTextRange(_nextData, length);
			_nextData += _data.Length;
			_dataIndex = 0;
		}
		else
		{
			_data = null;
		}
	}
}
