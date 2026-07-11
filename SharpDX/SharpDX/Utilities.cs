using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using SharpDX.Direct3D;

namespace SharpDX;

public static class Utilities
{
	[Flags]
	public enum CLSCTX : uint
	{
		ClsctxInprocServer = 1u,
		ClsctxInprocHandler = 2u,
		ClsctxLocalServer = 4u,
		ClsctxInprocServer16 = 8u,
		ClsctxRemoteServer = 0x10u,
		ClsctxInprocHandler16 = 0x20u,
		ClsctxReserved1 = 0x40u,
		ClsctxReserved2 = 0x80u,
		ClsctxReserved3 = 0x100u,
		ClsctxReserved4 = 0x200u,
		ClsctxNoCodeDownload = 0x400u,
		ClsctxReserved5 = 0x800u,
		ClsctxNoCustomMarshal = 0x1000u,
		ClsctxEnableCodeDownload = 0x2000u,
		ClsctxNoFailureLog = 0x4000u,
		ClsctxDisableAaa = 0x8000u,
		ClsctxEnableAaa = 0x10000u,
		ClsctxFromDefaultContext = 0x20000u,
		ClsctxInproc = 3u,
		ClsctxServer = 0x15u,
		ClsctxAll = 0x17u
	}

	public enum CoInit
	{
		MultiThreaded = 0,
		ApartmentThreaded = 2,
		DisableOle1Dde = 4,
		SpeedOverMemory = 8
	}

	internal struct Buffer<TElement>
	{
		internal TElement[] items;

		internal int count;

		internal Buffer(IEnumerable<TElement> source)
		{
			TElement[] array = null;
			int num = 0;
			if (source is ICollection<TElement> collection)
			{
				num = collection.Count;
				if (num > 0)
				{
					array = new TElement[num];
					collection.CopyTo(array, 0);
				}
			}
			else
			{
				foreach (TElement item in source)
				{
					if (array == null)
					{
						array = new TElement[4];
					}
					else if (array.Length == num)
					{
						TElement[] array2 = new TElement[checked(num * 2)];
						Array.Copy(array, 0, array2, 0, num);
						array = array2;
					}
					array[num] = item;
					num++;
				}
			}
			items = array;
			count = num;
		}

		internal TElement[] ToArray()
		{
			if (count == 0)
			{
				return new TElement[0];
			}
			if (items.Length == count)
			{
				return items;
			}
			TElement[] array = new TElement[count];
			Array.Copy(items, 0, array, 0, count);
			return array;
		}
	}

	public unsafe static void CopyMemory(IntPtr dest, IntPtr src, int sizeInBytesToCopy)
	{
		Interop.memcpy((void*)dest, (void*)src, sizeInBytesToCopy);
	}

	public unsafe static bool CompareMemory(IntPtr from, IntPtr against, int sizeToCompare)
	{
		byte* ptr = (byte*)(void*)from;
		byte* ptr2 = (byte*)(void*)against;
		for (int num = sizeToCompare >> 3; num > 0; num--)
		{
			if (*(long*)ptr != *(long*)ptr2)
			{
				return false;
			}
			ptr += 8;
			ptr2 += 8;
		}
		for (int num = sizeToCompare & 7; num > 0; num--)
		{
			if (*ptr != *ptr2)
			{
				return false;
			}
			ptr++;
			ptr2++;
		}
		return true;
	}

	public unsafe static void ClearMemory(IntPtr dest, byte value, int sizeInBytesToClear)
	{
		Interop.memset((void*)dest, value, sizeInBytesToClear);
	}

	public static int SizeOf<T>() where T : struct
	{
		return System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
	}

	public static int SizeOf<T>(T[] array) where T : struct
	{
		if (array != null)
		{
			return array.Length * System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
		}
		return 0;
	}

	public unsafe static void Pin<T>(ref T source, Action<IntPtr> pinAction) where T : struct
	{
		fixed (T* ptr = &source)
		{
			pinAction((IntPtr)ptr);
		}
	}

