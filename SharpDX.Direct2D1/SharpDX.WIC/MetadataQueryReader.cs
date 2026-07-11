using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpDX.Win32;

namespace SharpDX.WIC;

[Guid("30989668-E1C9-4597-B395-458EEDB808DF")]
public class MetadataQueryReader : ComObject
{
	public Guid ContainerFormat
	{
		get
		{
			GetContainerFormat(out var guidContainerFormatRef);
			return guidContainerFormatRef;
		}
	}

	public IEnumerable<string> Enumerator => new ComStringEnumerator(GetEnumerator());

	public IEnumerable<string> QueryPaths
	{
		get
		{
			foreach (string name in Enumerator)
			{
				if (TryGetMetadataByName(name, out var value).Success)
				{
					if (!(value is MetadataQueryReader subReader))
					{
						yield return name;
						continue;
					}
					foreach (string subPath in subReader.QueryPaths)
					{
						yield return name + subPath;
					}
				}
				else
				{
					yield return name;
				}
			}
		}
	}

	public unsafe string Location
	{
		get
		{
			int cchActualLengthRef = 0;
			GetLocation(0, IntPtr.Zero, out cchActualLengthRef);
			if (cchActualLengthRef == 0)
			{
				return null;
			}
			char* ptr = stackalloc char[cchActualLengthRef];
			GetLocation(cchActualLengthRef, (IntPtr)ptr, out cchActualLengthRef);
			return new string(ptr, 0, cchActualLengthRef - 1);
		}
	}

	public MetadataQueryReader(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator MetadataQueryReader(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new MetadataQueryReader(nativePointer);
		}
		return null;
	}

	internal unsafe void GetContainerFormat(out Guid guidContainerFormatRef)
	{
		guidContainerFormatRef = default(Guid);
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<Guid, IntPtr>(ref guidContainerFormatRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)3 * (nint)sizeof(void*))))(_nativePointer, ptr);
		}
		result.CheckError();
	}

	internal unsafe void GetLocation(int cchMaxLength, IntPtr @namespace, out int cchActualLengthRef)
	{
		Result result;
		fixed (IntPtr* ptr = &System.Runtime.CompilerServices.Unsafe.As<int, IntPtr>(ref cchActualLengthRef))
		{
			result = ((delegate* unmanaged[Stdcall]<void*, int, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, cchMaxLength, (void*)@namespace, ptr);
		}
		result.CheckError();
	}

	internal unsafe Result GetMetadataByName(string name, IntPtr varValueRef)
	{
		IntPtr intPtr = Utilities.StringToHGlobalUni(name);
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, (void*)intPtr, (void*)varValueRef);
		Marshal.FreeHGlobal(intPtr);
		return result;
	}

	internal unsafe IntPtr GetEnumerator()
	{
		IntPtr result = default(IntPtr);
		((Result)((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, &result)).CheckError();
		return result;
	}

	public unsafe Result TryGetMetadataByName(string name, out object value)
	{
		value = null;
		byte* ptr = stackalloc byte[512];
		Result metadataByName = GetMetadataByName(name, (IntPtr)ptr);
		if (metadataByName.Success)
		{
			Variant* ptr2 = (Variant*)ptr;
			value = ptr2->Value;
			if (value is ComObject comObject)
			{
				value = comObject.QueryInterfaceOrNull<MetadataQueryReader>();
			}
		}
		return metadataByName;
	}

	public object TryGetMetadataByName(string name)
	{
		object value;
		Result result = TryGetMetadataByName(name, out value);
		if (ResultCode.Propertynotfound != result && ResultCode.Propertynotsupported != result)
		{
			result.CheckError();
		}
		return value;
	}

	public object GetMetadataByName(string name)
	{
		TryGetMetadataByName(name, out var value).CheckError();
		return value;
	}

	public void Dump(TextWriter writer, int level = 0)
	{
		foreach (string item in Enumerator)
		{
			object metadataByName = GetMetadataByName(item);
			for (int i = 0; i < level; i++)
			{
				writer.Write("    ");
			}
			string arg = ((metadataByName is MetadataQueryReader) ? "..." : string.Concat((metadataByName is Array) ? Utilities.Join(",", ((Array)metadataByName).GetEnumerator()) : ((metadataByName is IntPtr) ? $"0x{metadataByName:X}" : metadataByName)));
			writer.WriteLine("{0} = {1}", item, arg);
			if (metadataByName is MetadataQueryReader)
			{
				((MetadataQueryReader)metadataByName).Dump(writer, level + 1);
			}
		}
	}
}
