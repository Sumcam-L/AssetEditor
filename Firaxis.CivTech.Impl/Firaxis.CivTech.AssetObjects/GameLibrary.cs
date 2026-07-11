using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class GameLibrary : IGameLibrary, IDisposable
{
	private unsafe global::AssetObjects.GameLibrary* m_lastLibrary;

	private GameArtSpecification m_parent;

	private string m_libraryName;

	private ISet<string> m_packagePaths;

	public unsafe virtual IEnumerable<string> RelativePackagePaths
	{
		get
		{
			global::AssetObjects.GameLibrary* ptr = m_parent.FindGameLibrary(m_libraryName);
			if (m_lastLibrary != ptr)
			{
				m_lastLibrary = ptr;
				CacheUnmanagedValues(ptr);
			}
			return m_packagePaths;
		}
	}

	public unsafe virtual string LibraryName
	{
		get
		{
			global::AssetObjects.GameLibrary* ptr = m_parent.FindGameLibrary(m_libraryName);
			if (m_lastLibrary != ptr)
			{
				m_lastLibrary = ptr;
				CacheUnmanagedValues(ptr);
			}
			return m_libraryName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			global::AssetObjects.GameLibrary* ptr = m_parent.FindGameLibrary(m_libraryName);
			m_libraryName = value;
			if (ptr != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EString_002EAssign((global::AssetObjects.String*)ptr, standardStringWrapper.Value);
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
	}

	public unsafe GameLibrary(GameArtSpecification parent, global::AssetObjects.GameLibrary* library)
	{
		//IL_003a: Expected I, but got I8
		//IL_0063: Expected I, but got I8
		m_lastLibrary = library;
		m_parent = parent;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x424d5b7e_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0GameLibrary_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVGameArtSpecification_0040234_0040PEAU12_0040_0040Z_00404_NA && library == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_07OPILGPEN_0040library_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040EPNPECGG_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 174u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x424d5b7e_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0GameLibrary_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVGameArtSpecification_0040234_0040PEAU12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0x424d5b7e_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0GameLibrary_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVGameArtSpecification_0040234_0040PEAU12_0040_0040Z_00404_NA && parent == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06MLKDMCBD_0040parent_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040EPNPECGG_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 175u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x424d5b7e_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0GameLibrary_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVGameArtSpecification_0040234_0040PEAU12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		CacheUnmanagedValues(library);
	}

	private void _007EGameLibrary()
	{
		_0021GameLibrary();
	}

	private unsafe void _0021GameLibrary()
	{
		//IL_001a: Expected I, but got I8
		m_packagePaths = null;
		m_libraryName = string.Empty;
		m_lastLibrary = null;
		m_parent = null;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool AddPath(string path)
	{
		//IL_0047: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (m_packagePaths.Add(path))
		{
			global::AssetObjects.GameLibrary* ptr = m_parent.FindGameLibrary(m_libraryName);
			if (ptr != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(path);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.String obj);
					global::AssetObjects.String* ptr2 = global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D(&obj, standardStringWrapper.Value);
					try
					{
						global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002Epush_back((Container_003CAssetObjects_003A_003AString_003E*)((ulong)(nint)ptr + 24uL), ptr2);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.String*, void>)(&global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D), &obj);
						throw;
					}
					global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D(&obj);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper).Dispose();
				return true;
			}
		}
		return false;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool RemovePath(string path)
	{
		//IL_003e: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (m_packagePaths.Remove(path))
		{
			global::AssetObjects.GameLibrary* ptr = m_parent.FindGameLibrary(m_libraryName);
			if (ptr != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(path);
				bool result;
				try
				{
					standardStringWrapper = standardStringWrapper2;
					result = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0x424d5b7e_002ERemoveStringFromContainer((Container_003CAssetObjects_003A_003AString_003E*)((ulong)(nint)ptr + 24uL), standardStringWrapper.Value);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper).Dispose();
				return result;
			}
		}
		return false;
	}

	public unsafe virtual void ClearPaths()
	{
		//IL_002a: Expected I, but got I8
		m_packagePaths.Clear();
		global::AssetObjects.GameLibrary* ptr = m_parent.FindGameLibrary(m_libraryName);
		if (ptr != null)
		{
			global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002Eclear((Container_003CAssetObjects_003A_003AString_003E*)((ulong)(nint)ptr + 24uL));
		}
	}

	private unsafe void UpdateCachedValuesIfNecessary(global::AssetObjects.GameLibrary* library)
	{
		if (m_lastLibrary != library)
		{
			m_lastLibrary = library;
			CacheUnmanagedValues(library);
		}
	}

	private unsafe void CacheUnmanagedValues(global::AssetObjects.GameLibrary* library)
	{
		//IL_0022: Expected I, but got I8
		if (library != null)
		{
			sbyte* value = global::_003CModule_003E.AssetObjects_002EString_002Ec_str((global::AssetObjects.String*)library);
			m_libraryName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(value);
			SortedSet<string> sortedSet = new SortedSet<string>();
			Container_003CAssetObjects_003A_003AString_003E* ptr = (Container_003CAssetObjects_003A_003AString_003E*)((ulong)(nint)library + 24uL);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator);
			global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002Ebegin(ptr, &iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator2);
			global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002Eend(ptr, &iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, &iterator2))
			{
				do
				{
					string item = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002A(&iterator)));
					sortedSet.Add(item);
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, &iterator2));
			}
			m_packagePaths = sortedSet;
		}
		else
		{
			m_packagePaths = new SortedSet<string>();
		}
	}

	private unsafe global::AssetObjects.GameLibrary* GetLibraryPointer()
	{
		return m_parent.FindGameLibrary(m_libraryName);
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EGameLibrary();
			return;
		}
		try
		{
			_0021GameLibrary();
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

	~GameLibrary()
	{
		Dispose(A_0: false);
	}
}
