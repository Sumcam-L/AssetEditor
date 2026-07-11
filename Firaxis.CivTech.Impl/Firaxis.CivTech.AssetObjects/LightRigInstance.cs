using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class LightRigInstance : InstanceEntity, ILightRigInstance
{
	private AnimationBindingSet m_pmAnimationBindings;

	private LightReferenceCollection m_pmLightRefs;

	public virtual ILightReferenceCollection LightReferences => m_pmLightRefs;

	public virtual IAnimationBindingSet AnimationBindings => m_pmAnimationBindings;

	public unsafe virtual string DSGName
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002ELightRigInstance_002EGetDSGName((global::AssetObjects.LightRigInstance*)m_pkEntity));
			return Marshal.PtrToStringAnsi(ptr);
		}
		set
		{
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002ELightRigInstance_002ESetDSGName((global::AssetObjects.LightRigInstance*)m_pkEntity, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe LightRigInstance(global::AssetObjects.LightRigInstance* pkInstance, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)pkInstance, pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public unsafe LightRigInstance(global::AssetObjects.InstanceSet* pkInstanceSet, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)global::_003CModule_003E.AssetObjects_002EInstanceSet_002EPush_003Cclass_0020AssetObjects_003A_003ALightRigInstance_003E(pkInstanceSet), pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public virtual IEnumerable<string> GetContainedEntityNames(InstanceType entityType)
	{
		switch (entityType)
		{
		default:
			return new List<string>();
		case InstanceType.IT_ENVIRONMENT_LIGHT:
		{
			List<string> list4 = new List<string>();
			{
				foreach (ILightReference environmentLightReference in m_pmLightRefs.EnvironmentLightReferences)
				{
					list4.Add(environmentLightReference.LightName);
				}
				return list4;
			}
		}
		case InstanceType.IT_ANALYTIC_LIGHT:
		{
			List<string> list3 = new List<string>();
			{
				foreach (ILightReference analyticLightReference in m_pmLightRefs.AnalyticLightReferences)
				{
					list3.Add(analyticLightReference.LightName);
				}
				return list3;
			}
		}
		case InstanceType.IT_DSG:
		{
			List<string> list2 = new List<string>();
			list2.Add(DSGName);
			return list2;
		}
		case InstanceType.IT_ANIMATION:
		{
			List<string> list = new List<string>();
			{
				foreach (IAnimationBinding binding in m_pmAnimationBindings.Bindings)
				{
					if (!list.Contains(binding.AnimationName))
					{
						list.Add(binding.AnimationName);
					}
				}
				return list;
			}
		}
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool AddEntity(IInstanceEntity entity, InstanceType entityType)
	{
		if ((uint)(entityType - 6) <= 1u)
		{
			return m_pmLightRefs.AddLightReference((ILightInstance)entity) != null;
		}
		return false;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool RemoveEntity(IInstanceEntity entity, InstanceType entityType)
	{
		ILightReference lightReference = null;
		switch (entityType)
		{
		default:
			return false;
		case InstanceType.IT_ENVIRONMENT_LIGHT:
			foreach (ILightReference environmentLightReference in m_pmLightRefs.EnvironmentLightReferences)
			{
				if (environmentLightReference.LightName == entity.Name)
				{
					lightReference = environmentLightReference;
				}
			}
			break;
		case InstanceType.IT_ANALYTIC_LIGHT:
			foreach (ILightReference analyticLightReference in m_pmLightRefs.AnalyticLightReferences)
			{
				if (analyticLightReference.LightName == entity.Name)
				{
					lightReference = analyticLightReference;
				}
			}
			break;
		}
		if (lightReference != null)
		{
			m_pmLightRefs.RemoveLightReference(lightReference);
			return true;
		}
		return false;
	}

	internal unsafe override void AddReferences()
	{
		base.AddReferences();
		global::AssetObjects.LightRigInstance* pkEntity = (global::AssetObjects.LightRigInstance*)m_pkEntity;
		m_pmAnimationBindings = new AnimationBindingSet(global::_003CModule_003E.AssetObjects_002ELightRigInstance_002EGetAnimationBindings(pkEntity));
		m_pmLightRefs = new LightReferenceCollection(global::_003CModule_003E.AssetObjects_002ELightRigInstance_002EGetLightReferences(pkEntity));
	}

	internal override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		m_pmAnimationBindings.RemoveReferences();
		m_pmLightRefs.RemoveReferences();
		if (bDisposing)
		{
			m_pmAnimationBindings = null;
			m_pmLightRefs = null;
		}
		base.RemoveReferences(bDisposing);
	}
}
