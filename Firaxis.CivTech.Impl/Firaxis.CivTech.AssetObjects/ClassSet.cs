using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ClassSet : IClassSet
{
	private List<IClassEntity> m_pmClassEntities;

	private unsafe global::AssetObjects.ClassSet* m_pkClassSet;

	private unsafe global::AssetObjects.Deserializer* m_pkDeserializer;

	internal unsafe global::AssetObjects.ClassSet* UnmanagedPtr => m_pkClassSet;

	public virtual IEnumerable<IClassEntity> Items => new List<IClassEntity>(m_pmClassEntities);

	public unsafe ClassSet(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
	{
		m_pmClassEntities = new List<IClassEntity>();
		m_pkClassSet = pkClassSet;
		m_pkDeserializer = pkDeserializer;
		base._002Ector();
	}

	public unsafe virtual T Push<T>(string pmName) where T : IClassEntity
	{
		if (typeof(T) == typeof(IClassEntity))
		{
			return default(T);
		}
		object[] array = new object[2];
		object obj = Pointer.Box(m_pkClassSet, typeof(global::AssetObjects.ClassSet*));
		array[0] = obj;
		object obj2 = Pointer.Box(m_pkDeserializer, typeof(global::AssetObjects.Deserializer*));
		array[1] = obj2;
		Type[] array2 = new Type[2];
		Type typeFromHandle = typeof(global::AssetObjects.ClassSet*);
		array2[0] = typeFromHandle;
		Type typeFromHandle2 = typeof(global::AssetObjects.Deserializer*);
		array2[1] = typeFromHandle2;
		T val = global::_003CModule_003E.Firaxis_002ECivTech_002EReflectionHelperEx_002E_003FA0x6aa5f60f_002ETypeLoader<T>(Assembly.GetExecutingAssembly(), array, array2);
		((ClassEntity)(object)val).AddReferences();
		val.Name = pmName;
		m_pmClassEntities.Add(val);
		return val;
	}

	public unsafe virtual void Clear()
	{
		List<IClassEntity>.Enumerator enumerator = m_pmClassEntities.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((ClassEntity)enumerator.Current).RemoveReferences(bDisposing: true);
			}
			while (enumerator.MoveNext());
		}
		global::_003CModule_003E.AssetObjects_002EClassSet_002Eclear(m_pkClassSet);
		m_pmClassEntities.Clear();
	}

	public unsafe virtual void Remove(IClassEntity pmClassEntity)
	{
		int num = m_pmClassEntities.IndexOf(pmClassEntity);
		if (num >= 0)
		{
			ClassEntity classEntity = (ClassEntity)pmClassEntity;
			global::_003CModule_003E.AssetObjects_002EClassSet_002ERemove(m_pkClassSet, (global::AssetObjects.ClassEntity*)classEntity.GetAssetObject());
			classEntity.RemoveReferences(bDisposing: true);
			m_pmClassEntities.RemoveAt(num);
		}
	}

	public unsafe virtual IClassEntity FindForInstance(IInstanceEntity pmInstanceEntity)
	{
		if (pmInstanceEntity == null)
		{
			return null;
		}
		global::AssetObjects.InstanceEntity* assetObject = ((InstanceEntity)pmInstanceEntity).GetAssetObject();
		return FindClassEntity(global::_003CModule_003E.AssetObjects_002EClassSet_002EFindForInstance(m_pkClassSet, assetObject));
	}

	public virtual IClassEntity FindByName(string pmClassName)
	{
		if (pmClassName != null)
		{
			List<IClassEntity>.Enumerator enumerator = m_pmClassEntities.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					IClassEntity current = enumerator.Current;
					if (current.Name == pmClassName)
					{
						return current;
					}
				}
				while (enumerator.MoveNext());
			}
		}
		return null;
	}

	public virtual List<IClassEntity> GetClasses()
	{
		return new List<IClassEntity>(m_pmClassEntities);
	}

	internal unsafe static global::AssetObjects.ClassEntity* CloneClassEntity(global::AssetObjects.ClassEntity* pkClassEntity)
	{
		//IL_0071: Expected I, but got I8
		//IL_00d7: Expected I, but got I8
		//IL_010b: Expected I, but got I8
		//IL_0172: Expected I, but got I8
		//IL_01d4: Expected I, but got I8
		//IL_0223: Expected I, but got I8
		//IL_0283: Expected I, but got I8
		//IL_02f9: Expected I, but got I8
		//IL_032c: Expected I, but got I8
		//IL_037b: Expected I, but got I8
		//IL_03af: Expected I, but got I8
		//IL_040a: Expected I, but got I8
		//IL_0458: Expected I, but got I8
		//IL_04b7: Expected I, but got I8
		//IL_0554: Expected I, but got I8
		//IL_0505: Expected I, but got I8
		//IL_049e: Expected I, but got I8
		//IL_049e: Expected I, but got I8
		switch (global::_003CModule_003E.AssetObjects_002EClassEntity_002EGetClassType(pkClassEntity))
		{
		case (global::AssetObjects.ClassType)0:
		{
			global::AssetObjects.AssetClass* ptr16 = (global::AssetObjects.AssetClass*)global::_003CModule_003E.@new(1072uL);
			try
			{
				if (ptr16 != null)
				{
					return (global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EAssetClass_002E_007Bctor_007D(ptr16, (global::AssetObjects.AssetClass*)pkClassEntity);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr16, 1072uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)1:
		{
			global::AssetObjects.MaterialClass* ptr15 = (global::AssetObjects.MaterialClass*)global::_003CModule_003E.@new(312uL);
			try
			{
				if (ptr15 != null)
				{
					global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bctor_007D((global::AssetObjects.ClassEntity*)ptr15, pkClassEntity);
					try
					{
						*(long*)ptr15 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7MaterialClass_0040AssetObjects_0040_00406B_0040);
						// IL cpblk instruction
						System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned((long)(nint)ptr15 + 304L, (long)(nint)pkClassEntity + 304L, 1);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.ClassEntity*, void>)(&global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bdtor_007D), ptr15);
						throw;
					}
					return (global::AssetObjects.ClassEntity*)ptr15;
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr15, 312uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)2:
		{
			global::AssetObjects.GeometryClass* ptr14 = (global::AssetObjects.GeometryClass*)global::_003CModule_003E.@new(384uL);
			try
			{
				if (ptr14 != null)
				{
					return (global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EGeometryClass_002E_007Bctor_007D(ptr14, (global::AssetObjects.GeometryClass*)pkClassEntity);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr14, 384uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)3:
		{
			global::AssetObjects.TextureClass* ptr13 = (global::AssetObjects.TextureClass*)global::_003CModule_003E.@new(368uL);
			try
			{
				if (ptr13 != null)
				{
					global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bctor_007D((global::AssetObjects.ClassEntity*)ptr13, pkClassEntity);
					try
					{
						*(long*)ptr13 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7TextureClass_0040AssetObjects_0040_00406B_0040);
						// IL cpblk instruction
						System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned((long)(nint)ptr13 + 304L, (long)(nint)pkClassEntity + 304L, 60);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.ClassEntity*, void>)(&global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bdtor_007D), ptr13);
						throw;
					}
					return (global::AssetObjects.ClassEntity*)ptr13;
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr13, 368uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)4:
		{
			global::AssetObjects.AnimationClass* ptr12 = (global::AssetObjects.AnimationClass*)global::_003CModule_003E.@new(312uL);
			try
			{
				if (ptr12 != null)
				{
					global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bctor_007D((global::AssetObjects.ClassEntity*)ptr12, pkClassEntity);
					try
					{
						*(long*)ptr12 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7AnimationClass_0040AssetObjects_0040_00406B_0040);
						*(int*)((ulong)(nint)ptr12 + 304uL) = *(int*)((ulong)(nint)pkClassEntity + 304uL);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.ClassEntity*, void>)(&global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bdtor_007D), ptr12);
						throw;
					}
					return (global::AssetObjects.ClassEntity*)ptr12;
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr12, 312uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)5:
		{
			global::AssetObjects.DSGClass* ptr11 = (global::AssetObjects.DSGClass*)global::_003CModule_003E.@new(304uL);
			try
			{
				if (ptr11 != null)
				{
					global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bctor_007D((global::AssetObjects.ClassEntity*)ptr11, pkClassEntity);
					try
					{
						*(long*)ptr11 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7DSGClass_0040AssetObjects_0040_00406B_0040);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.ClassEntity*, void>)(&global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bdtor_007D), ptr11);
						throw;
					}
					return (global::AssetObjects.ClassEntity*)ptr11;
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr11, 304uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)6:
		{
			global::AssetObjects.AnalyticLightClass* ptr10 = (global::AssetObjects.AnalyticLightClass*)global::_003CModule_003E.@new(304uL);
			try
			{
				if (ptr10 != null)
				{
					global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bctor_007D((global::AssetObjects.ClassEntity*)ptr10, pkClassEntity);
					try
					{
						*(long*)ptr10 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7LightClass_0040AssetObjects_0040_00406B_0040);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.ClassEntity*, void>)(&global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bdtor_007D), ptr10);
						throw;
					}
					try
					{
						*(long*)ptr10 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7AnalyticLightClass_0040AssetObjects_0040_00406B_0040);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<LightClass*, void>)(&global::_003CModule_003E.AssetObjects_002ELightClass_002E_007Bdtor_007D), ptr10);
						throw;
					}
					return (global::AssetObjects.ClassEntity*)ptr10;
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr10, 304uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)7:
		{
			global::AssetObjects.EnvironmentLightClass* ptr9 = (global::AssetObjects.EnvironmentLightClass*)global::_003CModule_003E.@new(320uL);
			try
			{
				if (ptr9 != null)
				{
					global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bctor_007D((global::AssetObjects.ClassEntity*)ptr9, pkClassEntity);
					try
					{
						*(long*)ptr9 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7LightClass_0040AssetObjects_0040_00406B_0040);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.ClassEntity*, void>)(&global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bdtor_007D), ptr9);
						throw;
					}
					try
					{
						*(long*)ptr9 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7EnvironmentLightClass_0040AssetObjects_0040_00406B_0040);
						// IL cpblk instruction
						System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned((long)(nint)ptr9 + 304L, (long)(nint)pkClassEntity + 304L, 16);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<LightClass*, void>)(&global::_003CModule_003E.AssetObjects_002ELightClass_002E_007Bdtor_007D), ptr9);
						throw;
					}
					return (global::AssetObjects.ClassEntity*)ptr9;
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr9, 320uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)8:
		{
			global::AssetObjects.LightRigClass* ptr8 = (global::AssetObjects.LightRigClass*)global::_003CModule_003E.@new(496uL);
			try
			{
				if (ptr8 != null)
				{
					return (global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002ELightRigClass_002E_007Bctor_007D(ptr8, (global::AssetObjects.LightRigClass*)pkClassEntity);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr8, 496uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)9:
		{
			global::AssetObjects.ParticleEffectClass* ptr7 = (global::AssetObjects.ParticleEffectClass*)global::_003CModule_003E.@new(304uL);
			try
			{
				if (ptr7 != null)
				{
					global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bctor_007D((global::AssetObjects.ClassEntity*)ptr7, pkClassEntity);
					try
					{
						*(long*)ptr7 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7ParticleEffectClass_0040AssetObjects_0040_00406B_0040);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.ClassEntity*, void>)(&global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bdtor_007D), ptr7);
						throw;
					}
					return (global::AssetObjects.ClassEntity*)ptr7;
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr7, 304uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)10:
		{
			global::AssetObjects.BehaviorClass* ptr6 = (global::AssetObjects.BehaviorClass*)global::_003CModule_003E.@new(688uL);
			try
			{
				if (ptr6 != null)
				{
					return (global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EBehaviorClass_002E_007Bctor_007D(ptr6, (global::AssetObjects.BehaviorClass*)pkClassEntity);
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr6, 688uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)11:
		{
			global::AssetObjects.SplineClass* ptr5 = (global::AssetObjects.SplineClass*)global::_003CModule_003E.@new(312uL);
			try
			{
				if (ptr5 != null)
				{
					global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bctor_007D((global::AssetObjects.ClassEntity*)ptr5, pkClassEntity);
					try
					{
						*(long*)ptr5 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7SplineClass_0040AssetObjects_0040_00406B_0040);
						*(sbyte*)((ulong)(nint)ptr5 + 304uL) = *(sbyte*)((ulong)(nint)pkClassEntity + 304uL);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.ClassEntity*, void>)(&global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bdtor_007D), ptr5);
						throw;
					}
					return (global::AssetObjects.ClassEntity*)ptr5;
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr5, 312uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)12:
		{
			global::AssetObjects.TriggerClass* ptr4 = (global::AssetObjects.TriggerClass*)global::_003CModule_003E.@new(304uL);
			try
			{
				if (ptr4 != null)
				{
					global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bctor_007D((global::AssetObjects.ClassEntity*)ptr4, pkClassEntity);
					try
					{
						*(long*)ptr4 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7TriggerClass_0040AssetObjects_0040_00406B_0040);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.ClassEntity*, void>)(&global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bdtor_007D), ptr4);
						throw;
					}
					return (global::AssetObjects.ClassEntity*)ptr4;
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr4, 304uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)13:
		{
			global::AssetObjects.CurveClass* ptr3 = (global::AssetObjects.CurveClass*)global::_003CModule_003E.@new(368uL);
			try
			{
				if (ptr3 != null)
				{
					global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bctor_007D((global::AssetObjects.ClassEntity*)ptr3, pkClassEntity);
					try
					{
						*(long*)ptr3 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7CurveClass_0040AssetObjects_0040_00406B_0040);
						global::_003CModule_003E.AssetObjects_002EContainer_003Cenum_0020AssetObjects_003A_003ACurveSegmentType_003E_002E_007Bctor_007D((Container_003Cenum_0020AssetObjects_003A_003ACurveSegmentType_003E*)((ulong)(nint)ptr3 + 304uL), (Container_003Cenum_0020AssetObjects_003A_003ACurveSegmentType_003E*)((ulong)(nint)pkClassEntity + 304uL));
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.ClassEntity*, void>)(&global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bdtor_007D), ptr3);
						throw;
					}
					return (global::AssetObjects.ClassEntity*)ptr3;
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr3, 368uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)15:
		{
			global::AssetObjects.FireFXClass* ptr2 = (global::AssetObjects.FireFXClass*)global::_003CModule_003E.@new(312uL);
			try
			{
				if (ptr2 != null)
				{
					global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bctor_007D((global::AssetObjects.ClassEntity*)ptr2, pkClassEntity);
					try
					{
						*(long*)ptr2 = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7FireFXClass_0040AssetObjects_0040_00406B_0040);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.ClassEntity*, void>)(&global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bdtor_007D), ptr2);
						throw;
					}
					return (global::AssetObjects.ClassEntity*)ptr2;
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr2, 312uL);
				throw;
			}
		}
		case (global::AssetObjects.ClassType)14:
		{
			global::AssetObjects.TupleClass* ptr = (global::AssetObjects.TupleClass*)global::_003CModule_003E.@new(304uL);
			try
			{
				if (ptr != null)
				{
					global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bctor_007D((global::AssetObjects.ClassEntity*)ptr, pkClassEntity);
					try
					{
						*(long*)ptr = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7TupleClass_0040AssetObjects_0040_00406B_0040);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.ClassEntity*, void>)(&global::_003CModule_003E.AssetObjects_002EClassEntity_002E_007Bdtor_007D), ptr);
						throw;
					}
					return (global::AssetObjects.ClassEntity*)ptr;
				}
				return null;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr, 304uL);
				throw;
			}
		}
		default:
			throw new Exception("IMPLEMENT ME!");
		}
	}

	internal unsafe static ClassEntity CreateClassEntity(global::AssetObjects.ClassEntity* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
	{
		return global::_003CModule_003E.AssetObjects_002EClassEntity_002EGetClassType(pkClassEntity) switch
		{
			(global::AssetObjects.ClassType)0 => new AssetClass((global::AssetObjects.AssetClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)1 => new MaterialClass((global::AssetObjects.MaterialClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)2 => new GeometryClass((global::AssetObjects.GeometryClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)3 => new TextureClass((global::AssetObjects.TextureClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)4 => new AnimationClass((global::AssetObjects.AnimationClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)5 => new DSGClass((global::AssetObjects.DSGClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)6 => new AnalyticLightClass((global::AssetObjects.AnalyticLightClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)7 => new EnvironmentLightClass((global::AssetObjects.EnvironmentLightClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)8 => new LightRigClass((global::AssetObjects.LightRigClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)9 => new ParticleEffectClass((global::AssetObjects.ParticleEffectClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)10 => new BehaviorClass((global::AssetObjects.BehaviorClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)11 => new SplineClass((global::AssetObjects.SplineClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)12 => new TriggerClass((global::AssetObjects.TriggerClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)13 => new CurveClass((global::AssetObjects.CurveClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)15 => new FireFXClass((global::AssetObjects.FireFXClass*)pkClassEntity, pkDeserializer), 
			(global::AssetObjects.ClassType)14 => new TupleClass((global::AssetObjects.TupleClass*)pkClassEntity, pkDeserializer), 
			_ => throw new Exception("IMPLEMENT ME!"), 
		};
	}

	internal unsafe void AddReferences()
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AClassEntity_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AClassEntity_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AClassEntity_003E_003A_003ADereferencer_003E obj);
		global::_003CModule_003E.AssetObjects_002EClassSet_002Ebegin(m_pkClassSet, &obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AClassEntity_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AClassEntity_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AClassEntity_003E_003A_003ADereferencer_003E obj2);
		global::_003CModule_003E.AssetObjects_002EClassSet_002Eend(m_pkClassSet, &obj2);
		if (!global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AClassEntity_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AClassEntity_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AClassEntity_003E_003A_003ADereferencer_003E_002E_0021_003D(&obj, &obj2))
		{
			return;
		}
		do
		{
			ClassEntity classEntity = CreateClassEntity(global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AClassEntity_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AClassEntity_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AClassEntity_003E_003A_003ADereferencer_003E_002E_002A(&obj), m_pkDeserializer);
			if (classEntity != null)
			{
				m_pmClassEntities.Add(classEntity);
			}
			global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AClassEntity_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AClassEntity_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AClassEntity_003E_003A_003ADereferencer_003E_002E_002B_002B(&obj);
		}
		while (global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AClassEntity_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AClassEntity_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AClassEntity_003E_003A_003ADereferencer_003E_002E_0021_003D(&obj, &obj2));
	}

	internal unsafe void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		//IL_0046: Expected I, but got I8
		//IL_004e: Expected I, but got I8
		List<IClassEntity>.Enumerator enumerator = m_pmClassEntities.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((ClassEntity)enumerator.Current).RemoveReferences(bDisposing);
			}
			while (enumerator.MoveNext());
		}
		m_pmClassEntities.Clear();
		if (bDisposing)
		{
			m_pkClassSet = null;
			m_pkDeserializer = null;
			m_pmClassEntities = null;
		}
	}

	private unsafe ClassEntity FindClassEntity(global::AssetObjects.ClassEntity* pkClassEntity)
	{
		if (pkClassEntity != null)
		{
			List<IClassEntity>.Enumerator enumerator = m_pmClassEntities.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					ClassEntity classEntity = (ClassEntity)enumerator.Current;
					if (classEntity.GetAssetObject() == pkClassEntity)
					{
						return classEntity;
					}
				}
				while (enumerator.MoveNext());
			}
		}
		return null;
	}
}
