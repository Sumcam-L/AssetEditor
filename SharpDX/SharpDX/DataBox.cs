using System;

namespace SharpDX;

public struct DataBox
{
	public IntPtr DataPointer;

	public int RowPitch;

	public int SlicePitch;

	public bool IsEmpty
	{
		get
		{
			if (DataPointer == IntPtr.Zero && RowPitch == 0)
			{
				return SlicePitch == 0;
			}
			return false;
		}
	}

	public DataBox(IntPtr datapointer, int rowPitch, int slicePitch)
	{
		DataPointer = datapointer;
		RowPitch = rowPitch;
		SlicePitch = slicePitch;
	}

	public DataBox(IntPtr dataPointer)
	{
		DataPointer = dataPointer;
		RowPitch = 0;
		SlicePitch = 0;
	}
}
