using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ArtConsumer : IArtConsumer, IDisposable
{
	private unsafe global::AssetObjects.ArtConsumer* m_lastArtConsumer;

	private GameArtSpecification m_parent;

	private string m_consumerName;

	private bool m_loadsLibraries;

	private ISet<string> m_artDefPaths;

	private ISet<string> m_libraryReferences;

	public unsafe virtual bool LoadsLibraries
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			global::AssetObjects.ArtConsumer* ptr = m_parent.FindArtConsumer(m_consumerName);
			if (m_lastArtConsumer != ptr)
			{
				m_lastArtConsumer = ptr;
				CacheUnmanagedValues(ptr);
			}
			return m_loadsLibraries;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			m_loadsLibraries = value;
			global::AssetObjects.ArtConsumer* ptr = m_parent.FindArtConsumer(m_consumerName);
			if (ptr != null)
			{
				*(bool*)((ulong)(nint)ptr + 152uL) = value;
			}
		}
	}

	public unsafe virtual IEnumerable<string> ReferencedLibraries
	{
		get
		{
			global::AssetObjects.ArtConsumer* ptr = m_parent.FindArtConsumer(m_consumerName);
			if (m_lastArtConsumer != ptr)
			{
				m_lastArtConsumer = ptr;
				CacheUnmanagedValues(ptr);
			}
			return m_libraryReferences;
		}
		set
		{
			//IL_001a: Expected I, but got I8
			m_libraryReferences.Clear();
			global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002Eclear((Container_003CAssetObjects_003A_003AString_003E*)((ulong)(nint)m_lastArtConsumer + 88uL));
			foreach (string item in value)
			{
				AddLibrary(item);
			}
		}
	}

	public unsafe virtual IEnumerable<string> RelativeArtDefPaths
	{
		get
		{
			global::AssetObjects.ArtConsumer* ptr = m_parent.FindArtConsumer(m_consumerName);
			if (m_lastArtConsumer != ptr)
			{
				m_lastArtConsumer = ptr;
				CacheUnmanagedValues(ptr);
			}
			return m_artDefPaths;
		}
		set
		{
			//IL_002a: Expected I, but got I8
			m_artDefPaths.Clear();
			global::AssetObjects.ArtConsumer* ptr = m_parent.FindArtConsumer(m_consumerName);
			if (ptr != null)
			{
				global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002Eclear((Container_003CAssetObjects_003A_003AString_003E*)((ulong)(nint)ptr + 24uL));
			}
			foreach (string item in value)
			{
				AddArtDefPath(item);
			}
		}
	}

	public unsafe virtual string ConsumerName
	{
		get
		{
			global::AssetObjects.ArtConsumer* ptr = m_parent.FindArtConsumer(m_consumerName);
			if (m_lastArtConsumer != ptr)
			{
				m_lastArtConsumer = ptr;
				CacheUnmanagedValues(ptr);
			}
			return m_consumerName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			global::AssetObjects.ArtConsumer* ptr = m_parent.FindArtConsumer(m_consumerName);
			m_consumerName = value;
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

	public unsafe ArtConsumer(GameArtSpecification parent, global::AssetObjects.ArtConsumer* artConsumer)
	{
		//IL_003a: Expected I, but got I8
		//IL_0063: Expected I, but got I8
		m_lastArtConsumer = artConsumer;
		m_parent = parent;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x424d5b7e_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ArtConsumer_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVGameArtSpecification_0040234_0040PEAU12_0040_0040Z_00404_NA && artConsumer == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0M_0040HNLBEAPL_0040artConsumer_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040EPNPECGG_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 307u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x424d5b7e_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ArtConsumer_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVGameArtSpecification_0040234_0040PEAU12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0x424d5b7e_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0ArtConsumer_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVGameArtSpecification_0040234_0040PEAU12_0040_0040Z_00404_NA && parent == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06MLKDMCBD_0040parent_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040EPNPECGG_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 308u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x424d5b7e_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0ArtConsumer_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVGameArtSpecification_0040234_0040PEAU12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		CacheUnmanagedValues(artConsumer);
	}

	private void _007EArtConsumer()
	{
		_0021ArtConsumer();
	}

	private unsafe void _0021ArtConsumer()
	{
		//IL_0021: Expected I, but got I8
		m_libraryReferences = null;
		m_artDefPaths = null;
		m_consumerName = string.Empty;
		m_lastArtConsumer = null;
		m_parent = null;
	}

	public unsafe virtual void AddArtDefPath(string path)
	{
		//IL_0038: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (!m_artDefPaths.Add(path))
		{
			return;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(path);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetObjects.ArtConsumer* ptr = m_parent.FindArtConsumer(m_consumerName);
			if (ptr != null)
			{
				global::AssetObjects.ArtConsumer* ptr2 = (global::AssetObjects.ArtConsumer*)((ulong)(nint)ptr + 24uL);
				global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0x424d5b7e_002ERemoveStringFromContainer((Container_003CAssetObjects_003A_003AString_003E*)ptr2, standardStringWrapper.Value);
				System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.String obj);
				global::AssetObjects.String* ptr3 = global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D(&obj, standardStringWrapper.Value);
				try
				{
					global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002Epush_back((Container_003CAssetObjects_003A_003AString_003E*)ptr2, ptr3);
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.String*, void>)(&global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D), &obj);
					throw;
				}
				global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D(&obj);
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool RemoveArtDefPath(string path)
	{
		//IL_004b: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (m_artDefPaths.Contains(path))
		{
			m_artDefPaths.Remove(path);
			global::AssetObjects.ArtConsumer* ptr = m_parent.FindArtConsumer(m_consumerName);
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

	public unsafe virtual void ClearRelativeArtDefs()
	{
		//IL_002a: Expected I, but got I8
		m_artDefPaths.Clear();
		global::AssetObjects.ArtConsumer* ptr = m_parent.FindArtConsumer(m_consumerName);
		if (ptr != null)
		{
			global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002Eclear((Container_003CAssetObjects_003A_003AString_003E*)((ulong)(nint)ptr + 24uL));
		}
	}

	public unsafe virtual void AddLibrary(string libraryName)
	{
		if (m_libraryReferences.Add(libraryName))
		{
			global::AssetObjects.ArtConsumer* ptr = m_parent.FindArtConsumer(m_consumerName);
			if (ptr != null)
			{
				global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0x424d5b7e_002EAddLibraryIfUnique(ptr, libraryName);
			}
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool RemoveLibrary(string libraryName)
	{
		//IL_004b: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (m_libraryReferences.Contains(libraryName))
		{
			m_libraryReferences.Remove(libraryName);
			global::AssetObjects.ArtConsumer* ptr = m_parent.FindArtConsumer(m_consumerName);
			if (ptr != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(libraryName);
				bool result;
				try
				{
					standardStringWrapper = standardStringWrapper2;
					result = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0x424d5b7e_002ERemoveStringFromContainer((Container_003CAssetObjects_003A_003AString_003E*)((ulong)(nint)ptr + 88uL), standardStringWrapper.Value);
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

	public unsafe virtual void ClearLibraries()
	{
		//IL_002a: Expected I, but got I8
		m_libraryReferences.Clear();
		global::AssetObjects.ArtConsumer* ptr = m_parent.FindArtConsumer(m_consumerName);
		if (ptr != null)
		{
			global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002Eclear((Container_003CAssetObjects_003A_003AString_003E*)((ulong)(nint)ptr + 88uL));
		}
	}

	private unsafe void UpdateCachedValuesIfNecessary(global::AssetObjects.ArtConsumer* consumer)
	{
		if (m_lastArtConsumer != consumer)
		{
			m_lastArtConsumer = consumer;
			CacheUnmanagedValues(consumer);
		}
	}

	private unsafe void CacheUnmanagedValues(global::AssetObjects.ArtConsumer* consumer)
	{
		//IL_0036: Expected I, but got I8
		//IL_0095: Expected I, but got I8
		if (consumer != null)
		{
			sbyte* value = global::_003CModule_003E.AssetObjects_002EString_002Ec_str((global::AssetObjects.String*)consumer);
			m_consumerName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(value);
			m_loadsLibraries = *(bool*)((ulong)(nint)consumer + 152uL);
			SortedSet<string> sortedSet = new SortedSet<string>();
			Container_003CAssetObjects_003A_003AString_003E* ptr = (Container_003CAssetObjects_003A_003AString_003E*)((ulong)(nint)consumer + 24uL);
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
			m_artDefPaths = sortedSet;
			SortedSet<string> sortedSet2 = new SortedSet<string>();
			Container_003CAssetObjects_003A_003AString_003E* ptr2 = (Container_003CAssetObjects_003A_003AString_003E*)((ulong)(nint)consumer + 88uL);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator3);
			global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002Ebegin(ptr2, &iterator3);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator4);
			global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003AString_003E_002Eend(ptr2, &iterator4);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator3, &iterator4))
			{
				do
				{
					string item2 = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002A(&iterator3)));
					sortedSet2.Add(item2);
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002B_002B(&iterator3);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator3, &iterator4));
			}
			m_libraryReferences = sortedSet2;
		}
		else
		{
			m_artDefPaths = new SortedSet<string>();
			m_libraryReferences = new SortedSet<string>();
			m_loadsLibraries = false;
		}
	}

	private unsafe global::AssetObjects.ArtConsumer* GetConsumerPointer()
	{
		return m_parent.FindArtConsumer(m_consumerName);
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EArtConsumer();
			return;
		}
		try
		{
			_0021ArtConsumer();
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

	~ArtConsumer()
	{
		Dispose(A_0: false);
	}
}
