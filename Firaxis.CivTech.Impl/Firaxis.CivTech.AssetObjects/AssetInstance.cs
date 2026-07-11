using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class AssetInstance : InstanceEntity, IAssetInstance
{
	private TimelineSet m_pmTimelineSet = null;

	private SplineSet m_pmSplineSet = null;

	private GeometrySet m_pmGeometrySet = null;

	private AttachmentPointSet m_pmAttachmentPointSet = null;

	private TimelineBindingSet m_pmTimelineBindingSet = null;

	private AnimationBindingSet m_pmAnimationBindings = null;

	internal unsafe global::AssetObjects.AssetInstance* UnmanagedPtr => (global::AssetObjects.AssetInstance*)m_pkEntity;

	public unsafe virtual IEnumerable<string> ReferencedBehaviors
	{
		get
		{
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EAssetInstance_002Ebehaviors_begin((global::AssetObjects.AssetInstance*)m_pkEntity, &const_iterator);
			global::AssetObjects.InstanceEntity* pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetInstance_002Ebehaviors_end((global::AssetObjects.AssetInstance*)pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					string item = Marshal.PtrToStringAnsi(ptr);
					list.Add(item);
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
					pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetInstance_002Ebehaviors_end((global::AssetObjects.AssetInstance*)pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public virtual IAttachmentPointSet AttachmentPointSet => m_pmAttachmentPointSet;

	public virtual ISplineSet SplineSet => m_pmSplineSet;

	public virtual IGeometrySet GeometrySet => m_pmGeometrySet;

	public virtual ITimelineSet Timelines => m_pmTimelineSet;

	public virtual ITimelineBindingSet TimelineBindings => m_pmTimelineBindingSet;

	public virtual IAnimationBindingSet AnimationBindings => m_pmAnimationBindings;

	public unsafe virtual string DSGName
	{
		get
		{
			//IL_000e: Expected I, but got I8
			//IL_001d: Expected I, but got I8
			Entity* ptr = (Entity*)((ulong)(nint)m_pkEntity + 320uL);
			IntPtr ptr2 = new IntPtr(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*>)(*(ulong*)(*(long*)ptr + 40)))((nint)ptr));
			return Marshal.PtrToStringAnsi(ptr2);
		}
		set
		{
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002EAssetInstance_002ESetDSGName((global::AssetObjects.AssetInstance*)m_pkEntity, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe AssetInstance(global::AssetObjects.AssetInstance* pkInstance, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)pkInstance, pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public unsafe AssetInstance(global::AssetObjects.InstanceSet* pkInstanceSet, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)global::_003CModule_003E.AssetObjects_002EInstanceSet_002EPush_003Cclass_0020AssetObjects_003A_003AAssetInstance_003E(pkInstanceSet), pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public virtual IModelInstance AddModelInstance(string instanceName, IGeometryInstance geo)
	{
		return m_pmGeometrySet.AddModelInstance(instanceName, geo);
	}

	public virtual void RemoveModelInstance(string instanceName)
	{
		m_pmGeometrySet.RemoveModelInstance(instanceName);
	}

	public unsafe virtual void Flatten(IInstanceSet pmInstanceSet, IClassSet pmClassSet, IBehaviorInstance pmBehavior)
	{
		pmBehavior.ClearBehaviorOverrides();
		InstanceSet instanceSet = (InstanceSet)pmInstanceSet;
		BehaviorInstance behaviorInstance = (BehaviorInstance)pmBehavior;
		ClassSet classSet = (ClassSet)pmClassSet;
		global::AssetObjects.InstanceEntity* pkEntity = (global::AssetObjects.InstanceEntity*)((CloudEntity)behaviorInstance).m_pkEntity;
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002EFlatten((global::AssetObjects.AssetInstance*)m_pkEntity, instanceSet.UnmanagedPtr, classSet.UnmanagedPtr, (global::AssetObjects.BehaviorInstance*)pkEntity);
		behaviorInstance.RemoveReferences(bDisposing: false);
		behaviorInstance.AddReferences();
	}

	public unsafe virtual void Export(IBehaviorInstance pmBehavior)
	{
		BehaviorInstance behaviorInstance = (BehaviorInstance)pmBehavior;
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002EExport((global::AssetObjects.AssetInstance*)m_pkEntity, (global::AssetObjects.BehaviorInstance*)((CloudEntity)behaviorInstance).m_pkEntity);
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

	public unsafe virtual void AddBehavior(IBehaviorInstance pmBehavior)
	{
		BehaviorInstance behaviorInstance = (BehaviorInstance)pmBehavior;
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002EAddBehavior((global::AssetObjects.AssetInstance*)m_pkEntity, (global::AssetObjects.BehaviorInstance*)((CloudEntity)behaviorInstance).m_pkEntity);
	}

	public virtual void RemoveBehavior(IBehaviorInstance pmBehavior)
	{
		RemoveBehavior(pmBehavior.Name);
	}

	public unsafe virtual void RemoveBehavior(string pmBehaviorName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmBehaviorName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002ERemoveBehavior((global::AssetObjects.AssetInstance*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void MoveChildBehaviors(string pmChildNameOne, string pmChildNameTwo)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmChildNameOne).ToPointer();
		sbyte* ptr2 = (sbyte*)Marshal.StringToHGlobalAnsi(pmChildNameTwo).ToPointer();
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002EMoveChildBehaviors((global::AssetObjects.AssetInstance*)m_pkEntity, ptr, ptr2);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		IntPtr hglobal2 = new IntPtr(ptr2);
		Marshal.FreeHGlobal(hglobal2);
	}

	public unsafe virtual void MoveChildBehaviors(string pmChildName, int desiredLocation)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmChildName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002EMoveChildBehaviors((global::AssetObjects.AssetInstance*)m_pkEntity, ptr, (uint)desiredLocation);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void MoveChildBehaviors(int childIndex1, int childIndex2)
	{
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002EMoveChildBehaviors((global::AssetObjects.AssetInstance*)m_pkEntity, (uint)childIndex1, (uint)childIndex2);
	}

	public unsafe virtual void AddGeometry(IGeometryInstance geo)
	{
		InstanceEntity instanceEntity = (GeometryInstance)geo;
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002EAddGeometry((global::AssetObjects.AssetInstance*)m_pkEntity, (global::AssetObjects.GeometryInstance*)((CloudEntity)instanceEntity).m_pkEntity);
	}

	public unsafe virtual void AddAnimation(IAnimationInstance anim)
	{
		InstanceEntity instanceEntity = (AnimationInstance)anim;
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002EAddAnimation((global::AssetObjects.AssetInstance*)m_pkEntity, (global::AssetObjects.AnimationInstance*)((CloudEntity)instanceEntity).m_pkEntity);
	}

	public unsafe virtual void AddMaterial(IMaterialInstance mat)
	{
		InstanceEntity instanceEntity = (MaterialInstance)mat;
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002EAddMaterial((global::AssetObjects.AssetInstance*)m_pkEntity, (global::AssetObjects.MaterialInstance*)((CloudEntity)instanceEntity).m_pkEntity);
	}

	public unsafe virtual void AddParticleEffect(string vfxName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(vfxName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002EAddParticleEffect((global::AssetObjects.AssetInstance*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void AddParticleEffect(IParticleEffectInstance pfx)
	{
		InstanceEntity instanceEntity = (ParticleEffectInstance)pfx;
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002EAddParticleEffect((global::AssetObjects.AssetInstance*)m_pkEntity, (global::AssetObjects.ParticleEffectInstance*)((CloudEntity)instanceEntity).m_pkEntity);
	}

	public unsafe virtual void RemoveGeometry(IGeometryInstance geo)
	{
		InstanceEntity instanceEntity = (GeometryInstance)geo;
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002ERemoveGeometry((global::AssetObjects.AssetInstance*)m_pkEntity, (global::AssetObjects.GeometryInstance*)((CloudEntity)instanceEntity).m_pkEntity);
	}

	public unsafe virtual void RemoveAnimation(IAnimationInstance anim)
	{
		InstanceEntity instanceEntity = (AnimationInstance)anim;
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002ERemoveAnimation((global::AssetObjects.AssetInstance*)m_pkEntity, (global::AssetObjects.AnimationInstance*)((CloudEntity)instanceEntity).m_pkEntity);
	}

	public unsafe virtual void RemoveMaterial(IMaterialInstance mat)
	{
		InstanceEntity instanceEntity = (MaterialInstance)mat;
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002ERemoveMaterial((global::AssetObjects.AssetInstance*)m_pkEntity, (global::AssetObjects.MaterialInstance*)((CloudEntity)instanceEntity).m_pkEntity);
	}

	public unsafe virtual void RemoveParticleEffect(string pfxName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pfxName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002ERemoveParticleEffect((global::AssetObjects.AssetInstance*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void RemoveParticleEffect(IParticleEffectInstance pfx)
	{
		InstanceEntity instanceEntity = (ParticleEffectInstance)pfx;
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002ERemoveParticleEffect((global::AssetObjects.AssetInstance*)m_pkEntity, (global::AssetObjects.ParticleEffectInstance*)((CloudEntity)instanceEntity).m_pkEntity);
	}

	public unsafe virtual IEnumerable<string> GetGeometries()
	{
		List<string> list = new List<string>();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002Egeometries_begin((global::AssetObjects.AssetInstance*)m_pkEntity, &iterator);
		global::AssetObjects.InstanceEntity* pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAssetInstance_002Egeometries_end((global::AssetObjects.AssetInstance*)pkEntity, &iterator2)))
		{
			do
			{
				IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002D_003E(&iterator)));
				string item = Marshal.PtrToStringAnsi(ptr);
				list.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
				pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAssetInstance_002Egeometries_end((global::AssetObjects.AssetInstance*)pkEntity, &iterator2)));
		}
		return list;
	}

	public unsafe virtual IEnumerable<string> GetAnimations()
	{
		List<string> list = new List<string>();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002Eanimations_begin((global::AssetObjects.AssetInstance*)m_pkEntity, &iterator);
		global::AssetObjects.InstanceEntity* pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAssetInstance_002Eanimations_end((global::AssetObjects.AssetInstance*)pkEntity, &iterator2)))
		{
			do
			{
				IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002D_003E(&iterator)));
				string item = Marshal.PtrToStringAnsi(ptr);
				list.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
				pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAssetInstance_002Eanimations_end((global::AssetObjects.AssetInstance*)pkEntity, &iterator2)));
		}
		return list;
	}

	public unsafe virtual IEnumerable<string> GetMaterials()
	{
		List<string> list = new List<string>();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002Ematerials_begin((global::AssetObjects.AssetInstance*)m_pkEntity, &iterator);
		global::AssetObjects.InstanceEntity* pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAssetInstance_002Ematerials_end((global::AssetObjects.AssetInstance*)pkEntity, &iterator2)))
		{
			do
			{
				IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002D_003E(&iterator)));
				string item = Marshal.PtrToStringAnsi(ptr);
				list.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
				pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAssetInstance_002Ematerials_end((global::AssetObjects.AssetInstance*)pkEntity, &iterator2)));
		}
		return list;
	}

	public unsafe virtual IEnumerable<string> GetParticleEffects()
	{
		List<string> list = new List<string>();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EAssetInstance_002Eparticleeffects_begin((global::AssetObjects.AssetInstance*)m_pkEntity, &iterator);
		global::AssetObjects.InstanceEntity* pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAssetInstance_002Eparticleeffects_end((global::AssetObjects.AssetInstance*)pkEntity, &iterator2)))
		{
			do
			{
				IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002D_003E(&iterator)));
				string item = Marshal.PtrToStringAnsi(ptr);
				list.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
				pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAssetInstance_002Eparticleeffects_end((global::AssetObjects.AssetInstance*)pkEntity, &iterator2)));
		}
		return list;
	}

	public virtual IList<string> GetDependentAssets()
	{
		List<string> list = new List<string>();
		foreach (IAttachmentPoint item in AttachmentPointSet.Items)
		{
			IEnumerator<ITimeline> enumerator2 = Timelines.Timelines.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					IEnumerator<ITrigger> enumerator3 = enumerator2.Current.Triggers.GetEnumerator();
					try
					{
						while (enumerator3.MoveNext())
						{
							ITrigger current2 = enumerator3.Current;
							string fXName = current2.FXName;
							if (current2.Type == TriggerType.TT_ASSET_VFX && current2.AttachmentPointName.Equals(item.Name) && !list.Contains(fXName))
							{
								list.Add(fXName);
							}
						}
					}
					finally
					{
						IEnumerator<ITrigger> enumerator4 = enumerator3;
						IDisposable disposable = enumerator3;
						enumerator3?.Dispose();
					}
				}
			}
			finally
			{
				IEnumerator<ITimeline> enumerator5 = enumerator2;
				IDisposable disposable2 = enumerator2;
				enumerator2?.Dispose();
			}
		}
		return list;
	}

	public virtual IEnumerable<string> GetContainedEntityNames(InstanceType entityType)
	{
		List<string> list = new List<string>();
		switch (entityType)
		{
		case InstanceType.IT_PARTICLE_EFFECT:
			list.AddRange(GetParticleEffects());
			break;
		case InstanceType.IT_DSG:
			list.Add(DSGName);
			break;
		case InstanceType.IT_ANIMATION:
			list.AddRange(GetAnimations());
			break;
		case InstanceType.IT_GEOMETRY:
			list.AddRange(GetGeometries());
			break;
		case InstanceType.IT_MATERIAL:
			list.AddRange(GetMaterials());
			break;
		}
		return list;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool AddEntity(IInstanceEntity entity, InstanceType type)
	{
		switch (type)
		{
		default:
			return false;
		case InstanceType.IT_PARTICLE_EFFECT:
			AddParticleEffect((IParticleEffectInstance)entity);
			return true;
		case InstanceType.IT_ANIMATION:
			AddAnimation((IAnimationInstance)entity);
			return true;
		case InstanceType.IT_GEOMETRY:
			AddGeometry((IGeometryInstance)entity);
			return true;
		case InstanceType.IT_MATERIAL:
			AddMaterial((IMaterialInstance)entity);
			return true;
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool RemoveEntity(IInstanceEntity entity, InstanceType type)
	{
		switch (type)
		{
		default:
			return false;
		case InstanceType.IT_PARTICLE_EFFECT:
			RemoveParticleEffect((IParticleEffectInstance)entity);
			return true;
		case InstanceType.IT_ANIMATION:
			RemoveAnimation((IAnimationInstance)entity);
			return true;
		case InstanceType.IT_GEOMETRY:
			RemoveGeometry((IGeometryInstance)entity);
			return true;
		case InstanceType.IT_MATERIAL:
			RemoveMaterial((IMaterialInstance)entity);
			return true;
		}
	}

	internal unsafe override void AddReferences()
	{
		base.AddReferences();
		global::AssetObjects.AssetInstance* pkEntity = (global::AssetObjects.AssetInstance*)m_pkEntity;
		m_pmTimelineSet = new TimelineSet(global::_003CModule_003E.AssetObjects_002EAssetInstance_002EGetTimelines(pkEntity));
		m_pmGeometrySet = new GeometrySet(global::_003CModule_003E.AssetObjects_002EAssetInstance_002EGetGeometrySet(pkEntity), m_pkDeserializer);
		m_pmAttachmentPointSet = new AttachmentPointSet(global::_003CModule_003E.AssetObjects_002EAssetInstance_002EGetAttachmentPointSet(pkEntity), m_pkDeserializer);
		m_pmAnimationBindings = new AnimationBindingSet(global::_003CModule_003E.AssetObjects_002EAssetInstance_002EGetAnimationBindings(pkEntity));
		m_pmTimelineBindingSet = new TimelineBindingSet(global::_003CModule_003E.AssetObjects_002EAssetInstance_002EGetTimelineBindings(pkEntity));
		m_pmSplineSet = new SplineSet(global::_003CModule_003E.AssetObjects_002EAssetInstance_002EGetSplineSet(pkEntity), m_pkDeserializer);
	}

	internal override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		m_pmTimelineSet?.RemoveReferences(bDisposing);
		m_pmGeometrySet?.RemoveReferences();
		m_pmAttachmentPointSet?.RemoveReferences();
		m_pmAnimationBindings?.RemoveReferences();
		m_pmTimelineBindingSet?.RemoveReferences();
		m_pmSplineSet?.RemoveReferences();
		if (bDisposing)
		{
			m_pmTimelineSet = null;
			m_pmGeometrySet = null;
			m_pmAttachmentPointSet = null;
			m_pmAnimationBindings = null;
			m_pmTimelineBindingSet = null;
			m_pmSplineSet = null;
		}
		base.RemoveReferences(bDisposing);
	}
}
