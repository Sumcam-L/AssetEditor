using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Firaxis.CivTech.Packages;
using Firaxis.Error;
using Platform;
using Serialization;

namespace Firaxis.CivTech.AssetObjects;

public class ProjectConfig : IProjectConfig
{
	private ClassSet m_pmClasses = null;

	private ArtDefTemplateSet m_pmArtDefTemplates;

	private Firaxis.CivTech.Packages.XLPClassSet m_pmXLPClasses;

	private unsafe Serializer* m_pkSerializer;

	private unsafe global::AssetObjects.Deserializer* m_pkDeserializer;

	private unsafe global::AssetObjects.ProjectConfig* m_pkProjectConfig;

	public unsafe virtual Version Version
	{
		get
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out VersionInfo versionInfo);
			global::_003CModule_003E.AssetObjects_002EProjectConfig_002EGetVersion(m_pkProjectConfig, &versionInfo);
			return new Version(*(int*)(&versionInfo), System.Runtime.CompilerServices.Unsafe.As<VersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref versionInfo, 4)), System.Runtime.CompilerServices.Unsafe.As<VersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref versionInfo, 8)), System.Runtime.CompilerServices.Unsafe.As<VersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref versionInfo, 12)));
		}
	}

	public virtual IArtDefTemplateSet ArtDefTemplates => m_pmArtDefTemplates;

	public virtual IClassSet Classes => m_pmClasses;

	public virtual IXLPClassSet XLPClasses => m_pmXLPClasses;

	public unsafe virtual string Name
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EProjectConfig_002EGetName(m_pkProjectConfig));
			return Marshal.PtrToStringAnsi(ptr);
		}
		set
		{
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002EProjectConfig_002ESetName(m_pkProjectConfig, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe ProjectConfig()
	{
		//IL_0059: Expected I, but got I8
		//IL_0020: Expected I4, but got I8
		//IL_0031: Expected I, but got I8
		//IL_008d: Expected I, but got I8
		//IL_00bf: Expected I, but got I8
		Serializer* ptr = (Serializer*)global::_003CModule_003E.@new(320uL);
		Serializer* pkSerializer;
		try
		{
			if (ptr != null)
			{
				// IL initblk instruction
				System.Runtime.CompilerServices.Unsafe.InitBlock(ptr, 0, 320);
				global::_003CModule_003E.Serialization_002EContext_002E_007Bctor_007D((Context*)ptr);
				try
				{
					HeapAllocator_003C23_002C0_003E* ptr2 = (HeapAllocator_003C23_002C0_003E*)((ulong)(nint)ptr + 312uL);
					global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)ptr2);
					*(long*)ptr2 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), ptr);
					throw;
				}
				pkSerializer = ptr;
			}
			else
			{
				pkSerializer = null;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, 320uL);
			throw;
		}
		m_pkSerializer = pkSerializer;
		global::AssetObjects.Deserializer* ptr3 = (global::AssetObjects.Deserializer*)global::_003CModule_003E.@new(384uL);
		global::AssetObjects.Deserializer* pkDeserializer;
		try
		{
			pkDeserializer = ((ptr3 == null) ? null : global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bctor_007D(ptr3));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr3, 384uL);
			throw;
		}
		m_pkDeserializer = pkDeserializer;
		global::AssetObjects.ProjectConfig* ptr4 = (global::AssetObjects.ProjectConfig*)global::_003CModule_003E.@new(240uL);
		global::AssetObjects.ProjectConfig* pkProjectConfig;
		try
		{
			pkProjectConfig = ((ptr4 == null) ? null : global::_003CModule_003E.AssetObjects_002EProjectConfig_002E_007Bctor_007D(ptr4));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr4, 240uL);
			throw;
		}
		m_pkProjectConfig = pkProjectConfig;
		base._002Ector();
		m_pmClasses = new ClassSet(global::_003CModule_003E.AssetObjects_002EProjectConfig_002EGetClasses(m_pkProjectConfig), m_pkDeserializer);
		m_pmArtDefTemplates = new ArtDefTemplateSet(global::_003CModule_003E.AssetObjects_002EProjectConfig_002EGetArtDefTemplates(m_pkProjectConfig), m_pkDeserializer);
		m_pmXLPClasses = new Firaxis.CivTech.Packages.XLPClassSet(global::_003CModule_003E.AssetObjects_002EProjectConfig_002EGetXLPClasses(m_pkProjectConfig));
	}

	private void _007EProjectConfig()
	{
		RemoveReferences();
	}

	private void _0021ProjectConfig()
	{
		RemoveReferences();
	}

	public unsafe virtual void SetVersion(int major, int minor, int build, int revision)
	{
		global::_003CModule_003E.AssetObjects_002EProjectConfig_002ESetVersion(m_pkProjectConfig, major, minor, build, revision);
	}

	public unsafe virtual void SetVersion(string pmVersion)
	{
		Version result = null;
		if (Version.TryParse(pmVersion, out result))
		{
			global::_003CModule_003E.AssetObjects_002EProjectConfig_002ESetVersion(m_pkProjectConfig, result.Major, result.Minor, result.Build, result.Revision);
		}
	}

	public unsafe virtual string SerializeIntoXML()
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out HeapAllocator_003C23_002C0_003E heapAllocator_003C23_002C0_003E);
		global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)(&heapAllocator_003C23_002C0_003E));
		*(long*)(&heapAllocator_003C23_002C0_003E) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryOStream memoryOStream);
		global::_003CModule_003E.Serialization_002EMemoryOStream_002E_007Bctor_007D(&memoryOStream);
		string result;
		try
		{
			global::_003CModule_003E.Serialization_002EMemoryOStream_002EOpen(&memoryOStream, (Allocator*)(&heapAllocator_003C23_002C0_003E), 0uL);
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
			try
			{
				global::_003CModule_003E.AssetObjects_002ESerializer_002ESerialize_003Cclass_0020AssetObjects_003A_003AProjectConfig_003E(&serializer, m_pkProjectConfig, (OStream*)(&memoryOStream));
				global::_003CModule_003E.Serialization_002EMemoryOStream_002EWriteZeros(&memoryOStream, 1uL);
				System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
				global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer);
				try
				{
					global::_003CModule_003E.Serialization_002EMemoryOStream_002EClose(&memoryOStream, &memoryBuffer);
					IntPtr ptr = new IntPtr(global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer));
					result = Marshal.PtrToStringAnsi(ptr);
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
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Serializer*, void>)(&global::_003CModule_003E.AssetObjects_002ESerializer_002E_007Bdtor_007D), &serializer);
				throw;
			}
			global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&serializer));
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
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool DeserializeFromXML(string pmXML)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmXML).ToPointer();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
		bool flag = global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserialize_003Cclass_0020AssetObjects_003A_003AProjectConfig_003E(m_pkDeserializer, &resultCode, m_pkProjectConfig, ptr));
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		if (!flag)
		{
			return false;
		}
		m_pmClasses.RemoveReferences(bDisposing: false);
		m_pmClasses.AddReferences();
		m_pmArtDefTemplates.RemoveReferences(bDisposing: false);
		m_pmArtDefTemplates.AddReferences();
		m_pmXLPClasses.RemoveReferences(bDisposing: false);
		m_pmXLPClasses.AddReferences();
		return true;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool SerializeIntoFile(string pmFilename)
	{
		char* ptr = (char*)Marshal.StringToHGlobalUni(pmFilename).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeToFile_003Cclass_0020AssetObjects_003A_003AProjectConfig_003E(m_pkSerializer, m_pkProjectConfig, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	public unsafe virtual Firaxis.Error.ResultCode DeserializeFromFile(string pmFilename)
	{
		char* ptr = (char*)Marshal.StringToHGlobalUni(pmFilename).ToPointer();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
		bool flag = global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserializeFromFile_003Cclass_0020AssetObjects_003A_003AProjectConfig_003E(m_pkDeserializer, &resultCode, m_pkProjectConfig, ptr));
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		if (!flag)
		{
			return new Firaxis.Error.ResultCode("Failed to deserialize ProjectConfig from file \"{0}\"", pmFilename);
		}
		m_pmXLPClasses.RemoveReferences(bDisposing: false);
		m_pmXLPClasses.AddReferences();
		m_pmClasses.RemoveReferences(bDisposing: false);
		m_pmClasses.AddReferences();
		m_pmArtDefTemplates.RemoveReferences(bDisposing: false);
		m_pmArtDefTemplates.AddReferences();
		return Firaxis.Error.ResultCode.Success;
	}

	internal unsafe void RemoveReferences()
	{
		//IL_008b: Expected I, but got I8
		//IL_0093: Expected I, but got I8
		//IL_009b: Expected I, but got I8
		m_pmArtDefTemplates.RemoveReferences(bDisposing: true);
		m_pmClasses.RemoveReferences(bDisposing: true);
		m_pmXLPClasses.RemoveReferences(bDisposing: true);
		global::AssetObjects.ProjectConfig* pkProjectConfig = m_pkProjectConfig;
		if (pkProjectConfig != null)
		{
			global::_003CModule_003E.AssetObjects_002EProjectConfig_002E_007Bdtor_007D(pkProjectConfig);
			global::_003CModule_003E.delete(pkProjectConfig, 240uL);
		}
		Serializer* pkSerializer = m_pkSerializer;
		if (pkSerializer != null)
		{
			global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)pkSerializer);
			global::_003CModule_003E.delete(pkSerializer, 320uL);
		}
		global::AssetObjects.Deserializer* pkDeserializer = m_pkDeserializer;
		if (pkDeserializer != null)
		{
			global::_003CModule_003E.AssetObjects_002EDeserializer_002E__delDtor(pkDeserializer, 1u);
		}
		m_pmXLPClasses = null;
		m_pmArtDefTemplates = null;
		m_pmClasses = null;
		m_pkSerializer = null;
		m_pkDeserializer = null;
		m_pkProjectConfig = null;
	}

	internal unsafe global::AssetObjects.ProjectConfig* GetNativeConfig()
	{
		return m_pkProjectConfig;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			RemoveReferences();
			return;
		}
		try
		{
			RemoveReferences();
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

	~ProjectConfig()
	{
		Dispose(A_0: false);
	}
}
