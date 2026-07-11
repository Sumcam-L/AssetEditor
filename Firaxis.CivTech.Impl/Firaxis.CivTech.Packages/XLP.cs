using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;
using Platform;
using Serialization;
using Types;

namespace Firaxis.CivTech.Packages;

public class XLP : IXLP
{
	private ISet<Firaxis.CivTech.AssetObjects.Platforms> m_allowedPlatforms = new HashSet<Firaxis.CivTech.AssetObjects.Platforms>();

	private IList<IXLPEntry> m_entryList = new List<IXLPEntry>();

	private string m_className = string.Empty;

	private string m_package = string.Empty;

	private Version m_version = new Version(1, 0, 0, 0);

	public virtual IEnumerable<Firaxis.CivTech.AssetObjects.Platforms> AllowedPlatforms => m_allowedPlatforms;

	public virtual Version Version => m_version;

	public virtual IList<IXLPEntry> XLPEntries => m_entryList;

	public virtual string Package
	{
		get
		{
			return m_package;
		}
		set
		{
			m_package = value;
		}
	}

	public virtual string ClassName
	{
		get
		{
			return m_className;
		}
		set
		{
			m_className = value;
		}
	}

	private void _007EXLP()
	{
		_0021XLP();
		GC.SuppressFinalize(this);
	}

	private void _0021XLP()
	{
		m_allowedPlatforms.Clear();
		m_entryList.Clear();
	}

	public virtual void SetVersion(int major, int minor, int build, int revision)
	{
		m_version = new Version(major, minor, build, revision);
	}

	public virtual void SetVersion(string pmVersion)
	{
		Version result = null;
		if (Version.TryParse(pmVersion, out result))
		{
			m_version = result;
		}
	}

	public virtual void RemoveEntry(string ID)
	{
		int num = 0;
		if (0 >= m_entryList.Count)
		{
			return;
		}
		while (!m_entryList[num].ID.Equals(ID))
		{
			num++;
			if (num >= m_entryList.Count)
			{
				return;
			}
		}
		m_entryList.RemoveAt(num);
	}

	public virtual IXLPEntry AddEntry(string ID, string objectName)
	{
		XLPEntry xLPEntry = new XLPEntry();
		xLPEntry.ID = ID;
		xLPEntry.ObjectName = objectName;
		m_entryList.Add(xLPEntry);
		return xLPEntry;
	}

