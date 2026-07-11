using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using AssetObjects;
using Firaxis.Error;
using Firaxis.Threading;
using Firaxis.Utility;
using Platform;
using Serialization;
using std;
using String;

namespace Firaxis.CivTech.AssetObjects;

public class InstanceSet : IInstanceSet
{
	private ReaderWriterLockSlim m_pmLock;

	private List<IInstanceEntity> m_pmInstanceEntities;

	private IList<IDictionary<string, IInstanceEntity>> m_pmEntityNameMap;

	private IDictionary<string, IInstanceEntity> m_pmEntityPathMap;

	private InstanceEntityFactory m_pmEntityFactory;

	private unsafe global::AssetObjects.InstanceSet* m_pkInstanceSet;

	private unsafe global::AssetObjects.Deserializer* m_pkDeserializer;

	private unsafe Serializer* m_pkSerializer;

	private unsafe global::AssetObjects.VirtualPantry* m_pkVirtualPantry;

	internal unsafe global::AssetObjects.InstanceSet* UnmanagedPtr => m_pkInstanceSet;

	public virtual IEnumerable<IInstanceEntity> Items
	{
		get
		{
			ScopedReaderLock scopedReaderLock = null;
			ScopedReaderLock scopedReaderLock2 = new ScopedReaderLock(m_pmLock);
			IEnumerable<IInstanceEntity> result;
			try
			{
				scopedReaderLock = scopedReaderLock2;
				result = m_pmInstanceEntities.ToArray();
			}
			catch
			{
				//try-fault
				((IDisposable)scopedReaderLock).Dispose();
				throw;
			}
			((IDisposable)scopedReaderLock).Dispose();
			return result;
		}
	}

