using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class GeometryInstance : ImportedEntity, IGeometryInstance, IGeometryInstanceBuildable
{
	private List<IGeoMesh> m_pmGeoMeshes;

	private List<Lod> m_lods;

	internal unsafe global::AssetObjects.GeometryInstance* NativePointer => (global::AssetObjects.GeometryInstance*)m_pkEntity;

	public virtual IEnumerable<Lod> Lods => m_lods;

	public unsafe virtual IEnumerable<string> BoneNames
	{
		get
		{
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator);
			global::_003CModule_003E.AssetObjects_002EGeometryInstance_002Ebones_begin((global::AssetObjects.GeometryInstance*)m_pkEntity, &iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EGeometryInstance_002Ebones_end((global::AssetObjects.GeometryInstance*)m_pkEntity, &iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002D_003E(&iterator)));
					string item = Marshal.PtrToStringAnsi(ptr);
					list.Add(item);
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EGeometryInstance_002Ebones_end((global::AssetObjects.GeometryInstance*)m_pkEntity, &iterator2)));
			}
			return list;
		}
	}

	public virtual IEnumerable<IGeoMesh> GeometryMeshes => m_pmGeoMeshes;

	public unsafe virtual string ModelName
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EGeometryInstance_002EGetModelName((global::AssetObjects.GeometryInstance*)m_pkEntity));
			return Marshal.PtrToStringAnsi(ptr);
		}
	}

	internal unsafe GeometryInstance(global::AssetObjects.GeometryInstance* pkInstance, global::AssetObjects.VirtualPantry* pkVirtualPantry)
	{
		//IL_0017: Expected I, but got I8
		//IL_0017: Expected I, but got I8
		m_lods = new List<Lod>();
		base._002Ector((global::AssetObjects.InstanceEntity*)pkInstance, null, null, pkVirtualPantry);
		m_pmGeoMeshes = new List<IGeoMesh>();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AGeoMesh_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EGeometryInstance_002Emeshes_begin((global::AssetObjects.GeometryInstance*)m_pkEntity, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AGeoMesh_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGeoMesh_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EGeometryInstance_002Emeshes_end((global::AssetObjects.GeometryInstance*)m_pkEntity, &iterator2)))
		{
			do
			{
				GeoMesh item = new GeoMesh(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGeoMesh_002C4096_003E_002Eiterator_002E_002A(&iterator), this);
				m_pmGeoMeshes.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGeoMesh_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGeoMesh_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EGeometryInstance_002Emeshes_end((global::AssetObjects.GeometryInstance*)m_pkEntity, &iterator2)));
		}
	}

	public unsafe GeometryInstance(global::AssetObjects.GeometryInstance* pkInstance, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
	{
		m_pmGeoMeshes = new List<IGeoMesh>();
		m_lods = new List<Lod>();
		base._002Ector((global::AssetObjects.InstanceEntity*)pkInstance, pkDeserializer, pkSerializer, pkVirtualPantry);
	}

	public unsafe GeometryInstance(global::AssetObjects.InstanceSet* pkInstanceSet, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
	{
		m_pmGeoMeshes = new List<IGeoMesh>();
		m_lods = new List<Lod>();
		base._002Ector((global::AssetObjects.InstanceEntity*)global::_003CModule_003E.AssetObjects_002EInstanceSet_002EPush_003Cclass_0020AssetObjects_003A_003AGeometryInstance_003E(pkInstanceSet), pkDeserializer, pkSerializer, pkVirtualPantry);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool HasBone(string bone)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(bone).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002EGeometryInstance_002EHasBone((global::AssetObjects.GeometryInstance*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	private unsafe IGeoMesh AddGeometryMesh(global::AssetObjects.GeoMesh* pkGeoMesh)
	{
		if (pkGeoMesh != null)
		{
			GeoMesh geoMesh = new GeoMesh(pkGeoMesh, this);
			m_pmGeoMeshes.Add(geoMesh);
			return geoMesh;
		}
		return null;
	}

	public unsafe virtual IGeoMesh AddGeometryMesh(string meshName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(meshName).ToPointer();
		IGeoMesh result = AddGeometryMesh(global::_003CModule_003E.AssetObjects_002EGeometryInstance_002EAddMesh((global::AssetObjects.GeometryInstance*)m_pkEntity, ptr));
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	public unsafe virtual void AddBone(string bone)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(bone).ToPointer();
		global::_003CModule_003E.AssetObjects_002EGeometryInstance_002EAddBone((global::AssetObjects.GeometryInstance*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void SetModelName(string pmModelName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmModelName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EGeometryInstance_002ESetModelName((global::AssetObjects.GeometryInstance*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void ClearGeometryMeshes()
	{
		List<IGeoMesh>.Enumerator enumerator = m_pmGeoMeshes.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((GeoMesh)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pmGeoMeshes.Clear();
		global::_003CModule_003E.AssetObjects_002EGeometryInstance_002EClearGeometryMeshes((global::AssetObjects.GeometryInstance*)m_pkEntity);
	}

	public unsafe virtual void ClearBones()
	{
		global::_003CModule_003E.AssetObjects_002EGeometryInstance_002EClearBones((global::AssetObjects.GeometryInstance*)m_pkEntity);
	}

	public virtual void AddLod(Lod lod)
	{
		if (lod != null)
		{
			m_lods.Add(lod);
		}
	}

	public override void PublishStats(IDictionary<string, int> stats)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		List<IGeoMesh>.Enumerator enumerator = m_pmGeoMeshes.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				GeoMesh geoMesh = (GeoMesh)enumerator.Current;
				num += (int)geoMesh.VertexCount;
				num2 += (int)geoMesh.PrimitiveCount;
				num3 += (int)geoMesh.BoundBoneCount;
			}
			while (enumerator.MoveNext());
		}
		stats["Vertices"] = num;
		stats["Faces"] = num2;
		stats["Bones"] = BoneNames.Count();
		stats["Bound Bones"] = num3;
	}

	internal unsafe override void AddReferences()
	{
		base.AddReferences();
		global::AssetObjects.GeometryInstance* pkEntity = (global::AssetObjects.GeometryInstance*)m_pkEntity;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AGeoMesh_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EGeometryInstance_002Emeshes_begin(pkEntity, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AGeoMesh_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGeoMesh_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EGeometryInstance_002Emeshes_end(pkEntity, &iterator2)))
		{
			do
			{
				GeoMesh item = new GeoMesh(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGeoMesh_002C4096_003E_002Eiterator_002E_002A(&iterator), this);
				m_pmGeoMeshes.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGeoMesh_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AGeoMesh_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EGeometryInstance_002Emeshes_end(pkEntity, &iterator2)));
		}
	}

	internal override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		List<IGeoMesh>.Enumerator enumerator = m_pmGeoMeshes.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((GeoMesh)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pmGeoMeshes.Clear();
		if (bDisposing)
		{
			m_pmGeoMeshes = null;
		}
		base.RemoveReferences(bDisposing);
	}
}
