using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Platform;
using Primitives;
using TextureExport;

namespace Firaxis.CivTech.TextureExport;

public class LooseFileExporter : ILooseFileExporter
{
	private void _007ELooseFileExporter()
	{
	}

	private void _0021LooseFileExporter()
	{
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool ExportAsDDS(string inFilePath, string outFilePath, IExportSettingsParams settings, ref string errorMessage)
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out HeapAllocator_003C0_002C0_003E heapAllocator_003C0_002C0_003E);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out HeapAllocator_003C9_002C0_003E heapAllocator_003C9_002C0_003E);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
		ExportSettingsParams exportSettingsParams;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer2);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out TextureData textureData);
		if (!string.IsNullOrEmpty(inFilePath) && !string.IsNullOrEmpty(outFilePath) && settings != null)
		{
			global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)(&heapAllocator_003C0_002C0_003E));
			*(long*)(&heapAllocator_003C0_002C0_003E) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240A_0040_00240A_0040_0040Platform_0040_00406B_0040);
			global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)(&heapAllocator_003C9_002C0_003E));
			*(long*)(&heapAllocator_003C9_002C0_003E) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_002408_00240A_0040_0040Platform_0040_00406B_0040);
			global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer);
			try
			{
				exportSettingsParams = (ExportSettingsParams)settings;
				global::_003CModule_003E.Platform_002EMemoryBuffer_002EInit(&memoryBuffer, (Allocator*)(&heapAllocator_003C0_002C0_003E), 0uL);
				sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(inFilePath).ToPointer();
				bool flag = global::_003CModule_003E.Platform_002EReadCompleteFile(ptr, &memoryBuffer) == (IO_RESULT)0;
				IntPtr hglobal = new IntPtr(ptr);
				Marshal.FreeHGlobal(hglobal);
				if (!flag)
				{
					errorMessage = "DDS Export failed because the input file could not be opened.";
					goto IL_00a5;
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
				throw;
			}
			try
			{
				global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer2);
				try
				{
					bool flag2;
					if (StringComparer.OrdinalIgnoreCase.Compare(Path.GetExtension(inFilePath), ".tga") == 0)
					{
						flag2 = global::_003CModule_003E.TextureLib_002EInitTextureDataFromTGA(&textureData, global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer), &memoryBuffer2, (Allocator*)(&heapAllocator_003C0_002C0_003E));
						goto IL_0174;
					}
					if (StringComparer.OrdinalIgnoreCase.Compare(Path.GetExtension(inFilePath), ".png") == 0)
					{
						flag2 = global::_003CModule_003E.TextureLib_002EInitTextureDataFromPNG(&textureData, global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer), (uint)global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetOffset(&memoryBuffer), &memoryBuffer2, (Allocator*)(&heapAllocator_003C0_002C0_003E));
						goto IL_0174;
					}
					if (StringComparer.OrdinalIgnoreCase.Compare(Path.GetExtension(inFilePath), ".dds") == 0)
					{
						global::_003CModule_003E.Platform_002EMemoryBuffer_002EInit(&memoryBuffer2, (Allocator*)(&heapAllocator_003C0_002C0_003E), 0uL);
						global::_003CModule_003E.Platform_002EMemoryBuffer_002EGrowToFit(&memoryBuffer2, global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetCapacity(&memoryBuffer));
						global::_003CModule_003E.Platform_002EMemoryBuffer_002EAppend(&memoryBuffer2, global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer), global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetCapacity(&memoryBuffer));
						flag2 = global::_003CModule_003E.TextureLib_002EInitTextureDataFromDDS(&textureData, global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer2));
						goto IL_0174;
					}
					goto end_IL_00b7;
					IL_0174:
					global::_003CModule_003E.Platform_002EMemoryBuffer_002ERelease(&memoryBuffer);
					if (flag2)
					{
						goto IL_01b9;
					}
					errorMessage = "Export failed because a texture could not be initialized from the input file.";
					goto IL_0198;
					end_IL_00b7:;
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer2);
					throw;
				}
				goto end_IL_00af;
				IL_0198:
				global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer2);
				goto IL_01af;
				end_IL_00af:;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
				throw;
			}
			try
			{
				try
				{
					errorMessage = "The Loose File Exporter only supports PNG, TGA, and DDS file formats.";
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer2);
					throw;
				}
				global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer2);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
				throw;
			}
			global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
			return false;
		}
		throw new ArgumentNullException("ExportAsDDS requires all data fields be non-null.");
		IL_01af:
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
		return false;
		IL_01b9:
		System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer3);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out TextureData textureData2);
		try
		{
			try
			{
				global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer3);
				try
				{
					ExportResult exportResult = global::_003CModule_003E.TextureExport_002EExport(&textureData, exportSettingsParams.GetExportSettings(), &textureData2, (Allocator*)(&heapAllocator_003C0_002C0_003E), (Allocator*)(&heapAllocator_003C9_002C0_003E), &memoryBuffer3);
					if (exportResult != (ExportResult)8)
					{
						errorMessage = "Failed when attempting to create new texture.  Reason: " + GetEnumName(exportResult);
						goto IL_0202;
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer3);
					throw;
				}
				goto end_IL_01b9;
				IL_0202:
				global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer3);
				goto IL_0219;
				end_IL_01b9:;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer2);
				throw;
			}
			goto end_IL_01b9_2;
			IL_0219:
			global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer2);
			goto IL_0230;
			end_IL_01b9_2:;
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
			throw;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer4);
		try
		{
			try
			{
				try
				{
					global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer4);
					try
					{
						global::_003CModule_003E.Platform_002EMemoryBuffer_002EInit(&memoryBuffer4, (Allocator*)(&heapAllocator_003C9_002C0_003E), 0uL);
						if (!global::_003CModule_003E.TextureLib_002ECreateDDSInMemory(&textureData2, &memoryBuffer4))
						{
							errorMessage = "Export failed because the exported texture could not be converted to a DDS texture.";
							goto IL_0272;
						}
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer4);
						throw;
					}
					goto end_IL_023a;
					IL_0272:
					global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer4);
					goto IL_0289;
					end_IL_023a:;
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer3);
					throw;
				}
				goto end_IL_023a_2;
				IL_0289:
				global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer3);
				goto IL_02a0;
				end_IL_023a_2:;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer2);
				throw;
			}
			goto end_IL_023a_3;
			IL_02a0:
			global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer2);
			goto IL_02b7;
			end_IL_023a_3:;
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
			throw;
		}
		bool flag3;
		try
		{
			try
			{
				try
				{
					try
					{
						sbyte* ptr2 = (sbyte*)Marshal.StringToHGlobalAnsi(outFilePath).ToPointer();
						flag3 = global::_003CModule_003E.Platform_002EWriteCompleteFile(ptr2, &memoryBuffer4) == (IO_RESULT)0;
						IntPtr hglobal2 = new IntPtr(ptr2);
						Marshal.FreeHGlobal(hglobal2);
						if (!flag3)
						{
							errorMessage = "Export failed because the DDS file could not be written to disk.";
							goto IL_030a;
						}
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer4);
						throw;
					}
					goto end_IL_02c1;
					IL_030a:
					global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer4);
					goto IL_0321;
					end_IL_02c1:;
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer3);
					throw;
				}
				goto end_IL_02c1_2;
				IL_0321:
				global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer3);
				goto IL_0338;
				end_IL_02c1_2:;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer2);
				throw;
			}
			goto end_IL_02c1_3;
			IL_0338:
			global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer2);
			goto IL_034f;
			end_IL_02c1_3:;
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
			throw;
		}
		try
		{
			try
			{
				try
				{
					global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer4);
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer3);
					throw;
				}
				global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer3);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer2);
				throw;
			}
			global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer2);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
			throw;
		}
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
		return flag3;
		IL_0230:
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
		return false;
		IL_034f:
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
		return false;
		IL_00a5:
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
		return false;
		IL_02b7:
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
		return false;
	}

	internal void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool disposing)
	{
	}

	private string GetEnumName(ExportResult result)
	{
		return result switch
		{
			(ExportResult)1 => "3D Export requires slab width and height parameters to be set.", 
			(ExportResult)3 => "Slab height and width are greater than texture height and width.", 
			(ExportResult)4 => "Color key export requires that color key parameters be set.", 
			(ExportResult)5 => "The size of the color key is larger than the size of the texture.", 
			(ExportResult)6 => "Cube map export selected, but the texture does not have the dimensions of width = 6 * height.", 
			(ExportResult)7 => "Manual Mip Map mode has been selected but no mip maps are being generated..", 
			(ExportResult)11 => "Export failed when converting the input texture to the output texture.", 
			_ => "", 
		};
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
				base.Finalize();
			}
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~LooseFileExporter()
	{
		Dispose(A_0: false);
	}
}
