using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;
using Reflection;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ParameterSet : IParameterSet, IDisposable
{
	private List<IParameter> m_pmParameters;

	private unsafe global::AssetObjects.ParameterSet* m_pkParameterSet;

	public virtual IEnumerable<IParameter> Items => new List<IParameter>(m_pmParameters);

	public unsafe ParameterSet(global::AssetObjects.ParameterSet* pkParameterSet)
	{
		m_pmParameters = new List<IParameter>();
		m_pkParameterSet = pkParameterSet;
		base._002Ector();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AParameter_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AParameter_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AParameter_003E_003A_003ADereferencer_003E obj);
		global::_003CModule_003E.AssetObjects_002EParameterSet_002Ebegin(pkParameterSet, &obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AParameter_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AParameter_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AParameter_003E_003A_003ADereferencer_003E obj2);
		global::_003CModule_003E.AssetObjects_002EParameterSet_002Eend(pkParameterSet, &obj2);
		if (!global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AParameter_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AParameter_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AParameter_003E_003A_003ADereferencer_003E_002E_0021_003D(&obj, &obj2))
		{
			return;
		}
		do
		{
			IParameter parameter = CreateParameter(global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AParameter_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AParameter_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AParameter_003E_003A_003ADereferencer_003E_002E_002A(&obj));
			if (parameter != null)
			{
				m_pmParameters.Add(parameter);
			}
			global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AParameter_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AParameter_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AParameter_003E_003A_003ADereferencer_003E_002E_002B_002B(&obj);
		}
		while (global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AParameter_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AParameter_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AParameter_003E_003A_003ADereferencer_003E_002E_0021_003D(&obj, &obj2));
	}

	private void _007EParameterSet()
	{
		RemoveReferences();
	}

	private void _0021ParameterSet()
	{
		RemoveReferences();
	}

	public unsafe virtual T Push<T>(string pmParamName, InstanceType eInstanceType) where T : IObjectParameter
	{
		if (typeof(T) == typeof(IParameter))
		{
			return default(T);
		}
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmParamName).ToPointer();
		object[] array = new object[3];
		object obj = Pointer.Box(m_pkParameterSet, typeof(global::AssetObjects.ParameterSet*));
		array[0] = obj;
		object obj2 = Pointer.Box(ptr, typeof(sbyte*));
		array[1] = obj2;
		array[2] = eInstanceType;
		Type[] array2 = new Type[3];
		Type typeFromHandle = typeof(global::AssetObjects.ParameterSet*);
		array2[0] = typeFromHandle;
		Type typeFromHandle2 = typeof(sbyte*);
		array2[1] = typeFromHandle2;
		Type typeFromHandle3 = typeof(InstanceType);
		array2[2] = typeFromHandle3;
		T val = global::_003CModule_003E.Firaxis_002ECivTech_002EReflectionHelperEx_002E_003FA0x4726671b_002ETypeLoader<T>(Assembly.GetExecutingAssembly(), array, array2);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		m_pmParameters.Add(val);
		return val;
	}

	public unsafe virtual T Push<T>(string pmParamName) where T : IParameter
	{
		if (typeof(T) == typeof(IParameter))
		{
			return default(T);
		}
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmParamName).ToPointer();
		object[] array = new object[2];
		object obj = Pointer.Box(m_pkParameterSet, typeof(global::AssetObjects.ParameterSet*));
		array[0] = obj;
		object obj2 = Pointer.Box(ptr, typeof(sbyte*));
		array[1] = obj2;
		Type[] array2 = new Type[2];
		Type typeFromHandle = typeof(global::AssetObjects.ParameterSet*);
		array2[0] = typeFromHandle;
		Type typeFromHandle2 = typeof(sbyte*);
		array2[1] = typeFromHandle2;
		T val = global::_003CModule_003E.Firaxis_002ECivTech_002EReflectionHelperEx_002E_003FA0x4726671b_002ETypeLoader<T>(Assembly.GetExecutingAssembly(), array, array2);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		m_pmParameters.Add(val);
		return val;
	}

	public unsafe virtual T PushCollection<T>(string pmParamName, InstanceType eInstanceType) where T : IObjectCollectionParameter
	{
		if (typeof(T) == typeof(IParameter))
		{
			return default(T);
		}
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmParamName).ToPointer();
		object[] array = new object[3];
		object obj = Pointer.Box(m_pkParameterSet, typeof(global::AssetObjects.ParameterSet*));
		array[0] = obj;
		object obj2 = Pointer.Box(ptr, typeof(sbyte*));
		array[1] = obj2;
		array[2] = eInstanceType;
		Type[] array2 = new Type[3];
		Type typeFromHandle = typeof(global::AssetObjects.ParameterSet*);
		array2[0] = typeFromHandle;
		Type typeFromHandle2 = typeof(sbyte*);
		array2[1] = typeFromHandle2;
		Type typeFromHandle3 = typeof(InstanceType);
		array2[2] = typeFromHandle3;
		T val = global::_003CModule_003E.Firaxis_002ECivTech_002EReflectionHelperEx_002E_003FA0x4726671b_002ETypeLoader<T>(Assembly.GetExecutingAssembly(), array, array2);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		m_pmParameters.Add(val);
		return val;
	}

	public unsafe virtual void Clear()
	{
		List<IParameter>.Enumerator enumerator = m_pmParameters.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((Parameter)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		global::_003CModule_003E.AssetObjects_002EParameterSet_002Eclear(m_pkParameterSet);
		m_pmParameters.Clear();
	}

	public unsafe virtual void Remove(IParameter pmParameter)
	{
		int num = m_pmParameters.IndexOf(pmParameter);
		if (num >= 0)
		{
			Parameter parameter = (Parameter)pmParameter;
			global::_003CModule_003E.AssetObjects_002EParameterSet_002ERemove(m_pkParameterSet, parameter.GetAssetObject());
			parameter.RemoveReferences();
			m_pmParameters.RemoveAt(num);
			RefreshReferences();
		}
	}

	public unsafe virtual IParameter FindByName(string pmParamName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmParamName).ToPointer();
		Parameter result = FindParameter(global::_003CModule_003E.AssetObjects_002EParameterSet_002EFindByName(m_pkParameterSet, ptr));
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	public virtual List<IParameter> GetParameters()
	{
		return new List<IParameter>(m_pmParameters);
	}

	internal unsafe static Parameter CreateParameter(global::AssetObjects.Parameter* pkParameter)
	{
		//IL_0074: Expected I, but got I8
		//IL_0090: Expected I, but got I8
		//IL_0139: Expected I, but got I8
		switch (global::_003CModule_003E.AssetObjects_002EParameter_002EGetType(pkParameter))
		{
		case (global::AssetObjects.ParameterType)0:
			return new FloatParameter((global::AssetObjects.FloatParameter*)pkParameter);
		case (global::AssetObjects.ParameterType)1:
			return new BoolParameter((global::AssetObjects.BoolParameter*)pkParameter);
		case (global::AssetObjects.ParameterType)2:
			return new IntParameter((global::AssetObjects.IntParameter*)pkParameter);
		case (global::AssetObjects.ParameterType)3:
			return new RGBParameter((global::AssetObjects.RGBParameter*)pkParameter);
		case (global::AssetObjects.ParameterType)5:
			return new ObjectParameter((global::AssetObjects.ObjectParameter*)pkParameter);
		case (global::AssetObjects.ParameterType)4:
			if (global::_003CModule_003E.Reflection_002ETypeInfo_002EIsOfType(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::Reflection.TypeInfo*>)(*(ulong*)(*(ulong*)pkParameter)))((nint)pkParameter), global::_003CModule_003E.AssetObjects_002EEnumParameter_002EGetTypeInfo()))
			{
				return new EnumParameter((global::AssetObjects.EnumParameter*)pkParameter);
			}
			if (global::_003CModule_003E.Reflection_002ETypeInfo_002EIsOfType(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::Reflection.TypeInfo*>)(*(ulong*)(*(ulong*)pkParameter)))((nint)pkParameter), global::_003CModule_003E.AssetObjects_002EDynamicEnumParameter_002EGetTypeInfo()))
			{
				return new DynamicEnumParameter((global::AssetObjects.DynamicEnumParameter*)pkParameter);
			}
			if (!global::_003CModule_003E._003FA0x4726671b_002E_003FbIgnoreAlways_0040_003FO_0040_003F_003FCreateParameter_0040ParameterSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SMPE_0024AAVParameter_0040345_0040PEAV63_0040_0040Z_00404_NA)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CJ_0040EGLNCPBB_0040Update_003F5this_003F5when_003F5you_003F5add_003F5a_003F5new_003F5e_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040OIJLPA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 173u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x4726671b_002E_003FbIgnoreAlways_0040_003FO_0040_003F_003FCreateParameter_0040ParameterSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SMPE_0024AAVParameter_0040345_0040PEAV63_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			return null;
		case (global::AssetObjects.ParameterType)6:
			return new Coord2DParameter((global::AssetObjects.Coord2DParameter*)pkParameter);
		case (global::AssetObjects.ParameterType)7:
			return new Coord3DParameter((global::AssetObjects.Coord3DParameter*)pkParameter);
		case (global::AssetObjects.ParameterType)8:
			return new BLPEntryParameter((global::AssetObjects.BLPEntryParameter*)pkParameter);
		case (global::AssetObjects.ParameterType)9:
			return new ArtDefRefParameter((ArtDefReferenceParameter*)pkParameter);
		case (global::AssetObjects.ParameterType)10:
			return new StringParameter((global::AssetObjects.StringParameter*)pkParameter);
		case (global::AssetObjects.ParameterType)11:
			return CreateCollecionParameter((global::AssetObjects.CollectionParameter*)pkParameter);
		case (global::AssetObjects.ParameterType)12:
			return new CurveParameter((global::AssetObjects.CurveParameter*)pkParameter);
		case (global::AssetObjects.ParameterType)13:
			return new TupleParameter((global::AssetObjects.TupleParameter*)pkParameter);
		default:
			if (!global::_003CModule_003E._003FA0x4726671b_002E_003FbIgnoreAlways_0040_003FBF_0040_003F_003FCreateParameter_0040ParameterSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SMPE_0024AAVParameter_0040345_0040PEAV63_0040_0040Z_00404_NA && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_01GBGANLPD_00400_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040OIJLPA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 194u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x4726671b_002E_003FbIgnoreAlways_0040_003FBF_0040_003F_003FCreateParameter_0040ParameterSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040SMPE_0024AAVParameter_0040345_0040PEAV63_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return null;
		}
	}

	internal unsafe global::AssetObjects.ParameterSet* GetParameterSet()
	{
		return m_pkParameterSet;
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0042: Expected I, but got I8
		List<IParameter>.Enumerator enumerator = m_pmParameters.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((Parameter)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pmParameters.Clear();
		m_pkParameterSet = null;
	}

	internal unsafe void SetNativeParameterSet(global::AssetObjects.ParameterSet* pkParamSet)
	{
		m_pkParameterSet = pkParamSet;
		RefreshReferences();
	}

	internal unsafe void RefreshReferences()
	{
		uint num = 0u;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AParameter_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AParameter_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AParameter_003E_003A_003ADereferencer_003E obj);
		global::_003CModule_003E.AssetObjects_002EParameterSet_002Ebegin(m_pkParameterSet, &obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out IteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AParameter_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AParameter_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AParameter_003E_003A_003ADereferencer_003E obj2);
		if (global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AParameter_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AParameter_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AParameter_003E_003A_003ADereferencer_003E_002E_0021_003D(&obj, global::_003CModule_003E.AssetObjects_002EParameterSet_002Eend(m_pkParameterSet, &obj2)))
		{
			do
			{
				((Parameter)m_pmParameters[(int)num]).SetNativeParameter(global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AParameter_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AParameter_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AParameter_003E_003A_003ADereferencer_003E_002E_002A(&obj));
				num++;
				global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AParameter_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AParameter_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AParameter_003E_003A_003ADereferencer_003E_002E_002B_002B(&obj);
			}
			while (global::_003CModule_003E.Types_002EIteratorWrapper_003CTypes_003A_003AChunkedVector_003CAssetObjects_003A_003AParameter_0020_002A_002C256_003E_003A_003Aconst_iterator_002CAssetObjects_003A_003AParameter_002CAssetObjects_003A_003APolymorphicContainer_003CAssetObjects_003A_003AParameter_003E_003A_003ADereferencer_003E_002E_0021_003D(&obj, global::_003CModule_003E.AssetObjects_002EParameterSet_002Eend(m_pkParameterSet, &obj2)));
		}
	}

	private unsafe static Parameter CreateCollecionParameter(global::AssetObjects.CollectionParameter* pkParameter)
	{
		//IL_00c0: Expected I, but got I8
		switch (global::_003CModule_003E.AssetObjects_002ECollectionParameter_002EGetEntryParameterType(pkParameter))
		{
		case (global::AssetObjects.ParameterType)0:
			return new FloatCollectionParameter(pkParameter);
		case (global::AssetObjects.ParameterType)1:
			return new BoolCollectionParameter(pkParameter);
		case (global::AssetObjects.ParameterType)2:
			return new IntCollectionParameter(pkParameter);
		case (global::AssetObjects.ParameterType)3:
			return new RGBCollectionParameter(pkParameter);
		case (global::AssetObjects.ParameterType)5:
			return new ObjectCollectionParameter(pkParameter);
		case (global::AssetObjects.ParameterType)4:
			return new EnumCollectionParameter(pkParameter);
		case (global::AssetObjects.ParameterType)6:
			return new Coord2DCollectionParameter(pkParameter);
		case (global::AssetObjects.ParameterType)7:
			return new Coord3DCollectionParameter(pkParameter);
		case (global::AssetObjects.ParameterType)8:
			return new BLPEntryCollectionParameter(pkParameter);
		case (global::AssetObjects.ParameterType)9:
			return new ArtDefRefCollectionParameter(pkParameter);
		case (global::AssetObjects.ParameterType)10:
			return new StringCollectionParameter(pkParameter);
		case (global::AssetObjects.ParameterType)13:
			return new TupleCollectionParameter(pkParameter);
		case (global::AssetObjects.ParameterType)12:
			return null;
		case (global::AssetObjects.ParameterType)11:
			return null;
		default:
			if (!global::_003CModule_003E._003FA0x4726671b_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FCreateCollecionParameter_0040ParameterSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040CMPE_0024AAVParameter_0040345_0040PEAVCollectionParameter_00403_0040_0040Z_00404_NA && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_01GBGANLPD_00400_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040OIJLPA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 234u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x4726671b_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FCreateCollecionParameter_0040ParameterSet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040CMPE_0024AAVParameter_0040345_0040PEAVCollectionParameter_00403_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return null;
		}
	}

	private unsafe Parameter FindParameter(global::AssetObjects.Parameter* pkParameter)
	{
		if (pkParameter != null)
		{
			List<IParameter>.Enumerator enumerator = m_pmParameters.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					Parameter parameter = (Parameter)enumerator.Current;
					if (parameter.GetAssetObject() == pkParameter)
					{
						return parameter;
					}
				}
				while (enumerator.MoveNext());
			}
		}
		return null;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EParameterSet();
			return;
		}
		try
		{
			_0021ParameterSet();
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

	~ParameterSet()
	{
		Dispose(A_0: false);
	}
}
