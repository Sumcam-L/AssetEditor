using System;
using System.Runtime.CompilerServices;

namespace SharpDX;

internal class Interop
{
	public unsafe static void* Fixed<T>(ref T data)
	{
		throw new NotImplementedException();
	}

	public unsafe static void* Fixed<T>(T[] data)
	{
		throw new NotImplementedException();
	}

	public unsafe static void* Cast<T>(ref T data) where T : struct
	{
		return System.Runtime.CompilerServices.Unsafe.AsPointer(ref data);
	}

	public unsafe static void* CastOut<T>(out T data) where T : struct
	{
		return System.Runtime.CompilerServices.Unsafe.AsPointer(ref data);
	}

	public static TCAST[] CastArray<TCAST, T>(T[] arrayData) where TCAST : struct where T : struct
	{
		return (TCAST[])(object)arrayData;
	}

	public unsafe static void memcpy(void* pDest, void* pSrc, int count)
	{
		// IL cpblk instruction
		System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(pDest, pSrc, count);
	}

	public unsafe static void memset(void* pDest, byte value, int count)
	{
		// IL initblk instruction
		System.Runtime.CompilerServices.Unsafe.InitBlockUnaligned(pDest, value, count);
	}

	public unsafe static void* Read<T>(void* pSrc, ref T data) where T : struct
	{
		fixed (T* ptr = &data)
		{
			ref _003F reference = ref *(_003F*)ptr;
			int num = System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref reference, pSrc, num);
			return num + (byte*)pSrc;
		}
	}

	public unsafe static T ReadInline<T>(void* pSrc) where T : struct
	{
		throw new NotImplementedException();
	}

	public unsafe static void WriteInline<T>(void* pDest, ref T data) where T : struct
	{
		throw new NotImplementedException();
	}

	public unsafe static void CopyInline<T>(ref T data, void* pSrc) where T : struct
	{
		throw new NotImplementedException();
	}

	public unsafe static void CopyInline<T>(void* pDest, ref T srcData) where T : struct
	{
		throw new NotImplementedException();
	}

	public unsafe static void CopyInlineOut<T>(out T data, void* pSrc) where T : struct
	{
		throw new NotImplementedException();
	}

	public unsafe static void* ReadOut<T>(void* pSrc, out T data) where T : struct
	{
		fixed (T* ptr = &data)
		{
			ref _003F reference = ref *(_003F*)ptr;
			int num = System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref reference, pSrc, num);
			return num + (byte*)pSrc;
		}
	}

	public unsafe static void* Read<T>(void* pSrc, T[] data, int offset, int count) where T : struct
	{
		fixed (T* ptr = &data[offset])
		{
			ref _003F reference = ref *(_003F*)ptr;
			int num = System.Runtime.CompilerServices.Unsafe.SizeOf<T>() * count;
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref reference, pSrc, num);
			return num + (byte*)pSrc;
		}
	}

	public unsafe static void* Read2D<T>(void* pSrc, T[,] data, int offset, int count) where T : struct
	{
		fixed (T* ptr = &data[offset])
		{
			ref _003F reference = ref *(_003F*)ptr;
			int num = System.Runtime.CompilerServices.Unsafe.SizeOf<T>() * count;
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(ref reference, pSrc, num);
			return num + (byte*)pSrc;
		}
	}

	public static int SizeOf<T>()
	{
		throw new NotImplementedException();
	}

	public unsafe static void* Write<T>(void* pDest, ref T data) where T : struct
	{
		fixed (T* ptr = &data)
		{
			ref _003F reference = ref *(_003F*)ptr;
			int num = System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(pDest, ref reference, num);
			return num + (byte*)pDest;
		}
	}

	public unsafe static void* Write<T>(void* pDest, T[] data, int offset, int count) where T : struct
	{
		fixed (T* ptr = &data[offset])
		{
			ref _003F reference = ref *(_003F*)ptr;
			int num = System.Runtime.CompilerServices.Unsafe.SizeOf<T>() * count;
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(pDest, ref reference, num);
			return num + (byte*)pDest;
		}
	}

	public unsafe static void* Write2D<T>(void* pDest, T[,] data, int offset, int count) where T : struct
	{
		fixed (T* ptr = &data[offset])
		{
			ref _003F reference = ref *(_003F*)ptr;
			int num = System.Runtime.CompilerServices.Unsafe.SizeOf<T>() * count;
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned(pDest, ref reference, num);
			return num + (byte*)pDest;
		}
	}

	[Tag("SharpDX.ModuleInit")]
	public static void ModuleInit()
	{
	}
}
