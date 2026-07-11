using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class GeoMesh : IGeoMesh
{
	private List<IGeoPrimGroup> m_pmGeoPrimGroups;

	private unsafe global::AssetObjects.GeoMesh* m_pkGeoMesh;

	private IGeometryInstance m_pGeo;

	public virtual IGeometryInstance Geo => m_pGeo;

	public virtual IEnumerable<IGeoPrimGroup> GeoPrimGroups => m_pmGeoPrimGroups;

	public unsafe virtual uint BoundBoneCount
	{
		get
		{
			return global::_003CModule_003E.AssetObjects_002EGeoMesh_002EGetBoundBoneCount(m_pkGeoMesh);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EGeoMesh_002ESetBoundBoneCount(m_pkGeoMesh, value);
		}
	}

	public unsafe virtual uint PrimitiveCount
	{
		get
		{
			return global::_003CModule_003E.AssetObjects_002EGeoMesh_002EGetPrimitiveCount(m_pkGeoMesh);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EGeoMesh_002ESetPrimitiveCount(m_pkGeoMesh, value);
		}
	}

	public unsafe virtual uint VertexCount
	{
		get
		{
			return global::_003CModule_003E.AssetObjects_002EGeoMesh_002EGetVertexCount(m_pkGeoMesh);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EGeoMesh_002ESetVertexCount(m_pkGeoMesh, value);
		}
	}

	public unsafe virtual string Name
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EGeoMesh_002EGetName(m_pkGeoMesh));
			return Marshal.PtrToStringAnsi(ptr);
		}
	}

	private unsafe IGeoPrimGroup AddGeoPrimGroup(global::AssetObjects.GeoPrimGroup* pkGeoMesh)
	{
		if (pkGeoMesh != null)
		{
			List<IGeoPrimGroup>.Enumerator enumerator = m_pmGeoPrimGroups.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					IGeoPrimGroup current = enumerator.Current;
					if (((GeoPrimGroup)current).GetAssetObject() == pkGeoMesh)
					{
						return current;
					}
				}
				while (enumerator.MoveNext());
			}
			GeoPrimGroup geoPrimGroup = new GeoPrimGroup(pkGeoMesh, m_pGeo);
			m_pmGeoPrimGroups.Add(geoPrimGroup);
			return geoPrimGroup;
		}
		return null;
	}

	public unsafe virtual IGeoPrimGroup AddGeoPrimGroup(string name, uint numFirstPrim, uint numPrims)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(name).ToPointer();
		IGeoPrimGroup result = AddGeoPrimGroup(global::_003CModule_003E.AssetObjects_002EGeoMesh_002EAddPrimGroup(m_pkGeoMesh, ptr, numFirstPrim, numPrims));
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	public unsafe GeoMesh(global::AssetObjects.GeoMesh* pkGeoMesh, IGeometryInstance geo)
	{
		m_pmGeoPrimGroups = new List<IGeoPrimGroup>();
		m_pkGeoMesh = pkGeoMesh;
		m_pGeo = geo;
		base._002Ector();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AGeoPrimGroup_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EGeoMesh_002Egroups_begin(m_pkGeoMesh, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AGeoPrimGroup_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGeoPrimGroup_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EGeoMesh_002Egroups_end(m_pkGeoMesh, &iterator2)))
		{
			do
			{
				m_pmGeoPrimGroups.Add(new GeoPrimGroup(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGeoPrimGroup_002C4096_003E_002Eiterator_002E_002A(&iterator), m_pGeo));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGeoPrimGroup_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGeoPrimGroup_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EGeoMesh_002Egroups_end(m_pkGeoMesh, &iterator2)));
		}
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0037: Expected I, but got I8
		List<IGeoPrimGroup>.Enumerator enumerator = m_pmGeoPrimGroups.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((GeoPrimGroup)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pkGeoMesh = null;
	}

	internal unsafe global::AssetObjects.GeoMesh* GetAssetObject()
	{
		return m_pkGeoMesh;
	}
}
