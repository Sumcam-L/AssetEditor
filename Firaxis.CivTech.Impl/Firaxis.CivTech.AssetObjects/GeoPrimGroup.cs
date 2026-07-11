using System;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class GeoPrimGroup : IGeoPrimGroup
{
	private unsafe global::AssetObjects.GeoPrimGroup* m_pkGeoPrimGroup;

	private IGeometryInstance m_pGeo;

	public virtual IGeometryInstance Geo => m_pGeo;

	public unsafe virtual uint NumPrims => global::_003CModule_003E.AssetObjects_002EGeoPrimGroup_002EGetPrimitiveCount(m_pkGeoPrimGroup);

	public unsafe virtual uint NumFirstPrim => global::_003CModule_003E.AssetObjects_002EGeoPrimGroup_002EGetFirstPrimitive(m_pkGeoPrimGroup);

	public unsafe virtual string Name
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EGeoPrimGroup_002EGetName(m_pkGeoPrimGroup));
			return Marshal.PtrToStringAnsi(ptr);
		}
	}

	public unsafe GeoPrimGroup(global::AssetObjects.GeoPrimGroup* pkGeoPrimGroup, IGeometryInstance geo)
	{
		m_pkGeoPrimGroup = pkGeoPrimGroup;
		m_pGeo = geo;
		base._002Ector();
	}

	internal unsafe global::AssetObjects.GeoPrimGroup* GetAssetObject()
	{
		return m_pkGeoPrimGroup;
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pkGeoPrimGroup = null;
	}
}
