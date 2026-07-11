using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using IronPython.Runtime;
using IronPython.Runtime.Binding;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Ast;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Modules;

public static class CTypes
{
	internal enum SimpleTypeKind
	{
		Char,
		SignedByte,
		UnsignedByte,
		SignedShort,
		UnsignedShort,
		SignedInt,
		UnsignedInt,
		SignedLong,
		UnsignedLong,
		Single,
		Double,
		SignedLongLong,
		UnsignedLongLong,
		Object,
		Pointer,
		CharPointer,
		WCharPointer,
		WChar,
		Boolean,
		VariantBool,
		BStr
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr CastDelegate(IntPtr data, IntPtr obj, IntPtr type);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr StringAtDelegate(IntPtr addr, int length);

	private class RefCountInfo
	{
		public int RefCount;

		public GCHandle Handle;
	}

	[PythonHidden]
	[PythonType("_CData")]
	public abstract class CData : IPythonBufferable, IBufferProtocol
	{
		internal MemoryHolder _memHolder;

		public int Size
		{
			[PythonHidden]
			get
			{
				return NativeType.Size;
			}
		}

		public IntPtr UnsafeAddress
		{
			[PythonHidden]
			get
			{
				return _memHolder.UnsafeAddress;
			}
		}

		internal INativeType NativeType => (INativeType)DynamicHelpers.GetPythonType(this);

		public virtual object _objects => _memHolder.Objects;

		public virtual int ItemCount
		{
			[PythonHidden]
			get
			{
				return 1;
			}
		}

		string IBufferProtocol.Format => NativeType.TypeFormat;

		public virtual BigInteger ItemSize
		{
			[PythonHidden]
			get
			{
				return NativeType.Size;
			}
		}

		BigInteger IBufferProtocol.NumberDimensions => 0;

		bool IBufferProtocol.ReadOnly => false;

		PythonTuple IBufferProtocol.Strides => null;

		object IBufferProtocol.SubOffsets => null;

		byte[] IPythonBufferable.GetBytes(int offset, int length)
		{
			int num = checked(offset + length);
			byte[] array = new byte[length];
			for (int i = offset; i < num; i++)
			{
				array[i - offset] = _memHolder.ReadByte(i);
			}
			return array;
		}

		internal void SetAddress(IntPtr address)
		{
			_memHolder = new MemoryHolder(address, NativeType.Size);
		}

		internal virtual PythonTuple GetBufferInfo()
		{
			return PythonTuple.MakeTuple(NativeType.TypeFormat, 0, PythonTuple.EMPTY);
		}

		Bytes IBufferProtocol.GetItem(int index)
		{
			return new Bytes(((IPythonBufferable)this).GetBytes(index, NativeType.Size));
		}

		void IBufferProtocol.SetItem(int index, object value)
		{
			throw new NotImplementedException();
		}

		void IBufferProtocol.SetSlice(Slice index, object value)
		{
			throw new NotImplementedException();
		}

		[PythonHidden]
		public virtual IList<BigInteger> GetShape(int start, int? end)
		{
			return null;
		}

		Bytes IBufferProtocol.ToBytes(int start, int? end)
		{
			return new Bytes(((IPythonBufferable)this).GetBytes(start, NativeType.Size));
		}

		List IBufferProtocol.ToList(int start, int? end)
		{
			return new List(((IBufferProtocol)this).ToBytes(start, end));
		}
	}

	[PythonType("Array")]
	public abstract class _Array : CData
	{
		public object this[int index]
		{
			get
			{
				index = PythonOps.FixIndex(index, ((ArrayType)base.NativeType).Length);
				INativeType elementType = ElementType;
				return elementType.GetValue(_memHolder, this, checked(index * elementType.Size), raw: false);
			}
			set
			{
				index = PythonOps.FixIndex(index, ((ArrayType)base.NativeType).Length);
				INativeType elementType = ElementType;
				object obj = elementType.SetValue(_memHolder, checked(index * elementType.Size), value);
				if (obj != null)
				{
					_memHolder.AddObject(index.ToString(), obj);
				}
			}
		}

		public object this[[NotNull] Slice slice]
		{
			get
			{
				int length = ((ArrayType)base.NativeType).Length;
				SimpleType simpleType = ((ArrayType)base.NativeType).ElementType as SimpleType;
				slice.indices(length, out var ostart, out var ostop, out var ostep);
				if ((ostep > 0 && ostart >= ostop) || (ostep < 0 && ostart <= ostop))
				{
					if (simpleType != null && (simpleType._type == SimpleTypeKind.WChar || simpleType._type == SimpleTypeKind.Char))
					{
						return string.Empty;
					}
					return new List();
				}
				int num = (int)((ostep > 0) ? (((long)ostop - (long)ostart + ostep - 1) / ostep) : (((long)ostop - (long)ostart + ostep + 1) / ostep));
				if (simpleType != null && (simpleType._type == SimpleTypeKind.WChar || simpleType._type == SimpleTypeKind.Char))
				{
					int size = ((INativeType)simpleType).Size;
					StringBuilder stringBuilder = new StringBuilder(num);
					int num2 = 0;
					int num3 = ostart;
					while (num2 < num)
					{
						char value = simpleType.ReadChar(_memHolder, checked(num3 * size));
						stringBuilder.Append(value);
						num2++;
						num3 += ostep;
					}
					return stringBuilder.ToString();
				}
				object[] array = new object[num];
				int num4 = 0;
				int num5 = 0;
				int num6 = ostart;
				while (num5 < num)
				{
					array[num4++] = this[num6];
					num5++;
					num6 += ostep;
				}
				return new List(array);
			}
			set
			{
				int length = ((ArrayType)base.NativeType).Length;
				slice.indices(length, out var ostart, out var ostop, out var ostep);
				int num = (int)((ostep > 0) ? (((long)ostop - (long)ostart + ostep - 1) / ostep) : (((long)ostop - (long)ostart + ostep + 1) / ostep));
				IEnumerator enumerator = PythonOps.GetEnumerator(value);
				int num2 = 0;
				int num3 = ostart;
				while (num2 < num)
				{
					if (!enumerator.MoveNext())
					{
						throw PythonOps.ValueError("sequence not long enough");
					}
					this[num3] = enumerator.Current;
					num2++;
					num3 += ostep;
				}
				if (enumerator.MoveNext())
				{
					throw PythonOps.ValueError("not all values consumed while slicing");
				}
			}
		}

		private INativeType ElementType => ((ArrayType)base.NativeType).ElementType;

		public override int ItemCount
		{
			[PythonHidden]
			get
			{
				return __len__();
			}
		}

		public override BigInteger ItemSize
		{
			[PythonHidden]
			get
			{
				INativeType nativeType = base.NativeType;
				while (nativeType is ArrayType)
				{
					nativeType = ((ArrayType)nativeType).ElementType;
				}
				return nativeType.Size;
			}
		}

		public void __init__(params object[] args)
		{
			INativeType nativeType = base.NativeType;
			_memHolder = new MemoryHolder(nativeType.Size);
			if (args.Length > ((ArrayType)nativeType).Length)
			{
				throw PythonOps.IndexError("too many arguments");
			}
			nativeType.SetValue(_memHolder, 0, args);
		}

		public int __len__()
		{
			return ((ArrayType)base.NativeType).Length;
		}

		internal override PythonTuple GetBufferInfo()
		{
			INativeType elementType = ElementType;
			int num = 1;
			List<object> list = new List<object>();
			list.Add((BigInteger)__len__());
			while (elementType is ArrayType)
			{
				num++;
				list.Add((BigInteger)((ArrayType)elementType).Length);
				elementType = ((ArrayType)elementType).ElementType;
			}
			return PythonTuple.MakeTuple(base.NativeType.TypeFormat, num, PythonTuple.Make(list));
		}

		[PythonHidden]
		public override IList<BigInteger> GetShape(int start, int? end)
		{
			List<BigInteger> list = new List<BigInteger>();
			for (ArrayType arrayType = base.NativeType as ArrayType; arrayType != null; arrayType = arrayType.ElementType as ArrayType)
			{
				list.Add(arrayType.Length);
			}
			return list;
		}
	}

	internal interface INativeType
	{
		int Size { get; }

		int Alignment { get; }

		string TypeFormat { get; }

		object GetValue(MemoryHolder owner, object readingFrom, int offset, bool raw);

		object SetValue(MemoryHolder address, int offset, object value);

		Type GetNativeType();

		Type GetPythonType();

		MarshalCleanup EmitMarshalling(ILGenerator method, LocalOrArg argIndex, List<object> constantPool, int constantPoolArgument);

		void EmitReverseMarshalling(ILGenerator method, LocalOrArg value, List<object> constantPool, int constantPoolArgument);
	}

	[PythonType]
	[PythonHidden]
	public class ArrayType : PythonType, INativeType
	{
		private int _length;

		private INativeType _type;

		int INativeType.Size => GetSize();

		int INativeType.Alignment => _type.Alignment;

		private bool IsStringType
		{
			get
			{
				if (_type is SimpleType simpleType)
				{
					if (simpleType._type != SimpleTypeKind.WChar)
					{
						return simpleType._type == SimpleTypeKind.Char;
					}
					return true;
				}
				return false;
			}
		}

		internal int Length => _length;

		internal INativeType ElementType => _type;

		string INativeType.TypeFormat
		{
			get
			{
				string text = "(" + Length;
				INativeType elementType = ElementType;
				while (elementType is ArrayType)
				{
					text = text + "," + ((ArrayType)elementType).Length;
					elementType = ((ArrayType)elementType).ElementType;
				}
				text += ")";
				return text + elementType.TypeFormat;
			}
		}

		public ArrayType(CodeContext context, string name, PythonTuple bases, PythonDictionary dict)
			: base(context, name, bases, dict)
		{
			if (dict.TryGetValue("_length_", out var value) && value is int num)
			{
				int length = num;
				if (num >= 0)
				{
					if (!dict.TryGetValue("_type_", out var value2))
					{
						throw PythonOps.AttributeError("class must define a '_type_' attribute");
					}
					_length = length;
					_type = (INativeType)value2;
					if (_type is SimpleType)
					{
						SimpleType simpleType = (SimpleType)_type;
						if (simpleType._type == SimpleTypeKind.Char)
						{
							SetCustomMember(context, "value", new ReflectedExtensionProperty(new ExtensionPropertyInfo(this, typeof(CTypes).GetMethod("GetCharArrayValue")), NameType.PythonProperty));
							SetCustomMember(context, "raw", new ReflectedExtensionProperty(new ExtensionPropertyInfo(this, typeof(CTypes).GetMethod("GetWCharArrayRaw")), NameType.PythonProperty));
						}
						else if (simpleType._type == SimpleTypeKind.WChar)
						{
							SetCustomMember(context, "value", new ReflectedExtensionProperty(new ExtensionPropertyInfo(this, typeof(CTypes).GetMethod("GetWCharArrayValue")), NameType.PythonProperty));
							SetCustomMember(context, "raw", new ReflectedExtensionProperty(new ExtensionPropertyInfo(this, typeof(CTypes).GetMethod("GetWCharArrayRaw")), NameType.PythonProperty));
						}
					}
					return;
				}
			}
			throw PythonOps.AttributeError("arrays must have _length_ attribute and it must be a positive integer");
		}

		private ArrayType(Type underlyingSystemType)
			: base(underlyingSystemType)
		{
		}

		public _Array from_address(CodeContext context, int ptr)
		{
			_Array array = (_Array)CreateInstance(context);
			array.SetAddress(new IntPtr(ptr));
			return array;
		}

		public _Array from_address(CodeContext context, BigInteger ptr)
		{
			_Array array = (_Array)CreateInstance(context);
			array.SetAddress(new IntPtr((long)ptr));
			return array;
		}

		public _Array from_buffer(ArrayModule.array array, [DefaultParameterValue(0)] int offset)
		{
			ValidateArraySizes(array, offset, ((INativeType)this).Size);
			_Array array2 = (_Array)CreateInstance(base.Context.SharedContext);
			IntPtr arrayAddress = array.GetArrayAddress();
			array2._memHolder = new MemoryHolder(arrayAddress.Add(offset), ((INativeType)this).Size);
			array2._memHolder.AddObject("ffffffff", array);
			return array2;
		}

		public _Array from_buffer_copy(ArrayModule.array array, [DefaultParameterValue(0)] int offset)
		{
			ValidateArraySizes(array, offset, ((INativeType)this).Size);
			_Array array2 = (_Array)CreateInstance(base.Context.SharedContext);
			array2._memHolder = new MemoryHolder(((INativeType)this).Size);
			array2._memHolder.CopyFrom(array.GetArrayAddress().Add(offset), new IntPtr(((INativeType)this).Size));
			GC.KeepAlive(array);
			return array2;
		}

		public _Array from_buffer_copy(Bytes array, [DefaultParameterValue(0)] int offset)
		{
			ValidateArraySizes(array, offset, ((INativeType)this).Size);
			_Array array2 = (_Array)CreateInstance(base.Context.SharedContext);
			array2._memHolder = new MemoryHolder(((INativeType)this).Size);
			for (int i = 0; i < ((INativeType)this).Size; i++)
			{
				array2._memHolder.WriteByte(i, array._bytes[i]);
			}
			return array2;
		}

		public object from_param(object obj)
		{
			return null;
		}

		internal static PythonType MakeSystemType(Type underlyingSystemType)
		{
			return PythonType.SetPythonType(underlyingSystemType, new ArrayType(underlyingSystemType));
		}

		public static ArrayType operator *(ArrayType type, int count)
		{
			return MakeArrayType(type, count);
		}

		public static ArrayType operator *(int count, ArrayType type)
		{
			return MakeArrayType(type, count);
		}

		private int GetSize()
		{
			return _length * _type.Size;
		}

		object INativeType.GetValue(MemoryHolder owner, object readingFrom, int offset, bool raw)
		{
			if (IsStringType)
			{
				SimpleType simpleType = (SimpleType)_type;
				string text = ((simpleType._type != SimpleTypeKind.Char) ? owner.ReadUnicodeString(offset, _length) : owner.ReadAnsiString(offset, _length));
				for (int i = 0; i < text.Length; i++)
				{
					if (text[i] == '\0')
					{
						return text.Substring(0, i);
					}
				}
				return text;
			}
			_Array array = (_Array)CreateInstance(base.Context.SharedContext);
			array._memHolder = new MemoryHolder(owner.UnsafeAddress.Add(offset), ((INativeType)this).Size, owner);
			return array;
		}

		internal string GetRawValue(MemoryHolder owner, int offset)
		{
			SimpleType simpleType = (SimpleType)_type;
			if (simpleType._type == SimpleTypeKind.Char)
			{
				return owner.ReadAnsiString(offset, _length);
			}
			return owner.ReadUnicodeString(offset, _length);
		}

		object INativeType.SetValue(MemoryHolder address, int offset, object value)
		{
			if (value is string text)
			{
				if (!IsStringType)
				{
					throw PythonOps.TypeError("expected {0} instance, got str", base.Name);
				}
				if (text.Length > _length)
				{
					throw PythonOps.ValueError("string too long ({0}, maximum length {1})", text.Length, _length);
				}
				WriteString(address, offset, text);
				return null;
			}
			if (IsStringType)
			{
				if (value is IList<object> list)
				{
					StringBuilder stringBuilder = new StringBuilder(list.Count);
					foreach (object item in list)
					{
						stringBuilder.Append(Converter.ConvertToChar(item));
					}
					WriteString(address, offset, stringBuilder.ToString());
					return null;
				}
				throw PythonOps.TypeError("expected string or Unicode object, {0} found", DynamicHelpers.GetPythonType(value).Name);
			}
			object[] array = value as object[];
			if (array == null && value is PythonTuple pythonTuple)
			{
				array = pythonTuple._data;
			}
			if (array != null)
			{
				if (array.Length > _length)
				{
					throw PythonOps.RuntimeError("invalid index");
				}
				for (int i = 0; i < array.Length; i++)
				{
					_type.SetValue(address, checked(offset + i * _type.Size), array[i]);
				}
				return null;
			}
			if (value is _Array array2 && array2.NativeType == this)
			{
				array2._memHolder.CopyTo(address, offset, ((INativeType)this).Size);
				return array2._memHolder.EnsureObjects();
			}
			throw PythonOps.TypeError("unexpected {0} instance, got {1}", base.Name, DynamicHelpers.GetPythonType(value).Name);
		}

		private void WriteString(MemoryHolder address, int offset, string str)
		{
			SimpleType simpleType = (SimpleType)_type;
			if (str.Length < _length)
			{
				str += '\0';
			}
			if (simpleType._type == SimpleTypeKind.Char)
			{
				address.WriteAnsiString(offset, str);
			}
			else
			{
				address.WriteUnicodeString(offset, str);
			}
		}

		Type INativeType.GetNativeType()
		{
			return typeof(IntPtr);
		}

		MarshalCleanup INativeType.EmitMarshalling(ILGenerator method, LocalOrArg argIndex, List<object> constantPool, int constantPoolArgument)
		{
			Type type = argIndex.Type;
			Label label = method.DefineLabel();
			if (!type.IsValueType)
			{
				Label label2 = method.DefineLabel();
				argIndex.Emit(method);
				method.Emit(OpCodes.Ldnull);
				method.Emit(OpCodes.Bne_Un, label2);
				method.Emit(OpCodes.Ldc_I4_0);
				method.Emit(OpCodes.Conv_I);
				method.Emit(OpCodes.Br, label);
				method.MarkLabel(label2);
			}
			argIndex.Emit(method);
			if (type.IsValueType)
			{
				method.Emit(OpCodes.Box, type);
			}
			constantPool.Add(this);
			method.Emit(OpCodes.Ldarg, constantPoolArgument);
			method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
			method.Emit(OpCodes.Ldelem_Ref);
			method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CheckCDataType"));
			method.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
			method.MarkLabel(label);
			return null;
		}

