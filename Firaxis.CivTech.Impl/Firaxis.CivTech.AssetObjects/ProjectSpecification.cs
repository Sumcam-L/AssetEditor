using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Firaxis.Error;
using Platform;
using Serialization;
using String;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ProjectSpecification : IProjectSpecification
{
	private IDictionary<string, ProjectSpecificationDependency> m_dependencies = new Dictionary<string, ProjectSpecificationDependency>();

	private unsafe global::AssetObjects.Deserializer* m_deserializer;

	private unsafe global::AssetObjects.ProjectSpecification* m_projectSpecification;

	public virtual IDictionary<string, ProjectSpecificationDependency> Dependencies => m_dependencies;

	public unsafe virtual string ArtDefOutputRoot
	{
		get
		{
			//IL_000f: Expected I, but got I8
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)((ulong)(nint)m_projectSpecification + 96uL)));
		}
		set
		{
			//IL_0022: Expected I, but got I8
			IOStringWrapper iOStringWrapper = null;
			IOStringWrapper iOStringWrapper2 = new IOStringWrapper(value);
			try
			{
				iOStringWrapper = iOStringWrapper2;
				char* value2 = iOStringWrapper.Value;
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002EAssign((BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*)((ulong)(nint)m_projectSpecification + 96uL), value2);
			}
			catch
			{
				//try-fault
				((IDisposable)iOStringWrapper).Dispose();
				throw;
			}
			((IDisposable)iOStringWrapper).Dispose();
		}
	}

	public unsafe virtual string ArtDefRoot
	{
		get
		{
			//IL_000f: Expected I, but got I8
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)((ulong)(nint)m_projectSpecification + 88uL)));
		}
		set
		{
			//IL_0022: Expected I, but got I8
			IOStringWrapper iOStringWrapper = null;
			IOStringWrapper iOStringWrapper2 = new IOStringWrapper(value);
			try
			{
				iOStringWrapper = iOStringWrapper2;
				char* value2 = iOStringWrapper.Value;
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002EAssign((BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*)((ulong)(nint)m_projectSpecification + 88uL), value2);
			}
			catch
			{
				//try-fault
				((IDisposable)iOStringWrapper).Dispose();
				throw;
			}
			((IDisposable)iOStringWrapper).Dispose();
		}
	}

	public unsafe virtual string LooseAssetRoot
	{
		get
		{
			//IL_000f: Expected I, but got I8
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)((ulong)(nint)m_projectSpecification + 80uL)));
		}
		set
		{
			//IL_0022: Expected I, but got I8
			IOStringWrapper iOStringWrapper = null;
			IOStringWrapper iOStringWrapper2 = new IOStringWrapper(value);
			try
			{
				iOStringWrapper = iOStringWrapper2;
				char* value2 = iOStringWrapper.Value;
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002EAssign((BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*)((ulong)(nint)m_projectSpecification + 80uL), value2);
			}
			catch
			{
				//try-fault
				((IDisposable)iOStringWrapper).Dispose();
				throw;
			}
			((IDisposable)iOStringWrapper).Dispose();
		}
	}

	public unsafe virtual string CloudStream
	{
		get
		{
			//IL_000f: Expected I, but got I8
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)((ulong)(nint)m_projectSpecification + 104uL)));
		}
		set
		{
			//IL_0022: Expected I, but got I8
			IOStringWrapper iOStringWrapper = null;
			IOStringWrapper iOStringWrapper2 = new IOStringWrapper(value);
			try
			{
				iOStringWrapper = iOStringWrapper2;
				char* value2 = iOStringWrapper.Value;
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002EAssign((BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*)((ulong)(nint)m_projectSpecification + 104uL), value2);
			}
			catch
			{
				//try-fault
				((IDisposable)iOStringWrapper).Dispose();
				throw;
			}
			((IDisposable)iOStringWrapper).Dispose();
		}
	}

	public unsafe virtual string GamePantry
	{
		get
		{
			//IL_000f: Expected I, but got I8
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)((ulong)(nint)m_projectSpecification + 72uL)));
		}
		set
		{
			//IL_0022: Expected I, but got I8
			IOStringWrapper iOStringWrapper = null;
			IOStringWrapper iOStringWrapper2 = new IOStringWrapper(value);
			try
			{
				iOStringWrapper = iOStringWrapper2;
				char* value2 = iOStringWrapper.Value;
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002EAssign((BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*)((ulong)(nint)m_projectSpecification + 72uL), value2);
			}
			catch
			{
				//try-fault
				((IDisposable)iOStringWrapper).Dispose();
				throw;
			}
			((IDisposable)iOStringWrapper).Dispose();
		}
	}

	public unsafe virtual string ArtDev
	{
		get
		{
			//IL_000f: Expected I, but got I8
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)((ulong)(nint)m_projectSpecification + 64uL)));
		}
		set
		{
			//IL_0022: Expected I, but got I8
			IOStringWrapper iOStringWrapper = null;
			IOStringWrapper iOStringWrapper2 = new IOStringWrapper(value);
			try
			{
				iOStringWrapper = iOStringWrapper2;
				char* value2 = iOStringWrapper.Value;
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002EAssign((BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*)((ulong)(nint)m_projectSpecification + 64uL), value2);
			}
			catch
			{
				//try-fault
				((IDisposable)iOStringWrapper).Dispose();
				throw;
			}
			((IDisposable)iOStringWrapper).Dispose();
		}
	}

	public unsafe virtual string ProjectConfig
	{
		get
		{
			//IL_000f: Expected I, but got I8
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)((ulong)(nint)m_projectSpecification + 56uL)));
		}
		set
		{
			//IL_0022: Expected I, but got I8
			IOStringWrapper iOStringWrapper = null;
			IOStringWrapper iOStringWrapper2 = new IOStringWrapper(value);
			try
			{
				iOStringWrapper = iOStringWrapper2;
				char* value2 = iOStringWrapper.Value;
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002EAssign((BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*)((ulong)(nint)m_projectSpecification + 56uL), value2);
			}
			catch
			{
				//try-fault
				((IDisposable)iOStringWrapper).Dispose();
				throw;
			}
			((IDisposable)iOStringWrapper).Dispose();
		}
	}

	public unsafe virtual string GUID
	{
		get
		{
			//IL_000f: Expected I, but got I8
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str((global::AssetObjects.String*)((ulong)(nint)m_projectSpecification + 24uL)));
		}
		set
		{
			//IL_0022: Expected I, but got I8
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				sbyte* value2 = standardStringWrapper.Value;
				global::_003CModule_003E.AssetObjects_002EString_002EAssign((global::AssetObjects.String*)((ulong)(nint)m_projectSpecification + 24uL), value2);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
	}

	public unsafe virtual string Name
	{
		get
		{
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str((global::AssetObjects.String*)m_projectSpecification));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				sbyte* value2 = standardStringWrapper.Value;
				global::_003CModule_003E.AssetObjects_002EString_002EAssign((global::AssetObjects.String*)m_projectSpecification, value2);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
	}

	public unsafe ProjectSpecification()
	{
		//IL_0026: Expected I, but got I8
		//IL_0061: Expected I, but got I8
		//IL_0055: Expected I4, but got I8
		global::AssetObjects.Deserializer* ptr = (global::AssetObjects.Deserializer*)global::_003CModule_003E.@new(384uL);
		global::AssetObjects.Deserializer* deserializer;
		try
		{
			deserializer = ((ptr == null) ? null : global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bctor_007D(ptr));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, 384uL);
			throw;
		}
		m_deserializer = deserializer;
		global::AssetObjects.ProjectSpecification* ptr2 = (global::AssetObjects.ProjectSpecification*)global::_003CModule_003E.@new(176uL);
		global::AssetObjects.ProjectSpecification* projectSpecification;
		try
		{
			if (ptr2 != null)
			{
				// IL initblk instruction
				System.Runtime.CompilerServices.Unsafe.InitBlock(ptr2, 0, 176);
				projectSpecification = global::_003CModule_003E.AssetObjects_002EProjectSpecification_002E_007Bctor_007D(ptr2);
			}
			else
			{
				projectSpecification = null;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr2, 176uL);
			throw;
		}
		m_projectSpecification = projectSpecification;
		base._002Ector();
	}

	private void _007EProjectSpecification()
	{
		RemoveReferences();
	}

	private void _0021ProjectSpecification()
	{
		RemoveReferences();
	}

	public unsafe virtual string SerializeIntoXML()
	{
		UpdateUnmanagedDependencies();
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
				global::_003CModule_003E.AssetObjects_002ESerializer_002ESerialize_003Cclass_0020AssetObjects_003A_003AProjectSpecification_003E(&serializer, m_projectSpecification, (OStream*)(&memoryOStream));
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
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmXML);
		bool flag;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
			flag = global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserialize_003Cclass_0020AssetObjects_003A_003AProjectSpecification_003E(m_deserializer, &resultCode, m_projectSpecification, standardStringWrapper.Value));
			if (flag)
			{
				UpdateManagedDependencies();
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return flag;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool SerializeIntoFile(string pmFilename)
	{
		IOStringWrapper iOStringWrapper = null;
		UpdateUnmanagedDependencies();
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
			IOStringWrapper iOStringWrapper2 = new IOStringWrapper(pmFilename);
			try
			{
				iOStringWrapper = iOStringWrapper2;
				result = global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeToFile_003Cclass_0020AssetObjects_003A_003AProjectSpecification_003E(&serializer, m_projectSpecification, iOStringWrapper.Value);
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
		Firaxis.Error.ResultCode result;
		try
		{
			iOStringWrapper = iOStringWrapper2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
			global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserializeFromFile_003Cclass_0020AssetObjects_003A_003AProjectSpecification_003E(m_deserializer, &resultCode, m_projectSpecification, iOStringWrapper.Value);
			if (!global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(&resultCode))
			{
				result = new Firaxis.Error.ResultCode(new string(global::_003CModule_003E.Platform_002EResultCode_002EGetMessage(&resultCode)));
				goto IL_004b;
			}
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
			UpdateManagedDependencies();
			success = Firaxis.Error.ResultCode.Success;
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		((IDisposable)iOStringWrapper).Dispose();
		return success;
		IL_004b:
		((IDisposable)iOStringWrapper).Dispose();
		return result;
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0036: Expected I, but got I8
		//IL_003e: Expected I, but got I8
		global::AssetObjects.ProjectSpecification* projectSpecification = m_projectSpecification;
		if (projectSpecification != null)
		{
			global::_003CModule_003E.AssetObjects_002EProjectSpecification_002E_007Bdtor_007D(projectSpecification);
			global::_003CModule_003E.delete(projectSpecification, 176uL);
		}
		global::AssetObjects.Deserializer* deserializer = m_deserializer;
		if (deserializer != null)
		{
			global::_003CModule_003E.AssetObjects_002EDeserializer_002E__delDtor(deserializer, 1u);
		}
		m_deserializer = null;
		m_projectSpecification = null;
		m_dependencies.Clear();
	}

	private unsafe void UpdateManagedDependencies()
	{
		//IL_001c: Expected I, but got I8
		//IL_002e: Expected I, but got I8
		//IL_004d: Expected I, but got I8
		//IL_0088: Expected I, but got I8
		//IL_00b8: Expected I, but got I8
		m_dependencies.Clear();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ASpecificationDependency_002C4096_003E.const_iterator const_iterator);
		global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003ASpecificationDependency_003E_002Ecbegin((Container_003CAssetObjects_003A_003ASpecificationDependency_003E*)((ulong)(nint)m_projectSpecification + 112uL), &const_iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ASpecificationDependency_002C4096_003E.const_iterator const_iterator2);
		global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003ASpecificationDependency_003E_002Ecend((Container_003CAssetObjects_003A_003ASpecificationDependency_003E*)((ulong)(nint)m_projectSpecification + 112uL), &const_iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASpecificationDependency_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, &const_iterator2))
		{
			do
			{
				string text = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str((global::AssetObjects.String*)((ulong)(nint)global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASpecificationDependency_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator) + 24uL)));
				ProjectSpecificationDependency projectSpecificationDependency = new ProjectSpecificationDependency();
				projectSpecificationDependency.Name = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str((global::AssetObjects.String*)global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASpecificationDependency_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
				projectSpecificationDependency.GUID = text;
				projectSpecificationDependency.Path = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)((ulong)(nint)global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASpecificationDependency_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator) + 56uL)));
				m_dependencies[text] = projectSpecificationDependency;
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASpecificationDependency_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003ASpecificationDependency_003E_002Ecend((Container_003CAssetObjects_003A_003ASpecificationDependency_003E*)((ulong)(nint)m_projectSpecification + 112uL), &const_iterator2);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ASpecificationDependency_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, &const_iterator2));
		}
	}

	private unsafe void UpdateUnmanagedDependencies()
	{
		//IL_0015: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		IOStringWrapper iOStringWrapper = null;
		global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003ASpecificationDependency_003E_002Eclear((Container_003CAssetObjects_003A_003ASpecificationDependency_003E*)((ulong)(nint)m_projectSpecification + 112uL));
		foreach (ProjectSpecificationDependency value in m_dependencies.Values)
		{
			StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(value.Name);
			try
			{
				standardStringWrapper = standardStringWrapper3;
				StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(value.GUID);
				try
				{
					standardStringWrapper2 = standardStringWrapper4;
					IOStringWrapper iOStringWrapper2 = new IOStringWrapper(value.Path);
					try
					{
						iOStringWrapper = iOStringWrapper2;
						global::_003CModule_003E.AssetObjects_002EProjectSpecification_002EAddProjectDependency(m_projectSpecification, standardStringWrapper.Value, standardStringWrapper2.Value, iOStringWrapper.Value);
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

	~ProjectSpecification()
	{
		Dispose(A_0: false);
	}
}