	public virtual IXLPEntry FindEntry(string ID)
	{
		foreach (IXLPEntry entry in m_entryList)
		{
			if (entry.ID.Equals(ID))
			{
				return entry;
			}
		}
		return null;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool IsPlatformAllowed(Firaxis.CivTech.AssetObjects.Platforms ePlatform)
	{
		return m_allowedPlatforms.Contains(ePlatform);
	}

	public virtual void AllowPlatform(Firaxis.CivTech.AssetObjects.Platforms ePlatform)
	{
		m_allowedPlatforms.Add(ePlatform);
	}

	public virtual void ClearAllowedPlatforms()
	{
		m_allowedPlatforms.Clear();
	}

	public unsafe virtual string SerializeIntoXML()
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out Serializer serializer);
		global::_003CModule_003E.Serialization_002EContext_002E_007Bctor_007D((Context*)(&serializer));
		try
		{
			global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)));
			System.Runtime.CompilerServices.Unsafe.As<Serializer, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), &serializer);
			throw;
		}
		string result;
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.XLP xLP);
			global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bctor_007D(&xLP);
			try
			{
				ToNative(&xLP);
				System.Runtime.CompilerServices.Unsafe.SkipInit(out HeapAllocator_003C23_002C0_003E heapAllocator_003C23_002C0_003E);
				global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)(&heapAllocator_003C23_002C0_003E));
				*(long*)(&heapAllocator_003C23_002C0_003E) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
				System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryOStream memoryOStream);
				global::_003CModule_003E.Serialization_002EMemoryOStream_002E_007Bctor_007D(&memoryOStream);
				try
				{
					global::_003CModule_003E.Serialization_002EMemoryOStream_002EOpen(&memoryOStream, (Allocator*)(&heapAllocator_003C23_002C0_003E), 0uL);
					global::_003CModule_003E.AssetObjects_002ESerializer_002ESerialize_003Cclass_0020AssetObjects_003A_003AXLP_003E(&serializer, &xLP, (OStream*)(&memoryOStream));
					System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
					global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer);
					try
					{
						global::_003CModule_003E.Serialization_002EMemoryOStream_002EClose(&memoryOStream, &memoryBuffer);
						result = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString((sbyte*)global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer));
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
						throw;
					}
					global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryOStream*, void>)(&global::_003CModule_003E.Serialization_002EMemoryOStream_002E_007Bdtor_007D), &memoryOStream);
					throw;
				}
				try
				{
					global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D((MemoryBuffer*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref memoryOStream, 8)));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<OStream*, void>)(&global::_003CModule_003E.Serialization_002EOStream_002E_007Bdtor_007D), &memoryOStream);
					throw;
				}
				global::_003CModule_003E.Serialization_002EOStream_002E_007Bdtor_007D((OStream*)(&memoryOStream));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.XLP*, void>)(&global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bdtor_007D), &xLP);
				throw;
			}
			global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bdtor_007D(&xLP);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Serializer*, void>)(&global::_003CModule_003E.AssetObjects_002ESerializer_002E_007Bdtor_007D), &serializer);
			throw;
		}
		global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&serializer));
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool DeserializeFromXML(string xmlText)
	{
		StandardStringWrapper standardStringWrapper = null;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.Deserializer deserializer);
		global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bctor_007D(&deserializer);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.XLP xLP);
		try
		{
			global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bctor_007D(&xLP);
			try
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(xmlText);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
					if (global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserialize_003Cclass_0020AssetObjects_003A_003AXLP_003E(&deserializer, &resultCode, &xLP, standardStringWrapper.Value)))
					{
						FromNative(&xLP);
						goto IL_0049;
					}
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				goto end_IL_0012;
				IL_0049:
				((IDisposable)standardStringWrapper).Dispose();
				goto IL_005f;
				end_IL_0012:;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.XLP*, void>)(&global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bdtor_007D), &xLP);
				throw;
			}
			goto end_IL_000a;
			IL_005f:
			global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bdtor_007D(&xLP);
			goto IL_0077;
			end_IL_000a:;
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.Deserializer*, void>)(&global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bdtor_007D), &deserializer);
			throw;
		}
		try
		{
			try
			{
				((IDisposable)standardStringWrapper).Dispose();
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.XLP*, void>)(&global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bdtor_007D), &xLP);
				throw;
			}
			global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bdtor_007D(&xLP);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.Deserializer*, void>)(&global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bdtor_007D), &deserializer);
			throw;
		}
		try
		{
			try
			{
				global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E_002E_007Bdtor_007D((ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 352)));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E*, void>)(&global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E_002E_007Bdtor_007D), System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 312)));
				throw;
			}
			global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E_002E_007Bdtor_007D((ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 312)));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), &deserializer);
			throw;
		}
		global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&deserializer));
		return false;
		IL_0077:
		try
		{
			try
			{
				global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E_002E_007Bdtor_007D((ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 352)));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E*, void>)(&global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E_002E_007Bdtor_007D), System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 312)));
				throw;
			}
			global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E_002E_007Bdtor_007D((ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 312)));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), &deserializer);
			throw;
		}
		global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&deserializer));
		return true;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool SerializeIntoFile(string pmFilename)
	{
		IOStringWrapper iOStringWrapper = null;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out Serializer serializer);
		global::_003CModule_003E.Serialization_002EContext_002E_007Bctor_007D((Context*)(&serializer));
		try
		{
			global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)));
			System.Runtime.CompilerServices.Unsafe.As<Serializer, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), &serializer);
			throw;
		}
		bool result;
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.XLP xLP);
			global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bctor_007D(&xLP);
			try
			{
				ToNative(&xLP);
				IOStringWrapper iOStringWrapper2 = new IOStringWrapper(pmFilename);
				try
				{
					iOStringWrapper = iOStringWrapper2;
					result = global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeToFile_003Cclass_0020AssetObjects_003A_003AXLP_003E(&serializer, &xLP, iOStringWrapper.Value);
				}
				catch
				{
					//try-fault
					((IDisposable)iOStringWrapper).Dispose();
					throw;
				}
				((IDisposable)iOStringWrapper).Dispose();
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.XLP*, void>)(&global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bdtor_007D), &xLP);
				throw;
			}
			global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bdtor_007D(&xLP);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Serializer*, void>)(&global::_003CModule_003E.AssetObjects_002ESerializer_002E_007Bdtor_007D), &serializer);
			throw;
		}
		global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&serializer));
		return result;
	}

	public unsafe virtual Firaxis.Error.ResultCode DeserializeFromFile(string pmFilename)
	{
		IOStringWrapper iOStringWrapper = null;
		IOStringWrapper iOStringWrapper2 = new IOStringWrapper(pmFilename);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.Deserializer deserializer);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.XLP xLP);
		Firaxis.Error.ResultCode result;
		try
		{
			iOStringWrapper = iOStringWrapper2;
			global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bctor_007D(&deserializer);
			try
			{
				global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bctor_007D(&xLP);
				try
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
					global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserializeFromFile_003Cclass_0020AssetObjects_003A_003AXLP_003E(&deserializer, &resultCode, &xLP, iOStringWrapper.Value);
					if (!global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(&resultCode))
					{
						result = new Firaxis.Error.ResultCode(new string(global::_003CModule_003E.Platform_002EResultCode_002EGetMessage(&resultCode)));
						goto IL_005d;
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.XLP*, void>)(&global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bdtor_007D), &xLP);
					throw;
				}
				goto end_IL_0013;
				IL_005d:
				global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bdtor_007D(&xLP);
				goto IL_0075;
				end_IL_0013:;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.Deserializer*, void>)(&global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bdtor_007D), &deserializer);
				throw;
			}
			goto end_IL_0009;
			IL_0075:
			try
			{
				try
				{
					global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E_002E_007Bdtor_007D((ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 352)));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E*, void>)(&global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E_002E_007Bdtor_007D), System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 312)));
					throw;
				}
				global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E_002E_007Bdtor_007D((ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 312)));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), &deserializer);
				throw;
			}
			global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&deserializer));
			goto IL_00c6;
			end_IL_0009:;
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		Firaxis.Error.ResultCode success;
		try
		{
			try
			{
				try
				{
					FromNative(&xLP);
					success = Firaxis.Error.ResultCode.Success;
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.XLP*, void>)(&global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bdtor_007D), &xLP);
					throw;
				}
				global::_003CModule_003E.AssetObjects_002EXLP_002E_007Bdtor_007D(&xLP);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.Deserializer*, void>)(&global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bdtor_007D), &deserializer);
				throw;
			}
			try
			{
				try
				{
					global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E_002E_007Bdtor_007D((ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 352)));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E*, void>)(&global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E_002E_007Bdtor_007D), System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 312)));
					throw;
				}
				global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E_002E_007Bdtor_007D((ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C2_003E_002C8192_002C16_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref deserializer, 312)));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), &deserializer);
				throw;
			}
			global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&deserializer));
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		((IDisposable)iOStringWrapper).Dispose();
		return success;
		IL_00c6:
		((IDisposable)iOStringWrapper).Dispose();
		return result;
	}

	public override string ToString()
	{
		return $"Class Name: {ClassName}, Package: {Package}";
	}

	private unsafe void ToNative(global::AssetObjects.XLP* xlp)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(Package);
		try
		{
			standardStringWrapper = standardStringWrapper3;
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(ClassName);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				global::_003CModule_003E.AssetObjects_002EXLP_002ESetPackageName(xlp, standardStringWrapper.Value);
				global::_003CModule_003E.AssetObjects_002EXLP_002ESetClassName(xlp, standardStringWrapper2.Value);
				foreach (Firaxis.CivTech.AssetObjects.Platforms allowedPlatform in m_allowedPlatforms)
				{
					global::_003CModule_003E.AssetObjects_002EXLP_002EAllowPlatform(xlp, allowedPlatform switch
					{
						Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_LINUX => (global::AssetObjects.Platforms)8u, 
						Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_MACOS => (global::AssetObjects.Platforms)4u, 
						Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_IOS => (global::AssetObjects.Platforms)2u, 
						Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_WINDOWS => (global::AssetObjects.Platforms)1u, 
						Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_INVALID => (global::AssetObjects.Platforms)0u, 
						Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_XBONE => (global::AssetObjects.Platforms)16u, 
						Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_ALL => (global::AssetObjects.Platforms)255u, 
						Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_STADIA => (global::AssetObjects.Platforms)128u, 
						Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_PS4 => (global::AssetObjects.Platforms)64u, 
						Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_SWITCH => (global::AssetObjects.Platforms)32u, 
						_ => (global::AssetObjects.Platforms)0u, 
					});
				}
				global::_003CModule_003E.AssetObjects_002EXLP_002ESetVersion(xlp, m_version.Major, m_version.Minor, m_version.Build, m_version.Revision);
				foreach (XLPEntry entry in m_entryList)
				{
					entry.ToNative(xlp);
				}
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper2).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper2).Dispose();
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	private unsafe void FromNative(global::AssetObjects.XLP* xlp)
	{
		m_entryList.Clear();
		m_allowedPlatforms.Clear();
		Package = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EXLP_002EGetPackageName(xlp));
		ClassName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EXLP_002EGetClassName(xlp));
		System.Runtime.CompilerServices.Unsafe.SkipInit(out VersionInfo versionInfo);
		global::_003CModule_003E.AssetObjects_002EXLP_002EGetVersion(xlp, &versionInfo);
		m_version = new Version(*(int*)(&versionInfo), System.Runtime.CompilerServices.Unsafe.As<VersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref versionInfo, 4)), System.Runtime.CompilerServices.Unsafe.As<VersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref versionInfo, 8)), System.Runtime.CompilerServices.Unsafe.As<VersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref versionInfo, 12)));
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003Cenum_0020AssetObjects_003A_003APlatforms_002C4096_003E.const_iterator const_iterator);
		global::_003CModule_003E.AssetObjects_002EXLP_002Eplatforms_begin(xlp, &const_iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003Cenum_0020AssetObjects_003A_003APlatforms_002C4096_003E.const_iterator const_iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003Cenum_0020AssetObjects_003A_003APlatforms_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EXLP_002Eplatforms_end(xlp, &const_iterator2)))
		{
			do
			{
				uint num = (uint)(*global::_003CModule_003E.Types_002EChunkedVector_003Cenum_0020AssetObjects_003A_003APlatforms_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator));
				m_allowedPlatforms.Add(num switch
				{
					8u => Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_LINUX, 
					4u => Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_MACOS, 
					2u => Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_IOS, 
					1u => Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_WINDOWS, 
					0u => Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_INVALID, 
					16u => Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_XBONE, 
					255u => Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_ALL, 
					128u => Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_STADIA, 
					64u => Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_PS4, 
					32u => Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_SWITCH, 
					_ => Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_INVALID, 
				});
				global::_003CModule_003E.Types_002EChunkedVector_003Cenum_0020AssetObjects_003A_003APlatforms_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003Cenum_0020AssetObjects_003A_003APlatforms_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EXLP_002Eplatforms_end(xlp, &const_iterator2)));
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AXLPEntry_002C4096_003E.const_iterator const_iterator3);
		global::_003CModule_003E.AssetObjects_002EXLP_002Ebegin(xlp, &const_iterator3);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AXLPEntry_002C4096_003E.const_iterator const_iterator4);
		global::_003CModule_003E.AssetObjects_002EXLP_002Eend(xlp, &const_iterator4);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AXLPEntry_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator3, &const_iterator4))
		{
			do
			{
				global::AssetObjects.XLPEntry* entry = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AXLPEntry_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator3);
				XLPEntry xLPEntry = new XLPEntry();
				xLPEntry.FromNative(entry);
				m_entryList.Add(xLPEntry);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AXLPEntry_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator3);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AXLPEntry_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator3, &const_iterator4));
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EXLP();
			return;
		}
		try
		{
			_0021XLP();
		}
		finally
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~XLP()
	{
		Dispose(A_0: false);
	}
}
