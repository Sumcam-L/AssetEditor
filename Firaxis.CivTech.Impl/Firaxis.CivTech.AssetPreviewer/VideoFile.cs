using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using Firaxis.CivTech.AssetPreviewer.AviFileImpl;
using Platform;

namespace Firaxis.CivTech.AssetPreviewer;

public class VideoFile : IVideoFile
{
	private string m_filePath;

	private readonly IOStringWrapper m_filePathNative;

	private unsafe AviFile* m_filePointer;

	public virtual string FilePath => m_filePath;

	public unsafe VideoFile(string filePath, int frameRate, Size captureSize, long fccHandler, int compressionLevel)
	{
		//IL_0068: Expected I, but got I8
		m_filePath = filePath;
		IOStringWrapper filePathNative = new IOStringWrapper(filePath);
		try
		{
			m_filePathNative = filePathNative;
			base._002Ector();
			int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			AviFile* ptr = (AviFile*)global::_003CModule_003E.@new(272uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GG_0040MDBFGHLH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 143, 23, 0);
			AviFile* filePointer;
			try
			{
				filePointer = ((ptr == null) ? null : global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002EAviFileImpl_002EAviFile_002E_007Bctor_007D(ptr, m_filePathNative.RawValue, frameRate, captureSize.Width, captureSize.Height, fccHandler, compressionLevel));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GG_0040MDBFGHLH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 143, 23, 0);
				throw;
			}
			m_filePointer = filePointer;
			return;
		}
		catch
		{
			//try-fault
			((IDisposable)m_filePathNative).Dispose();
			throw;
		}
	}

	private void _007EVideoFile()
	{
		_0021VideoFile();
		GC.SuppressFinalize(this);
	}

	private unsafe void _0021VideoFile()
	{
		//IL_0026: Expected I, but got I8
		AviFile* filePointer = m_filePointer;
		if (filePointer != null)
		{
			AviFile* ptr = filePointer;
			global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002EAviFileImpl_002EAviFile_002E_007Bdtor_007D(ptr);
			global::_003CModule_003E.delete(ptr, 272uL);
			m_filePointer = null;
		}
	}

	public unsafe virtual void AddFrame(Bitmap bitmap)
	{
		//IL_0067: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x6a28d331_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddFrame_0040VideoFile_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVBitmap_0040Drawing_0040System_0040_0040_0040Z_00404_NA && bitmap == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040OKKKPACP_0040bitmap_003F5cannot_003F5be_003F5null_003F_0024CB_003F_0024AA_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040ELDNPJNA_0040bitmap_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GG_0040MDBFGHLH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 166u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x6a28d331_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddFrame_0040VideoFile_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVBitmap_0040Drawing_0040System_0040_0040_0040Z_00404_NA), (ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		if (!global::_003CModule_003E._003FA0x6a28d331_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FAddFrame_0040VideoFile_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVBitmap_0040Drawing_0040System_0040_0040_0040Z_00404_NA && m_filePointer == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040JMCJKHEO_0040m_filePointer_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GG_0040MDBFGHLH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 167u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x6a28d331_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FAddFrame_0040VideoFile_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVBitmap_0040Drawing_0040System_0040_0040_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
		BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		int byteCount = bitmapData.Stride * bitmapData.Height;
		void* bytes = bitmapData.Scan0.ToPointer();
		global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002EAviFileImpl_002EAviFile_002EAddFrame(m_filePointer, bytes, byteCount);
		bitmap.UnlockBits(bitmapData);
	}

	public virtual void SaveFile()
	{
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			try
			{
				_007EVideoFile();
				return;
			}
			finally
			{
				((IDisposable)m_filePathNative).Dispose();
			}
		}
		try
		{
			_0021VideoFile();
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

	~VideoFile()
	{
		Dispose(A_0: false);
	}
}
