using System;

namespace SharpDX;

public struct DataRectangle
{
	public IntPtr DataPointer;

	public int Pitch;

	public DataRectangle(IntPtr dataPointer, int pitch)
	{
		DataPointer = dataPointer;
		Pitch = pitch;
	}
}