		Type INativeType.GetPythonType()
		{
			return ((INativeType)this).GetNativeType();
		}

		void INativeType.EmitReverseMarshalling(ILGenerator method, LocalOrArg value, List<object> constantPool, int constantPoolArgument)
		{
			value.Emit(method);
		}
	}

	[PythonType("CFuncPtr")]
	public abstract class _CFuncPtr : CData, IDynamicMetaObjectProvider, ICodeFormattable
	{
		private class Meta : MetaPythonObject
		{
			private abstract class ArgumentMarshaller
			{
				private readonly Expression _argExpr;

				public abstract Type NativeType { get; }

				public Expression ArgumentExpression => _argExpr;

				public ArgumentMarshaller(Expression container)
				{
					_argExpr = container;
				}

				public abstract MarshalCleanup EmitCallStubArgument(ILGenerator generator, int argIndex, List<object> constantPool, int constantPoolArgument);

				public virtual Expression GetKeepAlive()
				{
					return null;
				}

				public virtual BindingRestrictions GetRestrictions()
				{
					return BindingRestrictions.Empty;
				}
			}

			private class PrimitiveMarshaller : ArgumentMarshaller
			{
				private readonly Type _type;

				private static MethodInfo _bigIntToInt32;

				private static MethodInfo BigIntToInt32
				{
					get
					{
						if (_bigIntToInt32 == null)
						{
							MemberInfo[] member = typeof(BigInteger).GetMember("op_Explicit", MemberTypes.Method, BindingFlags.Static | BindingFlags.Public);
							MemberInfo[] array = member;
							for (int i = 0; i < array.Length; i++)
							{
								MethodInfo methodInfo = (MethodInfo)array[i];
								if (methodInfo.ReturnType == typeof(int))
								{
									_bigIntToInt32 = methodInfo;
									break;
								}
							}
						}
						return _bigIntToInt32;
					}
				}

				public override Type NativeType
				{
					get
					{
						if (_type == typeof(BigInteger))
						{
							return typeof(int);
						}
						if (!_type.IsValueType)
						{
							return typeof(IntPtr);
						}
						return _type;
					}
				}

				public PrimitiveMarshaller(Expression container, Type type)
					: base(container)
				{
					_type = type;
				}

				public override MarshalCleanup EmitCallStubArgument(ILGenerator generator, int argIndex, List<object> constantPool, int constantPoolArgument)
				{
					if (_type == typeof(DynamicNull))
					{
						generator.Emit(OpCodes.Ldc_I4_0);
						generator.Emit(OpCodes.Conv_I);
						return null;
					}
					generator.Emit(OpCodes.Ldarg, argIndex);
					if (base.ArgumentExpression.Type != _type)
					{
						generator.Emit(OpCodes.Unbox_Any, _type);
					}
					if (_type == typeof(string))
					{
						LocalBuilder local = generator.DeclareLocal(typeof(string), pinned: true);
						generator.Emit(OpCodes.Stloc, local);
						generator.Emit(OpCodes.Ldloc, local);
						generator.Emit(OpCodes.Conv_I);
						generator.Emit(OpCodes.Ldc_I4, RuntimeHelpers.OffsetToStringData);
						generator.Emit(OpCodes.Add);
					}
					else if (_type == typeof(Bytes))
					{
						LocalBuilder local2 = generator.DeclareLocal(typeof(byte).MakeByRefType(), pinned: true);
						generator.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("GetBytes"));
						generator.Emit(OpCodes.Ldc_I4_0);
						generator.Emit(OpCodes.Ldelema, typeof(byte));
						generator.Emit(OpCodes.Stloc, local2);
						generator.Emit(OpCodes.Ldloc, local2);
					}
					else if (_type == typeof(BigInteger))
					{
						generator.Emit(OpCodes.Call, BigIntToInt32);
					}
					else if (!_type.IsValueType)
					{
						generator.Emit(OpCodes.Call, typeof(CTypes).GetMethod("PyObj_ToPtr"));
					}
					return null;
				}

				public override BindingRestrictions GetRestrictions()
				{
					if (_type == typeof(DynamicNull))
					{
						return BindingRestrictions.GetExpressionRestriction(Expression.Equal(base.ArgumentExpression, Expression.Constant(null)));
					}
					return BindingRestrictions.GetTypeRestriction(base.ArgumentExpression, _type);
				}
			}

			private class FromParamMarshaller : ArgumentMarshaller
			{
				public override Type NativeType
				{
					get
					{
						throw new NotImplementedException();
					}
				}

				public FromParamMarshaller(Expression container)
					: base(container)
				{
				}

				public override MarshalCleanup EmitCallStubArgument(ILGenerator generator, int argIndex, List<object> constantPool, int constantPoolArgument)
				{
					throw new NotImplementedException();
				}
			}

			private class CDataMarshaller : ArgumentMarshaller
			{
				private readonly Type _type;

				private readonly INativeType _cdataType;

				public override Type NativeType => _cdataType.GetNativeType();

				public CDataMarshaller(Expression container, Type type, INativeType cdataType)
					: base(container)
				{
					_type = type;
					_cdataType = cdataType;
				}

				public override MarshalCleanup EmitCallStubArgument(ILGenerator generator, int argIndex, List<object> constantPool, int constantPoolArgument)
				{
					return _cdataType.EmitMarshalling(generator, new Arg(argIndex, base.ArgumentExpression.Type), constantPool, constantPoolArgument);
				}

				public override Expression GetKeepAlive()
				{
					if (_type.IsValueType)
					{
						return null;
					}
					return Expression.Call(typeof(GC).GetMethod("KeepAlive"), base.ArgumentExpression);
				}

				public override BindingRestrictions GetRestrictions()
				{
					return BindingRestrictions.Empty;
				}
			}

			private class NativeArgumentMarshaller : ArgumentMarshaller
			{
				public override Type NativeType => typeof(IntPtr);

				public NativeArgumentMarshaller(Expression container)
					: base(container)
				{
				}

				public override MarshalCleanup EmitCallStubArgument(ILGenerator generator, int argIndex, List<object> constantPool, int constantPoolArgument)
				{
					generator.Emit(OpCodes.Ldarg, argIndex);
					generator.Emit(OpCodes.Castclass, typeof(NativeArgument));
					generator.Emit(OpCodes.Call, typeof(NativeArgument).GetMethod("get__obj"));
					generator.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
					return null;
				}

				public override Expression GetKeepAlive()
				{
					return Expression.Call(typeof(GC).GetMethod("KeepAlive"), base.ArgumentExpression);
				}

				public override BindingRestrictions GetRestrictions()
				{
					return BindingRestrictions.GetTypeRestriction(base.ArgumentExpression, typeof(NativeArgument));
				}
			}

			public new _CFuncPtr Value => (_CFuncPtr)base.Value;

			public Meta(Expression parameter, _CFuncPtr func)
				: base(parameter, BindingRestrictions.Empty, func)
			{
			}

			public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
			{
				CodeContext sharedContext = PythonContext.GetPythonContext(binder).SharedContext;
				ArgumentMarshaller[] argumentMarshallers = GetArgumentMarshallers(args);
				BindingRestrictions bindingRestrictions = BindingRestrictions.GetTypeRestriction(base.Expression, Value.GetType()).Merge(BindingRestrictions.GetExpressionRestriction(Expression.Call(typeof(ModuleOps).GetMethod("CheckFunctionId"), Expression.Convert(base.Expression, typeof(_CFuncPtr)), Expression.Constant(Value.Id))));
				ArgumentMarshaller[] array = argumentMarshallers;
				foreach (ArgumentMarshaller argumentMarshaller in array)
				{
					bindingRestrictions = bindingRestrictions.Merge(argumentMarshaller.GetRestrictions());
				}
				int num = args.Length;
				if (Value._comInterfaceIndex != -1)
				{
					num--;
				}
				if (Value._argtypes != null)
				{
					if (num < Value._argtypes.Count || (Value.CallingConvention != CallingConvention.Cdecl && num > Value._argtypes.Count))
					{
						return IncorrectArgCount(binder, bindingRestrictions, Value._argtypes.Count, num);
					}
				}
				else
				{
					CFuncPtrType cFuncPtrType = (CFuncPtrType)Value.NativeType;
					if (cFuncPtrType._argtypes != null && (num < cFuncPtrType._argtypes.Length || (Value.CallingConvention != CallingConvention.Cdecl && num > cFuncPtrType._argtypes.Length)))
					{
						return IncorrectArgCount(binder, bindingRestrictions, cFuncPtrType._argtypes.Length, num);
					}
				}
				if (Value._comInterfaceIndex != -1 && args.Length == 0)
				{
					return NoThisParam(binder, bindingRestrictions);
				}
				Expression expression = MakeCall(argumentMarshallers, GetNativeReturnType(), Value.Getrestype() == null, GetFunctionAddress(args));
				List<Expression> list = new List<Expression>();
				Expression res;
				if (expression.Type != typeof(void))
				{
					ParameterExpression parameterExpression = Expression.Parameter(expression.Type, "ret");
					list.Add(Expression.Assign(parameterExpression, expression));
					AddKeepAlives(argumentMarshallers, list);
					list.Add(parameterExpression);
					res = Expression.Block(new ParameterExpression[1] { parameterExpression }, list);
				}
				else
				{
					list.Add(expression);
					AddKeepAlives(argumentMarshallers, list);
					res = Expression.Block(list);
				}
				res = AddReturnChecks(sharedContext, args, res);
				return new DynamicMetaObject(Utils.Convert(res, typeof(object)), bindingRestrictions);
			}

			private Expression AddReturnChecks(CodeContext context, DynamicMetaObject[] args, Expression res)
			{
				PythonContext context2 = PythonContext.GetContext(context);
				object obj = Value.Getrestype();
				if (obj != null)
				{
					INativeType nativeType = obj as INativeType;
					object ret = null;
					if (nativeType == null)
					{
						ret = obj;
					}
					else if (!PythonOps.TryGetBoundAttr(context, nativeType, "_check_retval_", out ret))
					{
						ret = null;
					}
					if (ret != null)
					{
						res = Expression.Dynamic(context2.CompatInvoke(new CallInfo(1)), typeof(object), Expression.Constant(ret), res);
					}
				}
				object obj2 = Value.Geterrcheck();
				if (obj2 != null)
				{
					res = Expression.Dynamic(context2.CompatInvoke(new CallInfo(3)), typeof(object), Expression.Constant(obj2), res, base.Expression, Expression.Call(typeof(PythonOps).GetMethod("MakeTuple"), Expression.NewArrayInit(typeof(object), ArrayUtils.ConvertAll(args, (DynamicMetaObject x) => Utils.Convert(x.Expression, typeof(object))))));
				}
				return res;
			}

			private static DynamicMetaObject IncorrectArgCount(DynamicMetaObjectBinder binder, BindingRestrictions restrictions, int expected, int got)
			{
				return new DynamicMetaObject(binder.Throw(Expression.Call(typeof(PythonOps).GetMethod("TypeError"), Expression.Constant($"this function takes {expected} arguments ({got} given)"), Expression.NewArrayInit(typeof(object))), typeof(object)), restrictions);
			}

			private static DynamicMetaObject NoThisParam(DynamicMetaObjectBinder binder, BindingRestrictions restrictions)
			{
				return new DynamicMetaObject(binder.Throw(Expression.Call(typeof(PythonOps).GetMethod("ValueError"), Expression.Constant("native com method call without 'this' parameter"), Expression.NewArrayInit(typeof(object))), typeof(object)), restrictions);
			}

			private void AddKeepAlives(ArgumentMarshaller[] signature, List<Expression> block)
			{
				foreach (ArgumentMarshaller argumentMarshaller in signature)
				{
					Expression keepAlive = argumentMarshaller.GetKeepAlive();
					if (keepAlive != null)
					{
						block.Add(keepAlive);
					}
				}
			}

			private Expression MakeCall(ArgumentMarshaller[] signature, INativeType nativeRetType, bool retVoid, Expression address)
			{
				List<object> list = new List<object>();
				MethodInfo method = CreateInteropInvoker(GetCallingConvention(), signature, nativeRetType, retVoid, list);
				Expression[] array = new Expression[signature.Length + 2];
				array[0] = address;
				for (int i = 0; i < signature.Length; i++)
				{
					array[i + 1] = signature[i].ArgumentExpression;
				}
				array[array.Length - 1] = Expression.Constant(list.ToArray());
				return Expression.Call(method, array);
			}

			private Expression GetFunctionAddress(DynamicMetaObject[] args)
			{
				if (Value._comInterfaceIndex != -1)
				{
					return Expression.Call(typeof(ModuleOps).GetMethod("GetInterfacePointer"), Expression.Call(typeof(ModuleOps).GetMethod("GetPointer"), args[0].Expression), Expression.Constant(Value._comInterfaceIndex));
				}
				return Expression.Property(Expression.Convert(base.Expression, typeof(_CFuncPtr)), "addr");
			}

			private CallingConvention GetCallingConvention()
			{
				return Value.CallingConvention;
			}

			private INativeType GetNativeReturnType()
			{
				return Value.Getrestype() as INativeType;
			}

			private ArgumentMarshaller[] GetArgumentMarshallers(DynamicMetaObject[] args)
			{
				CFuncPtrType cFuncPtrType = (CFuncPtrType)Value.NativeType;
				ArgumentMarshaller[] array = new ArgumentMarshaller[args.Length];
				for (int i = 0; i < args.Length; i++)
				{
					DynamicMetaObject dynamicMetaObject = args[i];
					object nativeType = null;
					if (Value._comInterfaceIndex == -1 || i != 0)
					{
						int num = ((Value._comInterfaceIndex == -1) ? i : (i - 1));
						if (Value._argtypes != null && num < Value._argtypes.Count)
						{
							nativeType = Value._argtypes[num];
						}
						else if (cFuncPtrType._argtypes != null && num < cFuncPtrType._argtypes.Length)
						{
							nativeType = cFuncPtrType._argtypes[num];
						}
					}
					array[i] = GetMarshaller(dynamicMetaObject.Expression, dynamicMetaObject.Value, i, nativeType);
				}
				return array;
			}

			private ArgumentMarshaller GetMarshaller(Expression expr, object value, int index, object nativeType)
			{
				if (nativeType != null)
				{
					if (nativeType is INativeType cdataType)
					{
						return new CDataMarshaller(expr, CompilerHelpers.GetType(value), cdataType);
					}
					return new FromParamMarshaller(expr);
				}
				if (value is CData cData)
				{
					return new CDataMarshaller(expr, CompilerHelpers.GetType(value), cData.NativeType);
				}
				if (value is NativeArgument)
				{
					return new NativeArgumentMarshaller(expr);
				}
				if (PythonOps.TryGetBoundAttr(value, "_as_parameter_", out var _))
				{
					throw new NotImplementedException("_as_parameter");
				}
				return new PrimitiveMarshaller(expr, CompilerHelpers.GetType(value));
			}

			private static MethodInfo CreateInteropInvoker(CallingConvention convention, ArgumentMarshaller[] sig, INativeType nativeRetType, bool retVoid, List<object> constantPool)
			{
				Type[] array = new Type[sig.Length + 2];
				array[0] = typeof(IntPtr);
				for (int i = 0; i < sig.Length; i++)
				{
					array[i + 1] = sig[i].ArgumentExpression.Type;
				}
				array[array.Length - 1] = typeof(object[]);
				Type type = (retVoid ? typeof(void) : ((nativeRetType != null) ? nativeRetType.GetPythonType() : typeof(int)));
				Type type2 = (retVoid ? typeof(void) : ((nativeRetType != null) ? nativeRetType.GetNativeType() : typeof(int)));
				DynamicMethod dynamicMethod = new DynamicMethod("InteropInvoker", type, array, DynamicModule);
				ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
				LocalBuilder local = null;
				LocalBuilder local2 = null;
				if (dynamicMethod.ReturnType != typeof(void))
				{
					local = iLGenerator.DeclareLocal(type2);
					local2 = iLGenerator.DeclareLocal(dynamicMethod.ReturnType);
				}
				iLGenerator.BeginExceptionBlock();
				List<MarshalCleanup> list = null;
				for (int j = 0; j < sig.Length; j++)
				{
					MarshalCleanup marshalCleanup = sig[j].EmitCallStubArgument(iLGenerator, j + 1, constantPool, array.Length - 1);
					if (marshalCleanup != null)
					{
						if (list == null)
						{
							list = new List<MarshalCleanup>();
						}
						list.Add(marshalCleanup);
					}
				}
				iLGenerator.Emit(OpCodes.Ldarg_0);
				iLGenerator.Emit(OpCodes.Calli, GetCalliSignature(convention, sig, type2));
				if (type != typeof(void))
				{
					if (nativeRetType != null)
					{
						iLGenerator.Emit(OpCodes.Stloc, local);
						nativeRetType.EmitReverseMarshalling(iLGenerator, new Local(local), constantPool, sig.Length + 1);
						iLGenerator.Emit(OpCodes.Stloc, local2);
					}
					else
					{
						iLGenerator.Emit(OpCodes.Stloc, local2);
					}
				}
				iLGenerator.BeginFinallyBlock();
				if (list != null)
				{
					foreach (MarshalCleanup item in list)
					{
						item.Cleanup(iLGenerator);
					}
				}
				iLGenerator.EndExceptionBlock();
				if (type != typeof(void))
				{
					iLGenerator.Emit(OpCodes.Ldloc, local2);
				}
				iLGenerator.Emit(OpCodes.Ret);
				return dynamicMethod;
			}

