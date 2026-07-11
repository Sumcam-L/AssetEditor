using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using Firaxis.MathEx;
using std;

namespace Firaxis.Granny;

public class GrannyFileLoader : IGrannyFileLoader
{
	private Dictionary<uint, IntPtr> m_kStringDatabaseHash;

	public GrannyFileLoader()
	{
		m_kStringDatabaseHash = new Dictionary<uint, IntPtr>();
	}

	private void _007EGrannyFileLoader()
	{
	}

	private void _0021GrannyFileLoader()
	{
	}

	private unsafe IGrannyFile LoadGrannyFile(string szFilename, Stream kStream)
	{
		int num = (int)kStream.Length;
		if (num == 0)
		{
			throw new IOException($"Granny file {szFilename} loaded but has a length of 0.");
		}
		byte[] array = new byte[num];
		kStream.Read(array, 0, num);
		fixed (byte* ptr = &array[0])
		{
			granny_file* ptr2 = global::_003CModule_003E.GrannyReadEntireFileFromMemory(num, ptr);
			if (ptr2 == null)
			{
				throw new IOException("Unable to load granny file");
			}
			if (!RemapFileStrings(ptr2))
			{
				throw new Exception("Unable to demangle-strings");
			}
			granny_file_info* ptr3 = ((!global::_003CModule_003E.GrannyIsMODFile(ptr2)) ? global::_003CModule_003E.GrannyGetFileInfo(ptr2) : global::_003CModule_003E.GrannyGetModFileInfo(ptr2));
			if (ptr3 == null)
			{
				throw new Exception("Unable to convert granny file to dag2");
			}
			System.Runtime.CompilerServices.Unsafe.SkipInit(out basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E obj);
			global::_003CModule_003E.msclr_002Einterop_002Emarshal_as_003Cclass_0020std_003A_003Abasic_string_003Cchar_002Cstruct_0020std_003A_003Achar_traits_003Cchar_003E_002Cclass_0020std_003A_003Aallocator_003Cchar_003E_0020_003E_002Cclass_0020System_003A_003AString_0020_005E_003E(&obj, &szFilename);
			GrannyFile grannyFile;
			IGrannyFile result;
			try
			{
				grannyFile = new GrannyFile();
				if (!grannyFile.Attach(global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002Ec_str(&obj), ptr2, ptr3))
				{
					result = null;
					goto IL_00c0;
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E*, void>)(&global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
			global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D(&obj);
			return grannyFile;
			IL_00c0:
			global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D(&obj);
			return result;
		}
	}

	public unsafe virtual IGrannyFile LoadGrannyFile(string file)
	{
		//IL_0008: Expected I8, but got I
		//IL_006d: Expected I, but got I8
		//IL_003a: Expected I, but got I8
		//IL_004a: Expected I, but got I8
		//long num = (nint)stackalloc byte[global::_003CModule_003E.__CxxQueryExceptionSize()];
        byte* num = stackalloc byte[global::_003CModule_003E.__CxxQueryExceptionSize()];
        try
		{
			if (File.Exists(file))
			{
				Stream kStream = new FileStream(file, FileMode.Open, FileAccess.Read);
				return LoadGrannyFile(file, kStream);
			}
		}
		catch when (global::_003CModule_003E.__CxxExceptionFilter((void*)Marshal.GetExceptionPointers(), System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_R0_003FAVexception_0040stdext_0040_0040_00408), 9, null) != 0)
		{
			uint num2 = 0u;
			global::_003CModule_003E.__CxxRegisterExceptionObject((void*)Marshal.GetExceptionPointers(), (void*)num);
			try
			{
				try
				{
					goto end_IL_004b;
				}
				catch when (((Func<bool>)delegate
				{
					// Could not convert BlockContainer to single expression
					num2 = (uint)global::_003CModule_003E.__CxxDetectRethrow((void*)Marshal.GetExceptionPointers());
					return (byte)num2 != 0;
				}).Invoke())
				{
				}
				if (num2 != 0)
				{
					throw;
				}
				end_IL_004b:;
			}
			finally
			{
				global::_003CModule_003E.__CxxUnregisterExceptionObject((void*)num, (int)num2);
			}
		}
		return null;
	}

	public unsafe virtual IGrannyFile CreateEmptyGrannyFile(string file)
	{
		//IL_0021: Expected I, but got I8
		//IL_0021: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E obj);
		global::_003CModule_003E.msclr_002Einterop_002Emarshal_as_003Cclass_0020std_003A_003Abasic_string_003Cchar_002Cstruct_0020std_003A_003Achar_traits_003Cchar_003E_002Cclass_0020std_003A_003Aallocator_003Cchar_003E_0020_003E_002Cclass_0020System_003A_003AString_0020_005E_003E(&obj, &file);
		IGrannyFile result;
		try
		{
			GrannyFile grannyFile = new GrannyFile();
			if (!grannyFile.Attach(global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002Ec_str(&obj), null, null))
			{
				result = null;
			}
			else
			{
				grannyFile.Source = "Tool";
				result = grannyFile;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E*, void>)(&global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D), &obj);
			throw;
		}
		global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D(&obj);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	private unsafe bool LoadStringDatabase(Stream kStream)
	{
		//IL_007b: Expected I, but got I8
		//IL_00af: Expected I, but got I8
		int num = (int)kStream.Length;
		byte[] array = new byte[num];
		kStream.Read(array, 0, num);
		fixed (byte* ptr = &array[0])
		{
			granny_file* ptr2 = global::_003CModule_003E.GrannyReadEntireFileFromMemory(num, ptr);
			if (ptr2 == null)
			{
				throw new IOException("Unable to load string database");
			}
			granny_string_database* ptr3 = global::_003CModule_003E.GrannyGetStringDatabase(ptr2);
			if (ptr3 == null)
			{
				global::_003CModule_003E.GrannyFreeFile(ptr2);
				return false;
			}
			CRC32 cRC = new CRC32();
			int num2 = 0;
			if (0 < *(int*)ptr3)
			{
				long num3 = 0L;
				do
				{
					string text = new string((sbyte*)(*(ulong*)(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)ptr3 + 4L)) + num3)));
					uint num4 = cRC.Calc(Encoding.ASCII.GetBytes(text));
					if (!m_kStringDatabaseHash.ContainsKey(num4))
					{
						IntPtr value = new IntPtr((void*)(*(ulong*)(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)ptr3 + 4L)) + num3)));
						m_kStringDatabaseHash.Add(num4, value);
					}
					else
					{
						IntPtr value2 = default(IntPtr);
						if (m_kStringDatabaseHash.TryGetValue(num4, out value2))
						{
							string text2 = new string((sbyte*)value2.ToPointer());
							if (text2.CompareTo(text) != 0)
							{
								throw new InvalidOperationException($"Two strings have the same hash value, but different values. Hash = {num4}, String #1 = {text2}, String #2 = {text}");
							}
						}
					}
					num2++;
					num3 += 8;
				}
				while (num2 < *(int*)ptr3);
			}
			return true;
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool LoadStringDatabase(string file)
	{
		Stream kStream = new FileStream(file, FileMode.Open, FileAccess.Read);
		return LoadStringDatabase(kStream);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	private unsafe bool RemapFileStrings(granny_file* pkFile)
	{
		if (pkFile != null)
		{
			switch ((uint)(*(int*)(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)pkFile + 4L)) + 56)))
			{
			case 2u:
			{
				granny_file_info* ptr = ((!global::_003CModule_003E.GrannyIsMODFile(pkFile)) ? global::_003CModule_003E.GrannyGetFileInfo(pkFile) : global::_003CModule_003E.GrannyGetModFileInfo(pkFile));
				System.Runtime.CompilerServices.Unsafe.SkipInit(out GFLStringHashWrapper gFLStringHashWrapper);
				global::_003CModule_003E.gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E_002E_007Bctor_007D((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E*)(&gFLStringHashWrapper));
				bool result;
				try
				{
					global::_003CModule_003E.gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E_002E_003D((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E*)(&gFLStringHashWrapper), m_kStringDatabaseHash);
					result = global::_003CModule_003E.GrannyRebasePointersStringCallback(global::_003CModule_003E.GrannyFileInfoType, ptr, 0L, (delegate* <void*, uint, sbyte*>)global::_003CModule_003E.__unep_0040_003FGrannyFileLoaderRebaseToStringHash_0040_003FA0x7673ad18_0040Granny_0040Firaxis_0040_0040_0024_0024FYAPEADPEAXI_0040Z, &gFLStringHashWrapper);
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<GFLStringHashWrapper*, void>)(&global::_003CModule_003E.Firaxis_002EGranny_002EGFLStringHashWrapper_002E_007Bdtor_007D), &gFLStringHashWrapper);
					throw;
				}
				global::_003CModule_003E.gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E_002E_007Bdtor_007D((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E*)(&gFLStringHashWrapper));
				return result;
			}
			default:
				return false;
			case 0u:
				return true;
			}
		}
		return false;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (!A_0)
		{
			try
			{
			}
			finally
			{
				
			}
		}
	}

	public virtual  void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~GrannyFileLoader()
	{
		Dispose(A_0: false);
	}
}