	public unsafe InstanceSet(IEnumerable<string> pantryPaths)
	{
		//IL_005c: Expected I, but got I8
		//IL_004c: Expected I4, but got I8
		//IL_0090: Expected I, but got I8
		//IL_00fa: Expected I, but got I8
		//IL_00c1: Expected I4, but got I8
		//IL_00d2: Expected I, but got I8
		//IL_014b: Expected I, but got I8
		//IL_013b: Expected I4, but got I8
		ScopedWriterLock scopedWriterLock = null;
		IOStringWrapper iOStringWrapper = null;
		m_pmLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		m_pmInstanceEntities = new List<IInstanceEntity>();
		m_pmEntityNameMap = new List<IDictionary<string, IInstanceEntity>>();
		m_pmEntityPathMap = new Dictionary<string, IInstanceEntity>(new PathComparer(bIgnoreCase: true));
		global::AssetObjects.InstanceSet* ptr = (global::AssetObjects.InstanceSet*)global::_003CModule_003E.@new(64uL);
		global::AssetObjects.InstanceSet* pkInstanceSet;
		try
		{
			if (ptr != null)
			{
				// IL initblk instruction
				System.Runtime.CompilerServices.Unsafe.InitBlock(ptr, 0, 64);
				global::_003CModule_003E.AssetObjects_002EPolymorphicContainer_003CAssetObjects_003A_003AInstanceEntity_003E_002E_007Bctor_007D((PolymorphicContainer_003CAssetObjects_003A_003AInstanceEntity_003E*)ptr);
				pkInstanceSet = ptr;
			}
			else
			{
				pkInstanceSet = null;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, 64uL);
			throw;
		}
		m_pkInstanceSet = pkInstanceSet;
		global::AssetObjects.Deserializer* ptr2 = (global::AssetObjects.Deserializer*)global::_003CModule_003E.@new(384uL);
		global::AssetObjects.Deserializer* pkDeserializer;
		try
		{
			pkDeserializer = ((ptr2 == null) ? null : global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bctor_007D(ptr2));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr2, 384uL);
			throw;
		}
		m_pkDeserializer = pkDeserializer;
		Serializer* ptr3 = (Serializer*)global::_003CModule_003E.@new(320uL);
		Serializer* pkSerializer;
		try
		{
			if (ptr3 != null)
			{
				// IL initblk instruction
				System.Runtime.CompilerServices.Unsafe.InitBlock(ptr3, 0, 320);
				global::_003CModule_003E.Serialization_002EContext_002E_007Bctor_007D((Serialization.Context*)ptr3);
				try
				{
					HeapAllocator_003C23_002C0_003E* ptr4 = (HeapAllocator_003C23_002C0_003E*)((ulong)(nint)ptr3 + 312uL);
					global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)ptr4);
					*(long*)ptr4 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Serialization.Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), ptr3);
					throw;
				}
				pkSerializer = ptr3;
			}
			else
			{
				pkSerializer = null;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr3, 320uL);
			throw;
		}
		m_pkSerializer = pkSerializer;
		base._002Ector();
		ScopedWriterLock scopedWriterLock2 = new ScopedWriterLock(m_pmLock);
		try
		{
			scopedWriterLock = scopedWriterLock2;
			global::AssetObjects.VirtualPantry* ptr5 = (global::AssetObjects.VirtualPantry*)global::_003CModule_003E.@new(24uL);
			global::AssetObjects.VirtualPantry* pkVirtualPantry;
			try
			{
				if (ptr5 != null)
				{
					// IL initblk instruction
					System.Runtime.CompilerServices.Unsafe.InitBlock(ptr5, 0, 24);
					global::_003CModule_003E.std_002Evector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002Cstd_003A_003Aallocator_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_0020_003E_0020_003E_002E_007Bctor_007D((vector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002Cstd_003A_003Aallocator_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_0020_003E_0020_003E*)ptr5);
					pkVirtualPantry = ptr5;
				}
				else
				{
					pkVirtualPantry = null;
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr5, 24uL);
				throw;
			}
			m_pkVirtualPantry = pkVirtualPantry;
			uint num = 13u;
			do
			{
				m_pmEntityNameMap.Add(new Dictionary<string, IInstanceEntity>(new PathComparer(bIgnoreCase: true)));
				num += uint.MaxValue;
			}
			while (num != 0);
			foreach (string pantryPath in pantryPaths)
			{
				IOStringWrapper iOStringWrapper2 = new IOStringWrapper(pantryPath);
				try
				{
					iOStringWrapper = iOStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EAddPantryRoot(m_pkVirtualPantry, iOStringWrapper.Value);
				}
				catch
				{
					//try-fault
					((IDisposable)iOStringWrapper).Dispose();
					throw;
				}
				((IDisposable)iOStringWrapper).Dispose();
			}
			m_pmEntityFactory = new InstanceEntityFactory(m_pkInstanceSet, m_pkDeserializer, m_pkSerializer, m_pkVirtualPantry);
		}
		catch
		{
			//try-fault
			((IDisposable)scopedWriterLock).Dispose();
			throw;
		}
		((IDisposable)scopedWriterLock).Dispose();
	}

	private void _007EInstanceSet()
	{
		RemoveReferences();
	}

	private void _0021InstanceSet()
	{
		RemoveReferences();
	}

	public unsafe virtual T LoadByName<T>(string pmEntityName) where T : IInstanceEntity
	{
		StandardStringWrapper standardStringWrapper = null;
		ScopedWriterLock scopedWriterLock = null;
		IInstanceEntity instanceEntity = null;
		if (typeof(T) == typeof(IInstanceEntity))
		{
			return default(T);
		}
		T val = FindByNameTyped<T>(pmEntityName);
		if (val != null)
		{
			return val;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmEntityName);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
		T result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			try
			{
				global::AssetObjects.InstanceType instanceType = (global::AssetObjects.InstanceType)StaticMethods.InstanceTypeFromEntityType<T>();
				global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EGetPantryPath(m_pkVirtualPantry, standardStringWrapper.Value, instanceType, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				if (global::_003CModule_003E.String_002EBase_003C2_003E_002EIsEmpty((Base_003C2_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E)))
				{
					result = default(T);
					goto IL_0091;
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				throw;
			}
			goto end_IL_0047;
			IL_0091:
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			goto IL_00a1;
			end_IL_0047:;
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		T result2;
		try
		{
			try
			{
				ScopedWriterLock scopedWriterLock2 = new ScopedWriterLock(m_pmLock);
				try
				{
					scopedWriterLock = scopedWriterLock2;
					val = CreateNamedEntity<T>(pmEntityName);
					if (val == null)
					{
						result2 = default(T);
						goto IL_00de;
					}
				}
				catch
				{
					//try-fault
					((IDisposable)scopedWriterLock).Dispose();
					throw;
				}
				goto end_IL_00ab;
				IL_00de:
				((IDisposable)scopedWriterLock).Dispose();
				goto IL_00f4;
				end_IL_00ab:;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				throw;
			}
			goto end_IL_00ab_2;
			IL_00f4:
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			goto IL_0104;
			end_IL_00ab_2:;
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		T result3;
		try
		{
			try
			{
				try
				{
					string text = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E)));
					Firaxis.Error.ResultCode resultCode = val.DeserializeFromFile(text);
					if (!resultCode)
					{
						object[] fmtParams = new object[2] { text, resultCode.Message };
						byte condition = ((!File.Exists(text)) ? ((byte)1) : ((byte)0));
						BugSubmitter.SilentAssert(condition != 0, "Failed to deserialize from existent file \"{0}\". Error={1} @summary InstanceSet::LoadByName failed to deserialize from file @assign bwhitman", fmtParams);
						result3 = default(T);
						goto IL_0178;
					}
				}
				catch
				{
					//try-fault
					((IDisposable)scopedWriterLock).Dispose();
					throw;
				}
				goto end_IL_010e;
				IL_0178:
				((IDisposable)scopedWriterLock).Dispose();
				goto IL_018e;
				end_IL_010e:;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				throw;
			}
			goto end_IL_010e_2;
			IL_018e:
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			goto IL_019e;
			end_IL_010e_2:;
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		T result4;
		try
		{
			try
			{
				try
				{
					instanceEntity = val;
					LocklessAddToContainers(&instanceEntity);
					val = (T)instanceEntity;
					result4 = val;
				}
				catch
				{
					//try-fault
					((IDisposable)scopedWriterLock).Dispose();
					throw;
				}
				((IDisposable)scopedWriterLock).Dispose();
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				throw;
			}
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result4;
		IL_00a1:
		((IDisposable)standardStringWrapper).Dispose();
		return result;
		IL_019e:
		((IDisposable)standardStringWrapper).Dispose();
		return result3;
		IL_0104:
		((IDisposable)standardStringWrapper).Dispose();
		return result2;
	}

	public unsafe virtual T LoadByPath<T>(string pmEntityPath) where T : IInstanceEntity
	{
		ScopedWriterLock scopedWriterLock = null;
		IInstanceEntity instanceEntity = null;
		if (typeof(T) == typeof(IInstanceEntity))
		{
			return default(T);
		}
		T val = FindByPathTyped<T>(pmEntityPath);
		if (val != null)
		{
			return val;
		}
		val = CreateEntity<T>();
		if (val == null)
		{
			return default(T);
		}
		ScopedWriterLock scopedWriterLock2 = new ScopedWriterLock(m_pmLock);
		T result;
		try
		{
			scopedWriterLock = scopedWriterLock2;
			Firaxis.Error.ResultCode resultCode = val.DeserializeFromFile(pmEntityPath);
			if (!resultCode)
			{
				object[] fmtParams = new object[2] { pmEntityPath, resultCode.Message };
				byte condition = ((!File.Exists(pmEntityPath)) ? ((byte)1) : ((byte)0));
				BugSubmitter.SilentAssert(condition != 0, "Failed to deserialize from existent file \"{0}\". Error={1} @summary InstanceSet::LoadByPath failed to deserialize from file @assign bwhitman", fmtParams);
				result = default(T);
				goto IL_00c4;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)scopedWriterLock).Dispose();
			throw;
		}
		string fileNameWithoutExtension;
		T result2;
		try
		{
			fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pmEntityPath);
			if (string.IsNullOrEmpty(fileNameWithoutExtension))
			{
				object[] fmtParams2 = new object[1] { pmEntityPath };
				byte condition2 = ((!File.Exists(pmEntityPath)) ? ((byte)1) : ((byte)0));
				BugSubmitter.SilentAssert(condition2 != 0, "Got null or empty string for filename from path \"{0}\" @summary InstanceSet::LoadByPath failed to verify filename matches entity name @assign bwhitman", fmtParams2);
				result2 = default(T);
				goto IL_0116;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)scopedWriterLock).Dispose();
			throw;
		}
		T result3;
		try
		{
			if (!PathCompareHelper.EndsWith(val.Name, fileNameWithoutExtension, bIgnoreCase: true))
			{
				object[] fmtParams3 = new object[3] { fileNameWithoutExtension, val.Name, pmEntityPath };
				byte condition3 = ((!File.Exists(pmEntityPath)) ? ((byte)1) : ((byte)0));
				BugSubmitter.SilentAssert(condition3 != 0, "Filename \"{0}\" did not match entity name \"{1}\" while processing path \"{2}\" @summary InstanceSet::LoadByPath failed because filename did not match entity name @assign bwhitman", fmtParams3);
				result3 = default(T);
				goto IL_017c;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)scopedWriterLock).Dispose();
			throw;
		}
		T result4;
		try
		{
			instanceEntity = val;
			LocklessAddToContainers(&instanceEntity);
			val = (T)instanceEntity;
			result4 = val;
		}
		catch
		{
			//try-fault
			((IDisposable)scopedWriterLock).Dispose();
			throw;
		}
		((IDisposable)scopedWriterLock).Dispose();
		return result4;
		IL_00c4:
		((IDisposable)scopedWriterLock).Dispose();
		return result;
		IL_017c:
		((IDisposable)scopedWriterLock).Dispose();
		return result3;
		IL_0116:
		((IDisposable)scopedWriterLock).Dispose();
		return result2;
	}

	public unsafe virtual T Push<T>(string pmEntityName) where T : IInstanceEntity
	{
		ScopedUpgrableReaderLock scopedUpgrableReaderLock = null;
		ScopedWriterLock scopedWriterLock = null;
		IInstanceEntity instanceEntity = null;
		if (typeof(T) == typeof(IInstanceEntity))
		{
			return default(T);
		}
		InstanceType instanceType = StaticMethods.InstanceTypeFromEntityType<T>();
		if (instanceType >= InstanceType.IT_INVALID)
		{
			return default(T);
		}
		ScopedUpgrableReaderLock scopedUpgrableReaderLock2 = new ScopedUpgrableReaderLock(m_pmLock);
		T result;
		try
		{
			scopedUpgrableReaderLock = scopedUpgrableReaderLock2;
			if (m_pmEntityNameMap[(int)instanceType].ContainsKey(pmEntityName))
			{
				BugSubmitter.SilentAssert(false, "Attempted to add duplicate entity \"{0}\" of type {1} to instance set @summary Attempted to add duplicate entity instance set @assign bwhitman", pmEntityName, instanceType);
				result = default(T);
				goto IL_009b;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)scopedUpgrableReaderLock).Dispose();
			throw;
		}
		T val;
		try
		{
			ScopedWriterLock scopedWriterLock2 = new ScopedWriterLock(scopedUpgrableReaderLock);
			try
			{
				scopedWriterLock = scopedWriterLock2;
				val = CreateNamedEntity<T>(pmEntityName);
				instanceEntity = val;
				LocklessAddToContainers(&instanceEntity);
				val = (T)instanceEntity;
			}
			catch
			{
				//try-fault
				((IDisposable)scopedWriterLock).Dispose();
				throw;
			}
			((IDisposable)scopedWriterLock).Dispose();
		}
		catch
		{
			//try-fault
			((IDisposable)scopedUpgrableReaderLock).Dispose();
			throw;
		}
		((IDisposable)scopedUpgrableReaderLock).Dispose();
		return val;
		IL_009b:
		((IDisposable)scopedUpgrableReaderLock).Dispose();
		return result;
	}

	public unsafe virtual void Clear()
	{
		ScopedWriterLock scopedWriterLock = null;
		ScopedWriterLock scopedWriterLock2 = new ScopedWriterLock(m_pmLock);
		try
		{
			scopedWriterLock = scopedWriterLock2;
			List<IInstanceEntity>.Enumerator enumerator = m_pmInstanceEntities.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					((InstanceEntity)enumerator.Current).RemoveReferences(bDisposing: true);
				}
				while (enumerator.MoveNext());
			}
			global::_003CModule_003E.AssetObjects_002EInstanceSet_002Eclear(m_pkInstanceSet);
			m_pmInstanceEntities.Clear();
			foreach (IDictionary<string, IInstanceEntity> item in m_pmEntityNameMap)
			{
				item.Clear();
			}
			m_pmEntityPathMap.Clear();
		}
		catch
		{
			//try-fault
			((IDisposable)scopedWriterLock).Dispose();
			throw;
		}
		((IDisposable)scopedWriterLock).Dispose();
	}

	public virtual void Remove(IEnumerable<EntityID> entityIDs)
	{
		ScopedWriterLock scopedWriterLock = null;
		if (entityIDs.Any())
		{
			ScopedWriterLock scopedWriterLock2 = new ScopedWriterLock(m_pmLock);
			try
			{
				scopedWriterLock = scopedWriterLock2;
				LocklessRemove(entityIDs);
			}
			catch
			{
				//try-fault
				((IDisposable)scopedWriterLock).Dispose();
				throw;
			}
			((IDisposable)scopedWriterLock).Dispose();
		}
	}

	public virtual void Remove(IInstanceEntity pmInstanceEntity)
	{
		ScopedUpgrableReaderLock scopedUpgrableReaderLock = null;
		ScopedWriterLock scopedWriterLock = null;
		ScopedUpgrableReaderLock scopedUpgrableReaderLock2 = new ScopedUpgrableReaderLock(m_pmLock);
		try
		{
			scopedUpgrableReaderLock = scopedUpgrableReaderLock2;
			if (m_pmInstanceEntities.IndexOf(pmInstanceEntity) >= 0)
			{
				goto IL_0035;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)scopedUpgrableReaderLock).Dispose();
			throw;
		}
		((IDisposable)scopedUpgrableReaderLock).Dispose();
		return;
		IL_0035:
		try
		{
			ScopedWriterLock scopedWriterLock2 = new ScopedWriterLock(scopedUpgrableReaderLock);
			try
			{
				scopedWriterLock = scopedWriterLock2;
				LocklessRemove(pmInstanceEntity);
				LocklessFixup();
			}
			catch
			{
				//try-fault
				((IDisposable)scopedWriterLock).Dispose();
				throw;
			}
			((IDisposable)scopedWriterLock).Dispose();
		}
		catch
		{
			//try-fault
			((IDisposable)scopedUpgrableReaderLock).Dispose();
			throw;
		}
		((IDisposable)scopedUpgrableReaderLock).Dispose();
	}

	public virtual void RemoveExcept(IEnumerable<EntityID> entityIDs)
	{
		ScopedUpgrableReaderLock scopedUpgrableReaderLock = null;
		ScopedWriterLock scopedWriterLock = null;
		List<EntityID> list = new List<EntityID>();
		ScopedUpgrableReaderLock scopedUpgrableReaderLock2 = new ScopedUpgrableReaderLock(m_pmLock);
		try
		{
			scopedUpgrableReaderLock = scopedUpgrableReaderLock2;
			List<IInstanceEntity>.Enumerator enumerator = m_pmInstanceEntities.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					EntityID entityID = new EntityID(enumerator.Current);
					if (!entityIDs.Contains(entityID))
					{
						list.Add(entityID);
					}
				}
				while (enumerator.MoveNext());
			}
			ScopedWriterLock scopedWriterLock2 = new ScopedWriterLock(scopedUpgrableReaderLock);
			try
			{
				scopedWriterLock = scopedWriterLock2;
				LocklessRemove(list);
				list.Clear();
			}
			catch
			{
				//try-fault
				((IDisposable)scopedWriterLock).Dispose();
				throw;
			}
			((IDisposable)scopedWriterLock).Dispose();
		}
		catch
		{
			//try-fault
			((IDisposable)scopedUpgrableReaderLock).Dispose();
			throw;
		}
		((IDisposable)scopedUpgrableReaderLock).Dispose();
	}

	public virtual IInstanceEntity FindByNameAndType(string pmInstanceName, InstanceType eType)
	{
		ScopedReaderLock scopedReaderLock = null;
		if (eType != InstanceType.IT_COUNT && eType != InstanceType.IT_INVALID)
		{
			ScopedReaderLock scopedReaderLock2 = new ScopedReaderLock(m_pmLock);
			IInstanceEntity result;
			try
			{
				scopedReaderLock = scopedReaderLock2;
				result = LocklessFindByNameAndType(pmInstanceName, eType);
			}
			catch
			{
				//try-fault
				((IDisposable)scopedReaderLock).Dispose();
				throw;
			}
			((IDisposable)scopedReaderLock).Dispose();
			return result;
		}
		return null;
	}

	public virtual IInstanceEntity FindByPath(string pmEntityPath)
	{
		ScopedReaderLock scopedReaderLock = null;
		ScopedReaderLock scopedReaderLock2 = new ScopedReaderLock(m_pmLock);
		IInstanceEntity result;
		try
		{
			scopedReaderLock = scopedReaderLock2;
			result = LocklessFindByPath(pmEntityPath);
		}
		catch
		{
			//try-fault
			((IDisposable)scopedReaderLock).Dispose();
			throw;
		}
		((IDisposable)scopedReaderLock).Dispose();
		return result;
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0039: Expected I, but got I8
		//IL_005d: Expected I, but got I8
		//IL_0077: Expected I, but got I8
		ScopedWriterLock scopedWriterLock = null;
		Clear();
		ScopedWriterLock scopedWriterLock2 = new ScopedWriterLock(m_pmLock);
		try
		{
			scopedWriterLock = scopedWriterLock2;
			global::AssetObjects.InstanceSet* pkInstanceSet = m_pkInstanceSet;
			if (pkInstanceSet != null)
			{
				global::_003CModule_003E.AssetObjects_002EPolymorphicContainer_003CAssetObjects_003A_003AInstanceEntity_003E_002E_007Bdtor_007D((PolymorphicContainer_003CAssetObjects_003A_003AInstanceEntity_003E*)pkInstanceSet);
				global::_003CModule_003E.delete(pkInstanceSet, 64uL);
			}
			m_pkInstanceSet = null;
			Serializer* pkSerializer = m_pkSerializer;
			if (pkSerializer != null)
			{
				global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Serialization.Context*)pkSerializer);
				global::_003CModule_003E.delete(pkSerializer, 320uL);
			}
			m_pkSerializer = null;
			global::AssetObjects.Deserializer* pkDeserializer = m_pkDeserializer;
			if (pkDeserializer != null)
			{
				global::_003CModule_003E.AssetObjects_002EDeserializer_002E__delDtor(pkDeserializer, 1u);
			}
			m_pkDeserializer = null;
		}
		catch
		{
			//try-fault
			((IDisposable)scopedWriterLock).Dispose();
			throw;
		}
		((IDisposable)scopedWriterLock).Dispose();
	}

	private int FindEntityIndex(EntityID entityID)
	{
		ScopedReaderLock scopedReaderLock = null;
		IInstanceEntity instanceEntity = null;
		InstanceType type = entityID.Type;
		if (type != InstanceType.IT_COUNT && type != InstanceType.IT_INVALID)
		{
			ScopedReaderLock scopedReaderLock2 = new ScopedReaderLock(m_pmLock);
			try
			{
				scopedReaderLock = scopedReaderLock2;
				instanceEntity = null;
				if (m_pmEntityNameMap[(int)entityID.Type].TryGetValue(entityID.Name, out instanceEntity))
				{
					goto IL_005e;
				}
			}
			catch
			{
				//try-fault
				((IDisposable)scopedReaderLock).Dispose();
				throw;
			}
			((IDisposable)scopedReaderLock).Dispose();
			return -1;
		}
		return -1;
		IL_005e:
		int result;
		try
		{
			result = m_pmInstanceEntities.IndexOf(instanceEntity);
		}
		catch
		{
			//try-fault
			((IDisposable)scopedReaderLock).Dispose();
			throw;
		}
		((IDisposable)scopedReaderLock).Dispose();
		return result;
	}

	private T FindByNameTyped<T>(string pmInstanceName) where T : IInstanceEntity
	{
		InstanceType eType = StaticMethods.InstanceTypeFromEntityType<T>();
		return (T)FindByNameAndType(pmInstanceName, eType);
	}

	private T FindByPathTyped<T>(string pmEntityPath) where T : IInstanceEntity
	{
		ScopedReaderLock scopedReaderLock = null;
		IInstanceEntity instanceEntity = null;
		StaticMethods.InstanceTypeFromEntityType<T>();
		ScopedReaderLock scopedReaderLock2 = new ScopedReaderLock(m_pmLock);
		T result;
		try
		{
			scopedReaderLock = scopedReaderLock2;
			instanceEntity = null;
			if (m_pmEntityPathMap.TryGetValue(pmEntityPath, out instanceEntity))
			{
				result = (T)instanceEntity;
				goto IL_003e;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)scopedReaderLock).Dispose();
			throw;
		}
		T result2;
		try
		{
			result2 = default(T);
		}
		catch
		{
			//try-fault
			((IDisposable)scopedReaderLock).Dispose();
			throw;
		}
		((IDisposable)scopedReaderLock).Dispose();
		return result2;
		IL_003e:
		((IDisposable)scopedReaderLock).Dispose();
		return result;
	}

	private unsafe T CreateNamedEntity<T>(string pmEntityName) where T : IInstanceEntity
	{
		//IL_003e: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F_0024CreateNamedEntity_0040_0024VMAAABAAB_0040_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAM_003FAV_0024VMAAABAAB_0040_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && !(typeof(T) != typeof(IInstanceEntity)) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CF_0040PDDANBMO_0040T_003F3_003F3typeid_003F5_003F_0024CB_003F_0024DN_003F5IInstanceEntity_003F3_003F3ty_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 301u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F_0024CreateNamedEntity_0040_0024VMAAABAAB_0040_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAM_003FAV_0024VMAAABAAB_0040_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		T val = CreateEntity<T>();
		if (val != null)
		{
			val.Name = pmEntityName;
		}
		return val;
	}

	private unsafe T CreateEntity<T>() where T : IInstanceEntity
	{
		//IL_003e: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F_0024CreateEntity_0040_0024VMAAABAAB_0040_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAM_003FAV_0024VMAAABAAB_0040_0040XZ_00404_NA && !(typeof(T) != typeof(IInstanceEntity)) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CF_0040PDDANBMO_0040T_003F3_003F3typeid_003F5_003F_0024CB_003F_0024DN_003F5IInstanceEntity_003F3_003F3ty_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 315u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F_0024CreateEntity_0040_0024VMAAABAAB_0040_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAM_003FAV_0024VMAAABAAB_0040_0040XZ_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		return (T)m_pmEntityFactory.CreateEntity<T>();
	}

	private unsafe global::AssetObjects.InstanceEntity* DestroyEntity(InstanceEntity pmTypedInstanceEntity)
	{
		global::AssetObjects.InstanceEntity* assetObject = pmTypedInstanceEntity.GetAssetObject();
		pmTypedInstanceEntity.RemoveReferences(bDisposing: true);
		return assetObject;
	}

	private unsafe IInstanceEntity LocklessFindByNameAndType(string pmInstanceName, InstanceType eType)
	{
		//IL_0056: Expected I, but got I8
		IInstanceEntity instanceEntity = null;
		if (eType != InstanceType.IT_COUNT && eType != InstanceType.IT_INVALID)
		{
			if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F4_003F_003FLocklessFindByNameAndType_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPE_0024AAUIInstanceEntity_0040345_0040PE_0024AAVString_0040System_0040_0040W4InstanceType_0040345_0040_0040Z_00404_NA && !m_pmLock.IsReadLockHeld && !m_pmLock.IsUpgradeableReadLockHeld && !m_pmLock.IsWriteLockHeld && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FN_0040FDLDPJC_0040m_pmLock_003F9_003F_0024DOIsReadLockHeld_003F5_003F_0024HM_003F_0024HM_003F5m_pm_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 474u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F4_003F_003FLocklessFindByNameAndType_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPE_0024AAUIInstanceEntity_0040345_0040PE_0024AAVString_0040System_0040_0040W4InstanceType_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			instanceEntity = null;
			m_pmEntityNameMap[(int)eType].TryGetValue(pmInstanceName, out instanceEntity);
			return instanceEntity;
		}
		return null;
	}

	private unsafe IInstanceEntity LocklessFindByPath(string pmEntityPath)
	{
		//IL_004c: Expected I, but got I8
		IInstanceEntity instanceEntity = null;
		if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F2_003F_003FLocklessFindByPath_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPE_0024AAUIInstanceEntity_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && !m_pmLock.IsReadLockHeld && !m_pmLock.IsUpgradeableReadLockHeld && !m_pmLock.IsWriteLockHeld && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FN_0040FDLDPJC_0040m_pmLock_003F9_003F_0024DOIsReadLockHeld_003F5_003F_0024HM_003F_0024HM_003F5m_pm_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 491u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F2_003F_003FLocklessFindByPath_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPE_0024AAUIInstanceEntity_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		instanceEntity = null;
		m_pmEntityPathMap.TryGetValue(pmEntityPath, out instanceEntity);
		return instanceEntity;
	}

	private unsafe void LocklessAddToContainers(IInstanceEntity* pmEntity)
	{
		//IL_0050: Expected I, but got I8
		//IL_0084: Expected I, but got I8
		//IL_00cc: Expected I, but got I8
		IInstanceEntity instanceEntity = *pmEntity;
		if (instanceEntity == null)
		{
			return;
		}
		int type = (int)instanceEntity.Type;
		string xMLPath = pmEntity->GetXMLPath();
		string name = pmEntity->Name;
		if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F4_003F_003FLocklessAddToContainers_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXAEAPE_0024AAUIInstanceEntity_0040345_0040_0040Z_00404_NA && !m_pmLock.IsWriteLockHeld && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BK_0040FDNLOMNA_0040m_pmLock_003F9_003F_0024DOIsWriteLockHeld_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 261u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F4_003F_003FLocklessAddToContainers_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXAEAPE_0024AAUIInstanceEntity_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FLocklessAddToContainers_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXAEAPE_0024AAUIInstanceEntity_0040345_0040_0040Z_00404_NA && m_pmEntityPathMap.ContainsKey(xMLPath) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CM_0040FBEBBKID_0040_003F_0024CBm_pmEntityPathMap_003F9_003F_0024DOContainsKey_003F_0024CI_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 262u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FLocklessAddToContainers_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXAEAPE_0024AAUIInstanceEntity_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (m_pmEntityNameMap[type].ContainsKey(name))
		{
			if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003FBG_0040_003F_003FLocklessAddToContainers_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXAEAPE_0024AAUIInstanceEntity_0040345_0040_0040Z_00404_NA && m_pmEntityPathMap.ContainsKey(xMLPath) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CM_0040FBEBBKID_0040_003F_0024CBm_pmEntityPathMap_003F9_003F_0024DOContainsKey_003F_0024CI_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 267u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003FBG_0040_003F_003FLocklessAddToContainers_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXAEAPE_0024AAUIInstanceEntity_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			System.Runtime.CompilerServices.Unsafe.Write(pmEntity, m_pmEntityNameMap[type][name]);
		}
		else
		{
			m_pmInstanceEntities.Add(*pmEntity);
			m_pmEntityNameMap[type].Add(name, *pmEntity);
			m_pmEntityPathMap[xMLPath] = *pmEntity;
		}
	}

	private unsafe void LocklessRemoveFromContainers<T>(T pmEntity) where T : IInstanceEntity
	{
		//IL_0061: Expected I, but got I8
		//IL_009b: Expected I, but got I8
		//IL_00cf: Expected I, but got I8
		if (pmEntity != null)
		{
			int type = (int)pmEntity.Type;
			string xMLPath = pmEntity.GetXMLPath();
			string name = pmEntity.Name;
			if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F4_003F_003F_003F_0024LocklessRemoveFromContainers_0040_0024VMAAABAAB_0040_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXV_0024VMAAABAAB_0040_0040_0040Z_00404_NA && !m_pmLock.IsWriteLockHeld && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BK_0040FDNLOMNA_0040m_pmLock_003F9_003F_0024DOIsWriteLockHeld_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 288u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F4_003F_003F_003F_0024LocklessRemoveFromContainers_0040_0024VMAAABAAB_0040_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXV_0024VMAAABAAB_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003F_003F_0024LocklessRemoveFromContainers_0040_0024VMAAABAAB_0040_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXV_0024VMAAABAAB_0040_0040_0040Z_00404_NA && !m_pmEntityNameMap[type].ContainsKey(name) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DB_0040COPBNIOH_0040m_pmEntityNameMap_003F_0024FLinsType_003F_0024FN_003F9_003F_0024DOCont_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 289u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003F_003F_0024LocklessRemoveFromContainers_0040_0024VMAAABAAB_0040_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXV_0024VMAAABAAB_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003FBD_0040_003F_003F_003F_0024LocklessRemoveFromContainers_0040_0024VMAAABAAB_0040_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXV_0024VMAAABAAB_0040_0040_0040Z_00404_NA && !m_pmEntityPathMap.ContainsKey(xMLPath) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CL_0040EFJEPAEL_0040m_pmEntityPathMap_003F9_003F_0024DOContainsKey_003F_0024CIi_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 290u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003FBD_0040_003F_003F_003F_0024LocklessRemoveFromContainers_0040_0024VMAAABAAB_0040_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXV_0024VMAAABAAB_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			m_pmEntityPathMap.Remove(xMLPath);
			m_pmEntityNameMap[type].Remove(name);
			m_pmInstanceEntities.Remove(pmEntity);
		}
	}

	private unsafe void LocklessRemove(IInstanceEntity pmEntity)
	{
		//IL_0026: Expected I, but got I8
		//IL_0059: Expected I, but got I8
		//IL_0089: Expected I, but got I8
		//IL_00c7: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F2_003F_003FLocklessRemove_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXPE_0024AAUIInstanceEntity_0040345_0040_0040Z_00404_NA && pmEntity == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BB_0040NJDHLFFB_0040pmInstanceEntity_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 366u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F2_003F_003FLocklessRemove_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXPE_0024AAUIInstanceEntity_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F9_003F_003FLocklessRemove_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXPE_0024AAUIInstanceEntity_0040345_0040_0040Z_00404_NA && !m_pmLock.IsWriteLockHeld && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BK_0040FDNLOMNA_0040m_pmLock_003F9_003F_0024DOIsWriteLockHeld_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 367u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F9_003F_003FLocklessRemove_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXPE_0024AAUIInstanceEntity_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		InstanceEntity instanceEntity = (InstanceEntity)pmEntity;
		if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003FBB_0040_003F_003FLocklessRemove_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXPE_0024AAUIInstanceEntity_0040345_0040_0040Z_00404_NA && instanceEntity == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0P_0040LPGELHDP_0040pmCastedEntity_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 370u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003FBB_0040_003F_003FLocklessRemove_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXPE_0024AAUIInstanceEntity_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		LocklessRemoveFromContainers(instanceEntity);
		global::AssetObjects.InstanceEntity* assetObject = instanceEntity.GetAssetObject();
		instanceEntity.RemoveReferences(bDisposing: true);
		if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003FBI_0040_003F_003FLocklessRemove_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXPE_0024AAUIInstanceEntity_0040345_0040_0040Z_00404_NA && assetObject == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BB_0040IKKGNMLF_0040pkInstanceEntity_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 375u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003FBI_0040_003F_003FLocklessRemove_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXPE_0024AAUIInstanceEntity_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		global::_003CModule_003E.AssetObjects_002EInstanceSet_002ERemove(m_pkInstanceSet, assetObject);
	}

	private void LocklessRemove(IEnumerable<EntityID> entityIDs)
	{
		if (!entityIDs.Any())
		{
			return;
		}
		foreach (EntityID entityID in entityIDs)
		{
			IInstanceEntity instanceEntity = LocklessFindByNameAndType(entityID.Name, entityID.Type);
			if (instanceEntity != null)
			{
				LocklessRemove(instanceEntity);
			}
		}
		LocklessFixup();
	}

	private unsafe void LocklessFixup()
	{
		//IL_0030: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F2_003F_003FLocklessFixup_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA && !m_pmLock.IsWriteLockHeld && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BK_0040FDNLOMNA_0040m_pmLock_003F9_003F_0024DOIsWriteLockHeld_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HH_0040HMGKFNJD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 382u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee620231_002E_003FbIgnoreAlways_0040_003F2_003F_003FLocklessFixup_0040InstanceSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		List<IInstanceEntity>.Enumerator enumerator = m_pmInstanceEntities.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((InstanceEntity)enumerator.Current).ResolveNativePointer(m_pkInstanceSet);
			}
			while (enumerator.MoveNext());
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EInstanceSet();
			return;
		}
		try
		{
			_0021InstanceSet();
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

	~InstanceSet()
	{
		Dispose(A_0: false);
	}
}
