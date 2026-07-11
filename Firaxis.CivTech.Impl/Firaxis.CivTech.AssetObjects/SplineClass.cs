using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class SplineClass : ClassEntity, ISplineClass
{
	public unsafe virtual bool Is3D
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetObjects_002ESplineClass_002EIs3D((global::AssetObjects.SplineClass*)m_pkEntity);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetObjects_002ESplineClass_002ESet3D((global::AssetObjects.SplineClass*)m_pkEntity, value);
		}
	}

	public unsafe SplineClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003ASplineClass_003E(pkClassSet), pkDeserializer)
	{
	}

	public unsafe SplineClass(global::AssetObjects.SplineClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer)
	{
	}
}