	public unsafe static void Pin<T>(T[] source, Action<IntPtr> pinAction) where T : struct
	{
		//The blocks IL_0019 are reachable both inside and outside the pinned region starting at IL_000b. ILSpy has duplicated these blocks in order to place them both within and outside the `fixed` statement.
		IntPtr obj;
		if (source != null)
		{
			fixed (T* ptr = &source[0])
			{
				obj = (IntPtr)ptr;
				pinAction(obj);
				return;
			}
		}
		obj = IntPtr.Zero;
		pinAction(obj);
	}

	public unsafe static byte[] ToByteArray<T>(T[] source) where T : struct
	{
		if (source == null)
		{
			return null;
		}
		byte[] array = new byte[SizeOf<T>() * source.Length];
		if (source.Length == 0)
		{
			return array;
		}
		fixed (IntPtr* pDest = array)
		{
			Interop.Write(pDest, source, 0, source.Length);
		}
		return array;
	}

	public static void Swap<T>(ref T left, ref T right)
	{
		T val = left;
		left = right;
		right = val;
	}

	public unsafe static T Read<T>(IntPtr source) where T : struct
	{
		return System.Runtime.CompilerServices.Unsafe.Read<T>((void*)source);
	}

	public unsafe static void Read<T>(IntPtr source, ref T data) where T : struct
	{
		data = System.Runtime.CompilerServices.Unsafe.Read<T>((void*)source);
	}

	public unsafe static void ReadOut<T>(IntPtr source, out T data) where T : struct
	{
		data = System.Runtime.CompilerServices.Unsafe.Read<T>((void*)source);
	}

	public unsafe static IntPtr ReadAndPosition<T>(IntPtr source, ref T data) where T : struct
	{
		return (IntPtr)Interop.Read((void*)source, ref data);
	}

	public unsafe static IntPtr Read<T>(IntPtr source, T[] data, int offset, int count) where T : struct
	{
		return (IntPtr)Interop.Read((void*)source, data, offset, count);
	}

	public unsafe static void Write<T>(IntPtr destination, ref T data) where T : struct
	{
		System.Runtime.CompilerServices.Unsafe.Write((void*)destination, data);
	}

	public unsafe static IntPtr WriteAndPosition<T>(IntPtr destination, ref T data) where T : struct
	{
		return (IntPtr)Interop.Write((void*)destination, ref data);
	}

	public unsafe static IntPtr Write<T>(IntPtr destination, T[] data, int offset, int count) where T : struct
	{
		return (IntPtr)Interop.Write((void*)destination, data, offset, count);
	}

	public unsafe static void ConvertToIntArray(bool[] array, int* dest)
	{
		for (int i = 0; i < array.Length; i++)
		{
			dest[i] = (array[i] ? 1 : 0);
		}
	}

	public static Bool[] ConvertToIntArray(bool[] array)
	{
		Bool[] array2 = new Bool[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			ref Bool reference = ref array2[i];
			reference = array[i];
		}
		return array2;
	}

	public unsafe static bool[] ConvertToBoolArray(int* array, int length)
	{
		bool[] array2 = new bool[length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = array[i] != 0;
		}
		return array2;
	}

	public static bool[] ConvertToBoolArray(Bool[] array)
	{
		bool[] array2 = new bool[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = array[i];
		}
		return array2;
	}

	public static Guid GetGuidFromType(Type type)
	{
		return type.GUID;
	}

