using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Firaxis.Error;
using Platform;
using Serialization;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class GameArtSpecification : IGameArtSpecification
{
	private readonly GameArtSpecWrapper m_gameArtSpecification;

	private GameArtID m_pmGameArtID;

	private ICollection<IArtConsumer> m_ArtConsumers;

	private IDictionary<string, IGameLibrary> m_GameLibraries;

	private ICollection<IGameArtID> m_DependencyGameArtIDs;

	public unsafe virtual IEnumerable<IGameArtID> RequiredGameArtIDs
	{
		get
		{
			return m_DependencyGameArtIDs;
		}
		set
		{
			//IL_0023: Expected I, but got I8
			m_DependencyGameArtIDs = new List<IGameArtID>(value);
			global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AGameArtID_003E_002Eclear((Container_003CAssetObjects_003A_003AGameArtID_003E*)((ulong)(nint)m_gameArtSpecification.op_MemberSelection() + 184uL));
			foreach (IGameArtID item in value)
			{
				AddRequiredGameArtID(item.Name, item.ID);
			}
		}
	}

	public virtual IEnumerable<IGameLibrary> GameLibraries => m_GameLibraries.Values;

	public virtual IEnumerable<IArtConsumer> ArtConsumers => m_ArtConsumers;

	public virtual IGameArtID ID => m_pmGameArtID;

	public unsafe GameArtSpecification()
	{
		GameArtSpecWrapper gameArtSpecification = new GameArtSpecWrapper();
		try
		{
			m_gameArtSpecification = gameArtSpecification;
			m_ArtConsumers = new List<IArtConsumer>();
			m_GameLibraries = new Dictionary<string, IGameLibrary>();
			m_DependencyGameArtIDs = new List<IGameArtID>();
			base._002Ector();
			global::AssetObjects.GameArtID* pGameArtID = (global::AssetObjects.GameArtID*)m_gameArtSpecification.op_MemberSelection();
			m_pmGameArtID = new GameArtID(this, pGameArtID);
			return;
		}
		catch
		{
			//try-fault
			((IDisposable)m_gameArtSpecification).Dispose();
			throw;
		}
	}

	private void _007EGameArtSpecification()
	{
		m_ArtConsumers = null;
		m_GameLibraries = null;
		GC.SuppressFinalize(this);
	}

	private void _0021GameArtSpecification()
	{
		m_ArtConsumers = null;
		m_GameLibraries = null;
	}

	public unsafe virtual IArtConsumer AddConsumer(string consumerName)
	{
		StandardStringWrapper standardStringWrapper = null;
		foreach (IArtConsumer artConsumer in m_ArtConsumers)
		{
			if (artConsumer.ConsumerName == consumerName)
			{
				return artConsumer;
			}
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(consumerName);
		IArtConsumer result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			result = AddNewConsumer(standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool RemoveConsumer(string consumerName)
	{
		//IL_0025: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(consumerName);
		bool flag;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			flag = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0x424d5b7e_002ERemoveConsumerFromCollection((Container_003CAssetObjects_003A_003AArtConsumer_003E*)((ulong)(nint)m_gameArtSpecification.op_MemberSelection() + 56uL), standardStringWrapper.Value);
			if (flag)
			{
				ResolveReferences();
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

	public unsafe virtual void ClearConsumers()
	{
		//IL_0014: Expected I, but got I8
		global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AArtConsumer_003E_002Eclear((Container_003CAssetObjects_003A_003AArtConsumer_003E*)((ulong)(nint)m_gameArtSpecification.op_MemberSelection() + 56uL));
		ResolveReferences();
	}

	public unsafe virtual IGameLibrary AddGameLibrary(string libraryName)
	{
		//IL_002d: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (!m_GameLibraries.ContainsKey(libraryName))
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(libraryName);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::AssetObjects.GameLibrary* ptr = global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AGameLibrary_003E_002Epush_back((Container_003CAssetObjects_003A_003AGameLibrary_003E*)((ulong)(nint)m_gameArtSpecification.op_MemberSelection() + 120uL));
				global::_003CModule_003E.AssetObjects_002EString_002EAssign((global::AssetObjects.String*)ptr, standardStringWrapper.Value);
				GameLibrary value = new GameLibrary(this, ptr);
				m_GameLibraries[libraryName] = value;
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
		return m_GameLibraries[libraryName];
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool RemoveGameLibrary(string libraryName)
	{
		if (m_GameLibraries.ContainsKey(libraryName))
		{
			m_GameLibraries.Remove(libraryName);
			return global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0x424d5b7e_002ERemoveGameLibraryFromSpecification(m_gameArtSpecification.GameArtSpecification, libraryName);
		}
		return false;
	}

	public unsafe virtual void ClearGameLibraries()
	{
		//IL_001f: Expected I, but got I8
		m_GameLibraries.Clear();
		global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AGameLibrary_003E_002Eclear((Container_003CAssetObjects_003A_003AGameLibrary_003E*)((ulong)(nint)m_gameArtSpecification.op_MemberSelection() + 120uL));
	}

	public unsafe virtual IGameArtID AddRequiredGameArtID(string name, string id)
	{
		//IL_0035: Expected I, but got I8
		//IL_00b0: Expected I, but got I8
		//IL_00c9: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(name);
		IGameArtID gameArtID2;
		try
		{
			standardStringWrapper = standardStringWrapper3;
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(id);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0x424d5b7e_002ERemoveIDFromContainer((Container_003CAssetObjects_003A_003AGameArtID_003E*)((ulong)(nint)m_gameArtSpecification.op_MemberSelection() + 184uL), standardStringWrapper2.Value);
				System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.GameArtID gameArtID);
				global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D((global::AssetObjects.String*)(&gameArtID));
				try
				{
					global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D((global::AssetObjects.String*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gameArtID, 24)));
					try
					{
						System.Runtime.CompilerServices.Unsafe.As<global::AssetObjects.GameArtID, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gameArtID, 48)) = 0;
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.String*, void>)(&global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D), System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gameArtID, 24)));
						throw;
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.String*, void>)(&global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D), &gameArtID);
					throw;
				}
				try
				{
					global::_003CModule_003E.AssetObjects_002EString_002EAssign((global::AssetObjects.String*)(&gameArtID), standardStringWrapper.Value);
					global::_003CModule_003E.AssetObjects_002EString_002EAssign((global::AssetObjects.String*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gameArtID, 24)), standardStringWrapper2.Value);
					global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AGameArtID_003E_002Epush_back((Container_003CAssetObjects_003A_003AGameArtID_003E*)((ulong)(nint)m_gameArtSpecification.op_MemberSelection() + 184uL), global::_003CModule_003E.std_002Emove_003Cstruct_0020AssetObjects_003A_003AGameArtID_0020_0026_003E(&gameArtID));
					gameArtID2 = new GameArtID(this, global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AGameArtID_003E_002Eback((Container_003CAssetObjects_003A_003AGameArtID_003E*)((ulong)(nint)m_gameArtSpecification.op_MemberSelection() + 184uL)));
					m_DependencyGameArtIDs.Add(gameArtID2);
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.GameArtID*, void>)(&global::_003CModule_003E.AssetObjects_002EGameArtID_002E_007Bdtor_007D), &gameArtID);
					throw;
				}
				try
				{
					global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D((global::AssetObjects.String*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref gameArtID, 24)));
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.String*, void>)(&global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D), &gameArtID);
					throw;
				}
				global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D((global::AssetObjects.String*)(&gameArtID));
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
		return gameArtID2;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool RemoveRequiredGameArtID(string id)
	{
		//IL_0028: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(id);
		bool result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			result = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0x424d5b7e_002ERemoveIDFromContainer((Container_003CAssetObjects_003A_003AGameArtID_003E*)((ulong)(nint)m_gameArtSpecification.op_MemberSelection() + 184uL), standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
	}

	public unsafe virtual void ClearRequiredGameArtIDs()
	{
		//IL_0017: Expected I, but got I8
		global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AGameArtID_003E_002Eclear((Container_003CAssetObjects_003A_003AGameArtID_003E*)((ulong)(nint)m_gameArtSpecification.op_MemberSelection() + 184uL));
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
			System.Runtime.CompilerServices.Unsafe.SkipInit(out HeapAllocator_003C23_002C0_003E heapAllocator_003C23_002C0_003E);
			global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)(&heapAllocator_003C23_002C0_003E));
			*(long*)(&heapAllocator_003C23_002C0_003E) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryOStream memoryOStream);
			global::_003CModule_003E.Serialization_002EMemoryOStream_002E_007Bctor_007D(&memoryOStream);
			try
			{
				global::_003CModule_003E.Serialization_002EMemoryOStream_002EOpen(&memoryOStream, (Allocator*)(&heapAllocator_003C23_002C0_003E), 0uL);
				global::_003CModule_003E.AssetObjects_002ESerializer_002ESerialize_003Cstruct_0020AssetObjects_003A_003AGameArtSpecification_003E(&serializer, m_gameArtSpecification.GameArtSpecification, (OStream*)(&memoryOStream));
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
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Serializer*, void>)(&global::_003CModule_003E.AssetObjects_002ESerializer_002E_007Bdtor_007D), &serializer);
			throw;
		}
		global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&serializer));
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool DeserializeFromXML(string xml)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(xml);
		bool result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
			result = global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserialize_003Cstruct_0020AssetObjects_003A_003AGameArtSpecification_003E(m_gameArtSpecification.Deserializer, &resultCode, m_gameArtSpecification.GameArtSpecification, standardStringWrapper.Value));
			ResolveReferences();
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool SerializeIntoFile(string filePath)
	{
		IOStringWrapper iOStringWrapper = null;
		IOStringWrapper iOStringWrapper2 = new IOStringWrapper(filePath);
		bool result;
		try
		{
			iOStringWrapper = iOStringWrapper2;
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
				result = global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeToFile_003Cstruct_0020AssetObjects_003A_003AGameArtSpecification_003E(&serializer, m_gameArtSpecification.GameArtSpecification, iOStringWrapper.Value);
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
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		((IDisposable)iOStringWrapper).Dispose();
		return result;
	}

	public unsafe virtual Firaxis.Error.ResultCode DeserializeFromFile(string filePath)
	{
		IOStringWrapper iOStringWrapper = null;
		IOStringWrapper iOStringWrapper2 = new IOStringWrapper(filePath);
		Firaxis.Error.ResultCode result;
		try
		{
			iOStringWrapper = iOStringWrapper2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
			global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserializeFromFile_003Cstruct_0020AssetObjects_003A_003AGameArtSpecification_003E(m_gameArtSpecification.Deserializer, &resultCode, m_gameArtSpecification.GameArtSpecification, iOStringWrapper.Value);
			if (!global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(&resultCode))
			{
				result = new Firaxis.Error.ResultCode(new string(global::_003CModule_003E.Platform_002EResultCode_002EGetMessage(&resultCode)));
				goto IL_0055;
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
			ResolveReferences();
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
		IL_0055:
		((IDisposable)iOStringWrapper).Dispose();
		return result;
	}

	public override string ToString()
	{
		return $"{ID.Name} ({ID.ID})";
	}

	internal unsafe global::AssetObjects.ArtConsumer* FindArtConsumer(string consumerName)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(consumerName);
		global::AssetObjects.ArtConsumer* result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			sbyte* value = standardStringWrapper.Value;
			result = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0x424d5b7e_002EFindNativeArtConsumer(m_gameArtSpecification.GameArtSpecification, value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
	}

	internal unsafe global::AssetObjects.GameLibrary* FindGameLibrary(string libraryName)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(libraryName);
		global::AssetObjects.GameLibrary* result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			sbyte* value = standardStringWrapper.Value;
			result = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0x424d5b7e_002EFindNativeGameLibrary(m_gameArtSpecification.GameArtSpecification, value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
	}

	internal unsafe global::AssetObjects.GameArtID* FindGameIDPointer(string name, string guid)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(name);
		global::AssetObjects.GameArtID* result;
		try
		{
			standardStringWrapper = standardStringWrapper3;
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(guid);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				sbyte* value = standardStringWrapper.Value;
				sbyte* value2 = standardStringWrapper2.Value;
				result = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0x424d5b7e_002EFindNativeGameArtID(m_gameArtSpecification.GameArtSpecification, value, value2);
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
		return result;
	}

	private unsafe IArtConsumer AddNewConsumer(sbyte* consumerName)
	{
		//IL_0078: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.ArtConsumer artConsumer);
		global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D((global::AssetObjects.String*)(&artConsumer), consumerName);
		try
		{
			global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002E_007Bctor_007D((Container_003CAssetObjects_003A_003AString_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref artConsumer, 24)));
			try
			{
				global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002E_007Bctor_007D((Container_003CAssetObjects_003A_003AString_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref artConsumer, 88)));
				try
				{
					System.Runtime.CompilerServices.Unsafe.As<global::AssetObjects.ArtConsumer, sbyte>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref artConsumer, 152)) = 1;
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Container_003CAssetObjects_003A_003AString_003E*, void>)(&global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002E_007Bdtor_007D), System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref artConsumer, 88)));
					throw;
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Container_003CAssetObjects_003A_003AString_003E*, void>)(&global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002E_007Bdtor_007D), System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref artConsumer, 24)));
				throw;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.String*, void>)(&global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D), &artConsumer);
			throw;
		}
		global::AssetObjects.ArtConsumer* artConsumer2;
		try
		{
			artConsumer2 = global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AArtConsumer_003E_002Epush_back((Container_003CAssetObjects_003A_003AArtConsumer_003E*)((ulong)(nint)m_gameArtSpecification.op_MemberSelection() + 56uL), &artConsumer);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.ArtConsumer*, void>)(&global::_003CModule_003E.AssetObjects_002EArtConsumer_002E_007Bdtor_007D), &artConsumer);
			throw;
		}
		try
		{
			try
			{
				global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002E_007Bdtor_007D((Container_003CAssetObjects_003A_003AString_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref artConsumer, 88)));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Container_003CAssetObjects_003A_003AString_003E*, void>)(&global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002E_007Bdtor_007D), System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref artConsumer, 24)));
				throw;
			}
			global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002E_007Bdtor_007D((Container_003CAssetObjects_003A_003AString_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref artConsumer, 24)));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.String*, void>)(&global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D), &artConsumer);
			throw;
		}
		global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D((global::AssetObjects.String*)(&artConsumer));
		ArtConsumer artConsumer3 = new ArtConsumer(this, artConsumer2);
		m_ArtConsumers.Add(artConsumer3);
		return artConsumer3;
	}

	private unsafe void ResolveReferences()
	{
		//IL_0018: Expected I, but got I8
		//IL_0085: Expected I, but got I8
		//IL_00ff: Expected I, but got I8
		ICollection<IArtConsumer> collection = new List<IArtConsumer>();
		Container_003CAssetObjects_003A_003AArtConsumer_003E* ptr = (Container_003CAssetObjects_003A_003AArtConsumer_003E*)((ulong)(nint)m_gameArtSpecification.op_MemberSelection() + 56uL);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtConsumer_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AArtConsumer_003E_002Ebegin(ptr, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtConsumer_002C4096_003E.iterator iterator2);
		global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AArtConsumer_003E_002Eend(ptr, &iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtConsumer_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, &iterator2))
		{
			do
			{
				global::AssetObjects.ArtConsumer* artConsumer = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtConsumer_002C4096_003E_002Eiterator_002E_002A(&iterator);
				ArtConsumer item = new ArtConsumer(this, artConsumer);
				collection.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtConsumer_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtConsumer_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, &iterator2));
		}
		m_ArtConsumers = collection;
		IDictionary<string, IGameLibrary> dictionary = new Dictionary<string, IGameLibrary>();
		Container_003CAssetObjects_003A_003AGameLibrary_003E* ptr2 = (Container_003CAssetObjects_003A_003AGameLibrary_003E*)((ulong)(nint)m_gameArtSpecification.op_MemberSelection() + 120uL);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AGameLibrary_002C4096_003E.iterator iterator3);
		global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AGameLibrary_003E_002Ebegin(ptr2, &iterator3);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AGameLibrary_002C4096_003E.iterator iterator4);
		global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AGameLibrary_003E_002Eend(ptr2, &iterator4);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGameLibrary_002C4096_003E_002Eiterator_002E_0021_003D(&iterator3, &iterator4))
		{
			do
			{
				global::AssetObjects.GameLibrary* ptr3 = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGameLibrary_002C4096_003E_002Eiterator_002E_002A(&iterator3);
				string key = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str((global::AssetObjects.String*)ptr3));
				GameLibrary value = new GameLibrary(this, ptr3);
				dictionary[key] = value;
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGameLibrary_002C4096_003E_002Eiterator_002E_002B_002B(&iterator3);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGameLibrary_002C4096_003E_002Eiterator_002E_0021_003D(&iterator3, &iterator4));
		}
		m_GameLibraries = dictionary;
		ICollection<IGameArtID> collection2 = new List<IGameArtID>();
		Container_003CAssetObjects_003A_003AGameArtID_003E* ptr4 = (Container_003CAssetObjects_003A_003AGameArtID_003E*)((ulong)(nint)m_gameArtSpecification.op_MemberSelection() + 184uL);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AGameArtID_002C4096_003E.iterator iterator5);
		global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AGameArtID_003E_002Ebegin(ptr4, &iterator5);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AGameArtID_002C4096_003E.iterator iterator6);
		global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AGameArtID_003E_002Eend(ptr4, &iterator6);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGameArtID_002C4096_003E_002Eiterator_002E_0021_003D(&iterator5, &iterator6))
		{
			do
			{
				global::AssetObjects.GameArtID* pGameArtID = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGameArtID_002C4096_003E_002Eiterator_002E_002A(&iterator5);
				IGameArtID item2 = new GameArtID(this, pGameArtID);
				collection2.Add(item2);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGameArtID_002C4096_003E_002Eiterator_002E_002B_002B(&iterator5);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGameArtID_002C4096_003E_002Eiterator_002E_0021_003D(&iterator5, &iterator6));
		}
		m_DependencyGameArtIDs = collection2;
		m_pmGameArtID = new GameArtID(this, (global::AssetObjects.GameArtID*)m_gameArtSpecification.op_MemberSelection());
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			try
			{
				_007EGameArtSpecification();
				return;
			}
			finally
			{
				((IDisposable)m_gameArtSpecification).Dispose();
			}
		}
		try
		{
			_0021GameArtSpecification();
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

	~GameArtSpecification()
	{
		Dispose(A_0: false);
	}
}
