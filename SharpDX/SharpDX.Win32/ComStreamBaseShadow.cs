using System;
using System.Runtime.InteropServices;

namespace SharpDX.Win32;

internal class ComStreamBaseShadow : ComObjectShadow
{
	internal class ComStreamBaseVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int ReadDelegate(IntPtr thisPtr, IntPtr buffer, int sizeOfBytes, out int bytesRead);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int WriteDelegate(IntPtr thisPtr, IntPtr buffer, int sizeOfBytes, out int bytesWrite);

		public ComStreamBaseVtbl(int numberOfMethods)
			: base(numberOfMethods + 2)
		{
			AddMethod(new ReadDelegate(ReadImpl));
			AddMethod(new WriteDelegate(WriteImpl));
		}

		private static int ReadImpl(IntPtr thisPtr, IntPtr buffer, int sizeOfBytes, out int bytesRead)
		{
			bytesRead = 0;
			try
			{
				ComStreamBaseShadow comStreamBaseShadow = CppObjectShadow.ToShadow<ComStreamBaseShadow>(thisPtr);
				IStream stream = (IStream)comStreamBaseShadow.Callback;
				bytesRead = stream.Read(buffer, sizeOfBytes);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int WriteImpl(IntPtr thisPtr, IntPtr buffer, int sizeOfBytes, out int bytesWrite)
		{
			bytesWrite = 0;
			try
			{
				ComStreamBaseShadow comStreamBaseShadow = CppObjectShadow.ToShadow<ComStreamBaseShadow>(thisPtr);
				IStream stream = (IStream)comStreamBaseShadow.Callback;
				bytesWrite = stream.Write(buffer, sizeOfBytes);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly ComStreamBaseVtbl Vtbl = new ComStreamBaseVtbl(0);

	protected override CppObjectVtbl GetVtbl => Vtbl;
}
