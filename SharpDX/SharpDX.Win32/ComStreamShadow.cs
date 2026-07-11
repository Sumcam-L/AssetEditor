using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpDX.Win32;

internal class ComStreamShadow : ComStreamBaseShadow
{
	private class ComStreamVtbl : ComStreamBaseVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SeekDelegate(IntPtr thisPtr, long offset, SeekOrigin origin, IntPtr newPosition);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate Result SetSizeDelegate(IntPtr thisPtr, long newSize);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int CopyToDelegate(IntPtr thisPtr, IntPtr streamPointer, long numberOfBytes, out long numberOfBytesRead, out long numberOfBytesWritten);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate Result CommitDelegate(IntPtr thisPtr, CommitFlags flags);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate Result RevertDelegate(IntPtr thisPtr);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate Result LockRegionDelegate(IntPtr thisPtr, long offset, long numberOfBytes, LockType lockType);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate Result UnlockRegionDelegate(IntPtr thisPtr, long offset, long numberOfBytes, LockType lockType);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate Result StatDelegate(IntPtr thisPtr, ref StorageStatistics.__Native statisticsPtr, StorageStatisticsFlags flags);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate Result CloneDelegate(IntPtr thisPtr, out IntPtr streamPointer);

		public ComStreamVtbl()
			: base(9)
		{
			AddMethod(new SeekDelegate(SeekImpl));
			AddMethod(new SetSizeDelegate(SetSizeImpl));
			AddMethod(new CopyToDelegate(CopyToImpl));
			AddMethod(new CommitDelegate(CommitImpl));
			AddMethod(new RevertDelegate(RevertImpl));
			AddMethod(new LockRegionDelegate(LockRegionImpl));
			AddMethod(new UnlockRegionDelegate(UnlockRegionImpl));
			AddMethod(new StatDelegate(StatImpl));
			AddMethod(new CloneDelegate(CloneImpl));
		}

		private unsafe static int SeekImpl(IntPtr thisPtr, long offset, SeekOrigin origin, IntPtr newPosition)
		{
			try
			{
				ComStreamShadow comStreamShadow = CppObjectShadow.ToShadow<ComStreamShadow>(thisPtr);
				IStream stream = (IStream)comStreamShadow.Callback;
				long num = stream.Seek(offset, origin);
				if (newPosition != IntPtr.Zero)
				{
					*(long*)(void*)newPosition = num;
				}
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static Result SetSizeImpl(IntPtr thisPtr, long newSize)
		{
			Result result = Result.Ok;
			try
			{
				ComStreamShadow comStreamShadow = CppObjectShadow.ToShadow<ComStreamShadow>(thisPtr);
				IStream stream = (IStream)comStreamShadow.Callback;
				stream.SetSize(newSize);
			}
			catch (SharpDXException ex)
			{
				result = ex.ResultCode;
			}
			catch (Exception)
			{
				result = Result.Fail.Code;
			}
			return result;
		}

		private static int CopyToImpl(IntPtr thisPtr, IntPtr streamPointer, long numberOfBytes, out long numberOfBytesRead, out long numberOfBytesWritten)
		{
			numberOfBytesRead = 0L;
			numberOfBytesWritten = 0L;
			try
			{
				ComStreamShadow comStreamShadow = CppObjectShadow.ToShadow<ComStreamShadow>(thisPtr);
				IStream stream = (IStream)comStreamShadow.Callback;
				numberOfBytesRead = stream.CopyTo(new ComStream(streamPointer), numberOfBytes, out numberOfBytesWritten);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static Result CommitImpl(IntPtr thisPtr, CommitFlags flags)
		{
			Result result = Result.Ok;
			try
			{
				ComStreamShadow comStreamShadow = CppObjectShadow.ToShadow<ComStreamShadow>(thisPtr);
				IStream stream = (IStream)comStreamShadow.Callback;
				stream.Commit(flags);
			}
			catch (SharpDXException ex)
			{
				result = ex.ResultCode;
			}
			catch (Exception)
			{
				result = Result.Fail.Code;
			}
			return result;
		}

		private static Result RevertImpl(IntPtr thisPtr)
		{
			Result result = Result.Ok;
			try
			{
				ComStreamShadow comStreamShadow = CppObjectShadow.ToShadow<ComStreamShadow>(thisPtr);
				IStream stream = (IStream)comStreamShadow.Callback;
				stream.Revert();
			}
			catch (SharpDXException ex)
			{
				result = ex.ResultCode;
			}
			catch (Exception)
			{
				result = Result.Fail.Code;
			}
			return result;
		}

		private static Result LockRegionImpl(IntPtr thisPtr, long offset, long numberOfBytes, LockType lockType)
		{
			Result result = Result.Ok;
			try
			{
				ComStreamShadow comStreamShadow = CppObjectShadow.ToShadow<ComStreamShadow>(thisPtr);
				IStream stream = (IStream)comStreamShadow.Callback;
				stream.LockRegion(offset, numberOfBytes, lockType);
			}
			catch (SharpDXException ex)
			{
				result = ex.ResultCode;
			}
			catch (Exception)
			{
				result = Result.Fail.Code;
			}
			return result;
		}

		private static Result UnlockRegionImpl(IntPtr thisPtr, long offset, long numberOfBytes, LockType lockType)
		{
			Result result = Result.Ok;
			try
			{
				ComStreamShadow comStreamShadow = CppObjectShadow.ToShadow<ComStreamShadow>(thisPtr);
				IStream stream = (IStream)comStreamShadow.Callback;
				stream.UnlockRegion(offset, numberOfBytes, lockType);
			}
			catch (SharpDXException ex)
			{
				result = ex.ResultCode;
			}
			catch (Exception)
			{
				result = Result.Fail.Code;
			}
			return result;
		}

		private static Result StatImpl(IntPtr thisPtr, ref StorageStatistics.__Native statisticsPtr, StorageStatisticsFlags flags)
		{
			try
			{
				ComStreamShadow comStreamShadow = CppObjectShadow.ToShadow<ComStreamShadow>(thisPtr);
				IStream stream = (IStream)comStreamShadow.Callback;
				stream.GetStatistics(flags).__MarshalTo(ref statisticsPtr);
			}
			catch (SharpDXException ex)
			{
				return ex.ResultCode;
			}
			catch (Exception)
			{
				return Result.Fail.Code;
			}
			return Result.Ok;
		}

		private static Result CloneImpl(IntPtr thisPtr, out IntPtr streamPointer)
		{
			streamPointer = IntPtr.Zero;
			Result result = Result.Ok;
			try
			{
				ComStreamShadow comStreamShadow = CppObjectShadow.ToShadow<ComStreamShadow>(thisPtr);
				IStream stream = (IStream)comStreamShadow.Callback;
				IStream stream2 = stream.Clone();
				streamPointer = ComStream.ToIntPtr(stream2);
			}
			catch (SharpDXException ex)
			{
				result = ex.ResultCode;
			}
			catch (Exception)
			{
				result = Result.Fail.Code;
			}
			return result;
		}
	}

	private static readonly ComStreamVtbl Vtbl = new ComStreamVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(IStream stream)
	{
		return CppObject.ToCallbackPtr<IStream>(stream);
	}
}
