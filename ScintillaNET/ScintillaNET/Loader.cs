using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ScintillaNET;

internal sealed class Loader : ILoader
{
	private readonly IntPtr self;

	private readonly NativeMethods.ILoaderVTable32 loader32;

	private readonly NativeMethods.ILoaderVTable64 loader64;

	private readonly Encoding encoding;

	public unsafe bool AddData(char[] data, int length)
	{
		if (data != null)
		{
			length = Helpers.Clamp(length, 0, data.Length);
			byte[] bytes = Helpers.GetBytes(data, length, encoding, zeroTerminated: false);
			fixed (byte* data2 = bytes)
			{
				if (((IntPtr.Size == 4) ? loader32.AddData(self, data2, bytes.Length) : loader64.AddData(self, data2, bytes.Length)) != 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	public Document ConvertToDocument()
	{
		IntPtr value = ((IntPtr.Size == 4) ? loader32.ConvertToDocument(self) : loader64.ConvertToDocument(self));
		return new Document
		{
			Value = value
		};
	}

	public int Release()
	{
		return (IntPtr.Size == 4) ? loader32.Release(self) : loader64.Release(self);
	}

	public unsafe Loader(IntPtr ptr, Encoding encoding)
	{
		self = ptr;
		this.encoding = encoding;
		IntPtr ptr2 = *(IntPtr*)(void*)ptr;
		if (IntPtr.Size == 4)
		{
			loader32 = (NativeMethods.ILoaderVTable32)Marshal.PtrToStructure(ptr2, typeof(NativeMethods.ILoaderVTable32));
		}
		else
		{
			loader64 = (NativeMethods.ILoaderVTable64)Marshal.PtrToStructure(ptr2, typeof(NativeMethods.ILoaderVTable64));
		}
	}
}
