using System;

namespace Sce.Atf.VectorMath;

[Flags]
public enum EulerAngleChannels
{
	X = 1,
	Y = 2,
	Z = 4,
	XY = 3,
	XZ = 5,
	YZ = 6,
	XYZ = 7
}
