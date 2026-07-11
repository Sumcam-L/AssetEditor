using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class PythonPickle
{
	internal abstract class FileInput
	{
		public abstract string Read(CodeContext context, int size);

		public abstract string ReadLine(CodeContext context);

		public virtual string ReadLineNoNewLine(CodeContext context)
		{
			string text = ReadLine(context);
			return text.Substring(0, text.Length - 1);
		}

		public virtual char ReadChar(CodeContext context)
		{
			string text = Read(context, 1);
			if (text.Length < 1)
			{
				throw PythonOps.EofError("unexpected EOF while unpickling");
			}
			return text[0];
		}

		public virtual int ReadInt(CodeContext context)
		{
			return (int)(ReadChar(context) | ((uint)ReadChar(context) << 8) | ((uint)ReadChar(context) << 16) | ((uint)ReadChar(context) << 24));
		}
	}

	internal abstract class FileOutput
	{
		private readonly char[] int32chars = new char[4];

		public abstract void Write(CodeContext context, string data);

		public virtual void Write(CodeContext context, int data)
		{
			int32chars[0] = (char)(data & 0xFF);
			int32chars[1] = (char)((data >> 8) & 0xFF);
			int32chars[2] = (char)((data >> 16) & 0xFF);
			int32chars[3] = (char)((data >> 24) & 0xFF);
			Write(context, new string(int32chars));
		}

		public virtual void Write(CodeContext context, char data)
		{
			Write(context, ScriptingRuntimeHelpers.CharToString(data));
		}
	}

	private class PythonFileInput : FileInput
	{
		private object _readMethod;

		private object _readLineMethod;

		public PythonFileInput(CodeContext context, object file)
		{
			if (!PythonOps.TryGetBoundAttr(context, file, "read", out _readMethod) || !PythonOps.IsCallable(context, _readMethod) || !PythonOps.TryGetBoundAttr(context, file, "readline", out _readLineMethod) || !PythonOps.IsCallable(context, _readLineMethod))
			{
				throw PythonOps.TypeError("argument must have callable 'read' and 'readline' attributes");
			}
		}

		public override string Read(CodeContext context, int size)
		{
			return Converter.ConvertToString(PythonCalls.Call(context, _readMethod, size));
		}

		public override string ReadLine(CodeContext context)
		{
			return Converter.ConvertToString(PythonCalls.Call(context, _readLineMethod));
		}
	}

	internal class PythonStringInput : FileInput
	{
		private readonly string _data;

		private int _offset;

		public PythonStringInput(string data)
		{
			_data = data;
		}

		public override string Read(CodeContext context, int size)
		{
			string result = _data.Substring(_offset, size);
			_offset += size;
			return result;
		}

		public override string ReadLine(CodeContext context)
		{
			return ReadLineWorker(includeNewLine: true);
		}

		public override string ReadLineNoNewLine(CodeContext context)
		{
			return ReadLineWorker(includeNewLine: false);
		}

		public override char ReadChar(CodeContext context)
		{
			if (_offset < _data.Length)
			{
				return _data[_offset++];
			}
			throw PythonOps.EofError("unexpected EOF while unpickling");
		}

		public override int ReadInt(CodeContext context)
		{
			if (_offset + 4 <= _data.Length)
			{
				int result = (int)(_data[_offset] | ((uint)_data[_offset + 1] << 8) | ((uint)_data[_offset + 2] << 16) | ((uint)_data[_offset + 3] << 24));
				_offset += 4;
				return result;
			}
			throw PythonOps.EofError("unexpected EOF while unpickling");
		}

		private string ReadLineWorker(bool includeNewLine)
		{
			string result;
			for (int i = _offset; i < _data.Length; i++)
			{
				if (_data[i] == '\n')
				{
					result = _data.Substring(_offset, i - _offset + (includeNewLine ? 1 : 0));
					_offset = i + 1;
					return result;
				}
			}
			result = _data.Substring(_offset);
			_offset = _data.Length;
			return result;
		}
	}

	private class PythonFileLikeOutput : FileOutput
	{
		private object _writeMethod;

		public PythonFileLikeOutput(CodeContext context, object file)
		{
			if (!PythonOps.TryGetBoundAttr(context, file, "write", out _writeMethod) || !PythonOps.IsCallable(context, _writeMethod))
			{
				throw PythonOps.TypeError("argument must have callable 'write' attribute");
			}
		}

		public override void Write(CodeContext context, string data)
		{
			PythonCalls.Call(context, _writeMethod, data);
		}
	}

	private class PythonFileOutput : FileOutput
	{
		private readonly PythonFile _file;

		public PythonFileOutput(PythonFile file)
		{
			_file = file;
		}

		public override void Write(CodeContext context, string data)
		{
			_file.write(data);
		}
	}

	private class StringBuilderOutput : FileOutput
	{
		private readonly StringBuilder _builder = new StringBuilder(4096);

		public string GetString()
		{
			return _builder.ToString();
		}

		public override void Write(CodeContext context, char data)
		{
			_builder.Append(data);
		}

		public override void Write(CodeContext context, int data)
		{
			_builder.Append((char)(data & 0xFF));
			_builder.Append((char)((data >> 8) & 0xFF));
			_builder.Append((char)((data >> 16) & 0xFF));
			_builder.Append((char)((data >> 24) & 0xFF));
		}

		public override void Write(CodeContext context, string data)
		{
			_builder.Append(data);
		}
	}

	private class PythonReadableFileOutput : PythonFileLikeOutput
	{
		private object _getValueMethod;

		public PythonReadableFileOutput(CodeContext context, object file)
			: base(context, file)
		{
			if (!PythonOps.TryGetBoundAttr(context, file, "getvalue", out _getValueMethod) || !PythonOps.IsCallable(context, _getValueMethod))
			{
				throw PythonOps.TypeError("argument must have callable 'getvalue' attribute");
			}
		}

		public object GetValue(CodeContext context)
		{
			return PythonCalls.Call(context, _getValueMethod);
		}
	}

	internal static class Opcode
	{
		public const char Append = 'a';

		public const char Appends = 'e';

		public const char BinFloat = 'G';

		public const char BinGet = 'h';

		public const char BinInt = 'J';

		public const char BinInt1 = 'K';

		public const char BinInt2 = 'M';

		public const char BinPersid = 'Q';

		public const char BinPut = 'q';

		public const char BinString = 'T';

		public const char BinUnicode = 'X';

		public const char Build = 'b';

		public const char Dict = 'd';

		public const char Dup = '2';

		public const char EmptyDict = '}';

		public const char EmptyList = ']';

		public const char EmptyTuple = ')';

		public const char Ext1 = '\u0082';

		public const char Ext2 = '\u0083';

		public const char Ext4 = '\u0084';

		public const char Float = 'F';

		public const char Get = 'g';

		public const char Global = 'c';

		public const char Inst = 'i';

		public const char Int = 'I';

		public const char List = 'l';

		public const char Long = 'L';

		public const char Long1 = '\u008a';

		public const char Long4 = '\u008b';

		public const char LongBinGet = 'j';

		public const char LongBinPut = 'r';

		public const char Mark = '(';

		public const char NewFalse = '\u0089';

		public const char NewObj = '\u0081';

		public const char NewTrue = '\u0088';

		public const char NoneValue = 'N';

		public const char Obj = 'o';

		public const char PersId = 'P';

		public const char Pop = '0';

		public const char PopMark = '1';

		public const char Proto = '\u0080';

		public const char Put = 'p';

		public const char Reduce = 'R';

		public const char SetItem = 's';

		public const char SetItems = 'u';

		public const char ShortBinstring = 'U';

		public const char Stop = '.';

		public const char String = 'S';

		public const char Tuple = 't';

		public const char Tuple1 = '\u0085';

		public const char Tuple2 = '\u0086';

		public const char Tuple3 = '\u0087';

		public const char Unicode = 'V';
	}

	[Documentation("Pickler(file, protocol=0) -> Pickler object\n\nA Pickler object serializes Python objects to a pickle bytecode stream, which\ncan then be converted back into equivalent objects using an Unpickler.\n\nfile: an object (such as an open file) that has a write(string) method.\nprotocol: if omitted, protocol 0 is used. If HIGHEST_PROTOCOL or a negative\n    number, the highest available protocol is used.\nbin: (deprecated; use protocol instead) for backwards compability, a 'bin'\n    keyword parameter is supported. When protocol is specified it is ignored.\n    If protocol is not specified, then protocol 0 is used if bin is false, and\n    protocol 1 is used if bin is true.")]
	[PythonType("Pickler")]
	[PythonHidden]
	public class PicklerObject
	{
		private delegate void PickleFunction(PicklerObject pickler, CodeContext context, object value);

		private const char LowestPrintableChar = ' ';

		private const char HighestPrintableChar = '~';

		private static readonly Dictionary<Type, PickleFunction> _dispatchTable;

		private int _batchSize = 1000;

		private FileOutput _file;

		private int _protocol;

		private PythonDictionary _memo;

		private Dictionary<object, int> _privMemo;

		private object _persist_id;

		private static readonly BigInteger MaxInt;

		private static readonly BigInteger MinInt;

		public PythonDictionary memo
		{
			get
			{
				if (_memo == null)
				{
					PythonDictionary pythonDictionary = new PythonDictionary();
					foreach (KeyValuePair<object, int> item in _privMemo)
					{
						pythonDictionary._storage.AddNoLock(ref pythonDictionary._storage, Builtin.id(item.Key), PythonTuple.MakeTuple(item.Value, item.Key));
					}
					_memo = pythonDictionary;
				}
				return _memo;
			}
			set
			{
				_memo = value;
				_privMemo = null;
			}
		}

		public int proto
		{
			get
			{
				return _protocol;
			}
			set
			{
				_protocol = value;
			}
		}

		public int _BATCHSIZE
		{
			get
			{
				return _batchSize;
			}
			set
			{
				_batchSize = value;
			}
		}

		public object persistent_id
		{
			get
			{
				return _persist_id;
			}
			set
			{
				_persist_id = value;
			}
		}

		public int binary
		{
			get
			{
				if (_protocol != 0)
				{
					return 0;
				}
				return 1;
			}
			set
			{
				_protocol = value;
			}
		}

		public int fast
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		static PicklerObject()
		{
			MaxInt = new BigInteger(int.MaxValue);
			MinInt = new BigInteger(int.MinValue);
			_dispatchTable = new Dictionary<Type, PickleFunction>();
			_dispatchTable[typeof(PythonDictionary)] = SaveDict;
			_dispatchTable[typeof(PythonTuple)] = SaveTuple;
			_dispatchTable[typeof(List)] = SaveList;
			_dispatchTable[typeof(OldClass)] = SaveGlobal;
			_dispatchTable[typeof(PythonFunction)] = SaveGlobal;
			_dispatchTable[typeof(BuiltinFunction)] = SaveGlobal;
			_dispatchTable[typeof(PythonType)] = SaveGlobal;
			_dispatchTable[typeof(OldInstance)] = SaveInstance;
		}

		public PicklerObject(CodeContext context, object file, object protocol, object bin)
		{
			int result;
			if (file == null)
			{
				_file = new PythonReadableFileOutput(context, new PythonStringIO.StringO());
			}
			else if (Converter.TryConvertToInt32(file, out result))
			{
				_file = new PythonReadableFileOutput(context, new PythonStringIO.StringO());
				protocol = file;
			}
			else if (file is PythonFile)
			{
				_file = new PythonFileOutput((PythonFile)file);
			}
			else if (file is FileOutput)
			{
				_file = (FileOutput)file;
			}
			else
			{
				_file = new PythonFileLikeOutput(context, file);
			}
			_privMemo = new Dictionary<object, int>(256, ReferenceEqualityComparer.Instance);
			if (protocol == null)
			{
				protocol = (PythonOps.IsTrue(bin) ? 1 : 0);
			}
			result = context.LanguageContext.ConvertToInt32(protocol);
			if (result > 2)
			{
				throw PythonOps.ValueError("pickle protocol {0} asked for; the highest available protocol is {1}", result, 2);
			}
			if (result < 0)
			{
				_protocol = 2;
			}
			else
			{
				_protocol = result;
			}
		}

		[Documentation("dump(obj) -> None\n\nPickle obj and write the result to the file object that was passed to the\nconstructor\n.\nNote that you may call dump() multiple times to pickle multiple objects. To\nunpickle the stream, you will need to call Unpickler's load() method a\ncorresponding number of times.\n\nThe first time a particular object is encountered, it will be pickled normally.\nIf the object is encountered again (in the same or a later dump() call), a\nreference to the previously generated value will be pickled. Unpickling will\nthen create multiple references to a single object.")]
		public void dump(CodeContext context, object obj)
		{
			if (_protocol >= 2)
			{
				WriteProto(context);
			}
			Save(context, obj);
			Write(context, '.');
		}

		[Documentation("clear_memo() -> None\n\nClear the memo, which is used internally by the pickler to keep track of which\nobjects have already been pickled (so that shared or recursive objects are\npickled only once).")]
		public void clear_memo()
		{
			if (_memo != null)
			{
				_memo.Clear();
			}
			else
			{
				_privMemo.Clear();
			}
		}

		private void Memoize(object obj)
		{
			if (_memo != null)
			{
				if (!MemoContains(PythonOps.Id(obj)))
				{
					_memo[PythonOps.Id(obj)] = PythonTuple.MakeTuple(_memo.Count, obj);
				}
			}
			else if (!_privMemo.ContainsKey(obj))
			{
				_privMemo[obj] = _privMemo.Count;
			}
		}

		private int MemoizeNew(object obj)
		{
			int result;
			if (_memo == null)
			{
				result = (_privMemo[obj] = _privMemo.Count);
			}
			else
			{
				_memo[PythonOps.Id(obj)] = PythonTuple.MakeTuple(result = _memo.Count, obj);
			}
			return result;
		}

		private bool MemoContains(object obj)
		{
			if (_memo != null)
			{
				return _memo.Contains(PythonOps.Id(obj));
			}
			return _privMemo.ContainsKey(obj);
		}

		private bool TryWriteFastGet(CodeContext context, object obj)
		{
			if (_memo != null)
			{
				return TryWriteSlowGet(context, obj);
			}
			if (_privMemo.TryGetValue(obj, out var value))
			{
				WriteGetOrPut(context, isGet: true, value);
				return true;
			}
			return false;
		}

		private bool TryWriteSlowGet(CodeContext context, object obj)
		{
			if (_memo.TryGetValue(obj, out var value))
			{
				WriteGetOrPut(context, isGet: true, (PythonTuple)value);
				return true;
			}
			return false;
		}

		[Documentation("getvalue() -> string\n\nReturn the value of the internal string. Raises PicklingError if a file object\nwas passed to this pickler's constructor.")]
		public object getvalue(CodeContext context)
		{
			if (_file is PythonReadableFileOutput)
			{
				return ((PythonReadableFileOutput)_file).GetValue(context);
			}
			throw PythonExceptions.CreateThrowable(PicklingError(context), "Attempt to getvalue() a non-list-based pickler");
		}

		private void Save(CodeContext context, object obj)
		{
			if (_persist_id != null && TrySavePersistId(context, obj))
			{
				return;
			}
			if (obj == null)
			{
				SaveNone(this, context, obj);
			}
			else if (obj is int)
			{
				SaveInteger(this, context, obj);
			}
			else if (obj is BigInteger)
			{
				SaveLong(this, context, obj);
			}
			else if (obj is bool)
			{
				SaveBoolean(this, context, obj);
			}
			else if (obj is double)
			{
				SaveFloat(this, context, obj);
			}
			else
			{
				if (TryWriteFastGet(context, obj))
				{
					return;
				}
				if (obj is string)
				{
					SaveUnicode(this, context, obj);
					return;
				}
				if (!_dispatchTable.TryGetValue(obj.GetType(), out var value))
				{
					value = ((!(obj is PythonType)) ? new PickleFunction(SaveObject) : new PickleFunction(SaveGlobal));
				}
				value(this, context, obj);
			}
		}

		private bool TrySavePersistId(CodeContext context, object obj)
		{
			string text = Converter.ConvertToString(PythonContext.GetContext(context).CallSplat(_persist_id, obj));
			if (text != null)
			{
				SavePersId(context, text);
				return true;
			}
			return false;
		}

		private void SavePersId(CodeContext context, string res)
		{
			if (binary != 0)
			{
				Save(context, res);
				Write(context, 'Q');
			}
			else
			{
				Write(context, 'P');
				Write(context, res);
				Write(context, "\n");
			}
		}

		private static void SaveBoolean(PicklerObject pickler, CodeContext context, object obj)
		{
			if (pickler._protocol < 2)
			{
				pickler.Write(context, 'I');
				pickler.Write(context, $"0{(((bool)obj) ? 1 : 0)}");
				pickler.Write(context, "\n");
			}
			else if ((bool)obj)
			{
				pickler.Write(context, '\u0088');
			}
			else
			{
				pickler.Write(context, '\u0089');
			}
		}

		private static void SaveDict(PicklerObject pickler, CodeContext context, object obj)
		{
			int index = pickler.MemoizeNew(obj);
			if (pickler._protocol < 1)
			{
				pickler.Write(context, '(');
				pickler.Write(context, 'd');
			}
			else
			{
				pickler.Write(context, '}');
			}
			pickler.WritePut(context, index);
			pickler.BatchSetItems(context, (PythonDictionary)obj);
		}

		private static void SaveFloat(PicklerObject pickler, CodeContext context, object obj)
		{
			if (pickler._protocol < 1)
			{
				pickler.Write(context, 'F');
				pickler.WriteFloatAsString(context, obj);
			}
			else
			{
				pickler.Write(context, 'G');
				pickler.WriteFloat64(context, obj);
			}
		}

		private static void SaveGlobal(PicklerObject pickler, CodeContext context, object obj)
		{
			if (obj is PythonType pythonType)
			{
				pickler.SaveGlobalByName(context, obj, pythonType.Name);
				return;
			}
			if (PythonOps.TryGetBoundAttr(context, obj, "__name__", out var ret))
			{
				pickler.SaveGlobalByName(context, obj, ret);
				return;
			}
			throw pickler.CannotPickle(context, obj, "could not determine its __name__");
		}

		private void SaveGlobalByName(CodeContext context, object obj, object name)
		{
			object obj2 = FindModuleForGlobal(context, obj, name);
			if (_protocol >= 2 && PythonCopyReg.GetExtensionRegistry(context).TryGetValue(PythonTuple.MakeTuple(obj2, name), out var value))
			{
				if (IsUInt8(context, value))
				{
					Write(context, '\u0082');
					WriteUInt8(context, value);
					return;
				}
				if (IsUInt16(context, value))
				{
					Write(context, '\u0083');
					WriteUInt16(context, value);
					return;
				}
				if (!IsInt32(context, value))
				{
					throw PythonOps.RuntimeError("unrecognized integer format");
				}
				Write(context, '\u0084');
				WriteInt32(context, value);
			}
			else
			{
				MemoizeNew(obj);
				Write(context, 'c');
				WriteStringPair(context, obj2, name);
				WritePut(context, obj);
			}
		}

		private static void SaveInstance(PicklerObject pickler, CodeContext context, object obj)
		{
			pickler.Write(context, '(');
			if (!PythonOps.TryGetBoundAttr(context, obj, "__class__", out var ret))
			{
				throw pickler.CannotPickle(context, obj, "could not determine its __class__");
			}
			if (pickler._protocol < 1)
			{
				if (!PythonOps.TryGetBoundAttr(context, ret, "__name__", out var ret2))
				{
					throw pickler.CannotPickle(context, obj, "its __class__ has no __name__");
				}
				object value = pickler.FindModuleForGlobal(context, ret, ret2);
				pickler.MemoizeNew(obj);
				pickler.WriteInitArgs(context, obj);
				pickler.Write(context, 'i');
				pickler.WriteStringPair(context, value, ret2);
			}
			else
			{
				pickler.Save(context, ret);
				pickler.Memoize(obj);
				pickler.WriteInitArgs(context, obj);
				pickler.Write(context, 'o');
			}
			pickler.WritePut(context, obj);
			if (PythonOps.TryGetBoundAttr(context, obj, "__getstate__", out var ret3))
			{
				pickler.Save(context, PythonCalls.Call(context, ret3));
			}
			else
			{
				pickler.Save(context, PythonOps.GetBoundAttr(context, obj, "__dict__"));
			}
			pickler.Write(context, 'b');
		}

		private static void SaveInteger(PicklerObject pickler, CodeContext context, object obj)
		{
			if (pickler._protocol < 1)
			{
				pickler.Write(context, 'I');
				pickler.WriteIntAsString(context, obj);
				return;
			}
			if (IsUInt8(context, obj))
			{
				pickler.Write(context, 'K');
				pickler.WriteUInt8(context, obj);
				return;
			}
			if (IsUInt16(context, obj))
			{
				pickler.Write(context, 'M');
				pickler.WriteUInt16(context, obj);
				return;
			}
			if (IsInt32(context, obj))
			{
				pickler.Write(context, 'J');
				pickler.WriteInt32(context, obj);
				return;
			}
			throw PythonOps.RuntimeError("unrecognized integer format");
		}

		private static void SaveList(PicklerObject pickler, CodeContext context, object obj)
		{
			int index = pickler.MemoizeNew(obj);
			if (pickler._protocol < 1)
			{
				pickler.Write(context, '(');
				pickler.Write(context, 'l');
			}
			else
			{
				pickler.Write(context, ']');
			}
			pickler.WritePut(context, index);
			pickler.BatchAppends(context, ((IEnumerable)obj).GetEnumerator());
		}

		private static void SaveLong(PicklerObject pickler, CodeContext context, object obj)
		{
			BigInteger bigInteger = (BigInteger)obj;
			if (pickler._protocol < 2)
			{
				pickler.Write(context, 'L');
				pickler.WriteLongAsString(context, obj);
			}
			else if (bigInteger.IsZero())
			{
				pickler.Write(context, '\u008a');
				pickler.WriteUInt8(context, 0);
			}
			else if (bigInteger <= MaxInt && bigInteger >= MinInt)
			{
				pickler.Write(context, '\u008a');
				int num = (int)bigInteger;
				if (IsInt8(num))
				{
					pickler.WriteUInt8(context, 1);
					pickler._file.Write(context, (char)(byte)num);
				}
				else if (IsInt16(num))
				{
					pickler.WriteUInt8(context, 2);
					pickler.WriteUInt8(context, num & 0xFF);
					pickler.WriteUInt8(context, (num >> 8) & 0xFF);
				}
				else
				{
					pickler.WriteUInt8(context, 4);
					pickler.WriteInt32(context, num);
				}
			}
			else
			{
				byte[] array = bigInteger.ToByteArray();
				if (array.Length < 256)
				{
					pickler.Write(context, '\u008a');
					pickler.WriteUInt8(context, array.Length);
				}
				else
				{
					pickler.Write(context, '\u008b');
					pickler.WriteInt32(context, array.Length);
				}
				byte[] array2 = array;
				foreach (byte value in array2)
				{
					pickler.WriteUInt8(context, value);
				}
			}
		}

		private static void SaveNone(PicklerObject pickler, CodeContext context, object obj)
		{
			pickler.Write(context, 'N');
		}

		private void SaveObject(PicklerObject pickler, CodeContext context, object obj)
		{
			MemoizeNew(obj);
			PythonType pythonType = DynamicHelpers.GetPythonType(obj);
			object obj2;
			if (((IDictionary<object, object>)PythonCopyReg.GetDispatchTable(context)).TryGetValue((object)pythonType, out object value))
			{
				obj2 = PythonCalls.Call(context, value, obj);
			}
			else if (PythonOps.TryGetBoundAttr(context, obj, "__reduce_ex__", out value))
			{
				obj2 = ((!(obj is PythonType)) ? context.LanguageContext.Call(context, value, _protocol) : context.LanguageContext.Call(context, value, obj, _protocol));
			}
			else
			{
				if (!PythonOps.TryGetBoundAttr(context, obj, "__reduce__", out value))
				{
					throw PythonOps.AttributeError("no reduce function found for {0}", obj);
				}
				obj2 = ((!(obj is PythonType)) ? context.LanguageContext.Call(context, value) : context.LanguageContext.Call(context, value, obj));
			}
			if (pythonType.Equals(TypeCache.String))
			{
				if (!TryWriteFastGet(context, obj))
				{
					SaveGlobalByName(context, obj, obj2);
				}
				return;
			}
			if (obj2 is PythonTuple)
			{
				PythonTuple pythonTuple = (PythonTuple)obj2;
				switch (pythonTuple.__len__())
				{
				case 2:
					SaveReduce(context, obj, value, pythonTuple[0], pythonTuple[1], null, null, null);
					break;
				case 3:
					SaveReduce(context, obj, value, pythonTuple[0], pythonTuple[1], pythonTuple[2], null, null);
					break;
				case 4:
					SaveReduce(context, obj, value, pythonTuple[0], pythonTuple[1], pythonTuple[2], pythonTuple[3], null);
					break;
				case 5:
					SaveReduce(context, obj, value, pythonTuple[0], pythonTuple[1], pythonTuple[2], pythonTuple[3], pythonTuple[4]);
					break;
				default:
					throw CannotPickle(context, obj, "tuple returned by {0} must have to to five elements", value);
				}
				return;
			}
			throw CannotPickle(context, obj, "{0} must return string or tuple", value);
		}

		private void SaveReduce(CodeContext context, object obj, object reduceCallable, object func, object args, object state, object listItems, object dictItems)
		{
			if (!PythonOps.IsCallable(context, func))
			{
				throw CannotPickle(context, obj, "func from reduce() should be callable");
			}
			if (!(args is PythonTuple) && args != null)
			{
				throw CannotPickle(context, obj, "args from reduce() should be a tuple");
			}
			if (listItems != null && !(listItems is IEnumerator))
			{
				throw CannotPickle(context, obj, "listitems from reduce() should be a list iterator");
			}
			if (dictItems != null && !(dictItems is IEnumerator))
			{
				throw CannotPickle(context, obj, "dictitems from reduce() should be a dict iterator");
			}
			string result;
			if (func is PythonType)
			{
				result = ((PythonType)func).Name;
			}
			else
			{
				if (!PythonOps.TryGetBoundAttr(context, func, "__name__", out var ret))
				{
					throw CannotPickle(context, obj, "func from reduce() ({0}) should have a __name__ attribute");
				}
				if (!Converter.TryConvertToString(ret, out result) || result == null)
				{
					throw CannotPickle(context, obj, "__name__ of func from reduce() must be string");
				}
			}
			if (_protocol >= 2 && "__newobj__" == result)
			{
				if (args == null)
				{
					throw CannotPickle(context, obj, "__newobj__ arglist is None");
				}
				PythonTuple pythonTuple = (PythonTuple)args;
				if (pythonTuple.__len__() == 0)
				{
					throw CannotPickle(context, obj, "__newobj__ arglist is empty");
				}
				if (!DynamicHelpers.GetPythonType(obj).Equals(pythonTuple[0]))
				{
					throw CannotPickle(context, obj, "args[0] from __newobj__ args has the wrong class");
				}
				Save(context, pythonTuple[0]);
				Save(context, pythonTuple[new Slice(1, null)]);
				Write(context, '\u0081');
			}
			else
			{
				Save(context, func);
				Save(context, args);
				Write(context, 'R');
			}
			WritePut(context, obj);
			if (state != null)
			{
				Save(context, state);
				Write(context, 'b');
			}
			if (listItems != null)
			{
				BatchAppends(context, (IEnumerator)listItems);
			}
			if (dictItems != null)
			{
				BatchSetItems(context, (IEnumerator)dictItems);
			}
		}

		private static void SaveTuple(PicklerObject pickler, CodeContext context, object obj)
		{
			PythonTuple pythonTuple = (PythonTuple)obj;
			bool flag = false;
			int num = pythonTuple._data.Length;
			char data;
			if (pickler._protocol > 0 && num == 0)
			{
				data = ')';
			}
			else if (pickler._protocol >= 2 && num == 1)
			{
				data = '\u0085';
			}
			else if (pickler._protocol >= 2 && num == 2)
			{
				data = '\u0086';
			}
			else if (pickler._protocol >= 2 && num == 3)
			{
				data = '\u0087';
			}
			else
			{
				data = 't';
				flag = true;
			}
			if (flag)
			{
				pickler.Write(context, '(');
			}
			object[] data2 = pythonTuple._data;
			for (int i = 0; i < data2.Length; i++)
			{
				pickler.Save(context, data2[i]);
			}
			if (num > 0)
			{
				if (pickler.MemoContains(obj))
				{
					if (pickler._protocol == 1)
					{
						pickler.Write(context, '1');
					}
					else
					{
						if (pickler._protocol == 0)
						{
							pickler.Write(context, '0');
						}
						for (int j = 0; j < num; j++)
						{
							pickler.Write(context, '0');
						}
					}
					pickler.WriteGet(context, obj);
				}
				else
				{
					pickler.Write(context, data);
					pickler.Memoize(pythonTuple);
					pickler.WritePut(context, pythonTuple);
				}
			}
			else
			{
				pickler.Write(context, data);
			}
		}

		private static void SaveUnicode(PicklerObject pickler, CodeContext context, object obj)
		{
			if (pickler._memo != null)
			{
				pickler.MemoizeNew(obj);
				if (pickler._protocol < 1)
				{
					pickler.Write(context, 'V');
					pickler.WriteUnicodeStringRaw(context, obj);
				}
				else
				{
					pickler.Write(context, 'X');
					pickler.WriteUnicodeStringUtf8(context, obj);
				}
				pickler.WritePut(context, obj);
				return;
			}
			int num = (pickler._privMemo[obj] = pickler._privMemo.Count);
			int index = num;
			if (pickler._protocol < 1)
			{
				pickler.Write(context, 'V');
				pickler.WriteUnicodeStringRaw(context, obj);
			}
			else
			{
				pickler.Write(context, 'X');
				pickler.WriteUnicodeStringUtf8(context, obj);
			}
			pickler.WriteGetOrPut(context, isGet: false, index);
		}

		private void WriteFloatAsString(CodeContext context, object value)
		{
			Write(context, DoubleOps.__repr__(context, (double)value));
			Write(context, "\n");
		}

		private void WriteFloat64(CodeContext context, object value)
		{
			Write(context, _float64.pack(context, value));
		}

		private void WriteUInt8(CodeContext context, object value)
		{
			if (value is int)
			{
				Write(context, ScriptingRuntimeHelpers.CharToString((char)(int)value));
				return;
			}
			if (value is BigInteger)
			{
				Write(context, ScriptingRuntimeHelpers.CharToString((char)(int)(BigInteger)value));
				return;
			}
			if (value is byte)
			{
				Write(context, ScriptingRuntimeHelpers.CharToString((char)(byte)value));
				return;
			}
			throw Assert.Unreachable;
		}

		private void WriteUInt8(CodeContext context, int value)
		{
			_file.Write(context, (char)value);
		}

		private void WriteUInt16(CodeContext context, object value)
		{
			int num = (int)value;
			WriteUInt8(context, num & 0xFF);
			WriteUInt8(context, (num >> 8) & 0xFF);
		}

		private void WriteInt32(CodeContext context, object value)
		{
			int val = (int)value;
			WriteInt32(context, val);
		}

		private void WriteInt32(CodeContext context, int val)
		{
			_file.Write(context, val);
		}

		private void WriteIntAsString(CodeContext context, object value)
		{
			Write(context, PythonOps.Repr(context, value));
			Write(context, "\n");
		}

		private void WriteIntAsString(CodeContext context, int value)
		{
			Write(context, value.ToString());
			Write(context, "\n");
		}

		private void WriteLongAsString(CodeContext context, object value)
		{
			Write(context, PythonOps.Repr(context, value));
			Write(context, "\n");
		}

		private void WriteUnicodeStringRaw(CodeContext context, object value)
		{
			Write(context, StringOps.RawUnicodeEscapeEncode(((string)value).Replace("\\", "\\u005c").Replace("\n", "\\u000a")));
			Write(context, "\n");
		}

		private void WriteUnicodeStringUtf8(CodeContext context, object value)
		{
			string text = (string)value;
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] >= '\u0080')
				{
					string text2 = Encoding.UTF8.GetBytes((string)value).MakeString();
					WriteInt32(context, text2.Length);
					Write(context, text2);
					return;
				}
			}
			WriteInt32(context, text.Length);
			Write(context, text);
		}

		private void WriteStringPair(CodeContext context, object value1, object value2)
		{
			Write(context, (string)value1);
			Write(context, "\n");
			Write(context, (string)value2);
			Write(context, "\n");
		}

		private static bool IsUInt8(CodeContext context, object value)
		{
			if (value is int)
			{
				return IsUInt8((int)value);
			}
			PythonContext context2 = PythonContext.GetContext(context);
			if (context2.LessThanOrEqual(0, value))
			{
				return context2.LessThan(value, 256);
			}
			return false;
		}

		private static bool IsUInt8(int value)
		{
			if (value >= 0)
			{
				return value < 256;
			}
			return false;
		}

		private static bool IsInt8(int value)
		{
			if (value >= -128)
			{
				return value <= 127;
			}
			return false;
		}

		private static bool IsUInt16(CodeContext context, object value)
		{
			if (value is int)
			{
				return IsUInt16((int)value);
			}
			PythonContext context2 = PythonContext.GetContext(context);
			if (context2.LessThanOrEqual(256, value))
			{
				return context2.LessThan(value, 65536);
			}
			return false;
		}

		private static bool IsUInt16(int value)
		{
			if (value >= 0)
			{
				return value < 65536;
			}
			return false;
		}

		private static bool IsInt16(int value)
		{
			if (value >= -32768)
			{
				return value <= 32767;
			}
			return false;
		}

		private static bool IsInt32(CodeContext context, object value)
		{
			PythonContext context2 = PythonContext.GetContext(context);
			if (context2.LessThanOrEqual(int.MinValue, value))
			{
				return context2.LessThanOrEqual(value, int.MaxValue);
			}
			return false;
		}

		private void Write(CodeContext context, string data)
		{
			_file.Write(context, data);
		}

		private void Write(CodeContext context, char data)
		{
			_file.Write(context, data);
		}

		private void WriteGet(CodeContext context, object obj)
		{
			WriteGetOrPut(context, obj, isGet: true);
		}

		private void WriteGetOrPut(CodeContext context, object obj, bool isGet)
		{
			if (_memo == null)
			{
				WriteGetOrPut(context, isGet, _privMemo[obj]);
			}
			else
			{
				WriteGetOrPut(context, isGet, (PythonTuple)_memo[PythonOps.Id(obj)]);
			}
		}

		private void WriteGetOrPut(CodeContext context, bool isGet, PythonTuple tup)
		{
			object value = tup[0];
			if (_protocol < 1)
			{
				Write(context, isGet ? 'g' : 'p');
				WriteIntAsString(context, value);
			}
			else if (IsUInt8(context, value))
			{
				Write(context, isGet ? 'h' : 'q');
				WriteUInt8(context, value);
			}
			else
			{
				Write(context, isGet ? 'j' : 'r');
				WriteInt32(context, value);
			}
		}

		private void WriteGetOrPut(CodeContext context, bool isGet, int index)
		{
			if (_protocol < 1)
			{
				Write(context, isGet ? 'g' : 'p');
				WriteIntAsString(context, index);
			}
			else if (index >= 0 && index <= 256)
			{
				Write(context, isGet ? 'h' : 'q');
				WriteUInt8(context, index);
			}
			else
			{
				Write(context, isGet ? 'j' : 'r');
				WriteInt32(context, index);
			}
		}

		private void WriteInitArgs(CodeContext context, object obj)
		{
			if (!PythonOps.TryGetBoundAttr(context, obj, "__getinitargs__", out var ret))
			{
				return;
			}
			object obj2 = PythonCalls.Call(context, ret);
			if (!(obj2 is PythonTuple))
			{
				throw CannotPickle(context, obj, "__getinitargs__() must return tuple");
			}
			foreach (object item in (PythonTuple)obj2)
			{
				Save(context, item);
			}
		}

		private void WritePut(CodeContext context, object obj)
		{
			WriteGetOrPut(context, obj, isGet: false);
		}

		private void WritePut(CodeContext context, int index)
		{
			WriteGetOrPut(context, isGet: false, index);
		}

		private void WriteProto(CodeContext context)
		{
			Write(context, '\u0080');
			WriteUInt8(context, _protocol);
		}

		private void BatchAppends(CodeContext context, IEnumerator enumerator)
		{
			if (_protocol < 1)
			{
				while (enumerator.MoveNext())
				{
					Save(context, enumerator.Current);
					Write(context, 'a');
				}
			}
			else
			{
				if (!enumerator.MoveNext())
				{
					return;
				}
				object current = enumerator.Current;
				int num = 0;
				while (enumerator.MoveNext())
				{
					object obj = current;
					current = enumerator.Current;
					if (num == _BATCHSIZE)
					{
						Write(context, 'e');
						num = 0;
					}
					if (num == 0)
					{
						Write(context, '(');
					}
					Save(context, obj);
					num++;
				}
				if (num == _BATCHSIZE)
				{
					Write(context, 'e');
					num = 0;
				}
				Save(context, current);
				num++;
				if (num > 1)
				{
					Write(context, 'e');
				}
				else
				{
					Write(context, 'a');
				}
			}
		}

		private void BatchSetItems(CodeContext context, PythonDictionary dict)
		{
			using IEnumerator<KeyValuePair<object, object>> enumerator = dict._storage.GetEnumerator();
			if (_protocol < 1)
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<object, object> current = enumerator.Current;
					Save(context, current.Key);
					Save(context, current.Value);
					Write(context, 's');
				}
			}
			else
			{
				if (!enumerator.MoveNext())
				{
					return;
				}
				KeyValuePair<object, object> current = enumerator.Current;
				object key = current.Key;
				object value = current.Value;
				int num = 0;
				while (enumerator.MoveNext())
				{
					object obj = key;
					object obj2 = value;
					current = enumerator.Current;
					key = current.Key;
					value = current.Value;
					if (num == _BATCHSIZE)
					{
						Write(context, 'u');
						num = 0;
					}
					if (num == 0)
					{
						Write(context, '(');
					}
					Save(context, obj);
					Save(context, obj2);
					num++;
				}
				if (num == _BATCHSIZE)
				{
					Write(context, 'u');
					num = 0;
				}
				Save(context, key);
				Save(context, value);
				num++;
				if (num > 1)
				{
					Write(context, 'u');
				}
				else
				{
					Write(context, 's');
				}
			}
		}

		private void BatchSetItems(CodeContext context, IEnumerator enumerator)
		{
			if (_protocol < 1)
			{
				while (enumerator.MoveNext())
				{
					PythonTuple pythonTuple = (PythonTuple)enumerator.Current;
					Save(context, pythonTuple[0]);
					Save(context, pythonTuple[1]);
					Write(context, 's');
				}
			}
			else
			{
				if (!enumerator.MoveNext())
				{
					return;
				}
				PythonTuple pythonTuple = (PythonTuple)enumerator.Current;
				object obj = pythonTuple[0];
				object obj2 = pythonTuple[1];
				int num = 0;
				while (enumerator.MoveNext())
				{
					object obj3 = obj;
					object obj4 = obj2;
					pythonTuple = (PythonTuple)enumerator.Current;
					obj = pythonTuple[0];
					obj2 = pythonTuple[1];
					if (num == _BATCHSIZE)
					{
						Write(context, 'u');
						num = 0;
					}
					if (num == 0)
					{
						Write(context, '(');
					}
					Save(context, obj3);
					Save(context, obj4);
					num++;
				}
				if (num == _BATCHSIZE)
				{
					Write(context, 'u');
					num = 0;
				}
				Save(context, obj);
				Save(context, obj2);
				num++;
				if (num > 1)
				{
					Write(context, 'u');
				}
				else
				{
					Write(context, 's');
				}
			}
		}

		private Exception CannotPickle(CodeContext context, object obj, string format, params object[] args)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Can't pickle ");
			stringBuilder.Append(PythonOps.ToString(context, obj));
			if (format != null)
			{
				stringBuilder.Append(": ");
				stringBuilder.Append(string.Format(format, args));
			}
			return PythonExceptions.CreateThrowable(PickleError(context), stringBuilder.ToString());
		}

		private object FindModuleForGlobal(CodeContext context, object obj, object name)
		{
			if (obj is PythonType self)
			{
				return PythonType.Get__module__(context, self);
			}
			if (PythonOps.TryGetBoundAttr(context, obj, "__module__", out var ret))
			{
				LightExceptions.CheckAndThrow(Builtin.__import__(context, Converter.ConvertToString(ret)));
				if (Importer.TryGetExistingModule(context, Converter.ConvertToString(ret), out var ret2) && PythonOps.TryGetBoundAttr(context, ret2, Converter.ConvertToString(name), out var ret3))
				{
					if (PythonOps.IsRetBool(ret3, obj))
					{
						return ret;
					}
					throw CannotPickle(context, obj, "it's not the same object as {0}.{1}", ret, name);
				}
				throw CannotPickle(context, obj, "it's not found as {0}.{1}", ret, name);
			}
			foreach (KeyValuePair<object, object> systemStateModule in context.LanguageContext.SystemStateModules)
			{
				ret = systemStateModule.Key;
				object ret2 = systemStateModule.Value;
				if (PythonOps.TryGetBoundAttr(context, ret2, Converter.ConvertToString(name), out var ret4) && PythonOps.IsRetBool(ret4, obj))
				{
					return ret;
				}
			}
			throw CannotPickle(context, obj, "could not determine its module");
		}
	}

	[Documentation("Unpickler(file) -> Unpickler object\n\nAn Unpickler object reads a pickle bytecode stream and creates corresponding\nobjects.\nfile: an object (such as an open file or a StringIO) with read(num_chars) and\n    readline() methods that return strings")]
	[PythonType("Unpickler")]
	[PythonHidden]
	public class UnpicklerObject
	{
		private static readonly object _mark = new object();

		private FileInput _file;

		private List<object> _stack;

		private PythonDictionary _memo;

		private List<object> _privMemo;

		private object _pers_loader;

		public PythonDictionary memo
		{
			get
			{
				if (_memo == null)
				{
					PythonDictionary pythonDictionary = new PythonDictionary();
					for (int i = 0; i < _privMemo.Count; i++)
					{
						if (_privMemo[i] != _mark)
						{
							pythonDictionary[i] = _privMemo[i];
						}
					}
					_memo = pythonDictionary;
				}
				return _memo;
			}
			set
			{
				_memo = value;
				_privMemo = null;
			}
		}

		public object persistent_load
		{
			get
			{
				return _pers_loader;
			}
			set
			{
				_pers_loader = value;
			}
		}

		public UnpicklerObject()
		{
			_privMemo = new List<object>(200);
		}

		public UnpicklerObject(CodeContext context, object file)
			: this()
		{
			_file = new PythonFileInput(context, file);
		}

		internal UnpicklerObject(CodeContext context, FileInput input)
			: this()
		{
			_file = input;
		}

		[Documentation("load() -> unpickled object\n\nRead pickle data from the file object that was passed to the constructor and\nreturn the corresponding unpickled objects.")]
		public object load(CodeContext context)
		{
			_stack = new List<object>(32);
			while (true)
			{
				char c = _file.ReadChar(context);
				switch (c)
				{
				case 'a':
					LoadAppend(context);
					break;
				case 'e':
					LoadAppends(context);
					break;
				case 'G':
					LoadBinFloat(context);
					break;
				case 'h':
					LoadBinGet(context);
					break;
				case 'J':
					LoadBinInt(context);
					break;
				case 'K':
					LoadBinInt1(context);
					break;
				case 'M':
					LoadBinInt2(context);
					break;
				case 'Q':
					LoadBinPersid(context);
					break;
				case 'q':
					LoadBinPut(context);
					break;
				case 'T':
					LoadBinString(context);
					break;
				case 'X':
					LoadBinUnicode(context);
					break;
				case 'b':
					LoadBuild(context);
					break;
				case 'd':
					LoadDict(context);
					break;
				case '2':
					LoadDup(context);
					break;
				case '}':
					LoadEmptyDict(context);
					break;
				case ']':
					LoadEmptyList(context);
					break;
				case ')':
					LoadEmptyTuple(context);
					break;
				case '\u0082':
					LoadExt1(context);
					break;
				case '\u0083':
					LoadExt2(context);
					break;
				case '\u0084':
					LoadExt4(context);
					break;
				case 'F':
					LoadFloat(context);
					break;
				case 'g':
					LoadGet(context);
					break;
				case 'c':
					LoadGlobal(context);
					break;
				case 'i':
					LoadInst(context);
					break;
				case 'I':
					LoadInt(context);
					break;
				case 'l':
					LoadList(context);
					break;
				case 'L':
					LoadLong(context);
					break;
				case '\u008a':
					LoadLong1(context);
					break;
				case '\u008b':
					LoadLong4(context);
					break;
				case 'j':
					LoadLongBinGet(context);
					break;
				case 'r':
					LoadLongBinPut(context);
					break;
				case '(':
					LoadMark(context);
					break;
				case '\u0089':
					LoadNewFalse(context);
					break;
				case '\u0081':
					LoadNewObj(context);
					break;
				case '\u0088':
					LoadNewTrue(context);
					break;
				case 'N':
					LoadNoneValue(context);
					break;
				case 'o':
					LoadObj(context);
					break;
				case 'P':
					LoadPersId(context);
					break;
				case '0':
					LoadPop(context);
					break;
				case '1':
					LoadPopMark(context);
					break;
				case '\u0080':
					LoadProto(context);
					break;
				case 'p':
					LoadPut(context);
					break;
				case 'R':
					LoadReduce(context);
					break;
				case 's':
					LoadSetItem(context);
					break;
				case 'u':
					LoadSetItems(context);
					break;
				case 'U':
					LoadShortBinstring(context);
					break;
				case 'S':
					LoadString(context);
					break;
				case 't':
					LoadTuple(context);
					break;
				case '\u0085':
					LoadTuple1(context);
					break;
				case '\u0086':
					LoadTuple2(context);
					break;
				case '\u0087':
					LoadTuple3(context);
					break;
				case 'V':
					LoadUnicode(context);
					break;
				case '.':
					return PopStack();
				default:
					throw CannotUnpickle(context, "invalid opcode: {0}", PythonOps.Repr(context, c));
				}
			}
		}

		private object PopStack()
		{
			object result = _stack[_stack.Count - 1];
			_stack.RemoveAt(_stack.Count - 1);
			return result;
		}

		private object PeekStack()
		{
			return _stack[_stack.Count - 1];
		}

		public object[] StackGetSliceAsArray(int start)
		{
			object[] array = new object[_stack.Count - start];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = _stack[i + start];
			}
			return array;
		}

		[Documentation("noload() -> unpickled object\n\nLike load(), but don't import any modules or create create any instances of\nuser-defined types. (Builtin objects such as ints, tuples, etc. are created as\nwith load().)\n\nThis is primarily useful for scanning a pickle for persistent ids without\nincurring the overhead of completely unpickling an object. See the pickle\nmodule documentation for more information about persistent ids.")]
		public void noload(CodeContext context)
		{
			throw PythonOps.NotImplementedError("noload() is not implemented");
		}

		private Exception CannotUnpickle(CodeContext context, string format, params object[] args)
		{
			return PythonExceptions.CreateThrowable(UnpicklingError(context), string.Format(format, args));
		}

		private object MemoGet(CodeContext context, int key)
		{
			object value;
			if (_memo != null)
			{
				if (_memo.TryGetValue(key, out value))
				{
					return value;
				}
			}
			else if (key < _privMemo.Count && (value = _privMemo[key]) != _mark)
			{
				return value;
			}
			throw PythonExceptions.CreateThrowable(BadPickleGet(context), $"memo key {key} not found");
		}

		private void MemoPut(int key, object value)
		{
			if (_memo != null)
			{
				_memo[key] = value;
				return;
			}
			while (key >= _privMemo.Count)
			{
				_privMemo.Add(_mark);
			}
			_privMemo[key] = value;
		}

		private int GetMarkIndex(CodeContext context)
		{
			int num = _stack.Count - 1;
			while (num > 0 && _stack[num] != _mark)
			{
				num--;
			}
			if (num == -1)
			{
				throw CannotUnpickle(context, "mark not found");
			}
			return num;
		}

		private string Read(CodeContext context, int size)
		{
			string text = _file.Read(context, size);
			if (text.Length < size)
			{
				throw PythonOps.EofError("unexpected EOF while unpickling");
			}
			return text;
		}

		private string ReadLineNoNewline(CodeContext context)
		{
			string text = _file.ReadLine(context);
			return text.Substring(0, text.Length - 1);
		}

		private object ReadFloatString(CodeContext context)
		{
			return DoubleOps.__new__(context, TypeCache.Double, ReadLineNoNewline(context));
		}

		private double ReadFloat64(CodeContext context)
		{
			int index = 0;
			return PythonStruct.CreateDoubleValue(context, ref index, fLittleEndian: false, Read(context, 8));
		}

		private object ReadIntFromString(CodeContext context)
		{
			string text = ReadLineNoNewline(context);
			if ("00" == text)
			{
				return ScriptingRuntimeHelpers.False;
			}
			if ("01" == text)
			{
				return ScriptingRuntimeHelpers.True;
			}
			return Int32Ops.__new__(context, TypeCache.Int32, text);
		}

		private int ReadInt32(CodeContext context)
		{
			return _file.ReadInt(context);
		}

		private object ReadLongFromString(CodeContext context)
		{
			return BigIntegerOps.__new__(context, TypeCache.BigInteger, ReadLineNoNewline(context));
		}

		private object ReadLong(CodeContext context, int size)
		{
			return new BigInteger(Read(context, size).MakeByteArray());
		}

		private char ReadUInt8(CodeContext context)
		{
			return _file.ReadChar(context);
		}

		private ushort ReadUInt16(CodeContext context)
		{
			int index = 0;
			return PythonStruct.CreateUShortValue(context, ref index, fLittleEndian: true, Read(context, 2));
		}

		public object find_global(CodeContext context, object module, object attr)
		{
			if (!Importer.TryGetExistingModule(context, Converter.ConvertToString(module), out var ret))
			{
				LightExceptions.CheckAndThrow(Builtin.__import__(context, Converter.ConvertToString(module)));
				ret = context.LanguageContext.SystemStateModules[module];
			}
			return PythonOps.GetBoundAttr(context, ret, Converter.ConvertToString(attr));
		}

		private object MakeInstance(CodeContext context, object cls, object[] args)
		{
			if (cls is OldClass oldClass)
			{
				OldInstance oldInstance = new OldInstance(context, oldClass);
				if (args.Length != 0 || PythonOps.HasAttr(context, cls, "__getinitargs__"))
				{
					PythonOps.CallWithContext(context, PythonOps.GetBoundAttr(context, oldInstance, "__init__"), args);
				}
				return oldInstance;
			}
			return PythonOps.CallWithContext(context, cls, args);
		}

		private void PopMark(int markIndex)
		{
			for (int num = _stack.Count - 1; num >= markIndex; num--)
			{
				_stack.RemoveAt(num);
			}
		}

		private void SetItems(PythonDictionary dict, int markIndex)
		{
			DictionaryStorage storage = dict._storage;
			storage.EnsureCapacityNoLock((_stack.Count - (markIndex + 1)) / 2);
			for (int i = markIndex + 1; i < _stack.Count; i += 2)
			{
				storage.AddNoLock(ref dict._storage, _stack[i], _stack[i + 1]);
			}
			PopMark(markIndex);
		}

		private void LoadAppend(CodeContext context)
		{
			object obj = PopStack();
			object obj2 = PeekStack();
			if (obj2 is List)
			{
				((List)obj2).append(obj);
			}
			else
			{
				PythonCalls.Call(context, PythonOps.GetBoundAttr(context, obj2, "append"), obj);
			}
		}

		private void LoadAppends(CodeContext context)
		{
			int markIndex = GetMarkIndex(context);
			List list = (List)_stack[markIndex - 1];
			for (int i = markIndex + 1; i < _stack.Count; i++)
			{
				list.AddNoLock(_stack[i]);
			}
			PopMark(markIndex);
		}

		private void LoadBinFloat(CodeContext context)
		{
			_stack.Add(ReadFloat64(context));
		}

		private void LoadBinGet(CodeContext context)
		{
			_stack.Add(MemoGet(context, ReadUInt8(context)));
		}

		private void LoadBinInt(CodeContext context)
		{
			_stack.Add(ReadInt32(context));
		}

		private void LoadBinInt1(CodeContext context)
		{
			_stack.Add((int)ReadUInt8(context));
		}

		private void LoadBinInt2(CodeContext context)
		{
			_stack.Add((int)ReadUInt16(context));
		}

		private void LoadBinPersid(CodeContext context)
		{
			if (_pers_loader == null)
			{
				throw CannotUnpickle(context, "cannot unpickle binary persistent ID w/o persistent_load");
			}
			_stack.Add(PythonContext.GetContext(context).CallSplat(_pers_loader, PopStack()));
		}

		private void LoadBinPut(CodeContext context)
		{
			MemoPut(ReadUInt8(context), PeekStack());
		}

		private void LoadBinString(CodeContext context)
		{
			_stack.Add(Read(context, ReadInt32(context)));
		}

		private void LoadBinUnicode(CodeContext context)
		{
			string text = Read(context, ReadInt32(context));
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] >= '\u0080')
				{
					_stack.Add(StringOps.decode(context, text, "utf-8", "strict"));
					return;
				}
			}
			_stack.Add(text);
		}

		private void LoadBuild(CodeContext context)
		{
			object obj = PopStack();
			object obj2 = PeekStack();
			if (PythonOps.TryGetBoundAttr(context, obj2, "__setstate__", out var ret))
			{
				PythonOps.CallWithContext(context, ret, obj);
				return;
			}
			PythonDictionary pythonDictionary;
			PythonDictionary pythonDictionary2;
			if (obj == null)
			{
				pythonDictionary = null;
				pythonDictionary2 = null;
			}
			else if (obj is PythonDictionary)
			{
				pythonDictionary = (PythonDictionary)obj;
				pythonDictionary2 = null;
			}
			else
			{
				if (!(obj is PythonTuple))
				{
					throw PythonOps.ValueError("state for object without __setstate__ must be None, dict, or 2-tuple");
				}
				PythonTuple pythonTuple = (PythonTuple)obj;
				if (pythonTuple.__len__() != 2)
				{
					throw PythonOps.ValueError("state for object without __setstate__ must be None, dict, or 2-tuple");
				}
				pythonDictionary = (PythonDictionary)pythonTuple[0];
				pythonDictionary2 = (PythonDictionary)pythonTuple[1];
			}
			if (pythonDictionary != null && PythonOps.TryGetBoundAttr(context, obj2, "__dict__", out var ret2))
			{
				if (ret2 is PythonDictionary pythonDictionary3)
				{
					pythonDictionary3.update(context, pythonDictionary);
				}
				else
				{
					if (!PythonOps.TryGetBoundAttr(context, ret2, "update", out var ret3))
					{
						throw CannotUnpickle(context, "could not update __dict__ {0} when building {1}", pythonDictionary, obj2);
					}
					PythonOps.CallWithContext(context, ret3, pythonDictionary);
				}
			}
			if (pythonDictionary2 == null)
			{
				return;
			}
			foreach (object item in (IEnumerable)pythonDictionary2)
			{
				PythonOps.SetAttr(context, obj2, (string)item, pythonDictionary2[item]);
			}
		}

		private void LoadDict(CodeContext context)
		{
			int markIndex = GetMarkIndex(context);
			PythonDictionary pythonDictionary = new PythonDictionary((_stack.Count - 1 - markIndex) / 2);
			SetItems(pythonDictionary, markIndex);
			_stack.Add(pythonDictionary);
		}

		private void LoadDup(CodeContext context)
		{
			_stack.Add(PeekStack());
		}

		private void LoadEmptyDict(CodeContext context)
		{
			_stack.Add(new PythonDictionary(new CommonDictionaryStorage()));
		}

		private void LoadEmptyList(CodeContext context)
		{
			_stack.Add(PythonOps.MakeList());
		}

		private void LoadEmptyTuple(CodeContext context)
		{
			_stack.Add(PythonTuple.MakeTuple());
		}

		private void LoadExt1(CodeContext context)
		{
			PythonTuple pythonTuple = (PythonTuple)PythonCopyReg.GetInvertedRegistry(context)[(int)ReadUInt8(context)];
			_stack.Add(find_global(context, pythonTuple[0], pythonTuple[1]));
		}

		private void LoadExt2(CodeContext context)
		{
			PythonTuple pythonTuple = (PythonTuple)PythonCopyReg.GetInvertedRegistry(context)[(int)ReadUInt16(context)];
			_stack.Add(find_global(context, pythonTuple[0], pythonTuple[1]));
		}

		private void LoadExt4(CodeContext context)
		{
			PythonTuple pythonTuple = (PythonTuple)PythonCopyReg.GetInvertedRegistry(context)[ReadInt32(context)];
			_stack.Add(find_global(context, pythonTuple[0], pythonTuple[1]));
		}

		private void LoadFloat(CodeContext context)
		{
			_stack.Add(ReadFloatString(context));
		}

		private void LoadGet(CodeContext context)
		{
			try
			{
				_stack.Add(MemoGet(context, (int)ReadIntFromString(context)));
			}
			catch (ArgumentException)
			{
				throw PythonExceptions.CreateThrowable(BadPickleGet(context), "while executing GET: invalid integer value");
			}
		}

		private void LoadGlobal(CodeContext context)
		{
			string module = ReadLineNoNewline(context);
			string attr = ReadLineNoNewline(context);
			_stack.Add(find_global(context, module, attr));
		}

		private void LoadInst(CodeContext context)
		{
			LoadGlobal(context);
			object obj = PopStack();
			if (obj is OldClass || obj is PythonType)
			{
				int markIndex = GetMarkIndex(context);
				object[] args = StackGetSliceAsArray(markIndex + 1);
				PopMark(markIndex);
				_stack.Add(MakeInstance(context, obj, args));
				return;
			}
			throw PythonOps.TypeError("expected class or type after INST, got {0}", DynamicHelpers.GetPythonType(obj));
		}

		private void LoadInt(CodeContext context)
		{
			_stack.Add(ReadIntFromString(context));
		}

		private void LoadList(CodeContext context)
		{
			int markIndex = GetMarkIndex(context);
			List item = List.FromArrayNoCopy(StackGetSliceAsArray(markIndex + 1));
			PopMark(markIndex);
			_stack.Add(item);
		}

		private void LoadLong(CodeContext context)
		{
			_stack.Add(ReadLongFromString(context));
		}

		private void LoadLong1(CodeContext context)
		{
			int num = ReadUInt8(context);
			if (num == 4)
			{
				_stack.Add((BigInteger)ReadInt32(context));
			}
			else
			{
				_stack.Add(ReadLong(context, num));
			}
		}

		private void LoadLong4(CodeContext context)
		{
			_stack.Add(ReadLong(context, ReadInt32(context)));
		}

		private void LoadLongBinGet(CodeContext context)
		{
			_stack.Add(MemoGet(context, ReadInt32(context)));
		}

		private void LoadLongBinPut(CodeContext context)
		{
			MemoPut(ReadInt32(context), PeekStack());
		}

		private void LoadMark(CodeContext context)
		{
			_stack.Add(_mark);
		}

		private void LoadNewFalse(CodeContext context)
		{
			_stack.Add(ScriptingRuntimeHelpers.False);
		}

		private void LoadNewObj(CodeContext context)
		{
			PythonTuple pythonTuple = PopStack() as PythonTuple;
			if (pythonTuple == null)
			{
				throw PythonOps.TypeError("expected tuple as second argument to NEWOBJ, got {0}", DynamicHelpers.GetPythonType(pythonTuple));
			}
			PythonType pythonType = PopStack() as PythonType;
			if (pythonTuple == null)
			{
				throw PythonOps.TypeError("expected new-style type as first argument to NEWOBJ, got {0}", DynamicHelpers.GetPythonType(pythonTuple));
			}
			if (pythonType.TryResolveSlot(context, "__new__", out var slot) && slot.TryGetValue(context, null, pythonType, out var value))
			{
				object[] array = new object[pythonTuple.__len__() + 1];
				((ICollection)pythonTuple).CopyTo((Array)array, 1);
				array[0] = pythonType;
				_stack.Add(PythonOps.CallWithContext(context, value, array));
				return;
			}
			throw PythonOps.TypeError("didn't find __new__");
		}

		private void LoadNewTrue(CodeContext context)
		{
			_stack.Add(ScriptingRuntimeHelpers.True);
		}

		private void LoadNoneValue(CodeContext context)
		{
			_stack.Add(null);
		}

		private void LoadObj(CodeContext context)
		{
			int markIndex = GetMarkIndex(context);
			if (markIndex + 1 >= _stack.Count)
			{
				throw PythonExceptions.CreateThrowable(UnpicklingError(context), "could not find MARK");
			}
			object obj = _stack[markIndex + 1];
			if (obj is OldClass || obj is PythonType)
			{
				object[] args = StackGetSliceAsArray(markIndex + 2);
				PopMark(markIndex);
				_stack.Add(MakeInstance(context, obj, args));
				return;
			}
			throw PythonOps.TypeError("expected class or type as first argument to INST, got {0}", DynamicHelpers.GetPythonType(obj));
		}

		private void LoadPersId(CodeContext context)
		{
			if (_pers_loader == null)
			{
				throw CannotUnpickle(context, "A load persistent ID instruction is present but no persistent_load function is available");
			}
			_stack.Add(PythonContext.GetContext(context).CallSplat(_pers_loader, ReadLineNoNewline(context)));
		}

		private void LoadPop(CodeContext context)
		{
			PopStack();
		}

		private void LoadPopMark(CodeContext context)
		{
			PopMark(GetMarkIndex(context));
		}

		private void LoadProto(CodeContext context)
		{
			int num = ReadUInt8(context);
			if (num > 2)
			{
				throw PythonOps.ValueError("unsupported pickle protocol: {0}", num);
			}
		}

		private void LoadPut(CodeContext context)
		{
			MemoPut((int)ReadIntFromString(context), PeekStack());
		}

		private void LoadReduce(CodeContext context)
		{
			object obj = PopStack();
			object obj2 = PopStack();
			if (obj == null)
			{
				_stack.Add(PythonCalls.Call(context, PythonOps.GetBoundAttr(context, obj2, "__basicnew__")));
			}
			else if (obj.GetType() != typeof(PythonTuple))
			{
				throw PythonOps.TypeError("while executing REDUCE, expected tuple at the top of the stack, but got {0}", DynamicHelpers.GetPythonType(obj));
			}
			_stack.Add(PythonCalls.Call(context, obj2, ((PythonTuple)obj)._data));
		}

		private void LoadSetItem(CodeContext context)
		{
			object value = PopStack();
			object key = PopStack();
			if (!(PeekStack() is PythonDictionary pythonDictionary))
			{
				throw PythonOps.TypeError("while executing SETITEM, expected dict at stack[-3], but got {0}", DynamicHelpers.GetPythonType(PeekStack()));
			}
			pythonDictionary[key] = value;
		}

		private void LoadSetItems(CodeContext context)
		{
			int markIndex = GetMarkIndex(context);
			if (!(_stack[markIndex - 1] is PythonDictionary dict))
			{
				throw PythonOps.TypeError("while executing SETITEMS, expected dict below last mark, but got {0}", DynamicHelpers.GetPythonType(_stack[markIndex - 1]));
			}
			SetItems(dict, markIndex);
		}

		private void LoadShortBinstring(CodeContext context)
		{
			_stack.Add(Read(context, ReadUInt8(context)));
		}

		private void LoadString(CodeContext context)
		{
			string text = ReadLineNoNewline(context);
			if (text.Length < 2 || ((text[0] != '"' || text[text.Length - 1] != '"') && (text[0] != '\'' || text[text.Length - 1] != '\'')))
			{
				throw PythonOps.ValueError("while executing STRING, expected string that starts and ends with quotes");
			}
			_stack.Add(StringOps.decode(context, text.Substring(1, text.Length - 2), "string-escape", "strict"));
		}

		private void LoadTuple(CodeContext context)
		{
			int markIndex = GetMarkIndex(context);
			PythonTuple item = PythonTuple.MakeTuple(StackGetSliceAsArray(markIndex + 1));
			PopMark(markIndex);
			_stack.Add(item);
		}

		private void LoadTuple1(CodeContext context)
		{
			object obj = PopStack();
			_stack.Add(PythonTuple.MakeTuple(obj));
		}

		private void LoadTuple2(CodeContext context)
		{
			object obj = PopStack();
			object obj2 = PopStack();
			_stack.Add(PythonTuple.MakeTuple(obj2, obj));
		}

		private void LoadTuple3(CodeContext context)
		{
			object obj = PopStack();
			object obj2 = PopStack();
			object obj3 = PopStack();
			_stack.Add(PythonTuple.MakeTuple(obj3, obj2, obj));
		}

		private void LoadUnicode(CodeContext context)
		{
			_stack.Add(StringOps.decode(context, ReadLineNoNewline(context), "raw-unicode-escape", "strict"));
		}
	}

	private class ReferenceEqualityComparer : IEqualityComparer<object>
	{
		public static ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

		public new bool Equals(object x, object y)
		{
			return x == y;
		}

		public int GetHashCode(object obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	public const string __doc__ = "Fast object serialization/deserialization.\n\nDifferences from CPython:\n - does not implement the undocumented fast mode\n";

	private const int highestProtocol = 2;

	public const string __version__ = "1.71";

	public const string format_version = "2.0";

	private const string Newline = "\n";

	private static readonly PythonStruct.Struct _float64 = PythonStruct.Struct.Create(">d");

	public static int HIGHEST_PROTOCOL => 2;

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		context.EnsureModuleException("PickleError", dict, "PickleError", "cPickle");
		context.EnsureModuleException("PicklingError", dict, "PicklingError", "cPickle");
		context.EnsureModuleException("UnpicklingError", dict, "UnpicklingError", "cPickle");
		context.EnsureModuleException("UnpickleableError", dict, "UnpickleableError", "cPickle");
		context.EnsureModuleException("BadPickleGet", dict, "BadPickleGet", "cPickle");
		dict["__builtins__"] = context.BuiltinModuleInstance;
		dict["compatible_formats"] = PythonOps.MakeList("1.0", "1.1", "1.2", "1.3", "2.0");
	}

	[Documentation("dump(obj, file, protocol=0) -> None\n\nPickle obj and write the result to file.\n\nSee documentation for Pickler() for a description the file, protocol, and\n(deprecated) bin parameters.")]
	public static void dump(CodeContext context, object obj, object file, [DefaultParameterValue(null)] object protocol, [DefaultParameterValue(null)] object bin)
	{
		PicklerObject picklerObject = new PicklerObject(context, file, protocol, bin);
		picklerObject.dump(context, obj);
	}

	[Documentation("dumps(obj, protocol=0) -> pickle string\n\nPickle obj and return the result as a string.\n\nSee the documentation for Pickler() for a description of the protocol and\n(deprecated) bin parameters.")]
	public static string dumps(CodeContext context, object obj, [DefaultParameterValue(null)] object protocol, [DefaultParameterValue(null)] object bin)
	{
		StringBuilderOutput stringBuilderOutput = new StringBuilderOutput();
		PicklerObject picklerObject = new PicklerObject(context, stringBuilderOutput, protocol, bin);
		picklerObject.dump(context, obj);
		return stringBuilderOutput.GetString();
	}

	[Documentation("load(file) -> unpickled object\n\nRead pickle data from the open file object and return the corresponding\nunpickled object. Data after the first pickle found is ignored, but the file\ncursor is not reset, so if a file objects contains multiple pickles, then\nload() may be called multiple times to unpickle them.\n\nfile: an object (such as an open file or a StringIO) with read(num_chars) and\n    readline() methods that return strings\n\nload() automatically determines if the pickle data was written in binary or\ntext mode.")]
	public static object load(CodeContext context, object file)
	{
		return new UnpicklerObject(context, file).load(context);
	}

	[Documentation("loads(string) -> unpickled object\n\nRead a pickle object from a string, unpickle it, and return the resulting\nreconstructed object. Characters in the string beyond the end of the first\npickle are ignored.")]
	public static object loads(CodeContext context, string @string)
	{
		return new UnpicklerObject(context, new PythonStringInput(@string)).load(context);
	}

	public static PicklerObject Pickler(CodeContext context, [DefaultParameterValue(null)] object file, [DefaultParameterValue(null)] object protocol, [DefaultParameterValue(null)] object bin)
	{
		return new PicklerObject(context, file, protocol, bin);
	}

	public static UnpicklerObject Unpickler(CodeContext context, object file)
	{
		return new UnpicklerObject(context, file);
	}

	private static PythonType PicklingError(CodeContext context)
	{
		return (PythonType)PythonContext.GetContext(context).GetModuleState("PicklingError");
	}

	private static PythonType PickleError(CodeContext context)
	{
		return (PythonType)PythonContext.GetContext(context).GetModuleState("PickleError");
	}

	private static PythonType UnpicklingError(CodeContext context)
	{
		return (PythonType)PythonContext.GetContext(context).GetModuleState("UnpicklingError");
	}

	private static PythonType BadPickleGet(CodeContext context)
	{
		return (PythonType)PythonContext.GetContext(context).GetModuleState("BadPickleGet");
	}
}
