using System;
using System.IO;
using System.Text;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;

namespace IronPython.Modules;

internal class StringStream
{
	private StringBuilder _data;

	private string _lastValue;

	private int _position;

	public bool EOF => _position >= _data.Length;

	public int Position => _position;

	public string Data
	{
		get
		{
			if (_lastValue == null)
			{
				_lastValue = _data.ToString();
			}
			return _lastValue;
		}
	}

	public string Prefix => _data.ToString(0, _position);

	public StringStream(string data)
	{
		_data = new StringBuilder(_lastValue = data);
		_position = 0;
	}

	public string Read(int i)
	{
		if (_position + i > _data.Length)
		{
			i = _data.Length - _position;
		}
		string result = _data.ToString(_position, i);
		_position += i;
		return result;
	}

	public string ReadLine(int size)
	{
		if (size < 0)
		{
			size = int.MaxValue;
		}
		int num = _position;
		int num2 = 0;
		while (num < _data.Length && num2 < size)
		{
			char c = _data[num];
			if (c == '\n' || c == '\r')
			{
				num++;
				if (c == '\r' && _position < _data.Length && _data[num] == '\n')
				{
					num++;
				}
				string result = _data.ToString(_position, num - _position);
				_position = num;
				return result;
			}
			num++;
			num2++;
		}
		if (num > _position)
		{
			string result2 = _data.ToString(_position, num - _position);
			_position = num;
			return result2;
		}
		return "";
	}

	public string ReadToEnd()
	{
		if (_position < _data.Length)
		{
			string result = _data.ToString(_position, _data.Length - _position);
			_position = _data.Length;
			return result;
		}
		return string.Empty;
	}

	public void Reset()
	{
		_position = 0;
	}

	public int Seek(int offset, SeekOrigin origin)
	{
		switch (origin)
		{
		case SeekOrigin.Begin:
			_position = offset;
			break;
		case SeekOrigin.Current:
			_position += offset;
			break;
		case SeekOrigin.End:
			_position = _data.Length + offset;
			break;
		default:
			throw new ValueErrorException("origin");
		}
		return _position;
	}

	public void Truncate()
	{
		_lastValue = null;
		_data.Length = _position;
	}

	public void Truncate(int size)
	{
		_lastValue = null;
		if (size > _data.Length)
		{
			size = _data.Length;
		}
		else if (size < 0)
		{
			throw PythonOps.IOError("(22, 'Negative size not allowed')");
		}
		_data.Length = size;
		_position = size;
	}

	internal void Write(string s)
	{
		if (_data.Length < _position)
		{
			_data.Length = _position;
		}
		_lastValue = null;
		if (_position == _data.Length)
		{
			_data.Append(s);
		}
		else
		{
			_data.Remove(_position, Math.Min(s.Length, _data.Length - _position));
			_data.Insert(_position, s);
		}
		_position += s.Length;
	}
}
