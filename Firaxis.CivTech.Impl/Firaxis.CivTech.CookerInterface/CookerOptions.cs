using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using AssetCookerHelpers;
using AssetObjects;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;
using Platform;
using Serialization;
using String;
using Types;

namespace Firaxis.CivTech.CookerInterface;

public class CookerOptions : ICookerOptions
{
	private SortedSet<string> m_XLPs;

	private SortedSet<string> m_artDefs;

	private SortedSet<string> m_artSpecificationFiles;

	private unsafe global::AssetObjects.Deserializer* m_deserializer;

	private unsafe AssetCookerHelpers.ICookerOptions* m_options;

	internal unsafe AssetCookerHelpers.ICookerOptions* Options => m_options;

	public unsafe virtual bool AppendLogging
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EGetAppendLogging(m_options);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetAppendLogging(m_options, value);
		}
	}

	public unsafe virtual bool AllArtDefs
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EGetAllArtDefs(m_options);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetAllArtDefs(m_options, value);
		}
	}

	public unsafe virtual bool AllXLPs
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EGetAllXLPs(m_options);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetAllXLPs(m_options, value);
		}
	}

	public unsafe virtual bool MultithreadingEnabled
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			//IL_0012: Expected I, but got I8
			AssetCookerHelpers.ICookerOptions* options = m_options;
			return ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, byte>)(*(ulong*)(*(long*)options + 40)))((nint)options) != 0;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetMultithreadingEnabled(m_options, value);
		}
	}

	public unsafe virtual bool LogBLPStats
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			//IL_0012: Expected I, but got I8
			AssetCookerHelpers.ICookerOptions* options = m_options;
			return ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, byte>)(*(ulong*)(*(long*)options + 32)))((nint)options) != 0;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetLogBLPStats(m_options, value);
		}
	}

	public unsafe virtual uint TempHeapSize
	{
		get
		{
			//IL_0012: Expected I, but got I8
			AssetCookerHelpers.ICookerOptions* options = m_options;
			return ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, uint>)(*(ulong*)(*(long*)options + 24)))((nint)options);
		}
		set
		{
			global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetTempHeapSize(m_options, value);
		}
	}

	public unsafe virtual string Layout
	{
		get
		{
			//IL_0012: Expected I, but got I8
			AssetCookerHelpers.ICookerOptions* options = m_options;
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*>)(*(ulong*)(*(long*)options + 16)))((nint)options));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetLayout(m_options, standardStringWrapper.Value);
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

	public unsafe virtual string DependencyOutputRoot
	{
		get
		{
			//IL_0012: Expected I, but got I8
			AssetCookerHelpers.ICookerOptions* options = m_options;
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, char*>)(*(ulong*)(*(long*)options + 104)))((nint)options));
		}
		set
		{
			IOStringWrapper iOStringWrapper = null;
			IOStringWrapper iOStringWrapper2 = new IOStringWrapper(value);
			try
			{
				iOStringWrapper = iOStringWrapper2;
				global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetDependencyOutputDirectory(m_options, iOStringWrapper.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)iOStringWrapper).Dispose();
				throw;
			}
			((IDisposable)iOStringWrapper).Dispose();
		}
	}

	public unsafe virtual string ArtDefDestinationRoot
	{
		get
		{
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EGetArtDefOutputRoot(m_options)));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetArtDefOutputRoot(m_options, standardStringWrapper.Value);
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

	public unsafe virtual string ShaderDefRoot
	{
		get
		{
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040));
		}
		set
		{
		}
	}

	public unsafe virtual string ConfigPath
	{
		get
		{
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EGetConfig(m_options)));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetConfig(m_options, standardStringWrapper.Value);
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

	public unsafe virtual IEnumerable<string> PantryRoots
	{
		get
		{
			//IL_0018: Expected I, but got I8
			IList<string> list = new List<string>();
			AssetCookerHelpers.ICookerOptions* options = m_options;
			global::AssetObjects.VirtualPantry* ptr = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.VirtualPantry*>)(*(ulong*)(*(long*)options + 72)))((nint)options);
			int num = 0;
			if (0 < global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EGetNumPantryRoots(ptr))
			{
				do
				{
					char* value = global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EGetPantryRoot(ptr, num);
					list.Add(global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(value));
					num++;
				}
				while (num < global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EGetNumPantryRoots(ptr));
			}
			return list;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EClearPantryRoots(m_options);
			foreach (string item in value)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(item);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EAddPantry(m_options, standardStringWrapper.Value);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper).Dispose();
			}
			FixupManagedFilesToCook();
		}
	}

	public unsafe virtual string PackageRoot
	{
		get
		{
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EGetStewpot(m_options)));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetStewpot(m_options, standardStringWrapper.Value);
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

	public unsafe virtual CookerMode Mode
	{
		get
		{
			//IL_0012: Expected I, but got I8
			AssetCookerHelpers.ICookerOptions* options = m_options;
			int result;
			switch (((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, AssetCookerHelpers.CookerMode>)(*(ulong*)(*(long*)options + 56)))((nint)options))
			{
			case (AssetCookerHelpers.CookerMode)0:
				return CookerMode.XLP;
			default:
				result = 0;
				break;
			case (AssetCookerHelpers.CookerMode)1:
				result = 2;
				break;
			}
			return (CookerMode)result;
		}
		set
		{
			switch (value)
			{
			case CookerMode.ArtDef:
				global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetCookerMode(m_options, (AssetCookerHelpers.CookerMode)1);
				break;
			case CookerMode.XLP:
				global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetCookerMode(m_options, (AssetCookerHelpers.CookerMode)0);
				break;
			case CookerMode.ArtSpecification:
				global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetCookerMode(m_options, (AssetCookerHelpers.CookerMode)2);
				break;
			}
		}
	}

	public unsafe virtual Firaxis.CivTech.AssetObjects.Platforms Platform
	{
		get
		{
			//IL_0012: Expected I, but got I8
			AssetCookerHelpers.ICookerOptions* options = m_options;
			global::AssetObjects.Platforms platforms = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.Platforms>)(*(ulong*)(*(long*)options + 48)))((nint)options);
			Firaxis.CivTech.AssetObjects.Platforms platforms2 = Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_INVALID;
			platforms2 = (((platforms & (global::AssetObjects.Platforms)1u) != 0) ? Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_WINDOWS : platforms2);
			if ((platforms & (global::AssetObjects.Platforms)2u) != 0)
			{
				platforms2 |= Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_IOS;
			}
			if ((platforms & (global::AssetObjects.Platforms)4u) != 0)
			{
				platforms2 |= Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_MACOS;
			}
			if ((platforms & (global::AssetObjects.Platforms)8u) != 0)
			{
				platforms2 |= Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_LINUX;
			}
			if ((platforms & (global::AssetObjects.Platforms)16u) != 0)
			{
				platforms2 |= Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_XBONE;
			}
			if ((platforms & (global::AssetObjects.Platforms)64u) != 0)
			{
				platforms2 |= Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_PS4;
			}
			if ((platforms & (global::AssetObjects.Platforms)128u) != 0)
			{
				platforms2 |= Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_STADIA;
			}
			if ((platforms & (global::AssetObjects.Platforms)32u) != 0)
			{
				platforms2 |= Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_SWITCH;
			}
			return platforms2;
		}
		set
		{
			global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetPlatform(m_options, (global::AssetObjects.Platforms)value);
			if (PackageRoot.Contains("{PLATFORM}"))
			{
				string text = string.Empty;
				switch (value)
				{
				case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_WINDOWS:
					text = "Windows";
					break;
				case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_IOS:
					text = "iOS";
					break;
				case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_MACOS:
					text = "MacOS";
					break;
				case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_LINUX:
					text = "Linux";
					break;
				case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_XBONE:
					text = "XBone";
					break;
				case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_PS4:
					text = "PS4";
					break;
				case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_SWITCH:
					text = "Switch";
					break;
				case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_STADIA:
					text = "Stadia";
					break;
				}
				if (text.Length > 0)
				{
					PackageRoot = PackageRoot.Replace("{PLATFORM}", text);
				}
			}
		}
	}

	public unsafe virtual int NumCores
	{
		get
		{
			//IL_0011: Expected I, but got I8
			AssetCookerHelpers.ICookerOptions* options = m_options;
			return ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, int>)(*(ulong*)(*(long*)options + 8)))((nint)options);
		}
		set
		{
			global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetCores(m_options, value);
		}
	}

	public virtual IEnumerable<string> FilesToCook
	{
		get
		{
			if (Mode == CookerMode.XLP)
			{
				return m_XLPs;
			}
			if (Mode == CookerMode.ArtDef)
			{
				return m_artDefs;
			}
			if (Mode == CookerMode.ArtSpecification)
			{
				return m_artSpecificationFiles;
			}
			return Enumerable.Empty<string>();
		}
	}

	public virtual ICollection<string> ArtSpecificationFiles => m_artSpecificationFiles;

	public virtual ICollection<string> ArtDefs => m_artDefs;

	public virtual ICollection<string> XLPs => m_XLPs;

	public unsafe virtual bool UseAbsolutePaths
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			//IL_000e: Expected I, but got I8
			AssetCookerHelpers.ICookerOptions* options = m_options;
			return ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, byte>)(*(ulong*)(*(ulong*)options)))((nint)options) != 0;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetInputPathsAreAbsolute(m_options, value);
		}
	}

	public unsafe virtual bool IsCustomCook
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EGetIsCustomCook(m_options);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetIsCustomCook(m_options, value);
		}
	}

	public unsafe virtual string ProfileName
	{
		get
		{
			return global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EGetProfileName(m_options));
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetProfileName(m_options, standardStringWrapper.Value);
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

	public unsafe CookerOptions(IEnumerable<string> pantryPaths, string projectName)
	{
		IOStringWrapper iOStringWrapper = null;
		StandardStringWrapper standardStringWrapper = null;
		this._002Ector();
		try
		{
			foreach (string pantryPath in pantryPaths)
			{
				IOStringWrapper iOStringWrapper2 = new IOStringWrapper(pantryPath);
				try
				{
					iOStringWrapper = iOStringWrapper2;
					global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EAddPantry(m_options, iOStringWrapper.Value);
				}
				catch
				{
					//try-fault
					((IDisposable)iOStringWrapper).Dispose();
					throw;
				}
				((IDisposable)iOStringWrapper).Dispose();
			}
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(projectName);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002ESetProjectName(m_options, standardStringWrapper.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
			return;
		}
		catch
		{
			//try-fault
			((IDisposable)this).Dispose();
			throw;
		}
	}

	public unsafe CookerOptions()
	{
		//IL_0051: Expected I, but got I8
		//IL_009a: Expected I, but got I8
		m_XLPs = new SortedSet<string>();
		m_artDefs = new SortedSet<string>();
		m_artSpecificationFiles = new SortedSet<string>();
		int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		global::AssetObjects.Deserializer* ptr = (global::AssetObjects.Deserializer*)global::_003CModule_003E.@new(384uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GH_0040KDEMAMGB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 18, 49, 0);
		global::AssetObjects.Deserializer* deserializer;
		try
		{
			deserializer = ((ptr == null) ? null : global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bctor_007D(ptr));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GH_0040KDEMAMGB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 18, 49, 0);
			throw;
		}
		m_deserializer = deserializer;
		int num2 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		AssetCookerHelpers.ICookerOptions* ptr2 = (AssetCookerHelpers.ICookerOptions*)global::_003CModule_003E.@new(640uL, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GH_0040KDEMAMGB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 17, 49, 0);
		AssetCookerHelpers.ICookerOptions* options;
		try
		{
			options = ((ptr2 == null) ? null : global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002E_007Bctor_007D(ptr2));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr2, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GH_0040KDEMAMGB_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 17, 49, 0);
			throw;
		}
		m_options = options;
		base._002Ector();
	}

	private void _007ECookerOptions()
	{
		_0021CookerOptions();
	}

	private unsafe void _0021CookerOptions()
	{
		//IL_0026: Expected I, but got I8
		//IL_0040: Expected I, but got I8
		AssetCookerHelpers.ICookerOptions* options = m_options;
		if (options != null)
		{
			AssetCookerHelpers.ICookerOptions* ptr = options;
			global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002E_007Bdtor_007D(ptr);
			global::_003CModule_003E.delete(ptr, 640uL);
			m_options = null;
		}
		global::AssetObjects.Deserializer* deserializer = m_deserializer;
		if (deserializer != null)
		{
			global::_003CModule_003E.AssetObjects_002EDeserializer_002E__delDtor(deserializer, 1u);
			m_deserializer = null;
		}
	}

	public unsafe virtual string SerializeIntoXML()
	{
		ReconcileFilesToCook();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out Serializer serializer);
		global::_003CModule_003E.Serialization_002EContext_002E_007Bctor_007D((Context*)(&serializer));
		try
		{
			global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)));
			System.Runtime.CompilerServices.Unsafe.As<Serializer, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), &serializer);
			throw;
		}
		string result;
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.String obj);
			global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D(&obj);
			try
			{
				global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeIntoString_003Cclass_0020AssetCookerHelpers_003A_003AICookerOptions_003E(&serializer, &obj, m_options);
				result = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(&obj));
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
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Serializer*, void>)(&global::_003CModule_003E.AssetObjects_002ESerializer_002E_007Bdtor_007D), &serializer);
			throw;
		}
		global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&serializer));
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool DeserializeFromXML(string xml)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(xml);
		bool result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out global::Platform.ResultCode resultCode);
			result = global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserialize_003Cclass_0020AssetCookerHelpers_003A_003AICookerOptions_003E(m_deserializer, &resultCode, m_options, standardStringWrapper.Value));
			FixupManagedFilesToCook();
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

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool SerializeIntoFile(string path)
	{
		IOStringWrapper iOStringWrapper = null;
		ReconcileFilesToCook();
		IOStringWrapper iOStringWrapper2 = new IOStringWrapper(path);
		bool result;
		try
		{
			iOStringWrapper = iOStringWrapper2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out Serializer serializer);
			global::_003CModule_003E.Serialization_002EContext_002E_007Bctor_007D((Context*)(&serializer));
			try
			{
				global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)));
				System.Runtime.CompilerServices.Unsafe.As<Serializer, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), &serializer);
				throw;
			}
			try
			{
				result = global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeToFile_003Cclass_0020AssetCookerHelpers_003A_003AICookerOptions_003E(&serializer, m_options, iOStringWrapper.Value);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Serializer*, void>)(&global::_003CModule_003E.AssetObjects_002ESerializer_002E_007Bdtor_007D), &serializer);
				throw;
			}
			global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&serializer));
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		((IDisposable)iOStringWrapper).Dispose();
		return result;
	}

	public unsafe virtual Firaxis.Error.ResultCode DeserializeFromFile(string path)
	{
		IOStringWrapper iOStringWrapper = null;
		IOStringWrapper iOStringWrapper2 = new IOStringWrapper(path);
		Firaxis.Error.ResultCode result;
		try
		{
			iOStringWrapper = iOStringWrapper2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out global::Platform.ResultCode resultCode);
			global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserializeFromFile_003Cclass_0020AssetCookerHelpers_003A_003AICookerOptions_003E(m_deserializer, &resultCode, m_options, iOStringWrapper.Value);
			if (!global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(&resultCode))
			{
				result = new Firaxis.Error.ResultCode(new string(global::_003CModule_003E.Platform_002EResultCode_002EGetMessage(&resultCode)));
				goto IL_004b;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		Firaxis.Error.ResultCode success;
		try
		{
			FixupManagedFilesToCook();
			success = Firaxis.Error.ResultCode.Success;
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		((IDisposable)iOStringWrapper).Dispose();
		return success;
		IL_004b:
		((IDisposable)iOStringWrapper).Dispose();
		return result;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("--mode {0} ", Mode);
		stringBuilder.AppendFormat("--cores {0} ", NumCores);
		if (!MultithreadingEnabled)
		{
			object[] args = new object[0];
			stringBuilder.AppendFormat("--no_mt ", args);
		}
		if (LogBLPStats)
		{
			object[] args2 = new object[0];
			stringBuilder.AppendFormat("--log_sizes ", args2);
		}
		string arg = null;
		switch (Platform)
		{
		case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_WINDOWS:
			arg = "Windows";
			break;
		case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_MACOS:
			arg = "MacOS";
			break;
		case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_IOS:
			arg = "iOS";
			break;
		case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_LINUX:
			arg = "Linux";
			break;
		case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_XBONE:
			arg = "XBone";
			break;
		case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_PS4:
			arg = "PS4";
			break;
		case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_STADIA:
			arg = "Stadia";
			break;
		case Firaxis.CivTech.AssetObjects.Platforms.PLATFORM_SWITCH:
			arg = "Switch";
			break;
		}
		stringBuilder.AppendFormat("--platform {0} ", arg);
		if (!string.IsNullOrEmpty(ConfigPath))
		{
			stringBuilder.AppendFormat("--config {0} ", ConfigPath);
		}
		if (PantryRoots.Any())
		{
			stringBuilder.Append("-- pantry ");
			foreach (string pantryRoot in PantryRoots)
			{
				stringBuilder.AppendFormat("{0} ", pantryRoot);
			}
		}
		if (Mode == CookerMode.XLP)
		{
			if (!string.IsNullOrEmpty(PackageRoot))
			{
				stringBuilder.AppendFormat("--stewpot {0} ", PackageRoot);
			}
			if (!string.IsNullOrEmpty(Layout))
			{
				stringBuilder.AppendFormat("--layout {0} ", Layout);
			}
			if (!string.IsNullOrEmpty(ShaderDefRoot))
			{
				stringBuilder.AppendFormat("--shaders {0} ", ShaderDefRoot);
			}
			foreach (string xLP in XLPs)
			{
				stringBuilder.AppendFormat("{0} ", xLP);
			}
		}
		else if (Mode == CookerMode.ArtDef)
		{
			if (!string.IsNullOrEmpty(ArtDefDestinationRoot))
			{
				stringBuilder.AppendFormat("--banquet_hall {0} ", ArtDefDestinationRoot);
			}
			foreach (string artDef in ArtDefs)
			{
				stringBuilder.AppendFormat("{0} ", artDef);
			}
		}
		return stringBuilder.ToString();
	}

	internal unsafe void ReconcileFilesToCook()
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = null;
		global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EClearXLPs(m_options);
		SortedSet<string>.Enumerator enumerator = m_XLPs.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(enumerator.Current);
				try
				{
					standardStringWrapper = standardStringWrapper4;
					global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EAddXLP(m_options, standardStringWrapper.Value);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper).Dispose();
			}
			while (enumerator.MoveNext());
		}
		global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EClearArtDefs(m_options);
		SortedSet<string>.Enumerator enumerator2 = m_artDefs.GetEnumerator();
		if (enumerator2.MoveNext())
		{
			do
			{
				StandardStringWrapper standardStringWrapper5 = new StandardStringWrapper(enumerator2.Current);
				try
				{
					standardStringWrapper2 = standardStringWrapper5;
					global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EAddArtDef(m_options, standardStringWrapper2.Value);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper2).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper2).Dispose();
			}
			while (enumerator2.MoveNext());
		}
		global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EClearArtSpecifications(m_options);
		SortedSet<string>.Enumerator enumerator3 = m_artSpecificationFiles.GetEnumerator();
		if (!enumerator3.MoveNext())
		{
			return;
		}
		do
		{
			StandardStringWrapper standardStringWrapper6 = new StandardStringWrapper(enumerator3.Current);
			try
			{
				standardStringWrapper3 = standardStringWrapper6;
				global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002EAddArtSpecificationFile(m_options, standardStringWrapper3.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper3).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper3).Dispose();
		}
		while (enumerator3.MoveNext());
	}

	private unsafe void FixupManagedFilesToCook()
	{
		m_XLPs.Clear();
		m_artDefs.Clear();
		string text = PantryRoots.FirstOrDefault();
		if (AllXLPs)
		{
			if (!string.IsNullOrEmpty(text))
			{
				string text2 = Path.Combine(text, "XLPs");
				if (Directory.Exists(text2))
				{
					string[] files = Directory.GetFiles(text2, "*.xlp", SearchOption.AllDirectories);
					int num = 0;
					if (0 < (nint)files.LongLength)
					{
						do
						{
							string text3 = files[num].Replace(text2, string.Empty);
							text3 = text3.TrimStart(Path.DirectorySeparatorChar);
							text3 = text3.TrimStart(Path.AltDirectorySeparatorChar);
							m_XLPs.Add(text3);
							num++;
						}
						while (num < (nint)files.LongLength);
					}
				}
			}
		}
		else
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002C4096_003E.iterator iterator);
			global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002Exlp_begin(m_options, &iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002C4096_003E.iterator iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002Exlp_end(m_options, &iterator2)))
			{
				do
				{
					string item = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)global::_003CModule_003E.Types_002EChunkedVector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002C4096_003E_002Eiterator_002E_002D_003E(&iterator)));
					m_XLPs.Add(item);
					global::_003CModule_003E.Types_002EChunkedVector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002Exlp_end(m_options, &iterator2)));
			}
		}
		if (AllArtDefs)
		{
			if (!string.IsNullOrEmpty(text))
			{
				string text4 = Path.Combine(text, "ArtDefs");
				if (Directory.Exists(text4))
				{
					string[] files2 = Directory.GetFiles(text4, "*.artdef", SearchOption.AllDirectories);
					int num2 = 0;
					if (0 < (nint)files2.LongLength)
					{
						do
						{
							string text5 = files2[num2].Replace(text4, string.Empty);
							text5 = text5.TrimStart(Path.DirectorySeparatorChar);
							text5 = text5.TrimStart(Path.AltDirectorySeparatorChar);
							m_artDefs.Add(text5);
							num2++;
						}
						while (num2 < (nint)files2.LongLength);
					}
				}
			}
		}
		else
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002C4096_003E.iterator iterator3);
			global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002Eart_def_begin(m_options, &iterator3);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002C4096_003E.iterator iterator4);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002C4096_003E_002Eiterator_002E_0021_003D(&iterator3, global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002Eart_def_end(m_options, &iterator4)))
			{
				do
				{
					string item2 = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)global::_003CModule_003E.Types_002EChunkedVector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002C4096_003E_002Eiterator_002E_002D_003E(&iterator3)));
					m_artDefs.Add(item2);
					global::_003CModule_003E.Types_002EChunkedVector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002C4096_003E_002Eiterator_002E_002B_002B(&iterator3);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002C4096_003E_002Eiterator_002E_0021_003D(&iterator3, global::_003CModule_003E.AssetCookerHelpers_002EICookerOptions_002Eart_def_end(m_options, &iterator4)));
			}
		}
		global::_003CModule_003E.Firaxis_002ECivTech_002ECookerInterface_002E_003FA0x4bcd4891_002EFixupArtSpecificationFiles((INewCookerOptions*)m_options, m_artSpecificationFiles);
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007ECookerOptions();
			return;
		}
		try
		{
			_0021CookerOptions();
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

	~CookerOptions()
	{
		Dispose(A_0: false);
	}
}