			private static SignatureHelper GetCalliSignature(CallingConvention convention, ArgumentMarshaller[] sig, Type calliRetType)
			{
				SignatureHelper methodSigHelper = SignatureHelper.GetMethodSigHelper(convention, calliRetType);
				foreach (ArgumentMarshaller argumentMarshaller in sig)
				{
					methodSigHelper.AddArgument(argumentMarshaller.NativeType);
				}
				return methodSigHelper;
			}
		}

		private readonly Delegate _delegate;

		private readonly int _comInterfaceIndex = -1;

		private object _errcheck;

		private object _restype = _noResType;

		private IList<object> _argtypes;

		private int _id;

		private static int _curId = 0;

		internal static object _noResType = new object();

		public object argtypes
		{
			get
			{
				if (_argtypes != null)
				{
					return _argtypes;
				}
				if (((CFuncPtrType)base.NativeType)._argtypes != null)
				{
					return PythonTuple.MakeTuple(((CFuncPtrType)base.NativeType)._argtypes);
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					if (!(value is IList<object> list))
					{
						throw PythonOps.TypeErrorForTypeMismatch("sequence", value);
					}
					foreach (object item in list)
					{
						if (!(item is INativeType) && !PythonOps.HasAttr(DefaultContext.Default, item, "from_param"))
						{
							throw PythonOps.TypeErrorForTypeMismatch("ctype or object with from_param", item);
						}
					}
					_argtypes = list;
				}
				else
				{
					_argtypes = null;
				}
				_id = Interlocked.Increment(ref _curId);
			}
		}

		internal CallingConvention CallingConvention => ((CFuncPtrType)DynamicHelpers.GetPythonType(this)).CallingConvention;

		public IntPtr addr
		{
			[PythonHidden]
			get
			{
				return _memHolder.ReadIntPtr(0);
			}
			[PythonHidden]
			set
			{
				_memHolder.WriteIntPtr(0, value);
			}
		}

		internal int Id => _id;

		public _CFuncPtr(PythonTuple args)
		{
			if (args == null)
			{
				throw PythonOps.TypeError("expected sequence, got None");
			}
			if (args.Count != 2)
			{
				throw PythonOps.TypeError("argument 1 must be a sequence of length 2, not {0}", args.Count);
			}
			object obj = args[0];
			object dll = args[1];
			IntPtr handleFromObject = GetHandleFromObject(dll, "the _handle attribute of the second element must be an integer");
			string text = args[0] as string;
			IntPtr intPtr = ((text == null) ? NativeFunctions.LoadFunction(handleFromObject, new IntPtr((int)obj)) : NativeFunctions.LoadFunction(handleFromObject, text));
			if (intPtr == IntPtr.Zero)
			{
				if (CallingConvention == CallingConvention.StdCall && text != null)
				{
					string text2 = "_" + text + "@";
					for (int i = 0; i < 128; i += 4)
					{
						if (!(intPtr == IntPtr.Zero))
						{
							break;
						}
						intPtr = NativeFunctions.LoadFunction(handleFromObject, text2 + i);
					}
				}
				if (intPtr == IntPtr.Zero)
				{
					throw PythonOps.AttributeError("function {0} is not defined", args[0]);
				}
			}
			_memHolder = new MemoryHolder(IntPtr.Size);
			addr = intPtr;
			_id = Interlocked.Increment(ref _curId);
		}

		public _CFuncPtr()
		{
			_id = Interlocked.Increment(ref _curId);
			_memHolder = new MemoryHolder(IntPtr.Size);
		}

		public _CFuncPtr(CodeContext context, object function)
		{
			_memHolder = new MemoryHolder(IntPtr.Size);
			if (function != null)
			{
				if (!PythonOps.IsCallable(context, function))
				{
					throw PythonOps.TypeError("argument must be called or address of function");
				}
				_delegate = ((CFuncPtrType)DynamicHelpers.GetPythonType(this)).MakeReverseDelegate(context, function);
				addr = Marshal.GetFunctionPointerForDelegate(_delegate);
				CFuncPtrType cFuncPtrType = (CFuncPtrType)base.NativeType;
				PythonType restype = cFuncPtrType._restype;
				if (restype != null && (!(restype is INativeType) || restype is PointerType))
				{
					throw PythonOps.TypeError("invalid result type {0} for callback function", restype.Name);
				}
			}
			_id = Interlocked.Increment(ref _curId);
		}

		public _CFuncPtr(int index, string name)
		{
			_memHolder = new MemoryHolder(IntPtr.Size);
			_comInterfaceIndex = index;
			_id = Interlocked.Increment(ref _curId);
		}

		public _CFuncPtr(int handle)
		{
			_memHolder = new MemoryHolder(IntPtr.Size);
			addr = new IntPtr(handle);
			_id = Interlocked.Increment(ref _curId);
		}

		public _CFuncPtr([NotNull] BigInteger handle)
		{
			_memHolder = new MemoryHolder(IntPtr.Size);
			addr = new IntPtr((long)handle);
			_id = Interlocked.Increment(ref _curId);
		}

		public _CFuncPtr(IntPtr handle)
		{
			_memHolder = new MemoryHolder(IntPtr.Size);
			addr = handle;
			_id = Interlocked.Increment(ref _curId);
		}

		public bool __nonzero__()
		{
			return addr != IntPtr.Zero;
		}

		[SpecialName]
		[PropertyMethod]
		public object Geterrcheck()
		{
			return _errcheck;
		}

		[SpecialName]
		[PropertyMethod]
		public void Seterrcheck(object value)
		{
			_errcheck = value;
		}

		[SpecialName]
		[PropertyMethod]
		public void Deleteerrcheck()
		{
			_errcheck = null;
			_id = Interlocked.Increment(ref _curId);
		}

		[SpecialName]
		[PropertyMethod]
		public object Getrestype()
		{
			if (_restype == _noResType)
			{
				return ((CFuncPtrType)base.NativeType)._restype;
			}
			return _restype;
		}

		[SpecialName]
		[PropertyMethod]
		public void Setrestype(object value)
		{
			INativeType nativeType = value as INativeType;
			if (nativeType != null || value == null || PythonOps.IsCallable(((PythonType)base.NativeType).Context.SharedContext, value))
			{
				_restype = value;
				_id = Interlocked.Increment(ref _curId);
				return;
			}
			throw PythonOps.TypeError("restype must be a type, a callable, or None");
		}

		[SpecialName]
		[PropertyMethod]
		public void Deleterestype()
		{
			_restype = _noResType;
			_id = Interlocked.Increment(ref _curId);
		}

		[PythonHidden]
		public DynamicMetaObject GetMetaObject(Expression parameter)
		{
			return new Meta(parameter, this);
		}

		public string __repr__(CodeContext context)
		{
			if (_comInterfaceIndex != -1)
			{
				return $"<COM method offset {_comInterfaceIndex}: {DynamicHelpers.GetPythonType(this).Name} at {_id}>";
			}
			return ObjectOps.__repr__(this);
		}
	}

	[PythonType]
	[PythonHidden]
	public class CFuncPtrType : PythonType, INativeType
	{
		private struct DelegateCacheKey : IEquatable<DelegateCacheKey>
		{
			private readonly Type[] _types;

			private readonly CallingConvention _callConv;

			public DelegateCacheKey(Type[] sig, CallingConvention callingConvention)
			{
				_types = sig;
				_callConv = callingConvention;
			}

			public override int GetHashCode()
			{
				int num = _callConv.GetHashCode();
				for (int i = 0; i < _types.Length; i++)
				{
					num ^= _types[i].GetHashCode();
				}
				return num;
			}

			public override bool Equals(object obj)
			{
				if (obj is DelegateCacheKey)
				{
					return Equals((DelegateCacheKey)obj);
				}
				return false;
			}

			public bool Equals(DelegateCacheKey other)
			{
				if (other._types.Length != _types.Length || other._callConv != _callConv)
				{
					return false;
				}
				for (int i = 0; i < _types.Length; i++)
				{
					if (_types[i] != other._types[i])
					{
						return false;
					}
				}
				return true;
			}
		}

		internal readonly int _flags;

		internal readonly PythonType _restype;

		internal readonly INativeType[] _argtypes;

		private DynamicMethod _reverseDelegate;

		private List<object> _reverseDelegateConstants;

		private Type _reverseDelegateType;

		private static Dictionary<DelegateCacheKey, Type> _reverseDelegates = new Dictionary<DelegateCacheKey, Type>();

		public object internal_restype => _restype;

		int INativeType.Size => IntPtr.Size;

		int INativeType.Alignment => IntPtr.Size;

		string INativeType.TypeFormat => "X{}";

		internal CallingConvention CallingConvention => (_flags & 7) switch
		{
			0 => CallingConvention.StdCall, 
			1 => CallingConvention.Cdecl, 
			_ => CallingConvention.Cdecl, 
		};

		public CFuncPtrType(CodeContext context, string name, PythonTuple bases, PythonDictionary members)
			: base(context, name, bases, members)
		{
			if (!members.TryGetValue("_flags_", out var value) || !(value is int))
			{
				throw PythonOps.TypeError("class must define _flags_ which must be an integer");
			}
			_flags = (int)value;
			if (members.TryGetValue("_restype_", out var value2) && value2 is PythonType)
			{
				_restype = (PythonType)value2;
			}
			if (members.TryGetValue("_argtypes_", out var value3) && value3 is PythonTuple)
			{
				PythonTuple pythonTuple = value3 as PythonTuple;
				_argtypes = new INativeType[pythonTuple.Count];
				for (int i = 0; i < pythonTuple.Count; i++)
				{
					_argtypes[i] = (INativeType)pythonTuple[i];
				}
			}
		}

		private CFuncPtrType(Type underlyingSystemType)
			: base(underlyingSystemType)
		{
		}

		internal static PythonType MakeSystemType(Type underlyingSystemType)
		{
			return PythonType.SetPythonType(underlyingSystemType, new CFuncPtrType(underlyingSystemType));
		}

		public object from_param(object obj)
		{
			return null;
		}

		object INativeType.GetValue(MemoryHolder owner, object readingFrom, int offset, bool raw)
		{
			IntPtr intPtr = owner.ReadIntPtr(offset);
			if (raw)
			{
				return intPtr.ToPython();
			}
			return CreateInstance(base.Context.SharedContext, intPtr);
		}

		object INativeType.SetValue(MemoryHolder address, int offset, object value)
		{
			if (value is int)
			{
				address.WriteIntPtr(offset, new IntPtr((int)value));
			}
			else
			{
				if (!(value is BigInteger))
				{
					if (value is _CFuncPtr)
					{
						address.WriteIntPtr(offset, ((_CFuncPtr)value).addr);
						return value;
					}
					throw PythonOps.TypeErrorForTypeMismatch("func pointer", value);
				}
				address.WriteIntPtr(offset, new IntPtr((long)(BigInteger)value));
			}
			return null;
		}

		Type INativeType.GetNativeType()
		{
			return typeof(IntPtr);
		}

		MarshalCleanup INativeType.EmitMarshalling(ILGenerator method, LocalOrArg argIndex, List<object> constantPool, int constantPoolArgument)
		{
			Type type = argIndex.Type;
			argIndex.Emit(method);
			if (type.IsValueType)
			{
				method.Emit(OpCodes.Box, type);
			}
			constantPool.Add(this);
			method.Emit(OpCodes.Ldarg, constantPoolArgument);
			method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
			method.Emit(OpCodes.Ldelem_Ref);
			method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("GetFunctionPointerValue"));
			return null;
		}

		Type INativeType.GetPythonType()
		{
			return typeof(_CFuncPtr);
		}

		void INativeType.EmitReverseMarshalling(ILGenerator method, LocalOrArg value, List<object> constantPool, int constantPoolArgument)
		{
			value.Emit(method);
			constantPool.Add(this);
			method.Emit(OpCodes.Ldarg, constantPoolArgument);
			method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
			method.Emit(OpCodes.Ldelem_Ref);
			method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CreateCFunction"));
		}

		internal Delegate MakeReverseDelegate(CodeContext context, object target)
		{
			if (_reverseDelegate == null)
			{
				lock (this)
				{
					if (_reverseDelegate == null)
					{
						MakeReverseDelegateWorker(context);
					}
				}
			}
			object[] array = _reverseDelegateConstants.ToArray();
			array[0] = target;
			return _reverseDelegate.CreateDelegate(_reverseDelegateType, array);
		}

