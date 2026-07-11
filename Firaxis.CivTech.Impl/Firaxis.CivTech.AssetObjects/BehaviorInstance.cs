using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class BehaviorInstance : InstanceEntity, IBehaviorInstance
{
	private string m_dsgName;

	private TimelineSet m_pmTimelineSet = null;

	private AttachmentPointSet m_pmAttachmentPointSet = null;

	private TimelineBindingSet m_pmTimelineBindingSet = null;

	private AnimationBindingSet m_pmAnimationBindings = null;

	internal unsafe global::AssetObjects.BehaviorInstance* UnmanagedPtr => (global::AssetObjects.BehaviorInstance*)m_pkEntity;

	public unsafe virtual IEnumerable<string> ReferencedBehaviors
	{
		get
		{
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002Ebehaviors_begin((global::AssetObjects.BehaviorInstance*)m_pkEntity, &const_iterator);
			global::AssetObjects.InstanceEntity* pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002Ebehaviors_end((global::AssetObjects.BehaviorInstance*)pkEntity, &const_iterator2)))
			{
				do
				{
					string item = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(item);
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
					pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002Ebehaviors_end((global::AssetObjects.BehaviorInstance*)pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public unsafe virtual IEnumerable<string> ReferenceGeometries
	{
		get
		{
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002Egeos_begin((global::AssetObjects.BehaviorInstance*)m_pkEntity, &const_iterator);
			global::AssetObjects.InstanceEntity* pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002Egeos_end((global::AssetObjects.BehaviorInstance*)pkEntity, &const_iterator2)))
			{
				do
				{
					string item = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(item);
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
					pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002Egeos_end((global::AssetObjects.BehaviorInstance*)pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public virtual IAttachmentPointSet AttachmentPointSet => m_pmAttachmentPointSet;

	public virtual ITimelineSet Timelines => m_pmTimelineSet;

	public virtual ITimelineBindingSet TimelineBindings => m_pmTimelineBindingSet;

	public virtual IAnimationBindingSet AnimationBindings => m_pmAnimationBindings;

	public unsafe virtual string DSGName
	{
		get
		{
			//IL_0026: Expected I, but got I8
			//IL_0026: Expected I, but got I8
			if (string.IsNullOrEmpty(m_dsgName))
			{
				long num = (long)(nint)m_pkEntity + 320L;
				sbyte* value = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*>)(*(ulong*)(*(long*)num + 40)))((nint)num);
				m_dsgName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(value);
			}
			return m_dsgName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_dsgName = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002ESetDSGName((global::AssetObjects.BehaviorInstance*)m_pkEntity, standardStringWrapper.Value);
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

	public unsafe BehaviorInstance(global::AssetObjects.BehaviorInstance* pkInstance, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)pkInstance, pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public unsafe BehaviorInstance(global::AssetObjects.InstanceSet* pkInstanceSet, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)global::_003CModule_003E.AssetObjects_002EInstanceSet_002EPush_003Cclass_0020AssetObjects_003A_003ABehaviorInstance_003E(pkInstanceSet), pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public virtual IEnumerable<EntityID> GetDependentAssets()
	{
		ISet<EntityID> set = new SortedSet<EntityID>();
		foreach (ITimeline timeline in Timelines.Timelines)
		{
			IEnumerator<ITrigger> enumerator2 = timeline.Triggers.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					ITrigger current = enumerator2.Current;
					string fXName = current.FXName;
					if (current.Type == TriggerType.TT_ASSET_VFX)
					{
						EntityID item = new EntityID(fXName, InstanceType.IT_ASSET);
						set.Add(item);
					}
					else if (current.Type == TriggerType.TT_LIGHT)
					{
						EntityID item2 = new EntityID(fXName, InstanceType.IT_ANALYTIC_LIGHT);
						set.Add(item2);
					}
				}
			}
			finally
			{
				IEnumerator<ITrigger> enumerator3 = enumerator2;
				IDisposable disposable = enumerator2;
				enumerator2?.Dispose();
			}
		}
		return set;
	}

	public unsafe virtual void Flatten(IInstanceSet pmInstanceSet, IClassSet pmClassSet, IBehaviorInstance pmBehavior)
	{
		pmBehavior.ClearBehaviorOverrides();
		InstanceSet obj = (InstanceSet)pmInstanceSet;
		ClassSet classSet = (ClassSet)pmClassSet;
		BehaviorInstance behaviorInstance = (BehaviorInstance)pmBehavior;
		global::AssetObjects.InstanceSet* unmanagedPtr = obj.UnmanagedPtr;
		global::AssetObjects.ClassSet* unmanagedPtr2 = classSet.UnmanagedPtr;
		global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002EFlatten((global::AssetObjects.BehaviorInstance*)m_pkEntity, unmanagedPtr, unmanagedPtr2, (global::AssetObjects.BehaviorInstance*)behaviorInstance.m_pkEntity);
		behaviorInstance.RemoveReferences(bDisposing: false);
		behaviorInstance.AddReferences();
	}

	public unsafe virtual void Export(IBehaviorInstance pmBehavior)
	{
		BehaviorInstance behaviorInstance = (BehaviorInstance)pmBehavior;
		global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002EExport((global::AssetObjects.BehaviorInstance*)m_pkEntity, (global::AssetObjects.BehaviorInstance*)behaviorInstance.m_pkEntity);
		behaviorInstance.RemoveReferences(bDisposing: false);
		behaviorInstance.AddReferences();
	}

	public virtual void ClearBehaviorOverrides()
	{
		m_pmTimelineSet.ClearTimelines();
		m_pmAttachmentPointSet.ClearAttachmentPoints();
		m_pmTimelineBindingSet.ClearBindings();
		m_pmAnimationBindings.ClearBindings();
	}

	public unsafe virtual void AddReferenceGeometry(string pmGeoName)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmGeoName);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002EAddReferenceGeometry((global::AssetObjects.BehaviorInstance*)m_pkEntity, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	public unsafe virtual void RemoveReferenceGeometry(string pmGeoName)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmGeoName);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002ERemoveReferenceGeometry((global::AssetObjects.BehaviorInstance*)m_pkEntity, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	public unsafe virtual void AddBehavior(IBehaviorInstance pmBehavior)
	{
		BehaviorInstance behaviorInstance = (BehaviorInstance)pmBehavior;
		global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002EAddBehavior((global::AssetObjects.BehaviorInstance*)m_pkEntity, (global::AssetObjects.BehaviorInstance*)behaviorInstance.m_pkEntity);
	}

	public virtual void RemoveBehavior(IBehaviorInstance pmBehavior)
	{
		RemoveBehavior(pmBehavior.Name);
	}

	public unsafe virtual void RemoveBehavior(string pmBehaviorName)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmBehaviorName);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002ERemoveBehavior((global::AssetObjects.BehaviorInstance*)m_pkEntity, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	public unsafe virtual void MoveChildBehaviors(string pmChildNameOne, string pmChildNameTwo)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(pmChildNameOne);
		try
		{
			standardStringWrapper = standardStringWrapper3;
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(pmChildNameTwo);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002EMoveChildBehaviors((global::AssetObjects.BehaviorInstance*)m_pkEntity, standardStringWrapper.Value, standardStringWrapper2.Value);
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

	public unsafe virtual void MoveChildBehaviors(string pmChildName, int desiredLocation)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmChildName);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002EMoveChildBehaviors((global::AssetObjects.BehaviorInstance*)m_pkEntity, standardStringWrapper.Value, (uint)desiredLocation);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	public unsafe virtual void MoveChildBehaviors(int childIndex1, int childIndex2)
	{
		global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002EMoveChildBehaviors((global::AssetObjects.BehaviorInstance*)m_pkEntity, (uint)childIndex1, (uint)childIndex2);
	}

	internal unsafe override void AddReferences()
	{
		//IL_0050: Expected I, but got I8
		//IL_0050: Expected I, but got I8
		base.AddReferences();
		global::AssetObjects.TimelineSet* timelineSet = global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002EGetTimelines((global::AssetObjects.BehaviorInstance*)m_pkEntity);
		global::AssetObjects.AttachmentPointSet* pkAttachmentPointSet = global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002EGetAttachmentPointSet((global::AssetObjects.BehaviorInstance*)m_pkEntity);
		global::AssetObjects.AnimationBindingSet* pkBindingSet = global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002EGetAnimationBindings((global::AssetObjects.BehaviorInstance*)m_pkEntity);
		global::AssetObjects.TimelineBindingSet* pkBindingSet2 = global::_003CModule_003E.AssetObjects_002EBehaviorInstance_002EGetTimelineBindings((global::AssetObjects.BehaviorInstance*)m_pkEntity);
		long num = (long)(nint)m_pkEntity + 320L;
		sbyte* value = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*>)(*(ulong*)(*(long*)num + 40)))((nint)num);
		m_pmTimelineSet = new TimelineSet(timelineSet);
		m_pmAttachmentPointSet = new AttachmentPointSet(pkAttachmentPointSet, m_pkDeserializer);
		m_pmAnimationBindings = new AnimationBindingSet(pkBindingSet);
		m_pmTimelineBindingSet = new TimelineBindingSet(pkBindingSet2);
		m_dsgName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(value);
	}

	internal override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		m_pmTimelineSet.RemoveReferences(bDisposing);
		m_pmAttachmentPointSet.RemoveReferences();
		m_pmAnimationBindings.RemoveReferences();
		m_pmTimelineBindingSet.RemoveReferences();
		m_dsgName = string.Empty;
		if (bDisposing)
		{
			m_pmTimelineSet = null;
			m_pmAttachmentPointSet = null;
			m_pmAnimationBindings = null;
			m_pmTimelineBindingSet = null;
		}
		base.RemoveReferences(bDisposing);
	}
}
