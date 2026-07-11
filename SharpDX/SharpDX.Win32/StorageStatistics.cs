using System;
using System.Runtime.InteropServices;

namespace SharpDX.Win32;

public struct StorageStatistics
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	internal struct __Native
	{
		public IntPtr PwcsName;

		public int Type;

		public long CbSize;

		public long Mtime;

		public long Ctime;

		public long Atime;

		public int GrfMode;

		public int GrfLocksSupported;

		public Guid Clsid;

		public int GrfStateBits;

		public int Reserved;

		internal void __MarshalFree()
		{
			if (PwcsName != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(PwcsName);
			}
		}
	}

	public string PwcsName;

	public int Type;

	public long CbSize;

	public long Mtime;

	public long Ctime;

	public long Atime;

	public int GrfMode;

	public int GrfLocksSupported;

	public Guid Clsid;

	public int GrfStateBits;

	public int Reserved;

	internal void __MarshalFree(ref __Native @ref)
	{
		@ref.__MarshalFree();
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		PwcsName = ((@ref.PwcsName == IntPtr.Zero) ? null : Marshal.PtrToStringUni(@ref.PwcsName));
		Type = @ref.Type;
		CbSize = @ref.CbSize;
		Mtime = @ref.Mtime;
		Ctime = @ref.Ctime;
		Atime = @ref.Atime;
		GrfMode = @ref.GrfMode;
		GrfLocksSupported = @ref.GrfLocksSupported;
		Clsid = @ref.Clsid;
		GrfStateBits = @ref.GrfStateBits;
		Reserved = @ref.Reserved;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.PwcsName = ((PwcsName == null) ? IntPtr.Zero : Utilities.StringToHGlobalUni(PwcsName));
		@ref.Type = Type;
		@ref.CbSize = CbSize;
		@ref.Mtime = Mtime;
		@ref.Ctime = Ctime;
		@ref.Atime = Atime;
		@ref.GrfMode = GrfMode;
		@ref.GrfLocksSupported = GrfLocksSupported;
		@ref.Clsid = Clsid;
		@ref.GrfStateBits = GrfStateBits;
		@ref.Reserved = Reserved;
	}
}
