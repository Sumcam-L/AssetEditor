using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class EntityChangeList : IEntityChangeList, IChangedEntityProvider
{
	private List<IEntityChangedEvent> m_entityChangeEvents = new List<IEntityChangedEvent>();

	private SortedSet<EntityID> m_changedEntities = new SortedSet<EntityID>();

	private bool m_sendImmediately = false;

	private bool m_sendAlone = false;

	public virtual IEnumerable<IEntityChangedEvent> EntityChanges => m_entityChangeEvents;

	private void _007EEntityChangeList()
	{
		Clear();
	}

	private void _0021EntityChangeList()
	{
		Clear();
	}

	public virtual IEnumerable<EntityID> GetChangedEntities()
	{
		return m_changedEntities;
	}

	public virtual T Push<T>(EntityID entityID) where T : IEntityChangedEvent
	{
		T val = ChangeEventFactory.CreateChangeEvent<T>();
		byte condition = ((val != null) ? ((byte)1) : ((byte)0));
		BugSubmitter.SilentAssert(condition != 0, $"Failed to create a change event of type '{typeof(T).ToString()}'.  @assign bwhitman");
		if (val == null)
		{
			return val;
		}
		EntityChangedEvent entityChangedEvent = (EntityChangedEvent)(object)val;
		byte condition2 = ((entityChangedEvent != null) ? ((byte)1) : ((byte)0));
		BugSubmitter.SilentAssert(condition2 != 0, $"safe_cast<EntityChangedEvent^> failed for event of type '{typeof(T).ToString()}'.  @assign bwhitman");
		if (entityChangedEvent == null)
		{
			return val;
		}
		entityChangedEvent.SetEntity(entityID.Type, entityID.Name);
		m_changedEntities.Add(entityID);
		m_entityChangeEvents.Add(val);
		return val;
	}

	public virtual void Clear()
	{
		m_entityChangeEvents.Clear();
		m_changedEntities.Clear();
	}

	public unsafe static void SerializeToXML(IEnumerable<IEntityChangedEvent> changes, global::AssetObjects.String* destinationString)
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.EntityChangeList entityChangeList);
		*(long*)(&entityChangeList) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7EntityChangeList_0040AssetObjects_0040_00406B_0040);
		global::_003CModule_003E.AssetObjects_002EPolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E_002E_007Bctor_007D((PolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref entityChangeList, 8)));
		try
		{
			global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D((global::AssetObjects.String*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref entityChangeList, 72)));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<PolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E*, void>)(&global::_003CModule_003E.AssetObjects_002EPolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E_002E_007Bdtor_007D), System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref entityChangeList, 8)));
			throw;
		}
		try
		{
			BuildNativeChangeList(changes, &entityChangeList);
			global::_003CModule_003E.AssetObjects_002EEntityChangeList_002ESerializeToXML(&entityChangeList, destinationString);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.EntityChangeList*, void>)(&global::_003CModule_003E.AssetObjects_002EEntityChangeList_002E_007Bdtor_007D), &entityChangeList);
			throw;
		}
		try
		{
			global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D((global::AssetObjects.String*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref entityChangeList, 72)));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<PolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E*, void>)(&global::_003CModule_003E.AssetObjects_002EPolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E_002E_007Bdtor_007D), System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref entityChangeList, 8)));
			throw;
		}
		global::_003CModule_003E.AssetObjects_002EPolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E_002E_007Bdtor_007D((PolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref entityChangeList, 8)));
	}

	public unsafe void SerializeToXML(global::AssetObjects.String* destinationString)
	{
		SerializeToXML(m_entityChangeEvents, destinationString);
	}

	internal unsafe void Apply(global::AssetObjects.InstanceSet* rInstanceSet)
	{
		Apply(m_entityChangeEvents, rInstanceSet);
	}

	public unsafe static void Apply(IEnumerable<IEntityChangedEvent> changes, global::AssetObjects.InstanceSet* rInstanceSet)
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.EntityChangeList entityChangeList);
		*(long*)(&entityChangeList) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7EntityChangeList_0040AssetObjects_0040_00406B_0040);
		global::_003CModule_003E.AssetObjects_002EPolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E_002E_007Bctor_007D((PolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref entityChangeList, 8)));
		try
		{
			global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D((global::AssetObjects.String*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref entityChangeList, 72)));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<PolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E*, void>)(&global::_003CModule_003E.AssetObjects_002EPolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E_002E_007Bdtor_007D), System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref entityChangeList, 8)));
			throw;
		}
		try
		{
			BuildNativeChangeList(changes, &entityChangeList);
			global::_003CModule_003E.AssetObjects_002EEntityChangeList_002EApplyChanges(&entityChangeList, rInstanceSet);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.EntityChangeList*, void>)(&global::_003CModule_003E.AssetObjects_002EEntityChangeList_002E_007Bdtor_007D), &entityChangeList);
			throw;
		}
		try
		{
			global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D((global::AssetObjects.String*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref entityChangeList, 72)));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<PolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E*, void>)(&global::_003CModule_003E.AssetObjects_002EPolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E_002E_007Bdtor_007D), System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref entityChangeList, 8)));
			throw;
		}
		global::_003CModule_003E.AssetObjects_002EPolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E_002E_007Bdtor_007D((PolymorphicContainer_003CAssetObjects_003A_003AEntityChangedEvent_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref entityChangeList, 8)));
	}

	private unsafe static void BuildNativeChangeList(IEnumerable<IEntityChangedEvent> changeEvents, global::AssetObjects.EntityChangeList* changeList)
	{
		foreach (EntityChangedEvent changeEvent in changeEvents)
		{
			changeEvent.AddToChangeList(changeList);
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			Clear();
			return;
		}
		try
		{
			Clear();
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

	~EntityChangeList()
	{
		Dispose(A_0: false);
	}
}
