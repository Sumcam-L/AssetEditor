using System;

namespace ComponentAce.Compression.Libs.ZLib;

public sealed class ZStream
{
	private const int DEF_WBITS = 15;

	private const int MAX_MEM_LEVEL = 9;

	private byte[] _next_in;

	private int _next_in_index;

	private int _avail_in;

	private long _total_in;

	private byte[] _next_out;

	private int _next_out_index;

	private int _avail_out;

	private long _total_out;

	private string _msg;

	private Deflate _dstate;

	private Inflate _istate;

	private BlockType data_type;

	private long _adler;

	internal long adler
	{
		get
		{
			return _adler;
		}
		set
		{
			_adler = value;
		}
	}

	internal BlockType Data_type
	{
		get
		{
			return data_type;
		}
		set
		{
			data_type = value;
		}
	}

	public byte[] next_in
	{
		get
		{
			return _next_in;
		}
		set
		{
			_next_in = value;
		}
	}

	public int next_in_index
	{
		get
		{
			return _next_in_index;
		}
		set
		{
			_next_in_index = value;
		}
	}

	public int avail_in
	{
		get
		{
			return _avail_in;
		}
		set
		{
			_avail_in = value;
		}
	}

	public long total_in
	{
		get
		{
			return _total_in;
		}
		set
		{
			_total_in = value;
		}
	}

	public byte[] next_out
	{
		get
		{
			return _next_out;
		}
		set
		{
			_next_out = value;
		}
	}

	public int next_out_index
	{
		get
		{
			return _next_out_index;
		}
		set
		{
			_next_out_index = value;
		}
	}

	public int avail_out
	{
		get
		{
			return _avail_out;
		}
		set
		{
			_avail_out = value;
		}
	}

	public long total_out
	{
		get
		{
			return _total_out;
		}
		set
		{
			_total_out = value;
		}
	}

	public string msg
	{
		get
		{
			return _msg;
		}
		set
		{
			_msg = value;
		}
	}

	internal Deflate dstate
	{
		get
		{
			return _dstate;
		}
		set
		{
			_dstate = value;
		}
	}

	internal Inflate istate
	{
		get
		{
			return _istate;
		}
		set
		{
			_istate = value;
		}
	}

	public int inflateInit()
	{
		return inflateInit(15);
	}

	public int inflateInit(int windowBits)
	{
		_istate = new Inflate();
		return _istate.inflateInit(this, windowBits);
	}

	public int inflate(FlushStrategy flush)
	{
		if (_istate == null)
		{
			return -2;
		}
		return _istate.inflate(this, flush);
	}

	public int inflateEnd()
	{
		next_in_index = 0;
		next_out_index = 0;
		if (_istate == null)
		{
			return -2;
		}
		int result = _istate.inflateEnd(this);
		_istate = null;
		return result;
	}

	public int inflateSync()
	{
		if (_istate == null)
		{
			return -2;
		}
		return _istate.inflateSync(this);
	}

	public int inflateSetDictionary(byte[] dictionary, int dictLength)
	{
		if (_istate == null)
		{
			return -2;
		}
		return _istate.inflateSetDictionary(this, dictionary, dictLength);
	}

	public int deflateInit(int level)
	{
		return deflateInit(level, 15);
	}

	public int deflateInit(int level, int bits)
	{
		_dstate = new Deflate();
		return _dstate.deflateInit(this, level, bits);
	}

	public int deflate(FlushStrategy flush)
	{
		if (_dstate == null)
		{
			return -2;
		}
		return _dstate.deflate(this, flush);
	}

	public int deflateEnd()
	{
		next_in_index = 0;
		next_out_index = 0;
		if (_dstate == null)
		{
			return -2;
		}
		int result = _dstate.deflateEnd();
		_dstate = null;
		return result;
	}

	public int deflateParams(int level, CompressionStrategy strategy)
	{
		if (_dstate == null)
		{
			return -2;
		}
		return _dstate.deflateParams(this, level, strategy);
	}

	public int deflateSetDictionary(byte[] dictionary, int dictLength)
	{
		if (_dstate == null)
		{
			return -2;
		}
		return _dstate.deflateSetDictionary(this, dictionary, dictLength);
	}

	public void flush_pending()
	{
		int pending = _dstate.Pending;
		if (pending > _avail_out)
		{
			pending = _avail_out;
		}
		if (pending != 0)
		{
			Array.Copy(_dstate.Pending_buf, _dstate.Pending_out, _next_out, _next_out_index, pending);
			_next_out_index += pending;
			_dstate.Pending_out += pending;
			_total_out += pending;
			_avail_out -= pending;
			_dstate.Pending -= pending;
			if (_dstate.Pending == 0)
			{
				_dstate.Pending_out = 0;
			}
		}
	}

	public int read_buf(byte[] buf, int start, int size)
	{
		int num = _avail_in;
		if (num > size)
		{
			num = size;
		}
		if (num == 0)
		{
			return 0;
		}
		_avail_in -= num;
		if (_dstate.NoHeader == 0)
		{
			adler = Adler32.GetAdler32Checksum(adler, _next_in, _next_in_index, num);
		}
		Array.Copy(_next_in, _next_in_index, buf, start, num);
		_next_in_index += num;
		_total_in += num;
		return num;
	}

	public void free()
	{
		_next_in = null;
		_next_out = null;
		_msg = null;
	}
}