	public static bool IsAssignableToGenericType(Type givenType, Type genericType)
	{
		Type[] interfaces = givenType.GetInterfaces();
		Type[] array = interfaces;
		foreach (Type type in array)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
			{
				return true;
			}
		}
		if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
		{
			return true;
		}
		Type baseType = givenType.BaseType;
		if (baseType == null)
		{
			return false;
		}
		return IsAssignableToGenericType(baseType, genericType);
	}

	public unsafe static IntPtr AllocateMemory(int sizeInBytes, int align = 16)
	{
		int num = align - 1;
		IntPtr intPtr = Marshal.AllocHGlobal(sizeInBytes + num + IntPtr.Size);
		long num2 = (long)((byte*)(void*)intPtr + sizeof(void*) + num) & (long)(~num);
		*(IntPtr*)((nint)num2 + (nint)(-1) * (nint)sizeof(IntPtr)) = intPtr;
		return new IntPtr(num2);
	}

	public static IntPtr AllocateClearedMemory(int sizeInBytes, byte clearValue = 0, int align = 16)
	{
		IntPtr intPtr = AllocateMemory(sizeInBytes, align);
		ClearMemory(intPtr, clearValue, sizeInBytes);
		return intPtr;
	}

	public static bool IsMemoryAligned(IntPtr memoryPtr, int align = 16)
	{
		return (memoryPtr.ToInt64() & (align - 1)) == 0;
	}

	public unsafe static void FreeMemory(IntPtr alignedBuffer)
	{
		if (!(alignedBuffer == IntPtr.Zero))
		{
			Marshal.FreeHGlobal(((IntPtr*)(void*)alignedBuffer)[-1]);
		}
	}

	public unsafe static string PtrToStringAnsi(IntPtr pointer, int maxLength)
	{
		byte* ptr = (byte*)(void*)pointer;
		for (int i = 0; i < maxLength; i++)
		{
			if (*(ptr++) == 0)
			{
				return new string((sbyte*)(void*)pointer);
			}
		}
		return new string((sbyte*)(void*)pointer, 0, maxLength);
	}

	public unsafe static string PtrToStringUni(IntPtr pointer, int maxLength)
	{
		char* ptr = (char*)(void*)pointer;
		for (int i = 0; i < maxLength; i++)
		{
			if (*(ptr++) == '\0')
			{
				return new string((char*)(void*)pointer);
			}
		}
		return new string((char*)(void*)pointer, 0, maxLength);
	}

	public static IntPtr StringToHGlobalAnsi(string s)
	{
		return Marshal.StringToHGlobalAnsi(s);
	}

	public static IntPtr StringToHGlobalUni(string s)
	{
		return Marshal.StringToHGlobalUni(s);
	}

	public static IntPtr StringToCoTaskMemUni(string s)
	{
		if (s == null)
		{
			return IntPtr.Zero;
		}
		int num = (s.Length + 1) * 2;
		if (num < s.Length)
		{
			throw new ArgumentOutOfRangeException("s");
		}
		IntPtr intPtr = Marshal.AllocCoTaskMem(num);
		if (intPtr == IntPtr.Zero)
		{
			throw new OutOfMemoryException();
		}
		CopyStringToUnmanaged(intPtr, s);
		return intPtr;
	}

	private unsafe static void CopyStringToUnmanaged(IntPtr ptr, string str)
	{
		fixed (char* value = str)
		{
			CopyMemory(ptr, new IntPtr(value), (str.Length + 1) * 2);
		}
	}

	public static IntPtr GetIUnknownForObject(object obj)
	{
		return (obj == null) ? IntPtr.Zero : Marshal.GetIUnknownForObject(obj);
	}

	public static object GetObjectForIUnknown(IntPtr iunknownPtr)
	{
		if (!(iunknownPtr == IntPtr.Zero))
		{
			return Marshal.GetObjectForIUnknown(iunknownPtr);
		}
		return null;
	}

	public static string Join<T>(string separator, T[] array)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(separator);
				}
				stringBuilder.Append(array[i]);
			}
		}
		return stringBuilder.ToString();
	}

	public static string Join(string separator, IEnumerable elements)
	{
		List<string> list = new List<string>();
		foreach (object element in elements)
		{
			list.Add(element.ToString());
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			string value = list[i];
			if (i > 0)
			{
				stringBuilder.Append(separator);
			}
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}

	public static string Join(string separator, IEnumerator elements)
	{
		List<string> list = new List<string>();
		while (elements.MoveNext())
		{
			list.Add(elements.Current.ToString());
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			string value = list[i];
			if (i > 0)
			{
				stringBuilder.Append(separator);
			}
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}

	public unsafe static string BlobToString(Blob blob)
	{
		if (blob == null)
		{
			return null;
		}
		string result = new string((sbyte*)(void*)blob.BufferPointer);
		blob.Dispose();
		return result;
	}

	public unsafe static IntPtr IntPtrAdd(IntPtr ptr, int offset)
	{
		return new IntPtr((byte*)(void*)ptr + offset);
	}

	public static byte[] ReadStream(Stream stream)
	{
		int readLength = 0;
		return ReadStream(stream, ref readLength);
	}

	public static byte[] ReadStream(Stream stream, ref int readLength)
	{
		if (readLength == 0)
		{
			readLength = (int)(stream.Length - stream.Position);
		}
		int num = readLength;
		if (num == 0)
		{
			return new byte[0];
		}
		byte[] array = new byte[num];
		int num2 = 0;
		if (num > 0)
		{
			do
			{
				num2 += stream.Read(array, num2, readLength - num2);
			}
			while (num2 < readLength);
		}
		return array;
	}

	public static bool Compare(IEnumerable left, IEnumerable right)
	{
		if (object.ReferenceEquals(left, right))
		{
			return true;
		}
		if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
		{
			return false;
		}
		return Compare(left.GetEnumerator(), right.GetEnumerator());
	}

	public static bool Compare(IEnumerator leftIt, IEnumerator rightIt)
	{
		if (object.ReferenceEquals(leftIt, rightIt))
		{
			return true;
		}
		if (object.ReferenceEquals(leftIt, null) || object.ReferenceEquals(rightIt, null))
		{
			return false;
		}
		bool flag;
		bool flag2;
		while (true)
		{
			flag = leftIt.MoveNext();
			flag2 = rightIt.MoveNext();
			if (!flag || !flag2)
			{
				break;
			}
			if (!object.Equals(leftIt.Current, rightIt.Current))
			{
				return false;
			}
		}
		if (flag != flag2)
		{
			return false;
		}
		return true;
	}

	public static bool Compare(ICollection left, ICollection right)
	{
		if (object.ReferenceEquals(left, right))
		{
			return true;
		}
		if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
		{
			return false;
		}
		if (left.Count != right.Count)
		{
			return false;
		}
		int num = 0;
		IEnumerator enumerator = left.GetEnumerator();
		IEnumerator enumerator2 = right.GetEnumerator();
		while (enumerator.MoveNext() && enumerator2.MoveNext())
		{
			if (!object.Equals(enumerator.Current, enumerator2.Current))
			{
				return false;
			}
			num++;
		}
		if (num != left.Count)
		{
			return false;
		}
		return true;
	}

	public static T GetCustomAttribute<T>(MemberInfo memberInfo, bool inherited = false) where T : Attribute
	{
		object[] customAttributes = memberInfo.GetCustomAttributes(typeof(T), inherited);
		if (customAttributes.Length == 0)
		{
			return null;
		}
		return (T)customAttributes[0];
	}

	public static IEnumerable<T> GetCustomAttributes<T>(MemberInfo memberInfo, bool inherited = false) where T : Attribute
	{
		object[] customAttributes = memberInfo.GetCustomAttributes(typeof(T), inherited);
		if (customAttributes.Length == 0)
		{
			return new T[0];
		}
		T[] array = new T[customAttributes.Length];
		Array.Copy(customAttributes, array, customAttributes.Length);
		return array;
	}

	public static bool IsAssignableFrom(Type toType, Type fromType)
	{
		return toType.IsAssignableFrom(fromType);
	}

	public static bool IsEnum(Type typeToTest)
	{
		return typeToTest.IsEnum;
	}

	public static bool IsValueType(Type typeToTest)
	{
		return typeToTest.IsValueType;
	}

	private static MethodInfo GetMethod(Type type, string name, Type[] typeArgs)
	{
		return type.GetMethod(name, typeArgs);
	}

	public static GetValueFastDelegate<T> BuildPropertyGetter<T>(Type customEffectType, PropertyInfo propertyInfo)
	{
		Type typeFromHandle = typeof(T);
		Type propertyType = propertyInfo.PropertyType;
		DynamicMethod dynamicMethod = new DynamicMethod("GetValueDelegate", typeof(void), new Type[2]
		{
			typeof(object),
			typeFromHandle.MakeByRefType()
		});
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_1);
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Castclass, customEffectType);
		iLGenerator.EmitCall(OpCodes.Callvirt, propertyInfo.GetGetMethod(), null);
		if (typeFromHandle == typeof(byte) || typeFromHandle == typeof(sbyte))
		{
			iLGenerator.Emit(OpCodes.Stind_I1);
		}
		else if (typeFromHandle == typeof(short) || typeFromHandle == typeof(ushort))
		{
			iLGenerator.Emit(OpCodes.Stind_I2);
		}
		else if (typeFromHandle == typeof(int) || typeFromHandle == typeof(uint))
		{
			if (propertyType == typeof(bool))
			{
				iLGenerator.EmitCall(OpCodes.Call, GetMethod(typeof(Convert), "ToInt32", new Type[1] { typeof(bool) }), null);
			}
			iLGenerator.Emit(OpCodes.Stind_I4);
		}
		else if (typeFromHandle == typeof(long) || typeFromHandle == typeof(ulong))
		{
			iLGenerator.Emit(OpCodes.Stind_I8);
		}
		else if (typeFromHandle == typeof(float))
		{
			iLGenerator.Emit(OpCodes.Stind_R4);
		}
		else if (typeFromHandle == typeof(double))
		{
			iLGenerator.Emit(OpCodes.Stind_R8);
		}
		else
		{
			MethodInfo methodInfo = FindExplicitConverstion(propertyType, typeFromHandle);
			if (methodInfo != null)
			{
				iLGenerator.EmitCall(OpCodes.Call, methodInfo, null);
			}
			iLGenerator.Emit(OpCodes.Stobj, typeof(T));
		}
		iLGenerator.Emit(OpCodes.Ret);
		return (GetValueFastDelegate<T>)dynamicMethod.CreateDelegate(typeof(GetValueFastDelegate<T>));
	}

	public static SetValueFastDelegate<T> BuildPropertySetter<T>(Type customEffectType, PropertyInfo propertyInfo)
	{
		Type typeFromHandle = typeof(T);
		Type propertyType = propertyInfo.PropertyType;
		DynamicMethod dynamicMethod = new DynamicMethod("SetValueDelegate", typeof(void), new Type[2]
		{
			typeof(object),
			typeFromHandle.MakeByRefType()
		});
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Castclass, customEffectType);
		iLGenerator.Emit(OpCodes.Ldarg_1);
		if (typeFromHandle == typeof(byte) || typeFromHandle == typeof(sbyte))
		{
			iLGenerator.Emit(OpCodes.Ldind_I1);
		}
		else if (typeFromHandle == typeof(short) || typeFromHandle == typeof(ushort))
		{
			iLGenerator.Emit(OpCodes.Ldind_I2);
		}
		else if (typeFromHandle == typeof(int) || typeFromHandle == typeof(uint))
		{
			iLGenerator.Emit(OpCodes.Ldind_I4);
			if (propertyType == typeof(bool))
			{
				iLGenerator.EmitCall(OpCodes.Call, GetMethod(typeof(Convert), "ToBoolean", new Type[1] { typeFromHandle }), null);
			}
		}
		else if (typeFromHandle == typeof(long) || typeFromHandle == typeof(ulong))
		{
			iLGenerator.Emit(OpCodes.Ldind_I8);
		}
		else if (typeFromHandle == typeof(float))
		{
			iLGenerator.Emit(OpCodes.Ldind_R4);
		}
		else if (typeFromHandle == typeof(double))
		{
			iLGenerator.Emit(OpCodes.Ldind_R8);
		}
		else
		{
			iLGenerator.Emit(OpCodes.Ldobj, typeof(T));
			MethodInfo methodInfo = FindExplicitConverstion(typeFromHandle, propertyType);
			if (methodInfo != null)
			{
				iLGenerator.EmitCall(OpCodes.Call, methodInfo, null);
			}
		}
		iLGenerator.EmitCall(OpCodes.Callvirt, propertyInfo.GetSetMethod(), null);
		iLGenerator.Emit(OpCodes.Ret);
		return (SetValueFastDelegate<T>)dynamicMethod.CreateDelegate(typeof(SetValueFastDelegate<T>));
	}

	public static void Sleep(TimeSpan sleepTimeInMillis)
	{
		Thread.Sleep(sleepTimeInMillis);
	}

	private static MethodInfo FindExplicitConverstion(Type sourceType, Type targetType)
	{
		if (sourceType == targetType)
		{
			return null;
		}
		List<MethodInfo> list = new List<MethodInfo>();
		Type type = sourceType;
		while (type != null)
		{
			list.AddRange(type.GetMethods(BindingFlags.Static | BindingFlags.Public));
			type = type.BaseType;
		}
		type = targetType;
		while (type != null)
		{
			list.AddRange(type.GetMethods(BindingFlags.Static | BindingFlags.Public));
			type = type.BaseType;
		}
		foreach (MethodInfo item in list)
		{
			if (item.Name == "op_Explicit" && item.ReturnType == targetType && IsAssignableFrom(item.GetParameters()[0].ParameterType, sourceType))
			{
				return item;
			}
		}
		return null;
	}

	[DllImport("ole32.dll", ExactSpelling = true)]
	private static extern Result CoCreateInstance([In][MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, IntPtr pUnkOuter, CLSCTX dwClsContext, [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IntPtr comObject);

	internal static void CreateComInstance(Guid clsid, CLSCTX clsctx, Guid riid, ComObject comObject)
	{
		CoCreateInstance(clsid, IntPtr.Zero, clsctx, riid, out var comObject2).CheckError();
		comObject.NativePointer = comObject2;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern bool CloseHandle(IntPtr handle);

	public static IntPtr LoadLibrary(string dllName)
	{
		IntPtr intPtr = LoadLibrary_(dllName);
		if (intPtr == IntPtr.Zero)
		{
			throw new DllNotFoundException($"Unable to find [{dllName}] in the PATH");
		}
		return intPtr;
	}

	[DllImport("kernel32", CharSet = CharSet.Unicode, EntryPoint = "LoadLibrary", SetLastError = true)]
	private static extern IntPtr LoadLibrary_(string lpFileName);

	public static IntPtr GetProcAddress(IntPtr handle, string dllFunctionToImport)
	{
		IntPtr procAddress_ = GetProcAddress_(handle, dllFunctionToImport);
		if (procAddress_ == IntPtr.Zero)
		{
			throw new SharpDXException(dllFunctionToImport);
		}
		return procAddress_;
	}

	[DllImport("kernel32", CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress", ExactSpelling = true, SetLastError = true)]
	private static extern IntPtr GetProcAddress_(IntPtr hModule, string procName);

	public static int ComputeHashFNVModified(byte[] data)
	{
		uint num = 2166136261u;
		foreach (byte b in data)
		{
			num = (num ^ b) * 16777619;
		}
		num += num << 13;
		num ^= num >> 7;
		num += num << 3;
		num ^= num >> 17;
		return (int)(num + (num << 5));
	}

	public static void Dispose<T>(ref T comObject) where T : class, IDisposable
	{
		if (comObject != null)
		{
			comObject.Dispose();
			comObject = null;
		}
	}

	public static T[] ToArray<T>(IEnumerable<T> source)
	{
		return new Buffer<T>(source).ToArray();
	}

	public static bool Any<T>(IEnumerable<T> source)
	{
		return source.GetEnumerator().MoveNext();
	}

	public static IEnumerable<TResult> SelectMany<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
	{
		foreach (TSource sourceItem in source)
		{
			foreach (TResult item in selector(sourceItem))
			{
				yield return item;
			}
		}
	}

	public static IEnumerable<TSource> Distinct<TSource>(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer = null)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<TSource>.Default;
		}
		Dictionary<TSource, object> values = new Dictionary<TSource, object>(comparer);
		foreach (TSource sourceItem in source)
		{
			if (!values.ContainsKey(sourceItem))
			{
				values.Add(sourceItem, null);
				yield return sourceItem;
			}
		}
	}

	public static bool IsTypeInheritFrom(Type type, string parentType)
	{
		while (type != null)
		{
			if (type.FullName == parentType)
			{
				return true;
			}
			type = type.BaseType;
		}
		return false;
	}
}