		private void MakeReverseDelegateWorker(CodeContext context)
		{
			GetSignatureInfo(out var sigTypes, out var callSiteType, out var retType);
			DynamicMethod dynamicMethod = new DynamicMethod("ReverseInteropInvoker", retType, ArrayUtils.RemoveLast(sigTypes), DynamicModule);
			ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
			PythonContext context2 = PythonContext.GetContext(context);
			Type type = CompilerHelpers.MakeCallSiteDelegateType(callSiteType);
			CallSite callSite = CallSite.Create(type, context2.Invoke(new CallSignature(_argtypes.Length)));
			List<object> list = new List<object>();
			list.Add(null);
			list.Add(callSite);
			iLGenerator.BeginExceptionBlock();
			LocalBuilder local = iLGenerator.DeclareLocal(callSite.GetType());
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldc_I4, list.Count - 1);
			iLGenerator.Emit(OpCodes.Ldelem_Ref);
			iLGenerator.Emit(OpCodes.Castclass, callSite.GetType());
			iLGenerator.Emit(OpCodes.Stloc, local);
			iLGenerator.Emit(OpCodes.Ldloc, local);
			iLGenerator.Emit(OpCodes.Ldfld, callSite.GetType().GetField("Target"));
			iLGenerator.Emit(OpCodes.Ldloc, local);
			int count = list.Count;
			list.Add(context2.SharedContext);
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldc_I4, count);
			iLGenerator.Emit(OpCodes.Ldelem_Ref);
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldc_I4_0);
			iLGenerator.Emit(OpCodes.Ldelem_Ref);
			for (int i = 0; i < _argtypes.Length; i++)
			{
				INativeType nativeType = _argtypes[i];
				nativeType.EmitReverseMarshalling(iLGenerator, new Arg(i + 1, sigTypes[i + 1]), list, 0);
			}
			iLGenerator.Emit(OpCodes.Call, type.GetMethod("Invoke"));
			LocalBuilder local2 = null;
			if (_restype != null)
			{
				LocalBuilder local3 = iLGenerator.DeclareLocal(typeof(object));
				iLGenerator.Emit(OpCodes.Stloc, local3);
				local2 = iLGenerator.DeclareLocal(retType);
				((INativeType)_restype).EmitMarshalling(iLGenerator, new Local(local3), list, 0);
				iLGenerator.Emit(OpCodes.Stloc, local2);
			}
			else
			{
				iLGenerator.Emit(OpCodes.Pop);
			}
			iLGenerator.BeginCatchBlock(typeof(Exception));
			iLGenerator.Emit(OpCodes.Ldarg_0);
			iLGenerator.Emit(OpCodes.Ldc_I4, count);
			iLGenerator.Emit(OpCodes.Ldelem_Ref);
			iLGenerator.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CallbackException"));
			iLGenerator.EndExceptionBlock();
			if (_restype != null)
			{
				iLGenerator.Emit(OpCodes.Ldloc, local2);
			}
			iLGenerator.Emit(OpCodes.Ret);
			_reverseDelegateConstants = list;
			_reverseDelegateType = GetReverseDelegateType(ArrayUtils.RemoveFirst(sigTypes), CallingConvention);
			_reverseDelegate = dynamicMethod;
		}

		private void GetSignatureInfo(out Type[] sigTypes, out Type[] callSiteType, out Type retType)
		{
			sigTypes = new Type[_argtypes.Length + 2];
			callSiteType = new Type[_argtypes.Length + 4];
			sigTypes[0] = typeof(object[]);
			callSiteType[0] = typeof(CallSite);
			callSiteType[1] = typeof(CodeContext);
			callSiteType[2] = typeof(object);
			callSiteType[callSiteType.Length - 1] = typeof(object);
			for (int i = 0; i < _argtypes.Length; i++)
			{
				sigTypes[i + 1] = _argtypes[i].GetNativeType();
				callSiteType[i + 3] = _argtypes[i].GetPythonType();
			}
			if (_restype != null)
			{
				sigTypes[sigTypes.Length - 1] = (retType = ((INativeType)_restype).GetNativeType());
			}
			else
			{
				sigTypes[sigTypes.Length - 1] = (retType = typeof(void));
			}
		}

		private static Type GetReverseDelegateType(Type[] nativeSig, CallingConvention callingConvention)
		{
			Type value;
			lock (_reverseDelegates)
			{
				DelegateCacheKey key = new DelegateCacheKey(nativeSig, callingConvention);
				if (!_reverseDelegates.TryGetValue(key, out value))
				{
					Type type = (_reverseDelegates[key] = PythonOps.MakeNewCustomDelegate(nativeSig, callingConvention));
					value = type;
					return value;
				}
			}
			return value;
		}
	}

	[PythonType]
	[PythonHidden]
	public sealed class Field : PythonTypeDataSlot, ICodeFormattable
	{
		private readonly INativeType _fieldType;

		private readonly int _offset;

		private readonly int _index;

		private readonly int _bits = -1;

		private readonly int _bitsOffset;

		private readonly string _fieldName;

		public int offset => _offset;

		public int size => _fieldType.Size;

		internal override bool GetAlwaysSucceeds => true;

		internal INativeType NativeType => _fieldType;

		internal int? BitCount
		{
			get
			{
				if (_bits == -1)
				{
					return null;
				}
				return _bits;
			}
		}

		internal string FieldName => _fieldName;

		private bool IsSignedType
		{
			get
			{
				switch (((SimpleType)_fieldType)._type)
				{
				case SimpleTypeKind.SignedByte:
				case SimpleTypeKind.SignedShort:
				case SimpleTypeKind.SignedInt:
				case SimpleTypeKind.SignedLong:
				case SimpleTypeKind.SignedLongLong:
					return true;
				default:
					return false;
				}
			}
		}

		internal Field(string fieldName, INativeType fieldType, int offset, int index)
		{
			_offset = offset;
			_fieldType = fieldType;
			_index = index;
			_fieldName = fieldName;
		}

		internal Field(string fieldName, INativeType fieldType, int offset, int index, int? bits, int? bitOffset)
		{
			_offset = offset;
			_fieldType = fieldType;
			_index = index;
			_fieldName = fieldName;
			if (bits.HasValue)
			{
				_bits = bits.Value;
				_bitsOffset = bitOffset.Value;
			}
		}

		internal override bool TryGetValue(CodeContext context, object instance, PythonType owner, out object value)
		{
			if (instance != null)
			{
				CData cData = (CData)instance;
				value = _fieldType.GetValue(cData._memHolder, cData, _offset, raw: false);
				if (_bits == -1)
				{
					return true;
				}
				value = ExtractBits(value);
				return true;
			}
			value = this;
			return true;
		}

		internal override bool TrySetValue(CodeContext context, object instance, PythonType owner, object value)
		{
			if (instance != null)
			{
				SetValue(((CData)instance)._memHolder, 0, value);
				return true;
			}
			return base.TrySetValue(context, instance, owner, value);
		}

		internal void SetValue(MemoryHolder address, int baseOffset, object value)
		{
			if (_bits == -1)
			{
				object obj = _fieldType.SetValue(address, baseOffset + _offset, value);
				if (obj != null)
				{
					address.AddObject(_index.ToString(), obj);
				}
			}
			else
			{
				SetBitsValue(address, baseOffset, value);
			}
		}

		internal override bool TryDeleteValue(CodeContext context, object instance, PythonType owner)
		{
			throw PythonOps.TypeError("cannot delete fields in ctypes structures/unions");
		}

		public string __repr__(CodeContext context)
		{
			if (_bits == -1)
			{
				return $"<Field type={((PythonType)_fieldType).Name}, ofs={offset}, size={size}>";
			}
			return $"<Field type={((PythonType)_fieldType).Name}, ofs={offset}:{_bitsOffset}, bits={_bits}>";
		}

		private object ExtractBits(object value)
		{
			if (value is int)
			{
				int num = (1 << _bits) - 1;
				int num2 = (int)value;
				num2 = (num2 >> _bitsOffset) & num;
				if (IsSignedType && (num2 & (1 << _bits - 1)) != 0)
				{
					num2 |= -1 ^ num;
				}
				value = ScriptingRuntimeHelpers.Int32ToObject(num2);
			}
			else
			{
				ulong num3 = (ulong)((1L << _bits) - 1);
				BigInteger bigInteger = (BigInteger)value;
				ulong num4 = ((!IsSignedType) ? ((ulong)bigInteger) : ((ulong)(long)bigInteger));
				num4 = (num4 >> _bitsOffset) & num3;
				if (IsSignedType)
				{
					if ((num4 & (ulong)(1L << _bits - 1)) != 0)
					{
						num4 |= 0xFFFFFFFFFFFFFFFFuL ^ num3;
					}
					value = (BigInteger)(long)num4;
				}
				else
				{
					value = (BigInteger)num4;
				}
			}
			return value;
		}

		private void SetBitsValue(MemoryHolder address, int baseOffset, object value)
		{
			ulong num;
			if (value is int)
			{
				num = (ulong)(int)value;
			}
			else
			{
				if (!(value is BigInteger))
				{
					throw PythonOps.TypeErrorForTypeMismatch("int or long", value);
				}
				num = (ulong)(long)(BigInteger)value;
			}
			int num2 = checked(_offset + baseOffset);
			object value2 = _fieldType.GetValue(address, null, num2, raw: false);
			ulong num3 = (ulong)((!(value2 is int)) ? ((long)(BigInteger)value2) : ((int)value2));
			ulong num4 = (ulong)((1L << _bits) - 1 << _bitsOffset);
			num3 &= ~num4;
			num3 |= (num << _bitsOffset) & num4;
			if (IsSignedType)
			{
				if (_fieldType.Size <= 4)
				{
					_fieldType.SetValue(address, num2, (int)num3);
				}
				else
				{
					_fieldType.SetValue(address, num2, (BigInteger)(long)num3);
				}
			}
			else if (_fieldType.Size < 4)
			{
				_fieldType.SetValue(address, num2, (int)num3);
			}
			else
			{
				_fieldType.SetValue(address, num2, (BigInteger)num3);
			}
		}
	}

	[PythonType]
	[PythonHidden]
	public sealed class NativeArgument : ICodeFormattable
	{
		private readonly CData __obj;

		private readonly string _type;

		public CData _obj => __obj;

		internal NativeArgument(CData value, string type)
		{
			__obj = value;
			_type = type;
		}

		public string __repr__(CodeContext context)
		{
			return $"<cparam '{_type}' ({IdDispenser.GetId(__obj)})>";
		}
	}

	[PythonType("_Pointer")]
	public abstract class Pointer : CData
	{
		private readonly CData _object;

		public object contents
		{
			get
			{
				PythonType pythonType = (PythonType)((PointerType)base.NativeType)._type;
				CData cData = (CData)pythonType.CreateInstance(pythonType.Context.SharedContext);
				cData._memHolder = _memHolder.ReadMemoryHolder(0);
				if (cData._memHolder.UnsafeAddress == IntPtr.Zero)
				{
					throw PythonOps.ValueError("NULL value access");
				}
				return cData;
			}
			set
			{
			}
		}

		public object this[int index]
		{
			get
			{
				INativeType type = ((PointerType)base.NativeType)._type;
				MemoryHolder owner = _memHolder.ReadMemoryHolder(0);
				return type.GetValue(owner, this, checked(type.Size * index), raw: false);
			}
			set
			{
				MemoryHolder address = _memHolder.ReadMemoryHolder(0);
				INativeType type = ((PointerType)base.NativeType)._type;
				object obj = type.SetValue(address, checked(type.Size * index), value);
				if (obj != null)
				{
					_memHolder.AddObject(index.ToString(), obj);
				}
			}
		}

		public object this[Slice index]
		{
			get
			{
				if (index.stop == null)
				{
					throw PythonOps.ValueError("slice stop is required");
				}
				int num = ((index.start != null) ? ((int)index.start) : 0);
				int num2 = ((index.stop != null) ? ((int)index.stop) : 0);
				int num3 = ((index.step == null) ? 1 : ((int)index.step));
				if (num3 < 0 && index.start == null)
				{
					throw PythonOps.ValueError("slice start is required for step < 0");
				}
				if (num < 0)
				{
					num = 0;
				}
				INativeType type = ((PointerType)base.NativeType)._type;
				SimpleType simpleType = type as SimpleType;
				if ((num2 < num && num3 > 0) || (num < num2 && num3 < 0))
				{
					if (simpleType != null && (simpleType._type == SimpleTypeKind.WChar || simpleType._type == SimpleTypeKind.Char))
					{
						return string.Empty;
					}
					return new List();
				}
				MemoryHolder owner = _memHolder.ReadMemoryHolder(0);
				if (simpleType != null && (simpleType._type == SimpleTypeKind.WChar || simpleType._type == SimpleTypeKind.Char))
				{
					int size = ((INativeType)simpleType).Size;
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = num; (num2 > num) ? (i < num2) : (i > num2); i += num3)
					{
						stringBuilder.Append(simpleType.ReadChar(owner, checked(i * size)));
					}
					return stringBuilder.ToString();
				}
				List list = new List((num2 - num) / num3);
				for (int j = num; (num2 > num) ? (j < num2) : (j > num2); j += num3)
				{
					list.AddNoLock(type.GetValue(owner, this, checked(type.Size * j), raw: false));
				}
				return list;
			}
		}

		public Pointer()
		{
			_memHolder = new MemoryHolder(IntPtr.Size);
		}

		public Pointer(CData value)
		{
			_object = value;
			_memHolder = new MemoryHolder(IntPtr.Size);
			_memHolder.WriteIntPtr(0, value._memHolder);
			_memHolder.AddObject("1", value);
			if (value._objects != null)
			{
				_memHolder.AddObject("0", value._objects);
			}
		}

		public bool __nonzero__()
		{
			return _memHolder.ReadIntPtr(0) != IntPtr.Zero;
		}
	}

	[PythonType]
	[PythonHidden]
	public class PointerType : PythonType, INativeType
	{
		internal INativeType _type;

		private readonly string _typeFormat;

		int INativeType.Size => IntPtr.Size;

		int INativeType.Alignment => IntPtr.Size;

		string INativeType.TypeFormat => "&" + (_typeFormat ?? _type.TypeFormat);

		public PointerType(CodeContext context, string name, PythonTuple bases, PythonDictionary members)
			: base(context, name, bases, members)
		{
			if (members.TryGetValue("_type_", out var value) && !(value is INativeType))
			{
				throw PythonOps.TypeError("_type_ must be a type");
			}
			_type = (INativeType)value;
			if (_type != null)
			{
				_typeFormat = _type.TypeFormat;
			}
		}

		private PointerType(Type underlyingSystemType)
			: base(underlyingSystemType)
		{
		}

		public object from_param([NotNull] CData obj)
		{
			return new NativeArgument((CData)PythonCalls.Call(this, obj), "P");
		}

		public object from_param(Pointer obj)
		{
			if (obj == null)
			{
				return ScriptingRuntimeHelpers.Int32ToObject(0);
			}
			if (obj.NativeType != this)
			{
				throw PythonOps.TypeError("assign to pointer of type {0} from {1} is not valid", base.Name, ((PythonType)obj.NativeType).Name);
			}
			Pointer pointer = (Pointer)PythonCalls.Call(this);
			pointer._memHolder.WriteIntPtr(0, obj._memHolder.ReadMemoryHolder(0));
			return pointer;
		}

		public object from_param([NotNull] NativeArgument obj)
		{
			return (CData)PythonCalls.Call(this, obj._obj);
		}

		public object from_address(object obj)
		{
			throw new NotImplementedException("pointer from address");
		}

		public void set_type(PythonType type)
		{
			_type = (INativeType)type;
		}

		internal static PythonType MakeSystemType(Type underlyingSystemType)
		{
			return PythonType.SetPythonType(underlyingSystemType, new PointerType(underlyingSystemType));
		}

		public static ArrayType operator *(PointerType type, int count)
		{
			return MakeArrayType(type, count);
		}

		public static ArrayType operator *(int count, PointerType type)
		{
			return MakeArrayType(type, count);
		}

		object INativeType.GetValue(MemoryHolder owner, object readingFrom, int offset, bool raw)
		{
			if (!raw)
			{
				Pointer pointer = (Pointer)PythonCalls.Call(base.Context.SharedContext, this);
				pointer._memHolder.WriteIntPtr(0, owner.ReadIntPtr(offset));
				pointer._memHolder.AddObject(offset, readingFrom);
				return pointer;
			}
			return owner.ReadIntPtr(offset).ToPython();
		}

		object INativeType.SetValue(MemoryHolder address, int offset, object value)
		{
			if (value == null)
			{
				address.WriteIntPtr(offset, IntPtr.Zero);
			}
			else if (value is int)
			{
				address.WriteIntPtr(offset, new IntPtr((int)value));
			}
			else
			{
				if (!(value is BigInteger))
				{
					if (value is Pointer pointer)
					{
						address.WriteIntPtr(offset, pointer._memHolder.ReadMemoryHolder(0));
						return PythonOps.MakeDictFromItems(pointer, "0", pointer._objects, "1");
					}
					if (value is _Array array)
					{
						address.WriteIntPtr(offset, array._memHolder);
						return array;
					}
					throw PythonOps.TypeErrorForTypeMismatch(base.Name, value);
				}
				address.WriteIntPtr(offset, new IntPtr((long)(BigInteger)value));
			}
			return null;
		}

		Type INativeType.GetNativeType()
		{
			return typeof(IntPtr);
		}

		MarshalCleanup INativeType.EmitMarshalling(ILGenerator method, LocalOrArg argIndex, List<object> constantPool, int constantPoolArgument)
		{
			Type type = argIndex.Type;
			Label label = method.DefineLabel();
			Label label2 = method.DefineLabel();
			if (!type.IsValueType)
			{
				argIndex.Emit(method);
				method.Emit(OpCodes.Ldnull);
				method.Emit(OpCodes.Bne_Un, label);
				method.Emit(OpCodes.Ldc_I4_0);
				method.Emit(OpCodes.Conv_I);
				method.Emit(OpCodes.Br, label2);
			}
			method.MarkLabel(label);
			label = method.DefineLabel();
			argIndex.Emit(method);
			if (type.IsValueType)
			{
				method.Emit(OpCodes.Box, type);
			}
			constantPool.Add(this);
			SimpleType simpleType = _type as SimpleType;
			MarshalCleanup result = null;
			if (simpleType != null && !argIndex.Type.IsValueType && (simpleType._type == SimpleTypeKind.Char || simpleType._type == SimpleTypeKind.WChar))
			{
				if (simpleType._type == SimpleTypeKind.Char)
				{
					SimpleType.TryToCharPtrConversion(method, argIndex, type, label2);
				}
				else
				{
					SimpleType.TryArrayToWCharPtrConversion(method, argIndex, type, label2);
				}
				Label label3 = method.DefineLabel();
				LocalOrArg argIndex2 = argIndex;
				if (type != typeof(string))
				{
					LocalBuilder local = method.DeclareLocal(typeof(string));
					method.Emit(OpCodes.Isinst, typeof(string));
					method.Emit(OpCodes.Brfalse, label3);
					argIndex.Emit(method);
					method.Emit(OpCodes.Castclass, typeof(string));
					method.Emit(OpCodes.Stloc, local);
					method.Emit(OpCodes.Ldloc, local);
					argIndex2 = new Local(local);
				}
				if (simpleType._type == SimpleTypeKind.Char)
				{
					result = SimpleType.MarshalCharPointer(method, argIndex2);
				}
				else
				{
					SimpleType.MarshalWCharPointer(method, argIndex2);
				}
				method.Emit(OpCodes.Br, label2);
				method.MarkLabel(label3);
				argIndex.Emit(method);
			}
			method.Emit(OpCodes.Ldarg, constantPoolArgument);
			method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
			method.Emit(OpCodes.Ldelem_Ref);
			method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CheckNativeArgument"));
			method.Emit(OpCodes.Dup);
			method.Emit(OpCodes.Brfalse, label);
			method.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
			method.Emit(OpCodes.Br, label2);
			method.MarkLabel(label);
			label = method.DefineLabel();
			method.Emit(OpCodes.Pop);
			argIndex.Emit(method);
			if (type.IsValueType)
			{
				method.Emit(OpCodes.Box, type);
			}
			method.Emit(OpCodes.Ldarg, constantPoolArgument);
			method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
			method.Emit(OpCodes.Ldelem_Ref);
			method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("TryCheckCDataPointerType"));
			method.Emit(OpCodes.Dup);
			method.Emit(OpCodes.Brfalse, label);
			method.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
			method.Emit(OpCodes.Br, label2);
			method.MarkLabel(label);
			method.Emit(OpCodes.Pop);
			argIndex.Emit(method);
			if (type.IsValueType)
			{
				method.Emit(OpCodes.Box, type);
			}
			method.Emit(OpCodes.Ldarg, constantPoolArgument);
			method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
			method.Emit(OpCodes.Ldelem_Ref);
			method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CheckCDataType"));
			method.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
			method.Emit(OpCodes.Ldind_I);
			method.MarkLabel(label2);
			return result;
		}

		Type INativeType.GetPythonType()
		{
			return typeof(object);
		}

		void INativeType.EmitReverseMarshalling(ILGenerator method, LocalOrArg value, List<object> constantPool, int constantPoolArgument)
		{
			value.Emit(method);
			EmitCDataCreation(this, method, constantPool, constantPoolArgument);
		}
	}

	[PythonType("_SimpleCData")]
	public abstract class SimpleCData : CData, ICodeFormattable
	{
		public override object _objects
		{
			get
			{
				if (IsString)
				{
					PythonDictionary objects = _memHolder.Objects;
					if (objects != null)
					{
						return objects["str"];
					}
				}
				return _memHolder.Objects;
			}
		}

		private bool IsString
		{
			get
			{
				if (((SimpleType)base.NativeType)._type != SimpleTypeKind.CharPointer)
				{
					return ((SimpleType)base.NativeType)._type == SimpleTypeKind.WCharPointer;
				}
				return true;
			}
		}

		protected SimpleCData()
		{
		}

		protected SimpleCData(params object[] args)
		{
		}

		public void __init__()
		{
			_memHolder = new MemoryHolder(base.Size);
		}

		public void __init__(object value)
		{
			_memHolder = new MemoryHolder(base.Size);
			base.NativeType.SetValue(_memHolder, 0, value);
			if (IsString)
			{
				_memHolder.AddObject("str", value);
			}
		}

		[SpecialName]
		[PropertyMethod]
		public void Setvalue(object value)
		{
			base.NativeType.SetValue(_memHolder, 0, value);
			if (IsString)
			{
				_memHolder.AddObject("str", value);
			}
		}

		[SpecialName]
		[PropertyMethod]
		public object Getvalue()
		{
			return base.NativeType.GetValue(_memHolder, this, 0, raw: true);
		}

		[SpecialName]
		[PropertyMethod]
		public void Deletevalue()
		{
			throw PythonOps.TypeError("cannot delete value property from simple cdata");
		}

		public string __repr__(CodeContext context)
		{
			if (DynamicHelpers.GetPythonType(this).BaseTypes[0] == _SimpleCData)
			{
				return $"{DynamicHelpers.GetPythonType(this).Name}({GetDataRepr(context)})";
			}
			return ObjectOps.__repr__(this);
		}

		private string GetDataRepr(CodeContext context)
		{
			return PythonOps.Repr(context, base.NativeType.GetValue(_memHolder, this, 0, raw: false));
		}
	}

	[PythonType]
	[PythonHidden]
	public class SimpleType : PythonType, INativeType
	{
		internal readonly SimpleTypeKind _type;

		private readonly char _charType;

		int INativeType.Size
		{
			get
			{
				switch (_type)
				{
				case SimpleTypeKind.Char:
				case SimpleTypeKind.SignedByte:
				case SimpleTypeKind.UnsignedByte:
				case SimpleTypeKind.Boolean:
					return 1;
				case SimpleTypeKind.SignedShort:
				case SimpleTypeKind.UnsignedShort:
				case SimpleTypeKind.WChar:
				case SimpleTypeKind.VariantBool:
					return 2;
				case SimpleTypeKind.SignedInt:
				case SimpleTypeKind.UnsignedInt:
				case SimpleTypeKind.SignedLong:
				case SimpleTypeKind.UnsignedLong:
				case SimpleTypeKind.Single:
					return 4;
				case SimpleTypeKind.Double:
				case SimpleTypeKind.SignedLongLong:
				case SimpleTypeKind.UnsignedLongLong:
					return 8;
				case SimpleTypeKind.Object:
				case SimpleTypeKind.Pointer:
				case SimpleTypeKind.CharPointer:
				case SimpleTypeKind.WCharPointer:
				case SimpleTypeKind.BStr:
					return IntPtr.Size;
				default:
					throw new InvalidOperationException(_type.ToString());
				}
			}
		}

		int INativeType.Alignment => ((INativeType)this).Size;

		private bool IsSubClass
		{
			get
			{
				if (base.BaseTypes.Count == 1)
				{
					return base.BaseTypes[0] != _SimpleCData;
				}
				return true;
			}
		}

		string INativeType.TypeFormat => (BitConverter.IsLittleEndian ? '<' : '>') + _charType.ToString();

		public SimpleType(CodeContext context, string name, PythonTuple bases, PythonDictionary dict)
			: base(context, name, bases, dict)
		{
			string text;
			if (!TryGetBoundCustomMember(context, "_type_", out var value) || (text = StringOps.AsString(value)) == null || text.Length != 1 || "?cbBghHiIlLdfuzZqQPXOv".IndexOf(text[0]) == -1)
			{
				throw PythonOps.AttributeError("AttributeError: class must define a '_type_' attribute which must be a single character string containing one of '{0}'.", "?cbBghHiIlLdfuzZqQPXOv");
			}
			_charType = text[0];
			switch (text[0])
			{
			case '?':
				_type = SimpleTypeKind.Boolean;
				break;
			case 'c':
				_type = SimpleTypeKind.Char;
				break;
			case 'b':
				_type = SimpleTypeKind.SignedByte;
				break;
			case 'B':
				_type = SimpleTypeKind.UnsignedByte;
				break;
			case 'h':
				_type = SimpleTypeKind.SignedShort;
				break;
			case 'H':
				_type = SimpleTypeKind.UnsignedShort;
				break;
			case 'i':
				_type = SimpleTypeKind.SignedInt;
				break;
			case 'I':
				_type = SimpleTypeKind.UnsignedInt;
				break;
			case 'l':
				_type = SimpleTypeKind.SignedLong;
				break;
			case 'L':
				_type = SimpleTypeKind.UnsignedLong;
				break;
			case 'f':
				_type = SimpleTypeKind.Single;
				break;
			case 'd':
			case 'g':
				_type = SimpleTypeKind.Double;
				break;
			case 'q':
				_type = SimpleTypeKind.SignedLongLong;
				break;
			case 'Q':
				_type = SimpleTypeKind.UnsignedLongLong;
				break;
			case 'O':
				_type = SimpleTypeKind.Object;
				break;
			case 'P':
				_type = SimpleTypeKind.Pointer;
				break;
			case 'z':
				_type = SimpleTypeKind.CharPointer;
				break;
			case 'Z':
				_type = SimpleTypeKind.WCharPointer;
				break;
			case 'u':
				_type = SimpleTypeKind.WChar;
				break;
			case 'v':
				_type = SimpleTypeKind.VariantBool;
				break;
			case 'X':
				_type = SimpleTypeKind.BStr;
				break;
			default:
				throw new NotImplementedException("simple type " + text);
			}
		}

		private SimpleType(Type underlyingSystemType)
			: base(underlyingSystemType)
		{
		}

		public static ArrayType operator *(SimpleType type, int count)
		{
			return MakeArrayType(type, count);
		}

		public static ArrayType operator *(int count, SimpleType type)
		{
			return MakeArrayType(type, count);
		}

		internal static PythonType MakeSystemType(Type underlyingSystemType)
		{
			return PythonType.SetPythonType(underlyingSystemType, new SimpleType(underlyingSystemType));
		}

		public SimpleCData from_address(CodeContext context, int address)
		{
			return from_address(context, new IntPtr(address));
		}

		public SimpleCData from_address(CodeContext context, BigInteger address)
		{
			return from_address(context, new IntPtr((long)address));
		}

		public SimpleCData from_address(CodeContext context, IntPtr ptr)
		{
			SimpleCData simpleCData = (SimpleCData)CreateInstance(context);
			simpleCData.SetAddress(ptr);
			return simpleCData;
		}

		public SimpleCData from_buffer(ArrayModule.array array, [DefaultParameterValue(0)] int offset)
		{
			ValidateArraySizes(array, offset, ((INativeType)this).Size);
			SimpleCData simpleCData = (SimpleCData)CreateInstance(base.Context.SharedContext);
			IntPtr arrayAddress = array.GetArrayAddress();
			simpleCData._memHolder = new MemoryHolder(arrayAddress.Add(offset), ((INativeType)this).Size);
			simpleCData._memHolder.AddObject("ffffffff", array);
			return simpleCData;
		}

		public SimpleCData from_buffer_copy(ArrayModule.array array, [DefaultParameterValue(0)] int offset)
		{
			ValidateArraySizes(array, offset, ((INativeType)this).Size);
			SimpleCData simpleCData = (SimpleCData)CreateInstance(base.Context.SharedContext);
			simpleCData._memHolder = new MemoryHolder(((INativeType)this).Size);
			simpleCData._memHolder.CopyFrom(array.GetArrayAddress().Add(offset), new IntPtr(((INativeType)this).Size));
			GC.KeepAlive(array);
			return simpleCData;
		}

		public object from_param(object obj)
		{
			return new NativeArgument((CData)PythonCalls.Call(this, obj), _charType.ToString());
		}

		public SimpleCData in_dll(CodeContext context, object library, string name)
		{
			IntPtr handleFromObject = GetHandleFromObject(library, "in_dll expected object with _handle attribute");
			IntPtr intPtr = NativeFunctions.LoadFunction(handleFromObject, name);
			if (intPtr == IntPtr.Zero)
			{
				throw PythonOps.ValueError("{0} not found when attempting to load {1} from dll", name, base.Name);
			}
			SimpleCData simpleCData = (SimpleCData)CreateInstance(context);
			simpleCData.SetAddress(intPtr);
			return simpleCData;
		}

		object INativeType.GetValue(MemoryHolder owner, object readingFrom, int offset, bool raw)
		{
			object obj = _type switch
			{
				SimpleTypeKind.Boolean => (owner.ReadByte(offset) != 0) ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False, 
				SimpleTypeKind.Char => new string((char)owner.ReadByte(offset), 1), 
				SimpleTypeKind.SignedByte => GetIntReturn((sbyte)owner.ReadByte(offset)), 
				SimpleTypeKind.UnsignedByte => GetIntReturn(owner.ReadByte(offset)), 
				SimpleTypeKind.SignedShort => GetIntReturn(owner.ReadInt16(offset)), 
				SimpleTypeKind.WChar => new string((char)owner.ReadInt16(offset), 1), 
				SimpleTypeKind.UnsignedShort => GetIntReturn((ushort)owner.ReadInt16(offset)), 
				SimpleTypeKind.VariantBool => (owner.ReadInt16(offset) != 0) ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False, 
				SimpleTypeKind.SignedInt => GetIntReturn(owner.ReadInt32(offset)), 
				SimpleTypeKind.UnsignedInt => GetIntReturn((uint)owner.ReadInt32(offset)), 
				SimpleTypeKind.UnsignedLong => GetIntReturn((uint)owner.ReadInt32(offset)), 
				SimpleTypeKind.SignedLong => GetIntReturn(owner.ReadInt32(offset)), 
				SimpleTypeKind.Single => GetSingleReturn(owner.ReadInt32(offset)), 
				SimpleTypeKind.Double => GetDoubleReturn(owner.ReadInt64(offset)), 
				SimpleTypeKind.UnsignedLongLong => GetIntReturn((ulong)owner.ReadInt64(offset)), 
				SimpleTypeKind.SignedLongLong => GetIntReturn(owner.ReadInt64(offset)), 
				SimpleTypeKind.Object => GetObjectReturn(owner.ReadIntPtr(offset)), 
				SimpleTypeKind.Pointer => owner.ReadIntPtr(offset).ToPython(), 
				SimpleTypeKind.CharPointer => owner.ReadMemoryHolder(offset).ReadAnsiString(0), 
				SimpleTypeKind.WCharPointer => owner.ReadMemoryHolder(offset).ReadUnicodeString(0), 
				SimpleTypeKind.BStr => Marshal.PtrToStringBSTR(owner.ReadIntPtr(offset)), 
				_ => throw new InvalidOperationException(), 
			};
			if (!raw && IsSubClass)
			{
				obj = PythonCalls.Call(this, obj);
			}
			return obj;
		}

		internal char ReadChar(MemoryHolder owner, int offset)
		{
			return _type switch
			{
				SimpleTypeKind.Char => (char)owner.ReadByte(offset), 
				SimpleTypeKind.WChar => (char)owner.ReadInt16(offset), 
				_ => throw new InvalidOperationException(), 
			};
		}

		object INativeType.SetValue(MemoryHolder owner, int offset, object value)
		{
			if (value is SimpleCData simpleCData && simpleCData.NativeType == this)
			{
				simpleCData._memHolder.CopyTo(owner, offset, ((INativeType)this).Size);
				return null;
			}
			switch (_type)
			{
			case SimpleTypeKind.Boolean:
				owner.WriteByte(offset, ModuleOps.GetBoolean(value, this));
				break;
			case SimpleTypeKind.Char:
				owner.WriteByte(offset, ModuleOps.GetChar(value, this));
				break;
			case SimpleTypeKind.SignedByte:
				owner.WriteByte(offset, ModuleOps.GetSignedByte(value, this));
				break;
			case SimpleTypeKind.UnsignedByte:
				owner.WriteByte(offset, ModuleOps.GetUnsignedByte(value, this));
				break;
			case SimpleTypeKind.WChar:
				owner.WriteInt16(offset, (short)ModuleOps.GetWChar(value, this));
				break;
			case SimpleTypeKind.SignedShort:
				owner.WriteInt16(offset, ModuleOps.GetSignedShort(value, this));
				break;
			case SimpleTypeKind.UnsignedShort:
				owner.WriteInt16(offset, ModuleOps.GetUnsignedShort(value, this));
				break;
			case SimpleTypeKind.VariantBool:
				owner.WriteInt16(offset, (short)ModuleOps.GetVariantBool(value, this));
				break;
			case SimpleTypeKind.SignedInt:
				owner.WriteInt32(offset, ModuleOps.GetSignedInt(value, this));
				break;
			case SimpleTypeKind.UnsignedInt:
				owner.WriteInt32(offset, ModuleOps.GetUnsignedInt(value, this));
				break;
			case SimpleTypeKind.UnsignedLong:
				owner.WriteInt32(offset, ModuleOps.GetUnsignedLong(value, this));
				break;
			case SimpleTypeKind.SignedLong:
				owner.WriteInt32(offset, ModuleOps.GetSignedLong(value, this));
				break;
			case SimpleTypeKind.Single:
				owner.WriteInt32(offset, ModuleOps.GetSingleBits(value));
				break;
			case SimpleTypeKind.Double:
				owner.WriteInt64(offset, ModuleOps.GetDoubleBits(value));
				break;
			case SimpleTypeKind.UnsignedLongLong:
				owner.WriteInt64(offset, ModuleOps.GetUnsignedLongLong(value, this));
				break;
			case SimpleTypeKind.SignedLongLong:
				owner.WriteInt64(offset, ModuleOps.GetSignedLongLong(value, this));
				break;
			case SimpleTypeKind.Object:
				owner.WriteIntPtr(offset, ModuleOps.GetObject(value));
				break;
			case SimpleTypeKind.Pointer:
				owner.WriteIntPtr(offset, ModuleOps.GetPointer(value));
				break;
			case SimpleTypeKind.CharPointer:
				owner.WriteIntPtr(offset, ModuleOps.GetCharPointer(value));
				return value;
			case SimpleTypeKind.WCharPointer:
				owner.WriteIntPtr(offset, ModuleOps.GetWCharPointer(value));
				return value;
			case SimpleTypeKind.BStr:
				owner.WriteIntPtr(offset, ModuleOps.GetBSTR(value));
				return value;
			default:
				throw new InvalidOperationException();
			}
			return null;
		}

		Type INativeType.GetNativeType()
		{
			switch (_type)
			{
			case SimpleTypeKind.Boolean:
				return typeof(bool);
			case SimpleTypeKind.Char:
				return typeof(byte);
			case SimpleTypeKind.SignedByte:
				return typeof(sbyte);
			case SimpleTypeKind.UnsignedByte:
				return typeof(byte);
			case SimpleTypeKind.SignedShort:
			case SimpleTypeKind.VariantBool:
				return typeof(short);
			case SimpleTypeKind.UnsignedShort:
				return typeof(ushort);
			case SimpleTypeKind.WChar:
				return typeof(char);
			case SimpleTypeKind.SignedInt:
			case SimpleTypeKind.SignedLong:
				return typeof(int);
			case SimpleTypeKind.UnsignedInt:
			case SimpleTypeKind.UnsignedLong:
				return typeof(uint);
			case SimpleTypeKind.Single:
				return typeof(float);
			case SimpleTypeKind.Double:
				return typeof(double);
			case SimpleTypeKind.UnsignedLongLong:
				return typeof(ulong);
			case SimpleTypeKind.SignedLongLong:
				return typeof(long);
			case SimpleTypeKind.Object:
				return typeof(IntPtr);
			case SimpleTypeKind.Pointer:
			case SimpleTypeKind.CharPointer:
			case SimpleTypeKind.WCharPointer:
			case SimpleTypeKind.BStr:
				return typeof(IntPtr);
			default:
				throw new InvalidOperationException();
			}
		}

		MarshalCleanup INativeType.EmitMarshalling(ILGenerator method, LocalOrArg argIndex, List<object> constantPool, int constantPoolArgument)
		{
			MarshalCleanup result = null;
			Label label = method.DefineLabel();
			Type type = argIndex.Type;
			if (!type.IsValueType && _type != SimpleTypeKind.Object && _type != SimpleTypeKind.Pointer)
			{
				Label label2 = method.DefineLabel();
				argIndex.Emit(method);
				constantPool.Add(this);
				method.Emit(OpCodes.Ldarg, constantPoolArgument);
				method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
				method.Emit(OpCodes.Ldelem_Ref);
				method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CheckSimpleCDataType"));
				method.Emit(OpCodes.Brfalse, label2);
				argIndex.Emit(method);
				method.Emit(OpCodes.Castclass, typeof(CData));
				method.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
				method.Emit(OpCodes.Ldobj, ((INativeType)this).GetNativeType());
				method.Emit(OpCodes.Br, label);
				method.MarkLabel(label2);
			}
			argIndex.Emit(method);
			if (type.IsValueType)
			{
				method.Emit(OpCodes.Box, type);
			}
			switch (_type)
			{
			case SimpleTypeKind.Char:
			case SimpleTypeKind.SignedByte:
			case SimpleTypeKind.UnsignedByte:
			case SimpleTypeKind.SignedShort:
			case SimpleTypeKind.UnsignedShort:
			case SimpleTypeKind.SignedInt:
			case SimpleTypeKind.UnsignedInt:
			case SimpleTypeKind.SignedLong:
			case SimpleTypeKind.UnsignedLong:
			case SimpleTypeKind.Single:
			case SimpleTypeKind.Double:
			case SimpleTypeKind.SignedLongLong:
			case SimpleTypeKind.UnsignedLongLong:
			case SimpleTypeKind.WChar:
			case SimpleTypeKind.Boolean:
			case SimpleTypeKind.VariantBool:
				constantPool.Add(this);
				method.Emit(OpCodes.Ldarg, constantPoolArgument);
				method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
				method.Emit(OpCodes.Ldelem_Ref);
				method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("Get" + _type));
				break;
			case SimpleTypeKind.Pointer:
			{
				Label label3 = method.DefineLabel();
				TryBytesConversion(method, label3);
				Label label4 = method.DefineLabel();
				argIndex.Emit(method);
				if (type.IsValueType)
				{
					method.Emit(OpCodes.Box, type);
				}
				method.Emit(OpCodes.Isinst, typeof(string));
				method.Emit(OpCodes.Dup);
				method.Emit(OpCodes.Brfalse, label4);
				LocalBuilder local = method.DeclareLocal(typeof(string), pinned: true);
				method.Emit(OpCodes.Stloc, local);
				method.Emit(OpCodes.Ldloc, local);
				method.Emit(OpCodes.Conv_I);
				method.Emit(OpCodes.Ldc_I4, RuntimeHelpers.OffsetToStringData);
				method.Emit(OpCodes.Add);
				method.Emit(OpCodes.Br, label3);
				method.MarkLabel(label4);
				method.Emit(OpCodes.Pop);
				argIndex.Emit(method);
				if (type.IsValueType)
				{
					method.Emit(OpCodes.Box, type);
				}
				method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("GetPointer"));
				method.MarkLabel(label3);
				break;
			}
			case SimpleTypeKind.Object:
				method.Emit(OpCodes.Call, typeof(CTypes).GetMethod("PyObj_ToPtr"));
				break;
			case SimpleTypeKind.CharPointer:
			{
				Label label3 = method.DefineLabel();
				TryToCharPtrConversion(method, argIndex, type, label3);
				result = MarshalCharPointer(method, argIndex);
				method.MarkLabel(label3);
				break;
			}
			case SimpleTypeKind.WCharPointer:
			{
				Label label3 = method.DefineLabel();
				TryArrayToWCharPtrConversion(method, argIndex, type, label3);
				MarshalWCharPointer(method, argIndex);
				method.MarkLabel(label3);
				break;
			}
			case SimpleTypeKind.BStr:
				throw new NotImplementedException("BSTR marshalling");
			}
			method.MarkLabel(label);
			return result;
		}

		private static void TryBytesConversion(ILGenerator method, Label done)
		{
			Label label = method.DefineLabel();
			LocalBuilder local = method.DeclareLocal(typeof(byte).MakeByRefType(), pinned: true);
			method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("TryCheckBytes"));
			method.Emit(OpCodes.Dup);
			method.Emit(OpCodes.Brfalse, label);
			method.Emit(OpCodes.Ldc_I4_0);
			method.Emit(OpCodes.Ldelema, typeof(byte));
			method.Emit(OpCodes.Stloc, local);
			method.Emit(OpCodes.Ldloc, local);
			method.Emit(OpCodes.Br, done);
			method.MarkLabel(label);
			method.Emit(OpCodes.Pop);
		}

		internal static void TryArrayToWCharPtrConversion(ILGenerator method, LocalOrArg argIndex, Type argumentType, Label done)
		{
			Label label = method.DefineLabel();
			method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("TryCheckWCharArray"));
			method.Emit(OpCodes.Dup);
			method.Emit(OpCodes.Brfalse, label);
			method.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
			method.Emit(OpCodes.Br, done);
			method.MarkLabel(label);
			method.Emit(OpCodes.Pop);
			argIndex.Emit(method);
			if (argumentType.IsValueType)
			{
				method.Emit(OpCodes.Box, argumentType);
			}
		}

		internal static void TryToCharPtrConversion(ILGenerator method, LocalOrArg argIndex, Type argumentType, Label done)
		{
			TryBytesConversion(method, done);
			Label label = method.DefineLabel();
			argIndex.Emit(method);
			method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("TryCheckCharArray"));
			method.Emit(OpCodes.Dup);
			method.Emit(OpCodes.Brfalse, label);
			method.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
			method.Emit(OpCodes.Br, done);
			method.MarkLabel(label);
			method.Emit(OpCodes.Pop);
			argIndex.Emit(method);
			if (argumentType.IsValueType)
			{
				method.Emit(OpCodes.Box, argumentType);
			}
		}

		internal static void MarshalWCharPointer(ILGenerator method, LocalOrArg argIndex)
		{
			Type type = argIndex.Type;
			Label label = method.DefineLabel();
			Label label2 = method.DefineLabel();
			method.Emit(OpCodes.Brfalse, label);
			argIndex.Emit(method);
			if (type.IsValueType)
			{
				method.Emit(OpCodes.Box, type);
			}
			LocalBuilder local = method.DeclareLocal(typeof(string), pinned: true);
			method.Emit(OpCodes.Stloc, local);
			method.Emit(OpCodes.Ldloc, local);
			method.Emit(OpCodes.Conv_I);
			method.Emit(OpCodes.Ldc_I4, RuntimeHelpers.OffsetToStringData);
			method.Emit(OpCodes.Add);
			method.Emit(OpCodes.Br, label2);
			method.MarkLabel(label);
			method.Emit(OpCodes.Ldc_I4_0);
			method.Emit(OpCodes.Conv_I);
			method.MarkLabel(label2);
		}

		internal static MarshalCleanup MarshalCharPointer(ILGenerator method, LocalOrArg argIndex)
		{
			Type type = argIndex.Type;
			Label label = method.DefineLabel();
			Label label2 = method.DefineLabel();
			method.Emit(OpCodes.Brfalse, label);
			argIndex.Emit(method);
			if (type.IsValueType)
			{
				method.Emit(OpCodes.Box, type);
			}
			LocalBuilder local = method.DeclareLocal(typeof(IntPtr));
			method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("StringToHGlobalAnsi"));
			method.Emit(OpCodes.Stloc, local);
			method.Emit(OpCodes.Ldloc, local);
			method.Emit(OpCodes.Br, label2);
			method.MarkLabel(label);
			method.Emit(OpCodes.Ldc_I4_0);
			method.Emit(OpCodes.Conv_I);
			method.MarkLabel(label2);
			return new StringCleanup(local);
		}

		Type INativeType.GetPythonType()
		{
			if (IsSubClass)
			{
				return typeof(object);
			}
			return GetPythonTypeWorker();
		}

		private Type GetPythonTypeWorker()
		{
			switch (_type)
			{
			case SimpleTypeKind.Boolean:
				return typeof(bool);
			case SimpleTypeKind.Char:
			case SimpleTypeKind.CharPointer:
			case SimpleTypeKind.WCharPointer:
			case SimpleTypeKind.WChar:
			case SimpleTypeKind.BStr:
				return typeof(string);
			case SimpleTypeKind.SignedByte:
			case SimpleTypeKind.UnsignedByte:
			case SimpleTypeKind.SignedShort:
			case SimpleTypeKind.UnsignedShort:
			case SimpleTypeKind.SignedInt:
			case SimpleTypeKind.SignedLong:
			case SimpleTypeKind.VariantBool:
				return typeof(int);
			case SimpleTypeKind.UnsignedInt:
			case SimpleTypeKind.UnsignedLong:
			case SimpleTypeKind.SignedLongLong:
			case SimpleTypeKind.UnsignedLongLong:
			case SimpleTypeKind.Object:
			case SimpleTypeKind.Pointer:
				return typeof(object);
			case SimpleTypeKind.Single:
			case SimpleTypeKind.Double:
				return typeof(double);
			default:
				throw new InvalidOperationException();
			}
		}

		void INativeType.EmitReverseMarshalling(ILGenerator method, LocalOrArg value, List<object> constantPool, int constantPoolArgument)
		{
			value.Emit(method);
			switch (_type)
			{
			case SimpleTypeKind.SignedByte:
			case SimpleTypeKind.UnsignedByte:
			case SimpleTypeKind.SignedShort:
			case SimpleTypeKind.UnsignedShort:
			case SimpleTypeKind.VariantBool:
				method.Emit(OpCodes.Conv_I4);
				break;
			case SimpleTypeKind.Single:
				method.Emit(OpCodes.Conv_R8);
				break;
			case SimpleTypeKind.UnsignedInt:
			case SimpleTypeKind.UnsignedLong:
				EmitInt32ToObject(method, value);
				break;
			case SimpleTypeKind.SignedLongLong:
			case SimpleTypeKind.UnsignedLongLong:
				EmitInt64ToObject(method, value);
				break;
			case SimpleTypeKind.Object:
				method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("IntPtrToObject"));
				break;
			case SimpleTypeKind.WCharPointer:
				method.Emit(OpCodes.Call, typeof(Marshal).GetMethod("PtrToStringUni", new Type[1] { typeof(IntPtr) }));
				break;
			case SimpleTypeKind.CharPointer:
				method.Emit(OpCodes.Call, typeof(Marshal).GetMethod("PtrToStringAnsi", new Type[1] { typeof(IntPtr) }));
				break;
			case SimpleTypeKind.BStr:
				method.Emit(OpCodes.Call, typeof(Marshal).GetMethod("PtrToStringBSTR", new Type[1] { typeof(IntPtr) }));
				break;
			case SimpleTypeKind.Char:
				method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CharToString"));
				break;
			case SimpleTypeKind.WChar:
				method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("WCharToString"));
				break;
			case SimpleTypeKind.Pointer:
			{
				Label label = method.DefineLabel();
				Label label2 = method.DefineLabel();
				if (IntPtr.Size == 4)
				{
					LocalBuilder local = method.DeclareLocal(typeof(uint));
					method.Emit(OpCodes.Conv_U4);
					method.Emit(OpCodes.Stloc, local);
					method.Emit(OpCodes.Ldloc, local);
					method.Emit(OpCodes.Ldc_I4_0);
					method.Emit(OpCodes.Conv_U4);
					method.Emit(OpCodes.Bne_Un, label2);
					method.Emit(OpCodes.Ldnull);
					method.Emit(OpCodes.Br, label);
					method.MarkLabel(label2);
					method.Emit(OpCodes.Ldloc, local);
					EmitInt32ToObject(method, new Local(local));
				}
				else
				{
					LocalBuilder local2 = method.DeclareLocal(typeof(long));
					method.Emit(OpCodes.Conv_I8);
					method.Emit(OpCodes.Stloc, local2);
					method.Emit(OpCodes.Ldloc, local2);
					method.Emit(OpCodes.Ldc_I4_0);
					method.Emit(OpCodes.Conv_U8);
					method.Emit(OpCodes.Bne_Un, label2);
					method.Emit(OpCodes.Ldnull);
					method.Emit(OpCodes.Br, label);
					method.MarkLabel(label2);
					method.Emit(OpCodes.Ldloc, local2);
					EmitInt64ToObject(method, new Local(local2));
				}
				method.MarkLabel(label);
				break;
			}
			}
			if (IsSubClass)
			{
				LocalBuilder local3 = method.DeclareLocal(typeof(object));
				if (GetPythonTypeWorker().IsValueType)
				{
					method.Emit(OpCodes.Box, GetPythonTypeWorker());
				}
				method.Emit(OpCodes.Stloc, local3);
				constantPool.Add(this);
				method.Emit(OpCodes.Ldarg, constantPoolArgument);
				method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
				method.Emit(OpCodes.Ldelem_Ref);
				method.Emit(OpCodes.Ldloc, local3);
				method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CreateSubclassInstance"));
			}
		}

		private static void EmitInt64ToObject(ILGenerator method, LocalOrArg value)
		{
			Label label = method.DefineLabel();
			Label label2 = method.DefineLabel();
			method.Emit(OpCodes.Ldc_I4, int.MaxValue);
			method.Emit(OpCodes.Conv_I8);
			method.Emit(OpCodes.Bgt, label);
			value.Emit(method);
			method.Emit(OpCodes.Ldc_I4, int.MinValue);
			method.Emit(OpCodes.Conv_I8);
			method.Emit(OpCodes.Blt, label);
			value.Emit(method);
			method.Emit(OpCodes.Conv_I4);
			method.Emit(OpCodes.Box, typeof(int));
			method.Emit(OpCodes.Br, label2);
			method.MarkLabel(label);
			value.Emit(method);
			method.Emit(OpCodes.Call, typeof(BigInteger).GetMethod("op_Implicit", new Type[1] { value.Type }));
			method.Emit(OpCodes.Box, typeof(BigInteger));
			method.MarkLabel(label2);
		}

		private static void EmitInt32ToObject(ILGenerator method, LocalOrArg value)
		{
			Label label = method.DefineLabel();
			Label label2 = method.DefineLabel();
			method.Emit(OpCodes.Ldc_I4, int.MaxValue);
			method.Emit((value.Type == typeof(uint)) ? OpCodes.Conv_U4 : OpCodes.Conv_U8);
			method.Emit(OpCodes.Ble, label);
			value.Emit(method);
			method.Emit(OpCodes.Call, typeof(BigInteger).GetMethod("op_Implicit", new Type[1] { value.Type }));
			method.Emit(OpCodes.Box, typeof(BigInteger));
			method.Emit(OpCodes.Br, label2);
			method.MarkLabel(label);
			value.Emit(method);
			method.Emit(OpCodes.Conv_I4);
			method.Emit(OpCodes.Box, typeof(int));
			method.MarkLabel(label2);
		}

		private object GetObjectReturn(IntPtr intPtr)
		{
			return GCHandle.FromIntPtr(intPtr).Target;
		}

		private object GetDoubleReturn(long p)
		{
			return BitConverter.ToDouble(BitConverter.GetBytes(p), 0);
		}

		private object GetSingleReturn(int p)
		{
			return BitConverter.ToSingle(BitConverter.GetBytes(p), 0);
		}

		private static object GetIntReturn(int value)
		{
			return ScriptingRuntimeHelpers.Int32ToObject(value);
		}

		private static object GetIntReturn(uint value)
		{
			if (value > int.MaxValue)
			{
				return (BigInteger)value;
			}
			return ScriptingRuntimeHelpers.Int32ToObject((int)value);
		}

		private static object GetIntReturn(long value)
		{
			if (value <= int.MaxValue && value >= int.MinValue)
			{
				return (int)value;
			}
			return (BigInteger)value;
		}

		private static object GetIntReturn(ulong value)
		{
			if (value <= int.MaxValue)
			{
				return (int)value;
			}
			return (BigInteger)value;
		}
	}

	[PythonType]
	[PythonHidden]
	public class StructType : PythonType, INativeType
	{
		internal Field[] _fields;

		private int? _size;

		private int? _alignment;

		private int? _pack;

		private static readonly Field[] _emptyFields = new Field[0];

		int INativeType.Size
		{
			get
			{
				EnsureSizeAndAlignment();
				return _size.Value;
			}
		}

		int INativeType.Alignment
		{
			get
			{
				EnsureSizeAndAlignment();
				return _alignment.Value;
			}
		}

		string INativeType.TypeFormat
		{
			get
			{
				if (_pack.HasValue || _fields == _emptyFields || _fields == null)
				{
					return "B";
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("T{");
				Field[] fields = _fields;
				foreach (Field field in fields)
				{
					stringBuilder.Append(field.NativeType.TypeFormat);
					stringBuilder.Append(':');
					stringBuilder.Append(field.FieldName);
					stringBuilder.Append(':');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}
		}

		public StructType(CodeContext context, string name, PythonTuple bases, PythonDictionary members)
			: base(context, name, bases, members)
		{
			foreach (PythonType item in base.ResolutionOrder)
			{
				StructType structType = item as StructType;
				if (structType != this)
				{
					structType?.EnsureFinal();
				}
				if (item is UnionType unionType)
				{
					unionType.EnsureFinal();
				}
			}
			if (members.TryGetValue("_pack_", out var value))
			{
				if (!(value is int) || (int)value < 0)
				{
					throw PythonOps.ValueError("pack must be a non-negative integer");
				}
				_pack = (int)value;
			}
			if (members.TryGetValue("_fields_", out var value2))
			{
				SetFields(value2);
			}
		}

		private StructType(Type underlyingSystemType)
			: base(underlyingSystemType)
		{
		}

		public static ArrayType operator *(StructType type, int count)
		{
			return MakeArrayType(type, count);
		}

		public static ArrayType operator *(int count, StructType type)
		{
			return MakeArrayType(type, count);
		}

		public _Structure from_address(CodeContext context, int address)
		{
			return from_address(context, new IntPtr(address));
		}

		public _Structure from_address(CodeContext context, BigInteger address)
		{
			return from_address(context, new IntPtr((long)address));
		}

		public _Structure from_address(CodeContext context, IntPtr ptr)
		{
			_Structure structure = (_Structure)CreateInstance(context);
			structure.SetAddress(ptr);
			return structure;
		}

		public _Structure from_buffer(ArrayModule.array array, [DefaultParameterValue(0)] int offset)
		{
			ValidateArraySizes(array, offset, ((INativeType)this).Size);
			_Structure structure = (_Structure)CreateInstance(base.Context.SharedContext);
			IntPtr arrayAddress = array.GetArrayAddress();
			structure._memHolder = new MemoryHolder(arrayAddress.Add(offset), ((INativeType)this).Size);
			structure._memHolder.AddObject("ffffffff", array);
			return structure;
		}

		public _Structure from_buffer_copy(ArrayModule.array array, [DefaultParameterValue(0)] int offset)
		{
			ValidateArraySizes(array, offset, ((INativeType)this).Size);
			_Structure structure = (_Structure)CreateInstance(base.Context.SharedContext);
			structure._memHolder = new MemoryHolder(((INativeType)this).Size);
			structure._memHolder.CopyFrom(array.GetArrayAddress().Add(offset), new IntPtr(((INativeType)this).Size));
			GC.KeepAlive(array);
			return structure;
		}

		public object from_param(object obj)
		{
			if (!Builtin.isinstance(obj, this))
			{
				throw PythonOps.TypeError("expected {0} instance got {1}", base.Name, PythonTypeOps.GetName(obj));
			}
			return obj;
		}

		public object in_dll(object library, string name)
		{
			throw new NotImplementedException("in dll");
		}

		public new virtual void __setattr__(CodeContext context, string name, object value)
		{
			if (name == "_fields_")
			{
				lock (this)
				{
					if (_fields != null)
					{
						throw PythonOps.AttributeError("_fields_ is final");
					}
					SetFields(value);
				}
			}
			base.__setattr__(context, name, value);
		}

		object INativeType.GetValue(MemoryHolder owner, object readingFrom, int offset, bool raw)
		{
			_Structure structure = (_Structure)CreateInstance(base.Context.SharedContext);
			structure._memHolder = owner.GetSubBlock(offset);
			return structure;
		}

		object INativeType.SetValue(MemoryHolder address, int offset, object value)
		{
			try
			{
				return SetValueInternal(address, offset, value);
			}
			catch (ArgumentTypeException ex)
			{
				throw PythonOps.RuntimeError("({0}) <type 'exceptions.TypeError'>: {1}", base.Name, ex.Message);
			}
			catch (ArgumentException ex2)
			{
				throw PythonOps.RuntimeError("({0}) <type 'exceptions.ValueError'>: {1}", base.Name, ex2.Message);
			}
		}

		internal object SetValueInternal(MemoryHolder address, int offset, object value)
		{
			if (value is IList<object> list)
			{
				if (list.Count > _fields.Length)
				{
					throw PythonOps.TypeError("too many initializers");
				}
				for (int i = 0; i < list.Count; i++)
				{
					_fields[i].SetValue(address, offset, list[i]);
				}
				return null;
			}
			if (value is CData cData)
			{
				cData._memHolder.CopyTo(address, offset, cData.Size);
				return cData._memHolder.EnsureObjects();
			}
			throw new NotImplementedException("set value");
		}

		Type INativeType.GetNativeType()
		{
			EnsureFinal();
			return GetMarshalTypeFromSize(_size.Value);
		}

		MarshalCleanup INativeType.EmitMarshalling(ILGenerator method, LocalOrArg argIndex, List<object> constantPool, int constantPoolArgument)
		{
			Type type = argIndex.Type;
			argIndex.Emit(method);
			if (type.IsValueType)
			{
				method.Emit(OpCodes.Box, type);
			}
			constantPool.Add(this);
			method.Emit(OpCodes.Ldarg, constantPoolArgument);
			method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
			method.Emit(OpCodes.Ldelem_Ref);
			method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CheckCDataType"));
			method.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
			method.Emit(OpCodes.Ldobj, ((INativeType)this).GetNativeType());
			return null;
		}

		Type INativeType.GetPythonType()
		{
			return typeof(object);
		}

		void INativeType.EmitReverseMarshalling(ILGenerator method, LocalOrArg value, List<object> constantPool, int constantPoolArgument)
		{
			value.Emit(method);
			EmitCDataCreation(this, method, constantPool, constantPoolArgument);
		}

		internal static PythonType MakeSystemType(Type underlyingSystemType)
		{
			return PythonType.SetPythonType(underlyingSystemType, new StructType(underlyingSystemType));
		}

		private void SetFields(object fields)
		{
			lock (this)
			{
				IList<object> fieldsList = GetFieldsList(fields);
				int? bitCount = null;
				int? totalBitCount = null;
				INativeType nativeType = null;
				int size;
				int alignment;
				List<Field> baseSizeAlignmentAndFields = GetBaseSizeAlignmentAndFields(out size, out alignment);
				IList<object> anonymousFields = GetAnonymousFields(this);
				for (int i = 0; i < fieldsList.Count; i++)
				{
					object o = fieldsList[i];
					GetFieldInfo(this, o, out var fieldName, out var cdata, out bitCount);
					int offset = UpdateSizeAndAlignment(cdata, bitCount, nativeType, ref size, ref alignment, ref totalBitCount);
					Field field = new Field(fieldName, cdata, offset, baseSizeAlignmentAndFields.Count, bitCount, totalBitCount - bitCount);
					baseSizeAlignmentAndFields.Add(field);
					AddSlot(fieldName, field);
					if (anonymousFields != null && anonymousFields.Contains(fieldName))
					{
						AddAnonymousFields(this, baseSizeAlignmentAndFields, cdata, field);
					}
					nativeType = cdata;
				}
				CheckAnonymousFields(baseSizeAlignmentAndFields, anonymousFields);
				if (bitCount.HasValue)
				{
					size += nativeType.Size;
				}
				_fields = baseSizeAlignmentAndFields.ToArray();
				_size = PythonStruct.Align(size, alignment);
				_alignment = alignment;
			}
		}

		internal static void CheckAnonymousFields(List<Field> allFields, IList<object> anonFields)
		{
			if (anonFields == null)
			{
				return;
			}
			foreach (string anonField in anonFields)
			{
				bool flag = false;
				foreach (Field allField in allFields)
				{
					if (allField.FieldName == anonField)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					throw PythonOps.AttributeError("anonymous field {0} is not defined in this structure", anonField);
				}
			}
		}

		internal static IList<object> GetAnonymousFields(PythonType type)
		{
			IList<object> list = null;
			if (type.TryGetBoundAttr(type.Context.SharedContext, type, "_anonymous_", out var ret))
			{
				list = ret as IList<object>;
				if (list == null)
				{
					throw PythonOps.TypeError("_anonymous_ must be a sequence");
				}
			}
			return list;
		}

		internal static void AddAnonymousFields(PythonType type, List<Field> allFields, INativeType cdata, Field newField)
		{
			Field[] fields;
			if (cdata is StructType)
			{
				fields = ((StructType)cdata)._fields;
			}
			else
			{
				if (!(cdata is UnionType))
				{
					throw PythonOps.TypeError("anonymous field must be struct or union");
				}
				fields = ((UnionType)cdata)._fields;
			}
			Field[] array = fields;
			foreach (Field field in array)
			{
				Field field2 = new Field(field.FieldName, field.NativeType, checked(field.offset + newField.offset), allFields.Count);
				type.AddSlot(field.FieldName, field2);
				allFields.Add(field2);
			}
		}

		private List<Field> GetBaseSizeAlignmentAndFields(out int size, out int alignment)
		{
			size = 0;
			alignment = 1;
			List<Field> list = new List<Field>();
			INativeType lastType = null;
			int? totalBitCount = null;
			foreach (PythonType baseType in base.BaseTypes)
			{
				if (!(baseType is StructType { _fields: var fields }))
				{
					continue;
				}
				foreach (Field field in fields)
				{
					list.Add(field);
					UpdateSizeAndAlignment(field.NativeType, field.BitCount, lastType, ref size, ref alignment, ref totalBitCount);
					if (field.NativeType == this)
					{
						throw StructureCannotContainSelf();
					}
					lastType = field.NativeType;
				}
			}
			return list;
		}

		private int UpdateSizeAndAlignment(INativeType cdata, int? bitCount, INativeType lastType, ref int size, ref int alignment, ref int? totalBitCount)
		{
			int result = size;
			if (bitCount.HasValue)
			{
				if (lastType != null && lastType.Size != cdata.Size)
				{
					totalBitCount = null;
					result = (size += lastType.Size);
				}
				size = PythonStruct.Align(size, cdata.Alignment);
				if (totalBitCount.HasValue)
				{
					if ((bitCount + totalBitCount + 7) / 8 <= cdata.Size)
					{
						totalBitCount = bitCount + totalBitCount;
					}
					else
					{
						size += lastType.Size;
						result = size;
						totalBitCount = bitCount;
					}
				}
				else
				{
					totalBitCount = bitCount;
				}
			}
			else
			{
				if (totalBitCount.HasValue)
				{
					size += lastType.Size;
					result = size;
					totalBitCount = null;
				}
				if (_pack.HasValue)
				{
					alignment = _pack.Value;
					result = (size = PythonStruct.Align(size, _pack.Value));
					size += cdata.Size;
				}
				else
				{
					alignment = Math.Max(alignment, cdata.Alignment);
					result = (size = PythonStruct.Align(size, cdata.Alignment));
					size += cdata.Size;
				}
			}
			return result;
		}

		internal void EnsureFinal()
		{
			if (_fields == null)
			{
				SetFields(PythonTuple.EMPTY);
				if (_fields.Length == 0)
				{
					_fields = _emptyFields;
				}
			}
		}

		private void EnsureSizeAndAlignment()
		{
			if (_size.HasValue)
			{
				return;
			}
			lock (this)
			{
				if (!_size.HasValue)
				{
					GetBaseSizeAlignmentAndFields(out var size, out var alignment);
					_size = size;
					_alignment = alignment;
				}
			}
		}
	}

	[PythonType("Structure")]
	public abstract class _Structure : CData
	{
		protected _Structure()
		{
			((StructType)base.NativeType).EnsureFinal();
			_memHolder = new MemoryHolder(base.NativeType.Size);
		}

		public void __init__(params object[] args)
		{
			CheckAbstract();
			INativeType nativeType = base.NativeType;
			StructType structType = (StructType)nativeType;
			structType.SetValueInternal(_memHolder, 0, args);
		}

		public void __init__(CodeContext context, [ParamDictionary] IDictionary<string, object> kwargs)
		{
			CheckAbstract();
			foreach (KeyValuePair<string, object> kwarg in kwargs)
			{
				PythonOps.SetAttr(context, this, kwarg.Key, kwarg.Value);
			}
		}

		private void CheckAbstract()
		{
			if (((PythonType)base.NativeType).TryGetBoundAttr(((PythonType)base.NativeType).Context.SharedContext, this, "_abstract_", out var _))
			{
				throw PythonOps.TypeError("abstract class");
			}
		}
	}

	[PythonType("Union")]
	public abstract class _Union : CData
	{
	}

	[PythonHidden]
	[PythonType]
	public class UnionType : PythonType, INativeType
	{
		internal Field[] _fields;

		private int _size;

		private int _alignment;

		int INativeType.Size => _size;

		int INativeType.Alignment => _alignment;

		string INativeType.TypeFormat => "B";

		public UnionType(CodeContext context, string name, PythonTuple bases, PythonDictionary members)
			: base(context, name, bases, members)
		{
			if (members.TryGetValue("_fields_", out var value))
			{
				SetFields(value);
			}
		}

		public new void __setattr__(CodeContext context, string name, object value)
		{
			if (name == "_fields_")
			{
				lock (this)
				{
					if (_fields != null)
					{
						throw PythonOps.AttributeError("_fields_ is final");
					}
					SetFields(value);
				}
			}
			base.__setattr__(context, name, value);
		}

		private UnionType(Type underlyingSystemType)
			: base(underlyingSystemType)
		{
		}

		public object from_param(object obj)
		{
			return null;
		}

		internal static PythonType MakeSystemType(Type underlyingSystemType)
		{
			return PythonType.SetPythonType(underlyingSystemType, new UnionType(underlyingSystemType));
		}

		public static ArrayType operator *(UnionType type, int count)
		{
			return MakeArrayType(type, count);
		}

		public static ArrayType operator *(int count, UnionType type)
		{
			return MakeArrayType(type, count);
		}

		object INativeType.GetValue(MemoryHolder owner, object readingFrom, int offset, bool raw)
		{
			_Union union = (_Union)CreateInstance(base.Context.SharedContext);
			union._memHolder = owner.GetSubBlock(offset);
			return union;
		}

		object INativeType.SetValue(MemoryHolder address, int offset, object value)
		{
			if (value is IList<object> list)
			{
				if (list.Count > _fields.Length)
				{
					throw PythonOps.TypeError("too many initializers");
				}
				for (int i = 0; i < list.Count; i++)
				{
					_fields[i].SetValue(address, offset, list[i]);
				}
				return null;
			}
			if (value is CData cData)
			{
				cData._memHolder.CopyTo(address, offset, cData.Size);
				return cData._memHolder.EnsureObjects();
			}
			throw new NotImplementedException("Union set value");
		}

		Type INativeType.GetNativeType()
		{
			return GetMarshalTypeFromSize(_size);
		}

		MarshalCleanup INativeType.EmitMarshalling(ILGenerator method, LocalOrArg argIndex, List<object> constantPool, int constantPoolArgument)
		{
			Type type = argIndex.Type;
			argIndex.Emit(method);
			if (type.IsValueType)
			{
				method.Emit(OpCodes.Box, type);
			}
			constantPool.Add(this);
			method.Emit(OpCodes.Ldarg, constantPoolArgument);
			method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
			method.Emit(OpCodes.Ldelem_Ref);
			method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CheckCDataType"));
			method.Emit(OpCodes.Call, typeof(CData).GetMethod("get_UnsafeAddress"));
			method.Emit(OpCodes.Ldobj, ((INativeType)this).GetNativeType());
			return null;
		}

		Type INativeType.GetPythonType()
		{
			return typeof(object);
		}

		void INativeType.EmitReverseMarshalling(ILGenerator method, LocalOrArg value, List<object> constantPool, int constantPoolArgument)
		{
			value.Emit(method);
			EmitCDataCreation(this, method, constantPool, constantPoolArgument);
		}

		private void SetFields(object fields)
		{
			lock (this)
			{
				IList<object> fieldsList = GetFieldsList(fields);
				IList<object> anonymousFields = StructType.GetAnonymousFields(this);
				int num = 0;
				int num2 = 1;
				List<Field> list = new List<Field>();
				for (int i = 0; i < fieldsList.Count; i++)
				{
					object o = fieldsList[i];
					GetFieldInfo(this, o, out var fieldName, out var cdata, out var _);
					num2 = Math.Max(num2, cdata.Alignment);
					num = Math.Max(num, cdata.Size);
					Field field = new Field(fieldName, cdata, 0, list.Count);
					list.Add(field);
					AddSlot(fieldName, field);
					if (anonymousFields != null && anonymousFields.Contains(fieldName))
					{
						StructType.AddAnonymousFields(this, list, cdata, field);
					}
				}
				StructType.CheckAnonymousFields(list, anonymousFields);
				_fields = list.ToArray();
				_size = PythonStruct.Align(num, num2);
				_alignment = num2;
			}
		}

		internal void EnsureFinal()
		{
			if (_fields == null)
			{
				SetFields(PythonTuple.EMPTY);
			}
		}
	}

	public const string __version__ = "1.1.0";

	public const int FUNCFLAG_STDCALL = 0;

	public const int FUNCFLAG_CDECL = 1;

	public const int FUNCFLAG_HRESULT = 2;

	public const int FUNCFLAG_PYTHONAPI = 4;

	public const int FUNCFLAG_USE_ERRNO = 8;

	public const int FUNCFLAG_USE_LASTERROR = 16;

	public const int RTLD_GLOBAL = 0;

	public const int RTLD_LOCAL = 0;

	private static readonly object _lock = new object();

	private static readonly object _pointerTypeCacheKey = new object();

	private static readonly object _conversion_mode = new object();

	private static Dictionary<object, RefCountInfo> _refCountTable;

	private static ModuleBuilder _dynamicModule;

	private static Dictionary<int, Type> _nativeTypes = new Dictionary<int, Type>();

	private static StringAtDelegate _stringAt = StringAt;

	private static StringAtDelegate _wstringAt = WStringAt;

	private static CastDelegate _cast = Cast;

	private static WeakDictionary<PythonType, Dictionary<int, ArrayType>> _arrayTypes = new WeakDictionary<PythonType, Dictionary<int, ArrayType>>();

	public static readonly PythonType _SimpleCData = SimpleType.MakeSystemType(typeof(SimpleCData));

	public static readonly PythonType CFuncPtr = CFuncPtrType.MakeSystemType(typeof(_CFuncPtr));

	public static readonly PythonType Structure = StructType.MakeSystemType(typeof(_Structure));

	public static readonly PythonType Union = UnionType.MakeSystemType(typeof(_Union));

	public static readonly PythonType _Pointer = PointerType.MakeSystemType(typeof(Pointer));

	public static readonly PythonType Array = ArrayType.MakeSystemType(typeof(_Array));

	public static object _cast_addr => Marshal.GetFunctionPointerForDelegate((Delegate)_cast).ToPython();

	public static object _memmove_addr => NativeFunctions.GetMemMoveAddress().ToPython();

	public static object _memset_addr => NativeFunctions.GetMemSetAddress().ToPython();

	public static object _string_at_addr => Marshal.GetFunctionPointerForDelegate((Delegate)_stringAt).ToPython();

	public static object _wstring_at_addr => Marshal.GetFunctionPointerForDelegate((Delegate)_wstringAt).ToPython();

	private static ModuleBuilder DynamicModule
	{
		get
		{
			if (_dynamicModule == null)
			{
				lock (_lock)
				{
					if (_dynamicModule == null)
					{
						CustomAttributeBuilder[] assemblyAttributes = new CustomAttributeBuilder[2]
						{
							new CustomAttributeBuilder(typeof(UnverifiableCodeAttribute).GetConstructor(ReflectionUtils.EmptyTypes), new object[0]),
							new CustomAttributeBuilder(typeof(PermissionSetAttribute).GetConstructor(new Type[1] { typeof(SecurityAction) }), new object[1] { SecurityAction.Demand }, new PropertyInfo[1] { typeof(PermissionSetAttribute).GetProperty("Unrestricted") }, new object[1] { true })
						};
						string text = typeof(CTypes).Namespace + ".DynamicAssembly";
						AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(text), AssemblyBuilderAccess.Run, assemblyAttributes);
						assemblyBuilder.DefineVersionInfoResource();
						_dynamicModule = assemblyBuilder.DefineDynamicModule(text);
					}
				}
			}
			return _dynamicModule;
		}
	}

	[SpecialName]
	public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
	{
		context.EnsureModuleException("ArgumentError", dict, "ArgumentError", "_ctypes");
		context.EnsureModuleException("COMError", dict, "COMError", "_ctypes");
		context.SystemState.__dict__["getrefcount"] = null;
		context.SetModuleState(value: dict["_pointer_type_cache"] = new PythonDictionary(), key: _pointerTypeCacheKey);
		if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows || Environment.OSVersion.Platform == PlatformID.WinCE)
		{
			context.SetModuleState(_conversion_mode, PythonTuple.MakeTuple("mbcs", "ignore"));
		}
		else
		{
			context.SetModuleState(_conversion_mode, PythonTuple.MakeTuple("ascii", "strict"));
		}
	}

	private static IntPtr Cast(IntPtr data, IntPtr obj, IntPtr type)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(obj);
		GCHandle gCHandle2 = GCHandle.FromIntPtr(type);
		try
		{
			CData cData = gCHandle.Target as CData;
			PythonType pythonType = (PythonType)gCHandle2.Target;
			CData cData2 = (CData)pythonType.CreateInstance(pythonType.Context.SharedContext);
			if (IsPointer(pythonType))
			{
				cData2._memHolder = new MemoryHolder(IntPtr.Size);
				if (IsPointer(DynamicHelpers.GetPythonType(cData)))
				{
					cData2._memHolder.WriteIntPtr(0, cData._memHolder.ReadIntPtr(0));
				}
				else
				{
					cData2._memHolder.WriteIntPtr(0, data);
				}
				if (cData != null)
				{
					cData2._memHolder.Objects = cData._memHolder.Objects;
					cData2._memHolder.AddObject(IdDispenser.GetId(cData), cData);
				}
			}
			else if (cData != null)
			{
				cData2._memHolder = new MemoryHolder(data, ((INativeType)pythonType).Size, cData._memHolder);
			}
			else
			{
				cData2._memHolder = new MemoryHolder(data, ((INativeType)pythonType).Size);
			}
			return GCHandle.ToIntPtr(GCHandle.Alloc(cData2));
		}
		finally
		{
			gCHandle2.Free();
			gCHandle.Free();
		}
	}

	private static bool IsPointer(PythonType pt)
	{
		if (!(pt is PointerType))
		{
			if (pt is SimpleType simpleType)
			{
				if (simpleType._type != SimpleTypeKind.Pointer && simpleType._type != SimpleTypeKind.CharPointer)
				{
					return simpleType._type == SimpleTypeKind.WCharPointer;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public static int CopyComPointer(object src, object dest)
	{
		throw new NotImplementedException("CopyComPointer");
	}

	public static string FormatError()
	{
		return FormatError(get_last_error());
	}

	public static string FormatError(int errorCode)
	{
		return new Win32Exception(errorCode).Message;
	}

	public static void FreeLibrary(int handle)
	{
		FreeLibrary(new IntPtr(handle));
	}

	public static void FreeLibrary(BigInteger handle)
	{
		FreeLibrary(new IntPtr((long)handle));
	}

	public static void FreeLibrary(IntPtr handle)
	{
		NativeFunctions.FreeLibrary(handle);
	}

	public static object LoadLibrary(string library, [DefaultParameterValue(0)] int mode)
	{
		IntPtr intPtr = NativeFunctions.LoadDLL(library, mode);
		if (intPtr == IntPtr.Zero)
		{
			throw PythonOps.OSError("cannot load library {0}", library);
		}
		return intPtr.ToPython();
	}

	public static object dlopen(string library, [DefaultParameterValue(0)] int mode)
	{
		return LoadLibrary(library, mode);
	}

	public static PythonType POINTER(CodeContext context, PythonType type)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		PythonDictionary pythonDictionary = (PythonDictionary)context2.GetModuleState(_pointerTypeCacheKey);
		lock (pythonDictionary)
		{
			if (!pythonDictionary.TryGetValue(type, out var value))
			{
				string name = ((type != null) ? ("LP_" + type.Name) : "c_void_p");
				value = (pythonDictionary[type] = MakePointer(context, name, PythonOps.MakeDictFromItems(type, "_type_")));
			}
			return value as PythonType;
		}
	}

	private static PointerType MakePointer(CodeContext context, string name, PythonDictionary dict)
	{
		return new PointerType(context, name, PythonTuple.MakeTuple(_Pointer), dict);
	}

	public static PythonType POINTER(CodeContext context, [NotNull] string name)
	{
		PythonType pythonType = MakePointer(context, name, new PythonDictionary());
		PythonContext context2 = PythonContext.GetContext(context);
		PythonDictionary pythonDictionary = (PythonDictionary)context2.GetModuleState(_pointerTypeCacheKey);
		lock (pythonDictionary)
		{
			pythonDictionary[Builtin.id(pythonType)] = pythonType;
			return pythonType;
		}
	}

	public static object PyObj_FromPtr(IntPtr address)
	{
		GCHandle gCHandle = GCHandle.FromIntPtr(address);
		object target = gCHandle.Target;
		gCHandle.Free();
		return target;
	}

	public static IntPtr PyObj_ToPtr(object obj)
	{
		return GCHandle.ToIntPtr(GCHandle.Alloc(obj));
	}

	public static void Py_DECREF(object key)
	{
		EnsureRefCountTable();
		lock (_refCountTable)
		{
			if (!_refCountTable.TryGetValue(key, out var value))
			{
				throw new InvalidOperationException();
			}
			value.RefCount--;
			if (value.RefCount == 0)
			{
				value.Handle.Free();
				_refCountTable.Remove(key);
			}
		}
	}

	public static void Py_INCREF(object key)
	{
		EnsureRefCountTable();
		lock (_refCountTable)
		{
			if (!_refCountTable.TryGetValue(key, out var value))
			{
				value = (_refCountTable[key] = new RefCountInfo());
				value.Handle = GCHandle.Alloc(key, GCHandleType.Pinned);
			}
			value.RefCount++;
		}
	}

	public static PythonTuple _buffer_info(CData data)
	{
		return data.GetBufferInfo();
	}

	public static void _check_HRESULT(int hresult)
	{
		if (hresult < 0)
		{
			throw PythonOps.WindowsError("ctypes function returned failed HRESULT: {0}", PythonOps.Hex((BigInteger)(uint)hresult));
		}
	}

	public static void _unpickle()
	{
	}

	public static object addressof(CData data)
	{
		return data._memHolder.UnsafeAddress.ToPython();
	}

	public static int alignment(PythonType type)
	{
		if (!(type is INativeType nativeType))
		{
			throw PythonOps.TypeError("this type has no size");
		}
		return nativeType.Alignment;
	}

	public static int alignment(object o)
	{
		return alignment(DynamicHelpers.GetPythonType(o));
	}

	public static object byref(CData instance, [DefaultParameterValue(0)] int offset)
	{
		if (offset != 0)
		{
			throw new NotImplementedException("byref w/ arg");
		}
		return new NativeArgument(instance, "P");
	}

	public static object call_cdeclfunction(CodeContext context, int address, PythonTuple args)
	{
		return call_cdeclfunction(context, new IntPtr(address), args);
	}

	public static object call_cdeclfunction(CodeContext context, BigInteger address, PythonTuple args)
	{
		return call_cdeclfunction(context, new IntPtr((long)address), args);
	}

	public static object call_cdeclfunction(CodeContext context, IntPtr address, PythonTuple args)
	{
		CFuncPtrType functionType = GetFunctionType(context, 1);
		_CFuncPtr func = (_CFuncPtr)functionType.CreateInstance(context, address);
		return PythonOps.CallWithArgsTuple(func, new object[0], args);
	}

	public static void call_commethod()
	{
	}

	public static object call_function(CodeContext context, int address, PythonTuple args)
	{
		return call_function(context, new IntPtr(address), args);
	}

	public static object call_function(CodeContext context, BigInteger address, PythonTuple args)
	{
		return call_function(context, new IntPtr((long)address), args);
	}

	public static object call_function(CodeContext context, IntPtr address, PythonTuple args)
	{
		CFuncPtrType functionType = GetFunctionType(context, 0);
		_CFuncPtr func = (_CFuncPtr)functionType.CreateInstance(context, address);
		return PythonOps.CallWithArgsTuple(func, new object[0], args);
	}

	private static CFuncPtrType GetFunctionType(CodeContext context, int flags)
	{
		SimpleType simpleType = new SimpleType(context, "int", PythonTuple.MakeTuple(DynamicHelpers.GetPythonTypeFromType(typeof(SimpleCData))), PythonOps.MakeHomogeneousDictFromItems(new object[2] { "i", "_type_" }));
		return new CFuncPtrType(context, "func", PythonTuple.MakeTuple(DynamicHelpers.GetPythonTypeFromType(typeof(_CFuncPtr))), PythonOps.MakeHomogeneousDictFromItems(new object[4] { 0, "_flags_", simpleType, "_restype_" }));
	}

	public static int get_errno()
	{
		return 0;
	}

	public static int get_last_error()
	{
		return Marshal.GetLastWin32Error();
	}

	public static Pointer pointer(CodeContext context, CData data)
	{
		PythonType pythonType = POINTER(context, DynamicHelpers.GetPythonType(data));
		return (Pointer)pythonType.CreateInstance(context, data);
	}

	public static void resize(CData obj, int newSize)
	{
		if (newSize < obj.NativeType.Size)
		{
			throw PythonOps.ValueError("minimum size is {0}", newSize);
		}
		MemoryHolder memoryHolder = new MemoryHolder(newSize);
		obj._memHolder.CopyTo(memoryHolder, 0, Math.Min(obj._memHolder.Size, newSize));
		obj._memHolder = memoryHolder;
	}

	public static PythonTuple set_conversion_mode(CodeContext context, string encoding, string errors)
	{
		PythonContext context2 = PythonContext.GetContext(context);
		PythonTuple result = (PythonTuple)context2.GetModuleState(_conversion_mode);
		context2.SetModuleState(_conversion_mode, PythonTuple.MakeTuple(encoding, errors));
		return result;
	}

	public static void set_errno()
	{
	}

	public static void set_last_error(int errorCode)
	{
		NativeFunctions.SetLastError(errorCode);
	}

	public static int @sizeof(PythonType type)
	{
		if (!(type is INativeType nativeType))
		{
			throw PythonOps.TypeError("this type has no size");
		}
		return nativeType.Size;
	}

	public static int @sizeof(object instance)
	{
		if (instance is CData { _memHolder: not null } cData)
		{
			return cData._memHolder.Size;
		}
		return @sizeof(DynamicHelpers.GetPythonType(instance));
	}

	private static Type GetMarshalTypeFromSize(int size)
	{
		lock (_nativeTypes)
		{
			if (!_nativeTypes.TryGetValue(size, out var value))
			{
				int num = size;
				TypeBuilder typeBuilder = DynamicModule.DefineType("interop_type_size_" + size, TypeAttributes.Public | TypeAttributes.SequentialLayout | TypeAttributes.Sealed | TypeAttributes.Serializable, typeof(ValueType), size);
				while (num > 8)
				{
					typeBuilder.DefineField("field" + num, typeof(long), FieldAttributes.Private);
					num -= 8;
				}
				while (num > 4)
				{
					typeBuilder.DefineField("field" + num, typeof(int), FieldAttributes.Private);
					num -= 4;
				}
				while (num > 0)
				{
					typeBuilder.DefineField("field" + num, typeof(byte), FieldAttributes.Private);
					num--;
				}
				value = (_nativeTypes[size] = typeBuilder.CreateType());
			}
			return value;
		}
	}

	private static void GetFieldInfo(INativeType type, object o, out string fieldName, out INativeType cdata, out int? bitCount)
	{
		PythonTuple pythonTuple = o as PythonTuple;
		if (pythonTuple.Count != 2 && pythonTuple.Count != 3)
		{
			throw PythonOps.AttributeError("'_fields_' must be a sequence of pairs");
		}
		fieldName = pythonTuple[0] as string;
		if (fieldName == null)
		{
			throw PythonOps.TypeError("first item in _fields_ tuple must be a string, got", PythonTypeOps.GetName(pythonTuple[0]));
		}
		cdata = pythonTuple[1] as INativeType;
		if (cdata == null)
		{
			throw PythonOps.TypeError("second item in _fields_ tuple must be a C type, got {0}", PythonTypeOps.GetName(pythonTuple[0]));
		}
		if (cdata == type)
		{
			throw StructureCannotContainSelf();
		}
		if (cdata is StructType structType)
		{
			structType.EnsureFinal();
		}
		if (pythonTuple.Count != 3)
		{
			bitCount = null;
		}
		else
		{
			bitCount = CheckBits(cdata, pythonTuple);
		}
	}

	private static int CheckBits(INativeType cdata, PythonTuple pt)
	{
		int num = Converter.ConvertToInt32(pt[2]);
		if (!(cdata is SimpleType { _type: var type }))
		{
			throw PythonOps.TypeError("bit fields not allowed for type {0}", ((PythonType)cdata).Name);
		}
		switch (type)
		{
		case SimpleTypeKind.Char:
		case SimpleTypeKind.Single:
		case SimpleTypeKind.Double:
		case SimpleTypeKind.Object:
		case SimpleTypeKind.Pointer:
		case SimpleTypeKind.CharPointer:
		case SimpleTypeKind.WCharPointer:
		case SimpleTypeKind.WChar:
			throw PythonOps.TypeError("bit fields not allowed for type {0}", ((PythonType)cdata).Name);
		default:
			if (num <= 0 || num > cdata.Size * 8)
			{
				throw PythonOps.ValueError("number of bits invalid for bit field");
			}
			return num;
		}
	}

	private static IList<object> GetFieldsList(object fields)
	{
		if (!(fields is IList<object> result))
		{
			throw PythonOps.TypeError("class must be a sequence of pairs");
		}
		return result;
	}

	private static Exception StructureCannotContainSelf()
	{
		return PythonOps.AttributeError("Structure or union cannot contain itself");
	}

	private static IntPtr StringAt(IntPtr src, int len)
	{
		string value = ((len != -1) ? MemoryHolder.ReadAnsiString(src, 0, len) : MemoryHolder.ReadAnsiString(src, 0));
		return GCHandle.ToIntPtr(GCHandle.Alloc(value));
	}

	private static IntPtr WStringAt(IntPtr src, int len)
	{
		string value = ((len != -1) ? Marshal.PtrToStringUni(src, len) : Marshal.PtrToStringUni(src));
		return GCHandle.ToIntPtr(GCHandle.Alloc(value));
	}

	private static IntPtr GetHandleFromObject(object dll, string errorMsg)
	{
		object boundAttr = PythonOps.GetBoundAttr(DefaultContext.Default, dll, "_handle");
		if (!Converter.TryConvertToBigInteger(boundAttr, out var result))
		{
			throw PythonOps.TypeError(errorMsg);
		}
		return new IntPtr((long)result);
	}

	private static void ValidateArraySizes(ArrayModule.array array, int offset, int size)
	{
		ValidateArraySizes(array.__len__() * array.itemsize, offset, size);
	}

	private static void ValidateArraySizes(Bytes bytes, int offset, int size)
	{
		ValidateArraySizes(bytes.Count, offset, size);
	}

	private static void ValidateArraySizes(int arraySize, int offset, int size)
	{
		if (offset < 0)
		{
			throw PythonOps.ValueError("offset cannot be negative");
		}
		if (arraySize < size + offset)
		{
			throw PythonOps.ValueError("Buffer size too small ({0} instead of at least {1} bytes)", arraySize, size);
		}
	}

	public static object GetCharArrayValue(_Array arr)
	{
		return arr.NativeType.GetValue(arr._memHolder, arr, 0, raw: false);
	}

	public static void SetCharArrayValue(_Array arr, object value)
	{
		if (value is PythonBuffer pythonBuffer && pythonBuffer._object is string)
		{
			value = pythonBuffer.ToString();
		}
		arr.NativeType.SetValue(arr._memHolder, 0, value);
	}

	public static void DeleteCharArrayValue(_Array arr, object value)
	{
		throw PythonOps.TypeError("cannot delete char array value");
	}

	public static object GetWCharArrayValue(_Array arr)
	{
		return arr.NativeType.GetValue(arr._memHolder, arr, 0, raw: false);
	}

	public static void SetWCharArrayValue(_Array arr, object value)
	{
		arr.NativeType.SetValue(arr._memHolder, 0, value);
	}

	public static object DeleteWCharArrayValue(_Array arr)
	{
		throw PythonOps.TypeError("cannot delete wchar array value");
	}

	public static object GetWCharArrayRaw(_Array arr)
	{
		return ((ArrayType)arr.NativeType).GetRawValue(arr._memHolder, 0);
	}

	public static void SetWCharArrayRaw(_Array arr, object value)
	{
		if (value is PythonBuffer pythonBuffer && (pythonBuffer._object is string || pythonBuffer._object is Bytes))
		{
			value = pythonBuffer.ToString();
		}
		if (value is MemoryView memoryView)
		{
			string text = memoryView.tobytes().ToString();
			if (text.Length > arr.__len__())
			{
				throw PythonOps.ValueError("string too long");
			}
			value = text;
		}
		arr.NativeType.SetValue(arr._memHolder, 0, value);
	}

	public static object DeleteWCharArrayRaw(_Array arr)
	{
		throw PythonOps.TypeError("cannot delete wchar array raw");
	}

	private static void EmitCDataCreation(INativeType type, ILGenerator method, List<object> constantPool, int constantPoolArgument)
	{
		LocalBuilder local = method.DeclareLocal(type.GetNativeType());
		method.Emit(OpCodes.Stloc, local);
		method.Emit(OpCodes.Ldloca, local);
		constantPool.Add(type);
		method.Emit(OpCodes.Ldarg, constantPoolArgument);
		method.Emit(OpCodes.Ldc_I4, constantPool.Count - 1);
		method.Emit(OpCodes.Ldelem_Ref);
		method.Emit(OpCodes.Call, typeof(ModuleOps).GetMethod("CreateCData"));
	}

	private static void EnsureRefCountTable()
	{
		if (_refCountTable == null)
		{
			Interlocked.CompareExchange(ref _refCountTable, new Dictionary<object, RefCountInfo>(), null);
		}
	}

	private static ArrayType MakeArrayType(PythonType type, int count)
	{
		if (count < 0)
		{
			throw PythonOps.ValueError("cannot multiply ctype by negative number");
		}
		lock (_arrayTypes)
		{
			if (!_arrayTypes.TryGetValue(type, out var value))
			{
				value = (_arrayTypes[type] = new Dictionary<int, ArrayType>());
			}
			if (!value.TryGetValue(count, out var value2))
			{
				return value[count] = new ArrayType(type.Context.SharedContext, type.Name + "_Array_" + count, PythonTuple.MakeTuple(Array), PythonOps.MakeDictFromItems(type, "_type_", count, "_length_"));
			}
			return value2;
		}
	}
}
