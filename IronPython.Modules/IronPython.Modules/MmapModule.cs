using System;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class MmapModule
{
	[PythonType]
	public class mmap
	{
		private struct MmapLocker : IDisposable
		{
			private readonly mmap _mmap;

			public MmapLocker(mmap mmap)
			{
				_mmap = mmap;
				Interlocked.Increment(ref _mmap._refCount);
				_mmap.EnsureOpen();
			}

			public void Dispose()
			{
				_mmap.CloseWorker();
			}
		}

		private MemoryMappedFile _file;

		private MemoryMappedViewAccessor _view;

		private long _position;

		private FileStream _sourceStream;

		private readonly long _offset;

		private readonly string _mapName;

		private readonly MemoryMappedFileAccess _fileAccess;

		private volatile bool _isClosed;

		private int _refCount = 1;

		public string this[long index]
		{
			get
			{
				using (new MmapLocker(this))
				{
					CheckIndex(index);
					return ((char)_view.ReadByte(index)).ToString();
				}
			}
			set
			{
				using (new MmapLocker(this))
				{
					if (value == null || value.Length != 1)
					{
						throw PythonOps.IndexError("mmap assignment must be a single-character string");
					}
					EnsureWritable();
					CheckIndex(index);
					_view.Write(index, (byte)value[0]);
				}
			}
		}

		public string this[Slice slice]
		{
			get
			{
				using (new MmapLocker(this))
				{
					PythonOps.FixSlice(_view.Capacity, GetLong(slice.start), GetLong(slice.stop), GetLong(slice.step), out var ostart, out var _, out var ostep, out var ocount);
					int num = (int)ocount;
					if (num == 0)
					{
						return "";
					}
					StringBuilder stringBuilder = new StringBuilder(num);
					while (num > 0)
					{
						stringBuilder.Append((char)_view.ReadByte(ostart));
						ostart += ostep;
						num--;
					}
					return stringBuilder.ToString();
				}
			}
			set
			{
				using (new MmapLocker(this))
				{
					if (value == null)
					{
						throw PythonOps.TypeError("mmap slice assignment must be a string");
					}
					EnsureWritable();
					PythonOps.FixSlice(_view.Capacity, GetLong(slice.start), GetLong(slice.stop), GetLong(slice.step), out var ostart, out var _, out var ostep, out var ocount);
					int num = (int)ocount;
					if (value.Length != num)
					{
						throw PythonOps.IndexError("mmap slice assignment is wrong size");
					}
					if (num == 0)
					{
						return;
					}
					byte[] array = value.MakeByteArray();
					if (ostep == 1)
					{
						_view.WriteArray(ostart, array, 0, value.Length);
						return;
					}
					byte[] array2 = array;
					foreach (byte value2 in array2)
					{
						_view.Write(ostart, value2);
						ostart += ostep;
					}
				}
			}
		}

		private long Position
		{
			get
			{
				return Interlocked.Read(ref _position);
			}
			set
			{
				Interlocked.Exchange(ref _position, value);
			}
		}

		public mmap(CodeContext context, int fileno, long length, [DefaultParameterValue(null)] string tagname, [DefaultParameterValue(2)] int access, [DefaultParameterValue(0L)] long offset)
		{
			switch (access)
			{
			case 1:
				_fileAccess = MemoryMappedFileAccess.Read;
				break;
			case 2:
				_fileAccess = MemoryMappedFileAccess.ReadWrite;
				break;
			case 3:
				_fileAccess = MemoryMappedFileAccess.CopyOnWrite;
				break;
			default:
				throw PythonOps.ValueError("mmap invalid access parameter");
			}
			if (length < 0)
			{
				throw PythonOps.OverflowError("memory mapped size must be positive");
			}
			if (offset < 0)
			{
				throw PythonOps.OverflowError("memory mapped offset must be positive");
			}
			if (offset % ALLOCATIONGRANULARITY != 0)
			{
				throw WindowsError(1132);
			}
			_mapName = ((tagname == "") ? null : tagname);
			if (fileno == -1 || fileno == 0)
			{
				_offset = 0L;
				_sourceStream = null;
				if (_mapName == null)
				{
					_mapName = Guid.NewGuid().ToString();
				}
				_file = MemoryMappedFile.CreateOrOpen(_mapName, length, _fileAccess);
			}
			else
			{
				long num = checked(_offset + length);
				_offset = offset;
				PythonContext context2 = PythonContext.GetContext(context);
				if (!context2.FileManager.TryGetFileFromId(context2, fileno, out var pf))
				{
					throw Error(context, 9, "Bad file descriptor");
				}
				if ((_sourceStream = pf._stream as FileStream) == null)
				{
					throw WindowsError(6);
				}
				if (_fileAccess == MemoryMappedFileAccess.ReadWrite && !_sourceStream.CanWrite)
				{
					throw WindowsError(5);
				}
				if (num > _sourceStream.Length)
				{
					if (!_sourceStream.CanWrite)
					{
						throw WindowsError(8);
					}
					_sourceStream.SetLength(num);
				}
				_file = MemoryMappedFile.CreateFromFile(_sourceStream, _mapName, _sourceStream.Length, _fileAccess, null, HandleInheritability.None, leaveOpen: true);
			}
			_view = _file.CreateViewAccessor(_offset, length, _fileAccess);
			_position = 0L;
		}

		public object __len__()
		{
			using (new MmapLocker(this))
			{
				return ReturnLong(_view.Capacity);
			}
		}

		public void __delitem__(long index)
		{
			using (new MmapLocker(this))
			{
				CheckIndex(index);
				throw PythonOps.TypeError("mmap object doesn't support item deletion");
			}
		}

		public void __delslice__(Slice slice)
		{
			using (new MmapLocker(this))
			{
				throw PythonOps.TypeError("mmap object doesn't support slice deletion");
			}
		}

		public void close()
		{
			if (_isClosed)
			{
				return;
			}
			lock (this)
			{
				if (!_isClosed)
				{
					_isClosed = true;
					CloseWorker();
				}
			}
		}

		private void CloseWorker()
		{
			if (Interlocked.Decrement(ref _refCount) == 0)
			{
				_view.Flush();
				_view.Dispose();
				_file.Dispose();
				_sourceStream = null;
				_view = null;
				_file = null;
			}
		}

		public object find([NotNull] string s)
		{
			using (new MmapLocker(this))
			{
				return FindWorker(s, Position, _view.Capacity);
			}
		}

		public object find([NotNull] string s, long start)
		{
			using (new MmapLocker(this))
			{
				return FindWorker(s, start, _view.Capacity);
			}
		}

		public object find([NotNull] string s, long start, long end)
		{
			using (new MmapLocker(this))
			{
				return FindWorker(s, start, end);
			}
		}

		private object FindWorker(string s, long start, long end)
		{
			ContractUtils.RequiresNotNull(s, "s");
			start = PythonOps.FixSliceIndex(start, _view.Capacity);
			end = PythonOps.FixSliceIndex(end, _view.Capacity);
			if (s == "")
			{
				if (start > end)
				{
					return -1;
				}
				return ReturnLong(start);
			}
			long num = end - start;
			if (s.Length > num)
			{
				return -1;
			}
			int num2 = -1;
			int num3 = Math.Max(s.Length, PAGESIZE);
			CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
			if (num <= num3 * 2)
			{
				byte[] array = new byte[num];
				_view.ReadArray(start, array, 0, (int)num);
				string source = array.MakeString();
				num2 = compareInfo.IndexOf(source, s, CompareOptions.Ordinal);
			}
			else
			{
				byte[] array2 = new byte[num3];
				byte[] array3 = new byte[num3];
				_view.ReadArray(start, array2, 0, num3);
				int num4 = _view.ReadArray(start + num3, array3, 0, num3);
				start += num3 * 2;
				num -= num3 * 2;
				while (num > 0 && num4 > 0)
				{
					string source2 = GetString(array2, array3, num4);
					num2 = compareInfo.IndexOf(source2, s, CompareOptions.Ordinal);
					if (num2 != -1)
					{
						return ReturnLong(start - 2 * num3 + num2);
					}
					byte[] array4 = array2;
					array2 = array3;
					array3 = array4;
					int count = (int)((num < num3) ? num : num3);
					num -= num4;
					num4 = _view.ReadArray(start, array3, 0, count);
					start += num4;
				}
			}
			if (num2 != -1)
			{
				return ReturnLong(start + num2);
			}
			return -1;
		}

		public int flush()
		{
			using (new MmapLocker(this))
			{
				_view.Flush();
				return 1;
			}
		}

		public int flush(long offset, long size)
		{
			using (new MmapLocker(this))
			{
				CheckIndex(offset, inclusive: false);
				CheckIndex(checked(offset + size), inclusive: false);
				_view.Flush();
				return 1;
			}
		}

		public void move(long dest, long src, long count)
		{
			using (new MmapLocker(this))
			{
				EnsureWritable();
				if (dest < 0 || src < 0 || count < 0 || checked(Math.Max(src, dest) + count) > _view.Capacity)
				{
					throw PythonOps.ValueError("source or destination out of range");
				}
				if (src == dest || count == 0)
				{
					return;
				}
				if (count <= PAGESIZE)
				{
					byte[] buffer = new byte[count];
					MoveWorker(buffer, src, dest, (int)count);
					return;
				}
				if (src < dest)
				{
					byte[] buffer2 = new byte[PAGESIZE];
					while (count >= PAGESIZE)
					{
						MoveWorker(buffer2, src, dest, PAGESIZE);
						src += PAGESIZE;
						dest += PAGESIZE;
						count -= PAGESIZE;
					}
					if (count > 0)
					{
						MoveWorker(buffer2, src, dest, (int)count);
					}
					return;
				}
				byte[] buffer3 = new byte[PAGESIZE];
				src += count;
				dest += count;
				int num = (int)(count % PAGESIZE);
				if (num != 0)
				{
					src -= num;
					dest -= num;
					count -= num;
					MoveWorker(buffer3, src, dest, num);
				}
				while (count > 0)
				{
					src -= PAGESIZE;
					dest -= PAGESIZE;
					count -= PAGESIZE;
					MoveWorker(buffer3, src, dest, PAGESIZE);
				}
			}
		}

		private void MoveWorker(byte[] buffer, long src, long dest, int count)
		{
			_view.ReadArray(src, buffer, 0, count);
			_view.WriteArray(dest, buffer, 0, count);
		}

		public string read(int len)
		{
			using (new MmapLocker(this))
			{
				long position = Position;
				if (len < 0)
				{
					len = checked((int)(_view.Capacity - position));
				}
				else if (len > _view.Capacity - position)
				{
					len = checked((int)(_view.Capacity - position));
				}
				if (len == 0)
				{
					return "";
				}
				byte[] array = new byte[len];
				len = _view.ReadArray(position, array, 0, len);
				Position = position + len;
				return array.MakeString(len);
			}
		}

		public string read_byte()
		{
			using (new MmapLocker(this))
			{
				long position = Position;
				if (position >= _view.Capacity)
				{
					throw PythonOps.ValueError("read byte out of range");
				}
				byte b = _view.ReadByte(position);
				Position = position + 1;
				char c = (char)b;
				return c.ToString();
			}
		}

		public string readline()
		{
			using (new MmapLocker(this))
			{
				StringBuilder stringBuilder = new StringBuilder();
				long num = Position;
				char c = '\0';
				while (c != '\n' && num < _view.Capacity)
				{
					c = (char)_view.ReadByte(num);
					stringBuilder.Append(c);
					num++;
				}
				Position = num;
				return stringBuilder.ToString();
			}
		}

		public void resize(long newsize)
		{
			using (new MmapLocker(this))
			{
				if (_fileAccess != MemoryMappedFileAccess.ReadWrite)
				{
					throw PythonOps.TypeError("mmap can't resize a readonly or copy-on-write memory map.");
				}
				if (_sourceStream == null)
				{
					throw WindowsError(87);
				}
				if (newsize == 0)
				{
					throw WindowsError((_offset == 0) ? 5 : 1006);
				}
				if (_view.Capacity == newsize)
				{
					return;
				}
				long num = checked(_offset + newsize);
				try
				{
					_view.Flush();
					_view.Dispose();
					_file.Dispose();
					if (!_sourceStream.CanWrite)
					{
						_sourceStream = new FileStream(_sourceStream.Name, FileMode.OpenOrCreate, FileAccess.ReadWrite);
					}
					if (num != _sourceStream.Length)
					{
						_sourceStream.SetLength(num);
					}
					_file = MemoryMappedFile.CreateFromFile(_sourceStream, _mapName, _sourceStream.Length, _fileAccess, null, HandleInheritability.None, leaveOpen: true);
					_view = _file.CreateViewAccessor(_offset, newsize, _fileAccess);
				}
				catch
				{
					close();
					throw;
				}
			}
		}

		public object rfind([NotNull] string s)
		{
			using (new MmapLocker(this))
			{
				return RFindWorker(s, Position, _view.Capacity);
			}
		}

		public object rfind([NotNull] string s, long start)
		{
			using (new MmapLocker(this))
			{
				return RFindWorker(s, start, _view.Capacity);
			}
		}

		public object rfind([NotNull] string s, long start, long end)
		{
			using (new MmapLocker(this))
			{
				return RFindWorker(s, start, end);
			}
		}

		private object RFindWorker(string s, long start, long end)
		{
			ContractUtils.RequiresNotNull(s, "s");
			start = PythonOps.FixSliceIndex(start, _view.Capacity);
			end = PythonOps.FixSliceIndex(end, _view.Capacity);
			if (s == "")
			{
				if (start > end)
				{
					return -1;
				}
				return ReturnLong(start);
			}
			long num = end - start;
			if (s.Length > num)
			{
				return -1;
			}
			int num2 = -1;
			int num3 = Math.Max(s.Length, PAGESIZE);
			CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
			if (num <= num3 * 2)
			{
				byte[] array = new byte[num];
				num = _view.ReadArray(start, array, 0, (int)num);
				string source = array.MakeString();
				num2 = compareInfo.LastIndexOf(source, s, CompareOptions.Ordinal);
			}
			else
			{
				byte[] array2 = new byte[num3];
				byte[] array3 = new byte[num3];
				int num4 = (int)((end - start) % num3);
				if (num4 == 0)
				{
					num4 = num3;
				}
				start = end - num3 - num4;
				num -= num3 + num4;
				_view.ReadArray(start, array2, 0, num3);
				int num5 = _view.ReadArray(start + num3, array3, 0, num4);
				while (num >= 0)
				{
					string source2 = GetString(array2, array3, num5);
					num2 = compareInfo.LastIndexOf(source2, s, CompareOptions.Ordinal);
					if (num2 != -1)
					{
						return ReturnLong(num2 + start);
					}
					byte[] array4 = array2;
					array2 = array3;
					array3 = array4;
					start -= num3;
					num5 = _view.ReadArray(start, array2, 0, num3);
					num -= num5;
				}
			}
			if (num2 != -1)
			{
				return ReturnLong(num2 + start);
			}
			return -1;
		}

		public void seek(long pos, [DefaultParameterValue(0)] int whence)
		{
			checked
			{
				using (new MmapLocker(this))
				{
					switch (whence)
					{
					case 1:
						pos += Position;
						break;
					case 2:
						pos += _view.Capacity;
						break;
					default:
						throw PythonOps.ValueError("unknown seek type");
					case 0:
						break;
					}
					CheckSeekIndex(pos);
					Position = pos;
				}
			}
		}

		public object size()
		{
			using (new MmapLocker(this))
			{
				return ReturnLong(_offset + _view.Capacity);
			}
		}

		public object tell()
		{
			using (new MmapLocker(this))
			{
				return ReturnLong(Position);
			}
		}

		public void write(string s)
		{
			using (new MmapLocker(this))
			{
				EnsureWritable();
				long position = Position;
				if (_view.Capacity - position < s.Length)
				{
					throw PythonOps.ValueError("data out of range");
				}
				byte[] array = s.MakeByteArray();
				_view.WriteArray(position, array, 0, s.Length);
				Position = position + s.Length;
			}
		}

		public void write_byte(string s)
		{
			using (new MmapLocker(this))
			{
				if (s.Length != 1)
				{
					throw PythonOps.TypeError("write_byte() argument 1 must be char, not str");
				}
				EnsureWritable();
				long position = Position;
				if (Position >= _view.Capacity)
				{
					throw PythonOps.ValueError("write byte out of range");
				}
				_view.Write(position, (byte)s[0]);
				Position = position + 1;
			}
		}

		private void EnsureWritable()
		{
			if (_fileAccess == MemoryMappedFileAccess.Read)
			{
				throw PythonOps.TypeError("mmap can't modify a read-only memory map.");
			}
		}

		private void CheckIndex(long index)
		{
			CheckIndex(index, inclusive: true);
		}

		private void CheckIndex(long index, bool inclusive)
		{
			if (index > _view.Capacity || index < 0 || (inclusive && index == _view.Capacity))
			{
				throw PythonOps.IndexError("mmap index out of range");
			}
		}

		private void CheckSeekIndex(long index)
		{
			if (index > _view.Capacity || index < 0)
			{
				throw PythonOps.ValueError("seek out of range");
			}
		}

		private static long? GetLong(object o)
		{
			if (o == null)
			{
				return null;
			}
			if (o is int)
			{
				return (int)o;
			}
			if (o is BigInteger)
			{
				return (long)(BigInteger)o;
			}
			if (o is long)
			{
				return (long)o;
			}
			return (long)Converter.ConvertToBigInteger(o);
		}

		private static object ReturnLong(long l)
		{
			if (l <= int.MaxValue && l >= int.MinValue)
			{
				return (int)l;
			}
			return (BigInteger)l;
		}

		private static string GetString(byte[] buffer0, byte[] buffer1, int length1)
		{
			StringBuilder stringBuilder = new StringBuilder(buffer0.Length + length1);
			foreach (byte value in buffer0)
			{
				stringBuilder.Append((char)value);
			}
			for (int j = 0; j < length1; j++)
			{
				stringBuilder.Append((char)buffer1[j]);
			}
			return stringBuilder.ToString();
		}

		internal string GetSearchString()
		{
			using (new MmapLocker(this))
			{
				return this[new Slice(0, null)];
			}
		}

		private void EnsureOpen()
		{
			if (_isClosed)
			{
				throw PythonOps.ValueError("mmap closed or invalid");
			}
		}
	}

	private struct SYSTEM_INFO
	{
		internal int dwOemId;

		internal int dwPageSize;

		internal IntPtr lpMinimumApplicationAddress;

		internal IntPtr lpMaximumApplicationAddress;

		internal IntPtr dwActiveProcessorMask;

		internal int dwNumberOfProcessors;

		internal int dwProcessorType;

		internal int dwAllocationGranularity;

		internal short wProcessorLevel;

		internal short wProcessorRevision;
	}

	public const int ACCESS_READ = 1;

	public const int ACCESS_WRITE = 2;

	public const int ACCESS_COPY = 3;

	private const int SEEK_SET = 0;

	private const int SEEK_CUR = 1;

	private const int SEEK_END = 2;

	public static readonly int ALLOCATIONGRANULARITY = GetAllocationGranularity();

	public static readonly int PAGESIZE = Environment.SystemPageSize;

	private static readonly object _mmapErrorKey = new object();

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		context.EnsureModuleException(_mmapErrorKey, PythonExceptions.EnvironmentError, dict, "error", "mmap");
	}

	private static Exception Error(CodeContext context, string message)
	{
		return PythonExceptions.CreateThrowable((PythonType)PythonContext.GetContext(context).GetModuleState(_mmapErrorKey), message);
	}

	private static Exception Error(CodeContext context, int errno, string message)
	{
		return PythonExceptions.CreateThrowable((PythonType)PythonContext.GetContext(context).GetModuleState(_mmapErrorKey), errno, message);
	}

	private static Exception WindowsError(int code)
	{
		string text = CTypes.FormatError(code);
		return PythonExceptions.CreateThrowable(PythonExceptions.WindowsError, code, text);
	}

	[DllImport("kernel32", SetLastError = true)]
	private static extern void GetSystemInfo(ref SYSTEM_INFO lpSystemInfo);

	private static int GetAllocationGranularity()
	{
		try
		{
			return GetAllocationGranularityWorker();
		}
		catch
		{
			return Environment.SystemPageSize;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static int GetAllocationGranularityWorker()
	{
		SYSTEM_INFO lpSystemInfo = default(SYSTEM_INFO);
		GetSystemInfo(ref lpSystemInfo);
		return lpSystemInfo.dwAllocationGranularity;
	}
}
